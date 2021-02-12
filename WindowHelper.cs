﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace BiuBiuClick
{
    class WindowHelper
    {
        /// <summary>
        /// 查找当前用户空间下所有符合条件的窗口。如果不指定条件，将仅查找可见窗口。
        /// </summary>
        /// <param name="match">过滤窗口的条件。如果设置为 null，将仅查找可见窗口。</param>
        /// <returns>找到的所有窗口信息。</returns>
        public static IReadOnlyList<WindowInfo> FindAll(Predicate<WindowInfo> match = null)
        {
            var windowList = new List<WindowInfo>();
            EnumWindows(OnWindowEnum, 0);
            return windowList.FindAll(match ?? DefaultPredicate);

            bool OnWindowEnum(IntPtr hWnd, int lparam)
            {
                // 仅查找顶层窗口。
                if (GetParent(hWnd) == IntPtr.Zero)
                {
                    // 获取窗口类名。
                    var lpString = new StringBuilder(512);
                    GetClassName(hWnd, lpString, lpString.Capacity);
                    var className = lpString.ToString();

                    // 获取窗口标题。
                    var lptrString = new StringBuilder(512);
                    GetWindowText(hWnd, lptrString, lptrString.Capacity);
                    var title = lptrString.ToString().Trim();

                    // 获取窗口可见性。
                    var isVisible = IsWindowVisible(hWnd);

                    // 获取窗口位置和尺寸。
                    LPRECT rect = default;
                    GetWindowRect(hWnd, ref rect);
                    var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

                    // 添加到已找到的窗口列表。
                    windowList.Add(new WindowInfo(hWnd, className, title, isVisible, bounds));
                }

                return true;
            }
        }

        /// <summary>
        /// 默认的查找窗口的过滤条件。可见 + 非最小化 + 包含窗口标题。
        /// </summary>
        private static readonly Predicate<WindowInfo> DefaultPredicate = x => x.IsVisible && !x.IsMinimized && x.Title.Length > 0;

        private delegate bool WndEnumProc(IntPtr hWnd, int lParam);

        [DllImport("user32")]
        private static extern bool EnumWindows(WndEnumProc lpEnumFunc, int lParam);

        [DllImport("user32")]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lptrString, int nMaxCount);

        [DllImport("user32")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32")]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref LPRECT rect);

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct LPRECT
        {
            public readonly int Left;
            public readonly int Top;
            public readonly int Right;
            public readonly int Bottom;
        }

        public enum MoveMode : uint
        {
            SingleScreen = 1, TwoScreen = 2,
        }

        /// <summary>
        /// 设置目标窗体大小，位置
        /// </summary>
        /// <param name="hWnd">目标句柄</param>
        /// <param name="x">目标窗体新位置X轴坐标</param>
        /// <param name="y">目标窗体新位置Y轴坐标</param>
        /// <param name="nWidth">目标窗体新宽度</param>
        /// <param name="nHeight">目标窗体新高度</param>
        /// <param name="BRePaint">是否刷新窗体</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);

        public static void AlignWindow(string className, MoveMode moveMode = MoveMode.SingleScreen)
        {
            Predicate<WindowInfo> match = x => x.IsVisible && !x.IsMinimized && x.Title.Length > 0 && className.Equals(x.ClassName);
            IReadOnlyList<WindowInfo> windows = WindowHelper.FindAll(match);
            // 窗口按X坐标排序，防止移动后乱序
            windows.OrderBy(w => w.Bounds.X);            
            if (windows.Count == 2)
            {
                switch (moveMode)
                {
                    case MoveMode.SingleScreen:
                        int x1 = 0, y1 = 0, y2 = 0;
                        int halfScreenWidth = SystemInformation.WorkingArea.Width/2, height = SystemInformation.WorkingArea.Height;
                        int x2 = halfScreenWidth;
                        MoveWindow(windows[0].Hwnd, x1, y1, halfScreenWidth, height, true);
                        MoveWindow(windows[1].Hwnd, x2, y2, halfScreenWidth, height, true);

                        break;
                    case MoveMode.TwoScreen:
                        if (SystemInformation.MonitorCount != 2)
                        {
                            throw new InvalidOperationException(String.Format("需要2台显示器，当前只有{0}台", SystemInformation.MonitorCount));
                        }
                        MoveWindow(windows[0].Hwnd, 0, 0, Screen.AllScreens[0].WorkingArea.Width, Screen.AllScreens[0].WorkingArea.Height, true);
                        MoveWindow(windows[1].Hwnd, Screen.AllScreens[0].WorkingArea.Width, 0, Screen.AllScreens[1].WorkingArea.Width, Screen.AllScreens[1].WorkingArea.Height, true);
                        break;
                }
            }
            else
            {
                throw new InvalidOperationException(String.Format("需要两个{0}窗口才能对齐，但当前只有{1}个", className, windows.Count));
            }

        }

        public static void Main(String[] args)
        {
            string className = "PotPlayer";
            WindowHelper.AlignWindow(className, MoveMode.TwoScreen);
        }
    }

    /// <summary>
    /// 获取 Win32 窗口的一些基本信息。
    /// </summary>
    public readonly struct WindowInfo
    {
        public WindowInfo(IntPtr hWnd, string className, string title, bool isVisible, Rectangle bounds) : this()
        {
            Hwnd = hWnd;
            ClassName = className;
            Title = title;
            IsVisible = isVisible;
            Bounds = bounds;
        }

        /// <summary>
        /// 获取窗口句柄。
        /// </summary>
        public IntPtr Hwnd { get; }

        /// <summary>
        /// 获取窗口类名。
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// 获取窗口标题。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 获取当前窗口是否可见。
        /// </summary>
        public bool IsVisible { get; }

        /// <summary>
        /// 获取窗口当前的位置和尺寸。
        /// </summary>
        public Rectangle Bounds { get; }

        /// <summary>
        /// 获取窗口当前是否是最小化的。
        /// </summary>
        public bool IsMinimized => Bounds.Left == -32000 && Bounds.Top == -32000;
    }
}

