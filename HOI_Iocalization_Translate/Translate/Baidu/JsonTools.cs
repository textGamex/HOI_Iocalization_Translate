using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace HOI_Iocalization_Translate.Translation.Baidu
{
    public partial class BaiduTranslationApi
    {
        #region 解析 https://fanyi-api.baidu.com/api/trans/vip/translate 返回的json格式
        /// <summary>
        /// 辅助类
        /// </summary>
        private static class JsonTools
        {
            public const int NoError = 0;
            public const int UnauthorizedUser = 52003;

            public static string GetErrorMessage(in int errorCode)
            {
                switch (errorCode)
                {
                    case NoError: return "无错误";
                    case UnauthorizedUser: return "未授权用户, 请检查ID或秘钥是否正确或者服务是否开通";
                    case 52001: return "请求超时";
                    case 52002: return "系统错误";
                    case 54003: return "访问频率受限";
                    case 54004: return "账户余额不足";
                    default: return "未知错误";
                }
            }

            public static Dictionary<string, string> GetTranslatedMap(string serverReturnedJson)
            {
                if (serverReturnedJson == null)
                    throw new ArgumentNullException(nameof(serverReturnedJson));

                Dictionary<string, string> data = new Dictionary<string, string>();
                JObject rawJson = JObject.Parse(serverReturnedJson);
                JToken translatedTextArray = rawJson.GetValue("trans_result");

                foreach (JToken translated in translatedTextArray)
                {
                    JObject single = JObject.Parse(translated.ToString());

                    string rawText = single.GetValue("src").ToString();
                    string translatedText = single.GetValue("dst").ToString();
                   
                    data[rawText] = translatedText;                              
                }
                return data;
            }
        }
    #endregion
    }

}
