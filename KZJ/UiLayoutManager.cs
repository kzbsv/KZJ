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
    public partial class UiLayoutManager {

        private Form _MainForm;

        public Form MainForm {
            get { return _MainForm; }
            set { _MainForm = value; }
        }
        
        private List<TabPage> _AllTabs;

        public List<TabPage> AllTabs {
            get { return _AllTabs; }
            set { _AllTabs = value; }
        }

        private string _Name;
        /// <summary>
        /// Name of the active layout.
        /// </summary>
        public string Name {
            get { return _Name; }
            set { _Name = value; }
        }

        XmlUiLayoutRoot _ActiveLayout;
        public XmlUiLayoutRoot ActiveLayout { get { return _ActiveLayout; } }

        private Control _Root;
        public Control Root {
            get { return _Root; }
            set { _Root = value; }
        }

        private Button _ButtonTemplate;

        public Button ButtonTemplate {
            get { return _ButtonTemplate; }
            set { _ButtonTemplate = value; }
        }

        private TabControl _TabsTemplate;

        public TabControl TabsTemplate {
            get { return _TabsTemplate; }
            set { _TabsTemplate = value; }
        }

        private Dictionary<string, XmlUiLayoutRoot> _Layouts;

        public IEnumerable<XmlUiLayoutRoot> Layouts {
            get {
                return _Layouts.Values.AsEnumerable();
            }
            set {
                _Layouts = new Dictionary<string, XmlUiLayoutRoot>();
                foreach (var l in value) _Layouts.Add(l.Name.ToLower(), l);
            }
        }

        /// <summary>
        /// Raised when layouts change due to UI manipulation.
        /// Not raised when Layouts are set.
        /// </summary>
        public event EventHandler<ValueEventArgs<IEnumerable<XmlUiLayoutRoot>>> LayoutsChanged;

        void OnLayoutsChanged() {
            if (LayoutsChanged != null)
                LayoutsChanged(this, new ValueEventArgs<IEnumerable<XmlUiLayoutRoot>>(_Layouts.Values.AsEnumerable()));
        }

        ButtonMenuForm _ButtonMenuForm;
        ContextMenuStrip _ButtonMenu;

        public UiLayoutManager() {
            _ButtonMenuForm = new ButtonMenuForm();
            _ButtonMenuForm.SetButtonMenuEventHandlers(this);
            _ButtonMenu = _ButtonMenuForm.ButtonMenu;
            ContextMenuEnabled = true;
        }

        /// <summary>
        /// Returns a new empty UI layout with only the MainForm's window size, location, and state.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        XmlUiLayoutRoot CreateEmptyUiLayoutRoot(string name) {
            var r = new XmlUiLayoutRoot();
            r.Name = name;
            r.WindowState = _MainForm.WindowState;
            r.ClientSize = _MainForm.ClientSize;
            r.Location = _MainForm.Location;
            r.Layout = new XmlUiLayout {
                SelectedIndex = 0,
                HideMvcButton = false,
                HideTabs = false,
                Tabs = new List<string>(),
            };
            return r;
        }

        public void VerifyLayout(string name) {
            if (!_Layouts.ContainsKey(name.ToLower())) {
                _Layouts.Add(name.ToLower(), CreateEmptyUiLayoutRoot(name));
                OnLayoutsChanged();
            }
        }

        Stack<string> _LayoutNameStack = new Stack<string>();
        
        public void PushLayout(string name, bool verify = true) {
            _LayoutNameStack.Push(_Name);
            ActivateLayout(name, verify);
        }

        public void PopLayout() {
            var name = _LayoutNameStack.Pop();
            ActivateLayout(name, false);
        }

        class OkCancelStackItem
        {
            public string Title { get; set; }
            public string Prompt { get; set; }
            public EventHandler<ValueEventArgs<DialogResult>> DialogResult { get; set; }
        }

        Stack<OkCancelStackItem> _OkCancelStack = new Stack<OkCancelStackItem>();

        OkCancelControl _OkCancel;
        public void SetOkCancel(OkCancelControl okCancel) { _OkCancel = okCancel; }

        public void PushOkCancel(string layout, EventHandler<ValueEventArgs<DialogResult>> dialogResult, string title, string prompt)
        {
            if (_OkCancelStack.Count > 0) {
                var si = _OkCancelStack.Peek();
                _OkCancel.DialogResult -= si.DialogResult;
            }
            var nsi = new OkCancelStackItem { DialogResult = dialogResult, Prompt = prompt, Title = title };
            _OkCancelStack.Push(nsi);
            _OkCancel.DialogResult += nsi.DialogResult;
            _OkCancel.Title = nsi.Title;
            _OkCancel.Prompt = nsi.Prompt;
            PushLayout(layout);
        }

        public void PopOkCancel()
        {
            var csi = _OkCancelStack.Pop();
            _OkCancel.DialogResult -= csi.DialogResult;
            PopLayout();
            if (_OkCancelStack.Count > 0) {
                var si = _OkCancelStack.Peek();
                _OkCancel.DialogResult += si.DialogResult;
                _OkCancel.Title = si.Title;
                _OkCancel.Prompt = si.Prompt;
            }
        }

        bool _Debug = false;
        string _DebugLevel = "";
        List<Action> _FinishLayout = null;

        void _MainForm_Load(object sender, EventArgs e) {
            _MainForm.Location = _ActiveLayout.Location;
            _MainForm.WindowState = _ActiveLayout.WindowState;
            _FinishLayout?.ForEach(fa => fa());
        }

        public void UpdateActiveLayout() {
            ExtractLayout(_ActiveLayout);
            _Layouts[_ActiveLayout.Name.ToLower()] = _ActiveLayout;
            OnLayoutsChanged();
        }

        public void ExtractAsNewLayoutAndSetActive(string newLayoutName) {
            var r = new XmlUiLayoutRoot();
            r.Name = newLayoutName;
            ExtractLayout(r);
            _Layouts.Add(newLayoutName.ToLower(), r);
            _ActiveLayout = r;
            _Name = r.Name;
            OnLayoutsChanged();
        }

        void ExtractLayout(XmlUiLayoutRoot r) {
            r.UseSuspendLayout = false;
            r.WindowState = _MainForm.WindowState;
            r.ClientSize = _MainForm.ClientSize;
            r.Location = _MainForm.Location;
            r.IsFullScreen = _MainForm.FormBorderStyle == FormBorderStyle.None;
            r.Layout = ExtractLayout(_Root);
            if (!_Root.Visible) r.Layout.HideMvcButton = false;
        }

        XmlUiLayout ExtractLayout(Control c) {
            var x = new XmlUiLayout();

            var tabs = c.Controls.AsEnumerable().SingleOrDefault(sod => sod is TabControl) as TabControl;
            var button = c.Controls.AsEnumerable().SingleOrDefault(sod => sod is Button) as Button;
            var splitter = c.Controls.AsEnumerable().SingleOrDefault(sod => sod is SplitContainer) as SplitContainer;
            if (tabs == null && splitter == null || tabs != null && splitter != null)
                throw new InvalidOperationException("UI layout levels must be either a TabControl or a SplitContainer");

            if (tabs != null) {
                x.SelectedIndex = tabs.SelectedIndex;
                x.Size = tabs.Size;
                x.HideMvcButton =
                    button != null
                    && button.BackgroundImage == ButtonTemplate.BackgroundImage
                    && !button.Visible;
                x.HideTabs = (tabs is UiLayoutTabControl) && !(tabs as UiLayoutTabControl).TabsVisible;
                x.IsCollapsed = (tabs is UiLayoutTabControl) && !(tabs as UiLayoutTabControl).Visible;
                x.Tabs = tabs.TabPages.AsEnumerable().Select(tp => tp.Name).ToList();
                x.Sizes = tabs.TabPages.AsEnumerable().Select(tp => tp.Size).ToList();
                foreach (var lt in tabs.TabPages.AsEnumerable().Where(lq => lq is LayoutTabPage)) {
                    var i = x.Tabs.Findi(lq => lq == lt.Name);
                    if (i >= 0) {
                        var name = x.Tabs[i];
                        x.Tabs[i] = _LayoutTabPageTypeName + x.Tabs[i];
                        var tpl = ExtractLayout(lt.Controls[0]);
                        tpl.TabName = name;
                        x.LayoutTabs.Add(tpl);
                    }
                }
            }

            if (splitter != null) {
                x.Size = splitter.Size;
                x.Sizes = new[] { splitter.Panel1.ClientSize, splitter.Panel2.ClientSize }.ToList();
                x.Split = new XmlUiSplitter {
                    Orientation = splitter.Orientation,
                    SplitterDistance = splitter.SplitterDistance,
                    IsSplitterFixed = splitter.IsSplitterFixed,
                    FixedPanel = splitter.FixedPanel,
                    Panel1 = ExtractLayout(splitter.Panel1),
                    Panel2 = ExtractLayout(splitter.Panel2)
                };
            }
            return x;
        }

        Button CreateNewButton(Button template, Control parent) {
            var newButton = new Button();
            newButton.FlatStyle = template.FlatStyle;
            newButton.Font = template.Font;
            newButton.Margin = template.Margin;
            newButton.TextAlign = template.TextAlign;
            newButton.UseVisualStyleBackColor = template.UseVisualStyleBackColor;
            newButton.Image = template.Image;
            newButton.ContextMenuStrip = _ButtonMenu;
            newButton.Size = template.Size;
            newButton.ForeColor = template.ForeColor;
            newButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            newButton.Location = new Point(parent.Width - template.Width - 3, 3);
            newButton.Click += Button_Click;
            return newButton;
        }

        UiLayoutTabControl CreateNewTabControl(TabControl template) {
            var newTabControl = new UiLayoutTabControl();
            newTabControl.Dock = DockStyle.Fill;
            newTabControl.AllowDrop = true;
            newTabControl.Appearance = template.Appearance;
            newTabControl.Multiline = true;
            newTabControl.DragDrop += new System.Windows.Forms.DragEventHandler(this.tabControl_DragDrop);
            newTabControl.DragEnter += new System.Windows.Forms.DragEventHandler(this.tabControl_DragEnter);
            newTabControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabControl_MouseDown);
            newTabControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tabControl_MouseMove);
            newTabControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tabControl_MouseUp);
            return newTabControl;
        }

        LayoutTabPage CreateNewLayoutTabPage(string name, Button buttonTemplate = null, TabControl tabsTemplate = null) {
            var tp = new LayoutTabPage { Text = name, Name = name };
            var p = new Panel { Dock = DockStyle.Fill };
            if (buttonTemplate != null) {
                var newButton = CreateNewButton(buttonTemplate, p);
                p.Controls.Add(newButton);
            }
            if (tabsTemplate != null) {
                var newTabControl = CreateNewTabControl(tabsTemplate);
                p.Controls.Add(newTabControl);
            }
            tp.Controls.Add(p);
            return tp;
        }

        const string _LayoutTabPageTypeName = "LayoutTabPage:";

        bool _IsMaximized;
        public bool IsMaximized {
            get { return _IsMaximized; }
        }

        Control[] _MaximizedOriginalChildren;
        Control _MaximizedSourceControl;

        void Button_Click(object sender, EventArgs e) {
            var button = sender as Button;
            if (button == null) return;
            _MainForm.SuspendLayout();
            if (!_IsMaximized) {
                _MaximizedSourceControl = button.Parent;
                if (_MaximizedSourceControl != null && _MaximizedSourceControl != _Root) {
                    _MaximizedOriginalChildren = _Root.Controls.AsEnumerable().ToArray();
                    _Root.Controls.Clear();
                    _Root.Controls.AddRange(_MaximizedSourceControl.Controls.AsEnumerable().ToArray());
                    button.Location = new Point(button.Parent.Width - 21, 3);
                    _IsMaximized = true;
                }
            } else if (_IsMaximized) {
                var controls = _Root.Controls.AsEnumerable().ToArray();
                _Root.Controls.Clear();
                _Root.Controls.AddRange(_MaximizedOriginalChildren);
                _MaximizedSourceControl.Controls.Clear();
                _MaximizedSourceControl.Controls.AddRange(controls);
                button.Location = new Point(button.Parent.Width - 21, 3);
                _IsMaximized = false;
                _MaximizedOriginalChildren = null;
            }
            _MainForm.ResumeLayout(true);
        }

        private void SplitterPaint(object sender, PaintEventArgs e) {
            SplitContainer s = sender as SplitContainer;
            if (s != null) {
                var inset = 0;
                if (s.Orientation == Orientation.Vertical) {
                    int top = inset;
                    int bottom = s.Height - inset;
                    int left = s.SplitterDistance;
                    int right = left + s.SplitterWidth - 1;
                    e.Graphics.DrawLine(Pens.Silver, left, top, left, bottom);
                    e.Graphics.DrawLine(Pens.Silver, right, top, right, bottom);
                } else {
                    int left = inset;
                    int right = s.Width - inset;
                    int top = s.SplitterDistance;
                    int bottom = top + s.SplitterWidth - 1;
                    e.Graphics.DrawLine(Pens.Silver, left, top, right, top);
                    e.Graphics.DrawLine(Pens.Silver, left, bottom, right, bottom);
                }
            }
        }

        private void splitterPanel_DragDrop(object sender, DragEventArgs e) {
            tabControl_DragDrop((sender as SplitterPanel).Controls[1], e);
        }

        private void splitterPanel_DragEnter(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.Move;
        }

        TabPage GetTabPageByLocation(Point screenLocation) {
            var tp1 = (TabPage)null;
            foreach (var tp in _AllTabs) {
                var tabControl = tp.Parent as TabControl;
                if (tabControl != null && tabControl.Visible && tabControl.Parent != null) {
                    var i = tabControl.TabPages.IndexOf(tp);
                    var c = tabControl.GetTabRect(i);
                    var s = tabControl.RectangleToScreen(c);
                    if (s.Contains(screenLocation)) {
                        tp1 = tp;
                        break;
                    }
                }
            }
            return tp1;
        }

        class TabPageDrag {
            public Point Start;
            public TabPage Page;
        }

        TabPageDrag tpd = null;

        private void tabControl_MouseDown(object sender, MouseEventArgs e) {
            var tp = GetTabPageByLocation((sender as Control).PointToScreen(e.Location));
            if (tp != null) {
                tpd = new TabPageDrag { Page = tp, Start = e.Location };
                //Console.WriteLine("MouseDown on tab {0}", tpd.Page.Name);
            }
        }

        private void tabControl_MouseMove(object sender, MouseEventArgs e) {
            if (tpd != null && (Math.Abs(tpd.Start.X - e.X) > 5 || Math.Abs(tpd.Start.Y - e.Y) > 5)) {
                //Console.WriteLine("DoDragDrop " + tpd.Page.Name);
                (sender as TabControl).DoDragDrop(tpd.Page, DragDropEffects.Move);
                tpd = null;
            }
        }

        private void tabControl_MouseUp(object sender, MouseEventArgs e) {
            tpd = null;
        }

        private void tabControl_DragEnter(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.Move;
        }

        private void tabControl_DragDrop(object sender, DragEventArgs e) {
            var targetTabPage = GetTabPageByLocation(new Point(e.X, e.Y));
            //if (targetTabPage != null) Console.WriteLine("targetTabPage is {0}", targetTabPage.Name);
            if (e.Data.GetDataPresent(typeof(TabPage))) {
                var movingTabPage = e.Data.GetData(typeof(TabPage)) as TabPage;
                //Console.WriteLine("Moving tab {0} ", movingTabPage.Name);
                if (targetTabPage == movingTabPage) return;
                var srcTabControl = movingTabPage.Parent as TabControl;
                var dstTabControl = sender as TabControl;
                if (targetTabPage == null) {
                    //Console.WriteLine("Moving tab {0} to last place", movingTabPage.Name);
                    srcTabControl.TabPages.Remove(movingTabPage);
                    dstTabControl.TabPages.Add(movingTabPage);
                } else {
                    var dstIndex = dstTabControl.TabPages.IndexOf(targetTabPage);
                    if (srcTabControl == dstTabControl) {
                        var srcIndex = dstTabControl.TabPages.IndexOf(movingTabPage);
                        //Console.WriteLine("Moving tab {0} to {1} ({2} to {3})", srcIndex, dstIndex, movingTabPage.Name, targetTabPage.Name);
                        if (dstIndex > srcIndex) dstIndex--;
                    } else {
                        //Console.WriteLine("Moving tab to {0} ({1} to {2})", dstIndex, movingTabPage.Name, targetTabPage.Name);
                    }
                    srcTabControl.TabPages.Remove(movingTabPage);
                    var tempTabs = new List<TabPage>();
                    while (dstTabControl.TabPages.Count > dstIndex) {
                        tempTabs.Add(dstTabControl.TabPages[dstIndex] as TabPage);
                        dstTabControl.TabPages.RemoveAt(dstIndex);
                    }
                    dstTabControl.TabPages.Add(movingTabPage);
                    dstTabControl.TabPages.AddRange(tempTabs.ToArray());
                }
                dstTabControl.SelectTab(movingTabPage);
                UpdateActiveLayout();
            }
        }

        public class LayoutTabPage : TabPage {
        }

        internal void addLayoutTabToolStripMenuItem(KZJ.ButtonMenuForm.MenuClickContext mcc, string tabName) {
            if (mcc.Tabs == null) return;
            var tp = CreateNewLayoutTabPage(tabName, mcc.Button, mcc.Tabs);
            mcc.Tabs.Controls.Add(tp);
            mcc.Tabs.SelectedTab = tp;
        }

        internal void splitToolStripMenuItem(object sender, Orientation orientation) {
            var menuItem = sender as ToolStripMenuItem;
            var parent = (menuItem.GetCurrentParent() as ContextMenuStrip).SourceControl;
            while (parent != null && !(parent is SplitterPanel || parent is Panel)) parent = parent.Parent;
            if (parent == null) return;
            var splitterPanel = parent as Control;
            var button = splitterPanel.Controls[0] as Button;
            var tabControl = splitterPanel.Controls[1] as TabControl;
            splitterPanel.Controls.Clear();
            var splitter = new SplitContainer();
            splitterPanel.Controls.Add(splitter);
            splitter.Dock = DockStyle.Fill;
            splitter.Orientation = orientation;
            splitter.SplitterDistance = (orientation == Orientation.Horizontal ? splitter.Size.Height : splitter.Size.Width) / 2;
            splitter.Paint += SplitterPaint;
            splitter.Panel1.Controls.Add(button);
            splitter.Panel1.Controls.Add(tabControl);
            var newTabControl = CreateNewTabControl(tabControl);
            splitterPanel.DragDrop -= splitterPanel_DragDrop;
            splitterPanel.DragEnter -= splitterPanel_DragEnter;
            splitterPanel.AllowDrop = false;
            splitter.Panel1.DragDrop += splitterPanel_DragDrop;
            splitter.Panel1.DragEnter += splitterPanel_DragEnter;
            splitter.Panel1.AllowDrop = true;
            splitter.Panel2.DragDrop += splitterPanel_DragDrop;
            splitter.Panel2.DragEnter += splitterPanel_DragEnter;
            splitter.Panel2.AllowDrop = true;
            var newButton = CreateNewButton(button, splitter.Panel2);
            button.Location =  new Point(splitter.Panel1.Width - 21, 3);
            splitter.Panel2.Controls.Add(newButton);
            splitter.Panel2.Controls.Add(newTabControl);
            UpdateActiveLayout();
        }


        public void DeleteLayout(string layoutName) {
            if (_Layouts.ContainsKey(layoutName.ToLower())) {
                _Layouts.Remove(layoutName.ToLower());
                bool needChanged = true;
                if (_Name.ToLower() == layoutName.ToLower()) {
                    var newLayoutName = "Default";
                    needChanged = !_Layouts.ContainsKey(newLayoutName.ToLower());
                    ActivateLayout(newLayoutName);
                }
                if (needChanged)
                    OnLayoutsChanged();
            }
        }

        bool _ContextMenuEnabled;
        public bool ContextMenuEnabled { get { return _ContextMenuEnabled; }
            set {
                _ContextMenuEnabled = value;
                _ButtonMenu.Enabled = value;
            }
        }

        public event EventHandler ReplaceUiFromFileRequested;

        internal void ReplaceUiFromRequested() {
            if (ReplaceUiFromFileRequested != null)
                ReplaceUiFromFileRequested(this, new EventArgs());
        }
    }

}
