using System;
using System.Net;

namespace E.Http
{
    internal class CookieContainerBuilder
    {
        private readonly AsyncCookieContainer _asyncCookieContainer;
        private readonly CookieContainer _cookieContainer;

        private CookieContainerBuilder(AsyncCookieContainer asyncCookieContainer)
        {
            _asyncCookieContainer = asyncCookieContainer;
        }

        private CookieContainerBuilder(CookieContainer cookieContainer)
        {
            _cookieContainer = cookieContainer;
        }

        public CookieContainer Builder(Uri uri)
        {
            return _asyncCookieContainer != null
                ? _asyncCookieContainer.ToCookieContainer(uri)
                : _cookieContainer;
        }

        public static CookieContainerBuilder Create(string cookies)
        {
            return new CookieContainerBuilder(AsyncCookieContainer.Create(cookies));
        }

        public static CookieContainerBuilder Create(AsyncCookieContainer cookies)
        {
            return new CookieContainerBuilder(cookies);
        }

        public static CookieContainerBuilder Create(CookieContainer cookies)
        {
            return new CookieContainerBuilder(cookies);
        }
    }
}