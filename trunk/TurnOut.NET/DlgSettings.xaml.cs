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
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Globalization;

namespace Daemonized.TurnOut
{
    /// <summary>
    /// Interaktionslogik für DlgSettings.xaml
    /// </summary>
    public sealed partial class DlgSettings : Window
    {
        private Microsoft.Win32.OpenFileDialog _dlg = new Microsoft.Win32.OpenFileDialog();
        public DlgSettings()
        {
            InitializeComponent();

            _dlg.DefaultExt = ".exe";
            _dlg.Filter = "Webbrowser executables (.exe)|*.exe";
            _dlg.CheckFileExists = true;
            _dlg.CustomPlaces.Add(
                new Microsoft.Win32.FileDialogCustomPlace(
                    Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles)));
            _dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            chkTopMost.IsChecked = Properties.Settings.Default.AlwaysOnTop;
            txtBrowser1.Text = Properties.Settings.Default.browser1;
            txtBrowser2.Text = Properties.Settings.Default.browser2;
            txtBrowser3.Text = Properties.Settings.Default.browser3;

            if (chkTopMost.IsChecked.Equals(true))
                this.Topmost = true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AlwaysOnTop = chkTopMost.IsChecked.Value;
            Properties.Settings.Default.browser1 = txtBrowser1.Text;
            Properties.Settings.Default.browser2 = txtBrowser2.Text;
            Properties.Settings.Default.browser3 = txtBrowser3.Text;
            
            Properties.Settings.Default.Save();

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = (System.Windows.Controls.Button)e.Source;
            _dlg.ShowDialog();
            if (_dlg.FileName.Length > 0)
                if (btn.Name.EndsWith("1", false, CultureInfo.InvariantCulture))
                    txtBrowser1.Text = _dlg.FileName;
                else if (btn.Name.EndsWith("2", false, CultureInfo.InvariantCulture))
                    txtBrowser2.Text = _dlg.FileName;
                else
                    txtBrowser3.Text = _dlg.FileName;
        }
    }
}
