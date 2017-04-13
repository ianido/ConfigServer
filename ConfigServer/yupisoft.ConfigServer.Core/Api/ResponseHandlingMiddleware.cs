using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core
{
    public class ResponseHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        protected readonly ILogger _log;
        private readonly ConfigServerTenants _tenants;
        private readonly ClusterConfigSection _clusterconfig;

        public ResponseHandlingMiddleware(RequestDelegate next, ILogger<ResponseHandlingMiddleware> logger, ConfigServerTenants tenants, IOptions<ClusterConfigSection> clusterConfig)
        {
            _next = next;
            _log = logger;
            _tenants = tenants;
            _clusterconfig = clusterConfig.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var original = context.Response.Body;
            var stream = new MemoryStream();
            context.Response.Body = stream;
            await _next(context);

            stream.Seek(0, SeekOrigin.Begin);
            var body = new StreamReader(stream).ReadToEnd();

            var groups = Regex.Match(context.Request.Path.ToUriComponent(), @"\/api\/([A-Za-z0-9]*)\/")?.Groups;
            if ((groups != null) && (groups.Count > 1))
            {
                var tenantId = groups[1].Value;
                var encryption = false;
                var sharedKey = "";

                if (_clusterconfig.Security.AppId == context.User.Identity.Name)
                {
                    // Usar informacion de Seguridad del Cluster
                    encryption = _clusterconfig.Security.Encrypted;
                    sharedKey = _clusterconfig.Security.SecretKey;
                }
                else
                {
                    // Buscar el Tenant y usar informacion de Seguridad del Tenant
                    var tenant = _tenants.Tenants.FirstOrDefault(t => t.TenantConfig.Id == tenantId);
                    if (tenant != null)
                    {
                        if (tenant.ACLToken != null)
                        {
                            sharedKey = tenant.ACL.Apps.FirstOrDefault(a => a.AppId == context.User.Identity.Name)?.Secret;
                        }
                        encryption = tenant.Encrypted;
                    }
                }

                if (!string.IsNullOrEmpty(sharedKey))
                {
                    byte[] content = Encoding.UTF8.GetBytes(body);
                    using (HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(sharedKey)))
                    {
                        byte[] signatureBytes = hmac.ComputeHash(content);
                        string responseSignatureBase64String = Convert.ToBase64String(signatureBytes);
                        if (encryption)
                        {
                            var salt = StringHandling.DefaultSalt;
                            var header = context.Request.Headers["authorization"].ToString().Split(':');
                            if (header.Length == 4) salt = header[2]; // Nonce
                            var bytes = StringHandling.Encrypt(Encoding.UTF8.GetBytes(body), sharedKey, salt);
                            stream = new MemoryStream(bytes);
                        }
                        context.Response.OnStarting(state =>
                        {
                            var httpContext = (HttpContext)state;
                            if (encryption)
                                httpContext.Response.Headers.Add("Encrypted", new[] { "true" });
                            httpContext.Response.Headers.Add("Signature", new[] { responseSignatureBase64String });
                            return Task.FromResult(0);
                        }, context);
                    }
                }
            }
            var url = UriHelper.GetDisplayUrl(context.Request);
            //_log.LogTrace("Response Url '{url}' Response Body '{responsebody}'", url, body);
            stream.Seek(0, SeekOrigin.Begin);
            await stream.CopyToAsync(original);
        }

    }
}
