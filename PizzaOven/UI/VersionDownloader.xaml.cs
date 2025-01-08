using PizzaOven.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PizzaOven
{
    /// <summary>
    /// Interaction logic for VersionDownloader.xaml
    /// </summary>
    public partial class VersionDownloader : Window
    {
        public List<PTVersion> Versions { get; set; } = PTVersion.Versions;
        public Action<string> JumpToInstallation { get; set; }
        public Action<string, string, string> CreateInstallation { get; set; }

        public VersionDownloader()
        {
            InitializeComponent();
            VersionGrid.ItemsSource = Versions;
        }

        private void VersionGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DoSearch()
        {
            string searchString = SearchBar.Text.Trim().ToLower();
            if (searchString == "")
            {
                Versions = PTVersion.Versions;
                VersionGrid.ItemsSource = Versions;
                return;
            }
            Versions = PTVersion.Versions.Where(v => v.DisplayName.ToLower().Contains(searchString)).ToList();
            VersionGrid.ItemsSource = Versions;
        }

        private void SearchBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.Enter))
                DoSearch();
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            DoSearch();
        }

        private void Clear_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SearchBar.Clear();
            DoSearch();
        }

        private void DownloadVersion_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            PTVersion version = button.DataContext as PTVersion;
            if (version.HasInstallation() && Directory.Exists(version.DownloadPath))
            {
                if (JumpToInstallation is not null) JumpToInstallation(version.DownloadPath);
                Close();
                return;
            }
            string ptExe = $"{version.DownloadPath}PizzaTower.exe";
            if (!File.Exists(ptExe))
            {
                string username = "marcsiebes3"; // TODO: allow user input
                if (!DownloadDepot(PTVersion.AppID, PTVersion.DepotID, version.ManifestID, username, version.DownloadPath)) return;

                // Disable Steam support so that launching the game doesn't run the original version
                try
                {
                    File.Move($"{version.DownloadPath}steam_api64.dll", $"{version.DownloadPath}_steam_api64.dll");
                }
                catch { }
            }
            if (!version.HasInstallation() && CreateInstallation is not null) CreateInstallation(version.DownloadPath, ptExe, version.Version);
            if (JumpToInstallation is not null) JumpToInstallation(version.DownloadPath);
        }

        private bool DownloadDepot(string appID, string depotID, string manifestID, string username, string outputDir)
        {
            if (Global.config.DownloadAutoMode)
            {
                if (VersionDownloadAuto.TryAutoDownload(appID, depotID, manifestID, outputDir, out bool downloadedDepot))
                {
                    return downloadedDepot;
                }
                return new VersionDownloadAuto(appID, depotID, manifestID, outputDir).ShowForDepot();
            }
            else
                return new VersionDownloadManual(appID, depotID, manifestID, outputDir).ShowForDepot();
        }
    }
}
