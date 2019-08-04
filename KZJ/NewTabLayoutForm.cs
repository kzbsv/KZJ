using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KZJ {
    public partial class NewTabLayoutForm : Form {

        public string NewTabName { get { return textBox1.Text; } }

        public NewTabLayoutForm () {
            InitializeComponent();
        }

        private void buttonSave_Click(object sender, EventArgs e) {
        }
    }
}
