using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;

namespace BlazeloaderInstaller {
    class LibrariesHandler : StageHandler {

        private MainWindow win;
        private LibraryManager man;

        private TextBlock message;
        private ProgressBar progress;

        private bool done = false;
        private int passed = 0;
        private int skipped = 0;

        private List<string[]> failed = new List<string[]>();

        public override ButtonAction leftAction() {
            return ButtonAction.NONE;
        }

        public override ButtonAction rightAction() {
            if (done) return ButtonAction.FINISH;
            return ButtonAction.CONTINUE;
        }

        public LibrariesHandler(LibraryManager manager) {
            man = manager;
        }

        public override void init(MainWindow window, IStageHandler prev) {
            win = window;
            message = new TextBlock() {
                Text = "Almost done! We just have to download some libraries.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10, 10, 10, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            Canvas.Children.Add(message);
            progress = new ProgressBar() {
                Style = (Style)Application.Current.Resources["ProgressStyle"],
                Margin = new Thickness(10, 98, 10, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            man.StageChanged += Man_StageChanged;
            man.Completed += Man_Completed;
        }

        private void Man_Completed(object sender, EventArgs args) {
            done = true;
            win.updateVisibilities(2);
            progress.Visibility = Visibility.Hidden;
            if (failed.Count > 0) {
                message.Text = "Some libraries could not be retrieved. You can continue now and the launcher will attempt to download any missing libraries, otherwise you can get the libraries from their respective sources.\n";
                foreach (string[] i in failed) {
                    Hyperlink link = new Hyperlink(new Run(i[1])) {
                        NavigateUri = new Uri(i[0])
                    };
                    link.RequestNavigate += Link_RequestNavigate;
                    message.Inlines.Add(link);
                    message.Inlines.Add("\n");
                }
            } else {
                message.Text = "Installation complete!";
            }
            win.NextEnabled = true;
        }

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Man_StageChanged(object sender, ChangedEventArgs args) {
            switch (args.type) {
                case EventType.SKIP: skipped++; break;
                case EventType.FAIL: failed.Add(args.message); break;
                case EventType.END: passed++; break;
            }
            this.message.Text = args.message[0];
            this.progress.Value = args.progress;
        }

        public override void next(MainWindow window) {
            if (done) {
                Application.Current.Shutdown();
            } else {
                message.Text = "Downloading...";
                Canvas.Children.Add(progress);
                win.NextEnabled = false;
                man.writeRequiredLibraries();
            }
        }
    }
}
