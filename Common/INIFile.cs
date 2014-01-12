using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Common
{
    /// <summary>
    /// INI文件读写类。
    /// Copyright (C) Maticsoft
    /// </summary>
    public class INIFile
    {
        /// <summary> 
        /// The maximum size of a section in an ini file. 
        /// </summary> 
        /// <remarks> 
        /// This property defines the maximum size of the buffers 
        /// used to retreive data from an ini file.  This value is 
        /// the maximum allowed by the win32 functions 
        /// GetPrivateProfileSectionNames() or 
        /// GetPrivateProfileString(). 
        /// </remarks> 
        public const int MaxSectionSize = 65535; // 32 KB 

        private static int MAX_BUFF_LENGH = 655350;
        //private static long BUFF_LENGH_CAPACITY = long.MaxValue-1;
        public string path;

        public INIFile(string INIPath)
        {
            path = INIPath;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringA",CharSet = CharSet.Ansi)]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);

        ///// <summary> 
        ///// The GetPrivateProfileSectionNames function retrieves the names of all sections in an initialization file.
        ///// </summary>
        ///// <param name="lpszReturnBuffer">Pointer to a buffer that receives the section names associated with the named file. The buffer is filled with one orelse more null-terminated strings; the last string is followed by a second null character.</param>
        ///// <param name="nSize">Specifies the size, in TCHARs, of the buffer pointed to by the lpszReturnBuffer parameter.</param>
        ///// <param name="lpFileName">Pointer to a null-terminated string that specifies the name of the initialization file. If this parameter is NULL, the function searches the Win.ini file. If this parameter does not contain a full path to the file, the system searches for the file in the Windows directory.</param>
        ///// <returns>The return value specifies the number of characters copied to the specified buffer, not including the terminating null character. If the buffer is not large enough to contain all the section names associated with the specified initialization file, the return value is equal to the length specified by nSize minus two.</returns>
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileSectionNamesA", ExactSpelling = true, SetLastError = true)]
        private static extern int GetPrivateProfileSectionNames(byte[] lpszReturnBuffer, int nSize, string lpFileName);

        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            //StringBuilder temp = new StringBuilder(MAX_BUFF_LENGH, BUFF_LENGH_CAPACITY);
            StringBuilder temp = new StringBuilder(INIFile.MaxSectionSize);
            long i = GetPrivateProfileString(Section, Key, "", temp, int.MaxValue-1, this.path);
            return temp.ToString();

            //Byte[] Buffer = new Byte[65535];
            //long bufLen = GetPrivateProfileString(Section, Key, "", Buffer, Buffer.GetUpperBound(0), path);
            ////必须设定0（系统默认的代码页）的编码方式，否则无法支持中文
            //string s = Encoding.ASCII.GetString(Buffer);
            ////s = s.Substring(0, bufLen);
            //return s.Trim();
        }
        public byte[] IniReadValues(string section, string key)
        {
            byte[] temp = new byte[MAX_BUFF_LENGH];
            long i = GetPrivateProfileString(section, key, "", temp, INIFile.MaxSectionSize, this.path);
            return temp;

        }

        public List<string> GetAllSectionNames()
        {
            try
            {
                byte[] buffer = new byte[MAX_BUFF_LENGH + 1];
                GetPrivateProfileSectionNames(buffer, MAX_BUFF_LENGH, this.path);
                string[] parts = Encoding.Default.GetString(buffer).Trim('\0').Split('\0');
                //return new ArrayList(int.Parse(parts));
                //return parts.
                return new List<string>(parts);
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// 删除ini文件下所有段落
        /// </summary>
        public void ClearAllSection()
        {
            //IniWriteValue(null, null, null);
            List<string> sections = GetAllSectionNames();
            foreach (string sec in sections)
            {
                ClearSection(sec);
            }
        }
        /// <summary>
        /// 删除ini文件下personal段落下的所有键
        /// </summary>
        /// <param name="Section"></param>
        public void ClearSection(string Section)
        {
            IniWriteValue(Section, null, null);
        }

    }
}
