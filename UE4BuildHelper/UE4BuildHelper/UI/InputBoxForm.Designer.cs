namespace UE4BuildHelper.UI
{
    partial class InputBoxForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OkButton = new System.Windows.Forms.Button();
            this.MessageL = new System.Windows.Forms.Label();
            this.InputTextTB = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(100, 56);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(100, 23);
            this.OkButton.TabIndex = 0;
            this.OkButton.Text = "Ok";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // MessageL
            // 
            this.MessageL.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.MessageL.Location = new System.Drawing.Point(50, 10);
            this.MessageL.Name = "MessageL";
            this.MessageL.Size = new System.Drawing.Size(200, 13);
            this.MessageL.TabIndex = 1;
            this.MessageL.Text = "label1";
            this.MessageL.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // InputTextTB
            // 
            this.InputTextTB.Location = new System.Drawing.Point(50, 30);
            this.InputTextTB.Name = "InputTextTB";
            this.InputTextTB.Size = new System.Drawing.Size(200, 20);
            this.InputTextTB.TabIndex = 2;
            // 
            // InputBoxForm
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 91);
            this.Controls.Add(this.InputTextTB);
            this.Controls.Add(this.MessageL);
            this.Controls.Add(this.OkButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputBoxForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Label MessageL;
        private System.Windows.Forms.TextBox InputTextTB;
    }
}