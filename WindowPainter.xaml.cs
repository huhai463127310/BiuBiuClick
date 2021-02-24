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

namespace BiuBiuClick
{
    /// <summary>
    /// WindowPainter.xaml 的交互逻辑
    /// </summary>
    public partial class WindowPainter : Window
    {
        public delegate void StopCapture();
        public StopCapture stopCapture;
        private MouseHook mouseHook;
        
        public MouseHook.MouseMoveHandler MouseMoveEventHandler;
        public MouseHook.MouseDownHandler MouseDownEventHandler;
        public MouseHook.MouseUpHandler MouseUpEventHandler;
        public MouseHook.MouseDoubleClickHandler MouseDoubleClickHandler;

        public WindowPainter()
        {
            InitializeComponent();
            mouseHook = new MouseHook();
        }

        public void DrawWindowBorder(WindowInfo windowInfo)
        {
            setMouseHook();
            Rectangle rect = new Rectangle()
            {
                Margin = new Thickness(windowInfo.Bounds.Left, windowInfo.Bounds.Top, 0, 0),
                Width = windowInfo.Bounds.Width,
                Height = windowInfo.Bounds.Height,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Red,
                StrokeThickness = 1,
            };
            this.canvas.Children.Add(rect);            
        }

        public void CleanWindowBorder()
        {
            this.canvas.Children.Clear();
            unsetMouseHook();
        }

        private void setMouseHook()
        {
            mouseHook.SetHook();
            mouseHook.MouseDownEvent += MouseDownEventHandler;
            mouseHook.MouseUpEvent += MouseUpEventHandler;
            mouseHook.MouseMoveEvent += MouseMoveEventHandler;
            mouseHook.MouseDoubleClickEvent += MouseDoubleClickHandler;
        }

        private void unsetMouseHook()
        {
            mouseHook.UnHook();
        }

        private void CommandBinding_ExitCaptureMode_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_ExitCaptureMode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CleanWindowBorder();
            stopCapture();
        }

    }
}
