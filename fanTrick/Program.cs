using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fanfouModel;
using FanfouBiz;
using System.Net;
using System.IO;
using System.Web;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace fanTrick
{
    [Serializable]
    [DataContract]
    public class SCWResult
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string status { set; get; }
        [DataMember(Order = 1, IsRequired = true)]
        public List<SCWSWord> words { set; get; }
    }
    [Serializable]
    [DataContract]
    public class SCWSWord
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string word { set; get; }
        [DataMember(Order = 1, IsRequired = true)]
        public int off { set; get; }
        [DataMember(Order = 2, IsRequired = false)]
        public int len { set; get; }
        [DataMember(Order = 3, IsRequired = false)]
        public decimal idf { set; get; }
        [DataMember(Order = 4, IsRequired = false)]
        public string attr { set; get; }
    }

    [Serializable]
    [DataContract]
    public class SAESWord
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string word { set; get; }
        [DataMember(Order = 1, IsRequired = false)]
        public string word_tag { set; get; }
        [DataMember(Order = 2, IsRequired = true)]
        public string index { set; get; }
    }
    /// <summary>
    /// http://www.xunsearch.com/scws/demo/v48.php
    /// API:http://www.xunsearch.com/scws/api.php
    /// </summary>
    public class Program
    {
        //private static readonly string SINA_SEGMENT_URL = "http://5.tbip.sinaapp.com/api.php";
        private static readonly string SINA_SEGMENT_URL = "http://arthuridea.sinaapp.com/Api.php?m=segment";
        private static readonly string SCWS_URL = "http://www.xunsearch.com/scws/api.php";
        private static readonly int FETCH_STATUS_COUNT = 500;
        private static readonly int CLOUD_WORDS_UPBOUND = 150;
        private static int HTTP_TIMEOUT = 30;
        //static void Main(string[] args)
        public static string run(string userid)
        {
            StringBuilder wordstoanalyse = new StringBuilder();
            StringBuilder generatedHtml = new StringBuilder();
            List<StringBuilder> analysisqueue = new List<StringBuilder>();
            for (int i = 0; i < 30; i++)
            {
                analysisqueue.Add(new StringBuilder());
            }
            int peek = 0;

            List<FanfouStatus> status = getAnalyseStatus(userid, FETCH_STATUS_COUNT);
            foreach (var s in status)
            {
                if (string.IsNullOrEmpty(s.in_reply_to_status_id) && string.IsNullOrEmpty(s.in_reply_to_user_id) && string.IsNullOrEmpty(s.in_reply_to_screen_name))
                {
                    string filtered_text = StatusFiler(s.text);
                    if (analysisqueue[peek].Length >= 2500)
                    {
                        peek++;
                    }
                    StringBuilder b = analysisqueue[peek];
                    b.AppendFormat("{0} ", filtered_text);
                }
            }
            //新浪分词
            #region 新浪分词
            List<SAESWord> sinawords = new List<SAESWord>();
            foreach (var sb in analysisqueue)
            {
                if (sb.Length <= 0) break;
                List<APIParameter> sina_pars = new List<APIParameter>();
                sina_pars.Add(new APIParameter("msg", sb.ToString(), true, true));
                ReturnObject sinars = Query(SINA_SEGMENT_URL, serializeParameters(sina_pars));
                if (sinars.Code != 1)
                {
                    return sinars.Text + "\n" + sb.ToString();
                }
                sinawords.AddRange(JSON.parse<List<SAESWord>>(sinars.Text));
            }
            analysisqueue.Clear();
            if (sinawords.Count > 0)
            {
                #region 分词word_tag
                /*
                 * 
                 * 
0   POSTAG_ID_UNKNOW 未知
10  POSTAG_ID_A      形容词
20  POSTAG_ID_B      区别词
30  POSTAG_ID_C      连词
31  POSTAG_ID_C_N    体词连接
32  POSTAG_ID_C_Z    分句连接
40  POSTAG_ID_D      副词
41  POSTAG_ID_D_B    副词("不")
42  POSTAG_ID_D_M    副词("没")
50  POSTAG_ID_E      叹词
60  POSTAG_ID_F      方位词
61  POSTAG_ID_F_S    方位短语(处所词+方位词)
62  POSTAG_ID_F_N    方位短语(名词+方位词“地上”)
63  POSTAG_ID_F_V    方位短语(动词+方位词“取前”)
64  POSTAG_ID_F_Z    方位短语(动词+方位词“取前”)
70  POSTAG_ID_H      前接成分
71  POSTAG_ID_H_M    数词前缀(“数”---数十)
72  POSTAG_ID_H_T    时间词前缀(“公元”“明永乐”)
73  POSTAG_ID_H_NR   姓氏
74  POSTAG_ID_H_N    姓氏
80  POSTAG_ID_K      后接成分
81  POSTAG_ID_K_M    数词后缀(“来”--,十来个)
82  POSTAG_ID_K_T    时间词后缀(“初”“末”“时”)
83  POSTAG_ID_K_N    名词后缀(“们”)
84  POSTAG_ID_K_S    处所词后缀(“苑”“里”)
85  POSTAG_ID_K_Z    状态词后缀(“然”)
86  POSTAG_ID_K_NT   状态词后缀(“然”)
87  POSTAG_ID_K_NS   状态词后缀(“然”)
90  POSTAG_ID_M      数词
95  POSTAG_ID_N      名词
96  POSTAG_ID_N_RZ   人名(“毛泽东”)
97  POSTAG_ID_N_T    机构团体(“团”的声母为t，名词代码n和t并在一起。“公司”)
98  POSTAG_ID_N_TA   ....
99  POSTAG_ID_N_TZ   机构团体名("北大")
100 POSTAG_ID_N_Z    其他专名(“专”的声母的第1个字母为z，名词代码n和z并在一起。)
101 POSTAG_ID_NS     名处词
102 POSTAG_ID_NS_Z   地名(名处词专指：“中国”)
103 POSTAG_ID_N_M    n-m,数词开头的名词(三个学生)
104 POSTAG_ID_N_RB   n-rb,以区别词/代词开头的名词(该学校，该生)
107 POSTAG_ID_O      拟声词
108 POSTAG_ID_P      介词
110 POSTAG_ID_Q      量词
111 POSTAG_ID_Q_V    动量词(“趟”“遍”)
112 POSTAG_ID_Q_T    时间量词(“年”“月”“期”)
113 POSTAG_ID_Q_H    货币量词(“元”“美元”“英镑”)
120 POSTAG_ID_R      代词
121 POSTAG_ID_R_D    副词性代词(“怎么”)
122 POSTAG_ID_R_M    数词性代词(“多少”)
123 POSTAG_ID_R_N    名词性代词(“什么”“谁”)
124 POSTAG_ID_R_S    处所词性代词(“哪儿”)
125 POSTAG_ID_R_T    时间词性代词(“何时”)
126 POSTAG_ID_R_Z    谓词性代词(“怎么样”)
127 POSTAG_ID_R_B    区别词性代词(“某”“每”)
130 POSTAG_ID_S      处所词(取英语space的第1个字母。“东部”)
131 POSTAG_ID_S_Z    处所词(取英语space的第1个字母。“东部”)
132 POSTAG_ID_T      时间词(取英语time的第1个字母)
133 POSTAG_ID_T_Z    时间专指(“唐代”“西周”)
140 POSTAG_ID_U      助词
141 POSTAG_ID_U_N    定语助词(“的”)
142 POSTAG_ID_U_D    状语助词(“地”)
143 POSTAG_ID_U_C    补语助词(“得”)
144 POSTAG_ID_U_Z    谓词后助词(“了、着、过”)
145 POSTAG_ID_U_S    体词后助词(“等、等等”)
146 POSTAG_ID_U_SO   助词(“所”)
150 POSTAG_ID_W      标点符号
151 POSTAG_ID_W_D    顿号(“、”)
152 POSTAG_ID_W_SP   句号(“。”)
153 POSTAG_ID_W_S    分句尾标点(“，”“；”)
154 POSTAG_ID_W_L    搭配型标点左部
155 POSTAG_ID_W_R    搭配型标点右部(“》”“]”“）”)
156 POSTAG_ID_W_H    中缀型符号
160 POSTAG_ID_Y      语气词(取汉字“语”的声母。“吗”“吧”“啦”)
170 POSTAG_ID_V      及物动词(取英语动词verb的第一个字母。)
171 POSTAG_ID_V_O    不及物谓词(谓宾结构“剃头”)
172 POSTAG_ID_V_E    动补结构动词(“取出”“放到”)
173 POSTAG_ID_V_SH   动词“是”
174 POSTAG_ID_V_YO   动词“有”
175 POSTAG_ID_V_Q    趋向动词(“来”“去”“进来”)
176 POSTAG_ID_V_A    助动词(“应该”“能够”)
180 POSTAG_ID_Z      状态词(不及物动词,v-o、sp之外的不及物动词)
190 POSTAG_ID_X      语素字
191 POSTAG_ID_X_N    名词语素(“琥”)
192 POSTAG_ID_X_V    动词语素(“酹”)
193 POSTAG_ID_X_S    处所词语素(“中”“日”“美”)
194 POSTAG_ID_X_T    时间词语素(“唐”“宋”“元”)
195 POSTAG_ID_X_Z    状态词语素(“伟”“芳”)
196 POSTAG_ID_X_B    状态词语素(“伟”“芳”)
200 POSTAG_ID_SP     不及物谓词(主谓结构“腰酸”“头疼”)
201 POSTAG_ID_MQ     数量短语(“叁个”)
202 POSTAG_ID_RQ     代量短语(“这个”)
210 POSTAG_ID_AD     副形词(直接作状语的形容词)
211 POSTAG_ID_AN     名形词(具有名词功能的形容词)
212 POSTAG_ID_VD     副动词(直接作状语的动词)
213 POSTAG_ID_VN     名动词(指具有名词功能的动词)
230 POSTAG_ID_SPACE  空格
                 * 
                 * 
                 */
                #endregion
                List<SAESWord> targetwords = sinawords.FindAll(o => isTargetWord(o) && o.word.Length > 1);
                var resultwords = targetwords.GroupBy(o => o.word).Select(o => new { o.Key, Count = o.Count()}).ToList();
                resultwords = resultwords.OrderBy(o => o.Count).ToList();
                resultwords.Reverse();
                int checkIdx = 0;
                generatedHtml.Append("[");
                foreach (var o in resultwords)
                {
                    int limit = CLOUD_WORDS_UPBOUND > resultwords.Count ? CLOUD_WORDS_UPBOUND : resultwords.Count;
                    if (checkIdx++ > limit) break;
                    Console.WriteLine(string.Format("词语:{0}，个数{1}", o.Key, o.Count));
                    int fontsize = 100 * o.Count / limit;
                    generatedHtml.AppendFormat("{{text:'{0}',weight:{1},link:'http://fanfou.com/search?ct={0}'}},", o.Key, fontsize);
                }
                generatedHtml.Remove(generatedHtml.Length - 1, 1).Append("]");                
            }

            #endregion

            #region SCWS分词
            /*
            List<APIParameter> pars = new List<APIParameter>(); 
            pars.Add(new APIParameter("data", wordstoanalyse.ToString()));
            pars.Add(new APIParameter("respond", "json"));
            pars.Add(new APIParameter("ignore", "yes"));
            pars.Add(new APIParameter("multi","3"));
            string rs = Query(SCWS_URL, serializeParameters(pars));
            SCWResult rslist = JSON.parse<SCWResult>(rs);
            if (rslist != null && rslist.words!=null && rslist.words.Count>0)
            {
                //List<SCWSWord> words = rslist.words.OrderBy(o => o.idf).ToList();
                //words.Reverse();
                //foreach (var w in words)
                //{
                //    if (w.attr == "n")
                //    {
                //        Console.WriteLine(string.Format("word:{0},off:{1},idf:{2},len:{3},attr:{4}", w.word, w.off, w.idf, w.len, w.attr));
                //    }
                //}
                //int GroupNum=0;
                List<SCWSWord> words = rslist.words.FindAll(o => o.attr == "n" && o.word.Length > 1);
                var rtn=words.GroupBy(o=>o.word).Select(o=>new {o.Key,GroupNum=o.Count()}).ToList();
                rtn = rtn.OrderBy(o => o.GroupNum).ToList();
                rtn.Reverse();
                int n = 0;
                foreach (var o in rtn)
                {
                    if (n > 15) break;
                    Console.WriteLine(string.Format("词语:{0}，个数{1}", o.Key, o.GroupNum));
                    n++;
                }
            }
            */
            #endregion SCWS分词
            //Console.ReadKey();
            return generatedHtml.ToString();
        }

        private static bool isTargetWord(SAESWord w)
        {
            string[] target = { "95", "96", "97", "99", "100", "102" };
            return target.Contains(w.word_tag);
        }
        private static string StatusFiler(string str)
        {
            //移除  javascript code.
            str = Regex.Replace(str, @"<a[\d\D]*?>[\d\D]*?</a>", String.Empty);

            //移除html tag.
            str = Regex.Replace(str, @"<[^>]*>", String.Empty);
            Regex reg = new Regex(@"([转|RT|「])*@.+\s+(.)+?$");
            Regex reg_num_and_latin = new Regex(@"([0-9a-zA-Z])+");
            str = reg.Replace(str, string.Empty);
            str = reg_num_and_latin.Replace(str, string.Empty);
            str = str.Replace(Environment.NewLine, string.Empty);
            //非中文
            Regex regex = new Regex("([\u4e00-\u9fa5]+)");
            StringBuilder tf = new StringBuilder();
            for (Match match = regex.Match(str, 0); match.Success; match = match.NextMatch())
            {
                tf.Append(match.Groups[1].ToString());
            }
            str = tf.ToString();
            str = Regex.Replace(str, @"\s+", string.Empty);

            return str;
        }
        public static List<FanfouStatus> getStatus(string maxid,string userid)
        {
            //string maxIDStr = maxid == 0 ? string.Empty : maxid.ToString();
            List<FanfouStatus> s = FanfouBiz.FanfouBizController.getUserTimeline(userid, string.Empty, 60, maxid, "lite");
            s = s.OrderBy(o => o.rawid).ToList();
            s.Reverse();
            return s;
        }

        public static List<FanfouStatus> getAnalyseStatus(string userid, int count)
        {
            bool fetch = true;
            string maxid = string.Empty;
            List<FanfouStatus> status = new List<FanfouStatus>();
            while (fetch && count > 0)
            {
                List<FanfouStatus> fetchlist = getStatus(maxid, userid);
                if (fetchlist != null && fetchlist.Count > 0)
                {
                    maxid = fetchlist[fetchlist.Count - 1].id;
                    count -= fetchlist.Count;
                    status.AddRange(fetchlist);
                }
            }
            return status;
        }

        private static ReturnObject Query(string url,string postData)
        {
            try
            {
                //执行request
                HttpWebRequest req = HttpWebRequest.Create(url) as HttpWebRequest;
                //req.Headers.Add("Authorization", "OAuth " + OAuthHeader);
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1468.0 Safari/537.36;iArthur ";
                req.Timeout = HTTP_TIMEOUT * 1000;
                req.ReadWriteTimeout = HTTP_TIMEOUT * 1000;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ServicePoint.Expect100Continue = false;
                byte[] bytePostData = Encoding.UTF8.GetBytes(postData);
                req.ContentLength = bytePostData.Length;
                Stream postStream = req.GetRequestStream();
                postStream.Write(bytePostData, 0, bytePostData.Length);
                postStream.Close();
                bytePostData = null;
                WebResponse resp = req.GetResponse();
                return new ReturnObject(1, new StreamReader(resp.GetResponseStream(), System.Text.Encoding.UTF8).ReadToEnd());
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
                    v = FanfouHelper.UrlEncode(par.Value);
                }
                p.AppendFormat("{0}={1}&", par.Name, v);
            }
            if (p.Length > 0)
            {
                p = p.Remove(p.Length - 1, 1);//删除最后一个&
            }
            return p.ToString();
        }
    }
}
