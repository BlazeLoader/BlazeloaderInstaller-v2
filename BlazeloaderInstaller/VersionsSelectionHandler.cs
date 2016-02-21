using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace BlazeloaderInstaller {
    class VersionsSelectionHandler : StageHandler {
        private MinecraftDirectory man;

        private MainWindow win;

        private ListBox listing;

        public VersionsSelectionHandler(string path) {
            man = new MinecraftDirectory(path);
        }
        
        public override ButtonAction rightAction() {
            if (man.DetectedTotal > 0) return ButtonAction.NEXT;
            return ButtonAction.NONE;
        }

        public override void init(MainWindow window, IStageHandler prev) {
            win = window;
            man.readVersions();
            if  (man.DetectedTotal  == 0) {
                Canvas.Children.Add(new TextBlock() {
                    Text = "Error: No valid minecraft installation was found at the given location.",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10, 10, 10, 0),
                    Foreground = Brushes.Red,
                    VerticalAlignment = VerticalAlignment.Top
                });
            } else {
                win.NextEnabled = false;
                Canvas.Children.Add(new TextBlock() {
                    Text = "Please select a version to extend from.",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10, 10, 10, 0),
                    VerticalAlignment = VerticalAlignment.Top
                });
                if (window.isDev) {
                    CheckBox showa = new CheckBox() {
                        Content = "Display All Versions",
                        Margin = new Thickness(10, 30, 0, 0),
                        Width = 130,
                        Height = 18,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Foreground = (SolidColorBrush)Application.Current.Resources["Element_Foreground"]
                    };
                    Canvas.Children.Add(showa);
                    showa.Unchecked += Showa_Unchecked;
                    showa.Checked += Showa_Checked;
                }
                ScrollViewer scroll = new ScrollViewer() {
                    Margin = new Thickness(10, 50, 10, 10),
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    Background = (SolidColorBrush)Application.Current.Resources["Element_Background"]
                };
                Canvas.Children.Add(scroll);
                scroll.Content = new StackPanel();
                listing = new ListBox() {
                    ItemTemplate = (DataTemplate)Application.Current.Resources["VersionItems"],
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0)
                };
                listing.SelectionChanged += Listing_SelectionChanged;
                ((StackPanel)scroll.Content).Children.Add(listing);
                listing.GroupStyle.Add((GroupStyle)Application.Current.Resources["VersionGrouping"]);
                setSource(man.detectedVersions);
            }
        }

        private void Listing_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            win.NextEnabled = listing.SelectedItem != null;
        }

        private void setSource(List<Version> versions) {
            listing.ItemsSource = versions;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(versions);
            if (view.GroupDescriptions.Count == 0) view.GroupDescriptions.Add(new PropertyGroupDescription("Api"));
        }

        private void Showa_Checked(object sender, RoutedEventArgs e) {
            setSource(man.allDetectedVersions);
        }

        private void Showa_Unchecked(object sender, RoutedEventArgs e) {
            setSource(man.detectedVersions);
        }

        public override void next(MainWindow window) {
            if (win.NextEnabled) {
                window.initHandler(new VersionsWritingHandler(man, ((Version)listing.SelectedItem)));
            }
        }

        public override void prev(MainWindow window) {
            win.NextEnabled = true;
            base.prev(window);
        }
    }
}
