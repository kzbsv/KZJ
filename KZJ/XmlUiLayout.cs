using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
//using KzL.Utility1;

namespace KzL.Windows.Forms {

    [Serializable]
    [XmlType("UiLayoutRoot")]
    public class XmlUiLayoutRoot : ICloneBySerialization {
        public override string ToString() {
            return "<UI Root " + Name + ">";
        }
        [XmlAttribute]
        public string Name { get; set; }
        public Size ClientSize { get; set; }
        public Point Location { get; set; }
        [XmlAttribute]
        public FormWindowState WindowState { get; set; }
        [XmlAttribute]
        public bool IsFullScreen { get; set; }
        [XmlAttribute]
        public bool UseSuspendLayout { get; set; }
        [Editor(typeof(KzPropertiesGridEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public XmlUiLayout Layout { get; set; }
        public XmlUiLayoutRoot() {
        }

        public void CopyRootGeometry(XmlUiLayoutRoot target) {
            target.ClientSize = ClientSize;
            target.IsFullScreen = IsFullScreen;
            target.Location = Location;
            target.WindowState = WindowState;
        }

        public XmlUiLayoutRoot CloneRootGeometry() {
            var t = new XmlUiLayoutRoot();
            CopyRootGeometry(t);
            return t;
        }

        public object CloneBySerialization() { return Clone(); }
        public XmlUiLayoutRoot Clone() {
            var x = new XmlSerializer(this.GetType());
            using (var s = new MemoryStream()) { x.Serialize(s, this); s.Position = 0; return x.Deserialize(s) as XmlUiLayoutRoot; }
        }

    }

    [Serializable]
    public class XmlUiLayout : ICloneBySerialization {
        public override string ToString() {
            return "<UI Layout>";
        }
        public List<string> Tabs { get; set; }
        [XmlAttribute]
        public int SelectedIndex { get; set; }
        [XmlAttribute]
        public bool HideTabs { get; set; }
        [XmlAttribute]
        public bool HideMvcButton { get; set; }
        [XmlAttribute]
        public bool IsCollapsed { get; set; }
        /// <summary>
        /// Has a value if this layout belongs to a LayoutTabPage.
        /// </summary>
        [XmlAttribute]
        public string TabName { get; set; }
        /// <summary>
        /// Size of control at this level
        /// </summary>
        public Size Size { get; set; }
        /// <summary>
        /// Size of non-layout child controls.
        /// If null, don't restore with SuspendLayout...
        /// </summary>
        public List<Size> Sizes { get; set; }

        /// <summary>
        /// Layout info for tabs that are LayoutTabPages.
        /// </summary>
        public List<XmlUiLayout> LayoutTabs { get; set; }
        [Editor(typeof(KzPropertiesGridEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public XmlUiSplitter Split { get; set; }
        public XmlUiLayout() {
            Tabs = new List<string>();
            LayoutTabs = new List<XmlUiLayout>();
        }

        public object CloneBySerialization() { return Clone(); }
        public XmlUiLayout Clone() {
            var x = new XmlSerializer(this.GetType());
            using (var s = new MemoryStream()) { x.Serialize(s, this); s.Position = 0; return x.Deserialize(s) as XmlUiLayout; }
        }

    }

    [Serializable]
    public class XmlUiSplitter : ICloneBySerialization {
        [XmlAttribute]
        public Orientation Orientation { get; set; }
        [XmlAttribute]
        public int SplitterDistance { get; set; }
        [XmlAttribute]
        public bool IsSplitterFixed { get; set; }
        [XmlAttribute]
        public FixedPanel FixedPanel { get; set; }
        public XmlUiLayout Panel1 { get; set; }
        public XmlUiLayout Panel2 { get; set; }

        public object CloneBySerialization() { return Clone(); }
        public XmlUiSplitter Clone() {
            var x = new XmlSerializer(this.GetType());
            using (var s = new MemoryStream()) { x.Serialize(s, this); s.Position = 0; return x.Deserialize(s) as XmlUiSplitter; }
        }

        public override string ToString() {
            return "<UI Splitter>";
        }
    }
}