﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KzL.Windows.Forms {
    public partial class LayoutSaveAsForm : Form {

        public string SaveAsName { get { return textBox1.Text; } }

        public LayoutSaveAsForm() {
            InitializeComponent();
        }

        private void buttonSave_Click(object sender, EventArgs e) {
        }
    }
}