using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace BlazeloaderInstaller {
    public static class MCPatcherOpts {
        private static char[] LINES = new char[] { '\n', '\r' };
        public static string getMCPatcherVersion(Version ver) {
            string jar = Path.Combine(ver.path, ver.Name + ".jar");
            if (File.Exists(jar)) {
                using (ZipArchive z = ZipFile.OpenRead(jar)) {
                    var entry = z.GetEntry("mcpatcher.properties");
                    if (entry != null) {
                        string data;
                        using (StreamReader reader = new StreamReader(entry.Open())) {
                            data = reader.ReadToEnd();
                        }
                        string[] lines = data.Split(LINES);
                        foreach (string i in lines) {
                            string[] pair = i.Split('=');
                            if (pair.Length > 1 && pair[0] == "patcherVersion") return pair[1].Trim();
                        }
                    }
                }
            }
            return null;
        }
    }
}
