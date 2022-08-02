using HOI_Iocalization_Translate.Translation.Baidu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HOI_Iocalization_Translate.Translate
{
    public class TextData
    {
        private const string TEXT_REGEX = "(?<=\").+(?=\")";        

        private readonly FileInfo _fileInfo;
        public string FileName => _fileInfo.Name;
        private readonly List<string> _data = new List<string>();
        private string _rawData;

        public TextData(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
            _rawData = File.ReadAllText(_fileInfo.FullName);
            var list = new List<string>();
            foreach (var line in GetLocalisationFileTextList(_fileInfo.FullName))
            {
                list.AddRange(ExtractTranslationPart(line));
            }
            _data.AddRange(list.Distinct());
        }

        /// <summary>
        /// 获得一个本地化文件中的所有文本
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static List<string> GetLocalisationFileTextList(string filePath)
        {
            List<string> list = new List<string>();

            //忽略BOM头
            list.AddRange(File.ReadAllLines(filePath, new UTF8Encoding(false)));
            if (list.Count > 0 && list[0] == "l_english:")
                list.RemoveAt(0);
            Console.WriteLine($"移除空行: {list.RemoveAll((str) => str.Trim() == string.Empty)}");
            return GetNeedTranslateTextList(list);
        }

        /// <summary>
        /// 提取 "" 中的字符串
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        private static List<string> GetNeedTranslateTextList(List<string> strings)
        {
            List<string> list = new List<string>(strings.Count);
            foreach (string input in strings)
            {
                list.Add(Regex.Match(input, TEXT_REGEX, RegexOptions.Compiled).Value);
            }
            return list;
        }

        /// <summary>
        /// 分割翻译部分
        /// </summary>
        /// <param name="text">需要翻译的一整段文本</param>
        /// <returns></returns>
        private static List<string> ExtractTranslationPart(string text)
        {
            var sb = new StringBuilder();
            var list = new List<string>();
            var chars = text.ToCharArray();
            char c;

            void Settlement()
            {
                string s;
                if ((s = sb.ToString()) == string.Empty)
                {
                    sb.Clear();
                    return;
                }
                list.Add(s);
                sb.Clear();
            }

            for (int i = 0; i < chars.Length; ++i)
            {
                c = chars[i];
                //处理引用
                if (c == '[')
                {
                    Settlement();
                    while (i + 1 < chars.Length && chars[++i] != ']')
                        ;
                    continue;
                }

                //处理换行符
                if (c == '\\')
                {
                    if (i + 1 < chars.Length && chars[i + 1] == 'n')
                    {
                        //跳过 n
                        ++i;
                        Settlement();
                        continue;
                    }
                }

                //处理颜色代码
                if (c == '§')
                {
                    if (i + 1 < chars.Length && IsColorCode(chars[i + 1]))
                    {
                        //跳过颜色代码
                        ++i;
                        Settlement();
                        bool isColorEndChar = false;
                        while (i + 1 < chars.Length)
                        {
                            c = chars[++i];
                            if (c != '§')
                            {
                                if (c == '[')
                                {
                                    Settlement();
                                    while (i + 1 < chars.Length && chars[++i] != ']')
                                        ;
                                    continue;
                                }
                                sb.Append(c);
                            }
                            else
                            {
                                if (i + 1 < chars.Length && chars[i + 1] == '!')
                                {
                                    ++i;
                                    Settlement();
                                    isColorEndChar = true;
                                    break;
                                }
                                else
                                {
                                    sb.Append(c);
                                }
                            }
                        }
                        if (isColorEndChar)
                        {
                            continue;
                        }
                    }
                }
                sb.Append(c);
            }
            Settlement();
            return list;
        }

        private static bool IsColorCode(char code)
        {
            //颜色代码来自 https://hoi4.paradoxwikis.com/Localisation
            char[] codes = { 'C', 'L', 'W', 'B', 'G', 'R', 'b', 'g', 'Y', 'H', 'T', 't' };
            return codes.Contains(code) || (code >= '0' && code <= '9');
        }

        public void TranslateText(BaiduTranslationApi api, string language)
        {
            //uint i = 0;
            var needTranslateList = JoinTextByCharactersNumber(_data, 6000);
            var translationList = new List<Baidu.BaiduTransResult>();

            foreach (var data in needTranslateList)
            {
                translationList.Add(api.GetTransResult(data, language));
                System.Threading.Thread.Sleep(100);
            }

            foreach (var baiduTransResult in translationList)
            {
                foreach (var transResult in baiduTransResult?.TransResult ?? new List<Baidu.TransResult>())
                {
                    _rawData = _rawData.Replace(transResult.Src, transResult.Dst);
                }
            }

            using (var stream = new FileStream($@"C:\Users\Programmer\Desktop\新建文件夹\{_fileInfo.Name}.txt", FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(_rawData);
            }
        }

        ///<summary>
        /// 根据限制的字符数拼接字符串,返回的字符串长度 <= charactersNumber
        /// </summary>
        /// <param name="stringsList"></param>
        /// <param name="charactersNumber">最大字符数</param>
        /// <returns></returns>
        private static List<string> JoinTextByCharactersNumber(List<string> stringsList, int charactersNumber)
        {
            var sb = new StringBuilder(charactersNumber);
            const string newLine = "\n";
            var list = new List<string>();

            for (int i = 0; i < stringsList.Count; ++i)
            {
                if (stringsList[i].Length > charactersNumber)
                {
                    Console.WriteLine("跳过拼接");
                    continue;
                }

                if (sb.Length + stringsList[i].Length + newLine.Length > charactersNumber)
                {
                    list.Add(sb.ToString());
                    sb.Clear();
                    --i;
                    continue;
                }
                if (sb.Length + stringsList[i].Length == charactersNumber)
                {
                    sb.Append(stringsList[i]);
                    list.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }
                sb.Append(stringsList[i]).Append(newLine);
            }
            list.Add(sb.ToString());
            return list;
        }
    }
}
