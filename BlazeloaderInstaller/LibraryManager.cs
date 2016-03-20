using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private BackgroundWorker worker;

        private string libraries;
        
        public event ChangedEventHandler StageChanged;
        private void onChanged(object sender, ProgressChangedEventArgs e) {
            if (StageChanged != null) StageChanged(this, (ChangedEventArgs)e.UserState);
        }
        
        public event CompletedEventHandler Completed;
        private void onCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (Completed != null) Completed(this, new EventArgs());
            worker = null;
        }

        public LibraryManager(string minecraftDir) {
            libraries = Path.Combine(minecraftDir, "libraries");
        }
        
        public void writeRequiredLibraries() {
            worker = new BackgroundWorker();
            worker.DoWork += doWork;
            worker.RunWorkerCompleted += onCompleted;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += onChanged;
            worker.RunWorkerAsync();
        }

        private void doWork(object o, DoWorkEventArgs args) {
            int stage = 0;
            tryDownloadLibrary(Configs.LITELOADER_URL, Configs.LITELOADER_LIB, ref stage, 2, 0);
            tryDownloadLibrary(Configs.BLAZELOADER_URL, Configs.BLAZELOADER_LIB, ref stage, 2, 0);
        }

        private void changeState(EventType type, int percentage, params string[] message) {
            worker.ReportProgress(percentage, new ChangedEventArgs() {
                progress = percentage, message = message
            });
        }

        private void tryDownloadLibrary(string repo, string lib, ref int stage, int total, int progress) {
            WebClient client = new WebClient();
            string url = libUrl(repo, lib);
            string destination = libPath(libraries, lib);
            string fileName = Path.GetFileName(url);
            Directory.CreateDirectory(destination);
            destination = Path.Combine(destination, fileName);
            if (!File.Exists(destination)) {
                changeState(EventType.START, 0, "Stage " + stage + "/" + total + ": Downloading " + lib);
                try {
                    try {
                        client.DownloadFile(url, destination);          //Attempt to download the latest version
                    } catch (WebException) {
                        client.DownloadFile(libUrl(repo, lib + "-SNAPSHOT"), destination);
                    }
                    changeState(EventType.END, progress + 15, "");
                } catch (WebException e) {
                    Console.Write(e.Message);
                    try {
                        extractLibraryLocal(fileName, destination); //Otherwise extract it if present
                    } catch (Exception g) {
                        Console.Write(g.Message);                   //Fail if we can't get the file out D:
                        changeState(EventType.FAIL, progress + 15, url, fileName);
                    }
                }
            } else {
                changeState(EventType.SKIP, 50, "Stage " + stage + "/" + total + ": Downloading " + lib + " (SKIPPED)");
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
                    if (e.Response == null || ((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound) negativeCallback(); ;
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
