#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KZJ {

    public class DynamicGridControl<T> : UserControl where T : class {

        protected SortableBindingList<T> _Data = new SortableBindingList<T>();

        protected DataGridView _Grid;
        protected ToolStripContainer _ToolStrips;
        protected StatusStrip _Status;
        protected ToolStripStatusLabel _Label;
        protected ContextMenuStrip _GridMenu;

        public DataGridView Grid => _Grid;
        public ToolStripContainer ToolStrips => _ToolStrips;
        public StatusStrip Status => _Status;
        public ToolStripStatusLabel Label => _Label;
        public ContextMenuStrip GridMenu => _GridMenu;

        protected string IndexColumn { get; set; } = "Index";
        protected string DataColumn { get; set; } = string.Empty;
        protected string[] MonospacedColumns { get; set; } = new string[0];
        protected string[] LeftAlignedColumns { get; set; } = new string[0];

        public DynamicGridControl() {
            InitializeControls();
        }

        void InitializeControls() {
            _GridMenu = new ContextMenuStrip();
            GridMenuAddItem("&Copy").Click += _GridMenuCopy;

            _Grid = new DataGridView();
            _Grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _Grid.Dock = DockStyle.Fill;

            _Grid.AllowUserToAddRows = false;
            _Grid.AllowUserToDeleteRows = false;
            _Grid.AllowUserToResizeRows = false;
            _Grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _Grid.ContextMenuStrip = _GridMenu;
            _Grid.ReadOnly = true;
            _Grid.RowHeadersVisible = false;
            _Grid.ShowEditingIcon = false;

            _Grid.AutoGenerateColumns = true;

            _Grid.KeyPress += GridKeyPress;

            _Label = new ToolStripStatusLabel();
            _Label.BorderSides = ToolStripStatusLabelBorderSides.Right;
            _Label.BorderStyle = Border3DStyle.Etched;
            _Label.Text = "This is some _Status _Label text...";

            _Status = new StatusStrip();
            _Status.Items.AddRange(new ToolStripItem[] {
            _Label,
            });

            _ToolStrips = new ToolStripContainer();
            _ToolStrips.ContentPanel.Controls.Add(_Grid);
            _ToolStrips.TopToolStripPanel.Controls.Add(_Status);
            _ToolStrips.Dock = DockStyle.Fill;
            
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_ToolStrips);

            _Grid.DataSource = _Data;
        }

        public void SetData(IEnumerable<T> data, string description = null) => SetData(data.ToArray(), description);

        public void SetData(IList<T> data, string description = null) {
            //_Grid.CurrentCellChanged -= _Grid_CurrentCellChanged;
            //_Grid.SelectionChanged -= _Grid_SelectionChanged;
            try {
                _Data = new SortableBindingList<T>(data.ToList());
                _Grid.DataSource = _Data;
                SetColStyles();
                //foreach (var row in _Grid.Rows.AsEnumerable()) SetRowColors(row);
                _Grid.Columns[DataColumn].Visible = false;
                _Grid.ApplyDisplayFormat(_Data);
                _Grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                _Label.Text = description;
            } finally {
                //_Grid.SelectionChanged += _Grid_SelectionChanged;
                //_Grid.CurrentCellChanged += _Grid_CurrentCellChanged;
            }
        }

        void SetColStyles() {
            foreach (var col in _Grid.Columns.AsEnumerable()) {
                if (MonospacedColumns.Contains(col.Name)) {
                    var s = new DataGridViewCellStyle();
                    s.Font = new Font(new FontFamily("Consolas"), _Grid.Font.Size);
                    col.DefaultCellStyle = s;
                }
                if (LeftAlignedColumns.Contains(col.Name)) {
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                } else {
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        void _GridMenuCopy(object sender, EventArgs e) {
            try {
                Clipboard.SetDataObject(_Grid.GetClipboardContent());
            } catch { }
        }

        protected ToolStripMenuItem GridMenuAddItem(string text) {
            var mi = new ToolStripMenuItem(text);
            _GridMenu.Items.Add(mi);
            return mi;
        }

        public IEnumerable<(T data, string property)> GetSelectedDataAndProperty() {
            var indices = _Grid.SelectedCells.AsEnumerable().Select(c => (c.OwningRow.DataBoundItem as T, c.OwningColumn.DataPropertyName)).ToArray();
            return indices;
        }

        public IEnumerable<T> GetSelectedData() {
            var indices = _Grid.SelectedCells.AsEnumerable().Select(c => c.OwningRow.DataBoundItem as T).ToArray();
            return indices;
        }

        int NextRowIndex(int index) {
            int n = _Grid.Rows.Count;
            return (index + 1) % n;
        }

        int PreviousRowIndex(int index) {
            int n = _Grid.Rows.Count;
            return (index - 1 + n) % n;
        }

        void UpdateSelection(int rowIndex) {
            _Grid.ClearSelection();
            _Grid.Rows[rowIndex].Cells[IndexColumn].Selected = true;
        }

        void UpdateScroll(int rowIndex) {
            int c = _Grid.Rows.Count, i = rowIndex;
            int d = 0;
            for (int r = 0; r < _Grid.Rows.Count; r++) if (_Grid.Rows[r].Displayed) d++;
            int f = _Grid.FirstDisplayedCell.RowIndex;
            if (i < f + d / 3)
                _Grid.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIndex - 2 * d / 3);
            else if (i > f + 2 * d / 3)
                _Grid.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIndex - d / 3);
        }

        void GridKeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = true;
            int r = -1;
            switch (e.KeyChar) {
                case 'n':
                    if (_Grid.SelectedCells.Count == 0)
                        r = 0;
                    else
                        r = NextRowIndex(_Grid.SelectedCells[_Grid.SelectedCells.Count - 1].RowIndex);
                    UpdateSelection(r);
                    UpdateScroll(r);
                    break;
                case 'p':
                    if (_Grid.SelectedCells.Count == 0)
                        r = _Grid.Rows.Count - 1;
                    else
                        r = PreviousRowIndex(_Grid.SelectedCells[0].RowIndex);
                    UpdateSelection(r);
                    UpdateScroll(r);
                    break;
                default:
                    e.Handled = false;
                    break;
            }
        }

    }
}
