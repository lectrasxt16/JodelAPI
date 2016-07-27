﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace JodelAPI
{
    public static class API
    {
        public enum PostColor
        {
            Orange,
            Yellow,
            Red,
            Blue,
            Bluegreyish,
            Green,
            Random
        }


        private const string Key = "XpOTPTszrtNioQQAnrREKwjtWESeUMlPQcsxmbkC";

        public static string AccessToken = "";
        public static string Latitude = "";
        public static string Longitude = "";
        public static string CountryCode = "";
        public static string City = "";
        private static List<Tuple<string, string, string, bool, int, string, string, Tuple<string>>> _jodelCache = new List<Tuple<string, string, string, bool, int, string, string, Tuple<string>>>(); // postid, message, hexcolor, isImage, votecount, lat, lng, name
        private static string _lastPostId = "";

        public static List<Tuple<string, string, string, bool, int, string, string, Tuple<string>>> GetFirstJodels()
        {
            string plainJson = GetPageContent("https://api.go-tellm.com/api/v2/posts/location/combo?lat=" + Latitude + "&lng=" + Longitude + "&access_token=" + AccessToken);
            JodelsFirstRound.RootObject jfr = JsonConvert.DeserializeObject<JodelsFirstRound.RootObject>(plainJson);
            List<Tuple<string, string, string, bool, int, string, string, Tuple<string>>> temp = new List<Tuple<string, string, string, bool, int, string, string, Tuple<string>>>(); // List<post_id,message>
            int i = 0;
            foreach (var item in jfr.recent)
            {
                string msg = item.message;
                bool isUrl = false;
                if (msg == "Jodel")
                {
                    msg = "http:"+item.image_url;
                    isUrl = true;
                }

                temp.Add(new Tuple<string, string, string, bool, int, string, string, Tuple<string>>(item.post_id, msg, item.color, isUrl, item.vote_count, item.location.loc_coordinates.lat.ToString(), item.location.loc_coordinates.lng.ToString(), new Tuple<string>(item.location.name)));

                i++;
            }

            _lastPostId = FilterItem(temp, temp.IndexOf(temp.Last()), false); // Set the last post_id for next jodels

            return temp;
        }

        public static List<Tuple<string, string, string, bool, int, string, string, Tuple<string>>> GetNextJodels()
        {
            List<Tuple<string, string, string, bool, int, string, string, Tuple<string>>> temp = new List<Tuple<string, string, string, bool, int, string, string, Tuple<string>>>(); // List<counter,post_id,message>
            for (int e = 0; e < 3; e++)
            {
                string plainJson = GetPageContent("https://api.go-tellm.com/api/v2/posts/location?lng=" + Longitude + "&lat=" + Latitude + "&after=" + _lastPostId + "&access_token=" + AccessToken + "&limit=1000000");
                JodelsLastRound.RootObject jlr = JsonConvert.DeserializeObject<JodelsLastRound.RootObject>(plainJson);
                int i = 0;
                foreach (var item in jlr.posts)
                {
                    string msg = item.message;
                    bool isUrl = false;
                    if (msg == "Jodel")
                    {
                        msg = "http:" + item.image_url; // WELL THERE IS NO IMAGE_URL!!!!???
                        isUrl = true;
                    }

                    temp.Add(new Tuple<string, string, string, bool, int, string, string, Tuple<string>>(item.post_id, msg, item.color, isUrl, item.vote_count, item.location.loc_coordinates.lat.ToString(), item.location.loc_coordinates.lng.ToString(), new Tuple<string>(item.location.name)));
                    i++;
                }

                _lastPostId = FilterItem(temp, temp.IndexOf(temp.Last()), false); // Set the last post_id for next jodels
            }
            return temp;
        }

        public static List<Tuple<string, string, string, bool, int, string, string, Tuple<string>>> GetAllJodels()
        {
            var allJodels = GetFirstJodels();
            allJodels.AddRange(GetNextJodels());
            _jodelCache = allJodels;
            return allJodels;
        }

        public static string FilterItem(List<Tuple<string, string, string, bool, int, string, string, Tuple<string>>> unfiltered, int index, bool filterMessage)
        {
            return !filterMessage ? unfiltered[index].Item1 : unfiltered[index].Item2;
        }

        public static void Upvote(string postId)
        {
            DateTime dt = DateTime.UtcNow;

            string stringifiedPayload =
                @"PUT%api.go-tellm.com%443%/api/v2/posts/" + postId + "/" + "upvote/%" + AccessToken + "%" + $"{dt:s}Z" + "%%";

            var keyByte = Encoding.UTF8.GetBytes(Key);
            var hmacsha1 = new HMACSHA1(keyByte);
            hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringifiedPayload));

            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("User-Agent", "Jodel/4.11.2 Dalvik/2.1.0 (Linux; U; Android 6.0.1; Nexus 5 Build/MMB29V)"); //TODO: Randomize
                client.Headers.Add("Accept", "*/*");
                client.Headers.Add("Accept-Encoding", "gzip, deflate");
                client.Headers.Add("X-Client-Type", "android_4.11.2");
                client.Headers.Add("X-Api-Version", "0.2");
                client.Headers.Add("X-Timestamp", $"{dt:s}Z");
                client.Headers.Add("X-Authorization", "HMAC " + ByteToString(hmacsha1.Hash));
                client.Headers.Add("Authorization", "Bearer " + AccessToken);
                client.Encoding = Encoding.UTF8;
                client.UploadData(
                    "https://api.go-tellm.com/api/v2/posts/" + postId + "/" + "upvote/", "PUT", new byte[] { });
            }
        }

        public static void Upvote(int indexOfItem)
        {
            string postId = FilterItem(_jodelCache, indexOfItem, false);

            using (var client = new WebClient())
            {
                client.UploadData("https://api.go-tellm.com/api/v2/posts/" + postId + "/upvote?access_token=" + AccessToken, "PUT", new byte[] { });
            }
        } // cached List<> only

        public static void Downvote(string postId)
        {
            DateTime dt = DateTime.UtcNow;

            string stringifiedPayload =
                @"PUT%api.go-tellm.com%443%/api/v2/posts/" + postId + "/" + "downvote/%" + AccessToken + "%" + $"{dt:s}Z" + "%%";

            var keyByte = Encoding.UTF8.GetBytes(Key);
            var hmacsha1 = new HMACSHA1(keyByte);
            hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringifiedPayload));

            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("User-Agent", "Jodel/4.11.2 Dalvik/2.1.0 (Linux; U; Android 6.0.1; Nexus 5 Build/MMB29V)"); //TODO: Randomize
                client.Headers.Add("Accept", "*/*");
                client.Headers.Add("Accept-Encoding", "gzip, deflate");
                client.Headers.Add("X-Client-Type", "android_4.11.2");
                client.Headers.Add("X-Api-Version", "0.2");
                client.Headers.Add("X-Timestamp", $"{dt:s}Z");
                client.Headers.Add("X-Authorization", "HMAC " + ByteToString(hmacsha1.Hash));
                client.Headers.Add("Authorization", "Bearer " + AccessToken);
                client.Encoding = Encoding.UTF8;
                client.UploadData(
                    "https://api.go-tellm.com/api/v2/posts/" + postId + "/" + "downvote/", "PUT", new byte[] { });
            }
        }

        public static void Downvote(int indexOfItem)
        {
            string postId = FilterItem(_jodelCache, indexOfItem, false);

            using (var client = new WebClient())
            {
                client.UploadData("https://api.go-tellm.com/api/v2/posts/" + postId + "/downvote?access_token=" + AccessToken, "PUT", new byte[] { });
            }
        } // cached List<> only

        public static int GetKarma()
        {
            string resp = GetPageContent("https://api.go-tellm.com/api/v2/users/karma?access_token=" + AccessToken);
            string result = resp.Substring(resp.LastIndexOf(':') + 1);
            return Convert.ToInt32(result.Replace("}","").Replace("\"",""));
        }

        public static void PostJodel(string message, string postId = null, PostColor colorParam = PostColor.Random)
        {
            DateTime dt = DateTime.UtcNow;

            var color = GetColor(colorParam);

            string jsonCommentFragment = string.Empty;
            jsonCommentFragment = @"""ancestor"": """ + postId + @""", ";

            string stringifiedPayload = @"POST%api.go-tellm.com%443%/api/v2/posts/%" + AccessToken + "%" + $"{dt:s}Z" + @"%%{""color"": """ + color + @""", " + jsonCommentFragment + @"""message"": """ + message + @""", ""location"": {""loc_accuracy"": 1, ""city"": """ + City + @""", ""loc_coordinates"": {""lat"": " + Latitude + @", ""lng"": " + Longitude + @"}, ""country"": """ + CountryCode + @""", ""name"": """ + City + @"""}}";

            string payload = @"{""color"": """ + color + @""", " + jsonCommentFragment + @"""message"": """ + message + @""", ""location"": {""loc_accuracy"": 1, ""city"": """ + City + @""", ""loc_coordinates"": " + @"{""lat"": " + Latitude + @", ""lng"": " + Longitude + @"}, ""country"": """ + CountryCode + @""", ""name"": """ + City + @"""}}";

            var keyByte = Encoding.UTF8.GetBytes(Key);
            using (var hmacsha1 = new HMACSHA1(keyByte))
            {
                hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringifiedPayload));

                GetPageContentPost("https://api.go-tellm.com/api/v2/posts/", payload, true, ByteToString(hmacsha1.Hash), $"{dt:s}Z");
            }
        }

        public static List<Tuple<string, string, string, int>> GetComments(string postId)
        {
            string plainJson = GetPageContent("https://api.go-tellm.com/api/v2/posts/"+postId+"?access_token="+AccessToken);
            Comments.RootObject com = JsonConvert.DeserializeObject<Comments.RootObject>(plainJson);

            return com.children.Select(c => new Tuple<string, string, string, int>(c.post_id, c.message, c.user_handle, c.vote_count)).ToList();
        }

        public static string GenerateAccessToken()
        {
            DateTime dt = DateTime.UtcNow;

            string deviceUid = Sha256(RandomString(5, true));

            string stringifiedPayload
                = @"POST%api.go-tellm.com%443%/api/v2/users/%%" + $"{dt:s}Z" + @"%%{""device_uid"": """ + deviceUid +
            @""", ""location"": {""city"": ""Zurich"", ""loc_accuracy"": 100, ""loc_coordinates"": {""lat"": 47.3667, ""lng"": 8.55}, ""country"": ""CH""}, " +
            @"""client_id"": ""81e8a76e-1e02-4d17-9ba0-8a7020261b26""}";

            string payload = @"{""device_uid"": """ + deviceUid + @""", ""location"": {""city"": ""Zurich"", ""loc_accuracy"": 100, ""loc_coordinates"": " +
            @"{""lat"": 47.3667, ""lng"": 8.55}, ""country"": ""CH""}, ""client_id"": ""81e8a76e-1e02-4d17-9ba0-8a7020261b26""}";

            var keyByte = Encoding.UTF8.GetBytes(Key);
            using (var hmacsha1 = new HMACSHA1(keyByte))
            {
                hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringifiedPayload));

                return GetPageContentPost("https://api.go-tellm.com/api/v2/users/", payload, false, ByteToString(hmacsha1.Hash), $"{dt:s}Z");
            }
        }

        private static string ByteToString(byte[] buff)
        {
            return buff.Aggregate("", (current, t) => current + t.ToString("X2"));
        }

        private static string GetPageContent(string link)
        {
            string html;
            WebRequest request = WebRequest.Create(link);
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(data))
            {
                html = sr.ReadToEnd();
            }
            return html;
        }

        private static string GetPageContentPost(string link, string post, bool bearer, string hmac, string timestamp)
        {
            var request = (HttpWebRequest)WebRequest.Create(link);

            var data = Encoding.UTF8.GetBytes(post);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.LongLength;
            request.UserAgent = "Jodel/4.11.2 Dalvik/2.1.0 (Linux; U; Android 6.0.1; Nexus 5 Build/MMB29V)"; //TODO: Randomize
            request.KeepAlive = true;
            request.Accept = "*/*";
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("X-Client-Type", "android_4.11.2");
            request.Headers.Add("X-Api-Version", "0.2");
            if (timestamp != null)
                request.Headers.Add("X-Timestamp", timestamp);
            if (hmac != null)
                request.Headers.Add("X-Authorization", "HMAC " + hmac);

            if (bearer)
            {
                request.Headers.Add("Authorization", "Bearer " + AccessToken);
            }
            request.ServicePoint.Expect100Continue = false;
            request.AuthenticationLevel = AuthenticationLevel.None;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (hmac != null)
            {
                var responseJson = JsonConvert.DeserializeObject<dynamic>(responseString); // ugly solution, may throw exception if no access token is responded
                responseString = responseJson.access_token;
            }
            return responseString;
        }

        private static string Sha256(string value)
        {
            StringBuilder sb = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (byte b in result)
                    sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        private static string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            for (int i = 1; i < size + 1; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

        private static string GetColor(PostColor c)
        {
            switch (c)
            {
                case PostColor.Red:
                    return "DD5F5F";
                case PostColor.Orange:
                    return "FF9908";
                case PostColor.Yellow:
                    return "FFBA00";
                case PostColor.Blue:
                    return "DD5F5F";
                case PostColor.Bluegreyish:
                    return "8ABDB0";
                case PostColor.Green:
                    return "9EC41C";
                case PostColor.Random:
                    return "FFFFFF";
                default:
                    throw new ArgumentOutOfRangeException(nameof(c), c, null);
            }
        }
    }
}