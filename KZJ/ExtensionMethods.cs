using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.DataAnnotations;

namespace KZJ {
    public static class ExtensionMethods {

        public static IEnumerable<TreeNode> AsEnumerable(this TreeNodeCollection nodes) {
            for (int i = 0; i < nodes.Count; i++) {
                var n = nodes[i];
                yield return n;
                if (n.Nodes != null) foreach (var cn in n.Nodes.AsEnumerable()) yield return cn;
            }
        }

        public static IEnumerable<DataGridViewRow> AsEnumerable(this DataGridViewRowCollection rows) {
            for (int i = 0; i < rows.Count; i++) yield return rows[i];
        }

        public static IEnumerable<DataGridViewRow> AsEnumerable(this DataGridViewSelectedRowCollection rows) {
            for (int i = 0; i < rows.Count; i++) yield return rows[i];
        }

        public static IEnumerable<DataGridViewCell> AsEnumerable(this DataGridViewSelectedCellCollection cells) {
            for (int i = 0; i < cells.Count; i++) yield return cells[i];
        }

        public static IEnumerable<TabPage> AsEnumerable(this TabControl.TabPageCollection pages) {
            for (int i = 0; i < pages.Count; i++) yield return pages[i];
        }

        public static IEnumerable<Control> AsEnumerable(this Control.ControlCollection cc) {
            for (int i = 0; i < cc.Count; i++) yield return cc[i];
        }

        public static IEnumerable<Control> AllChildControls(this Control c) {
            for (int i = 0; i < c.Controls.Count; i++) {
                var d = c.Controls[i];
                yield return d;
                foreach (var child in d.AllChildControls())
                    yield return child;
            }
        }

        public static void ApplyDisplayFormat<T>(this DataGridView grid, IList<T> data) where T: class {
            var t = typeof(T);
            foreach (var p in t.GetProperties()) {
                var df = p.GetCustomAttributes(false).FirstOrDefault(fod => fod is DisplayFormatAttribute) as DisplayFormatAttribute;
                if (df != null) {
                    if (!string.IsNullOrWhiteSpace(df.DataFormatString)) {
                        var col = grid.Columns.AsEnumerable().FirstOrDefault(fod => fod.Name == p.Name);
                        col.DefaultCellStyle.Format = df.DataFormatString;
                    }
                }
            }
        }

        public static IEnumerable<DataGridViewColumn> AsEnumerable(this DataGridViewColumnCollection gridCols) {
            for (var i = 0; i < gridCols.Count; i++) yield return gridCols[i];
        }

        public static IEnumerable<DataGridViewCell> AsEnumerable(this DataGridViewCellCollection gridCells) {
            for (var i = 0; i < gridCells.Count; i++) yield return gridCells[i];
        }
    }
}
