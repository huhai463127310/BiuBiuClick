using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Diagnostics;

//#error version

namespace BiuBiuClick
{
    /// <summary>
    /// WindowInfoTool.xaml 的交互逻辑
    /// </summary>
    public partial class WindowInfoTool : Window
    {
        private MouseHook mouseHook;
        private bool captureState = false;
        private WindowPainter windowPainter;
        private WindowInfo windowInfo = WindowInfo.Empty();        

        public WindowInfoTool()
        {
            InitializeComponent();
            mouseHook = new MouseHook();
            windowPainter = new WindowPainter();
            windowPainter.stopCapture = ClickCapture;
            windowPainter.MouseMoveEventHandler = MouseMoveEventHandler;
            windowPainter.MouseDownEventHandler = MouseDownEventHandler;
            windowPainter.MouseUpEventHandler = MouseUpEventHandler;
            windowPainter.MouseDoubleClickHandler = MouseDownEventHandler;

            IReadOnlyList<WindowInfo> windows = WindowHelper.FindVisiable(true);
            foreach (WindowInfo windowInfo in windows)
            {
                Console.WriteLine(windowInfo.ClassName);
            }
        }

        private void setMouseHook()
        {
            captureState = true;
            mouseHook.SetHook();
            mouseHook.MouseDownEvent += MouseDownEventHandler;
            mouseHook.MouseUpEvent += MouseUpEventHandler;
            mouseHook.MouseMoveEvent += MouseMoveEventHandler;
            mouseHook.MouseDoubleClickEvent += MouseHook_MouseDoubleClickEvent;
        }
               
        private void unsetMouseHook()
        {
            captureState = false;
            mouseHook.UnHook();
        }

        private void showWindowPainter()
        {
            System.Drawing.Rectangle rect = Screen.GetWorkingArea(new System.Drawing.Point((int)this.Left, (int)this.Top));
            windowPainter.Left = rect.Left;
            windowPainter.Top = rect.Top;
            windowPainter.Show();
        }

        private void ClickCapture()
        {
            if (captureState)
            {
                button_Capture.Content = "开始捕获";
                unsetMouseHook();
                captureState = false;
                windowPainter.Hide();
            }
            else
            {
                button_Capture.Content = "停止捕获";
                setMouseHook();
                captureState = true;
                
                showWindowPainter();
            }
        }

        private void MouseMoveEventHandler(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            textBox_MouseLocation.Text = String.Format("X:{0} Y:{1}", e.X, e.Y);
            IReadOnlyList<WindowInfo> windows = WindowHelper.FindVisiable(true);
            int idx = 0;
            int selected = -1;
            double selectedDistance = -1;
            Process processes = Process.GetCurrentProcess();
            foreach (WindowInfo info in windows)
            {
                
                if (!processes.Id.Equals(info.ProcessId) && info.Bounds.Left <= e.X && info.Bounds.Right >= e.X && info.Bounds.Top <= e.Y && info.Bounds.Bottom >= e.Y)
                {
                    double distance = CalcDistance(info, e.X, e.Y);
                    if (selected == -1 || distance < selectedDistance)
                    {
                        if(selected > -1)
                        {
                            Console.WriteLine(String.Format("~~~~~~~~~~~~~~~ idx={0} old: {1} d={2}, new:{3} d={4}", idx, windows[selected].ClassName, selectedDistance, info.ClassName, distance));
                        }
                        
                        selected = idx;
                        selectedDistance = distance;
                    }
                }

                #region debug info
                if ("PotPlayer".Equals(info.ClassName)) 
                {
                    Console.WriteLine(String.Format("{0} {1}", idx, info.ClassName));
                    bool locationReady = false;
                    if (!processes.Id.Equals(info.ProcessId) && info.Bounds.Left <= e.X && info.Bounds.Right >= e.X && info.Bounds.Top <= e.Y && info.Bounds.Bottom >= e.Y)
                    {
                        locationReady = true;
                    }
                    if (processes.Id.Equals(info.ProcessId))
                    {
                        Console.WriteLine(String.Format("{0}: self process", info.ClassName));
                    }
                    if (info.Bounds.Left > e.X)
                    {
                        Console.WriteLine(String.Format("{0}: left {1} > x {2}", info.ClassName, info.Bounds.Left, e.X));
                    }
                    if (info.Bounds.Right < e.X)
                    {
                        Console.WriteLine(String.Format("{0}: right {1} < x {2}", info.ClassName, info.Bounds.Right, e.X));
                    }
                    if (info.Bounds.Top > e.Y)
                    {
                        Console.WriteLine(String.Format("{0}: top {1} > y {2}", info.ClassName, info.Bounds.Top, e.Y));
                    }
                    if (info.Bounds.Bottom < e.Y)
                    {
                        Console.WriteLine(String.Format("{0}: bottom {1} < y {2}", info.ClassName, info.Bounds.Bottom, e.Y));
                    }
                    Console.WriteLine(String.Format("================{0} {1} X:{2} {3} Y:{4} {5}", info.ClassName, locationReady, e.Location.X, (info.Bounds.Left, info.Bounds.Right), e.Y, (info.Bounds.Top, info.Bounds.Bottom)));
                }
                
                #endregion

                idx++;
            }
            //this.windowInfo = WindowHelper.GetWindowInfoByPos(e.X, e.Y, false);
            if (selected > -1)
            {
                this.windowInfo = windows[selected];
                Console.WriteLine(String.Format("________________________{0} {1} {2} {3} {4}", this.windowInfo.ClassName, this.windowInfo.Title, this.windowInfo.ProcessId, this.windowInfo.IsVisible, this.windowInfo.Bounds));
            }
            else 
            {
                Console.WriteLine(String.Format("******************** idx={0} selected={1}", idx, selected));
            }
            if (!this.windowInfo.IsEmpty)
            {
                textBox_WindowTitle.Text = windowInfo.Title;
                textBox_WindowHandle.Text = windowInfo.Hwnd.ToString("X");
                textBox_WindowLocation.Text = String.Format("X:{0},Y:{1}", windowInfo.Bounds.Left, windowInfo.Bounds.Top);
                textBox_WindowSize.Text = String.Format("宽:{0},高:{1}", windowInfo.Bounds.Width, windowInfo.Bounds.Height);
                textBox_WindowProcess.Text = windowInfo.ProcessId.ToString();
                windowPainter.DrawWindowBorder(windowInfo);
            }
            else {
                textBox_WindowTitle.Text = String.Empty;
                textBox_WindowHandle.Text = String.Empty;
                textBox_WindowLocation.Text = String.Empty;
                textBox_WindowSize.Text = String.Empty;
                textBox_WindowProcess.Text = String.Empty;
                windowPainter.CleanWindowBorder();
            }
        }

        private double CalcDistance(WindowInfo windowInfo, int x, int y)
        {
            return Math.Pow(windowInfo.Bounds.X - x, 2) + Math.Pow(windowInfo.Bounds.Top -  y, 2);
        }

        private void MouseDownEventHandler(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Console.WriteLine("press left mouse button");
            }
            if (e.Button == MouseButtons.Right)
            {
                Console.WriteLine("press right mouse button");
            }
        }

        private void MouseUpEventHandler(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                
            }
            if (e.Button == MouseButtons.Right)
            {
                Console.WriteLine("release right mouse button");
            }
        }

        private void MouseHook_MouseDoubleClickEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (captureState)
            {
                ClickCapture();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            unsetMouseHook();            
        }

        private void button_Capture_Click(object sender, RoutedEventArgs e)
        {
            ClickCapture();
        }

        private void CommandBinding_ToolCapClick_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_ToolCapClick_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ClickCapture();
        }
    }
}
