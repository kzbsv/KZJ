using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KzL.Windows.Forms {
    public partial class OkCancelControl : UserControl {

        public event EventHandler<ValueEventArgs<DialogResult>> DialogResult;

        public string Title {
            get { return labelTitle.Text; }
            set {
                labelTitle.Visible = value != null;
                labelTitle.Text = value;
            }
        }

        public string Prompt {
            get { return labelPrompt.Text; }
            set {
                labelPrompt.Visible = value != null;
                labelPrompt.Text = value;
            }
        }

        void OnDialogResult(DialogResult dr) {
            if (DialogResult != null) DialogResult(this, new ValueEventArgs<DialogResult>(dr));
        }

        public OkCancelControl() {
            InitializeComponent();

            buttonCancel.Click += (s, e) => { OnDialogResult(System.Windows.Forms.DialogResult.Cancel); };
            buttonOk.Click += (s, e) => { OnDialogResult(System.Windows.Forms.DialogResult.OK); };

        }
    }
}
