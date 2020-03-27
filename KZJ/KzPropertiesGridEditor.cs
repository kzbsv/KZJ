#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;

//using KzL.Utility1;

namespace KZJ
{
    public class KzPropertiesGridEditor : System.Drawing.Design.UITypeEditor {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            if (value is ICloneBySerialization) {
                ICloneBySerialization v = value as ICloneBySerialization;
                object c = v.CloneBySerialization();
                KzPropertiesGridDialog f = new KzPropertiesGridDialog(c);
                PropertyInfo pi = context.GetType().GetProperty("PropertyLabel");
                if (pi != null) {
                    f.Text = pi.GetValue(context, null) as string;
                }
                if (f.ShowDialog() == DialogResult.OK) {
                    return f.Settings;
                } else {
                    return value;
                }
            } else {
                MessageBox.Show("Type does not derive from KzL.Drawing.ICloneBySerialization.");
                return value;
            }
        }
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            return System.Drawing.Design.UITypeEditorEditStyle.Modal;
        }
    }
}
