using System.Windows;
using System.Collections.Generic;
using System;
using System.Windows.Controls;

namespace BlazeloaderInstaller {
    public partial class MainWindow : Window {

        private IStageHandler currentHandler = null;

        private int _currentStage = 1;
        public int Stage {
            private get {
                return _currentStage;
            }
            set {
                if (value < 1) value = 1;
                _currentStage = value;
            }
        }

        public ButtonAction leftAction {
            get {
                if (currentHandler == null) {
                    return _currentStage > 1 ? ButtonAction.PREVIOUS : ButtonAction.NONE;
                }
                return currentHandler.leftAction();
            }
        }
        public ButtonAction rightAction {
            get {
                if (currentHandler == null) {
                    return _currentStage != 2 ? ButtonAction.NEXT : ButtonAction.NONE;
                }
                return currentHandler.rightAction();
            }
        }
        public ButtonAction centerAction {
            get {
                return currentHandler != null ? currentHandler.centerAction() : ButtonAction.NONE;
            }
        }

        public MainWindow() {
            InitializeComponent();
            subtitle.Content = Configs.BLAZELOADER_VERSION + " for Minecraft " + Configs.MINECRAFT_VERSION;
        }

        public Visibility PrevVisibility {
            get {
                return leftAction.getVisibility();
            }
        }
        public Visibility NextVisibility {
            get {
                return rightAction.getVisibility();
            }
        }
        public string PrevName {
            get {
                return leftAction.getName();
            }
        }
        public string NextName {
            get {
                return rightAction.getName();
            }
        }

        public bool PrevEnabled {
            get {
                return prev.IsEnabled;
            }
            set {
                prev.IsEnabled = value;
            }
        }
        public bool NextEnabled {
            get {
                return next.IsEnabled;
            }
            set {
                next.IsEnabled = value;
            }
        }
        
        public bool isDev {
            get {
                return iamdev.IsChecked == true;
            }
        }

        public void updateVisibilities(int stage) {
            prev.Visibility = PrevVisibility;
            prev.Content = PrevName;
            next.Visibility = NextVisibility;
            next.Content = NextName;
            if (stage == 1) {
                stage_1.Visibility = Visibility.Visible;
                stage_2.Visibility = Visibility.Hidden;
            } else if (stage == 2) {
                stage_1.Visibility = Visibility.Hidden;
                stage_2.Visibility = Visibility.Visible;
                dev_install.Visibility = bToV(iamdev.IsChecked);
            } else {
                stage_1.Visibility = Visibility.Hidden;
                stage_2.Visibility = Visibility.Hidden;
            }
        }

        private Visibility bToV(bool? b) {
            return b == true ? Visibility.Visible : Visibility.Hidden;
        }

        public void initHandler(IStageHandler h) {
            handlerManaged.Children.Clear();
            if (h != null) {
                h.Canvas = new Grid();
                handlerManaged.Children.Add(h.Canvas);
                h.Previous = currentHandler;
                h.init(this, currentHandler);
                Scripted.Visibility = Visibility.Hidden;
                handlerManaged.Visibility = Visibility.Visible;
            } else {
                Scripted.Visibility = Visibility.Visible;
                handlerManaged.Visibility = Visibility.Hidden;
            }
            currentHandler = h;
            updateVisibilities(Stage);
        }

        public void reinitHandler(IStageHandler h) {
            if (h != null) {
                handlerManaged.Children.Clear();
                handlerManaged.Children.Add(h.Canvas);
            } else {
                Scripted.Visibility = Visibility.Visible;
                handlerManaged.Visibility = Visibility.Hidden;
            }
            currentHandler = h;
            updateVisibilities(Stage);
        }

        private void next_Click(object sender, RoutedEventArgs e) {
            if (currentHandler != null) {
                currentHandler.next(this);
            } else {
                updateVisibilities(++Stage);
            }
        }

        private void prev_Click(object sender, RoutedEventArgs e) {
            if (currentHandler != null) {
                currentHandler.prev(this);
            } else {
                updateVisibilities(--Stage);
            }
        }
        
        private void manual_install_Click(object sender, RoutedEventArgs e) {
            initHandler(new ManualInstallHandler());
        }

        private void server_install_Click(object sender, RoutedEventArgs e) {

        }

        private void client_install_Click(object sender, RoutedEventArgs e) {
            initHandler(new ClientInstallHandler());
        }

        private void dev_install_Click(object sender, RoutedEventArgs e) {

        }
    }
}