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

        private Stream _readingStream;
        private Stream _writingStream;

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
            get
            {
                if(_writingStream == null)
                //return File.OpenWrite(_resourceFilePath);
                _writingStream = File.Open(_resourceFilePath, FileMode.Create, FileAccess.Write, FileShare.Write);

                return _writingStream;
            }
        }

        public Stream ContentReadStream
        {
            get
            {
                if(_readingStream == null)
                    //return File.OpenRead(_resourceFilePath);
                    _readingStream = File.Open(_resourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                return _readingStream;
            }
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
