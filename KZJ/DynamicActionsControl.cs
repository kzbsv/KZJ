#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KZJ {
    public class DynamicActionsControl : UserControl {

        public DynamicActionsControl(IEnumerable<(string label, Action action)> actions) {
            InitializeControls(actions);
        }

        void InitializeControls(IEnumerable<(string label, Action action)> actions) {
            SuspendLayout();

            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;

            var i = -1;
            foreach (var a in actions) {
                i++;
                Controls.Add(DynamicActionButton(i, a.label, a.action));
            }

            Size = new Size(81, 29 * i + 6);
            ResumeLayout(false);
        }

        Control DynamicActionButton(int i, string label, Action action) {
            var b = new Button {
                Location = new Point(3, i * 29 + 3),
                Size = new Size(75, 23),
                TabIndex = i,
                Text = label,
                UseVisualStyleBackColor = true
            };
            b.Click += (s, e) => action();
            return b;
        }
    }
}
