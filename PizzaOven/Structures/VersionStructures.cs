using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PizzaOven.Structures
{
    public class PTVersion
    {
        static List<PTVersion> ParsePTVersions()
        {
            string versionJSON = "";
            using (Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream("PizzaOven.Dependencies.Pizza_Tower_Versions.json"))
            using (StreamReader reader = new StreamReader(stream))
                versionJSON = reader.ReadToEnd();

            return JsonSerializer.Deserialize<List<PTVersion>>(versionJSON);
        }
        public static List<PTVersion> Versions = ParsePTVersions();

        public static string AppID = "2231450";
        public static string DepotID = "2231451";
        public static string VersionsPath => $"{Global.assemblyLocation}{Global.s}Versions{Global.s}";


        [JsonPropertyName("manifestID")]
        public string ManifestID { get; set; }

        [JsonIgnore]
        public DateTime SteamDBDate => DateTime.ParseExact(SteamDBDateString, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
        [JsonPropertyName("steamDBDate")]
        public string SteamDBDateString { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonIgnore]
        public string DisplayName => Title == "" ? Version : Version + " (" + Title + ")";

        [JsonPropertyName("title")]
        public string Title { get; set; }

        public string DownloadPath => $"{VersionsPath}{Version}{Global.s}";
        public bool HasInstallation() {
            foreach (Installation inst in Global.Installations) {
                if (inst.folder == DownloadPath) return true;
            }
            return false;
        }
    }
}
