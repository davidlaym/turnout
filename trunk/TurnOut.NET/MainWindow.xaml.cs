using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Effects;
using System.Threading;
using System.Windows.Threading;
using System.Globalization;

namespace Daemonized.TurnOut
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private bool _bIconHit = false;
        private bool _bCtrlKey = false;
        private Uri _url;
        private System.Windows.Forms.NotifyIcon _ni = new System.Windows.Forms.NotifyIcon();
        private readonly SingleInstance SingleInstance = new SingleInstance(new Guid("FDE9818F-8F87-4A9D-9D99-04B905EB0B70"));
        
        public Uri TargetUrl
        {
            get { return _url; }
            set
            {
                _url = value;

                foreach (KeyValuePair<string, Browser> browser in _browsers)
                    browser.Value.Arguments = _url.ToString();

                if (_url.ToString().Length > 93)
                    txtUrl.Text = string.Format(CultureInfo.InvariantCulture, "{0} (...)", _url.ToString().Substring(0, 89));
                else
                    txtUrl.Text = _url.ToString();
            }
        }

        private static Dictionary<string, Browser> _browsers = new Dictionary<string, Browser>();

        public MainWindow()
        {
            InitializeComponent();

            ApplySettings();

            _ni.Icon = Properties.Resources.app;
            _ni.Visible = false;
            //Show on Doubleclick
            _ni.DoubleClick +=
                delegate(object sender, EventArgs args)
                {
                    BringToFront();
                };
            //Close on rightclick
            _ni.MouseClick +=
                delegate(object sender, System.Windows.Forms.MouseEventArgs args)
                {
                    BringToFront();
                };

            _browsers.Add("001",
                new Browser(Properties.Settings.Default.browser1.Replace("%programfiles%",ProgramFilesx86()),
                    Properties.Resources.firefox));
            _browsers.Add("002",
                new Browser(Properties.Settings.Default.browser2.Replace("%programfiles%", ProgramFilesx86()),
                    Properties.Resources.chrome));
            _browsers.Add("003",
                new Browser(Properties.Settings.Default.browser3.Replace("%programfiles%", ProgramFilesx86()),
                    Properties.Resources.IE));
        }

        private void BringToFront()
        {
            if (this.IsVisible || this.IsFocused)
            {
                this.WindowState = WindowState.Minimized;
            }
            else
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            }
        }

        public bool IsSingle()
        {
            if (SingleInstance.IsFirstInstance)
            {
                SingleInstance.ArgumentsReceived += SingleInstanceParameter;
                SingleInstance.ListenForArgumentsFromSuccessiveInstances();
                _ni.Visible = true;
                return true;
            }
            else
            {
                SingleInstance.PassArgumentsToFirstInstance(_url.ToString());

                return false;
            }
        }

        private void SingleInstanceParameter(object sender, Daemonized.TurnOut.GenericEventArgs<string> e)
        {
            // Inform app of new arguments
            if (Dispatcher.Thread != Thread.CurrentThread)
                Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(delegate() {
                        this.TargetUrl = new Uri(e.Data.ToString());
                        this.Show();
                        this.WindowState = System.Windows.WindowState.Normal;
                        this.Focus();
                    }));
        }

        private void ApplySettings()
        {
            //Apply default settings
            this.Topmost = Properties.Settings.Default.AlwaysOnTop;

            if (_browsers.Count == 3)
            {
                _browsers["001"].Path = new FileInfo(Properties.Settings.Default.browser1);
                _browsers["002"].Path = new FileInfo(Properties.Settings.Default.browser2);
                _browsers["003"].Path = new FileInfo(Properties.Settings.Default.browser2);
            }
        }

        static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void IconsMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            StartUri(img.Tag.ToString());
        }

        private bool StartUri(string Browser)
        {
            try
            {
                Process.Start(_browsers[Browser].Path.FullName, string.Format("\"{0}\"", _browsers[Browser].Arguments));
                this.WindowState = System.Windows.WindowState.Minimized; 
                
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                this.WindowState = System.Windows.WindowState.Normal;
                return false;
            }
        }

        private void grid_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(string)) || sender == e.Source)
                e.Effects = DragDropEffects.None;
        }

        private void grid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
            {
                try
                {
                    this.TargetUrl = new Uri(e.Data.GetData(typeof(string)) as string);
                }
                catch (UriFormatException ex)
                {
                    txtUrl.Text = "Not a valid url!";
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_bIconHit)
                this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void imgFirefox_MouseEnter(object sender, MouseEventArgs e)
        {
            _bIconHit = true;
        }

        private void imgFirefox_MouseLeave(object sender, MouseEventArgs e)
        {
            _bIconHit = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem item = (System.Windows.Controls.MenuItem)sender;
            switch (item.Name)
            {
                case "exit":
                    ExitApp();
                    break;
                case "minimize":
                    this.WindowState = System.Windows.WindowState.Minimized;
                    break;
                case "settings":
                    DlgSettings dlg = new DlgSettings();
                    if (dlg.ShowDialog().Equals(true))
                        ApplySettings();

                    break;
                default:
                    break;
            }
        }
        
        private void ExitApp()
        {
            _ni.Visible = false;
            Environment.ExitCode = 0;
            this.Close();
        }

        private void imgSettings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DlgSettings dlg = new DlgSettings();
            if (dlg.ShowDialog().Equals(true))
                ApplySettings();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                _bCtrlKey = false;

            switch(e.Key)
            {
                case Key.S:
                    imgSettings_MouseLeftButtonUp(null, null);
                    break;
                case Key.M:
                case Key.Escape:
                    if (e.Key == Key.M && _bCtrlKey || e.Key == Key.Escape)
                        this.WindowState = System.Windows.WindowState.Minimized;
                    break;
                case Key.Q:
                    if (_bCtrlKey)
                        ExitApp();
                    break;
                default:
                    break;
            }            
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                _bCtrlKey = true;
        }
    }
}
