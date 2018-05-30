using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Bless_Screen_Fix
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        #region Save and Load Path to Registry
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BlessTempLauncher");
            key.SetValue("BlessPath", BlessPath.Text);
            key.Close();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BlessTempLauncher");
            if (key == null) return;
            BlessPath.Text = key.GetValue("BlessPath").ToString();
        }
        #endregion

        #region Button Click Events
        private void BrowseBlessFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    BlessPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void LaunchBlessButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var blessHelper = new Services.BlessHelperService(BlessPath.Text);
                blessHelper.Launch();
            }
            catch(FileNotFoundException ex)
            {
                System.Windows.MessageBox.Show(ex.Message, ex.FileName);
                return;
            }
            catch (Exception exx)
            {
                throw exx;
            }
            
        }
        #endregion
    }
}
