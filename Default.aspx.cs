using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using fanTrick;
using fanfouModel;
using System.Xml;
using FanfouBiz;

namespace FanfouDisney
{
    public partial class cloud : System.Web.UI.Page
    {
        public string tag = "";
        //private static readonly string ConsumerKey = "0e0ae1d4edc50be8ddbc6c0bcc83c309";
        //private static readonly string ConsumerSecret = "8a77d03b3d8665bf77225a02eab0e93b";
        //private static readonly string ConsumerKey = "757c6dc30a1ac11321a2788b38c2c3af";
        //private static readonly string ConsumerSecret = "5249c68a77158806dba9bb492ea1afe3";
        private static readonly string ConsumerKey = "a8b9ae213b7242d93e0a350733fbb7d7";
        private static readonly string ConsumerSecret = "852f1b11ce82797ef617d7a3d7a282b9";

        private static string requestToken = "";
        private static string requestTokenSecret = "";

        private static string AccessToken = "";
        private static string AccessTokenSecret = "";
        FanfouOAuthHelper p;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["id"]))
            {
                Session["queryId"] = Request.QueryString["id"];
            }
            else
            {
                if (Session["queryId"] == null)
                {
                    return;
                }
            }
            if (!IsPostBack)
            {
                if (Session["accessToken"] != null && Session["accessTokenSecret"] != null)
                {
                    //已认证
                    //if (Session["currentUser"] == null)
                    //{
                    //    User currentUserProfile = VerifyUser();
                    //    Session["currentUser"] = currentUserProfile;
                    //}
                    //else if (Session["queryId"] == null)
                    //{
                    //    Session["queryId"] = ((User)Session["currentUser"]).id;
                    //}
                    string u = Session["queryId"].ToString();
                    if (!string.IsNullOrEmpty(u))
                    {
                        tag = Program.run(u);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(Request.QueryString["oauth_token"]))
                    {
                        init();
                    }
                    else
                    {
                        if (authorize(Request.QueryString["oauth_token"]))
                        {
                            if (Session["queryId"] != null)
                            {
                                string u = Session["queryId"].ToString();
                                if (!string.IsNullOrEmpty(u))
                                {
                                    tag = Program.run(u);
                                    Session.Remove("queryId");
                                }
                            }
                        }
                    }
                }
            }
        }


        private void init()
        {
            p = new FanfouOAuthHelper(ConsumerKey, ConsumerSecret);
            //1.获取未授权的Request Token 该步骤使用API Key和API Key Secret签名
            ReturnObject rqResponse = p.getRequestToken(out requestToken, out requestTokenSecret);
            if (rqResponse.Code != 1)
            {
                //解析错误字符串
                string xmlrs = rqResponse.Text;
                string notAuthorizeErrMsg = "";
                try
                {
                    xmlrs = xmlrs.Replace("&", "&amp;");
                    XmlDocument x = new XmlDocument();
                    x.LoadXml(xmlrs);
                    notAuthorizeErrMsg = x.SelectSingleNode("/hash/error").InnerText;
                }
                catch
                {
                    notAuthorizeErrMsg = xmlrs;
                }
                return;
            }
            Session["consumerKey"] = ConsumerKey;
            Session["consumerSecret"] = ConsumerSecret;
            Session["requestToken"] = requestToken;
            Session["requestTokenSecret"] = requestTokenSecret;
            string url = p.authorization(requestToken);
            //Page.ClientScript.RegisterStartupScript(typeof(Page), "_authorize", "window.open('" + url.Replace("oauth_callback=oob&",string.Empty) + "');", true);
            Response.Redirect(url.Replace("oauth_callback=oob&", string.Empty));            
        }
        bool authorize(string requestTokenStr)
        {
            p = new FanfouOAuthHelper(Session["consumerKey"].ToString(), Session["consumerSecret"].ToString());
            ReturnObject accResponse = p.getAccessToken(requestTokenStr, requestTokenSecret, string.Empty, out AccessToken, out AccessTokenSecret);
            //持久化
            if (!string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(AccessTokenSecret))
            {
                Session["consumerKey"] = ConsumerKey;
                Session["consumerSecret"] = ConsumerSecret;
                Session["accessToken"] = AccessToken;
                Session["accessTokenSecret"] = AccessTokenSecret;
                return true;
            }
            else //未获得授权
            {
                //解析错误字符串
                string xmlrs = accResponse.Text;

                string notAuthorizeErrMsg = "";
                try
                {
                    xmlrs = xmlrs.Replace("&", "&amp;");
                    XmlDocument x = new XmlDocument();
                    x.LoadXml(xmlrs);
                    notAuthorizeErrMsg = x.SelectSingleNode("/hash/error").InnerText;
                }
                catch
                {
                    notAuthorizeErrMsg = xmlrs;
                }
                return false;
            }
        }

        private User VerifyUser()
        {
            ReturnObject ro = FanfouBizController.VerifyCredentials();
            if (ro.Code == 1)//成功登录
            {
                string ustr = ro.Text;
                User u = JSON.parse<User>(ustr);
                return u;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(ro.Text);
                return null;
            }
        }

    }
}