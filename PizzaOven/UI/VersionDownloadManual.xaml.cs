using Microsoft.Win32;
using PizzaOven.Structures;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
namespace PizzaOven
{
    /// <summary>
    /// Interaction logic for VersionDownloadManual.xaml
    /// </summary>
    public partial class VersionDownloadManual : Window
    {
        public string AppID { get; set; }
        public string DepotID { get; set; }
        public string ManifestID { get; set; }

        public string DepotPath { get; set; }
        public string OutputDir { get; set; }
        public bool DownloadedDepot = false;

        public VersionDownloadManual(string appID, string depotID, string manifestID, string outputDir)
        {
            AppID = appID;
            DepotID = depotID;
            ManifestID = manifestID;
            OutputDir = outputDir;

            DepotPath = GetDepotPath();
            InitializeComponent();
            DownloadCommand.Text = $"download_depot {AppID} {DepotID} {ManifestID}";
            if (DepotPath != "")
            {
                NoPathWarning.Visibility = Visibility.Collapsed;
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(DownloadCommand.Text);
        }

        private void OpenSteam_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("steam://open/console") { UseShellExecute = true });
        }

        private string GetDepotPath() {
            var key = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Valve\Steam");
            if (key != null)
                if (!String.IsNullOrEmpty(key.GetValue("SteamPath") as string))
                    return Path.GetFullPath($"{key.GetValue("SteamPath") as string}{Global.s}steamapps{Global.s}content{Global.s}app_{AppID}{Global.s}depot_{DepotID}\\");
            return null;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DownloadedDepot = false;
            DepotPath = null;
            Close();
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            Close();
            if (PerformFinalMove(DepotPath)) return;
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Title = "Select the path to the downloaded depot";
            if (dialog.ShowDialog() != true) {
                DownloadedDepot = false;
                MessageBox.Show("Did not select the depot path; version will not be added.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            };
            string folder = dialog.FolderName;
            if (PerformFinalMove(folder)) return;
            MessageBox.Show("You somehow selected a folder that doesn't exist???", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool PerformFinalMove(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.CreateDirectory(PTVersion.VersionsPath);
                try
                {
                    Directory.Move(folder, OutputDir);
                    DownloadedDepot = true;
                } catch (Exception ex)
                {
                    MessageBox.Show($"Could not move the folder to Pizza Oven's folder: {ex}\n" +
                        "Try using the Automatic mode or running Pizza Oven in administrator mode.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return true;
            }
            return false;
        }

        private void AutoMode_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Global.config.DownloadAutoMode = true;
            Global.UpdateConfig();
            if (VersionDownloadAuto.TryAutoDownload(AppID, DepotID, ManifestID, OutputDir, out DownloadedDepot)) return;
            DownloadedDepot = new VersionDownloadAuto(AppID, DepotID, ManifestID, OutputDir).ShowForDepot();
        }

        public bool ShowForDepot() {
            ShowDialog();
            return DownloadedDepot;
        }
    }
}
