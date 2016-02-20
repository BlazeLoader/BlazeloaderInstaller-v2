using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace BlazeloaderInstaller {
    public class JsonFile {
        private static DataContractJsonSerializer serialiser = new DataContractJsonSerializer(typeof(JsonFormat));

        private string path;
        public JsonFormat Data;

        public JsonFile(string path) {
            this.path = path;
            if (File.Exists(path)) {
                using (FileStream stream = File.OpenRead(path)) {
                    Data = (JsonFormat)serialiser.ReadObject(stream);
                }
            }
        }

        public JsonFile(string path, string name, JsonFormat inherited) {
            this.path = path;
            Data = new JsonFormat() {
                inheritsFrom = inherited.id,
                id = name,
                time = inherited.time,
                releaseTime = inherited.releaseTime,
                type = inherited.type,
                minecraftArguments = inherited.minecraftArguments,
                mainClass = "net.minecraft.launchwrapper.Launch",
                minimumLauncherVersion = inherited.minimumLauncherVersion,
                jar = inherited.jar
            };
            Data.libraries = new List<Library>();
            Data.libraries.Add(new Library() {
                name = "com.blazeloader:blazeloader:" + Configs.BLAZELOADER_VERSION,
                url = Configs.BLAZELOADER_URL
            });
            if (Data.minecraftArguments.ToLower().IndexOf("liteloader") == -1) {
                Data.minecraftArguments += " " + Configs.LITELOADER_TWEAK;
                Data.libraries.Add(new Library() {
                    name = "com.mumfrey:liteloader:" + Configs.LITELOADER_VERSION,
                    url = Configs.LITELOADER_URL
                });
            }
            Data.minecraftArguments += " " + Configs.BLAZELOADER_TWEAK;
            Data.libraries.Add(new Library() {
                name = "net.minecraft:launchwrapper:" + Configs.LAUNCH_WRAPPER
            });
            Data.libraries.Add(new Library() {
                name = "org.ow2.asm:asm-all:" + Configs.ASM
            });
        }

        public void save() {
            using (FileStream stream = File.Create(path)) {
                serialiser.WriteObject(stream, Data);
            }
        }
    }

    [DataContract]
    public class JsonFormat {
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string inheritsFrom;
        [DataMember] public string id;
        [DataMember] public string time;
        [DataMember] public string releaseTime;
        [DataMember] public string type;
        [DataMember] public string minecraftArguments;
        [DataMember] public List<Library> libraries;
        [DataMember] public string mainClass;
        [DataMember] public int minimumLauncherVersion;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string assets;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public Downloads downloads;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public AssetIndex assetIndex;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string jar;
    }

    [DataContract]
    public class Library {
        [DataMember] public string name;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string url;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public List<Rule> rules;
    }

    [DataContract]
    public class Rule {
        [DataMember] public string action;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public Named os;
    }

    [DataContract]
    public class Named {
        [DataMember] public string name;
    }

    [DataContract]
    public class Downloads {
        [DataMember] public Download client;
        [DataMember] public Download server;
        [DataMember] public Download windows_server;
    }

    [DataContract]
    public class Download {
        [DataMember] public string url;
        [DataMember] public string sha1;
        [DataMember] public long size;
    }

    [DataContract]
    public class AssetIndex {
        [DataMember] public long totalSize;
        [DataMember] public string id;
        [DataMember] public bool known;
        [DataMember] public string url;
        [DataMember] public string sha1;
        [DataMember] public long size;
    }
}
