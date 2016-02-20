using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace BlazeloaderInstaller {
    public class Repo {
        private static DataContractJsonSerializer serialiser = new DataContractJsonSerializer(typeof(Manifest), new DataContractJsonSerializerSettings() {
            UseSimpleDictionaryFormat = true
        });

        private string path;
        private Manifest manifest;

        public Repo(string url) {
            path = url;
        }

        public Manifest fetchManifest() {
            if (manifest != null) return manifest;
            WebClient client = new WebClient();
            using (Stream s = client.OpenRead(Path.Combine(path, "versions.json"))) {
                manifest = (Manifest)serialiser.ReadObject(s);
            }
            return manifest;
        }

        public Artefact find(string mcversion, string version, string project) {
            foreach (KeyValuePair<string, Artefact> k in fetchManifest().versions[mcversion].artefacts[project]) {
                if (k.Value.version == version) return k.Value;
            }
            return null;
        }

        public Artefact getArtefact(string mcversion, string project, string artefactId) {
            return fetchManifest().versions[mcversion].artefacts[project][artefactId];
        }

        public Artefact latest(string mcversion, string project) {
            return getArtefact(mcversion, project, "latest");
        }

        public string getFileLocation(Artefact artefact) {
            return Path.Combine(path, artefact.file);
        }
    }

    [DataContract]
    public class Manifest {
        [DataMember] public Meta meta;
        [DataMember] public Dictionary<string, RepoVersion> versions;
    }

    [DataContract]
    public class Meta {
        [DataMember] public string description;
        [DataMember] public string authors;
        [DataMember] public string url;
    }

    [DataContract]
    public class RepoVersion {
        [DataMember] public Dictionary<string, Dictionary<string, Artefact>> artefacts;
    }

    [DataContract]
    public class Artefact {
        [DataMember] public string tweakClass;
        [DataMember] public List<Library> libraries;
        [DataMember] public string file;
        [DataMember] public string version;
        [DataMember] public string md5;
        [DataMember] public string timestamp;
    }
}
