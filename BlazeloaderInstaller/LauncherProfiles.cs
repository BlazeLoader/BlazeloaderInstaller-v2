using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace BlazeloaderInstaller {
    public class LauncherProfiles {
        private static DataContractJsonSerializer serialiser = new DataContractJsonSerializer(typeof(Profiles), new DataContractJsonSerializerSettings() {
            UseSimpleDictionaryFormat = true,
            KnownTypes = new List<Type> {
                typeof(Dictionary<string, Profile>), typeof(Dictionary<string, Auth>),
                typeof(Profile), typeof(Auth), typeof(List<Prop>),
                typeof(List<string>), typeof(Prop), typeof(Ver),
                typeof(string), typeof(int), typeof(bool?)
            }
        });

        private string path;
        Profiles Data;

        public LauncherProfiles(string minecraftDir) {
            path = Path.Combine(minecraftDir, "launcher_profiles.json");
            if (File.Exists(path)) {
                try {
                    using (FileStream stream = File.OpenRead(path)) {
                        Data = (Profiles)serialiser.ReadObject(stream);
                    }
                } catch (SerializationException) {
                    Data = new Profiles();
                }
            }
        }

        public void save() {
            using (FileStream stream = File.Create(path)) {
                serialiser.WriteObject(stream, Data);
            }
        }

        public void AddProfile(Profile profile) {
            Data.profiles.Add(profile.name, profile);
            Data.selectedProfile = profile.name;
            save();
        }
    }
    
    [DataContract]
    public class Profiles {
        [DataMember] public Dictionary<string, Profile> profiles;
        [DataMember] public string selectedProfile;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string clientToken;
        [DataMember] public Dictionary<string, Auth> authenticationDatabase;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string selectedUser;
        [DataMember] public Ver launcherVersion;
    }

    [DataContract]
    public class Profile {
        [DataMember] public string name;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string gameDir;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string javaArgs;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public List<string> allowedReleases;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string launcherVisibilityOnGameClose;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string lastVersionId;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public bool? useHopperCrashService;
    }

    [DataContract]
    public class Auth {
        [DataMember] public string displayName;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public List<Prop> userProperties;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string accessToken;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string userid;
        [DataMember(EmitDefaultValue= false, IsRequired = false)] public string uuid;
        [DataMember] public string username;
    }

    [DataContract]
    public class Prop {
        [DataMember] public string name;
        [DataMember] public string value;
    }

    [DataContract]
    public class Ver {
        [DataMember] public string name;
        [DataMember] public string format;
    }
}
