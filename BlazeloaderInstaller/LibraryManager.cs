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
        

        private string blazeloaderLocation;
        private string liteloaderLocation;

        private string blazeloaderName;
        private string liteloaderName;
        
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
            blazeloaderName = "blazeloader-" + Configs.BLAZELOADER_VERSION + ".jar";
            liteloaderName = "liteloader-" + Configs.LITELOADER_VERSION + ".jar";
            blazeloaderLocation = Path.Combine(libraries, "com", "blazeloader", "blazeloader", Configs.BLAZELOADER_VERSION);
            liteloaderLocation = Path.Combine(libraries, "com", "mumfrey", "liteloader", Configs.LITELOADER_VERSION);
        }

        public void writeRequiredLibraries() {
            int stage = 0;
            tryDownloadLibrary(Configs.LITELOADER_URL, liteloaderLocation, liteloaderName, ref stage, 2, 0);
            tryDownloadLibrary(Configs.BLAZELOADER_URL, blazeloaderLocation, blazeloaderName, ref stage, 2, 0);
            onCompleted();
        }

        private void tryDownloadLibrary(string repo, string destination, string fileName, ref int stage, int total, int progress) {
            string jar = Path.Combine(destination, fileName);
            if (!File.Exists(jar)) {
                onChanged(EventType.START, 0, "Stage " + stage + "/" + total + ": Downloading " + fileName);
                Directory.CreateDirectory(destination);
                downloadLibrary(repo, fileName, jar, progress);
            } else {
                onChanged(EventType.SKIP, 50, "Stage " + stage + "/" + total + ": Downloading " + fileName + " (SKIPPED)");
            }
            stage++;
        }

        private void downloadLibrary(string repo, string fileName, string destination, int progress) {
            WebClient client = new WebClient();
            string url = repo + "/" + fileName;
            try {
                client.DownloadFile(url, destination);          //Attempt to download the latest version
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
        }

        private void extractLibraryLocal(string resource, string file) {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BlazeloaderInstaller.Resources." + resource)) {
                using (FileStream output = File.Create(file)) {
                    stream.CopyTo(output);
                }
            }
        }
    }

    public enum EventType {
        START, SKIP, END, FAIL
    }
}
