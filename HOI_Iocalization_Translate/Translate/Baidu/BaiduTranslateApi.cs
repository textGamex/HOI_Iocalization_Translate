using HOI_Iocalization_Translate.Translate.Baidu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace HOI_Iocalization_Translate.Translation.Baidu
{
    public partial class BaiduTranslationApi
    {
        private readonly string _appId;
        private readonly string _secretKey;
        private static readonly Random _random = new Random(Guid.NewGuid().ToString().GetHashCode());

        public BaiduTranslationApi(string newAppId, string newSecretKey)
        {
            _appId = newAppId.Trim() ?? throw new ArgumentNullException(nameof(newAppId));
            _secretKey = newSecretKey.Trim() ?? throw new ArgumentNullException(nameof(newSecretKey));
        }

        public BaiduTransResult GetTransResult(string query, string to)
        {
            return GetTransResult(query, "auto", to);
        }

        public BaiduTransResult GetTransResult(string query, string from, string to)
        {
            //Console.WriteLine(query);
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            string randomNumber = _random.Next().ToString();
            string sign = GetSign(query, randomNumber);
            string url = GetUrl(from.ToLower(), to.ToLower(), randomNumber, sign);
            var text = TranslateTextPost(url, GetMap(query, from.ToLower(), to.ToLower(), randomNumber, sign));
            //Console.WriteLine(text);
            return JsonConvert.DeserializeObject<BaiduTransResult>(text);
        }

        public string GetTranslate(string query, string from, string to)
        {
            string randomNumber = _random.Next().ToString();
            string sign = GetSign(query, randomNumber);
            string url = GetUrl(from.ToLower(), to.ToLower(), randomNumber, sign);
            return TranslateTextPost(url, GetMap(query, from.ToLower(), to.ToLower(), randomNumber, sign));
        }

        private string GetSign(string query, string randomNumber)
        {
            StringBuilder sb = new StringBuilder();

            _ = sb.Append(_appId).Append(query).Append(randomNumber).Append(_secretKey);
            return EncryptString(sb.ToString());
        }

        private string GetUrl(string from, string to, string randomNumber, string sign)
        {
            string url = "https://api.fanyi.baidu.com/api/trans/vip/translate";
            url += "?from=" + from;
            url += "&to=" + to;
            url += "&appid=" + _appId;
            url += "&salt=" + randomNumber;
            url += "&sign=" + sign;
            return url;
        }

        private Dictionary<string, string> GetMap(string query, string from, string to, string randomNumber, string sign)
        {
            var map = new Dictionary<string, string>()
            {
                ["q"] = query,
                ["from"] = from,
                ["to"] = to,
                ["appid"] = _appId,
                ["salt"] = randomNumber,
                ["sign"] = sign,
            };
            return map;
        }

        private string TranslateTextPost(string url, Dictionary<string, string> map)
        {
            string result;
            using (var wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                var reqparm = new System.Collections.Specialized.NameValueCollection();
                foreach (var keyPair in map)
                {
                    reqparm.Add(keyPair.Key, keyPair.Value);
                }
                byte[] responseBytes = wc.UploadValues(url, "POST", reqparm);
                result = Encoding.UTF8.GetString(responseBytes);
                return result;
            }
        }

        ///<summary>
        ///计算MD5值
        ///</summary> 
        private static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
    }
}
