using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;

namespace Common
{
    public sealed class ConsoleWin32EventArgs : EventArgs
    {

    }
    public static class ConsoleWin32Helper
    {
        //public delegate void callbackCmdHandler<TEventArgs>(TEventArgs e);
        //public event callbackCmdHandler<ConsoleWin32EventArgs> _command;
        public enum ConsoleOutputType
        {
            INFO,
            ERROR,
            INPUT,
            NOTE,
            TIP,
            WARNNING
        };
        public static void Output(ConsoleOutputType type, string message, bool newline)
        {
            switch (type)
            {

                case ConsoleOutputType.INPUT:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case ConsoleOutputType.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case ConsoleOutputType.WARNNING:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case ConsoleOutputType.TIP:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case ConsoleOutputType.NOTE:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case ConsoleOutputType.INFO:
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            if (newline)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
        }

        #region 句柄实用工具

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        private class __mc_IWin32Window : IWin32Window
        {
            public IntPtr Handle { get; private set; }
            public __mc_IWin32Window(IntPtr handle)
            {
                Handle = handle;
            }
        }

        /// <summary>自动寻找当前控制台句柄</summary>        
        public static IntPtr GetConsoleWindowHandle()
        {
            //线程睡眠，确保closebtn中能够正常FindWindow，否则有时会Find失败。。
            //Thread.Sleep(100);

            //lock (Console.Title)
            //{
            //    var newtitle = Guid.NewGuid().ToString();
            //    var orgtitle = Console.Title;
            //    Console.Title = newtitle;

            //    IntPtr handle = FindWindow(null, Console.Title);

            //    Console.Title = orgtitle;
            //    Console.WriteLine(handle + " " + FindWindow(null, Console.Title));
            //    return handle;
            //}
            return FindWindow(null, Console.Title);
        }

        /// <summary>禁用关闭按钮</summary>        
        public static void DisableCloseButton()
        {
            IntPtr closeMenu = GetSystemMenu(GetConsoleWindowHandle(), IntPtr.Zero);
            uint SC_CLOSE = 0xF060;
            RemoveMenu(closeMenu, SC_CLOSE, 0x0);
        }

        /// <summary>自动寻找当前控制台句柄,并封装成Winform对象</summary>        
        public static IWin32Window GetConsoleWindowHandleObj()
        {
            return new __mc_IWin32Window(GetConsoleWindowHandle());
        }

        static void HiddenConsoleWindow()
        {
            uint SW_HIDE = 0;
            ShowWindow(GetConsoleWindowHandle(), SW_HIDE);
        }

        static void ShowConsoleWindow()
        {
            uint SW_SHOW = 5;
            ShowWindow(GetConsoleWindowHandle(), SW_SHOW);
        }

        static bool _Visable = true;
        public static bool Visable
        {
            get { return _Visable; }
            set
            {
                _Visable = value;
                if (_Visable)
                    ShowConsoleWindow();
                else
                    HiddenConsoleWindow();
            }
        }

        #endregion
    }
    #region 系统托盘图标类
    /// <summary>
    /// 系统托盘图标
    /// </summary>
    public sealed class ConsoleNotifyIcon
    {
        NotifyIcon _icon;
        /// <summary>
        /// 托盘菜单
        /// </summary>
        ContextMenuStrip _menu;
        /// <summary>
        /// 默认显示时间
        /// </summary>
        int _displayTime = 10000;
        /// <summary>
        /// 系统托盘图标
        /// </summary>
        public NotifyIcon Icon
        {
            get { return _icon; }
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="title"></param>
        /// <param name="iconPath"></param>
        /// <param name="menuItems"></param>
        /// <param name="clickHandler"></param>
        public ConsoleNotifyIcon(string title, string iconPath, List<ToolStripMenuItem> menuItems, ToolStripItemClickedEventHandler clickHandler)
        {
            this._icon = new NotifyIcon();
            _icon.Icon = new Icon(iconPath);
            _icon.Text = title;
            _icon.Visible=false;
            this._menu = new ContextMenuStrip();
            foreach (var item in menuItems)
            {
                _menu.Items.Add(item);
            }
            if (clickHandler != null)
            {
                _menu.ItemClicked += clickHandler;
            }
            _icon.ContextMenuStrip = _menu;

        }
        #region 显示图标方法
        public void show(string tip)
        {
            Icon.Visible = true;
            Icon.ShowBalloonTip(_displayTime, Icon.Text, tip, ToolTipIcon.Info);
        }
        public void show(string tip,int period)
        {
            Icon.Visible = true;
            Icon.ShowBalloonTip(period, Icon.Text, tip, ToolTipIcon.Info);
        }
        public void show(string title, string tip, int period)
        {
            Icon.Visible = true;
            Icon.ShowBalloonTip(period, title, tip, ToolTipIcon.Info);
        }
        public void show(string tip, int period,ToolTipIcon icon)
        {
            Icon.Visible = true;
            Icon.ShowBalloonTip(period, Icon.Text, tip, icon);
        }
        public void show(string title, string tip, int period, ToolTipIcon icon)
        {
            Icon.Visible = true;
            Icon.ShowBalloonTip(period, title, tip, icon);
        }
        #endregion 显示图标方法
        /// <summary>
        /// 隐藏提示
        /// </summary>
        public void hide()
        {
            Icon.Visible = false;
        }
        /// <summary>
        /// 销毁托盘图标
        /// 请在程序最后调用保证托盘图标正常消失
        /// </summary>
        public void Dispose()
        {
            _menu.Dispose();
            Icon.Dispose();
        }
    }
    #endregion 系统托盘图标类
}