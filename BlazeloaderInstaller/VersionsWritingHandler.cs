using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlazeloaderInstaller {
    class VersionsWritingHandler : StageHandler {
        private MinecraftDirectory man;
        private Version selected;
        private JsonFile versionFile;
        private Library blazeLoaderLib;

        private TextBox name;
        private CheckBox includeProfile;

        private TextBox launcherName;
        private TextBox launcherArgs;

        private ListBox launcherVisibility;

        private bool blocked;
        private bool upgrading;

        private string VersionName {
            get {
                return name.Text;
            }
        }

        public override ButtonAction rightAction() {
            if (blocked) return ButtonAction.CANCEL;
            return ButtonAction.INSTALL;
        }

        public VersionsWritingHandler(MinecraftDirectory man, Version selected) {
            this.selected = selected;
            this.man = man;
            versionFile = selected.file;
        }
        
        private string upgradeVer() {
            foreach (Library i in versionFile.Data.libraries) {
                if (i.name.ToLower().IndexOf("blazeloader") != -1) {
                    blazeLoaderLib = i;
                    string ver = i.name.Split(':').Last();
                    return ver != Configs.BLAZELOADER_VERSION ? ver : null;
                }
            }
            return null;
        }


        public override void init(MainWindow window, IStageHandler prev) {
            if (selected.isBlazeLoader) {
                string ver = upgradeVer();
                TextBlock message = new TextBlock() {
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10, 10, 10, 0),
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Top
                };
                if (ver == null) {
                    message.Text = "Nothing to do at the moment. This version is Up to Date.";
                    blocked = true;
                } else {
                    message.Text = "The selected version already has a previous version of BlazeLoader (" + ver + ") installed. It will be upgraded to the current version.";
                    upgrading = true;
                }
                Canvas.Children.Add(message);
            } else {
                Canvas.Children.Add(new TextBlock() {
                    Text = "A new version will be created with the following name:",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10, 10, 10, 0),
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Top
                });
                name = new TextBox() {
                    Text = createVersionName(),
                    Margin = new Thickness(10, 71, 10, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Style = (Style)Application.Current.Resources["TextStyle"]
                };
                Canvas.Children.Add(name);
                includeProfile = new CheckBox() {
                    Content = "Create a Launcher Profile",
                    IsChecked = true,
                    Foreground = Brushes.White,
                    Margin = new Thickness(10, 180, 10, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };
                includeProfile.Checked += IncludeProfile_Changed;
                includeProfile.Unchecked += IncludeProfile_Changed;
                Canvas.Children.Add(includeProfile);
                launcherName = new TextBox() {
                    Text = createProfileName(),
                    Margin = new Thickness(10, 201, 10, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Style = (Style)Application.Current.Resources["TextStyle"]
                };
                Canvas.Children.Add(launcherName);
                launcherArgs = new TextBox() {
                    Text = Configs.LAUNCHER_ARGS,
                    Margin = new Thickness(10, 241, 10, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Style = (Style)Application.Current.Resources["TextStyle"]
                };
                launcherVisibility = new ListBox() {
                    SelectedItem = "Close launcher when game starts",
                    Margin = new Thickness(10, 281, 10, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Style = (Style)Application.Current.Resources["ComboStyle"],
                    Height = 100
                };
                launcherVisibility.ItemsSource = new List<string> { "Keep the launcher open", "Close launcher when game starts", "Hide launcher and re-open when game closes" };
                if (window.isDev) {
                    Canvas.Children.Add(launcherArgs);
                    Canvas.Children.Add(launcherVisibility);
                }
            }
        }

        private string createVersionName() {
            string result = selected.UnMatchedName + "-BlazeLoader-" + Configs.BLAZELOADER_VERSION;
            if (selected.isForge) {
                result += "-Forge-" + selected.forgeVer;
            }
            if (selected.isMCPatcher) {
                result += "-MCPatcher";
            }
            return result;
        }

        private string createProfileName() {
            string result = selected.UnMatchedName + " BlazeLoader " + Configs.BLAZELOADER_VERSION;
            if (selected.isForge) {
                result += " with Forge " + selected.forgeVer;
                if (selected.isMCPatcher) {
                    result += " and MCPatcher";
                }
            } else if (selected.isMCPatcher) {
                result += " with MCPatcher";
            }
            return result;
        }
        
        private void IncludeProfile_Changed(object sender, RoutedEventArgs e) {
            launcherVisibility.IsEnabled = launcherArgs.IsEnabled = launcherName.IsEnabled = includeProfile.IsChecked == true;
        }

        public override void next(MainWindow window) {
            if (blocked) {
                Application.Current.Shutdown();
                return;
            }
            if (upgrading) {
                blazeLoaderLib.name = "com.blazeloader:blazeloader:" + Configs.BLAZELOADER_VERSION;
                versionFile.save();
                window.initHandler(new LibrariesHandler(man.getLibraries()));
                return;
            }
            Version created = man.createVersion(VersionName, versionFile.Data);
            created.file.save();
            if (includeProfile.IsChecked == true) {
                man.getProfiles().AddProfile(new Profile() {
                    name = VersionName,
                    javaArgs = launcherArgs.Text,
                    launcherVisibilityOnGameClose = (string)launcherVisibility.SelectedItem,
                    lastVersionId = created.Name,
                    useHopperCrashService = false
                });
            }
            window.initHandler(new LibrariesHandler(man.getLibraries()));
        }
    }
}
