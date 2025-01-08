using Microsoft.Win32;
using PizzaOven.Structures;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PizzaOven
{
    /// <summary>
    /// Interaction logic for VersionDownloadAuto.xaml
    /// </summary>
    public partial class VersionDownloadAuto : Window
    {
        public string AppID { get; set; }
        public string DepotID { get; set; }
        public PTVersion Version { get; set; }

        public string OutputDir { get; set; }
        public bool DownloadedDepot = false;

        public VersionDownloadAuto(string appID, string depotID, PTVersion version, string outputDir)
        {
            AppID = appID;
            DepotID = depotID;
            Version = version;
            OutputDir = outputDir;
            InitializeComponent();
            RememberPassword.IsChecked = Global.config.DownloadRememberPassword;

            UsernameInput.Text = GetSteamUsername();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text.Trim().Replace("\"", "^\"").Replace("&", "^&");

            if (username == "")
            {
                MessageBox.Show("You must enter your Steam username!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Close();

            DownloadedDepot = false;

            string depotdownloader = $"{Global.assemblyLocation}{Global.s}Dependencies{Global.s}DepotDownloader.exe";
            if (!File.Exists(depotdownloader))
            {
                MessageBox.Show($"{depotdownloader} was not found.\nPlease try redownloading Pizza Oven.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string logFile = $"{Global.assemblyLocation}{Global.s}depotdownloader.log";

            string rememberPasswordArg = (bool)RememberPassword.IsChecked ? "-remember-password " : "";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = true;
            startInfo.FileName = depotdownloader;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.WorkingDirectory = Global.assemblyLocation;
            startInfo.Arguments = $@"-app {AppID} -depot {DepotID} -manifest {Version.ManifestID} {rememberPasswordArg}-username ""{UsernameInput.Text}"" -dir {OutputDir}";
            using Process process = new Process();
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();

            try
            {
                Directory.Delete($"{OutputDir}.DepotDownloader{Global.s}", true);
            }
            catch { }

            if (process.ExitCode != 0)
            {
                MessageBox.Show($"Could not download the depot.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DownloadedDepot = true;
            return;
        }

        private void ManualMode_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Global.config.DownloadAutoMode = false;
            Global.UpdateConfig();
            DownloadedDepot = new VersionDownloadManual(AppID, DepotID, Version, OutputDir).ShowForDepot();
        }
        public bool ShowForDepot()
        {
            ShowDialog();
            return DownloadedDepot;
        }

        private string GetSteamUsername()
        {
            var key = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Valve\Steam");
            if (key != null)
                if (!String.IsNullOrEmpty(key.GetValue("AutoLoginUser") as string))
                    return key.GetValue("AutoLoginUser") as string;
            return "";
        }

        private void RememberPassword_Checked(object sender, RoutedEventArgs e)
        {
            Global.config.DownloadRememberPassword = (bool)RememberPassword.IsChecked;
            Global.UpdateConfig();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
        }
    }
}
