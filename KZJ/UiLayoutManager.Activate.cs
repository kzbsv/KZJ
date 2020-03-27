#region Copyright
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
    partial class UiLayoutManager {

        public void ActivateLayout(string name, bool verify = true) {
            if (verify) VerifyLayout(name);
            ActivateLayoutRoot(_Layouts[name.ToLower()]);
        }

        void ActivateLayoutRoot(XmlUiLayoutRoot r) {
            if (_ActiveLayout != null &&
                _MainForm.WindowState == r.WindowState &&
                _MainForm.ClientSize == r.ClientSize &&
                _MainForm.Location == r.Location) {
                // If we're already displaying a layout in _MainForm and _MainForm's geometry is still what we want,
                // keep things a bit simpler.
                var currentLayout = _ActiveLayout.Layout;
                _ActiveLayout = r;
                _Name = r.Name;
                if (_Debug) Console.WriteLine("UIM ActivateLayout {0}", r.Name);
                ChangeLayout(currentLayout, r.Layout, _Root);
            } else {
                // From scratch, fully configure the _MainForm geometry and full layout.
                _ActiveLayout = r;
                _Name = r.Name;
                _MainForm.WindowState = FormWindowState.Normal;
                _MainForm.ClientSize = r.ClientSize;
                _FinishLayout = new List<Action>();
                CreateLayout(r.Layout, _Root, _FinishLayout);
                var oloc = _MainForm.Location;
                _MainForm.Location = r.Location;
                var loc = _MainForm.ClientRectangle.Location;
                var ok = false;
                foreach (var screen in Screen.AllScreens) {
                    if (screen.Bounds.Contains(loc)) {
                        ok = true;
                        break;
                    }
                }
                if (!ok) _MainForm.Location = oloc;
                _MainForm.FormBorderStyle = (_ActiveLayout.IsFullScreen) ? FormBorderStyle.None : FormBorderStyle.Sizable;
                if (_Root.Visible) {
                    _MainForm.WindowState = r.WindowState;
                    _MainForm.Load -= _MainForm_Load;
                    _FinishLayout?.ForEach(fa => fa());
                } else {
                    _MainForm.Load += _MainForm_Load;
                    _MainForm.VisibleChanged += _MainForm_VisibleChanged;
                }
            }
        }

        void _MainForm_VisibleChanged(object sender, EventArgs e) {
            if (_MainForm.Visible)
                _FinishLayout?.ForEach(fa => fa());
        }

        void CreateLayout(XmlUiLayout x, Control c, List<Action> finishLayout) {
            if (_Debug) _DebugLevel += "  ";
            try {
                c.Controls.Clear();
                if (x.Split == null) {
                    var tabControl = _TabsTemplate;
                    var newTabControl = CreateNewTabControl(tabControl);
                    newTabControl.TabsVisible = !x.HideTabs;
                    c.AllowDrop = true;
                    c.DragDrop += splitterPanel_DragDrop;
                    c.DragEnter += splitterPanel_DragEnter;
                    var button = _ButtonTemplate;
                    var newButton = CreateNewButton(button, c);
                    newButton.Visible = !x.HideMvcButton;
                    c.Controls.Add(newButton);
                    c.Controls.Add(newTabControl);
                    var a = c.Controls.GetChildIndex(newButton);
                    var b = c.Controls.GetChildIndex(newTabControl);
                    var i = -1;
                    foreach (var tabName in x.Tabs) {
                        var tp = (TabPage)null;
                        i++;
                        if (tabName.StartsWith(_LayoutTabPageTypeName)) {
                            var tn = tabName.Substring(_LayoutTabPageTypeName.Length);
                            var tl = x.LayoutTabs.FirstOrDefault(lq => lq.TabName == tn);
                            if (tl != null) {
                                tp = CreateNewLayoutTabPage(tn);
                                if (tp != null) {
                                    newTabControl.TabPages.Add(tp);
                                    if (_Debug) Console.WriteLine("UIM {1}LayoutTabPage {0}", tn, _DebugLevel);
                                    CreateLayout(tl, tp.Controls[0], finishLayout);
                                } else {
                                    if (_Debug) Console.WriteLine("UIM {0}LayoutTabPage ERROR tp == null", _DebugLevel);
                                }
                            } else {
                                if (_Debug) Console.WriteLine("UIM {0}LayoutTabPage ERROR tl == null", _DebugLevel);
                            }
                        } else {
                            tp = _AllTabs.SingleOrDefault(sod => sod.Name == tabName);
                            if (tp != null) {
                                if (_Debug) Console.WriteLine("UIM {1}TabPage tabName={0}", tabName, _DebugLevel);
                                newTabControl.TabPages.Add(tp);
                            } else {
                                if (_Debug) Console.WriteLine("UIM {1}TabPage ERROR tabName={0} not found.", tabName, _DebugLevel);
                            }
                        }
                    }
                    newTabControl.SelectedIndex = x.SelectedIndex;
                }
                if (x.Split != null) {
                    var splitter = new SplitContainer();
                    splitter.Orientation = x.Split.Orientation;
                    c.Controls.Add(splitter);
                    if (splitter.SplitterDistance < splitter.Panel1MinSize || splitter.SplitterDistance > splitter.Width - splitter.Panel2MinSize) {
                        var w = splitter.Width;
                    }
                    try {
                        splitter.Dock = DockStyle.Fill;
                    } catch (Exception ex) {
                        if (_Debug) Console.WriteLine("UIM {2}{0}Splitter Dock EXCEPTION={1}", (x.Split.Orientation == Orientation.Horizontal ? "H" : "V"), ex.LastInnerMessage(), _DebugLevel);
                        splitter.Dock = DockStyle.None;
                    }
                    splitter.IsSplitterFixed = x.Split.IsSplitterFixed;
                    splitter.FixedPanel = x.Split.FixedPanel;
                    splitter.Paint += SplitterPaint;
                    if (_Debug) Console.WriteLine("UIM {2}{0}Splitter Distance={1}", (x.Split.Orientation == Orientation.Horizontal ? "H" : "V"), x.Split.SplitterDistance, _DebugLevel);
                    finishLayout?.Add(() => { splitter.SplitterDistance = x.Split.SplitterDistance; });
                    if (_Debug) Console.WriteLine("UIM {0}  Panel1", _DebugLevel);
                    CreateLayout(x.Split.Panel1, splitter.Panel1, finishLayout);
                    if (_Debug) Console.WriteLine("UIM {0}  Panel2", _DebugLevel);
                    CreateLayout(x.Split.Panel2, splitter.Panel2, finishLayout);
                }
            } finally {
                if (_Debug) _DebugLevel = _DebugLevel.Substring(2);
            }
        }

        void ChangeLayout(XmlUiLayout x0, XmlUiLayout x1, Control c) {
            if (!NodeEquals(x0, x1)) {
                CreateLayout(x1, c, null);
            }
        }

        bool NodeEquals(XmlUiLayout x0, XmlUiLayout x1) {
            var e = x0.HideMvcButton == x1.HideMvcButton &&
                x0.HideTabs == x1.HideTabs &&
                x0.IsCollapsed == x1.IsCollapsed &&
                x0.SelectedIndex == x1.SelectedIndex &&
                (x0.Split != null) == (x1.Split != null) &&
                (x0.Tabs != null) == (x1.Tabs != null);
            if (e && x0.Tabs != null) {
                if (x0.Tabs.Count != x1.Tabs.Count)
                    e = false;
                else {
                    foreach (var t in x0.Tabs.Zip(x1.Tabs, (a, b) => new { a = a, b = b })) {
                        var aIsTp = t.a.StartsWith(_LayoutTabPageTypeName);
                        var bIsTp = t.b.StartsWith(_LayoutTabPageTypeName);
                        if (aIsTp != bIsTp) {
                            e = false;
                            break;
                        }
                        if (aIsTp) {
                            var atpl = x0.LayoutTabs.SingleOrDefault(lq => lq.TabName == t.a.Substring(_LayoutTabPageTypeName.Length));
                            var btpl = x1.LayoutTabs.SingleOrDefault(lq => lq.TabName == t.b.Substring(_LayoutTabPageTypeName.Length));
                            if (!NodeEquals(atpl, btpl)) {
                                e = false;
                                break;
                            }
                        } else {
                            if (t.a != t.b) {
                                e = false;
                                break;
                            }
                        }
                    }
                }
            }
            if (e && x0.Split != null) {
                e = x0.Split.FixedPanel == x1.Split.FixedPanel &&
                    x0.Split.IsSplitterFixed == x1.Split.IsSplitterFixed &&
                    x0.Split.Orientation == x1.Split.Orientation &&
                    x0.Split.SplitterDistance == x1.Split.SplitterDistance &&
                    NodeEquals(x0.Split.Panel1, x1.Split.Panel1) &&
                    NodeEquals(x0.Split.Panel2, x1.Split.Panel2);
            }
            return e;
        }

        void Suspend(Control c, List<Control> suspended) {
            if (suspended == null) return;
            c.SuspendLayout();
            if (c is SplitContainer) ((ISupportInitialize)c).BeginInit();
            suspended.Add(c);
        }

        void ActivateLayoutWithSuspend(XmlUiLayout x0, XmlUiLayout x1, Control c, List<Control> suspended) {
            if (!NodeEquals(x0, x1)) {
                ActivateLayoutWithSuspend(x1, c, suspended);
            }
        }

        void ActivateLayoutWithSuspend(XmlUiLayout x, Control c, List<Control> suspended) {
            if (_Debug) _DebugLevel += "  ";
            try {
                c.Controls.Clear();
                if (x.Split == null) {
                    var tabControl = _TabsTemplate;
                    var newTabControl = CreateNewTabControl(tabControl);
                    Suspend(newTabControl, suspended);
                    if (suspended != null)
                        newTabControl.Size = x.Size;
                    newTabControl.TabsVisible = !x.HideTabs;
                    c.AllowDrop = true;
                    c.DragDrop += splitterPanel_DragDrop;
                    c.DragEnter += splitterPanel_DragEnter;
                    var button = _ButtonTemplate;
                    var newButton = CreateNewButton(button, c);
                    newButton.Visible = !x.HideMvcButton;
                    c.Controls.Add(newButton);
                    c.Controls.Add(newTabControl);
                    var a = c.Controls.GetChildIndex(newButton);
                    var b = c.Controls.GetChildIndex(newTabControl);
                    var i = -1;
                    foreach (var tabName in x.Tabs) {
                        var tp = (TabPage)null;
                        i++;
                        var size = suspended != null ? x.Sizes[i] : new Size();
                        if (tabName.StartsWith(_LayoutTabPageTypeName)) {
                            var tn = tabName.Substring(_LayoutTabPageTypeName.Length);
                            var tl = x.LayoutTabs.FirstOrDefault(lq => lq.TabName == tn);
                            if (tl != null) {
                                tp = CreateNewLayoutTabPage(tn);
                                if (tp != null) {
                                    Suspend(tp, suspended);
                                    if (suspended != null) tp.Size = size;
                                    newTabControl.TabPages.Add(tp);
                                    if (_Debug) Console.WriteLine("UIM {1}LayoutTabPage {0}", tn, _DebugLevel);
                                    ActivateLayoutWithSuspend(tl, tp.Controls[0], suspended);
                                } else {
                                    if (_Debug) Console.WriteLine("UIM {0}LayoutTabPage ERROR tp == null", _DebugLevel);
                                }
                            } else {
                                if (_Debug) Console.WriteLine("UIM {0}LayoutTabPage ERROR tl == null", _DebugLevel);
                            }
                        } else {
                            tp = _AllTabs.SingleOrDefault(sod => sod.Name == tabName);
                            if (tp != null) {
                                if (_Debug) Console.WriteLine("UIM {1}TabPage tabName={0}", tabName, _DebugLevel);
                                Suspend(tp, suspended);
                                if (suspended != null) tp.Size = size;
                                newTabControl.TabPages.Add(tp);
                            } else {
                                if (_Debug) Console.WriteLine("UIM {1}TabPage ERROR tabName={0} not found.", tabName, _DebugLevel);
                            }
                        }
                    }
                    newTabControl.SelectedIndex = x.SelectedIndex;
                }
                if (x.Split != null) {
                    var splitter = new SplitContainer();
                    Suspend(splitter.Panel1, suspended);
                    Suspend(splitter.Panel2, suspended);
                    Suspend(splitter, suspended);
                    if (suspended != null) {
                        splitter.Size = x.Size;
                        splitter.Panel1.ClientSize = x.Sizes[0];
                        splitter.Panel2.ClientSize = x.Sizes[1];
                    }
                    splitter.Orientation = x.Split.Orientation;
                    c.Controls.Add(splitter);
                    if (splitter.SplitterDistance < splitter.Panel1MinSize || splitter.SplitterDistance > splitter.Width - splitter.Panel2MinSize) {
                        var w = splitter.Width;
                    }
                    {
                        try {
                            splitter.Dock = DockStyle.Fill;
                        } catch (Exception ex) {
                            if (_Debug) Console.WriteLine("UIM {2}{0}Splitter Dock EXCEPTION={1}", (x.Split.Orientation == Orientation.Horizontal ? "H" : "V"), ex.LastInnerMessage(), _DebugLevel);
                            splitter.Dock = DockStyle.None;
                        }
                        splitter.SplitterDistance = x.Split.SplitterDistance;
                    }
                    splitter.IsSplitterFixed = x.Split.IsSplitterFixed;
                    splitter.FixedPanel = x.Split.FixedPanel;
                    splitter.Paint += SplitterPaint;
                    if (_Debug) Console.WriteLine("UIM {2}{0}Splitter Distance={1}", (x.Split.Orientation == Orientation.Horizontal ? "H" : "V"), x.Split.SplitterDistance, _DebugLevel);
                    if (_Debug) Console.WriteLine("UIM {0}  Panel1", _DebugLevel);
                    ActivateLayoutWithSuspend(x.Split.Panel1, splitter.Panel1, suspended);
                    if (_Debug) Console.WriteLine("UIM {0}  Panel2", _DebugLevel);
                    ActivateLayoutWithSuspend(x.Split.Panel2, splitter.Panel2, suspended);
                    splitter.SplitterDistance = x.Split.SplitterDistance;
                }
            } finally {
                if (_Debug) _DebugLevel = _DebugLevel.Substring(2);
            }
        }

    }

}
