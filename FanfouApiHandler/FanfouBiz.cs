using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using OAuth;
using System.Xml;
using System.Runtime.Serialization;
using Common;
using System.Diagnostics;
using fanfouModel;


namespace FanfouBiz
{
    /// <summary>
    /// 业务操作类
    /// </summary>
    public class FanfouBizController
    {
        #region init
        
        #endregion init
        #region biz
        /// <summary>
        /// 获得所有好友数组列表
        /// 格式：ReturnObject.Code=0 Text JSON字符串
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ReturnObject getFriendList(string id)
        {
            string para = string.IsNullOrEmpty(id) ? "" : "?id=" + FanfouHelper.UrlEncode(id);
            return FanfouHelper.execFanfouAPI(Fanfou.Api["getFriendList"],para, FanfouHelper.enumHTTPContentType.NotSpecified, null);
        }
        /// <summary>
        /// 返回所有follower数字组列表
        /// 格式：ReturnObject.Code=0 Text JSON字符串
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ReturnObject getFollowerList(string id)
        {
            string para = string.IsNullOrEmpty(id) ? "" : "?id=" + FanfouHelper.UrlEncode(id);
            return FanfouHelper.execFanfouAPI(Fanfou.Api["getFollowerList"], para, FanfouHelper.enumHTTPContentType.NotSpecified, null);
        }

        public static bool friendshipExists(string user_a, string user_b)
        {
            string para = string.Format("?user_a={0}&user_b={1}", FanfouHelper.UrlEncode(user_a), FanfouHelper.UrlEncode(user_b));
            ReturnObject ro = FanfouHelper.execFanfouAPI("friendshipExists", para, FanfouHelper.enumHTTPContentType.NotSpecified, null);
            if (ro != null)
            {
                string rs = ro.Text;
                return rs == "true";
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// 查询公共timeline
        /// [API WIKI]https://github.com/FanfouAPI/FanFouAPIDoc/wiki/statuses.public-timeline
        /// 返回：
        /// [status0,status1,status2...]
        /// </summary>
        /// <param name="since_id"></param>
        /// <param name="count"></param>
        /// <param name="max_id"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static ReturnObject getPublicTimeline(string since_id, int count, string max_id, string mode)
        {
            StringBuilder para = new StringBuilder();
            para.Append("?");
            if (!string.IsNullOrEmpty(since_id))
            {
                para.AppendFormat("since_id={0}&", FanfouHelper.UrlEncode(since_id));
            }
            if (count > 0)
            {
                para.AppendFormat("count={0}&", count);
            }
            if (!string.IsNullOrEmpty(max_id))
            {
                para.AppendFormat("max_id={0}&", FanfouHelper.UrlEncode(max_id));
            }
            if (string.IsNullOrEmpty(mode))
            {
                para.Append("mode=lite");
            }
            else
            {
                para.AppendFormat("mode={0}", FanfouHelper.UrlEncode(mode));
            }
            //Console.WriteLine(para.ToString());
            return FanfouHelper.execFanfouAPI("getPublicTimeline", para.ToString(), FanfouHelper.enumHTTPContentType.NotSpecified, null);
        }
        /// <summary>
        /// 查询用户发送的消息
        /// [API WIKI]https://github.com/FanfouAPI/FanFouAPIDoc/wiki/statuses.user-timeline
        /// 返回：
        /// [status0,status1,status2...]
        /// </summary>
        /// <param name="id"></param>
        /// <param name="since_id"></param>
        /// <param name="count"></param>
        /// <param name="max_id"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static List<FanfouStatus> getUserTimeline(string id, string since_id, int count, string max_id, string mode)
        {
            if (string.IsNullOrEmpty(id))
            {
                ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, "[ERROR]未指定的用户id", true);
                return null;
            }
            StringBuilder para = new StringBuilder();
            para.Append("?");
            para.AppendFormat("id={0}&", FanfouHelper.UrlEncode(id));
            if (!string.IsNullOrEmpty(since_id))
            {
                para.AppendFormat("since_id={0}&", FanfouHelper.UrlEncode(since_id));
            }
            if (count > 0)
            {
                para.AppendFormat("count={0}&", count);
            }
            if (!string.IsNullOrEmpty(max_id))
            {
                para.AppendFormat("max_id={0}&", FanfouHelper.UrlEncode(max_id));
            }
            if (string.IsNullOrEmpty(mode))
            {
                para.Append("mode=lite");
            }
            else
            {
                para.AppendFormat("mode={0}", FanfouHelper.UrlEncode(mode));
            }
            //Console.WriteLine(para.ToString());
            ReturnObject ro= FanfouHelper.execFanfouAPI("getUserTimeline", para.ToString(), FanfouHelper.enumHTTPContentType.NotSpecified, null);
            if (ro.Code == 1)
            {
                try
                {
                    return JSON.parse<List<FanfouStatus>>(ro.Text);
                }
                catch (Exception ex)
                {
                    ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, string.Format("[ERROR]{0}", ex.Message), true);
                    return null;
                }
            }
            else
            {
                ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, string.Format("[ERROR]{0}", ro.Text), true);
                return null;
            }
        }

        /// <summary>
        /// 查询用户间的follow关系
        /// [API WIKI]https://github.com/FanfouAPI/FanFouAPIDoc/wiki/friendships.show
        /// 返回：
        /// {
        ///    "relationship":{"source":{"id":"test",
        ///                              "screen_name":"测试昵称",
        ///                              "following":"false",
        ///                              "followed_by":"false",
        ///                              "notifications_enabled":"false",
        ///                              "blocking":"true"
        ///                    },
        ///                    "target":{"id":"debug",
        ///                              "screen_name":"debug",
        ///                              "following":"false",
        ///                              "followed_by":"false"
        ///                              }
        ///    }
        /// }
        /// 失败返回错误
        /// </summary>
        /// <param name="source_id"></param>
        /// <param name="target_id"></param>
        /// <returns></returns>
        public static ReturnObject showRelationshipById(string source_id, string target_id)
        {
            string para = string.Format("?source_id={0}&target_id={1}", FanfouHelper.UrlEncode(source_id), FanfouHelper.UrlEncode(target_id));
            return FanfouHelper.execFanfouAPI("showRelationshipById", para, FanfouHelper.enumHTTPContentType.NotSpecified, null);
        }
        /// <summary>
        /// 验证是否登录返回值ro.Code=1为登录 ro.Text为当前用户信息 其他为未登录，ro.Code为错误码 ro.Text为用户信息JSON串
        /// </summary>
        /// <returns></returns>
        public static ReturnObject VerifyCredentials()
        {
            string para = "?mode=lite";
            return FanfouHelper.execFanfouAPI("verifyCredentials", para, FanfouHelper.enumHTTPContentType.NotSpecified, null);
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static ReturnObject getUser(string id, string format, string mode)
        {
            string para = string.Format("?id={0}{1}&mode={2}", id, (string.IsNullOrEmpty(format)? "" : (format == "html" ? "&format=html" : "")), mode);
            return FanfouHelper.execFanfouAPI("getUser", para, FanfouHelper.enumHTTPContentType.NotSpecified, null);
        }
        /// <summary>
        /// 发送消息
        /// [API WIKI]https://github.com/FanfouAPI/FanFouAPIDoc/wiki/statuses.update
        /// </summary>
        /// <param name="content">内容（小于140字符）</param>
        /// <param name="in_reply_to_status_id">回复的消息ID</param>
        /// <param name="in_reply_to_user_id">回复的用户ID</param>
        /// <param name="toUserName">回复中显示的用户名</param>
        /// <returns></returns>
        public static ReturnObject postStatus(string content, string in_reply_to_status_id, string in_reply_to_user_id,string toUserName)
        {
            string statPrefix = "";
            List<APIParameter> paras = new List<APIParameter>();
            if (!string.IsNullOrEmpty(toUserName))
            {
                statPrefix = string.Format("@{0} ", toUserName);
            }
            paras.Add(new APIParameter("status", statPrefix + content, false, false));//status参数不需要Encode
            if (!string.IsNullOrEmpty(in_reply_to_status_id))
            {
                paras.Add(new APIParameter("in_reply_to_status_id", in_reply_to_status_id));
            }
            if (!string.IsNullOrEmpty(in_reply_to_user_id))
            {
                paras.Add(new APIParameter("in_reply_to_user_id", in_reply_to_user_id));
            }
            content = "";
            return FanfouHelper.execFanfouAPI("postStatus", string.Empty, FanfouHelper.enumHTTPContentType.WWWFormURLEncoded, paras);
        }
        /// <summary>
        /// 发送私信
        /// [API WIKI]https://github.com/FanfouAPI/FanFouAPIDoc/wiki/direct-messages.new
        /// </summary>
        /// <param name="text">内容（小于140字符）</param>
        /// <param name="user">目标用户ID</param>
        /// <param name="in_reply_to_id">回复的私信ID</param>
        /// <returns></returns>
        public static ReturnObject postDirectMessage(string text, string user, string in_reply_to_id)
        {
            List<APIParameter> paras = new List<APIParameter>();
            if (!string.IsNullOrEmpty(in_reply_to_id))
            {
                paras.Add(new APIParameter("in_reply_to_id", in_reply_to_id));
            }
            paras.Add(new APIParameter("text", text, true, false));
            paras.Add(new APIParameter("user", user,false,false));
            text = "";
            return FanfouHelper.execFanfouAPI("postDirectMessage", string.Empty, FanfouHelper.enumHTTPContentType.WWWFormURLEncoded, paras);
        }
        #endregion biz

    }
    /// <summary>
    /// 饭否OAuth实用类
    /// </summary>
    public sealed class FanfouOAuthHelper
    {
        string apiKey = "";
        string apiKeySecret = "";
        //string requestToken = "";
        //string requestTokenSecret = "";
        string accessToken = "";
        string accessTokenSecret = "";

        OAuthBase oAuth = new OAuthBase();

        public FanfouOAuthHelper()
        {

        }
        public FanfouOAuthHelper(string key, string secret)
        {
            apiKey = key;
            apiKeySecret = secret;
        }
        private Dictionary<string, string> parseResponse(string parameters)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                    if (!string.IsNullOrEmpty(s))
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(temp[0], temp[1]);
                        }
                        else result.Add(s, string.Empty);
            }

            return result;
        }

        /// <summary>
        /// 获取Request Token，该步骤使用API Key和API Key Secret签名
        /// </summary>
        /// <param name="requestToken"></param>
        /// <param name="requestTokenSecret"></param>
        /// <returns></returns>
        public ReturnObject getRequestToken(out string requestToken, out string requestTokenSecret)
        {
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "[INFO]STEP:请求RequestToken", true);
            List<APIParameter> pars = new List<APIParameter>();
            ReturnObject rs = oauthAuthorizeRequest("requestToken", string.Empty, string.Empty, pars, out requestToken, out requestTokenSecret);
            return rs;
        }

        /// <summary>
        /// 用户确认授权
        /// </summary>
        /// <param name="requestToken"></param>
        /// <returns></returns> 
        public string authorization(string requestToken)
        {
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "[INFO]STEP:请求用户授权", true);
            //生成引导用户授权的url
            string url = Fanfou.OAuthProvider["authorize"].Action + requestToken;
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "--------------------------------", true);
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "请将下面url粘贴到浏览器中，并同意授权", true);
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.TIP, "\n小贴示：单击左上角图标，选择编辑，标记。用鼠标选择url，然后按enter键复制。打开浏览器粘贴刚才的url并按提示操作.\n", true);
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.NOTE, url, true);
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "--------------------------------", true);
            return url;
        }

        /// <summary>
        /// 换取Access Token，该步骤使用API Key、API Key Secret、Request Token和Request Token Secret签名
        /// </summary>
        /// <param name="requestToken"></param>
        /// <param name="requestTokenSecret"></param>
        /// <param name="verifyCode"></param>
        /// <param name="accesstoken"></param>
        /// <param name="accesssecret"></param>
        /// <returns></returns>
        public ReturnObject getAccessToken(string requestToken, string requestTokenSecret, string verifyCode, out string accesstoken, out string accesssecret)
        {
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "[INFO]STEP:换取AccessToken", true);
            List<APIParameter> pars = new List<APIParameter>();
            string accessTokenApiName = "accessToken";
            if (!string.IsNullOrEmpty(verifyCode))
            {
                pars.Add(new APIParameter("oauth_verifier", verifyCode, true, false));
            }
            else
            {
                accessTokenApiName = "accessToken";
            }
            ReturnObject rs = oauthAuthorizeRequest(accessTokenApiName, requestToken, requestTokenSecret, pars, out accesstoken, out accesssecret);
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, string.Format("成功获得OAuth授权：\nAccessToken:{0}\nAccessTokenSecret:{1}\n", accessToken, accessTokenSecret), true);
            return rs;
        }
        private ReturnObject oauthAuthorizeRequest(string oauthProviderApiName, string rqtk, string rqsc, List<APIParameter> paras, out string token, out string secret)
        {
            token = "";
            secret = "";
            Api oauthProviderApi = Fanfou.OAuthProvider[oauthProviderApiName];
            try
            {
                Uri uri = new Uri(oauthProviderApi.Action);
                string nonce = oAuth.GenerateNonce();
                string timeStamp = oAuth.GenerateTimeStamp();
                string normalizeUrl, normalizedRequestParameters;

                // 签名
                string sig = oAuth.GenerateSignature(
                    uri,
                    apiKey,
                    apiKeySecret,
                    rqtk,
                    rqsc,
                    "GET",
                    timeStamp,
                    nonce,
                    OAuthBase.SignatureTypes.HMACSHA1,
                    out normalizeUrl,
                    out normalizedRequestParameters);
                sig = (new OAuthBase()).UrlEncode(sig);

                //构造请求Access Token的url
                StringBuilder sb = new StringBuilder(uri.ToString());
                sb.AppendFormat("?oauth_consumer_key={0}&", apiKey);
                sb.AppendFormat("oauth_nonce={0}&", nonce);
                sb.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
                sb.AppendFormat("oauth_signature={0}&", sig);
                sb.AppendFormat("oauth_timestamp={0}&", timeStamp);
                if (oauthProviderApi.Name == "accessToken")
                {
                    sb.AppendFormat("oauth_token={0}&", rqtk);
                }

                //if (!string.IsNullOrEmpty(verifyCode))
                //{
                //    sb.AppendFormat("oauth_verifier={0}&", verifyCode);
                //}
                foreach (APIParameter par in paras)
                {
                    sb.AppendFormat("{0}={1}&", par.Name, par.RequiredEncodingParse ? (new OAuthBase()).UrlEncode(par.Value) : par.Value);
                }
                sb.AppendFormat("oauth_version={0}", "1.0");

                ReturnObject rs = FanfouHelper.sendHttpRequest(sb.ToString(), FanfouHelper.enumHTTPMethod.httpGET, FanfouHelper.enumHTTPContentType.NotSpecified, string.Empty, null);
                switch (rs.Code)
                {
                    case 1: // Normal return
                        Dictionary<string, string> responseValues = parseResponse(rs.Text);
                        token = responseValues["oauth_token"];
                        secret = responseValues["oauth_token_secret"];
                        return rs;
                    case 0: // Generic HTTP error
                        return new ReturnObject(0, string.Format("访问{0}时发生错误:{1}", oauthProviderApi.Action, FanfouHelper.ExtractJSONErrorMessage(rs.Text)));
                    case -1: // Timed out
                        return rs;
                    case -2: // Internal exception
                        return new ReturnObject(-2, "Operation terminated due to internal error");
                    default:
                        return new ReturnObject(-999, "unhandled exception");
                }

            }
            catch (WebException wex)
            {
                token = "";
                secret = "";
                if (wex.Status == WebExceptionStatus.Timeout)
                {
                    //HTTP Error: Connection timed out
                    return new ReturnObject(-1, "HTTP request operation timed out");
                }
                else
                {
                    Stream resp = wex.Response.GetResponseStream();
                    if (resp != null)
                    {
                        //HTTP Error
                        return new ReturnObject(0, new StreamReader(resp).ReadToEnd());
                    }
                    else
                    {
                        //Server did not respond any data
                        return new ReturnObject(0, "Server did not respond any data");
                    }
                }
            }
            catch (Exception ex)
            {
                //An internal exception occured while accessing the API
                token = "";
                secret = "";
                return new ReturnObject(-2, ex.Message);
            }

        }
    }
    /// <summary>
    /// 饭否访问基础类
    /// </summary>
    public static class FanfouHelper
    {
        private static string consumerKey = "";
        private static string consumerSecret="";
        private static string accessToken="";
        private static string accessTokenSecret = "";

        #region 构造器
        static FanfouHelper()
        {
            //读取配置文件
            //string config_path = Server.MapPath("~");
            string config_path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string config_ini_file_name = config_path + "config.ini";
            if (File.Exists(config_ini_file_name))
            {
                INIFile ini = new INIFile(config_ini_file_name);
                consumerKey = ini.IniReadValue("Consumer", "consumerKey");
                consumerSecret = ini.IniReadValue("Consumer", "consumerSecret");
                accessToken = ini.IniReadValue("Consumer", "AccessToken");
                accessTokenSecret = ini.IniReadValue("Consumer", "AccessTokenSecret");
            }
            else
            {
                if (HttpContext.Current != null && HttpContext.Current.Session["consumerKey"] != null)
                {
                    consumerKey = HttpContext.Current.Session["consumerKey"].ToString();
                    consumerSecret = HttpContext.Current.Session["consumerSecret"].ToString();
                    accessToken = HttpContext.Current.Session["accessToken"].ToString();
                    accessTokenSecret = HttpContext.Current.Session["accessTokenSecret"].ToString();
                }
            }
        }
        //public FanfouHelper(string ck, string cs, string at, string ats)
        //{
        //    consumerKey = ck;
        //    consumerSecret = cs;
        //    accessToken = at;
        //    accessTokenSecret = ats;
        //}
        #endregion 构造器
        #region fanfou api base method
        /// <summary>
        /// 查询API余量是否够用
        /// </summary>
        /// <returns></returns>
        public static bool queryApiUsage()
        {
            Api api = Fanfou.Api["queryApiLimit"];
            string RequestURL = Fanfou.ApiBase + api.Action;
            string header = GenerateRequestHeader(RequestURL, enumHTTPMethod.httpGET, enumHTTPContentType.NotSpecified, string.Empty);
            ReturnObject rs = sendHttpRequest(RequestURL, enumHTTPMethod.httpGET, enumHTTPContentType.NotSpecified, header, string.Empty);
            try
            {
                OAuthBase oa = new OAuthBase();
                XmlDocument x = new XmlDocument();
                x.LoadXml(rs.Text);
                OA_API_REM = int.Parse(x.SelectSingleNode("/hash/remaining_hits").InnerText);
                OA_API_MAX = int.Parse(x.SelectSingleNode("/hash/hourly_limit").InnerText);
                OA_API_RESET = getLocalDateTimeFromTimeStamp(x.SelectSingleNode("/hash/reset_time_in_seconds").InnerText, LOCAL_TIME_ZONE);
                //ConsoleColorHelper.Output(ConsoleColorHelper.ConsoleOutputType.INFO, string.Format("[INFO] API用量：{0}/{1} 重置时间{2}", OA_API_REM, OA_API_MAX, OA_API_RESET), true);
                System.Diagnostics.Debug.WriteLine(string.Format("[INFO] API用量：{0}/{1} 重置时间{2}", OA_API_REM, OA_API_MAX, OA_API_RESET));
                return true;
            }
            catch (Exception)
            {
                if (rs.Code == -1)
                {
                    if (OA_API_REM > 1)
                    {
                        OA_API_REM--;
                    }
                    return true;
                }
                else
                {
                    //modPublic.doWriteConsole("An error occured when querying API balance:", ConsoleColor.Red);
                    //modPublic.doWriteConsole(ex.ToString(), ConsoleColor.Magenta);
                    return false;
                }
            }
        }
        private static string GenerateRequestHeader(string url, enumHTTPMethod httpRequestMethod, enumHTTPContentType httpContentType, string postData)
        {
            OAuthBase oa = new OAuthBase();

            //读取配置文件
            /*
            string config_path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            INIFile ini = new INIFile(config_path + "config.ini");
            string consumerKey = ini.IniReadValue("Consumer", "consumerKey");
            string consumerSecret = ini.IniReadValue("Consumer", "consumerSecret");
            string accessToken = ini.IniReadValue("Consumer", "AccessToken");
            string accessTokenSecret = ini.IniReadValue("Consumer", "AccessTokenSecret");
            */
            //string accessToken = HttpContext.Current.Session["AccessToken"]!=null ? HttpContext.Current.Session["AccessToken"].ToString() : "";
            //string accessTokenSecret = HttpContext.Current.Session["AccessTokenSecret"]!=null ? HttpContext.Current.Session["AccessTokenSecret"].ToString() : "";

            string config_path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string config_ini_file_name = config_path + "config.ini";
            if (File.Exists(config_ini_file_name))
            {
                INIFile ini = new INIFile(config_ini_file_name);
                consumerKey = ini.IniReadValue("Consumer", "consumerKey");
                consumerSecret = ini.IniReadValue("Consumer", "consumerSecret");
                accessToken = ini.IniReadValue("Consumer", "AccessToken");
                accessTokenSecret = ini.IniReadValue("Consumer", "AccessTokenSecret");
            }
            else
            {
                if (HttpContext.Current != null && HttpContext.Current.Session["consumerKey"] != null)
                {
                    consumerKey = HttpContext.Current.Session["consumerKey"].ToString();
                    consumerSecret = HttpContext.Current.Session["consumerSecret"].ToString();
                    accessToken = HttpContext.Current.Session["accessToken"].ToString();
                    accessTokenSecret = HttpContext.Current.Session["accessTokenSecret"].ToString();
                }
            }

            string method = getHTTPMethod(httpRequestMethod);
            string contenttype = getHTTPContentType(httpContentType);
            string sOABaseStr = "";
            string sOASCurl = "";
            string sOASignature = "";
            string sOAHeader = "";
            ArrayList lstOAPara = new ArrayList();
            ArrayList lstSCPara = new ArrayList();
            HMACSHA1 objHMACSHA1 = new HMACSHA1(Encoding.ASCII.GetBytes(consumerSecret + "&" + accessTokenSecret));
            sOABaseStr = "";
            sOAHeader = "";
            lstOAPara.Add("oauth_consumer_key=" + consumerKey);
            lstOAPara.Add("oauth_token=" + accessToken);
            lstOAPara.Add("oauth_signature_method=" + "HMAC-SHA1");
            lstOAPara.Add("oauth_timestamp=" + oa.GenerateTimeStamp());
            lstOAPara.Add("oauth_nonce=" + oa.GenerateNonce());
            lstOAPara.Add("oauth_version=" + "1.0");
            lstOAPara.Sort();
            foreach (string sOAP in lstOAPara)
            {
                lstSCPara.Add(sOAP); //OAuth parameters for signature calculating
                sOAHeader = sOAHeader + "," + sOAP;
            }
            sOAHeader = sOAHeader.Substring(1);
            //= = = Signature calculating progress begin = = =
            //For GET requests, all parameters in query string is attended
            if (httpRequestMethod == enumHTTPMethod.httpGET & url.Contains("?"))
            {
                string[] sQueryStrParas = url.Substring(url.IndexOf("?") + 1).Split('&');
                foreach (var sQSP in sQueryStrParas)
                {
                    lstSCPara.Add(sQSP);
                }
            }
            //For POST requests with content type of application/x-www-form-urlencoded, all parameters in post data is attended
            //Note that POST + multipart/form-data is not considered here since it is not requirement so far.
            if (httpRequestMethod == enumHTTPMethod.httpPOST & httpContentType == enumHTTPContentType.WWWFormURLEncoded)
            {
                string[] sPostParas = postData.Split('&');
                string[] sParaParts = null;
                foreach (string sPP in sPostParas)
                {
                    sParaParts = sPP.Split('=');
                    sParaParts[1] = oa.UrlEncode(sParaParts[1]);
                    lstSCPara.Add(sParaParts[0] + "=" + sParaParts[1]);
                }
            }
            lstSCPara.Sort();
            foreach (string sSCPI in lstSCPara)
            {
                sOABaseStr = sOABaseStr + "&" + sSCPI;
            }
            sOABaseStr = sOABaseStr.Substring(1);
            //The url in base string of OAuth signature calculating should not contain any parameters
            sOASCurl = url.ToLower();
            if (sOASCurl.Contains("?"))
            {
                sOASCurl = sOASCurl.Substring(0, sOASCurl.IndexOf("?"));
            }
            //Join the base string
            sOABaseStr = method + "&" + oa.UrlEncode(sOASCurl) + "&" + oa.UrlEncode(sOABaseStr);
            Debug.Print("Base string: " + sOABaseStr);
            //string accinfostack = "";

            ////////////////////
            //Calculate the signature
            sOASignature = Convert.ToBase64String(objHMACSHA1.ComputeHash(Encoding.UTF8.GetBytes(sOABaseStr)));
            //According to Fanfou API documentation, the + in signature SHOULD BE REPLACED WITH %2B.
            //I think it a weird implemention. It's a little too 2B.
            sOASignature = sOASignature.Replace("+", "%2B");
            //= = = Signature calculating progress e n d = = =
            sOAHeader = sOAHeader + "," + "oauth_signature=" + sOASignature;
            return sOAHeader;
        }
        public static string ExtractJSONErrorMessage(string s)
        {
            try
            {
                ApiErrorMessage err = JSON.parse<ApiErrorMessage>(s);
                return err.error;
            }
            catch(Exception ex){
                Console.WriteLine(ex.Message);
                try
                {
                    return ExtractErrorMessage(s);
                }
                catch(Exception exx)
                {
                    Console.WriteLine(exx.Message);
                    return s;
                }
            }
        }
        public static string ExtractErrorMessage(string s)
        {
            try
            {
                XmlDocument x = new XmlDocument();
                x.LoadXml(s);
                if (x.SelectSingleNode("/hash/error") == null == false)
                {
                    return x.SelectSingleNode("/hash/error").InnerText;
                }
                if (x.SelectSingleNode("/request/error") == null == false)
                {
                    return x.SelectSingleNode("/request/error").InnerText;
                }
                return s;
            }
            catch
            {
                return s;
            }
        }
        public static DateTime getLocalDateTimeFromTimeStamp(string s, int Region)
        {
            try
            {
                return DateTime.Parse("1970/1/1 00:00:00").AddSeconds(int.Parse(s)).AddHours(Region);
            }
            catch
            {
                return DateTime.Now;
            }
        }
        public static ReturnObject execFanfouAPI(string ApiName,string parameter, enumHTTPContentType httpContentType, List<APIParameter> postData)
        {
            Api api = Fanfou.Api[ApiName];
            if (api != null)
            {
                enumHTTPMethod httpRequestMethod = api.Method == "GET" ? enumHTTPMethod.httpGET : enumHTTPMethod.httpPOST;
                if (httpRequestMethod == enumHTTPMethod.httpGET)
                {
                    return execApiGet(api, parameter);
                }
                else
                {
                    return execApiPost(api, parameter, httpContentType, postData);
                }
            }
            else
            {
                return new ReturnObject(0, string.Format("request api [{0}] not exists ", ApiName));
            }
        }
        public static ReturnObject execFanfouAPI(Api api, string parameter, enumHTTPContentType contenttype, List<APIParameter> postData)
        {
            if (api.Method == "GET")
            {
                return execApiGet(api, parameter);
            }
            else
            {
                return execApiPost(api,parameter, contenttype, postData);
            }
        }
        public static ReturnObject execApiGet(Api api,string parameter)
        {
            //Api api = Fanfou.Api[ApiName];
            string RequestURL = (api.Action.StartsWith("http") ? "" : Fanfou.ApiBase) + api.Action + parameter;
            if (queryApiUsage())
            {
                if (OA_API_REM > 0)
                {
                    string header = GenerateRequestHeader(RequestURL, enumHTTPMethod.httpGET, enumHTTPContentType.NotSpecified, null);
                    ReturnObject rs = sendHttpRequest(RequestURL, enumHTTPMethod.httpGET, enumHTTPContentType.NotSpecified, header, null);
                    switch (rs.Code)
                    {
                        case 1: // Normal return
                            return rs;
                        case 0: // Generic HTTP error
                            return new ReturnObject(0, string.Format("访问{0}时发生错误:{1}",api.Action,ExtractJSONErrorMessage(rs.Text)));
                        case -1: // Timed out
                            return rs;
                        case -2: // Internal exception
                            return new ReturnObject(-2, "Operation terminated due to internal error");
                    }
                }
                else
                {
                    return new ReturnObject(0, "API hits ran out, try again later");
                }
            }
            else
            {
                return new ReturnObject(0, "API hits query failed, try again later");
            }
            //Code below should be never executed, just to clear the compiler warning
            return new ReturnObject(-2, "Function run out of controlled route");
        }

        public static ReturnObject execApiPost(Api api,string parameter, enumHTTPContentType httpContentType, List<APIParameter> postParms)
        {
            string postData = serializeParameters(postParms);
            //Api api = Fanfou.Api[ApiName];
            string RequestURL = (api.Action.StartsWith("http") ? "" : Fanfou.ApiBase) + api.Action + parameter;
            if (queryApiUsage())
            {
                if (OA_API_REM > 0)
                {
                    string header = GenerateRequestHeader(RequestURL, enumHTTPMethod.httpPOST, httpContentType, postData);
                    ReturnObject rs = sendHttpRequest(RequestURL, enumHTTPMethod.httpPOST, httpContentType, header, postData);
                    switch (rs.Code)
                    {
                        case 1: // Normal return
                            return rs;
                        case 0: // Generic HTTP error
                            return new ReturnObject(0, string.Format("访问{0}时发生错误:{1}", api.Action, ExtractJSONErrorMessage(rs.Text)));
                        case -1: // Timed out
                            return rs;
                        case -2: // Internal exception
                            return new ReturnObject(-2, "Operation terminated due to internal error");
                    }
                }
                else
                {
                    return new ReturnObject(0, "API hits ran out, try again later");
                }
            }
            else
            {
                return new ReturnObject(0, "API hits query failed, try again later");
            }
            //Code below should be never executed, just to clear the compiler warning
            return new ReturnObject(-2, "Function run out of controlled route");
        }
        //private static string getI
        #endregion fanfou api base method
        #region CONSTANS
        public static int[] iSYS_ControlChar = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 127 };
        public static int HTTP_TIMEOUT = 30;
        public static int OA_API_REM = 1500;
        public static int OA_API_MAX;
        public static DateTime OA_API_RESET;
        public static int LOCAL_TIME_ZONE = 8;//东八区
        public enum enumHTTPMethod
        {
            httpGET,
            httpPOST
        }
        public enum enumHTTPContentType
        {
            WWWFormURLEncoded,
            MuitipartFormData,
            NotSpecified
        }
        #endregion CONSTANS
        #region Basic Operation
        private static string getHTTPMethod(enumHTTPMethod m)
        {
            switch (m)
            {

                case enumHTTPMethod.httpGET:
                    return "GET";
                case enumHTTPMethod.httpPOST:
                    return "POST";
                default:
                    return "GET";
            }
        }
        private static string getHTTPContentType(enumHTTPContentType t)
        {
            switch (t)
            {
                case enumHTTPContentType.WWWFormURLEncoded:
                    return "application/x-www-form-urlencoded";
                case enumHTTPContentType.MuitipartFormData:
                    return "multipart/form-data";
                default:
                    return "";
            }
        }
        public static string RemoveControlCharacters(string s)
        {
            s = s.Replace(Environment.NewLine, "{CRLF}");
            foreach (int i in iSYS_ControlChar)
            {
                s = s.Replace(char.ConvertFromUtf32(i), "");
            }
            return s.Replace("{CRLF}", Environment.NewLine);
        }
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
        /// <summary>
        /// 串行化参数
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        private static string serializeParameters(List<APIParameter> paras)
        {
            StringBuilder p = new StringBuilder();
            foreach (APIParameter par in paras)
            {
                string v = par.Value;
                if (par.RequiredEncodingParse)
                {
                    v = UrlEncode(par.Value);
                }
                p.AppendFormat("{0}={1}&", par.Name, v);
            }
            if (p.Length > 0)
            {
                p = p.Remove(p.Length - 1, 1);//删除最后一个&
            }
            return p.ToString();
        }
        public static ReturnObject sendHttpRequest(string url, enumHTTPMethod httpRequestMethod, enumHTTPContentType httpContentType, string OAuthHeader, string postData)
        {
            //ReturnObject o = null;
            string method = getHTTPMethod(httpRequestMethod);
            string contenttype = getHTTPContentType(httpContentType);
            //记录
            try
            {
                //执行request
                HttpWebRequest req = HttpWebRequest.Create(url) as HttpWebRequest;
                req.Headers.Add("Authorization", "OAuth " + OAuthHeader);
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1468.0 Safari/537.36;iArthur ";
                req.Timeout = HTTP_TIMEOUT * 1000;
                req.ReadWriteTimeout = HTTP_TIMEOUT * 1000;
                req.Method = method;
                req.ContentType = contenttype;

                if (httpRequestMethod == enumHTTPMethod.httpPOST)
                {
                    req.ServicePoint.Expect100Continue = false;
                    byte[] bytePostData = Encoding.UTF8.GetBytes(postData);
                    req.ContentLength = bytePostData.Length;
                    Stream postStream = req.GetRequestStream();
                    postStream.Write(bytePostData, 0, bytePostData.Length);
                    postStream.Close();
                    bytePostData = null;
                }
                WebResponse resp = req.GetResponse();
                return new ReturnObject(1, RemoveControlCharacters(new StreamReader(resp.GetResponseStream(), System.Text.Encoding.UTF8).ReadToEnd()));
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.Timeout)
                {
                    //HTTP Error: Connection timed out
                    return new ReturnObject(-1, "HTTP request operation timed out");
                }
                else
                {
                    Stream resp = wex.Response.GetResponseStream();
                    if (resp != null)
                    {
                        //HTTP Error
                        return new ReturnObject(0, new StreamReader(resp).ReadToEnd());
                    }
                    else
                    {
                        //Server did not respond any data
                        return new ReturnObject(0, "Server did not respond any data");
                    }
                }
            }
            catch (Exception ex)
            {
                //An internal exception occured while accessing the API
                return new ReturnObject(-2, "An internal exception occured while accessing the API:" + url + ex.Message);
            }
        }
        #endregion Basic Operation
    }
}
