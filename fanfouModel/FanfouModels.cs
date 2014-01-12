using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Serialization;
using System.IO;
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Json;

namespace fanfouModel
{
    /// <summary>
    /// 文本类型返回报文
    /// </summary>
    [Serializable]
    public class ReturnObject
    {
        //Properties
        public int Code { set; get; }
        public string Text { set; get; }

        public ReturnObject(int c, string t)
        {
            this.Code = c;
            this.Text = t;
        }
        public ReturnObject()
        {
            this.Code = 99999;
            this.Text = string.Empty;
        }
    }
    public sealed class PageInfo
    {
        public int pageIndex { set; get; }
        public int pageSize { set; get; }
        public int dataCount { set; get; }

        public int pageCount
        {
            get
            {
                if (pageSize > 0)
                {
                    double dpagecount = (double)dataCount / (double)pageSize;
                    return Convert.ToInt32(Math.Ceiling(dpagecount));
                }
                else
                {
                    return 0;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}/{1} 共{2}条", pageIndex, pageCount, dataCount);
        }
    }
    [Serializable]
    [DataContract]
    public class APIParameter
    {
        //Properties
        [DataMember(Order = 0,IsRequired = true)]
        public string Name { set; get; }
        [DataMember(Order = 1, IsRequired = false)]
        public string FormatPattern { set; get; }
        [DataMember(Order = 2, IsRequired = false)]
        public bool Optional { set; get; }
        [DataMember(Order = 3, IsRequired = true)]
        public string Value { set; get; }
        [DataMember(Order = 4, IsRequired = false)]
        public bool RequiredEncodingParse { set; get; }

        public APIParameter(string n,string v)
        {
            this.Name = n;
            this.Value = v;
            this.FormatPattern = string.Format("{0}=$", Name);
            this.Optional = false;
            this.RequiredEncodingParse = true;
        }
        public APIParameter(string n, string v, bool o)
        {
            this.Name = n;
            this.Value = v;
            this.FormatPattern = string.Format("{0}=$", Name);
            this.Optional = o;
            this.RequiredEncodingParse = true;
        }
        public APIParameter(string n, string v, bool o,bool r)
        {
            this.Name = n;
            this.Value = v;
            this.FormatPattern = string.Format("{0}=$", Name);
            this.Optional = o;
            this.RequiredEncodingParse = r;
        }
        public override string ToString()
        {
            return this.FormatPattern.Replace("$", HttpUtility.UrlEncode(Value));
        }
    }

    [Serializable]
    public class ReturnStatus
    {
        //Properties
        public int Code { set; get; }
        public Object Data { set; get; }

        public ReturnStatus(int c, object o)
        {
            this.Code = c;
            this.Data = o;
        }
        public ReturnStatus()
        {
            this.Code = 999;
            this.Data = null;
        }
    }
    /// <summary>
    /// 饭否API对象
    /// </summary>
    [Serializable]
    public class Api
    {
        //Properties
        public string Name { set; get; }
        public string Action { set; get; }
        public string Method { set; get; }
        public List<KeyValuePair<string,string>> Parameters { set; get; }

        public static string getQueryString(List<APIParameter> paras)
        {
            StringBuilder p = new StringBuilder();
            if (paras != null && paras.Count > 0)
            {
                paras.Sort();
                foreach (APIParameter kp in paras)
                {
                    string v = kp.RequiredEncodingParse ? UrlEncode(kp.Value) : kp.Value;
                    p.AppendFormat("{0}={1}&", kp.Name, v);
                }
                if (p.Length > 0)
                {
                    p.Remove(p.Length - 1, 1);//remove last &
                }
            }
            return p.ToString();
        }
        //TODO:handeler delegate

        /// <summary>
        /// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
        /// </summary>
        /// <param name="value">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        public static string UrlEncode(string value)
        {
            StringBuilder result = new StringBuilder();

            /*foreach (char symbol in value) {
                if (unreservedChars.IndexOf(symbol) != -1) {
                    result.Append(symbol);
                } else {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }*/
            if (string.IsNullOrEmpty(value)) return string.Empty;
            value = HttpUtility.UrlEncode(value).Replace("+", "%20");
            value = System.Text.RegularExpressions.Regex.Replace(value, "(%[0-9a-f][0-9a-f])", c => c.ToString().ToUpper());
            value = value.Replace("(", "%28").Replace(")", "%29").Replace("$", "%24").Replace("!", "%21").Replace("*", "%2A").Replace("'", "%27");
            value = value.Replace("%7E", "~");
            value = value.Replace("%253D", "%3D");
            return value;
            //return result.ToString();
        }

    }
    /// <summary>
    /// 饭否API错误对象
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApiErrorMessage
    {
        //Properties
        [DataMember(Order = 0, IsRequired = true)]
        public string request { set; get; }
        [DataMember(Order = 1, IsRequired = true)]
        public string error { set; get; }
    }
    [Serializable]
    [DataContract]
    public class UserBase
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string id { set; get; }
        [DataMember(Order = 1, IsRequired = true)]
        public string screen_name { set; get; }
        [DataMember(Order = 2, IsRequired = true)]
        public DateTime timestamp { set; get; }

        public UserBase(string id, string n)
        {
            this.id = id;
            this.screen_name = n;
            this.timestamp = DateTime.Now;
        }
        public UserBase(string id, string n, DateTime t)
        {
            this.id = id;
            this.screen_name = n;
            this.timestamp = t;
        }
    }
    [Serializable]
    [DataContract]
    public class Friendship
    {
        //Properties
        [DataMember(Order = 0, IsRequired = true)]
        public string id { set; get; }
        [DataMember(Order = 1, IsRequired = true)]
        public string screen_name { set; get; }
        [DataMember(Order = 2, IsRequired = true)]
        public string following { set; get; }
        [DataMember(Order = 3, IsRequired = true)]
        public string followed_by { set; get; }
    }
    [Serializable]
    [DataContract]
    public class Relationship
    {
        [DataMember(Order = 0, IsRequired = true)]
        public Friendship source { set; get; }
        [DataMember(Order = 1, IsRequired = true)]
        public Friendship target { set; get; }
    }
    [Serializable]
    [DataContract]
    public class Relationpack
    {
        [DataMember(Order = 0, IsRequired = true)]
        public Relationship relationship { set; get; }
    }

    [Serializable]
    [DataContract]
    public class BizInfo
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string Text { set; get; }
        [DataMember(Order = 1, IsRequired = true)]
        public DateTime CreateTime { set; get; }
        [DataMember(Order = 2, IsRequired = true)]
        public int Code { set; get; }
        [DataMember(Order = 3, IsRequired = true)]
        public int Status { set; get; }

        public BizInfo(string txt, int code)
        {
            this.Code = code;
            this.Text = txt;
            this.CreateTime = DateTime.Now;
            this.Status = 0;
        }
        public BizInfo(string txt)
        {
            this.Code = 0;
            this.Text = txt;
            this.CreateTime = DateTime.Now;
            this.Status = 0;
        }
        public BizInfo(string txt, int code, int status)
        {
            this.Code = code;
            this.Text = txt;
            this.CreateTime = DateTime.Now;
            this.Status = status;
        }
        public static string getStatusType(int Status)
        {
            string t="";
            switch (Status)
            {
                case 1: 
                    t = "涨fo";
                    break;
                case 2:
                    t = "掉fo";
                    break;
                case 3:
                    t = "注销";
                    break;
                case 4:
                    t = "删号";
                    break;
                case 5:
                    t = "回归";
                    break;
                default:
                    break;
            }
            return t;
        }
    }

    [Serializable]
    [DataContract]
    public class User
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string id { set; get; }
        [DataMember(Order = 1, IsRequired = true)]
        public string name { set; get; }
        [DataMember(Order = 2, IsRequired = true)]
        public string screen_name { set; get; }
        [DataMember(Order = 3, IsRequired = true)]
        public string location { set; get; }
        [DataMember(Order = 4, IsRequired = true)]
        public string gender { set; get; }
        [DataMember(Order = 5, IsRequired = true)]
        public string birthday { set; get; }
        [DataMember(Order = 6, IsRequired = true)]
        public string description { set; get; }
        [DataMember(Order = 7, IsRequired = true)]
        public string profile_image_url { set; get; }
        [DataMember(Order = 8, IsRequired = true)]
        public string profile_image_url_large { set; get; }
        [DataMember(Order = 9, IsRequired = true)]
        public string url { set; get; }
        [DataMember(Order = 10, IsRequired = true,Name="protected")]
        public string privacy {set;get;}
        [DataMember(Order = 11, IsRequired = true)]
        public string followers_count { set; get; }
        [DataMember(Order = 12, IsRequired = true)]
        public string friends_count { set; get; }
        [DataMember(Order = 13, IsRequired = true)]
        public string favourites_count { set; get; }
        [DataMember(Order = 14, IsRequired = true)]
        public string statuses_count { set; get; }
        [DataMember(Order = 15, IsRequired = true)]
        public string following { set; get; }
        [DataMember(Order = 16, IsRequired = true)]
        public string notifications { set; get; }
        [DataMember(Order = 17, IsRequired = true)]
        public string created_at { set; get; }
        [DataMember(Order = 18, IsRequired = true)]
        public string utc_offset { set; get; }
        [DataMember(Order = 19, IsRequired = false)]
        public string profile_background_color { set; get; }
        [DataMember(Order = 20, IsRequired = false)]
        public string profile_text_color { set; get; }
        [DataMember(Order = 21, IsRequired = false)]
        public string profile_link_color { set; get; }
        [DataMember(Order = 22, IsRequired = false)]
        public string profile_sidebar_fill_color { set; get; }
        [DataMember(Order = 23, IsRequired = false)]
        public string profile_sidebar_border_color { set; get; }
        [DataMember(Order = 24, IsRequired = false)]
        public string profile_background_image_url { set; get; }
        [DataMember(Order = 25, IsRequired = false)]
        public string profile_background_tile { set; get; }
        [DataMember(Order = 26, IsRequired = false)]
        public FanfouStatus status { set; get; }


    }
    [Serializable]
    [DataContract]
    public class FanfouStatus
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string created_at { set; get; }
        [DataMember(Order = 1, IsRequired = true)]
        public string id { set; get; }
        [DataMember(Order = 2, IsRequired = true)]
        public string rawid { set; get; }
        [DataMember(Order = 3, IsRequired = true)]
        public string text { set; get; }
        [DataMember(Order = 4, IsRequired = true)]
        public string source { set; get; }
        [DataMember(Order = 5, IsRequired = true)]
        public string truncated { set; get; }
        [DataMember(Order = 6, IsRequired = false)]
        public string in_reply_to_status_id { set; get; }
        [DataMember(Order = 7, IsRequired = false)]
        public string in_reply_to_user_id { set; get; }
        [DataMember(Order = 8, IsRequired = false)]
        public string favorited { set; get; }
        [DataMember(Order = 9, IsRequired = false)]
        public string in_reply_to_screen_name { set; get; }
        [DataMember(Order = 10, IsRequired = false)]
        public string is_self { set; get; }
        [DataMember(Order = 11, IsRequired = false)]
        public string location { set; get; }
        [DataMember(Order = 12, IsRequired = false)]
        public User user { set; get; }
    }

    /// <summary>
    /// JSON操作类
    /// </summary>
    public static class JSON
    {

        public static T parse<T>(string jsonString)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(ms);
            }
        }
        public static string stringify(object jsonObject)
        {
            using (var ms = new MemoryStream())
            {
                new DataContractJsonSerializer(jsonObject.GetType()).WriteObject(ms, jsonObject);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
