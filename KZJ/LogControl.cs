#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace KZJ {
    public partial class LogControl : UserControl {
        public LogControl() {
            InitializeComponent();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e) {
            Clipboard.SetText(_TextBox.Text);
            _TextBox.Text = "";
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
            Clipboard.SetText(_TextBox.Text);
        }

        public void TimeStamp(string format, params object[] args) {
            TimeStamp(DateTime.Now, format, args);
        }

        public void TimeStamp(DateTime when, string format, params object[] args) {
            WriteLine(string.Concat(when.ToString("HH:mm:ss "), format), args);
        }

        public void WriteLine(string format, params object[] args) => Write(format + "\r\n", args);
        public void Write(string format, params object[] args) {
            try {
                TextBox tb = _TextBox;
                if (tb.InvokeRequired) {
                    tb.BeginInvoke(new Action<string, object[]>(Write), format, args);
                    return;
                }
                string line = string.Format(format, args);
                tb.AppendText(line);
                if (tb.TextLength > 20000) {
                    tb.Text = tb.Text.Substring(10000);
                }
            } catch { }
        }

        /// <summary>
        /// Post a log entry for an exception. Will include a stack trace and the calling methods name.
        /// </summary>
        /// <param name="ex"></param>
        public void Exception(Exception ex) {
            var sf = new StackFrame(1); // callers frame
            var callingMethodName = sf.GetMethod().Name;
            WriteLine("{0:yyyy-MM-dd HH:mm:ss}\r\nException\r\n{1}\r\n{2}\r\n{3}\r\n-----\r\n"
                , DateTime.Now, callingMethodName, ex.Message, ex.StackTrace);
        }

        /// <summary>
        /// Post a log entry for an exception. Will include a stack trace and the calling methods name.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Exception(Exception ex, string format, params object[] args) {
            StackFrame sf = new StackFrame(1); // callers frame
            string callingMethodName = sf.GetMethod().Name;
            WriteLine("{0:yyyy-MM-dd HH:mm:ss}\r\nException {4}\r\n{1}\r\n{2}\r\n{3}\r\n-----\r\n"
                    , DateTime.Now, callingMethodName, ex.Message, ex.StackTrace, string.Format(format, args));
        }

    }
}
