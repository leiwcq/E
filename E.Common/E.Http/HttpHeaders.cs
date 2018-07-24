using System;
using System.Collections.Generic;

namespace E.Http
{
    public static class HttpHeaders
    {
        public const string X_PARAM_OVERRIDE_PREFIX = "X-Param-Override-";

        public const string X_HTTP_METHOD_OVERRIDE = "X-Http-Method-Override";

        public const string X_AUTO_BATCH_COMPLETED = "X-AutoBatch-Completed"; // How many requests were completed before first failure

        public const string X_TAG = "X-Tag";

        public const string X_USER_AUTH_ID = "X-UAId";

        public const string X_TRIGGER = "X-Trigger"; // Trigger Events on UserAgent

        public const string X_FORWARDED_FOR = "X-Forwarded-For"; // IP Address

        public const string X_FORWARDED_PORT = "X-Forwarded-Port";  // 80

        public const string X_FORWARDED_PROTOCOL = "X-Forwarded-Proto"; // http or https

        public const string X_REAL_IP = "X-Real-IP";

        public const string X_LOCATION = "X-Location";

        public const string X_STATUS = "X-Status";

        public const string REFERER = "Referer";

        public const string CACHE_CONTROL = "Cache-Control";

        public const string IF_MODIFIED_SINCE = "If-Modified-Since";

        public const string IF_UNMODIFIED_SINCE = "If-Unmodified-Since";

        public const string IF_NONE_MATCH = "If-None-Match";

        public const string IF_MATCH = "If-Match";

        public const string LAST_MODIFIED = "Last-Modified";

        public const string ACCEPT = "Accept";

        public const string ACCEPT_ENCODING = "Accept-Encoding";

        public const string CONTENT_TYPE = "Content-Type";

        public const string CONTENT_ENCODING = "Content-Encoding";

        public const string CONTENT_LENGTH = "Content-Length";

        public const string CONTENT_DISPOSITION = "Content-Disposition";

        public const string LOCATION = "Location";

        public const string SET_COOKIE = "Set-Cookie";

        public const string E_TAG = "ETag";

        public const string AGE = "Age";

        public const string EXPIRES = "Expires";

        public const string VARY = "Vary";

        public const string AUTHORIZATION = "Authorization";

        public const string WWW_AUTHENTICATE = "WWW-Authenticate";

        public const string ALLOW_ORIGIN = "Access-Control-Allow-Origin";

        public const string ALLOW_METHODS = "Access-Control-Allow-Methods";

        public const string ALLOW_HEADERS = "Access-Control-Allow-Headers";

        public const string ALLOW_CREDENTIALS = "Access-Control-Allow-Credentials";

        public const string EXPOSE_HEADERS = "Access-Control-Expose-Headers";

        public const string ACCESS_CONTROL_MAX_AGE = "Access-Control-Max-Age";

        public const string ORIGIN = "Origin";

        public const string REQUEST_METHOD = "Access-Control-Request-Method";

        public const string REQUEST_HEADERS = "Access-Control-Request-Headers";

        public const string ACCEPT_RANGES = "Accept-Ranges";

        public const string CONTENT_RANGE = "Content-Range";

        public const string RANGE = "Range";

        public const string SOAP_ACTION = "SOAPAction";

        public const string ALLOW = "Allow";

        public const string ACCEPT_CHARSET = "Accept-Charset";

        public const string ACCEPT_LANGUAGE = "Accept-Language";

        public const string CONNECTION = "Connection";

        public const string COOKIE = "Cookie";

        public const string CONTENT_LANGUAGE = "Content-Language";

        public const string EXPECT = "Expect";

        public const string PRAGMA = "Pragma";

        public const string PROXY_AUTHENTICATE = "Proxy-Authenticate";

        public const string PROXY_AUTHORIZATION = "Proxy-Authorization";

        public const string PROXY_CONNECTION = "Proxy-Connection";

        public const string SET_COOKIE2 = "Set-Cookie2";

        public const string TE = "TE";

        public const string TRAILER = "Trailer";

        public const string TRANSFER_ENCODING = "Transfer-Encoding";

        public const string UPGRADE = "Upgrade";

        public const string VIA = "Via";

        public const string WARNING = "Warning";

        public const string DATE = "Date";
        public const string HOST = "Host";
        public const string USER_AGENT = "User-Agent";

        public static HashSet<string> RestrictedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ACCEPT,
            CONNECTION,
            CONTENT_LENGTH,
            CONTENT_TYPE,
            DATE,
            EXPECT,
            HOST,
            IF_MODIFIED_SINCE,
            RANGE,
            REFERER,
            TRANSFER_ENCODING,
            USER_AGENT,
            PROXY_CONNECTION,
        };
    }
}