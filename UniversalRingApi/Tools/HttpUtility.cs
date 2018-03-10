using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Specialized;
using System;
using System.Threading.Tasks;

namespace UniversalRingApi.Tools
{
    /// <summary>
    /// Internal utility class for Http communication with the Ring API
    /// </summary>
    internal static class HttpUtility
    {
        private static string _applicationContentType = "application/x-www-form-urlencoded; charset=UTF-8";

        /// <summary>
        /// Performs a GET request to the provided url to return the contents
        /// </summary>
        /// <param name="url">Url of the request to make</param>
        /// <param name="cookieContainer">Cookies which have been recorded for this session</param>
        /// <param name="timeout">Timeout in milliseconds on how long the request may take. Default = 60000 = 60 seconds.</param>
        /// <returns>Contents of the result returned by the webserver</returns>
        public static async Task<string> GetContents(Uri url, CookieContainer cookieContainer, HttpWebRequest preGeneratedRequest = null, int timeout = 60000)
        {
            var responseFromServer = string.Empty;

            // Construct the HttpWebRequest - if not null we will use the supplied HttpWebRequest object - which is probably a Mock
            var request = preGeneratedRequest ?? CreateHttpWebRequestGetContents(url, cookieContainer, timeout);

            // Send the request to the webserver
            using (var response = await request.GetResponseAsync())

            // Get the stream containing content returned by the server.
            using (var dataStream = response.GetResponseStream())
            {
                if (dataStream == null) return null;

                // Open the stream using a StreamReader for easy access.
                using (var reader = new StreamReader(dataStream))
                {
                    // Read the content returned
                    responseFromServer = await reader.ReadToEndAsync();
                }
            }

            return responseFromServer;
        }

        /// <summary>
        /// Sends a POST request using the url encoded form method
        /// </summary>
        /// <param name="url">Url to POST to</param>
        /// <param name="formFields">Dictonary with key/value pairs containing the forms data to POST to the webserver</param>
        /// <param name="headerFields">NameValueCollection with the fields to add to the header sent to the server with the request</param>
        /// <param name="cookieContainer">Cookies which have been recorded for this session</param>
        /// <param name="timeout">Timeout in milliseconds on how long the request may take. Default = 60000 = 60 seconds.</param>
        /// <returns>The website contents returned by the webserver after posting the data</returns>
        public static async Task<string> FormPost(Uri url, Dictionary<string, string> formFields, NameValueCollection headerFields,
            CookieContainer cookieContainer, HttpWebRequest preGeneratedRequest = null, int timeout = 60000)
        {
            // Construct POST data
            var postDataByteArray = CreateHttpPostData(formFields);

            // Construct the HttpWebRequest - if not null we will use the supplied HttpWebRequest object - which is probably a Mock
            var request = preGeneratedRequest ?? CreateHttpWebRequestFormPost(url, headerFields, cookieContainer, postDataByteArray.Length, timeout);

            // Get the request stream
            using (var dataStream = await request.GetRequestStreamAsync())
            {
                // Write the POST data to the request stream
                await dataStream.WriteAsync(postDataByteArray, 0, postDataByteArray.Length);
            }

            // Receive the response from the webserver
            using (var response = await request.GetResponseAsync() as HttpWebResponse)
            {
                // Make sure the webserver has sent a response
                if (response == null) return null;

                using (var responseStream = response.GetResponseStream())
                {
                    // Make sure the datastream with the response is available
                    if (responseStream == null) return null;

                    using (var reader = new StreamReader(responseStream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Create the HttpPost concatenated values
        /// </summary>
        /// <param name="formFields"></param>
        /// <returns></returns>
        private static byte[] CreateHttpPostData(Dictionary<string, string> formFields)
        {
            var postData = new StringBuilder();
            foreach (var formField in formFields)
            {
                if (postData.Length > 0) postData.Append("&");
                postData.Append($"{formField.Key}={formField.Value}");
            }

            var postDataByteArray = Encoding.UTF8.GetBytes(postData.ToString());
            return postDataByteArray;
        }

        /// <summary>
        /// Create the HttpWebRequest for the GetContents functionality
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static HttpWebRequest CreateHttpWebRequestGetContents(Uri url, CookieContainer cookieContainer, int timeout = 60000)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.CookieContainer = cookieContainer;
            request.Timeout = timeout;

            return request;
        }

        /// <summary>
        /// Create the HttpWebRequest for httpPost functionality
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headerFields"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentLength"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static HttpWebRequest CreateHttpWebRequestFormPost(Uri url, NameValueCollection headerFields, CookieContainer cookieContainer, int contentLength, int timeout = 60000)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = WebRequestMethods.Http.Post;
            request.CookieContainer = cookieContainer;
            request.Timeout = timeout;
            request.Headers.Add(headerFields);
            request.ContentType = _applicationContentType;
            request.ContentLength = contentLength;

            return request;
        }

        /// <summary>
        /// Downloads the file from the provided Url
        /// </summary>
        /// <param name="url">Url to download the file from</param>
        /// <param name="cookieContainer">Cookies which have been recorded for this session</param>
        /// <param name="timeout">Timeout in milliseconds on how long the request may take. Default = 60000 = 60 seconds.</param>
        /// <returns>Byte array with the file download</returns>
        public static async Task<Stream> DownloadFile(Uri url, CookieContainer cookieContainer, HttpWebRequest preGeneratedRequest = null, int timeout = 60000)
        {
            //byte[] responseBytes = null;
            // Construct the HttpWebRequest - if not null we will use the supplied HttpWebRequest object - which is probably a Mock
            var request = preGeneratedRequest ?? CreateHttpWebRequestDownloadFile(url, cookieContainer, timeout);

            // Receive the response from the webserver
            var response = await request.GetResponseAsync() as HttpWebResponse;
            var httpResponseStream = response.GetResponseStream();

            return httpResponseStream;
        }

        private static HttpWebRequest CreateHttpWebRequestDownloadFile(Uri url, CookieContainer cookieContainer, int timeout = 60000)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.Accept = "*/*";
            request.AddRange("bytes", 0);
            request.CookieContainer = cookieContainer;
            request.Timeout = timeout;
            request.AllowAutoRedirect = true;

            return request;
        }
    }
}
