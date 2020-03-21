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
    public partial class ButtonMenuForm : Form {

        public ContextMenuStrip ButtonMenu { get { return _ButtonMenu; } }

        UiLayoutManager _M;
        int _UiLayoutFixedMenuItemsCount;
        int _UiLayoutFixedLayoutMenuItemsCount;

        public void SetButtonMenuEventHandlers(UiLayoutManager m) {
            _M = m;

            _UiLayoutFixedMenuItemsCount = _ButtonMenu.Items.Count;
            _UiLayoutFixedLayoutMenuItemsCount = layoutsToolStripMenuItem.DropDownItems.Count;

            _ButtonMenu.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(menuMvc_Closing);
            _ButtonMenu.Opening += new System.ComponentModel.CancelEventHandler(menuMvc_Opening);
            layoutsToolStripMenuItem.DropDownOpening += new System.EventHandler(layoutsToolStripMenuItem_DropDownOpening);
            newLayoutToolStripMenuItem.Click += newLayoutToolStripMenuItem_Click;
            layoutSaveToolStripMenuItem.Click += layoutSaveToolStripMenuItem_Click;
            layoutSaveAsToolStripMenuItem.Click += layoutSaveAsToolStripMenuItem_Click;
            splitHorizontalToolStripMenuItem.Click += splitHorizontalToolStripMenuItem_Click;
            splitVerticalToolStripMenuItem.Click += splitVerticalToolStripMenuItem_Click;
            removeTabsToolStripMenuItem.Click += removeTabsToolStripMenuItem_Click;
            hideMvcButtonToolStripMenuItem.Click += hideMvcButtonToolStripMenuItem_Click;
            hideOtherMvcButtonsToolStripMenuItem.Click += hideOtherMvcButtonsToolStripMenuItem_Click;
            showAllMvcButtonsToolStripMenuItem.Click += showAllMvcButtonsToolStripMenuItem_Click;
            hideTabsToolStripMenuItem.Click += hideTabsToolStripMenuItem_Click;
            fixedSizeTabToolStripMenuItem.Click += fixedSizeTabToolStripMenuItem_Click;
            tabsRemoveAllToolStripMenuItem.Click += tabsRemoveAllToolStripMenuItem_Click;
            deleteLayoutToolStripMenuItem.Click += deleteLayoutToolStripMenuItem_Click;
            replaceFromToolStripMenuItem.Click += replaceFromToolStripMenuItem_Click;
            fullScreenToolStripMenuItem.Click += fullScreenToolStripMenuItem_Click;
            addLayoutTabToolStripMenuItem.Click += addLayoutTabToolStripMenuItem_Click;
        }

        public ButtonMenuForm() {
            InitializeComponent();
        }

        bool skipOneAppFocusChange = false;

        internal void menuMvc_Closing(object sender, ToolStripDropDownClosingEventArgs e) {
            if (e.CloseReason == ToolStripDropDownCloseReason.AppFocusChange
                && skipOneAppFocusChange) {
                e.Cancel = true;
                skipOneAppFocusChange = false;
            } else if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked) {
                e.Cancel = true;
            } else {
                skipOneAppFocusChange = false;
            }
        }

        Control _MvcMenuParent;
        Button _MvcMenuButton;
        UiLayoutTabControl _MvcMenuTabs;

        private void menuMvc_Opening(object sender, CancelEventArgs e) {
            if (!_M.ContextMenuEnabled) { e.Cancel = true; return; }
            _MvcMenuParent = null;
            _MvcMenuButton = null;
            _MvcMenuTabs = null;
            var parent = (sender as ContextMenuStrip).SourceControl;
            while (parent != null && !(parent is SplitterPanel || parent is Panel)) parent = parent.Parent;
            if (parent == null) { e.Cancel = true; return; }
            _MvcMenuParent = parent;
            _MvcMenuButton = parent.Controls[0] as Button;
            _MvcMenuTabs = parent.Controls[1] as UiLayoutTabControl;
            hideTabsToolStripMenuItem.Checked = !_MvcMenuTabs.TabsVisible;
            hideMvcButtonToolStripMenuItem.Enabled = parent != _M.Root;
            layoutSaveToolStripMenuItem.Enabled = !_M.IsMaximized;
            var sc = parent.Parent as SplitContainer;
            fixedSizeTabToolStripMenuItem.Enabled = sc != null
                && (sc.FixedPanel == FixedPanel.None
                || sc.FixedPanel == FixedPanel.Panel1 && sc.Panel1 == parent
                || sc.FixedPanel == FixedPanel.Panel2 && sc.Panel2 == parent);
            fixedSizeTabToolStripMenuItem.Checked = fixedSizeTabToolStripMenuItem.Enabled && sc.FixedPanel != FixedPanel.None;
            fullScreenToolStripMenuItem.Checked = _M.MainForm.FormBorderStyle == System.Windows.Forms.FormBorderStyle.None;

            var items = _ButtonMenu.Items;
            var c = _UiLayoutFixedMenuItemsCount;
            while (items.Count > c) items.RemoveAt(c);
            var tabs = _MvcMenuTabs;
            if (tabs != null) {
                items.AddRange(tabs.TabPages.AsEnumerable().Where(tp => !(tp is UiLayoutManager.LayoutTabPage)).Select(tp => {
                    var i = new ToolStripMenuItem { Text = tp.Name.Substring(3), Checked = true, Tag = tp };
                    i.Click += tabMenuItem_Click;
                    return i as ToolStripItem;
                }).OrderBy(tsi => tsi.Text).ToArray());
            }
            var inUse = new HashSet<TabPage>(
                _M.Root.AllChildControls().Where(acc => acc is UiLayoutTabControl)
                .SelectMany(acc => (acc as UiLayoutTabControl).TabPages.AsEnumerable()));
            items.AddRange(
                _M.AllTabs.Where(tp => !inUse.Contains(tp)).Select(tp => {
                    var menuText = (tp.Name?.Length ?? 0) > 3 ? tp.Name.Substring(3) : tp.Text;
                    var i = new ToolStripMenuItem { Text = menuText, Checked = false, Tag = tp };
                    i.Click += tabMenuItem_Click;
                    return i as ToolStripItem;
                }).OrderBy(tsi => tsi.Text).ToArray());
        }

        void tabMenuItem_Click(object sender, EventArgs e) {
            skipOneAppFocusChange = true;
            var tabs = _MvcMenuTabs;
            var i = sender as ToolStripMenuItem;
            var tp = i.Tag as TabPage;
            if (i.Checked)
                tabs.TabPages.Remove(tp);
            else {
                tabs.TabPages.Add(tp);
                tabs.SelectTab(tabs.TabPages.Count - 1);
            }
            i.Checked = !i.Checked;
        }

        void splitHorizontalToolStripMenuItem_Click(object sender, EventArgs e) {
            _M.splitToolStripMenuItem(sender, Orientation.Horizontal);
            _ButtonMenu.Close();
        }

        void splitVerticalToolStripMenuItem_Click(object sender, EventArgs e) {
            _M.splitToolStripMenuItem(sender, Orientation.Vertical);
            _ButtonMenu.Close();
        }

        public class MenuClickContext {
            public ToolStripMenuItem Item;
            public Control Parent;
            public Button Button;
            public TabControl Tabs;

            public MenuClickContext(object sender) {
                var menuItem = sender as ToolStripMenuItem;
                var parent = (menuItem.GetCurrentParent() as ContextMenuStrip).SourceControl;
                while (parent != null && !(parent is SplitterPanel || parent is Panel)) parent = parent.Parent;
                if (parent == null) return;
                Item = menuItem;
                Parent = parent as Control;
                Button = Parent.Controls[0] as Button;
                Tabs = Parent.Controls[1] as TabControl;
            }
        }

        void addLayoutTabToolStripMenuItem_Click(object sender, EventArgs e) {
            var mcc = new MenuClickContext(sender);
            if (mcc.Parent == null) return;
            var d = new NewTabLayoutForm();
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _M.addLayoutTabToolStripMenuItem(mcc, d.NewTabName);
            }
            _ButtonMenu.Close();
        }

        private void layoutsToolStripMenuItem_DropDownOpening(object sender, EventArgs e) {
            var items = layoutsToolStripMenuItem.DropDownItems;
            var c = _UiLayoutFixedLayoutMenuItemsCount;
            while (items.Count > c) items.RemoveAt(c);
            items.AddRange(
                _M.Layouts.Select(ui => {
                    var i = new ToolStripMenuItem { Text = ui.Name, Tag = ui, Checked = ui.Name.ToLower() == _M.Name.ToLower() };
                    i.Click += (s, oe) => { _M.ActivateLayout(ui.Name); };
                    return i as ToolStripItem;
                }).OrderBy(tsi => tsi.Text).ToArray());
        }

        private void newLayoutToolStripMenuItem_Click(object sender, EventArgs e) {
            var d = new LayoutSaveAsForm();
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                if (_M.Layouts.Any(uia => uia.Name.ToLower() == d.SaveAsName.ToLower())) {
                    MessageBox.Show("Name already in use: " + d.SaveAsName);
                } else {
                    _M.ActivateLayout(d.SaveAsName);
                }
            }
            _ButtonMenu.Close();
        }

        private void layoutSaveAsToolStripMenuItem_Click(object sender, EventArgs e) {
            var d = new LayoutSaveAsForm();
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                var save = true;
                if (_M.Layouts.Any(uia => uia.Name.ToLower() == d.SaveAsName.ToLower())) {
                    if (MessageBox.Show("Name already in use: " + d.SaveAsName + "\nReplace it?", "Verify Replace", MessageBoxButtons.OKCancel)
                        == System.Windows.Forms.DialogResult.OK) {
                        _M.DeleteLayout(d.SaveAsName);
                    } else
                        save = false;
                }
                if (save)
                    _M.ExtractAsNewLayoutAndSetActive(d.SaveAsName);
            }
            _ButtonMenu.Close();
        }

        private void layoutSaveToolStripMenuItem_Click(object sender, EventArgs e) {
            _M.UpdateActiveLayout();
            _ButtonMenu.Close();
        }


        private void replaceFromToolStripMenuItem_Click(object sender, EventArgs e) {
            _M.ReplaceUiFromRequested();
        }

        private void removeTabsToolStripMenuItem_Click(object sender, EventArgs e) {
            var tabs = _MvcMenuTabs;
            var panelA = _MvcMenuParent as SplitterPanel;
            if (tabs != null && panelA != null) {
                tabs.TabPages.Clear();
                var splitter = panelA.Parent as SplitContainer;
                var splitterParent = splitter.Parent;
                var panelB = splitter.Panel1 == panelA ? splitter.Panel2 : splitter.Panel1;
                splitterParent.Controls.Clear();
                while (panelB.Controls.Count > 0) {
                    var c = panelB.Controls[0];
                    panelB.Controls.RemoveAt(0);
                    splitterParent.Controls.Add(c);
                }
                if (splitterParent.Controls[0] is Button) {
                    (splitterParent.Controls[0] as Button).Location =  new Point(splitterParent.Width - 21, 3);
                }
                _M.UpdateActiveLayout();
            } else if (_MvcMenuParent != null && _MvcMenuParent.Parent is UiLayoutManager.LayoutTabPage) {
                var tp = _MvcMenuParent.Parent as UiLayoutManager.LayoutTabPage;
                var tabs2 = tp.Parent as TabControl;
                tabs2.TabPages.Remove(tp);
            }
            _ButtonMenu.Close();
        }

        private void tabsRemoveAllToolStripMenuItem_Click(object sender, EventArgs e) {
            var tabs = _MvcMenuTabs;
            if (tabs != null) {
                tabs.TabPages.Clear();
                _M.UpdateActiveLayout();
            }
            _ButtonMenu.Close();
        }

        private void hideMvcButtonToolStripMenuItem_Click(object sender, EventArgs e) {
            var button = _MvcMenuButton;
            if (button != null) {
                button.Visible = false;
                _M.UpdateActiveLayout();
            }
            _ButtonMenu.Close();
        }

        private void hideOtherMvcButtonsToolStripMenuItem_Click(object sender, EventArgs e) {
            var button = _MvcMenuButton;
            if (button != null) {
                foreach (var b in _M.Root.AllChildControls()
                    .Where(c => (c is Button) && (c != button && c.BackgroundImage == button.BackgroundImage)))
                    b.Visible = false;
                _M.UpdateActiveLayout();
            }
            _ButtonMenu.Close();
        }

        private void showAllMvcButtonsToolStripMenuItem_Click(object sender, EventArgs e) {
            var button = _MvcMenuButton;
            if (button != null) {
                foreach (var b in _M.Root.AllChildControls()
                    .Where(c => (c is Button) && (c.BackgroundImage == button.BackgroundImage)))
                    b.Visible = true;
                _M.UpdateActiveLayout();
            }
            _ButtonMenu.Close();
        }

        private void hideTabsToolStripMenuItem_Click(object sender, EventArgs e) {
            var tabs = _MvcMenuTabs;
            if (tabs != null) {
                tabs.TabsVisible = !tabs.TabsVisible;
            }
            _ButtonMenu.Close();
        }

        private void fixedSizeTabToolStripMenuItem_Click(object sender, EventArgs e) {
            var tabs = _MvcMenuTabs;
            var panelA = _MvcMenuParent as SplitterPanel;
            if (tabs != null && panelA != null) {
                var splitter = panelA.Parent as SplitContainer;
                if (splitter.FixedPanel == FixedPanel.None)
                    splitter.FixedPanel = panelA == splitter.Panel1 ? FixedPanel.Panel1 : FixedPanel.Panel2;
                else if (splitter.FixedPanel == FixedPanel.Panel1) {
                    if (panelA == splitter.Panel1) splitter.FixedPanel = FixedPanel.None;
                } else if (splitter.FixedPanel == FixedPanel.Panel2) {
                    if (panelA == splitter.Panel2) splitter.FixedPanel = FixedPanel.None;
                }
                _M.UpdateActiveLayout();
            }
            _ButtonMenu.Close();
        }

        private void deleteLayoutToolStripMenuItem_Click(object sender, EventArgs e) {
            var d = new LayoutConfirmDeleteForm();
            d.LayoutName = _M.Name;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _M.DeleteLayout(d.LayoutName);
            }
            _ButtonMenu.Close();
        }

        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e) {
            var mi = sender as ToolStripMenuItem;
            mi.Checked = !mi.Checked;
            if (!mi.Checked) {
                _M.MainForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            } else {
                _M.MainForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                _M.MainForm.WindowState = FormWindowState.Maximized;
            }
        }

        private void setWindowSize1280x1024_Click(object sender, EventArgs e) {
            _M.MainForm.Size = new Size(1280, 1024);
        }

        private void setClientSize1280x1024_Click(object sender, EventArgs e) {
            _M.MainForm.ClientSize = new Size(1280, 1024);
        }

    }
}
