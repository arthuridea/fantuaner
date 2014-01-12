using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using fanfouModel;
using System.Data;
using System.Data.SQLite;

namespace FanfouBiz
{
    public class DataAccess
    {
        SQLiteDBHelper helper;
        public DataAccess()
        {
            string execpath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            helper = new SQLiteDBHelper(execpath + "fanfoubiz.db");
        }

        public void initUserdata(string id)
        {
            string sql = string.Format("insert or ignore into Friendship values('{0}','', '', '', '', '', null);", id);
            helper.ExecuteNonQuery(sql, null);
        }

        public string getUserFriendshipStatus(string id,string section)
        {
            string sql = string.Format("select {0} from Friendship where id=@id", section);
            SQLiteParameter[] pars={
                new SQLiteParameter("@id")
                                   };
            pars[0].Value=id;
            string rs = "";
            using (SQLiteDataReader rdr = helper.ExecuteReader(sql, pars))
            {
                try
                {
                    while (rdr.Read())
                    {
                        if (section.ToLower() == "lastupdatetime")
                        {
                            rs = rdr.GetValue(0).ToString();
                        }
                        else
                        {
                            rs = rdr.GetString(0);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    rdr.Close();
                }
            }
            return rs;
        }

        public void updateUser(string id,string section,string value)
        {
            string sql = "";
            if (section == "lastUpdateTime")
            {
                sql = "update Friendship set lastUpdateTime=datetime('now','localtime') where id=@id;";
                SQLiteParameter[] pars ={
                new SQLiteParameter("@id")
                                   };
                pars[0].Value = id;
                helper.ExecuteNonQuery(sql, pars);
            }
            else
            {
                sql = string.Format("update Friendship set {0}=@{0},lastUpdateTime=datetime('now','localtime') where id=@id;", section);
                SQLiteParameter[] pars ={
                new SQLiteParameter("@id"),
                new SQLiteParameter(string.Format("@{0}",section))
                                   };
                pars[0].Value = id;
                pars[1].Value = value;
                helper.ExecuteNonQuery(sql, pars);
            }            
        }

        public List<UserBase> getUserDetailInfoList()
        {
            List<UserBase> users = new List<UserBase>();
            string sql = "select id,screen_name,[timestamp] from userdetailinfo;";
            using (SQLiteDataReader sdr = helper.ExecuteReader(sql, null))
            {
                try
                {
                    while (sdr.Read())
                    {
                        string id = sdr.GetString(0);
                        string screen_name = sdr.GetString(1);
                        string ts = sdr.GetString(2);
                        DateTime timestamp = DateTime.Parse(ts);
                        UserBase u = new UserBase(id, screen_name, timestamp);
                        users.Add(u);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    sdr.Close();
                }
            }
            return users;
        }
        public void importUserDetailInfo(List<UserBase> list)
        {
            StringBuilder sqls=new StringBuilder();
            if(list.Count>0){
                //sqls.Append("begin;");
                foreach (UserBase u in list)
                {
                    string sql = string.Format("insert or replace into userdetailinfo values('{0}','{1}','{2}') ;", u.id, u.screen_name, u.timestamp);
                    sqls.Append(sql);
                }
                //sqls.Append(" commit;");
                helper.ExecuteNonQuery(sqls.ToString(), null);
            }
        }
        public int getBizInfoCount(string id)
        {
            string sql = "select count(1) from bizInfo where id=@id;";
            SQLiteParameter[] pars ={
                new SQLiteParameter("@id")
                                   };
            pars[0].Value = id;
            int count = 0;
            using (SQLiteDataReader rdr = helper.ExecuteReader(sql, pars))
            {
                try
                {
                    while (rdr.Read())
                    {
                        count = rdr.GetInt32(0);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    rdr.Close();
                }
            }
            return count;
        }
        public List<BizInfo> getBizInfoList(string id,int rows,int offset)
        {
            string con_row="";
            if (rows > 0) con_row = string.Format(" limit {0}{1} ", offset > 0 ? offset + "," : "", rows);
            List<BizInfo> list = new List<BizInfo>();
            string sql = "select message,CreateTime,[Status] from bizInfo where id=@id order by CreateTime desc " + con_row + ";";
            SQLiteParameter[] pars ={
                new SQLiteParameter("@id")
                                   };
            pars[0].Value = id;
            using (SQLiteDataReader sdr = helper.ExecuteReader(sql, pars))
            {
                try
                {
                    while (sdr.Read())
                    {
                        string message = sdr.GetString(0);
                        string ct = sdr.GetString(1);
                        DateTime createtime = DateTime.Parse(ct);
                        int status = int.Parse(sdr.GetString(2));
                        BizInfo biz = new BizInfo(message, status, status);
                        biz.CreateTime = createtime;
                        list.Add(biz);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    sdr.Close();
                }
            }
            return list;
        }
        public void appendBizInfoByUser(string id,List<BizInfo> list)
        {
            StringBuilder sqls = new StringBuilder();
            if (list.Count > 0)
            {
                //sqls.Append("delete from bizinfo where id=@id;");
                foreach (BizInfo b in list)
                {
                    string sql = string.Format("insert into bizinfo values('{0}','{1}','{2}','{3}');", id, b.Text, b.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"), b.Status.ToString());
                    sqls.Append(sql);
                }
                helper.ExecuteNonQuery(sqls.ToString(), null);
            }
        }
    }
}
