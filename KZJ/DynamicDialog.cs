#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace KZJ {

    /// <summary>
    /// For prototyping support.
    /// Allows simple input dialogs to be created directly by minimal code.
    /// Example:
    ///         var newName = string.Empty;
    ///         if (new DynamicDialog("Rename Wallet")
    ///             .AddLabel($"Renaming wallet currently named \"{w.Name}\".")
    ///             .AddTextBox("New name for wallet:", 300, s => newName = s)
    ///             .AddButtons(okLabel: "Rename")
    ///             .GetResults() == DialogResult.OK) {
    ///             w.Name = newName;
    ///        }
    /// Note that the lambda action passed to the AddTextBox method causes the local variable
    /// to be updated only when "OK" button or return key are pressed.
    /// </summary>
    public class DynamicDialog : Form {
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMain;

        int itemCount = 0;

        List<Action> results = new List<Action>();

        public DynamicDialog(string title = null) {
            this.flowLayoutPanelMain = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelMain.SuspendLayout();
            this.SuspendLayout();

            // 
            // flowLayoutPanelMain
            // 
            this.flowLayoutPanelMain.AutoSize = true;
            this.flowLayoutPanelMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelMain.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelMain.Name = "flowLayoutPanelMain";
            this.flowLayoutPanelMain.Size = new System.Drawing.Size(309, 93);
            this.flowLayoutPanelMain.TabIndex = 0;

            // 
            // DynamicDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(876, 489);
            this.ControlBox = false;
            this.Controls.Add(this.flowLayoutPanelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DynamicDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = title;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterParent;

            this.flowLayoutPanelMain.ResumeLayout(false);
            this.flowLayoutPanelMain.PerformLayout();
            this.ResumeLayout(false);

            this.PerformLayout();
        }

        /// <summary>
        /// Use \r\n to insert a line break in labelText.
        /// </summary>
        /// <param name="labelText"></param>
        /// <returns></returns>
        public DynamicDialog AddLabel(string labelText) {

            itemCount++;

            var label = new Label() {
                AutoSize = true,
                Location = new System.Drawing.Point(3, 0),
                Name = $"label{itemCount}",
                Margin = new Padding(3, 5, 3, 0),
                Size = new System.Drawing.Size(303, 26),
                TabIndex = itemCount,
                Text = labelText,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            };

            flowLayoutPanelMain.Controls.Add(label);

            return this;
        }

        public DynamicDialog AddTextBox(
            string labelText,
            int width = 200,
            Action<string> result = null,
            bool readOnly = false,
            string initialText = null) {

            itemCount++;

            var label = new Label() {
                AutoSize = true,
                Location = new System.Drawing.Point(3, 7),
                Margin = new System.Windows.Forms.Padding(3, 7, 3, 0),
                Name = $"label{itemCount}",
                Size = new System.Drawing.Size(35, 13),
                TabIndex = itemCount,
                Text = labelText,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            };

            itemCount++;

            var textBox = new TextBox() {
                Location = new System.Drawing.Point(44, 3),
                Name = $"textBox1{itemCount}",
                Size = new System.Drawing.Size(width, 20),
                TabIndex = itemCount,
                ReadOnly = readOnly,
                Text = initialText
            };

            if (result != null) results.Add(() => result(textBox.Text));

            itemCount++;

            var panel = new FlowLayoutPanel() {
                AutoSize = true,
                AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                Location = new System.Drawing.Point(3, 29),
                Name = $"panel{itemCount}",
                Size = new System.Drawing.Size(247, 26),
                TabIndex = itemCount,
            };
            panel.Controls.Add(label);
            panel.Controls.Add(textBox);

            flowLayoutPanelMain.Controls.Add(panel);

            return this;
        }

        public DialogResult GetResults() {
            var r = ShowDialog();
            if (r == DialogResult.OK) {
                foreach (var a in results) a();
            }
            return r;
        }

        public DynamicDialog AddButtons(string okLabel = "OK", string cancelLabel = "Cancel") {

            itemCount++;

            var panel = new FlowLayoutPanel() {
                AutoSize = true,
                AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                Location = new System.Drawing.Point(3, 29),
                Name = $"panel{itemCount}",
                Size = new System.Drawing.Size(247, 26),
                TabIndex = itemCount,
            };

            foreach (var buttonText in new[] { okLabel, cancelLabel }) {
                if (buttonText != null) {

                    itemCount++;

                    var button = new Button() {
                        Location = new System.Drawing.Point(3, 3),
                        Name = $"button{itemCount}",
                        Size = new System.Drawing.Size(75, 23),
                        TabIndex = itemCount,
                        Text = buttonText,
                        UseVisualStyleBackColor = true,
                    };

                    if (buttonText == okLabel) {
                        button.DialogResult = System.Windows.Forms.DialogResult.OK;
                        AcceptButton = button;
                    }

                    if (buttonText == cancelLabel) {
                        button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                        CancelButton = button;
                    }

                    panel.Controls.Add(button);
                }
            }

            flowLayoutPanelMain.Controls.Add(panel);

            return this;
        }

    }
}
