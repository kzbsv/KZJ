﻿#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion
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
    public partial class LayoutConfirmDeleteForm : Form {

        public string LayoutName { get { return labelLayoutName.Text; } set { labelLayoutName.Text = value; } }

        public LayoutConfirmDeleteForm() {
            InitializeComponent();
        }
    }
}
