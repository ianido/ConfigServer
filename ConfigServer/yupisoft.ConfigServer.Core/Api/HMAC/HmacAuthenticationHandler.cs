using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using yupisoft.ConfigServer.Core;
using yupisoft.ConfigServer.Core.Utils;

namespace Microsoft.AspNetCore.Authentication.Hmac
{
    internal class HmacAuthenticationHandler : AuthenticationHandler<HmacAuthenticationOptions>
    {
        
        public class HmacValidationResult
        {
            public string AppId { get; set; }
            public bool Valid { get; set; }
            public string Name { get; set; }
            public string[] Roles { get; set; }

            public static HmacValidationResult Fail(string appId)
            {
                return new HmacValidationResult() { Valid = false, AppId = appId };
            }

            public static HmacValidationResult Success(string appId, string name, string[] roles)
            {
                return new HmacValidationResult() { Valid = true, AppId = appId, Name= name, Roles = roles };
            }
        }

        private readonly IMemoryCache _memoryCache;
        private readonly ConfigServerTenants _tenants;

        public HmacAuthenticationHandler(IMemoryCache memoryCache, ConfigServerTenants tenants):base()
        {
            _memoryCache = memoryCache;
            _tenants = tenants;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorization = Request.Headers["authorization"];
            if (string.IsNullOrEmpty(authorization))
            {
                return await Task.FromResult(AuthenticateResult.Skip());
            }
            var res = Validate(Request);

            if (res.Valid)
            {
                var claimId = new ClaimsIdentity("Hmac");
                claimId.AddClaim(new Claim(ClaimTypes.Name, res.AppId));
                if ((res.Roles != null) && (res.Roles.Length > 0))
                foreach (var r in res.Roles)
                   claimId.AddClaim(new Claim(ClaimTypes.Role, r));

                var principal = new ClaimsPrincipal(claimId);
                var properties = new AuthenticationProperties();

                var ticket = new AuthenticationTicket(principal, properties, Options.AuthenticationScheme);

                return await Task.FromResult(AuthenticateResult.Success(ticket));
            }
            return await Task.FromResult(AuthenticateResult.Fail("Authentication failed"));
        }

        protected override Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            Response.Headers.Append("WWW-Authenticate", new StringValues(this.Options.AuthenticationScheme));
            return base.HandleUnauthorizedAsync(context);
        }
        protected override Task HandleSignOutAsync(SignOutContext context)
        {
            throw new NotSupportedException();
        }
        protected override Task HandleSignInAsync(SignInContext context)
        {
            throw new NotSupportedException();
        }
        
        #region Privated
        private HmacValidationResult Validate(HttpRequest request)
        {
            var header = request.Headers["authorization"];
            var authenticationHeader = AuthenticationHeaderValue.Parse(header);
            if (Options.AuthenticationScheme.Equals(authenticationHeader.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                var rawAuthenticationHeader = authenticationHeader.Parameter;
                var authenticationHeaderArray = GetAuthenticationValues(rawAuthenticationHeader);

                if (authenticationHeaderArray != null)
                {
                    var AppId = authenticationHeaderArray[0];
                    var incomingBase64Signature = authenticationHeaderArray[1];
                    var nonce = authenticationHeaderArray[2];
                    var requestTimeStamp = authenticationHeaderArray[3];

                    return isValidRequest(request, AppId, incomingBase64Signature, nonce, requestTimeStamp);
                }
            }

            return HmacValidationResult.Fail("");
        }
        private HmacValidationResult isValidRequest(HttpRequest req, string AppId, string incomingBase64Signature, string nonce, string requestTimeStamp)
        {
            string requestContentBase64String = "";
            var absoluteUri = string.Concat(
                        req.Scheme,
                        "://",
                        req.Host.ToUriComponent(),
                        req.PathBase.ToUriComponent(),
                        req.Path.ToUriComponent(),
                        req.QueryString.ToUriComponent());
            string requestUri = WebUtility.UrlEncode(absoluteUri).ToLower();
            string requestHttpMethod = req.Method;
            var sharedKey = "";
            string roles = null;
            string name = "";
            bool encryption = false;

            if (IsReplayRequest(nonce, requestTimeStamp))
            {
                return HmacValidationResult.Fail(AppId);
            }

            if (AppId == Options.AppId)
            {
                if (Options.AppId != AppId)
                {
                    return HmacValidationResult.Fail(AppId);
                }
                name = Options.AppId;
                sharedKey = Options.SecretKey;
                roles = "Cluster";
                encryption = Options.Encrypted;
            }
            else
            {
                // Determine Tenant by the route
                // api/{tenantId}/..

                var groups = Regex.Match(req.Path.ToUriComponent(), @"\/api\/([A-Za-z0-9]*)\/")?.Groups;
                if ((groups != null) && (groups.Count > 1))
                {
                    var tenantId = groups[1].Value;
                    var tenant = _tenants.Tenants.FirstOrDefault(t => t.TenantConfig.Id == tenantId);
                    
                    if (tenant != null)
                    {
                        JTenantACL _acl = tenant.ACL;
                        encryption = tenant.Encrypted;

                        if (tenant.ACLToken == null)
                        {
                            // No authentication is enabled
                            return HmacValidationResult.Success(AppId, AppId, new string[1] { "Customer" });
                        }
                        if (tenant.ACL == null)
                        {
                            // Incorrect serialization
                            return HmacValidationResult.Fail(AppId);
                        }
                        sharedKey = _acl.Apps.FirstOrDefault(a => a.AppId == AppId)?.Secret;
                        if (string.IsNullOrEmpty(sharedKey))
                            return HmacValidationResult.Fail(AppId);
                        else
                        {
                            name = AppId;
                            roles = _acl.Apps.FirstOrDefault(a => a.AppId == AppId)?.Roles;
                            if (string.IsNullOrEmpty(roles)) roles = "Customer";
                            else roles = "Customer,"+roles;
                        }
                    }
                    else
                        return HmacValidationResult.Fail(AppId);
                }
                 else
                    return HmacValidationResult.Fail(AppId);
            }

            req.EnableRewind();
            var body = ReadFully(req.Body);

            if (!encryption)
            {
                // Check if request was encrypted anyway.
                if (req.Headers.ContainsKey("Encrypted"))
                {
                    var encryptedHeader = req.Headers["Encrypted"];
                    bool.TryParse(encryptedHeader, out bool requestEncrypted);
                    if (requestEncrypted) encryption = true;
                } 
            }

            if (encryption)
            {
                body = StringHandling.Decrypt(body, sharedKey, nonce);
                req.Body = new MemoryStream(body);
            }

            byte[] hash = ComputeHash(body);
            req.Body.Seek(0, SeekOrigin.Begin);

            if (hash != null)
            {
                requestContentBase64String = Convert.ToBase64String(hash);
            }

            string data = String.Format("{0}{1}{2}{3}{4}{5}", AppId, requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String);

            var secretKeyBytes = Convert.FromBase64String(sharedKey);
            byte[] signature = Encoding.UTF8.GetBytes(data);
            using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                string signatureBase64 = Convert.ToBase64String(signatureBytes);
                bool sameSignature = (incomingBase64Signature.Equals(signatureBase64, StringComparison.Ordinal));
                if (sameSignature)
                {
                    if (!string.IsNullOrEmpty(roles)) 
                        return HmacValidationResult.Success(AppId, name, roles.Split(','));
                    else
                        return HmacValidationResult.Success(AppId, name, null);
                }
                else
                    return HmacValidationResult.Fail(AppId);
            }

        }
        private string[] GetAuthenticationValues(string rawAuthenticationHeader)
        {
            var credArray = rawAuthenticationHeader.Split(':');

            if (credArray.Length == 4)
            {
                return credArray;
            }
            else
            {
                return null;
            }
        }
        private bool IsReplayRequest(string nonce, string requestTimeStamp)
        {
            var nonceInMemory = _memoryCache.Get(nonce);
            if ( nonceInMemory != null)
            {
                return true;
            }

            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan currentTs = DateTime.UtcNow - epochStart;

            var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);
            var diff = (serverTotalSeconds - requestTotalSeconds);

            if (diff > Options.MaxRequestAgeInSeconds)
            {
                return true;
            }
            _memoryCache.Set(nonce, requestTimeStamp, DateTimeOffset.UtcNow.AddSeconds(Options.MaxRequestAgeInSeconds));
            return false;
        }
        private byte[] ComputeHash(byte[] content)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = null;
                if (content.Length != 0)
                {
                    hash = md5.ComputeHash(content);
                }
                return hash;
            }
        }
        private byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        #endregion
    }
}
