using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlazeloaderInstaller {
    class ClientInstallHandler : StageHandler {

        private TextBlock message;
        private TextBox url;
        private Button browse;

        public override void init(MainWindow window, IStageHandler prev) {
            message = new TextBlock() {
                Text = "Blazeloader will be installed to the following location.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10, 10, 10, 0),
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Top
            };
            Canvas.Children.Add(message);
            url = new TextBox() {
                Text = "%appdata%/.minecraft",
                Margin = new Thickness(10, 71, 70, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Style = (Style)Application.Current.Resources["TextStyle"]
            };
            Canvas.Children.Add(url);
            browse = new Button() {
                Content = "Browse",
                Margin = new Thickness(0, 71, 10, 0),
                Width = 60,
                Height = 31,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Template = (ControlTemplate)Application.Current.Resources["ButtonTemplate"]
            };
            browse.Click += Browse_Click;
            Canvas.Children.Add(browse);
            Canvas.Children.Add(new TextBlock() {
                Text = "If this is correct click next to continue, otherwise select a preferred location.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10, 107, 10, 0),
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Top
            });
        }

        private void Browse_Click(object sender, RoutedEventArgs e) {
            url.Text = selectFolder(url.Text);
        }

        public override void next(MainWindow window) {
            window.initHandler(new VersionsSelectionHandler(url.Text));
        }

    }
}
