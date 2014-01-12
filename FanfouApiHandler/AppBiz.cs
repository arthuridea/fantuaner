using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using FanfouBiz;
using fanfouModel;
using System.Web;
using System.Text.RegularExpressions;
//using NPOI.HSSF.UserModel;
//using NPOI.HPSF;
//using NPOI.POIFS.FileSystem;
//using NPOI.SS.UserModel;
//using NPOI.HSSF.Util;
/* To work eith EPPlus library */
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System.Drawing;
/* For I/O purpose */
using System.IO;
/* For Diagnostics */
using System.Diagnostics;
using OfficeOpenXml.Style;

namespace FanfouBiz
{
    /// <summary>
    /// 好友关系检查
    /// </summary>
    public sealed class AppBiz
    {
        public  List<BizInfo> bizinfolist = new List<BizInfo>();//消息列表
        public static int MAX_FOER_RESTRICTION = 800;
        public static bool runstat = true;
        public static string runstatDesc = "";
        public static bool webapp = false;

        #region 好友关系检查
        public  ReturnObject friends = new ReturnObject();
        public  List<UserBase> userinfolist = new List<UserBase>();
        public  List<string> flist = new List<string>();
        public  List<string> flist_o = new List<string>();
        public  List<string> unfo = new List<string>();
        public  List<string> newfo = new List<string>();
        public  List<string> unreg = new List<string>();
        public  List<string> del = new List<string>();
        public  List<string> reregister = new List<string>();
        public List<UserBase> newerUserinfo = new List<UserBase>();

        private static readonly string INI_SECTION = "Friendship";
        public string lastupdatetime = "";
        DataAccess da = new DataAccess();

        //excel object
        //static HSSFWorkbook hssfworkbook;

        /// <summary>
        /// 加载用户好友关系
        /// </summary>
        /// <param name="id"></param>
        private void loadUserFriendship(string id)
        {
            //ConsoleColorHelper.Output(ConsoleColorHelper.ConsoleOutputType.INFO, string.Format("[INFO]载入{0}被关注列表...", id), true);
            //System.Diagnostics.Debug.WriteLine(string.Format("==============loadUserFriendship {0} ===================", id));
            //加载好友
            friends = FanfouBizController.getFollowerList(id);
            if (friends == null || friends.Code != 1)
            {
                ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, "[ERROR]" + friends.Text, true);
                runstat = false;
                runstatDesc = friends.Text;
                return;
                //throw new NullReferenceException(friends.Text);
            }
            if (friends != null && !string.IsNullOrEmpty(friends.Text))
            {
                flist = JSON.parse<List<string>>(friends.Text);
            }            
            //读取限制
            string config_path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            INIFile ini = new INIFile(config_path + "config.ini");
            string maxStr = ini.IniReadValue("Service", "restriction");
            if (!string.IsNullOrEmpty(maxStr))
            {
                MAX_FOER_RESTRICTION = int.Parse(maxStr.Trim());
            }
            if (flist.Count > MAX_FOER_RESTRICTION)
            {
                ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, string.Format("[ERROR]hi~大红人！关注您的有{0}人哦~我最多支持{1}的好友数量哦~(´；ω；`) ", flist.Count, MAX_FOER_RESTRICTION), true);
                runstat = false;
                return;
            }
            
        }
        private List<BizInfo> CompareDiff(string id)
        {
            bool firstexec = false;
            //是不是第一次
            string lupdtime = da.getUserFriendshipStatus(id, "lastUpdateTime");
            if (string.IsNullOrEmpty(lupdtime)) firstexec = true;
            List<string> notifications = new List<string>();
            if (!runstat)
            {
                notifications.Add(runstatDesc);
                //return notifications;
                return bizinfolist;
            }
            string curNote = "";
            //ConsoleColorHelper.Output(ConsoleColorHelper.ConsoleOutputType.INFO, string.Format("[INFO]检查{0}被关注列表变动情况...", id), true);
            if (!runstat)
            {
                //return notifications;
                return bizinfolist;
            }
            //读取保存的好友
            //string config_path = HttpContext.Current.Server.MapPath("~");
            string config_path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            INIFile ini = new INIFile(config_path + "config.ini");
            //string savedfriends = ini.IniReadValue(INI_SECTION + "_" + id, "friendslist");
            //lastupdatetime = ini.IniReadValue(INI_SECTION + "_" + id, "lastUpdateTime");
            //从数据库读
            string savedfriends = da.getUserFriendshipStatus(id, "friendslist");
            lastupdatetime = da.getUserFriendshipStatus(id, "lastUpdateTime");

            /***************改成从数据库读***************
            string uil = ini.IniReadValue(INI_SECTION + "_" + id, "userdetailinfo");
            if (!string.IsNullOrEmpty(uil))
            {
                try
                {
                    userinfolist = JSON.parse<List<UserBase>>(uil);
                }
                catch (Exception ex)
                {
                    ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, "[ERROR]" + ex.Message, true);
                }
            }
            else
            {
                firstexec = true;
            }
            //清理数据
            uil = string.Empty;
            **********************************************/

            ///从数据库读
            List<UserBase> ublist = new List<UserBase>();
            try
            {
                ublist = da.getUserDetailInfoList();
            }
            catch (Exception ex)
            {
                ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, "[ERROR]读取用户信息库发生错误:" + ex.Message, true);
            }
            if (ublist.Count > 0)
            {
                userinfolist = ublist;
            }
            else
            {
                //firstexec = true;//TODO:此处应该判断每个用户的更新标识来确定是否是第一次
            }
            

            if (!string.IsNullOrEmpty(savedfriends))
            {
                flist_o = JSON.parse<List<string>>(savedfriends);
            }
            unfo = new List<string>();
            newfo = new List<string>();
            unreg = new List<string>();
            del = new List<string>();

            //比较每个当前fo列表中的用户ID是否在原列表中
            List<string> diff_flist_to_oflist = new List<string>();
            diff_flist_to_oflist = flist.Except(flist_o).ToList<string>();
            foreach (string foer in diff_flist_to_oflist)
            {
                //int idx=flist_o.FindIndex(o => o == foer);
                //if (idx < 0)//不在原列表中，新增foer
                //{
                    newfo.Add(foer);
                    //加入foer详细信息，以便获取名称等信息
                    try
                    {
                        ReturnObject ro = FanfouBizController.showRelationshipById(id, foer);
                        if (ro.Code == 1)//有效
                        {
                            Relationpack rp = JSON.parse<Relationpack>(ro.Text);
                            UserBase u = new UserBase(rp.relationship.target.id, rp.relationship.target.screen_name);
                            userinfolist.Add(u);
                            newerUserinfo.Add(u);
                            curNote = string.Format("{0}({1})开始关注你了.", u.screen_name, u.id);
                            bizinfolist.Add(new BizInfo(curNote,1,1));
                            if (!firstexec)
                            {
                                notifications.Add(curNote);
                                ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.TIP, curNote, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, "[ERROR]" + ex.Message, true);
                        runstat = false;
                        runstatDesc += "\n" + ex.Message;
                    }
                    
                //}
            }
            //Console.WriteLine(string.Format("==============比较新列表完毕 {0} ===================", id));
            //System.Diagnostics.Trace.WriteLine(string.Format("==============比较新列表完毕 {0} ===================", id));
            //比较每个原列表中的用户ID是否在当前列表中
            List<string> diff_oflist_to_flist = new List<string>();
            diff_oflist_to_flist = flist_o.Except(flist).ToList<string>();
            foreach (string ofo in diff_oflist_to_flist)
            {
                //int idx = flist.FindIndex(o => o == ofo);
                //if (idx < 0)//不在新列表中，为unfo或注销或删号用户
                //{
                ReturnObject ro = FanfouBizController.showRelationshipById(id, ofo);
                //判断是否删号
                if (ro.Code != 1)//未找到关系
                {
                    if (!string.IsNullOrEmpty(ro.Text) && ro.Text.Contains("没有找到target user"))
                    {
                        del.Add(ofo);
                        UserBase u = userinfolist.Find(o => o.id == ofo);
                        if (u != null)
                        {
                            curNote = string.Format("{0}({1})删号了.", u.screen_name, u.id);
                            bizinfolist.Add(new BizInfo(curNote,1,4));
                            notifications.Add(curNote);
                            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.TIP, curNote, true);
                        }
                        else
                        {
                            curNote = string.Format("{0}删号了.", ofo);
                            bizinfolist.Add(new BizInfo(curNote,1,4));
                            notifications.Add(curNote);
                            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.TIP, curNote, true);
                        }
                    }
                    //其余加入unfo
                    else
                    {
                        unfo.Add(ofo);
                        UserBase u = userinfolist.Find(o => o.id == ofo);
                        if (u != null)
                        {
                            curNote = string.Format("{0}({1})unfo了你.", u.screen_name, u.id);
                            bizinfolist.Add(new BizInfo(curNote,1,2));
                            notifications.Add(curNote);
                            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.TIP, curNote, true);
                        }
                        else
                        {
                            curNote = string.Format("{0}unfo了你.", ofo);
                            bizinfolist.Add(new BizInfo(curNote,1,2));
                            notifications.Add(curNote);
                            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.TIP, curNote, true);
                        }
                    }
                }
                //判断是否注销
                else
                {
                    try
                    {
                        Relationpack rp = JSON.parse<Relationpack>(ro.Text);
                        UserBase u = new UserBase(rp.relationship.target.id, rp.relationship.target.screen_name);
                        if (rp.relationship.target.following == "true")//关系仍为following但是不在fo列表里，判断为注销
                        {
                            unreg.Add(ofo);
                            curNote = string.Format("{0}({1})注销了 .", u.screen_name, u.id);
                            bizinfolist.Add(new BizInfo(curNote,1,3));
                            notifications.Add(curNote);
                            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.TIP, curNote, true);
                        }
                        else
                        {
                            unfo.Add(ofo);
                            curNote = string.Format("{0}({1})unfo了你.", u.screen_name, u.id);
                            ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.TIP, curNote, true);
                            bizinfolist.Add(new BizInfo(curNote,1,2));
                            notifications.Add(curNote);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, "[ERROR]" + ex.Message, true);
                        unfo.Add(ofo);
                        curNote = string.Format("{0}unfo了你.", ofo);
                        ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.TIP, curNote, true);
                        bizinfolist.Add(new BizInfo(curNote, 1, 2));
                        notifications.Add(curNote);
                        runstat = false;
                    }
                }

                //}
            }
            //return notifications;
            return bizinfolist;
            //Console.WriteLine(string.Format("==============比较旧列表完毕 {0} ===================", id));
            //System.Diagnostics.Trace.WriteLine(string.Format("==============比较旧列表完毕 {0} ===================", id));
        }

        private List<BizInfo> Upadte(string id)
        {
            List<string> note = new List<string>();
            List<BizInfo> bupd = new List<BizInfo>();
            if (!runstat) return null;
            //ConsoleColorHelper.Output(ConsoleColorHelper.ConsoleOutputType.INFO, string.Format("[INFO]保存{0}检查日志...", id), true);
            //System.Diagnostics.Trace.WriteLine(string.Format("==============Upadte {0} ===================", ""));
            string config_path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            INIFile ini = new INIFile(config_path + "config.ini");
            //更新当前fo列表
            //ini.IniWriteValue(INI_SECTION+"_"+id, "friendslist", friends.Text);
            da.updateUser(id, "friendslist", friends.Text);
            //更新unfo列表
            //string lastunfo = ini.IniReadValue(INI_SECTION+"_"+id, "unfolist");
            string lastunfo = da.getUserFriendshipStatus(id, "unfolist");
            List<string> lunfo = new List<string>();
            if (!string.IsNullOrEmpty(lastunfo))
            {
                lunfo = JSON.parse<List<string>>(lastunfo);
            }
            unfo.AddRange(lunfo);
            unfo.Except(newfo).ToList<string>();//去掉又fo回来的
            //ini.IniWriteValue(INI_SECTION+"_"+id, "unfolist", JSON.stringify(unfo));
            da.updateUser(id, "unfolist", JSON.stringify(unfo));
            //更新注销列表
            //string lastunreg = ini.IniReadValue(INI_SECTION+"_"+id, "unreglist");
            string lastunreg = da.getUserFriendshipStatus(id, "unreglist");
            List<string> lunreg = new List<string>();
            if (!string.IsNullOrEmpty(lastunreg))
            {
                lunreg = JSON.parse<List<string>>(lastunreg);
            }
            List<string> rereg = newfo.Intersect(lunreg).ToList<string>();//注销后又回来fo的
            unreg.AddRange(lunreg);
            unreg = unreg.Except(rereg).ToList<string>();//从合并后的注销列表中去掉回来的
            //ini.IniWriteValue(INI_SECTION+"_"+id, "unreglist", JSON.stringify(unreg));
            da.updateUser(id, "unreglist", JSON.stringify(unreg));
            //更新删号列表
            string lastdel = ini.IniReadValue(INI_SECTION+"_"+id, "dellist");
            List<string> ldel = new List<string>();
            if (!string.IsNullOrEmpty(lastdel))
            {
                ldel = JSON.parse<List<string>>(lastdel);
            }
            del.AddRange(ldel);
            //ini.IniWriteValue(INI_SECTION+"_"+id, "dellist", JSON.stringify(del));
            da.updateUser(id, "dellist", JSON.stringify(del));
            //更新新fo列表
            //string lastupdtime = ini.IniReadValue(INI_SECTION+"_"+id, "lastUpdateTime");
            string lastupdtime = da.getUserFriendshipStatus(id, "lastUpdateTime");
            if (!string.IsNullOrEmpty(lastupdtime))
            {//第一次使用读取新fo列表
                //string lastnewfo = ini.IniReadValue(INI_SECTION+"_"+id, "newfolist");
                string lastnewfo = da.getUserFriendshipStatus(id, "newfolist");
                List<string> lnewfo = new List<string>();
                if (!string.IsNullOrEmpty(lastnewfo))
                {
                    lnewfo = JSON.parse<List<string>>(lastnewfo);
                }
                newfo.AddRange(lnewfo);
                //ini.IniWriteValue(INI_SECTION+"_"+id, "newfolist", JSON.stringify(newfo));
                da.updateUser(id, "newfolist", JSON.stringify(newfo));
                lastnewfo = "";
            }
            else
            {//第一次初始化为空数组
                //ini.IniWriteValue(INI_SECTION+"_"+id, "newfolist", "[]");
                da.updateUser(id, "newfolist", "[]");
                bizinfolist = new List<BizInfo>();
            }
            //更新时间            
            //ini.IniWriteValue(INI_SECTION+"_"+id, "lastUpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            da.updateUser(id, "lastUpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //更新userbaselist列表
            /*
            string ul = ini.IniReadValue(INI_SECTION+"_"+id, "userdetailinfo");
            ul = string.IsNullOrEmpty(ul) ? "[]" : ul;
            if (userinfolist.Count > 0)
            {
                string newusers = JSON.stringify(userinfolist).Remove(0,1);
                ul = ul.Replace("]", (ul == "[]" ? "" : ","));
                ul += newusers;
                ini.IniWriteValue(INI_SECTION+"_"+id, "userdetailinfo", ul);
                ul = "";
                newusers = "";
            }*/

            //更新userdetailinfolist

            ////
            /*写入ini文件**************改数据库读*******************
            string newusers = JSON.stringify(userinfolist);
            ini.IniWriteValue(INI_SECTION + "_" + id, "userdetailinfo", newusers);
            newusers = "";
            *******************************************/
            ////            
            try
            {
                //da.importUserDetailInfo(userinfolist);
                da.importUserDetailInfo(newerUserinfo);//20140107 导入新fo的到userDetailInfo
            }
            catch (Exception ex)
            {
                ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, ex.Message, true);
            }
            

            //更新消息列表
            //1.把rereg加入infobiz和notification
            List<string> notification = new List<string>();
            foreach (var reid in rereg)
            {
                UserBase u=userinfolist.Find(o=>o.id==reid);
                string msg = string.Format("注销的{0}({1})回来了~~", u != null ? u.screen_name : reid, reid);
                bupd.Add(new BizInfo(msg, 1, 5));
                bizinfolist.Add(new BizInfo(msg, 1, 5));
                notification.Add(msg);
            }
            /**********写入ini*************************
            string bzl = ini.IniReadValue(INI_SECTION+"_"+id, "bizInfoList");
            bzl = string.IsNullOrEmpty(bzl) ? "[]" : bzl;
            if (bizinfolist.Count > 0)
            {
                string info = JSON.stringify(bizinfolist).Remove(0, 1);
                bzl = bzl.Replace("]", (bzl == "[]" ? "" : ","));
                bzl += info;
                ini.IniWriteValue(INI_SECTION+"_"+id, "bizInfoList", bzl);
                bzl = "";
                info = "";
            }
            ********************************************/
            //写入sqlite数据库
            da.appendBizInfoByUser(id, bizinfolist);
            
           // return notification;
            return bupd;
        }
        public void ShowResult()
        {
            try
            {
                Console.WriteLine(string.Format("[INFO]ShowResult {0}", ""));
                //System.Diagnostics.Trace.WriteLine(string.Format("==============ShowResult {0} ===================", ""));
                foreach (BizInfo bz in bizinfolist)
                {
                    string str = string.Format("{0} {1}", bz.CreateTime, bz.Text);
                    //Console.Write(str);
                    ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.NOTE, str, false);
                    //HttpContext.Current.Response.Write(str);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("[ERROR]" + ex.Message);
                ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, "[ERROR]" + ex.Message , true);
            }
        }

        #region 数据操作方法
        /// <summary>
        /// da封装方法 导入数据
        /// </summary>
        /// <param name="list"></param>
        public void importUserDetailInfo(List<UserBase> list)
        {
            da.importUserDetailInfo(list);
        }
        /// <summary>
        /// 分页时查询消息记录总数
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int queryBizInfoCount(string userid)
        {
            return da.getBizInfoCount(userid);
        }
        /// <summary>
        /// 分页查询消息记录，pageIndex从1开始
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<BizInfo> queryBizInfo(string userid, int pageSize, int pageIndex)
        {
            return da.getBizInfoList(userid, pageSize, pageSize * (pageIndex - 1));
        }
        public List<BizInfo> queryBizInfo(string userid, int count)
        {
            return da.getBizInfoList(userid, count, 0);
        }
        public void importBizInfo(string id, List<BizInfo> list)
        {
            da.appendBizInfoByUser(id, list);
        }
        public void initUserdata(string id)
        {
            da.initUserdata(id);
        }
        //static ICellStyle getColorRowStyleByStatus(int status)
        //{
        //    short colorIndex = HSSFColor.COLOR_NORMAL;
        //    switch (status)
        //    {
        //        case 1:
        //            colorIndex = HSSFColor.LIGHT_GREEN.index;
        //            break;
        //        case 2:
        //            colorIndex = HSSFColor.LIGHT_ORANGE.index;
        //            break;
        //        case 3:
        //            colorIndex = HSSFColor.ORANGE.index;
        //            break;
        //        case 4:
        //            colorIndex = HSSFColor.RED.index;
        //            break;
        //        case 5:
        //            colorIndex = HSSFColor.LIGHT_BLUE.index;
        //            break;
        //        default:
        //            break;
        //    }
        //    ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
        //    IDataFormat format = hssfworkbook.CreateDataFormat();

        //    cellStyle.FillPattern = FillPatternType.SOLID_FOREGROUND;
        //    cellStyle.FillForegroundColor = colorIndex;
        //    //cellStyle.FillBackgroundColor = colorIndex;
        //    return cellStyle;
        //}
        ///// <summary>
        ///// 导出excel
        ///// </summary>
        ///// <param name="ids"></param>
        //public static void exportBizInfoToExcel(List<string> ids)
        //{
        //    DataAccess da = new DataAccess();
        //    try
        //    {
        //        //read the template via FileStream, it is suggested to use FileAccess.Read to prevent file lock.
        //        //book1.xls is an Excel-2007-generated file, so some new unknown BIFF records are added. 
        //        //FileStream file = new FileStream(@"template.xls", FileMode.Open, FileAccess.Read);

        //        //hssfworkbook = new HSSFWorkbook(file);
        //        hssfworkbook = new HSSFWorkbook();

        //        //create a entry of DocumentSummaryInformation
        //        DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
        //        dsi.Company = "饭团儿";
        //        hssfworkbook.DocumentSummaryInformation = dsi;

        //        //create a entry of SummaryInformation
        //        SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
        //        si.Subject = "饭团儿数据导出";
        //        si.Author = "喵子ФωФ";
        //        hssfworkbook.SummaryInformation = si;

        //        //format entry
        //        ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
        //        IDataFormat format = hssfworkbook.CreateDataFormat();

        //        ICellStyle headerStyle = hssfworkbook.CreateCellStyle();
        //        IFont font = hssfworkbook.CreateFont();
        //        font.FontName = "微软雅黑";
        //        font.Boldweight = 700;
        //        font.Color = NPOI.HSSF.Util.HSSFColor.WHITE.index;
        //        headerStyle.SetFont(font);

        //        headerStyle.FillPattern = FillPatternType.SOLID_FOREGROUND;
        //        headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.BLACK.index;
        //        headerStyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.BLACK.index;

        //        headerStyle.Alignment = HorizontalAlignment.CENTER;
        //        headerStyle.VerticalAlignment = VerticalAlignment.CENTER;

        //        foreach (var id in ids)
        //        {
        //            ISheet sht = hssfworkbook.CreateSheet(id);
        //            IRow frow = sht.CreateRow(0);
        //            frow.HeightInPoints = 33;
        //            frow.RowStyle = headerStyle;

        //            ICell h0 = frow.CreateCell(0);
        //            h0.CellStyle = headerStyle;
        //            h0.SetCellValue("时间");
        //            ICell h1 = frow.CreateCell(1);
        //            h1.CellStyle = headerStyle;
        //            h1.SetCellValue("内容");
        //            ICell h2 = frow.CreateCell(2);
        //            h2.CellStyle = headerStyle;
        //            h2.SetCellValue("类型");
        //            ICell h3 = frow.CreateCell(3);
        //            h3.CellStyle = headerStyle;
        //            h3.SetCellValue("链接");

        //            sht.SetColumnWidth(0, 20 * 256);
        //            sht.SetColumnWidth(1, 60 * 256);
        //            sht.SetColumnWidth(2, 8 * 256);
        //            sht.SetColumnWidth(3, 60 * 256);

        //            List<BizInfo> bizlist = da.getBizInfoList(id, -1, 0);
        //            for (int i = 1, j = bizlist.Count; i <= j; i++)
        //            {
        //                var biz = bizlist[i - 1];
        //                IRow row = sht.CreateRow(i);
        //                //datetime
        //                ICell c0 = row.CreateCell(0);
        //                c0.SetCellValue(biz.CreateTime);
        //                cellStyle.DataFormat = format.GetFormat("yyyy-MM-dd HH:mm:ss");
        //                c0.CellStyle = cellStyle;
        //                //text
        //                ICell c1 = row.CreateCell(1);
        //                c1.SetCellValue(biz.Text);
        //                c1.CellStyle = getColorRowStyleByStatus(biz.Status);
        //                //status
        //                ICell c2 = row.CreateCell(2);
        //                c2.SetCellValue(BizInfo.getStatusType(biz.Status));
        //                //link
        //                ICell c3 = row.CreateCell(3);
        //                Match match = Regex.Match(biz.Text.Trim(), @"\((.+)\)", RegexOptions.None);
        //                if (match != null && match.Length > 1)
        //                {
        //                    string lnk = string.Format("http://fanfou.com/{0}", match.Groups[1].Value);
        //                    c3.SetCellValue(lnk);
        //                    IHyperlink hyper = new HSSFHyperlink(HyperlinkType.URL);
        //                    hyper.Address = lnk;
        //                    c3.Hyperlink = hyper;
        //                }
        //            }
        //            //freeze first line
        //            sht.CreateFreezePane(0, 1, 0, 1);
        //            sht.ForceFormulaRecalculation = true;
        //        }
        //        //Write the stream data of workbook to the root directory
        //        string exportPath = string.Format("{0}export/", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
        //        if (!Directory.Exists(exportPath))
        //        {
        //            Directory.CreateDirectory(exportPath);
        //        }
        //        string exportName = string.Format("饭团儿导出_{0}.xls", DateTime.Now.ToString("yyyyMMddHHmmss"));
        //        FileStream exportFile = new FileStream(exportPath + exportName, FileMode.Create);
        //        hssfworkbook.Write(exportFile);
        //        exportFile.Close();
        //        ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, string.Format("导出到文件：{0}", exportPath + exportName), true);
        //    }
        //    catch (Exception ex)
        //    {
        //        ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, ex.Message, true);
        //    }
        //}


        /// <summary>
        /// 将BizInfo导出到excel
        /// </summary>
        /// <param name="ids"></param>
        public static void exportBizInfoToExcelP(List<string> ids)
        {
            using (ExcelPackage p = new ExcelPackage())
            {
                try
                {
                    DataAccess da = new DataAccess();
                    foreach (var id in ids)
                    {
                        ExcelWorksheet sheet = p.Workbook.Worksheets.Add(id);
                        sheet.DefaultRowHeight = 20;
                        sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        sheet.Cells.Style.Font.Size = 10;
                        sheet.Cells.Style.Font.Name = "微软雅黑";
                        int colIndex = 1;
                        foreach (var col in "时间|内容|类型|链接".Split('|'))
                        {
                            sheet.Cells[1, colIndex++].Value = col;
                        }
                        sheet.Cells[1, 1, 1, 4].SetQuickStyle(Color.White, Color.Black, ExcelHorizontalAlignment.Center);
                        sheet.Row(1).Height = 33;
                        sheet.Row(1).Style.Font.Size = 12;
                        sheet.Row(1).Style.Font.Bold = true;
                        List<BizInfo> bizlist = da.getBizInfoList(id, -1, 0);
                        int rowid = 2;
                        foreach (var biz in bizlist)
                        {
                            sheet.Cells[rowid, 1].Value = biz.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            sheet.Cells[rowid, 1].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                            sheet.Cells[rowid, 2].Value = biz.Text;
                            sheet.Cells[rowid, 3].Value = BizInfo.getStatusType(biz.Status);

                            Match match = Regex.Match(biz.Text.Trim(), @"\((.+)\)", RegexOptions.None);
                            if (match != null && match.Length > 1)
                            {
                                string lnk = string.Format("http://fanfou.com/{0}", match.Groups[1].Value);
                                sheet.Cells[rowid, 4].Hyperlink = new Uri(lnk, UriKind.Absolute);
                            }
                            Color rowColor = Color.Transparent;
                            switch (biz.Status)
                            {
                                case 1:
                                    rowColor = Color.PaleGreen;break;
                                case 2:
                                    rowColor = Color.Salmon;break;
                                case 3:
                                    rowColor = Color.Red;break;
                                case 4:
                                    rowColor = Color.Crimson;break;
                                case 5:
                                    rowColor = Color.CornflowerBlue;break;
                                default:
                                    break;
                            }
                            ExcelRange currow = sheet.Cells[rowid, 1, rowid, 4];
                            currow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            currow.Style.Fill.BackgroundColor.SetColor(rowColor);
                            rowid++;
                        }                        
                        sheet.Column(1).AutoFit(10, 80);
                        sheet.Column(2).AutoFit(10, 80);
                        sheet.Column(3).AutoFit(8, 80);
                        sheet.Column(4).AutoFit(10, 80);
                    }
                    string exportPath = string.Format("{0}export/", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
                    if (!Directory.Exists(exportPath))
                    {
                        Directory.CreateDirectory(exportPath);
                    }
                    string exportName = string.Format("饭团儿导出_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    p.SaveAs(new FileInfo(exportPath + exportName));
                    ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.INFO, string.Format("导出到文件：{0}", exportName), true);
                }
                catch (Exception ex)
                {
                    ConsoleWin32Helper.Output(ConsoleWin32Helper.ConsoleOutputType.ERROR, ex.Message, true);
                }
                finally
                {
                    p.Dispose();
                }
            }
        }
        #endregion 数据操作方法
        private static string ConvertListToString(List<string> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in list)
            {
                sb.AppendFormat("\"{0}\",", s);
            }
            if (sb.Length > 0)
            {
                //删除最后一个,
                sb.Remove(sb.Length - 1, 1);
            }
            return string.Format("[{0}]", sb.ToString());
        }
        public static string getNewLineTag()
        {
            return webapp ? "<br />" : Environment.NewLine;//web返回<br /> 其他返回换行
        }

        public List<BizInfo> FriendshipCheck(string id)
        {
            List<string> notes = new List<string>();
            List<BizInfo> bs = new List<BizInfo>();
            //ConsoleColorHelper.Output(ConsoleColorHelper.ConsoleOutputType.INFO, string.Format("[INFO]检查{0}好友状态", id), true);
            //初始化
            friends = new ReturnObject();
            userinfolist = new List<UserBase>();
            flist = new List<string>();
            flist_o = new List<string>();
            unfo = new List<string>();
            newfo = new List<string>();
            unreg = new List<string>();
            del = new List<string>();
            newerUserinfo = new List<UserBase>();
            //reregister = new List<string>();

            runstat = true;

            loadUserFriendship(id);
            //notes = CompareDiff(id);
            bs = CompareDiff(id);
            //reregister=Upadte(id);
            bs.AddRange(Upadte(id));
            //notes.AddRange(reregister);
            //ShowResult();
            if (!string.IsNullOrEmpty(runstatDesc) && !string.IsNullOrEmpty(runstatDesc.Trim()))
            {
                //notes.Add(runstatDesc);
            }
            //ConsoleColorHelper.Output(ConsoleColorHelper.ConsoleOutputType.INFO, string.Format("[INFO]检查{0}完毕", id), true);
            //return notes;
            return bs;
        }
        #endregion 好友关系检查


    }
    public static class ExcelHelper
    {
        //扩展方法: SetQuickStyle，指定前景色/背景色/水平对齐
        public static void SetQuickStyle(this ExcelRange range,
            Color foreColor,
            Color bgColor = default(Color),
            ExcelHorizontalAlignment hAlign = ExcelHorizontalAlignment.Left)
        {
            range.Style.Font.Color.SetColor(foreColor);
            if (bgColor != default(Color))
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(bgColor);
            }
            range.Style.HorizontalAlignment = hAlign;
        }
    }
    #region 关键词消息
    //public sealed class 
    #endregion 关键词消息
}
