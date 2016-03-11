using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazeloaderInstaller {
    public sealed class ChangedEventArgs : EventArgs {
        public EventType type;
        public int progress;
        public string[] message;
    }
    public delegate void ChangedEventHandler(object sender, ChangedEventArgs args);

    public delegate void CompletedEventHandler(object sender, EventArgs args);

    public class LibraryManager {
        private string libraries;
        
        public event ChangedEventHandler StageChanged;
        private void onChanged(EventType ev, int progress, params string[] message) {
            if (StageChanged != null) StageChanged(this, new ChangedEventArgs() {
                type = ev, progress = progress, message = message
            });
        }
        
        public event CompletedEventHandler Completed;
        private void onCompleted() {
            if (Completed != null) Completed(this, new EventArgs());
        }

        public LibraryManager(string minecraftDir) {
            libraries = Path.Combine(minecraftDir, "libraries");
        }

        public void writeRequiredLibraries() {
            int stage = 0;
            tryDownloadLibrary(Configs.LITELOADER_URL, Configs.LITELOADER_LIB, ref stage, 2, 0);
            tryDownloadLibrary(Configs.BLAZELOADER_URL, Configs.BLAZELOADER_LIB, ref stage, 2, 0);
            onCompleted();
        }
        
        private void tryDownloadLibrary(string repo, string lib, ref int stage, int total, int progress) {
            WebClient client = new WebClient();
            string url = libUrl(repo, lib);
            string destination = libPath(libraries, lib);
            string fileName = Path.GetFileName(url);
            Directory.CreateDirectory(destination);
            destination = Path.Combine(destination, fileName);
            if (!File.Exists(destination)) {
                onChanged(EventType.START, 0, "Stage " + stage + "/" + total + ": Downloading " + lib);
                try {
                    try {
                        client.DownloadFile(url, destination);          //Attempt to download the latest version
                    } catch (WebException) {
                        client.DownloadFile(libUrl(repo, lib + "-SNAPSHOT"), destination);
                    }
                    onChanged(EventType.END, progress + 15, "");
                } catch (WebException e) {
                    Console.Write(e.Message);
                    try {
                        extractLibraryLocal(fileName, destination); //Otherwise extract it if present
                    } catch (Exception g) {
                        Console.Write(g.Message);                   //Fail if we can't get the file out D:
                        onChanged(EventType.FAIL, progress + 15, url, fileName);
                    }
                }
            } else {
                onChanged(EventType.SKIP, 50, "Stage " + stage + "/" + total + ": Downloading " + lib + " (SKIPPED)");
            }
            stage++;
        }

        public static string libUrl(string url, string lib) {
            string[] libParts = lib.Replace(":", " ").Trim().Split(' ').Reverse().ToArray();
            return Path.Combine(url, parseLib(lib), libParts[1] + "-" + libParts[0] + ".jar");
        }

        public static string libPath(string path, string lib) {
            return Path.Combine(path, parseLib(lib));
        }

        private static string parseLib(string lib) {
            string[] parts = lib.Replace(":", " ").Trim().Split(' ');
            parts[0] = Path.Combine(parts[0].Split('.'));
            return Path.Combine(parts);
        }

        public static void verifyLocation(string url, Action negativeCallback) {
            Task.Factory.StartNew(() => {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = null;
                request.Method = "HEAD";
                try {
                    response = request.GetResponse();
                } catch (WebException e) {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound) negativeCallback(); ;
                } finally {
                    if (response != null) response.Close();
                }
            });
        }

        public static void extractLibraryLocal(string resource, string file) {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BlazeloaderInstaller.Resources." + resource)) {
                using (FileStream output = File.Create(file)) {
                    stream.CopyTo(output);
                }
            }
        }

        public static bool hasLibraryEmbed(string resource) {
            foreach (string name in Assembly.GetExecutingAssembly().GetManifestResourceNames()) {
                if (name == "BlazeloaderInstaller.Resources." + resource) {
                    return true;
                }
            }
            return false;
        }
    }

    public enum EventType {
        START, SKIP, END, FAIL
    }
}
