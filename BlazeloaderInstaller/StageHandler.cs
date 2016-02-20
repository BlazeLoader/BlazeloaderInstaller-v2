using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace BlazeloaderInstaller {
    abstract class StageHandler : IStageHandler {
        public IStageHandler Previous {
            get; set;
        }

        public Grid Canvas {
            get; set;
        }
        
        public virtual ButtonAction leftAction() {
            return ButtonAction.PREVIOUS;
        }

        public virtual ButtonAction rightAction() {
            return ButtonAction.NEXT;
        }

        public virtual ButtonAction centerAction() {
            return ButtonAction.NONE;
        }

        public void prev(MainWindow window) {
            window.reinitHandler(Previous);
        }

        protected string selectFolder(string initialPath) {
            FolderBrowserDialog d = new FolderBrowserDialog() {
                ShowNewFolderButton = true,
                SelectedPath = Environment.ExpandEnvironmentVariables(initialPath)
            };
            if (d.ShowDialog() == DialogResult.OK) {
                return d.SelectedPath;
            }
            return initialPath;
        }

        public abstract void init(MainWindow window, IStageHandler prev);
        public abstract void next(MainWindow window);
    }
}
