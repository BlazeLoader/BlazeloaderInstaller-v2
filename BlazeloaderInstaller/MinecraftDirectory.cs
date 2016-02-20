using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlazeloaderInstaller {
    public class MinecraftDirectory {
        private string minecraftDir;
        private string versions;
        private string libraries;

        public bool showUnsupported;

        public int DetectedForge {
            get {
                return forgeVersions.Count;
            }
        }
        public int DetectedLiteLoader {
            get {
                return liteVersions.Count;
            }
        }
        public int DetectedSelf {
            get {
                return blazeVersions.Count;
            }
        }
        public int DetectedTotal {
            get {
                return detectedVersions.Count;
            }
        }

        public List<Version> allDetectedVersions = new List<Version>();

        public List<Version> detectedVersions = new List<Version>();
        public List<Version> forgeVersions = new List<Version>();
        public List<Version> liteVersions = new List<Version>();
        public List<Version> blazeVersions = new List<Version>();

        public MinecraftDirectory(String path) {
            minecraftDir = Environment.ExpandEnvironmentVariables(path);
            versions = Path.Combine(minecraftDir, "versions");
            libraries = Path.Combine(minecraftDir, "libraries");
        }

        public void readVersions() {
            if (Directory.Exists(versions)) {
                string[] dirs = Directory.GetDirectories(versions);
                foreach (string i in dirs) {
                    var v = new Version(i);
                    if (File.Exists(v.json)) {
                        allDetectedVersions.Add(v);
                        if (isVersionSupported(v)) {
                            detectedVersions.Add(v);
                        }
                    }
                }
            }
        }

        private bool isVersionSupported(Version v) {
            return showUnsupported || v.Name.Split('-')[0] == Configs.MINECRAFT_VERSION;
        }

        public Version createVersion(string name) {
            Version result = new Version(Path.Combine(versions, name));
            Directory.CreateDirectory(result.path);
            return result;
        }

        public LauncherProfiles getProfiles() {
            return new LauncherProfiles(minecraftDir);
        }

        public LibraryManager getLibraries() {
            return new LibraryManager(minecraftDir);
        }
    }

    public class Version {
        public string path;
        public string json;

        public string mcVer;

        public bool isLiteLoader = false;
        public string liteVer;
        public bool isForge = false;
        public string forgeVer;
        public bool isBlazeLoader = false;
        public string blazeVer;
        public bool isMCPatcher = false;

        public Version(string path) {
            this.path = path;
            Name = path.Split('/', '\\').Last();
            json = Path.Combine(path, Name + ".json");
            if (Name.Length > 50) {
                DisplayName = Name.Substring(0, 47) + "...";
            } else {
                DisplayName = Name;
            }
            initApis(Name.ToLower());
        }

        public string Name {
            get; private set;
        }
        public string DisplayName {
            get; private set;
        }

        private string unmatch = null;
        public string UnMatchedName {
            get {
                return unmatch == null ? unmatch = unmatched() : unmatch;
            }
        }

        public string Api {
            get {
                if (isBlazeLoader) return "BlazeLoader";
                if (isLiteLoader) return "LiteLoader";
                if (isForge) return "Minecraft Forge";
                if (isMCPatcher) return "MCPatcher";
                return "Vanilla";
            }
        }

        public void initApis(string nameL) {
            isBlazeLoader = nameL.IndexOf("blazeloader") != -1;
            isLiteLoader = nameL.IndexOf("liteloader") != -1;
            isForge = nameL.IndexOf("forge") != -1;
            isMCPatcher = nameL.IndexOf("mcpatcher") != -1;
            initApiVersions(nameL);
        }

        private void initApiVersions(string nameL) {
            mcVer = nameL.Split('-')[0];
            if (isLiteLoader) {
                liteVer = nameL.Split(new string[] { "liteloader" }, StringSplitOptions.None).Last().Split('-')[0];
            }
            if (isBlazeLoader) {
                blazeVer = nameL.Split(new string[] { "blazeloader" }, StringSplitOptions.None).Last().Split('-')[0];
            }
            if (isForge) {
                if (nameL.IndexOf("forge" + Configs.MINECRAFT_VERSION) == -1) {
                    forgeVer = nameL.Split(new string[] { "forge" }, StringSplitOptions.None).Last().Split('-')[0];
                } else {
                    forgeVer = nameL.Split(new string[] { "forge" + Configs.MINECRAFT_VERSION + "-" }, StringSplitOptions.None).Last().Split('-')[0];
                }
            }
        }

        private string unmatched() {
            string result = Regex.Replace(Name, "-" + mcVer + "-", "", RegexOptions.IgnoreCase);
            if (isBlazeLoader) result = Regex.Replace(result, "blazeloader-" + blazeVer, "", RegexOptions.IgnoreCase);
            if (isLiteLoader) result = Regex.Replace(result, "liteloader" + liteVer, "", RegexOptions.IgnoreCase);
            if (isForge) {
                result = Regex.Replace(result, "forge-" + forgeVer, "", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "forge" + mcVer + "-" + forgeVer, "", RegexOptions.IgnoreCase);
            }
            if (isMCPatcher) {
                result = Regex.Replace(result, "mcpatcher", "", RegexOptions.IgnoreCase);
            }
            result = result.Trim();
            while (result.Contains("--")) result = result.Replace("--", "-");
            if (result.StartsWith("-")) result = result.Substring(1, result.Length);
            if (result.EndsWith("-")) result = result.Substring(0, result.Length - 1);
            return result;
        }
    }
}
