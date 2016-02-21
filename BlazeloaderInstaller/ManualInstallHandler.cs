using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlazeloaderInstaller {
    class ManualInstallHandler : StageHandler {

        private StackPanel content;

        public override ButtonAction rightAction() {
            return ButtonAction.FINISH;
        }

        public override void init(MainWindow window, IStageHandler prev) {
            ScrollViewer scroller = new ScrollViewer() {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            scroller.Content = content = new StackPanel() {
                Margin = new Thickness(5)
            };
            Canvas.Children.Add(scroller);
            Text("Steps to perform a manual installation. This method assumes that one is already familliar with a minecraft version file.");
            Label("Minecraft Arguments:");
            Box(Configs.LITELOADER_TWEAK);
            Box(Configs.BLAZELOADER_TWEAK);
            Label("Libraries:");
            content.Children.Add(new TextBox() {
                Style = (Style)Application.Current.Resources["TextStyle"],
                Height = 200,
                Text = Configs.LIBRARIES,
                IsReadOnly = true
            });
            Lib("blazeloader-" + Configs.BLAZELOADER_VERSION + ".jar", Configs.BLAZELOADER_URL);
            Lib("liteloader-" + Configs.LITELOADER_VERSION + ".jar", Configs.LITELOADER_URL);
            Label("Main Class:");
            Box(Configs.MAIN_CLASS);
        }

        private void Box(string text) {
            content.Children.Add(new TextBox() {
                Style = (Style)Application.Current.Resources["TextStyle"],
                Text = text,
                IsReadOnly = true
            });
        }

        private void Text(string text) {
            content.Children.Add(new TextBlock() {
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                Text = text,
                Height = 50
            });
        }

        private void Label(string text) {
            content.Children.Add(new Label() {
                Foreground = Brushes.White,
                Content = text
            });
        }

        private void Lib(string name, string dlLocation) {
            StackPanel panel = new StackPanel() {
                Orientation = Orientation.Horizontal
            };
            content.Children.Add(panel);
            panel.Children.Add(new Label() {
                Foreground = Brushes.White,
                Content = name
            });
            Button b = new Button() {
                Content = "Download",
                Template = (ControlTemplate)Application.Current.Resources["ButtonTemplate"],
                Margin = new Thickness(5)
            };
            b.Click += delegate {
                Download_Click(b, dlLocation);
            };
            panel.Children.Add(b);
            if (LibraryManager.hasLibraryEmbed(name)) {
                b = new Button() {
                    Content = "Extract",
                    Template = (ControlTemplate)Application.Current.Resources["ButtonTemplate"],
                    Margin = new Thickness(5)
                };
                b.Click += delegate {
                    Extract_Click(b, name);
                };
                panel.Children.Add(b);
            }
        }

        private void Download_Click(Button sender, string dlLocation) {
            Process.Start(new ProcessStartInfo(dlLocation));
        }

        private void Extract_Click(Button sender, string fileName) {
            string file = this.selectFolder(null);
            if (file != null) {
                file = Path.Combine(file, fileName);
                LibraryManager.extractLibraryLocal(fileName, file);
            }
        }

        public override void next(MainWindow window) {
            Application.Current.Shutdown();
        }
    }
}
