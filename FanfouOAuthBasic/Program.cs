using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using OAuth;
using fanfouModel;
using Common;

namespace OAuthBasicApp
{
    /*
     * Douban OAuth认证包括以下四步内容
     * 
     * 1. 获取Request Token，该步骤使用API Key和API Key Secret签名
     * 2. 用户确认授权
     * 3. 换取Access Token，该步骤使用API Key、API Key Secret、Request Token和Request Token Secret签名
     * 4. 访问受限资源，该步骤使用API Key、API Key Secret、Access Token和Access Token Secret签名
     * 
     */
    public class fanApp
    {
        //string apiKey = "0e0ae1d4edc50be8ddbc6c0bcc83c309";
        //string apiKeySecret = "8a77d03b3d8665bf77225a02eab0e93b";
        string apiKey = "";
        string apiKeySecret = "";
        //string requestToken = "";
        //string requestTokenSecret = "";
        string accessToken = "";
        string accessTokenSecret = "";
        //string verifier = "";

        Uri requestTokenUri = new Uri("http://fanfou.com/oauth/request_token");
        Uri accessTokenUri = new Uri("http://fanfou.com/oauth/access_token");
        string authorizationUri = "http://fanfou.com/oauth/authorize?oauth_callback=oob&oauth_token=";
        Uri miniblogUri = new Uri("http://api.fanfou.com/statuses/mentions.json");

       OAuthBase oAuth = new OAuthBase();

       public fanApp()
       {

       }
       public fanApp(string key, string secret)
       {
           apiKey = key;
           apiKeySecret = secret;
       }
        private Dictionary<string, string> parseResponse(string parameters)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(parameters)) {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                    if (!string.IsNullOrEmpty(s))
                        if (s.IndexOf('=') > -1) {
                            string[] temp = s.Split('=');
                            result.Add(temp[0], temp[1]);
                        }
                        else result.Add(s, string.Empty);
            }

            return result;
        }

        //1. 获取Request Token，该步骤使用API Key和API Key Secret签名
        public ReturnObject getRequestToken(out string requestToken,out string requestTokenSecret)
        {
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "[INFO]STEP:请求RequestToken", true);
            try
            {
                Uri uri = requestTokenUri;
                string nonce = oAuth.GenerateNonce();
                string timeStamp = oAuth.GenerateTimeStamp();
                string normalizeUrl, normalizedRequestParameters;

                // 签名
                string sig = oAuth.GenerateSignature(
                    uri,
                    apiKey,
                    apiKeySecret,
                    string.Empty,
                    string.Empty,
                    "GET",
                    timeStamp,
                    nonce,
                    OAuthBase.SignatureTypes.HMACSHA1,
                    out normalizeUrl,
                    out normalizedRequestParameters);
                sig = HttpUtility.UrlEncode(sig);

                //构造请求Request Token的url
                StringBuilder sb = new StringBuilder(uri.ToString());
                sb.AppendFormat("?oauth_consumer_key={0}&", apiKey);
                sb.AppendFormat("oauth_nonce={0}&", nonce);
                sb.AppendFormat("oauth_timestamp={0}&", timeStamp);
                sb.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
                sb.AppendFormat("oauth_version={0}&", "1.0");
                sb.AppendFormat("oauth_signature={0}", sig);

                //Console.WriteLine("请求Request Token的url: \n" + sb.ToString());
                //HttpContext.Current.Response.Write("<p>请求Request Token的url: \n" + sb.ToString()+"</p>");

                //请求Request Token
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sb.ToString());
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader stream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                string responseBody = stream.ReadToEnd();
                stream.Close();
                response.Close();

                //Console.WriteLine("请求Request Token的返回值: \n" + responseBody);
                //HttpContext.Current.Response.Write("<p>请求Request Token的返回值: \n" + responseBody + "</p>");

                //解析返回的Request Token和Request Token Secret
                Dictionary<string, string> responseValues = parseResponse(responseBody);
                //保存RequestToken
                requestToken = responseValues["oauth_token"];
                requestTokenSecret = responseValues["oauth_token_secret"];

                return new ReturnObject(1, responseBody);
                //HttpContext.Current.Session.Add("requestToken", requestToken);
                //HttpContext.Current.Session.Add("requestTokenSecret", requestTokenSecret);
                //HttpContext.Current.Response.Write("<p>===============RequestToken End=================</p>");
                //////Console.WriteLine("获取RequestToken成功\n");
                //////Console.WriteLine(string.Format("RequestToken:{0}\nRequestTokenSecret:{1}\n", requestToken, requestTokenSecret));
            }
            catch (WebException wex)
            {
                //HttpContext.Current.Response.Write("<p>" + ex.Message + "</p>");
                //HttpContext.Current.Server.ClearError();
                //ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, "[ERROR]" + ex.Message, true);
                requestToken = "";
                requestTokenSecret = "";
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
                //return ex.Message;
            }
            catch (Exception ex)
            {
                //An internal exception occured while accessing the API
                requestToken = "";
                requestTokenSecret = "";
                return new ReturnObject(-2, ex.Message);
            }
            
        }

        // 2. 用户确认授权
        public string authorization(string requestToken)
        {
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "[INFO]STEP:请求用户授权", true);
            //生成引导用户授权的url
            string url =  authorizationUri + requestToken;

            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "--------------------------------", true);
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "请将下面url粘贴到浏览器中，并同意授权", true);
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.TIP, "\n小贴示：单击左上角图标，选择编辑，标记。用鼠标选择url，然后按enter键复制。打开浏览器粘贴刚才的url并按提示操作.\n", true);
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.NOTE, url, true);
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "--------------------------------", true);
            //Console.WriteLine("点击同意后，按任意键继续...");
            //Console.ReadKey();
            return url;
        }

        // 3. 换取Access Token，该步骤使用API Key、API Key Secret、Request Token和Request Token Secret签名
        public ReturnObject getAccessToken(string requestToken,string requestTokenSecret,string verifyCode,out string accesstoken,out string accesssecret)
        {
            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, "[INFO]STEP:换取AccessToken", true);
            Uri uri = accessTokenUri;
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string normalizeUrl, normalizedRequestParameters;

            // 签名
            string sig = oAuth.GenerateSignature(
                uri,
                apiKey,
                apiKeySecret,
                requestToken,
                requestTokenSecret,
                "GET",
                timeStamp,
                nonce,
                OAuthBase.SignatureTypes.HMACSHA1,
                out normalizeUrl,
                out normalizedRequestParameters);
            sig = (new OAuthBase()).UrlEncode(sig);
            //sig = HttpUtility.UrlEncode(sig,System.Text.Encoding.UTF8);
            //sig=(new OAuthBase()).

            //构造请求Access Token的url
            StringBuilder sb = new StringBuilder(uri.ToString());
            sb.AppendFormat("?oauth_consumer_key={0}&", apiKey);
            sb.AppendFormat("oauth_nonce={0}&", nonce);
            sb.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            sb.AppendFormat("oauth_signature={0}&", sig);
            sb.AppendFormat("oauth_timestamp={0}&", timeStamp);
            sb.AppendFormat("oauth_token={0}&", requestToken);
            
            if (!string.IsNullOrEmpty(verifyCode))
            {
                sb.AppendFormat("oauth_verifier={0}&", verifyCode);
            }
            
            sb.AppendFormat("oauth_version={0}", "1.0");

            //Console.WriteLine("请求Access Token的url: \n" + sb.ToString());

            //请求Access Token
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sb.ToString());
            request.Timeout = 300000;            
            try
            {
                request.Accept = "*";
                request.ContentType = "plain/text";
                //request.Headers.Add("Authorization", "OAuth " + sb.ToString());//.Replace("&", ","));
                //request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1468.0 Safari/537.36";
                //request.ContentType = "application/x-www-form-urlencoded";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader stream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                string responseBody = stream.ReadToEnd();
                stream.Close();
                response.Close();

                //Console.WriteLine("请求Access Token的返回值: \n" + responseBody);

                //解析返回的Request Token和Request Token Secret
                Dictionary<string, string> responseValues = parseResponse(responseBody);
                accessToken = responseValues["oauth_token"];
                accessTokenSecret = responseValues["oauth_token_secret"];
                accesstoken = responseValues["oauth_token"];
                accesssecret = responseValues["oauth_token_secret"];
                //////Console.WriteLine("提示：成功获得OAuth授权，请保存返回的token以便访问api");
                //////Console.WriteLine(string.Format("AccessToken:{0}\nAccessTokenSecret:{1}\n", accessToken, accessTokenSecret));
                ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, string.Format("成功获得OAuth授权：\nAccessToken:{0}\nAccessTokenSecret:{1}\n", accessToken, accessTokenSecret), true);
                //保存AccessToken
                //!!!!web需要取消注释HttpContext.Current.Session["AccessToken"] = accesstoken;
                //!!!!web需要取消注释HttpContext.Current.Session["AccessTokenSecret"] = accesssecret;
                return new ReturnObject(1, responseBody);
            }
            catch (WebException wex)
            {
                //HttpContext.Current.Response.Write("<p>" + ex.Message + "</p>");
                //HttpContext.Current.Server.ClearError();
                //ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, "[ERROR]" + ex.Message, true);
                accesstoken = "";
                accesssecret = "";
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
                //return ex.Message;
            }
            catch (Exception ex)
            {
                //An internal exception occured while accessing the API
                accesstoken = "";
                accesssecret = "";
                return new ReturnObject(-2, ex.Message);
            }
        }


        // 4. 访问受限资源，该步骤使用API Key、API Key Secret、Access Token和Access Token Secret签名
        //public void sendMiniBlog()
        //{
        //    Uri uri = miniblogUri;
        //    string nonce = oAuth.GenerateNonce();
        //    string timeStamp = oAuth.GenerateTimeStamp();
        //    string normalizeUrl, normalizedRequestParameters;

        //    // 签名
        //    string sig = oAuth.GenerateSignature(
        //        uri,
        //        apiKey,
        //        apiKeySecret,
        //        accessToken,
        //        accessTokenSecret,
        //        "POST",
        //        timeStamp,
        //        nonce,
        //        OAuthBase.SignatureTypes.HMACSHA1,
        //        out normalizeUrl,
        //        out normalizedRequestParameters);
        //    sig = HttpUtility.UrlEncode(sig);

        //    //构造OAuth头部
        //    StringBuilder oauthHeader = new StringBuilder();
        //    oauthHeader.AppendFormat("OAuth realm=\"\", oauth_consumer_key={0}, ", apiKey);
        //    oauthHeader.AppendFormat("oauth_nonce={0}, ", nonce);
        //    oauthHeader.AppendFormat("oauth_timestamp={0}, ", timeStamp);
        //    oauthHeader.AppendFormat("oauth_signature_method={0}, ", "HMAC-SHA1");
        //    oauthHeader.AppendFormat("oauth_version={0}, ", "1.0");
        //    oauthHeader.AppendFormat("oauth_signature={0}, ", sig);
        //    oauthHeader.AppendFormat("oauth_token={0}", accessToken);

        //    //构造请求
        //    StringBuilder requestBody =  new StringBuilder("<?xml version='1.0' encoding='UTF-8'?>");
        //    requestBody.Append("<entry xmlns:ns0=\"http://www.w3.org/2005/Atom\" xmlns:db=\"http://www.douban.com/xmlns/\">");
        //    requestBody.Append("<content>C# OAuth认证成功</content>");
        //    requestBody.Append("</entry>");
        //    Encoding encoding = Encoding.GetEncoding("utf-8");
        //    byte[] data = encoding.GetBytes(requestBody.ToString());

        //    // Http Request的设置
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        //    request.Headers.Set("Authorization", oauthHeader.ToString());
        //    request.ContentType = "application/atom+xml";
        //    request.Method = "POST";
        //    request.ContentLength = data.Length;
        //    Stream requestStream = request.GetRequestStream();
        //    requestStream.Write(data, 0, data.Length);
        //    requestStream.Close();
        //    try
        //    {
        //        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //        StreamReader stream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
        //        string responseBody = stream.ReadToEnd();
        //        stream.Close();
        //        response.Close();

        //        Console.WriteLine("发送广播成功");
        //    }
        //    catch (WebException e)
        //    {
        //        StreamReader stream = new StreamReader(e.Response.GetResponseStream(), System.Text.Encoding.UTF8);
        //        string responseBody = stream.ReadToEnd();
        //        stream.Close();

        //        Console.WriteLine("发送广播失败，原因: " + responseBody);
        //    }
        //}
        /*
        static void Main(string[] args)
        {

            // Fix HttpWebRequest vs. lighttpd bug
            // More details in http://www.gnegg.ch/2006/09/lighttpd-net-httpwebrequest/
            
            System.Net.ServicePointManager.Expect100Continue = false;

            Program p = new Program();
            Console.WriteLine(" 1. 获取Request Token，该步骤使用API Key和API Key Secret签名");
            p.getRequestToken();
            Console.WriteLine();
            Console.WriteLine("2. 用户确认授权");
            p.authorization();
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine("3. 换取Access Token，该步骤使用API Key、API Key Secret、Request Token和Request Token Secret签名");
            p.getAccessToken();
            Console.WriteLine();
            Console.WriteLine("4. 访问受限资源，该步骤使用API Key、API Key Secret、Access Token和Access Token Secret签名");
            //p.sendMiniBlog();
        }
        */
    }
}
