using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace BlazeloaderInstaller {
    public enum ButtonAction {
        PREVIOUS,
        NEXT,
        OK,
        CANCEL,
        CONTINUE,
        FINISH,
        INSTALL,
        NONE
    }

    public static class ButtonActions {
        public static Visibility getVisibility(this ButtonAction e) {
            return e == ButtonAction.NONE ? Visibility.Hidden : Visibility.Visible;
        }

        public static string getName(this ButtonAction e) {
            switch (e) {
                case ButtonAction.PREVIOUS: return "Previous";
                case ButtonAction.NEXT: return "Next";
                case ButtonAction.OK: return "Ok";
                case ButtonAction.CANCEL: return "Cancel";
                case ButtonAction.CONTINUE: return "Continue";
                case ButtonAction.FINISH: return "Finish";
                case ButtonAction.INSTALL: return "Install";
            }
            return "";
        }
    }
}
