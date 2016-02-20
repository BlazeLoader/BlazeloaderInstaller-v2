using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BlazeloaderInstaller {
    public interface IStageHandler {
        IStageHandler Previous {
            get; set;
        }
        Grid Canvas {
            get; set;
        }
        
        ButtonAction leftAction();

        ButtonAction rightAction();

        ButtonAction centerAction();

        void init(MainWindow window, IStageHandler prev);
        
        void next(MainWindow window);

        void prev(MainWindow window);
    }
}
