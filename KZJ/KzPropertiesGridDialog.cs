#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Windows.Forms.Design;

//using KzL.Utility1;

namespace KZJ
{
    public partial class KzPropertiesGridDialog : Form {

        object settings;
        
        public object Settings {
            get { return settings; }
            set {
                settings = value;
                propertyGrid.SelectedObject = settings;
            }
        }

        public KzPropertiesGridDialog() {
            InitializeComponent();
        }

        public KzPropertiesGridDialog(IPropertiesSettings settings, Icon icon = null, string text = null) : this() {
            Settings = settings;
            if (icon != null) Icon = icon;
            if (text != null) Text = text;
        }

        public KzPropertiesGridDialog(object settings, Icon icon = null, string text = null) : this() {
            Settings = settings;
            if (icon != null) Icon = icon;
            if (text != null) Text = text;
        }

        private void PropertiesDialog_FormClosing(object sender, FormClosingEventArgs e) {
            if (settings == null || !(settings is IPropertiesSettings)) return;
            IPropertiesSettings s = settings as IPropertiesSettings;
            if (DialogResult != DialogResult.OK)
                s.Reload();
            else
                s.Save();
        }
    }

    public interface IPropertiesSettings {
        void Reload();
        void Save();
    }
}
