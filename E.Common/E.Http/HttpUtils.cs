using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using E.Common;
using E.Extensions;
using E.Serializer.QueryString;
using E.Serializer;

namespace E.Http
{
    public static class HttpUtils
    {
        public static string UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.2) AppleWebKit/525.13 (KHTML, like Gecko) Chrome/0.2.149.27 Safari/525.13";

        public static Encoding UseEncoding { get; set; } = Encoding.UTF8;

        [ThreadStatic]
        public static IHttpResultsFilter ResultsFilter;

        public static string AddQueryParam(this string url, string key, object val, bool encode = true)
        {
            return url.AddQueryParam(key, val.ToString(), encode);
        }

        public static string AddQueryParam(this string url, object key, string val, bool encode = true)
        {
            return AddQueryParam(url, (key ?? "").ToString(), val, encode);
        }

        public static string AddQueryParam(this string url, string key, string val, bool encode = true)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var prefix = string.Empty;
            if (!url.EndsWith("?") && !url.EndsWith("&"))
            {
                prefix = url.IndexOf('?') == -1 ? "?" : "&";
            }
            return url + prefix + key + "=" + (encode ? val.UrlEncode() : val);
        }

        public static string SetQueryParam(this string url, string key, string val)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var qsPos = url.IndexOf('?');
            if (qsPos != -1)
            {
                var existingKeyPos = qsPos + 1 == url.IndexOf(key, qsPos, Common.PclExport.Instance.InvariantComparison)
                    ? qsPos
                    : url.IndexOf("&" + key, qsPos, Common.PclExport.Instance.InvariantComparison);

                if (existingKeyPos != -1)
                {
                    var endPos = url.IndexOf('&', existingKeyPos + 1);
                    if (endPos == -1)
                        endPos = url.Length;

                    var newUrl = url.Substring(0, existingKeyPos + key.Length + 1)
                                 + "="
                                 + val.UrlEncode()
                                 + url.Substring(endPos);
                    return newUrl;
                }
            }
            var prefix = qsPos == -1 ? "?" : "&";
            return url + prefix + key + "=" + val.UrlEncode();
        }

        public static string AddHashParam(this string url, string key, object val)
        {
            return url.AddHashParam(key, val.ToString());
        }

        public static string AddHashParam(this string url, string key, string val)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var prefix = url.IndexOf('#') == -1 ? "#" : "/";
            return url + prefix + key + "=" + val.UrlEncode();
        }

        public static string SetHashParam(this string url, string key, string val)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var hPos = url.IndexOf('#');
            if (hPos != -1)
            {
                var existingKeyPos = hPos + 1 == url.IndexOf(key, hPos, Common.PclExport.Instance.InvariantComparison)
                    ? hPos
                    : url.IndexOf("/" + key, hPos, Common.PclExport.Instance.InvariantComparison);

                if (existingKeyPos != -1)
                {
                    var endPos = url.IndexOf('/', existingKeyPos + 1);
                    if (endPos == -1)
                        endPos = url.Length;

                    var newUrl = url.Substring(0, existingKeyPos + key.Length + 1)
                                 + "="
                                 + val.UrlEncode()
                                 + url.Substring(endPos);
                    return newUrl;
                }
            }
            var prefix = url.IndexOf('#') == -1 ? "#" : "/";
            return url + prefix + key + "=" + val.UrlEncode();
        }

        public static string GetJsonFromUrl(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl(MimeTypes.JSON, requestFilter, responseFilter);
        }

        public static Task<string> GetJsonFromUrlAsync(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrlAsync(MimeTypes.JSON, requestFilter, responseFilter);
        }

        public static string GetXmlFromUrl(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl(MimeTypes.XML, requestFilter, responseFilter);
        }

        public static Task<string> GetXmlFromUrlAsync(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrlAsync(MimeTypes.XML, requestFilter, responseFilter);
        }

        public static string GetCsvFromUrl(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl(MimeTypes.CSV, requestFilter, responseFilter);
        }

        public static Task<string> GetCsvFromUrlAsync(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrlAsync(MimeTypes.CSV, requestFilter, responseFilter);
        }

        public static string GetStringFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> GetStringFromUrlAsync(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostStringToUrl(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostStringToUrlAsync(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostToUrl(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: formData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostToUrlAsync(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: formData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostToUrl(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrl(url, method: "POST",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostToUrlAsync(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrlAsync(url, method: "POST",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostJsonToUrl(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: json, contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostJsonToUrlAsync(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST", requestBody: json, contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToJson(), contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostJsonToUrlAsync(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST", requestBody: data.ToJson(), contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostXmlToUrl(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: xml, contentType: MimeTypes.XML, accept: MimeTypes.XML,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostXmlToUrlAsync(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST", requestBody: xml, contentType: MimeTypes.XML, accept: MimeTypes.XML,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostCsvToUrl(this string url, string csv,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: csv, contentType: MimeTypes.CSV, accept: MimeTypes.CSV,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostCsvToUrlAsync(this string url, string csv,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST", requestBody: csv, contentType: MimeTypes.CSV, accept: MimeTypes.CSV,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutStringToUrl(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutStringToUrlAsync(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutToUrl(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: formData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutToUrlAsync(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: formData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutToUrl(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrl(url, method: "PUT",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutToUrlAsync(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrlAsync(url, method: "PUT",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutJsonToUrl(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: json, contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutJsonToUrlAsync(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT", requestBody: json, contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: data.ToJson(), contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutJsonToUrlAsync(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT", requestBody: data.ToJson(), contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutXmlToUrl(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: xml, contentType: MimeTypes.XML, accept: MimeTypes.XML,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutXmlToUrlAsync(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT", requestBody: xml, contentType: MimeTypes.XML, accept: MimeTypes.XML,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutCsvToUrl(this string url, string csv,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: csv, contentType: MimeTypes.CSV, accept: MimeTypes.CSV,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutCsvToUrlAsync(this string url, string csv,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT", requestBody: csv, contentType: MimeTypes.CSV, accept: MimeTypes.CSV,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PatchStringToUrl(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PATCH",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PatchStringToUrlAsync(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PATCH",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PatchToUrl(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PATCH",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: formData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PatchToUrlAsync(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PATCH",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: formData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PatchToUrl(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrl(url, method: "PATCH",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PatchToUrlAsync(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrlAsync(url, method: "PATCH",
                contentType: MimeTypes.FORM_URL_ENCODED, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PatchJsonToUrl(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PATCH", requestBody: json, contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PatchJsonToUrlAsync(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PATCH", requestBody: json, contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PatchJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PATCH", requestBody: data.ToJson(), contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PatchJsonToUrlAsync(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PATCH", requestBody: data.ToJson(), contentType: MimeTypes.JSON, accept: MimeTypes.JSON,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string DeleteFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "DELETE", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> DeleteFromUrlAsync(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "DELETE", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string OptionsFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "OPTIONS", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> OptionsFromUrlAsync(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "OPTIONS", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string HeadFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "HEAD", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> HeadFromUrlAsync(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "HEAD", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string SendStringToUrl(this string url, string method = null,
            string requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;
            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = accept;
            Common.PclExport.Instance.AddCompression(webReq);

            requestFilter?.Invoke(webReq);

            if (ResultsFilter != null)
            {
                return ResultsFilter.GetString(webReq, requestBody);
            }

            if (requestBody != null)
            {
                using (var reqStream = Common.PclExport.Instance.GetRequestStream(webReq))
                using (var writer = new StreamWriter(reqStream, UseEncoding))
                {
                    writer.Write(requestBody);
                }
            }

            using (var webRes = Common.PclExport.Instance.GetResponse(webReq))
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream, UseEncoding))
            {
                responseFilter?.Invoke((HttpWebResponse)webRes);

                return reader.ReadToEnd();
            }
        }

        public static Task<string> SendStringToUrlAsync(this string url, string method = null, string requestBody = null,
            string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null,
            Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;
            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = accept;
            Common.PclExport.Instance.AddCompression(webReq);

            requestFilter?.Invoke(webReq);

            if (ResultsFilter != null)
            {
                var result = ResultsFilter.GetString(webReq, requestBody);
                var tcsResult = new TaskCompletionSource<string>();
                tcsResult.SetResult(result);
                return tcsResult.Task;
            }

            if (requestBody != null)
            {
                using (var reqStream = Common.PclExport.Instance.GetRequestStream(webReq))
                using (var writer = new StreamWriter(reqStream, UseEncoding))
                {
                    writer.Write(requestBody);
                }
            }

            var taskWebRes = webReq.GetResponseAsync();
            var tcs = new TaskCompletionSource<string>();

            taskWebRes.ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    tcs.SetException(task.Exception);
                    return;
                }
                if (task.IsCanceled)
                {
                    tcs.SetCanceled();
                    return;
                }

                var webRes = task.Result;
                responseFilter?.Invoke((HttpWebResponse)webRes);

                using (var stream = webRes.GetResponseStream())
                using (var reader = new StreamReader(stream, UseEncoding))
                {
                    tcs.SetResult(reader.ReadToEnd());
                }
            });

            return tcs.Task;
        }

        public static byte[] GetBytesFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.SendBytesToUrl(accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<byte[]> GetBytesFromUrlAsync(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.SendBytesToUrlAsync(accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static byte[] PostBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrl(url, method: "POST",
                contentType: contentType, requestBody: requestBody,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<byte[]> PostBytesToUrlAsync(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrlAsync(url, method: "POST",
                contentType: contentType, requestBody: requestBody,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static byte[] PutBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrl(url, method: "PUT",
                contentType: contentType, requestBody: requestBody,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<byte[]> PutBytesToUrlAsync(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrlAsync(url, method: "PUT",
                contentType: contentType, requestBody: requestBody,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static byte[] SendBytesToUrl(this string url, string method = null,
            byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;

            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = accept;
            Common.PclExport.Instance.AddCompression(webReq);

            requestFilter?.Invoke(webReq);

            if (ResultsFilter != null)
            {
                return ResultsFilter.GetBytes(webReq, requestBody);
            }

            if (requestBody != null)
            {
                using (var req = Common.PclExport.Instance.GetRequestStream(webReq))
                {
                    req.Write(requestBody, 0, requestBody.Length);
                }
            }

            using (var webRes = Common.PclExport.Instance.GetResponse(webReq))
            {
                responseFilter?.Invoke((HttpWebResponse)webRes);

                using (var stream = webRes.GetResponseStream())
                {
                    return stream.ReadFully();
                }
            }
        }

        public static Task<byte[]> SendBytesToUrlAsync(this string url, string method = null,
            byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;
            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = accept;
            Common.PclExport.Instance.AddCompression(webReq);

            requestFilter?.Invoke(webReq);

            if (ResultsFilter != null)
            {
                var result = ResultsFilter.GetBytes(webReq, requestBody);
                var tcsResult = new TaskCompletionSource<byte[]>();
                tcsResult.SetResult(result);
                return tcsResult.Task;
            }

            if (requestBody != null)
            {
                using (var req = Common.PclExport.Instance.GetRequestStream(webReq))
                {
                    req.Write(requestBody, 0, requestBody.Length);
                }
            }

            var taskWebRes = webReq.GetResponseAsync();
            var tcs = new TaskCompletionSource<byte[]>();

            taskWebRes.ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    tcs.SetException(task.Exception);
                    return;
                }
                if (task.IsCanceled)
                {
                    tcs.SetCanceled();
                    return;
                }

                var webRes = task.Result;
                responseFilter?.Invoke((HttpWebResponse)webRes);

                using (var stream = webRes.GetResponseStream())
                {
                    tcs.SetResult(stream.ReadFully());
                }
            });

            return tcs.Task;
        }

        public static bool IsAny300(this Exception ex)
        {
            var status = ex.GetStatus();
            return status >= HttpStatusCode.MultipleChoices && status < HttpStatusCode.BadRequest;
        }

        public static bool IsAny400(this Exception ex)
        {
            var status = ex.GetStatus();
            return status >= HttpStatusCode.BadRequest && status < HttpStatusCode.InternalServerError;
        }

        public static bool IsAny500(this Exception ex)
        {
            var status = ex.GetStatus();
            return status >= HttpStatusCode.InternalServerError && (int)status < 600;
        }

        public static bool IsNotModified(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.NotModified;
        }

        public static bool IsBadRequest(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.BadRequest;
        }

        public static bool IsNotFound(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.NotFound;
        }

        public static bool IsUnauthorized(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.Unauthorized;
        }

        public static bool IsForbidden(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.Forbidden;
        }

        public static bool IsInternalServerError(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.InternalServerError;
        }

        public static HttpStatusCode? GetResponseStatus(this string url)
        {
            try
            {
                var webReq = (HttpWebRequest)WebRequest.Create(url);
                using (var webRes = Common.PclExport.Instance.GetResponse(webReq))
                {
                    var httpRes = webRes as HttpWebResponse;
                    return httpRes?.StatusCode;
                }
            }
            catch (Exception ex)
            {
                return ex.GetStatus();
            }
        }

        public static HttpStatusCode? GetStatus(this Exception ex)
        {
            if (ex == null)
                return null;

            var webEx = ex as WebException;
            if (webEx != null)
                return GetStatus(webEx);

            var hasStatus = ex as IHasStatusCode;
            if (hasStatus != null)
                return (HttpStatusCode)hasStatus.StatusCode;

            return null;
        }

        public static HttpStatusCode? GetStatus(this WebException webEx)
        {
            var httpRes = webEx?.Response as HttpWebResponse;
            return httpRes?.StatusCode;
        }

        public static bool HasStatus(this Exception ex, HttpStatusCode statusCode)
        {
            return GetStatus(ex) == statusCode;
        }

        public static string GetResponseBody(this Exception ex)
        {
            var webEx = ex as WebException;
            if (webEx == null || webEx.Response == null
#if !(SL5 || PCL || NETSTANDARD1_1)
                || webEx.Status != WebExceptionStatus.ProtocolError
#endif
            ) return null;

            var errorResponse = (HttpWebResponse)webEx.Response;
            using (var reader = new StreamReader(errorResponse.GetResponseStream(), UseEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        public static string ReadToEnd(this WebResponse webRes)
        {
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream, UseEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        public static IEnumerable<string> ReadLines(this WebResponse webRes)
        {
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream, UseEncoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static HttpWebResponse GetErrorResponse(this string url)
        {
            try
            {
                var webReq = WebRequest.Create(url);
                using (var webRes = Common.PclExport.Instance.GetResponse(webReq))
                {
                    webRes.ReadToEnd();
                    return null;
                }
            }
            catch (WebException webEx)
            {
                return (HttpWebResponse)webEx.Response;
            }
        }

        public static Task<Stream> GetRequestStreamAsync(this WebRequest request)
        {
            return GetRequestStreamAsync((HttpWebRequest)request);
        }

        public static Task<Stream> GetRequestStreamAsync(this HttpWebRequest request)
        {
            var tcs = new TaskCompletionSource<Stream>();

            try
            {
                request.BeginGetRequestStream(iar =>
                {
                    try
                    {
                        var response = request.EndGetRequestStream(iar);
                        tcs.SetResult(response);
                    }
                    catch (Exception exc)
                    {
                        tcs.SetException(exc);
                    }
                }, null);
            }
            catch (Exception exc)
            {
                tcs.SetException(exc);
            }

            return tcs.Task;
        }

        public static Task<TBase> ConvertTo<TDerived, TBase>(this Task<TDerived> task) where TDerived : TBase
        {
            var tcs = new TaskCompletionSource<TBase>();
            task.ContinueWith(t => tcs.SetResult(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
            task.ContinueWith(t => tcs.SetException(t.Exception.InnerExceptions), TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(t => tcs.SetCanceled(), TaskContinuationOptions.OnlyOnCanceled);
            return tcs.Task;
        }

        public static Task<WebResponse> GetResponseAsync(this WebRequest request)
        {
            return GetResponseAsync((HttpWebRequest)request).ConvertTo<HttpWebResponse, WebResponse>();
        }

        public static Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request)
        {
            var tcs = new TaskCompletionSource<HttpWebResponse>();

            try
            {
                request.BeginGetResponse(iar =>
                {
                    try
                    {
                        var response = (HttpWebResponse)request.EndGetResponse(iar);
                        tcs.SetResult(response);
                    }
                    catch (Exception exc)
                    {
                        tcs.SetException(exc);
                    }
                }, null);
            }
            catch (Exception exc)
            {
                tcs.SetException(exc);
            }

            return tcs.Task;
        }

        public static void UploadFile(this WebRequest webRequest, Stream fileStream, string fileName, string mimeType,
            string accept = null, Action<HttpWebRequest> requestFilter = null, string method = "POST")
        {
            var httpReq = (HttpWebRequest)webRequest;
            httpReq.Method = method;

            if (accept != null)
                httpReq.Accept = accept;

            requestFilter?.Invoke(httpReq);

            var boundary = Guid.NewGuid().ToString("N");

            httpReq.ContentType = "multipart/form-data; boundary=\"" + boundary + "\"";

            var boundarybytes = ("\r\n--" + boundary + "--\r\n").ToAsciiBytes();

            var headerTemplate = "\r\n--" + boundary +
                                 "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n";

            var header = string.Format(headerTemplate, fileName, mimeType);

            var headerbytes = header.ToAsciiBytes();

            var contentLength = fileStream.Length + headerbytes.Length + boundarybytes.Length;
            Common.PclExport.Instance.InitHttpWebRequest(httpReq,
                contentLength: contentLength, allowAutoRedirect: false, keepAlive: false);

            if (ResultsFilter != null)
            {
                ResultsFilter.UploadStream(httpReq, fileStream, fileName);
                return;
            }

            using (var outputStream = Common.PclExport.Instance.GetRequestStream(httpReq))
            {
                outputStream.Write(headerbytes, 0, headerbytes.Length);

                fileStream.CopyTo(outputStream, 4096);

                outputStream.Write(boundarybytes, 0, boundarybytes.Length);

                Common.PclExport.Instance.CloseStream(outputStream);
            }
        }

        public static void UploadFile(this WebRequest webRequest, Stream fileStream, string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            var mimeType = MimeTypes.GetMimeType(fileName);
            if (mimeType == null)
                throw new ArgumentException("Mime-type not found for file: " + fileName);

            UploadFile(webRequest, fileStream, fileName, mimeType);
        }

        public static string PostXmlToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToXml(), contentType: MimeTypes.XML, accept: MimeTypes.XML,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostCsvToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToCsv(), contentType: MimeTypes.CSV, accept: MimeTypes.CSV,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutXmlToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: data.ToXml(), contentType: MimeTypes.XML, accept: MimeTypes.XML,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutCsvToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: data.ToCsv(), contentType: MimeTypes.CSV, accept: MimeTypes.CSV,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }
    }

    //Allow Exceptions to Customize HTTP StatusCode and StatusDescription returned
    public interface IHasStatusCode
    {
        int StatusCode { get; }
    }

    public interface IHasStatusDescription
    {
        string StatusDescription { get; }
    }

    public interface IHttpResultsFilter : IDisposable
    {
        string GetString(HttpWebRequest webReq, string reqBody);
        byte[] GetBytes(HttpWebRequest webReq, byte[] reqBody);
        void UploadStream(HttpWebRequest webRequest, Stream fileStream, string fileName);
    }

    public class HttpResultsFilter : IHttpResultsFilter
    {
        private readonly IHttpResultsFilter previousFilter;

        public string StringResult { get; set; }
        public byte[] BytesResult { get; set; }

        public Func<HttpWebRequest, string, string> StringResultFn { get; set; }
        public Func<HttpWebRequest, byte[], byte[]> BytesResultFn { get; set; }
        public Action<HttpWebRequest, Stream, string> UploadFileFn { get; set; }

        public HttpResultsFilter(string stringResult = null, byte[] bytesResult = null)
        {
            StringResult = stringResult;
            BytesResult = bytesResult;

            previousFilter = HttpUtils.ResultsFilter;
            HttpUtils.ResultsFilter = this;
        }

        public void Dispose()
        {
            HttpUtils.ResultsFilter = previousFilter;
        }

        public string GetString(HttpWebRequest webReq, string reqBody)
        {
            return StringResultFn != null
                ? StringResultFn(webReq, reqBody)
                : StringResult;
        }

        public byte[] GetBytes(HttpWebRequest webReq, byte[] reqBody)
        {
            return BytesResultFn != null
                ? BytesResultFn(webReq, reqBody)
                : BytesResult;
        }

        public void UploadStream(HttpWebRequest webRequest, Stream fileStream, string fileName)
        {
            UploadFileFn?.Invoke(webRequest, fileStream, fileName);
        }
    }
}