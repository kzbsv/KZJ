using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KZJ {
    public class UiLayoutTabControl : TabControl {
        bool tabsVisible = true;

        [DefaultValue(true)]
        public bool TabsVisible {
            get { return tabsVisible; }
            set {
                if (tabsVisible == value) return;
                tabsVisible = value;
                RecreateHandle();
            }
        }

        protected override void WndProc(ref Message m) {
            // Hide tabs by trapping the TCM_ADJUSTRECT message
            if (m.Msg == 0x1328) {
                if (!tabsVisible && !DesignMode) {
                    m.Result = (IntPtr)1;
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }
}
