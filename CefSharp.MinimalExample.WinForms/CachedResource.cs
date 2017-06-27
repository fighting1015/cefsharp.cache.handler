using System;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;

namespace CefSharp.MinimalExample.WinForms
{
    internal sealed class CachedResource
    {
        private const double CacheValidDays = 1;

        private readonly string _resourceFilePath;
        private readonly string _resourceHeaderFilePath;

        public CachedResource(string filePath)
        {
            _resourceFilePath = filePath;
            _resourceHeaderFilePath = GetHeadersPath(filePath);
        }

        public bool Exists
        {
            get
            {
                var fileInfo = new FileInfo(_resourceFilePath);
                return fileInfo.Exists && !IsExpired(fileInfo);
            }
        }

        public NameValueCollection Headers
        {
            get { return ReadNameValueCollection(_resourceHeaderFilePath); }
        }

        public Stream ContentWriteStream
        {
            get { return File.OpenWrite(_resourceFilePath); }
        }

        public Stream ContentReadStream
        {
            get { return File.OpenRead(_resourceFilePath); }
        }

        public long ContentLength
        {
            get { return new FileInfo(_resourceFilePath).Length; }
        }

        public void Save(NameValueCollection headers, Byte[] content)
        {
            File.WriteAllText(_resourceHeaderFilePath, JsonConvert.SerializeObject(headers));
        }

        private static bool IsExpired(FileInfo cachedResourceFile)
        {
            return cachedResourceFile.LastWriteTimeUtc.AddDays(CacheValidDays) < DateTime.UtcNow;
        }

        private static string GetHeadersPath(string cachedResourcePath)
        {
            return cachedResourcePath + ".headers";
        }

        private static NameValueCollection ReadNameValueCollection(string headerFilePath)
        {
            return JsonConvert.DeserializeObject<NameValueCollection>(File.ReadAllText(headerFilePath));
        }
    }
}
