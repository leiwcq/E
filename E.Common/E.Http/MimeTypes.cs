using System;
using System.Collections.Generic;

namespace E.Http
{
    public static class MimeTypes
    {
        public static Dictionary<string, string> ExtensionMimeTypes = new Dictionary<string, string>();

        public const string HTML = "text/html";
        public const string XML = "application/xml";
        public const string XML_TEXT = "text/xml";
        public const string JSON = "application/json";
        public const string JSON_TEXT = "text/json";
        public const string JSV = "application/jsv";
        public const string JSV_TEXT = "text/jsv";
        public const string CSV = "text/csv";
        public const string PROTO_BUF = "application/x-protobuf";
        public const string JAVA_SCRIPT = "text/javascript";

        public const string FORM_URL_ENCODED = "application/x-www-form-urlencoded";
        public const string MULTI_PART_FORM_DATA = "multipart/form-data";
        public const string JSON_REPORT = "text/jsonreport";
        public const string SOAP11 = "text/xml; charset=utf-8";
        public const string SOAP12 = "application/soap+xml";
        public const string YAML = "application/yaml";
        public const string YAML_TEXT = "text/yaml";
        public const string PLAIN_TEXT = "text/plain";
        public const string MARKDOWN_TEXT = "text/markdown";
        public const string MSG_PACK = "application/x-msgpack";
        public const string WIRE = "application/x-wire";
        public const string NET_SERIALIZER = "application/x-netserializer";

        public const string IMAGE_PNG = "image/png";
        public const string IMAGE_GIF = "image/gif";
        public const string IMAGE_JPG = "image/jpeg";

        public const string BSON = "application/bson";
        public const string BINARY = "application/octet-stream";
        public const string SERVER_SENT_EVENTS = "text/event-stream";

        public static string GetExtension(string mimeType)
        {
            switch (mimeType)
            {
                case PROTO_BUF:
                    return ".pbuf";
            }

            var parts = mimeType.Split('/');
            if (parts.Length == 1) return "." + parts[0];
            if (parts.Length == 2) return "." + parts[1];

            throw new NotSupportedException("Unknown mimeType: " + mimeType);
        }

        public static string GetMimeType(string fileNameOrExt)
        {
            if (string.IsNullOrEmpty(fileNameOrExt))
                throw new ArgumentNullException(nameof(fileNameOrExt));

            var parts = fileNameOrExt.Split('.');
            var fileExt = parts[parts.Length - 1];

            string mimeType;
            if (ExtensionMimeTypes.TryGetValue(fileExt, out mimeType))
            {
                return mimeType;
            }

            switch (fileExt)
            {
                case "jpeg":
                case "gif":
                case "png":
                case "tiff":
                case "bmp":
                case "webp":
                    return "image/" + fileExt;

                case "jpg":
                    return "image/jpeg";

                case "tif":
                    return "image/tiff";

                case "svg":
                    return "image/svg+xml";

                case "htm":
                case "html":
                case "shtml":
                    return "text/html";

                case "js":
                    return "text/javascript";

                case "ts":
                    return "text/x.typescript";

                case "jsx":
                    return "text/jsx";

                case "csv":
                case "css":
                case "sgml":
                    return "text/" + fileExt;

                case "txt":
                    return "text/plain";

                case "wav":
                    return "audio/wav";

                case "mp3":
                    return "audio/mpeg3";

                case "mid":
                    return "audio/midi";

                case "qt":
                case "mov":
                    return "video/quicktime";

                case "mpg":
                    return "video/mpeg";

                case "avi":
                case "mp4":
                case "ogg":
                case "webm":
                    return "video/" + fileExt;

                case "ogv":
                    return "video/ogg";

                case "rtf":
                    return "application/" + fileExt;

                case "xls":
                    return "application/x-excel";

                case "doc":
                    return "application/msword";

                case "ppt":
                    return "application/powerpoint";

                case "gz":
                case "tgz":
                    return "application/x-compressed";

                case "eot":
                    return "application/vnd.ms-fontobject";

                case "ttf":
                    return "application/octet-stream";

                case "woff":
                    return "application/font-woff";
                case "woff2":
                    return "application/font-woff2";

                default:
                    return "application/" + fileExt;
            }
        }
    }
}