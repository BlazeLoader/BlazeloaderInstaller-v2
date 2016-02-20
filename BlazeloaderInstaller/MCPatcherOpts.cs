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
        public static string getMCPatcherVersion(Version ver) {
            string jar = Path.Combine(ver.path, ver.Name + ".jar");
            if (File.Exists(jar)) {
                using (ZipArchive z = ZipFile.OpenRead(jar)) {
                    var entry = z.GetEntry("mcpatcher.properties");
                    if (entry != null) {
                        byte[] data;
                        using (Stream s = entry.Open()) {
                            data = new byte[s.Length];
                            s.Read(data, 0, data.Length);
                        }
                        string[] lines = Convert.ToBase64String(data).Split('\n');
                        foreach (string i in lines) {
                            string[] pair = i.Split('=');
                            if (pair.Length > 1 && pair[0] == "patcherVersion") return pair[1];
                        }
                    }
                }
            }
            return "unknown";
        }
    }
}
