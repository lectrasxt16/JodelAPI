using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace JodelAPI.Internal
{
    internal class MyWebHeaderCollection : System.Collections.IEnumerable
    {
        public System.Collections.IEnumerator GetEnumerator()
        {
            return headers.GetEnumerator();
        }
        public NameValueCollection headers = new NameValueCollection();
        public void Add(MyWebHeaderCollection collection)
        {
            headers.Add(collection.headers);
        }
        public void Add(string key, string value)
        {
            headers.Add(key, value);
        }
        public void Remove(string key)
        {
            headers.Remove(key);
        }

    }

    internal class MyWebClient : System.IDisposable
    {
        public Encoding Encoding = Encoding.UTF8;
        public MyWebHeaderCollection Headers = new MyWebHeaderCollection();
        public void Dispose()
        {

        }
        /*
        protected WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.ServicePoint.Expect100Continue = false;
            return request;
        }
        */
        public string DownloadString(string url)
        {
            HttpWebRequest wrq = WebRequest.CreateHttp(url);
            foreach (string str in Headers.headers.AllKeys)
            {
                wrq.Headers[str] = Headers.headers[str];
            }
            WebResponse wr = wrq.GetResponseAsync().Result;
            Stream s = null;
            if (wr.Headers[HttpResponseHeader.ContentEncoding] == "gzip")
            {
                s = new GZipStream(wr.GetResponseStream(), CompressionMode.Decompress);
            }
            else if (wr.Headers[HttpResponseHeader.ContentEncoding] == "deflate")
            {
                s = new DeflateStream(wr.GetResponseStream(), CompressionMode.Decompress);
            }
            else
            {
                s = wr.GetResponseStream();
            }
            return new StreamReader(s, Encoding).ReadToEnd();
        }
        public string UploadString(string url, string data)
        {
            try
            {
                WebRequest wrq = WebRequest.CreateHttp(url);
                wrq.Method = "POST";
                StreamWriter w = new StreamWriter(wrq.GetRequestStreamAsync().Result, Encoding);
                w.Write(data);
                w.Flush();
                wrq.ContentType = "application/x-www-form-urlencoded";
                foreach (string str in Headers.headers.AllKeys)
                {
                    wrq.Headers[str] = Headers.headers[str];
                }
                WebResponse wr = wrq.GetResponseAsync().Result;
                Stream s = null;
                if (wr.Headers[HttpResponseHeader.ContentEncoding] == "gzip")
                {
                    s = new GZipStream(wr.GetResponseStream(), CompressionMode.Decompress);
                }
                else if (wr.Headers[HttpResponseHeader.ContentEncoding] == "deflate")
                {
                    s = new DeflateStream(wr.GetResponseStream(), CompressionMode.Decompress);
                }
                else
                {
                    s = wr.GetResponseStream();
                }
                return new StreamReader(s, Encoding).ReadToEnd();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string UploadString(string url, string method, string data)
        {
            try
            {
                WebRequest wrq = WebRequest.CreateHttp(url);
                wrq.Method = method;
                StreamWriter w = new StreamWriter(wrq.GetRequestStreamAsync().Result, Encoding);
                w.Write(data);
                w.Flush();
                wrq.ContentType = "application/x-www-form-urlencoded";
                foreach (string str in Headers.headers.AllKeys)
                {
                    wrq.Headers[str] = Headers.headers[str];
                }
                WebResponse wr = wrq.GetResponseAsync().Result;
                Stream s = null;
                if (wr.Headers[HttpResponseHeader.ContentEncoding] == "gzip")
                {
                    s = new GZipStream(wr.GetResponseStream(), CompressionMode.Decompress);
                }
                else if (wr.Headers[HttpResponseHeader.ContentEncoding] == "deflate")
                {
                    s = new DeflateStream(wr.GetResponseStream(), CompressionMode.Decompress);
                }
                else
                {
                    s = wr.GetResponseStream();
                }
                return new StreamReader(s, Encoding).ReadToEnd();
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public string UploadData(string url, byte[] data)
        {
            try
            {
                WebRequest wrq = WebRequest.CreateHttp(url);
                wrq.Method = "POST";
                Stream w = wrq.GetRequestStreamAsync().Result;
                w.Write(data, 0, data.Length);
                w.Flush();
                wrq.ContentType = "application/x-www-form-urlencoded";
                foreach (string str in Headers.headers.AllKeys)
                {
                    wrq.Headers[str] = Headers.headers[str];
                }
                WebResponse wr = wrq.GetResponseAsync().Result;
                Stream s = null;
                if (wr.Headers[HttpResponseHeader.ContentEncoding] == "gzip")
                {
                    s = new GZipStream(wr.GetResponseStream(), CompressionMode.Decompress);
                }
                else if (wr.Headers[HttpResponseHeader.ContentEncoding] == "deflate")
                {
                    s = new DeflateStream(wr.GetResponseStream(), CompressionMode.Decompress);
                }
                else
                {
                    s = wr.GetResponseStream();
                }
                return new StreamReader(s, Encoding).ReadToEnd();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public string UploadData(string url, string method, byte[] data)
        {
            try
            {
                WebRequest wrq = WebRequest.CreateHttp(url);
                wrq.Method = method;
                Stream w = wrq.GetRequestStreamAsync().Result;
                w.Write(data, 0, data.Length);
                w.Flush();
                wrq.ContentType = "application/x-www-form-urlencoded";
                foreach (string str in Headers.headers.AllKeys)
                {
                    wrq.Headers[str] = Headers.headers[str];
                }
                WebResponse wr = wrq.GetResponseAsync().Result;
                Stream s = null;
                if (wr.Headers[HttpResponseHeader.ContentEncoding] == "gzip")
                {
                    s = new GZipStream(wr.GetResponseStream(), CompressionMode.Decompress);
                }
                else if (wr.Headers[HttpResponseHeader.ContentEncoding] == "deflate")
                {
                    s = new DeflateStream(wr.GetResponseStream(), CompressionMode.Decompress);
                }
                else
                {
                    s = wr.GetResponseStream();
                }
                return new StreamReader(s, Encoding).ReadToEnd();
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
    internal class JodelWebClient : MyWebClient
    {
        internal static JodelWebClient GetJodelWebClientWithHeaders(DateTime time, string stringifiedPayload, string accesstoken = "", bool addBearer = false, HttpMethod method = null)
        {
            JodelWebClient client = new JodelWebClient();

            var headers = Constants.Header;

            headers.Remove("X-Authorization");
            headers.Remove("X-Timestamp");
            headers.Remove("Authorization");

            var keyByte = Encoding.UTF8.GetBytes(Constants.Key);
            var hmacsha1 = new HMACSHA1(keyByte);
            hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringifiedPayload));

            headers.Add("X-Timestamp", $"{time:s}Z");
            headers.Add("X-Authorization", "HMAC " + hmacsha1.Hash.Aggregate("", (current, t) => current + t.ToString("X2")));

            if (addBearer)
            {
                headers.Add("Authorization", "Bearer " + accesstoken);
            }

            client.Headers.Add(headers);

            if (method != HttpMethod.Get)
                client.Headers.Add(Constants.JsonHeader);

            client.Encoding = Encoding.UTF8;

            return client;
        }

    }
}
