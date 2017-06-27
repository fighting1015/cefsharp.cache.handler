using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace CefSharp.MinimalExample.WinForms
{
    internal sealed class CachingResourceHandler : IResourceHandler
    {
        private readonly DirectoryInfo _cacheDirectory;

        private bool _resourceIsCached;
        private CachedResource _cachedResource;

        private HttpClient _httpClient;
        private HttpResponseMessage _httpReponse;
        private Stream _httpStream;

        private Stream _fileWriteStream;
        private Stream _fileReadStream;

        public CachingResourceHandler(DirectoryInfo cacheDirectory)
        {
            _cacheDirectory = cacheDirectory;
        }

        public bool ProcessRequest(IRequest request, ICallback callback)
        {
            if (!string.Equals(request.Method, "get", StringComparison.CurrentCultureIgnoreCase))
            {
                callback.Continue();
                return true;
            }

            _cachedResource = new CachedResource(GetCachedResourcePath(request.Url));
            _resourceIsCached = _cachedResource.Exists;

            if (_resourceIsCached)
            {
                _fileReadStream = _cachedResource.ContentReadStream;
            }
            else
            {
                _httpClient = new HttpClient();
                _httpReponse = _httpClient.GetAsync(request.Url).Result;

                _httpStream = _httpReponse.Content.ReadAsStreamAsync().Result;

                if(_httpReponse.IsSuccessStatusCode)
                    _fileWriteStream = _cachedResource.ContentWriteStream;
            }

            callback.Continue();
            return true;
        }

        public void GetResponseHeaders(IResponse response, out long responseLength, out string redirectUrl)
        {
            redirectUrl = null;

            if (_resourceIsCached)
            {
                //response.MimeType = 
                responseLength = _cachedResource.ContentLength;
                ////response.ResponseHeaders = _cachedResource.Headers;
            }
            else
            {
                responseLength = _httpReponse.Content.Headers.ContentLength ?? -1;
                response.ResponseHeaders = GetNameValueCollection(_httpReponse.Content.Headers);
            }
        }

        public bool ReadResponse(Stream dataOut, out int bytesRead, ICallback callback)
        {
            callback.Dispose();

            var buffer = new byte[dataOut.Length];

            if (_resourceIsCached)
            {
                bytesRead = _fileReadStream.Read(buffer, 0, buffer.Length);

                dataOut.Write(buffer, 0, buffer.Length);

                return bytesRead > 0;
            }
           
            
            bytesRead = _httpStream.Read(buffer, 0, buffer.Length);
            
            dataOut.Write(buffer, 0, buffer.Length);

            _fileWriteStream?.Write(buffer, 0, buffer.Length);

            return bytesRead > 0;
        }

        public bool CanGetCookie(Cookie cookie)
        {
            return false;
        }

        public bool CanSetCookie(Cookie cookie)
        {
            return false;
        }

        public void Cancel()
        {
            // nop
        }


        public void Dispose()
        {
            if (_fileWriteStream != null)
            {
                _fileWriteStream.Close();
                _fileWriteStream.Dispose();
            }

            if (_fileReadStream != null)
            {
                _fileReadStream.Close();
                _fileReadStream.Dispose();
            }

            _httpStream?.Dispose();
            _httpReponse?.Dispose();
            _httpClient?.Dispose();
        }

        private string GetCachedResourcePath(string url)
        {
            const int maxFileNameLength = 260;

            var fileName = Uri.EscapeDataString(url);

            if (fileName.Length > maxFileNameLength)
                fileName = GetMd5Hash(url);

            return Path.Combine(_cacheDirectory.FullName, fileName);
        }

        private static NameValueCollection GetNameValueCollection(HttpContentHeaders contentHeaders)
        {
            var nvc = new NameValueCollection();
            foreach (var headerKeyValues in contentHeaders)
            {
                var values = string.Join(",", headerKeyValues.Value);
                nvc.Add(headerKeyValues.Key, values);
            }

            return nvc;
        }
        
        private static string GetMd5Hash(string str)
        {
            var md5 = MD5.Create();

            var sb = new StringBuilder();
            foreach (byte b in md5.ComputeHash(Encoding.UTF8.GetBytes(str)))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
