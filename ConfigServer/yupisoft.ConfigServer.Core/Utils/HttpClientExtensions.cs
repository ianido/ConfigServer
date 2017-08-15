using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core.Utils
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, string requestUri, HttpContent content, string APPId, string APIKey, bool encrypt = false)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = content;
            return client.SendAsync(request, CancellationToken.None, APPId, APIKey, encrypt);
        }

        public static Task<HttpResponseMessage> GetAsync(this HttpClient client, string requestUri, HttpContent content, string APPId, string APIKey, bool encrypt = false)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Content = content;
            return client.SendAsync(request, CancellationToken.None, APPId, APIKey, encrypt);
        }

        public static Task<HttpResponseMessage> SendAsync(this HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken, string APPId, string APIKey, bool encrypt = false)
        {
            string requestContentBase64String = string.Empty;

            string requestUri = System.Net.WebUtility.UrlEncode(request.RequestUri.AbsoluteUri).ToLower();

            string requestHttpMethod = request.Method.Method;


            //Calculate UNIX time
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = DateTime.UtcNow - epochStart;
            string requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();

            //create random nonce for each request
            string nonce = Guid.NewGuid().ToString("N");

            //Checking if the request contains body, usually will be null wiht HTTP GET and DELETE
            if (request.Content != null)
            {
                byte[] content = request.Content.ReadAsByteArrayAsync().Result;
                MD5 md5 = MD5.Create();
                //Hashing the request body, any change in request body will result in different hash, we'll incure message integrity
                byte[] requestContentHash = md5.ComputeHash(content);
                requestContentBase64String = Convert.ToBase64String(requestContentHash);
            }

            //Creating the raw signature string
            string data = String.Format("{0}{1}{2}{3}{4}{5}", APPId, requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String);

            var secretKeyBytes = Convert.FromBase64String(APIKey);
            byte[] signature = Encoding.UTF8.GetBytes(data);
            using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
                //Setting the values in the Authorization header using custom scheme (amx)
                request.Headers.Authorization = new AuthenticationHeaderValue("Hmac", string.Format("{0}:{1}:{2}:{3}", APPId, requestSignatureBase64String, nonce, requestTimeStamp));
            }

            if ((encrypt) && (request.Content != null))
            {
                request.Headers.Add("Encrypted", "true");
                byte[] content = request.Content.ReadAsByteArrayAsync().Result;
                var econtent = StringHandling.Encrypt(content, APIKey, nonce);
                var headers = request.Content.Headers.ToArray();
                request.Content = new ByteArrayContent(econtent);
                foreach (var h in headers)
                    request.Content.Headers.Add(h.Key, h.Value);
            }

            var a = client.SendAsync(request, cancellationToken).ContinueWith((t) =>
            {
                if ((t.Status == TaskStatus.RanToCompletion) && (t.Result.IsSuccessStatusCode))
                {
                    var res = t.Result;
                    if (res.Headers.Contains("Encrypted"))
                    {
                        string[] enc = res.Headers.GetValues("Encrypted").ToArray();
                        if ((enc.Length > 0) && (res.Content != null))
                        {
                            // Signature is present in the message, check signature
                            if (bool.TryParse(enc[0], out bool encrypted))
                            {
                                byte[] content = res.Content.ReadAsByteArrayAsync().Result;
                                var dcontent = StringHandling.Decrypt(content, APIKey, nonce);
                                var headers = res.Content.Headers.ToArray();
                                res.Content = new ByteArrayContent(dcontent);
                                foreach (var h in headers)
                                    res.Content.Headers.Add(h.Key, h.Value);
                            }
                        }
                    }
                    if (res.Headers.Contains("Signature"))
                    {
                        string[] sig = res.Headers.GetValues("Signature").ToArray();

                        if ((sig.Length > 0) && (res.Content != null))
                        {
                            // Signature is present in the message, check signature
                            byte[] content = res.Content.ReadAsByteArrayAsync().Result;

                            using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
                            {
                                byte[] signatureBytes = hmac.ComputeHash(content);
                                string responseSignatureBase64String = Convert.ToBase64String(signatureBytes);
                                if (responseSignatureBase64String != sig[0])
                                    throw new Exception("Signature Check Failed");
                            }
                        }
                    }
                }
                return t;
            });
            return a.Unwrap();
        }
    }
}
