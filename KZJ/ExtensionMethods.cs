﻿#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Drawing;

namespace KZJ {
    public static class ExtensionMethods {

        static readonly char[] invalidFilenameChars = Path.GetInvalidPathChars();

        public static string StripInvalidFilenameChars(this string filename) {
            return new string(filename.Where(ch => !invalidFilenameChars.Contains(ch)).ToArray());
        }


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
            var monoFont = new Font(new FontFamily("Consolas"), grid.Font.Size);
            var t = typeof(T);
            foreach (var p in t.GetProperties()) {
                var col = grid.Columns.AsEnumerable().FirstOrDefault(fod => fod.Name == p.Name);

                var df = p.GetCustomAttributes(false).FirstOrDefault(fod => fod is DisplayFormatAttribute) as DisplayFormatAttribute;
                if (df != null) {
                    if (!string.IsNullOrWhiteSpace(df.DataFormatString)) {
                        col.DefaultCellStyle.Format = df.DataFormatString;
                    }
                }
                var gcf = p.GetCustomAttributes(false).FirstOrDefault(fod => fod is GridColumnFormatAttribute) as GridColumnFormatAttribute;
                if (gcf != null) {
                    col.Visible = !gcf.Hide;
                    if (gcf.Monospaced)
                        col.DefaultCellStyle.Font = monoFont;
                    col.DefaultCellStyle.Alignment = gcf.AlignLeft ? DataGridViewContentAlignment.MiddleLeft : DataGridViewContentAlignment.MiddleRight;
                } else {
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
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

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple =false)]
    public class GridColumnFormatAttribute : Attribute {
        public bool Monospaced { get; set; }
        public bool AlignLeft { get; set; }
        public bool Hide { get; set; }
    }
}
