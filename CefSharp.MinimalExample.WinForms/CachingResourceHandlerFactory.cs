using System;
using System.IO;

namespace CefSharp.MinimalExample.WinForms
{
    internal sealed class CachingResourceHandlerFactory : IResourceHandlerFactory
    {
        private readonly Predicate<Uri> _checkUriCacheable;
        private readonly DirectoryInfo _cacheDirectory;

        public CachingResourceHandlerFactory(Predicate<Uri> checkUriCacheable, DirectoryInfo cacheDirectory)
        {
            _checkUriCacheable = checkUriCacheable;
            _cacheDirectory = cacheDirectory;
        }

        public IResourceHandler GetResourceHandler(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request)
        {
            return _checkUriCacheable(new Uri(request.Url))
                ? new CachingResourceHandler(_cacheDirectory)
                : null;
        }

        public bool HasHandlers
        {
            get { return true; }
        }
    }
}
