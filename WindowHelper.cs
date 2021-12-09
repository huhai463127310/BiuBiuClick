using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

/// <summary>
/// 参考：https://walterlv.gitee.io/post/enumerate-all-windows.html
/// </summary>
namespace BiuBiuClick
{
    public class WindowHelper
    {
        /// <summary>
        /// 查找当前用户空间下所有符合条件的窗口。如果不指定条件，将仅查找可见且有标题的窗口。
        /// </summary>
        /// <param name="match">过滤窗口的条件。如果设置为 null，将仅查找可见窗口。</param>
        /// <returns>找到的所有窗口信息。</returns>
        public static IReadOnlyList<WindowInfo> FindAll(Predicate<WindowInfo> match = null, bool onlyTop = true)
        {
            var windowList = new List<WindowInfo>();
            EnumWindows(OnWindowEnum, 0);
            return windowList.FindAll(match ?? DefaultPredicate);

            bool OnWindowEnum(IntPtr hWnd, int lparam)
            {
                // 添加到已找到的窗口列表。
                windowList.Add(GetWindowInfo(hWnd, lparam, onlyTop));
                return true;
            }
        }

        /// <summary>
        /// 查找当前用户空间下所有可见的窗口。
        /// </summary>
        /// <param name="onlyTop"></param>
        /// <returns></returns>
        public static IReadOnlyList<WindowInfo> FindVisiable(bool onlyTop = true)
        {
            return FindAll(VisiblePredicate, onlyTop);
        }

        public static WindowInfo GetWindowInfo(IntPtr hWnd, int lparam, bool onlyTop=true)
        {
            if ((onlyTop && GetParent(hWnd) == IntPtr.Zero) || !onlyTop)
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
                var isVisible = IsWindowVisible(hWnd) && (GetWindowLong(hWnd, (int)GetWindowLongIndex.GWL_STYLE) != 0);

                // 获取窗口位置和尺寸。
                LPRECT rect = default;
                GetWindowRect(hWnd, ref rect);
                var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

                int processId;
                GetWindowThreadProcessId(hWnd, out processId);

                bool isFullyOccluded = IsFullyOccluded(hWnd);

                // 添加到已找到的窗口列表。
                return new WindowInfo(hWnd, className, title, isVisible, bounds, isFullyOccluded, processId);
            }

            return WindowInfo.Empty();
        }

        /// <summary>
        /// 判断窗体是否被遮挡
        /// </summary>
        /// <param name="hWnd">窗体句柄</param>
        /// <returns>返回窗体是否被完全遮挡</returns>
        public static bool IsFullyOccluded(IntPtr hWnd)
        {
            if (!IsWindowVisible(hWnd)) 
            {
                // 窗体不可见
                return false; 
            }
            
            IntPtr vDC = GetWindowDC(hWnd);
            try
            {
                Rectangle vRect = new Rectangle();
                GetClipBox(vDC, ref vRect);
                return vRect.Width - vRect.Left <= 0 && vRect.Height - vRect.Top <= 0;
                // 特别说明：Rectangle.Width对应API中RECT.Right、Rectangle.Height为RECT.Bottom
            }
            finally
            {
                ReleaseDC(hWnd, vDC);
            }
        }


        /// <summary>
        /// 默认的查找窗口的过滤条件。可见 + 非最小化 + 包含窗口标题。
        /// </summary>
        public static readonly Predicate<WindowInfo> DefaultPredicate = x => x.IsVisible && !x.IsMinimized && x.Title.Length > 0;

        /// <summary>
        /// 可见窗口的过滤条件。可见 + 非最小化。
        /// </summary>
        public static readonly Predicate<WindowInfo> VisiblePredicate = x => x.IsVisible && x.Bounds.Width > 0  && x.Bounds.Height > 0 && !x.IsMinimized && !x.IsFullyOccluded;

        private delegate bool WndEnumProc(IntPtr hWnd, int lParam);

        #region find window DLL

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

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

        ///
        /// 该函数返回与指定窗口有特定关系（如Z序或所有者）的窗口句柄。
        /// 函数原型：HWND GetWindow（HWND hWnd，UNIT nCmd）；
        ///
        /// 窗口句柄。要获得的窗口句柄是依据nCmd参数值相对于这个窗口的句柄。
        /// 说明指定窗口与要获得句柄的窗口之间的关系。该参数值参考GetWindowCmd枚举。
        /// 返回值：如果函数成功，返回值为窗口句柄；如果与指定窗口有特定关系的窗口不存在，则返回值为NULL。
        /// 若想获得更多错误信息，请调用GetLastError函数。
        /// 备注：在循环体中调用函数EnumChildWindow比调用GetWindow函数可靠。调用GetWindow函数实现该任务的应用程序可能会陷入死循环或退回一个已被销毁的窗口句柄。
        /// 速查：Windows NT：3.1以上版本；Windows：95以上版本；Windows CE：1.0以上版本；头文件：winuser.h；库文件：user32.lib。
        ///
        [DllImport("user32.dll", SetLastError = true)] 
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

        /// <summary>
        /// 返回包含了指定点的窗口的句柄。忽略屏蔽、隐藏以及透明窗口
        /// </summary>
        /// <param name="point">指定的鼠标坐标</param>
        /// <returns>鼠标坐标处的窗口句柄，如果没有，返回</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(Point point);

        /// <summary>
        /// 返回包含这个点的窗口句柄，即使窗口隐藏或者处于无效状态。（需要指定某个容器窗体，返回该容器窗体中包含点的窗口句柄）
        /// </summary>
        /// <param name="hWndParent"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr ChildWindowFromPoint(IntPtr hWndParent, Point point);

        /// <summary>
        /// 获取鼠标处的坐标
        /// </summary>
        /// <param name="lpPoint">随同指针在屏幕像素坐标中的位置载入的一个结构</param>
        /// <returns>非零表示成功，零表示失败</returns>
        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out Point lpPoint);

        /// <summary>
        /// 获得指定窗口的信息
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern long GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("gdi32.dll")]
        public static extern int GetClipBox(IntPtr hDC, ref Rectangle lpRect);
        #endregion

        /// <summary>
        /// GetWindowCmd指定结果窗口与源窗口的关系，它们建立在下述常数基础上：
        /// GW_CHILD：寻找源窗口的第一个子窗口
        /// GW_HWNDFIRST：为一个源子窗口寻找第一个兄弟（同级）窗口，或寻找第一个顶级窗口
        /// GW_HWNDLAST：为一个源子窗口寻找最后一个兄弟（同级）窗口，或寻找最后一个顶级窗口
        /// GW_HWNDNEXT：为源窗口寻找下一个兄弟窗口
        /// GW_HWNDPREV：为源窗口寻找前一个兄弟窗口
        /// GW_OWNER：寻找窗口的所有者
        /// </summary>
        enum GetWindowCmd : uint

        { 
            ///
            /// 返回的句柄标识了在Z序最高端的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在Z序最高端的最高端窗口；
            /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最高端的顶层窗口：
            /// 如果指定窗口是子窗口，则句柄标识了在Z序最高端的同属窗口。
            ///

            GW_HWNDFIRST = 0, 
            
            ///
            /// 返回的句柄标识了在z序最低端的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该柄标识了在z序最低端的最高端窗口：
            /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最低端的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在Z序最低端的同属窗口。
            ///

            GW_HWNDLAST = 1, 
            
            ///
            /// 返回的句柄标识了在Z序中指定窗口下的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口下的最高端窗口：
            /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口下的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在指定窗口下的同属窗口。
            ///

            GW_HWNDNEXT = 2, 
            
            ///
            /// 返回的句柄标识了在Z序中指定窗口上的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口上的最高端窗口；
            /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口上的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在指定窗口上的同属窗口。
            ///

            GW_HWNDPREV = 3, 
            
            ///
            /// 返回的句柄标识了指定窗口的所有者窗口（如果存在）。
            /// GW_OWNER与GW_CHILD不是相对的参数，没有父窗口的含义，如果想得到父窗口请使用GetParent()。
            /// 例如：例如有时对话框的控件的GW_OWNER，是不存在的。
            ///

            GW_OWNER = 4, 
            
            ///
            /// 如果指定窗口是父窗口，则获得的是在Tab序顶端的子窗口的句柄，否则为NULL。
            /// 函数仅检查指定父窗口的子窗口，不检查继承窗口。
            ///

            GW_CHILD = 5, 
            
            ///
            /// （WindowsNT 5.0）返回的句柄标识了属于指定窗口的处于使能状态弹出式窗口（检索使用第一个由GW_HWNDNEXT 查找到的满足前述条件的窗口）；
            /// 如果无使能窗口，则获得的句柄与指定窗口相同。
            ///

            GW_ENABLEDPOPUP = 6

        }

        enum GetWindowLongIndex : int 
        {
            /// <summary>
            /// Retrieves the extended window styles. 
            /// </summary>
            GWL_EXSTYLE = -20,
            /// <summary>
            /// Retrieves a handle to the application instance. 
            /// </summary>
            GWL_HINSTANCE = -6,
            /// <summary>
            /// Retrieves a handle to the parent window, if any. 
            /// </summary>
            GWL_HWNDPARENT = -8,
            /// <summary>
            /// Retrieves the identifier of the window.
            /// </summary>
            GWL_ID = -12,
            /// <summary>
            /// Retrieves the window styles. 
            /// </summary>
            GWL_STYLE = -16,
            /// <summary>
            /// Retrieves the user data associated with the window. This data is intended for use by the application that created the window. Its value is initially zero. 
            /// </summary>
            GWL_USERDATA = -21,
            /// <summary>
            /// Retrieves the address of the window procedure, or a handle representing the address of the window procedure. You must use the CallWindowProc function to call the window procedure. 
            /// </summary>
            GWL_WNDPROC = -4
        }

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

        /// <summary>
        /// 获取窗口所在进程
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        /// <summary>
        /// ShowWindow参数
        /// </summary>
        public enum SHOW_WINDOW_CMD {
            
            SW_SHOWNORMAL = 1,
            SW_RESTORE = 9
    };

        [DllImport("user32.dll")] 
        public static extern bool ShowWindow(IntPtr hWnd, SHOW_WINDOW_CMD nCmdShow);

        [DllImport("User32.dll")]
        public static extern int SetForegroundWindow(IntPtr point);

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
                        int halfScreenWidth = SystemInformation.WorkingArea.Width / 2, height = SystemInformation.WorkingArea.Height;
                        int x2 = halfScreenWidth;
                        MoveWindow(windows[0].Hwnd, x1, y1, halfScreenWidth, height, true);
                        MoveWindow(windows[1].Hwnd, x2, y2, halfScreenWidth, height, true);
                        SetForegroundWindow(windows[0].Hwnd);
                        SetForegroundWindow(windows[1].Hwnd);

                        break;
                    case MoveMode.TwoScreen:
                        if (SystemInformation.MonitorCount != 2)
                        {
                            throw new InvalidOperationException(String.Format("需要2台显示器，当前只有{0}台", SystemInformation.MonitorCount));
                        }
                        MoveWindow(windows[0].Hwnd, 0, 0, Screen.AllScreens[0].WorkingArea.Width, Screen.AllScreens[0].WorkingArea.Height, true);
                        MoveWindow(windows[1].Hwnd, Screen.AllScreens[0].WorkingArea.Width, 0, Screen.AllScreens[1].WorkingArea.Width, Screen.AllScreens[1].WorkingArea.Height, true);
                        SetForegroundWindow(windows[0].Hwnd);
                        SetForegroundWindow(windows[1].Hwnd);
                        break;
                }
            }
            else if (windows.Count == 1)
            {
                throw new InvalidOperationException(String.Format("需要两个{0}窗口才能对齐，但当前只有1个，请再开1个{1}播放器窗口", className, className));
            }
            else if (windows.Count == 0)
            {
                throw new InvalidOperationException(String.Format("当前没有{0}窗口，需要两个才能对齐, 请打开两个{1}播放器窗口", className, className));
            }
            else {
                throw new InvalidOperationException(String.Format("当前有{0}个{1}窗口，两个才能对齐，请关闭多余窗口", windows.Count, className));
            }
        }
        
        #region print screen DLL
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(
       string lpszDriver, // driver name驱动名
       string lpszDevice, // device name设备名
       string lpszOutput, // not used; should be NULL
       IntPtr lpInitData // optional printer data
       );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);    // handle to DC

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(
        IntPtr hdc, // handle to DC
        int nWidth, // width of bitmap, in pixels
        int nHeight // height of bitmap, in pixels
        );

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(
        IntPtr hdc, // handle to DC
        IntPtr hgdiobj // handle to object
        );

        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(IntPtr hdc);   // handle to DC


        [DllImport("gdi32.dll")]
        public static extern IntPtr DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern int GetDlgCtrlID(IntPtr hwndCtl);


        [DllImport("user32.dll")]
        public static extern bool PrintWindow(
            IntPtr hwnd, // Window to copy,Handle to the window that will be copied. 
            IntPtr hdcBlt, // HDC to print into,Handle to the device context. 
            UInt32 nFlags // Optional flags,Specifies the drawing options. It can be one of the following values. 
        );

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        
        #endregion

        /// <summary>
        /// 窗口截图
        /// </summary>
        /// <param name="hWnd">hWnd可以是窗口、控件等的handle</param>
        /// <returns></returns>
        public static Bitmap CaptureWindow(IntPtr hWnd)    
        {
            IntPtr hscrdc = GetWindowDC(hWnd);
            LPRECT rect = default;
            // 返回指定窗体的矩形尺寸
            GetWindowRect(hWnd, ref rect);
            // 返回指定设备环境句柄对应的位图区域句柄
            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, rect.Right - rect.Left, rect.Bottom - rect.Top);
            // 创建一个与指定设备兼容的内存设备上下文环境（DC）
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            //把位图选进内存DC 
            SelectObject(hmemdc, hbitmap);
            // 直接打印窗体到画布
            bool re = PrintWindow(hWnd, hmemdc, 0);
            Bitmap bmp = null;
            if (re)
            {
                bmp = Bitmap.FromHbitmap(hbitmap);
            }
            DeleteObject(hbitmap);
            //删除用过的对象
            DeleteDC(hmemdc);
            ReleaseDC(hWnd, hscrdc);
            return bmp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mHwnd">主窗口句柄</param>
        /// <param name="ID">控件的ID</param>
        /// <param name=""></param>
        /// <returns></returns>
        public static IntPtr GetChildHWnd(IntPtr mHwnd, int ID, List<IntPtr> childWnds)  
        {            
            IntPtr mIDHWnd = IntPtr.Zero, mChildHWnd = IntPtr.Zero;    //mIDHWnd返回的控件句柄，mChildHWnd是主窗口的子窗口句柄
            while (mHwnd != null && !mHwnd.Equals(IntPtr.Zero))
            {
                int id = 0;
                id = GetDlgCtrlID(mHwnd);
                if (id == ID)
                {
                    mIDHWnd = mHwnd;
                    break;
                }
                mChildHWnd = GetWindow(mHwnd, GetWindowCmd.GW_CHILD);
                if (mChildHWnd != null && !mChildHWnd.Equals(IntPtr.Zero))
                {
                    childWnds.Add(mChildHWnd);
                    GetChildHWnd(mChildHWnd, ID, childWnds);
                }
                //Console.WriteLine(String.Format("mHwnd:{0}", Convert.ToString(mHwnd.ToInt32(), 16)));
                mHwnd = GetWindow(mHwnd, GetWindowCmd.GW_HWNDNEXT);
            }
            if (!IntPtr.Zero.Equals(mIDHWnd)) {
                Console.WriteLine(String.Format("mIDHWnd:{0}", Convert.ToString(mIDHWnd.ToInt32(), 10)));
            }            
            return mIDHWnd;
        }

        public static WindowInfo GetWindowInfoByPos(int x, int y, bool onlyTop, bool excludeSelf=true)
        {
            Point p = new Point(x, y);
            IntPtr hwnd = WindowFromPoint(p);
            if (!IntPtr.Zero.Equals(hwnd))
            {
                WindowInfo windowInfo = GetWindowInfo(hwnd, 0, onlyTop);
                Process processes = Process.GetCurrentProcess();
                if (!excludeSelf ||  !processes.Id.Equals(windowInfo.ProcessId))
                {
                    return windowInfo;
                }
            }
            return WindowInfo.Empty();
        }

        public static void Main(String[] args)
        {
            string className = "PotPlayer";
            //className = "AfxWnd140su";
            //WindowHelper.AlignWindow(className, MoveMode.TwoScreen);
            IReadOnlyList<WindowInfo> windows = FindAll(x => x.IsVisible && !x.IsMinimized && x.Title.Length > 0 && className.Equals(x.ClassName), false);
            if (null != windows && windows.Count > 0)
            {
                IntPtr hwnd = windows[0].Hwnd;
                Bitmap image = CaptureWindow(hwnd);
                if (null != image)
                {
                    image.Save(@"./capture/" + className + ".bmp");
                }
                
                int controlID = 0x207aa;
                List<IntPtr>  childWnds = new List<IntPtr>();
                IntPtr chdwnd = GetChildHWnd(hwnd, controlID, childWnds);
                if (!IntPtr.Zero.Equals(chdwnd))
                {
                    image = CaptureWindow(chdwnd);
                    image.Save(@className + ".bmp");
                }
                else {
                    //throw new InvalidOperationException(String.Format("Contorl ID not found:{0}", controlID));
                    foreach (IntPtr chwnd in childWnds)
                    {
                        image = CaptureWindow(chwnd);
                        if (null != image) 
                        {
                            image.Save(@"./capture/" + chwnd + ".bmp");
                        }
                        
                    }
                }
            }
            
        }

       
    }

    /// <summary>
    /// 获取 Win32 窗口的一些基本信息。
    /// </summary>
    public readonly struct WindowInfo
    {        
        private static WindowInfo empty;
        private static bool isEmptyInited = false;
        private static readonly object locker = new object();
        public WindowInfo(IntPtr hWnd, string className, string title, bool isVisible, Rectangle bounds, bool isFullyOccluded) : this()
        {
            Hwnd = hWnd;
            ClassName = className;
            Title = title;
            IsVisible = isVisible;
            Bounds = bounds;
            IsFullyOccluded = isFullyOccluded;
        }

        public WindowInfo(IntPtr hWnd, string className, string title, bool isVisible, Rectangle bounds, bool isFullyOccluded, int processId) : this(hWnd, className, title, isVisible, bounds, isFullyOccluded)
        {
            ProcessId = processId;
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

        /// <summary>
        /// 进程ID
        /// </summary>
        public int ProcessId { get; }

        /// <summary>
        /// 是否被完全遮挡
        /// </summary>
        public bool IsFullyOccluded { get; }

        /// <summary>
        /// 是否是空对象
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.Equals(empty);
            }
        }

        public static WindowInfo Empty()
        {
            if (isEmptyInited)
            {
                lock (locker)
                {
                    empty = new WindowInfo();
                    isEmptyInited = true;
                }
            }
            
            return empty;
        }

       override public string ToString()
        {
            return ClassName + ":" + Title;
        }
    }
}

