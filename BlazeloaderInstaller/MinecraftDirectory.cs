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
                    var v = new Version(this, i);
                    if (v.file != null) {
                        allDetectedVersions.Add(v);
                        if (isVersionSupported(v)) {
                            detectedVersions.Add(v);
                        }
                    }
                }
                foreach (Version v in allDetectedVersions) {
                    v.initApis(v.Name.ToLower());
                }
            }
        }

        private bool isVersionSupported(Version v) {
            return showUnsupported || v.Name.Split('-')[0] == Configs.MINECRAFT_VERSION;
        }

        public Version createVersion(string name, JsonFormat data) {
            Version result = new Version(this, name, Path.Combine(versions, name), data);
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

        public JsonFile file {
            get; private set;
        }

        public string mcVer;

        public bool isLiteLoader = false;
        public string liteVer;
        public bool isForge = false;
        public string forgeVer;
        public bool isBlazeLoader = false;
        public string blazeVer;
        public bool isMCPatcher = false;
        public string mcpVer;

        private MinecraftDirectory mc;

        public Version(MinecraftDirectory mc, string path) {
            this.mc = mc;
            this.path = path;
            Name = path.Split('/', '\\').Last();
            json = Path.Combine(path, Name + ".json");
            if (Name.Length > 50) {
                DisplayName = Name.Substring(0, 47) + "...";
            } else {
                DisplayName = Name;
            }
            if (Name == Configs.MINECRAFT_VERSION) {
                DisplayName += " (recommended)";
            }
            if (File.Exists(json)) {
                file = new JsonFile(json);
                if (file.IsCorrupt) {
                    file = null;
                    return;
                }
            }
        }

        public Version(MinecraftDirectory mc, string name, string path, JsonFormat data) {
            this.mc = mc;
            this.path = path;
            Name = name;
            json = Path.Combine(path, Name + ".json");
            file = new JsonFile(json, Name, data);
        }

        private Version() { }

        public string Name {
            get; private set;
        }
        public string DisplayName {
            get; private set;
        }

        private string _desc = null;
        public string Description {
            get {
                return _desc == null ? _desc = desc() : _desc;
            }
        }

        private string unmatch = null;
        public string UnMatchedName {
            get {
                return unmatch == null ? unmatch = unmatched() : unmatch;
            }
        }

        private Version parent = null;
        public Version Parent {
            get {
                if (parent == null) {
                    foreach (Version i in mc.allDetectedVersions) {
                        if (i.Name == file.inheritsFrom) return i;
                    }
                }
                return parent;
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
            initialised = true;
            if (Parent != null) {
                Parent.inheritApis(this);
            }
            Library lib = null;
            mcVer = nameL.Split('-')[0];
            if ((lib = file.matchingLibrary("blazeloader")) != null) {
                isBlazeLoader = true;
                blazeVer = lib.name.Split(':').Last();
            }
            if ((lib = file.matchingLibrary("liteloader")) != null) {
                isLiteLoader = true;
                liteVer = lib.name.Split(':').Last();
            }
            if ((lib = file.matchingLibrary("forge")) != null) {
                isForge = true;
                forgeVer = lib.name.Split(':').Last().Split('-').Last();
            }
            if (mcpVer == null) {
                mcpVer = MCPatcherOpts.getMCPatcherVersion(this);
                isMCPatcher = mcpVer != null;
            }
        }

        private bool initialised = false;
        private bool recurseCatch = false;
        private void inheritApis(Version target) {
            if (target == this || recurseCatch) return;
            if (!initialised) {
                recurseCatch = true;
                initApis(Name.ToLower());
                recurseCatch = false;
            }
            if (isBlazeLoader) {
                target.isBlazeLoader = isBlazeLoader;
                target.blazeVer = blazeVer;
            }
            if (isLiteLoader) {
                target.isLiteLoader |= isLiteLoader;
                target.liteVer = liteVer;
            }
            if (isForge) {
                target.isForge |= isForge;
                target.forgeVer = forgeVer;
            }
            if (isMCPatcher) {
                target.isMCPatcher |= isMCPatcher;
                target.mcpVer = mcpVer;
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

        private string desc() {
            string result = Name;
            if (Api != "Vanilla") {
                result = UnMatchedName + " with";
                if (isBlazeLoader) result += "\n BlazeLoader " + blazeVer;
                if (isLiteLoader) result += "\n LiteLoader " + liteVer;
                if (isForge) result += "\n Minecraft Forge " + forgeVer;
                if (isMCPatcher) result += "\n MCPatcher " + mcpVer;
            }
            return result;
        }
    }
}
