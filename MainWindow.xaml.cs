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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;


namespace BiuBiuClick
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private DirectoryInfo ButtonsFolder;
        private List<String> buttonImages;
        private List<ListViewItem> items;
        static String DEFAULT_CONFIG_DIR = "app/config";
        //static String CLICK_APP_PATH = "app/click.exe";
        private KeyController controller;

        public MainWindow()
        {
            InitializeComponent();
            this.listbox_Menu.AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(MenuListBoxItem_MouseDown), true);
            this.button_list.AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(button_list_MouseDown), true);
            if (System.Windows.Forms.SystemInformation.MonitorCount < 2)
            {
                button_align_2.IsEnabled = false;
            }
            this.ButtonsFolder = new DirectoryInfo(DEFAULT_CONFIG_DIR);
            this.buttonImages = new List<String>();
            this.items = new List<ListViewItem>();
            this.controller = new KeyController();
            loadButtonConfig();
        }

        private void loadButtonConfig()
        {
            String selectedConfig = comboBox.SelectedItem == null ? null : comboBox.SelectedItem.ToString();

            int selectedIndex = -1;
            List<String> config = new List<String>();
            try
            {
                foreach (DirectoryInfo NextDir in this.ButtonsFolder.GetDirectories())
                {
                    config.Add(NextDir.Name);
                    if (NextDir.Name.Equals(selectedConfig))
                    {
                        selectedIndex = comboBox.SelectedIndex;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("加载按钮配置出现错误：" + e.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            comboBox.ItemsSource = config;
            if (config.Count > 0)
            {
                comboBox.SelectedIndex = selectedIndex > -1 ? selectedIndex : 0;
            }
        }

        private void loadButtons(String configName)
        {
            if (null != configName)
            {
                try
                {
                    this.buttonImages.Clear();
                    this.items.Clear();
                    this.button_list.ItemsSource = this.items;
                    this.button_list.Items.Refresh();
                    foreach (FileInfo NextFile in new DirectoryInfo(DEFAULT_CONFIG_DIR + "/" + configName).GetFiles())
                    {
                        if (NextFile.Extension.ToLower().Equals(".png"))
                        {
                            System.Drawing.Image button_image = System.Drawing.Image.FromFile(NextFile.FullName);
                            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(button_image);
                            IntPtr hBitmap = bitmap.GetHbitmap();
                            Image image = new Image();
                            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            image.Source = wpfBitmap;
                            ListViewItem item = new ListViewItem();
                            //item.Width = button_image.Width;
                            item.Height = button_image.Height;
                            item.Background = Brushes.LightGray;
                            item.Content = image;
                            this.items.Add(item);
                            this.buttonImages.Add(NextFile.FullName);
                        }
                    }
                    this.button_list.ItemsSource = this.items;
                    this.button_list.Items.Refresh();
                }
                catch (Exception e)
                {
                    MessageBox.Show("加载按钮图片出现错误：" + e.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void refresh_click(object sender, RoutedEventArgs e)
        {
            loadButtonConfig();
            loadButtons(comboBox.SelectedItem == null ? null : comboBox.SelectedItem.ToString());
        }

        private struct Config
        {
            internal string filePath;
            internal string className;
            internal string macthClassNameFromRight;
            internal string processName;
            internal Dictionary<string, string> keys;

            internal Config init()
            {
                keys = new Dictionary<string, string>();
                return this;
            }
        }

        private Config readConfig()
        {
            string iniPath = System.Environment.CurrentDirectory + "/" + DEFAULT_CONFIG_DIR + "/" + comboBox.SelectedItem.ToString() + "/config.ini";
           
            IniFile iniFile = new IniFile();
            iniFile.Load(iniPath);

            Config config = new Config().init();
            config.filePath = iniPath;
            config.className = iniFile["common"]["className"].Value;
            config.macthClassNameFromRight = iniFile["common"]["macthClassNameFromRight"].Value;

            config.processName = iniFile["common"]["processName"].Value;

            List<KeyValuePair<string, IniValue>> pairs = iniFile["keys"].ToList();
            foreach (KeyValuePair<string, IniValue> pair in pairs) 
            {
                config.keys.Add(pair.Key, pair.Value.ToString());
            }
            return config;
        }

        private void DoClick(String buttonImagePath)
        {
            try
            {
                string buttonImageName = System.IO.Path.GetFileNameWithoutExtension(buttonImagePath);
                string[] nameParts = buttonImageName.Split('-');
                if (nameParts.Length != 2)
                {
                    MessageBox.Show("invalid button image name: '" + buttonImageName + "' at " + buttonImagePath, "图片名称不符合规范", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Config config = readConfig();
                    string buttonKey = nameParts[1];
                    if (config.keys.Keys.Contains(buttonKey))
                    {
                        string key = config.keys[buttonKey];
                        this.controller.sendKeyToProcessAll(config.processName, key);
                    }
                    else {
                        MessageBox.Show("button  key: '" + buttonKey + "' not found in " + config.filePath, "按钮未在配置文件中定义", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                /*Process proc = Process.Start(System.Environment.CurrentDirectory + "/" + CLICK_APP_PATH, buttonImagePath);
                if (proc != null)
                {
                    proc.WaitForExit(5000);
                    if (!proc.HasExited)
                    {
                        // 如果外部程序没有结束运行则强行终止之。
                        proc.Kill();
                        //MessageBox.Show(String.Format("程序 {0} 运行时间太长被强行终止！", CLICK_APP_PATH), "执行点击按钮出现错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }                    
                }*/
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "执行点击按钮出现错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void align_button_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedItem != null)
            {
                try
                {
                    Config config = readConfig();
                    Button btn = (Button)sender;
                    WindowHelper.AlignWindow(config.className, (WindowHelper.MoveMode)Enum.Parse(typeof(WindowHelper.MoveMode), btn.CommandParameter.ToString()));
                }
                catch (Exception e1)
                {
                    MessageBox.Show("运行对齐窗口程序出现错误：" + e1.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadButtons(comboBox.SelectedItem == null ? null : comboBox.SelectedItem.ToString());
        }

        private void button_list_MouseDown(object sender, MouseButtonEventArgs e)
        {
            String buttonImage = this.buttonImages[button_list.SelectedIndex];
            var background = this.items[button_list.SelectedIndex].Background;
            this.items[button_list.SelectedIndex].Background = Brushes.LightBlue;
            button_list.Items.Refresh();
            DoClick(buttonImage);
            this.items[this.button_list.SelectedIndex].Background = background;
            button_list.Items.Refresh();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void button_WindowMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void button_WindowClose_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void MenuListBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string content = (listbox_Menu.SelectedValue as ListBoxItem).Content.ToString();
            switch (content)
            {
                case "退出":
                    Environment.Exit(Environment.ExitCode);
                    break;
                case "窗口信息工具":
                    {
                        WindowInfoTool window = new WindowInfoTool();
                        window.Show();
                        window.Activate();
                    }
                    break;
            }

        }

    }
}
