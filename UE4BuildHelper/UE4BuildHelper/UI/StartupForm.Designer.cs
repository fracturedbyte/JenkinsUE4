namespace UE4BuildHelper.UI
{
    partial class StartupForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.UsernameLabel = new System.Windows.Forms.Label();
            this.JenkinsUsernameTextBox = new System.Windows.Forms.TextBox();
            this.APITokenTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.JenkinsURLTB = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.DefaultProjectTB = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.ProjectRootDirTB = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.EngineRootDirTB = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.RepoRootDirTB = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.DelayTB = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.LaunchTokenTB = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.JobNameTB = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.JobBindingSelection = new System.Windows.Forms.ComboBox();
            this.JobBindingP = new System.Windows.Forms.Panel();
            this.SCMServerTB = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SCMPasswordTB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SCMUsernameTB = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.PerforceWorkspaceTB = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.AddJobBinding = new System.Windows.Forms.Button();
            this.RemoveSelectedJobBinding = new System.Windows.Forms.Button();
            this.InheritBindingButton = new System.Windows.Forms.Button();
            this.JobBindingP.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Jenkins Credentials";
            // 
            // UsernameLabel
            // 
            this.UsernameLabel.AutoSize = true;
            this.UsernameLabel.Location = new System.Drawing.Point(16, 39);
            this.UsernameLabel.Name = "UsernameLabel";
            this.UsernameLabel.Size = new System.Drawing.Size(58, 13);
            this.UsernameLabel.TabIndex = 1;
            this.UsernameLabel.Text = "Username:";
            // 
            // JenkinsUsernameTextBox
            // 
            this.JenkinsUsernameTextBox.Location = new System.Drawing.Point(147, 36);
            this.JenkinsUsernameTextBox.Name = "JenkinsUsernameTextBox";
            this.JenkinsUsernameTextBox.Size = new System.Drawing.Size(287, 20);
            this.JenkinsUsernameTextBox.TabIndex = 2;
            this.JenkinsUsernameTextBox.TextChanged += new System.EventHandler(this.JenkinsUsernameTextBox_TextChanged);
            // 
            // APITokenTextBox
            // 
            this.APITokenTextBox.Location = new System.Drawing.Point(147, 62);
            this.APITokenTextBox.Name = "APITokenTextBox";
            this.APITokenTextBox.Size = new System.Drawing.Size(287, 20);
            this.APITokenTextBox.TabIndex = 4;
            this.APITokenTextBox.TextChanged += new System.EventHandler(this.APITokenTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "API Token:";
            // 
            // JenkinsURLTB
            // 
            this.JenkinsURLTB.Location = new System.Drawing.Point(147, 120);
            this.JenkinsURLTB.Name = "JenkinsURLTB";
            this.JenkinsURLTB.Size = new System.Drawing.Size(287, 20);
            this.JenkinsURLTB.TabIndex = 16;
            this.JenkinsURLTB.TextChanged += new System.EventHandler(this.JenkinsURLTB_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 123);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(32, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "URL:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 97);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(43, 13);
            this.label9.TabIndex = 14;
            this.label9.Text = "Jenkins";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(13, 164);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(106, 13);
            this.label17.TabIndex = 18;
            this.label17.Text = "Jenkins Job Bindings";
            // 
            // DefaultProjectTB
            // 
            this.DefaultProjectTB.Location = new System.Drawing.Point(134, 175);
            this.DefaultProjectTB.Name = "DefaultProjectTB";
            this.DefaultProjectTB.Size = new System.Drawing.Size(284, 20);
            this.DefaultProjectTB.TabIndex = 35;
            this.DefaultProjectTB.TextChanged += new System.EventHandler(this.DefaultProjectTB_TextChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(3, 178);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(80, 13);
            this.label16.TabIndex = 34;
            this.label16.Text = "Default Project:";
            // 
            // ProjectRootDirTB
            // 
            this.ProjectRootDirTB.Location = new System.Drawing.Point(134, 149);
            this.ProjectRootDirTB.Name = "ProjectRootDirTB";
            this.ProjectRootDirTB.Size = new System.Drawing.Size(284, 20);
            this.ProjectRootDirTB.TabIndex = 33;
            this.ProjectRootDirTB.TextChanged += new System.EventHandler(this.ProjectRootDirTB_TextChanged);
            this.ProjectRootDirTB.Enter += new System.EventHandler(this.ProjectRootDirTB_Enter);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(3, 152);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(74, 13);
            this.label15.TabIndex = 32;
            this.label15.Text = "Projects Root:";
            // 
            // EngineRootDirTB
            // 
            this.EngineRootDirTB.Location = new System.Drawing.Point(134, 123);
            this.EngineRootDirTB.Name = "EngineRootDirTB";
            this.EngineRootDirTB.Size = new System.Drawing.Size(284, 20);
            this.EngineRootDirTB.TabIndex = 31;
            this.EngineRootDirTB.TextChanged += new System.EventHandler(this.EngineRootDirTB_TextChanged);
            this.EngineRootDirTB.Enter += new System.EventHandler(this.EngineRootDirTB_Enter);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(3, 126);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(69, 13);
            this.label14.TabIndex = 30;
            this.label14.Text = "Engine Root:";
            // 
            // RepoRootDirTB
            // 
            this.RepoRootDirTB.Location = new System.Drawing.Point(134, 97);
            this.RepoRootDirTB.Name = "RepoRootDirTB";
            this.RepoRootDirTB.Size = new System.Drawing.Size(284, 20);
            this.RepoRootDirTB.TabIndex = 29;
            this.RepoRootDirTB.TextChanged += new System.EventHandler(this.RepoRootDirTB_TextChanged);
            this.RepoRootDirTB.Enter += new System.EventHandler(this.RepoRootDirTB_Enter);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(3, 100);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(86, 13);
            this.label13.TabIndex = 28;
            this.label13.Text = "Repository Root:";
            // 
            // DelayTB
            // 
            this.DelayTB.Location = new System.Drawing.Point(134, 60);
            this.DelayTB.Name = "DelayTB";
            this.DelayTB.Size = new System.Drawing.Size(284, 20);
            this.DelayTB.TabIndex = 27;
            this.DelayTB.TextChanged += new System.EventHandler(this.DelayTB_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 63);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(37, 13);
            this.label12.TabIndex = 26;
            this.label12.Text = "Delay:";
            // 
            // LaunchTokenTB
            // 
            this.LaunchTokenTB.Location = new System.Drawing.Point(134, 34);
            this.LaunchTokenTB.Name = "LaunchTokenTB";
            this.LaunchTokenTB.Size = new System.Drawing.Size(284, 20);
            this.LaunchTokenTB.TabIndex = 25;
            this.LaunchTokenTB.TextChanged += new System.EventHandler(this.LaunchTokenTB_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 37);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(80, 13);
            this.label11.TabIndex = 24;
            this.label11.Text = "Launch Token:";
            // 
            // JobNameTB
            // 
            this.JobNameTB.Enabled = false;
            this.JobNameTB.Location = new System.Drawing.Point(134, 8);
            this.JobNameTB.Name = "JobNameTB";
            this.JobNameTB.Size = new System.Drawing.Size(284, 20);
            this.JobNameTB.TabIndex = 23;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 11);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(58, 13);
            this.label10.TabIndex = 22;
            this.label10.Text = "Job Name:";
            // 
            // JobBindingSelection
            // 
            this.JobBindingSelection.FormattingEnabled = true;
            this.JobBindingSelection.Location = new System.Drawing.Point(16, 192);
            this.JobBindingSelection.Name = "JobBindingSelection";
            this.JobBindingSelection.Size = new System.Drawing.Size(121, 21);
            this.JobBindingSelection.TabIndex = 36;
            this.JobBindingSelection.SelectedIndexChanged += new System.EventHandler(this.JobBindingSelection_SelectedIndexChanged);
            // 
            // JobBindingP
            // 
            this.JobBindingP.Controls.Add(this.SCMServerTB);
            this.JobBindingP.Controls.Add(this.label7);
            this.JobBindingP.Controls.Add(this.SCMPasswordTB);
            this.JobBindingP.Controls.Add(this.label2);
            this.JobBindingP.Controls.Add(this.SCMUsernameTB);
            this.JobBindingP.Controls.Add(this.label4);
            this.JobBindingP.Controls.Add(this.label5);
            this.JobBindingP.Controls.Add(this.PerforceWorkspaceTB);
            this.JobBindingP.Controls.Add(this.label6);
            this.JobBindingP.Controls.Add(this.label10);
            this.JobBindingP.Controls.Add(this.JobNameTB);
            this.JobBindingP.Controls.Add(this.DefaultProjectTB);
            this.JobBindingP.Controls.Add(this.label11);
            this.JobBindingP.Controls.Add(this.label16);
            this.JobBindingP.Controls.Add(this.LaunchTokenTB);
            this.JobBindingP.Controls.Add(this.ProjectRootDirTB);
            this.JobBindingP.Controls.Add(this.label12);
            this.JobBindingP.Controls.Add(this.label15);
            this.JobBindingP.Controls.Add(this.DelayTB);
            this.JobBindingP.Controls.Add(this.EngineRootDirTB);
            this.JobBindingP.Controls.Add(this.label13);
            this.JobBindingP.Controls.Add(this.label14);
            this.JobBindingP.Controls.Add(this.RepoRootDirTB);
            this.JobBindingP.Location = new System.Drawing.Point(16, 256);
            this.JobBindingP.Name = "JobBindingP";
            this.JobBindingP.Size = new System.Drawing.Size(430, 376);
            this.JobBindingP.TabIndex = 37;
            // 
            // SCMServerTB
            // 
            this.SCMServerTB.Location = new System.Drawing.Point(134, 332);
            this.SCMServerTB.Name = "SCMServerTB";
            this.SCMServerTB.Size = new System.Drawing.Size(284, 20);
            this.SCMServerTB.TabIndex = 44;
            this.SCMServerTB.TextChanged += new System.EventHandler(this.SCMServerTB_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 335);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 43;
            this.label7.Text = "Server Address:";
            // 
            // SCMPasswordTB
            // 
            this.SCMPasswordTB.Location = new System.Drawing.Point(134, 297);
            this.SCMPasswordTB.Name = "SCMPasswordTB";
            this.SCMPasswordTB.PasswordChar = '*';
            this.SCMPasswordTB.Size = new System.Drawing.Size(284, 20);
            this.SCMPasswordTB.TabIndex = 42;
            this.SCMPasswordTB.TextChanged += new System.EventHandler(this.SCMPasswordTB_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 300);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 41;
            this.label2.Text = "Password/Ticket:";
            // 
            // SCMUsernameTB
            // 
            this.SCMUsernameTB.Location = new System.Drawing.Point(134, 271);
            this.SCMUsernameTB.Name = "SCMUsernameTB";
            this.SCMUsernameTB.Size = new System.Drawing.Size(284, 20);
            this.SCMUsernameTB.TabIndex = 40;
            this.SCMUsernameTB.TextChanged += new System.EventHandler(this.SCMUsernameTB_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 274);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 39;
            this.label4.Text = "Username:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 248);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 13);
            this.label5.TabIndex = 38;
            this.label5.Text = "SCM Credentials";
            // 
            // PerforceWorkspaceTB
            // 
            this.PerforceWorkspaceTB.Location = new System.Drawing.Point(134, 210);
            this.PerforceWorkspaceTB.Name = "PerforceWorkspaceTB";
            this.PerforceWorkspaceTB.Size = new System.Drawing.Size(284, 20);
            this.PerforceWorkspaceTB.TabIndex = 37;
            this.PerforceWorkspaceTB.TextChanged += new System.EventHandler(this.PerforceWorkspaceTB_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 213);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 13);
            this.label6.TabIndex = 36;
            this.label6.Text = "P4 Workspace:";
            // 
            // AddJobBinding
            // 
            this.AddJobBinding.Location = new System.Drawing.Point(147, 192);
            this.AddJobBinding.Name = "AddJobBinding";
            this.AddJobBinding.Size = new System.Drawing.Size(146, 23);
            this.AddJobBinding.TabIndex = 38;
            this.AddJobBinding.Text = "Add Job Binding";
            this.AddJobBinding.UseVisualStyleBackColor = true;
            this.AddJobBinding.Click += new System.EventHandler(this.AddJobBinding_Click);
            // 
            // RemoveSelectedJobBinding
            // 
            this.RemoveSelectedJobBinding.Location = new System.Drawing.Point(299, 192);
            this.RemoveSelectedJobBinding.Name = "RemoveSelectedJobBinding";
            this.RemoveSelectedJobBinding.Size = new System.Drawing.Size(135, 23);
            this.RemoveSelectedJobBinding.TabIndex = 39;
            this.RemoveSelectedJobBinding.Text = "Remove Selected";
            this.RemoveSelectedJobBinding.UseVisualStyleBackColor = true;
            this.RemoveSelectedJobBinding.Click += new System.EventHandler(this.RemoveSelectedJobBinding_Click);
            // 
            // InheritBindingButton
            // 
            this.InheritBindingButton.Location = new System.Drawing.Point(147, 221);
            this.InheritBindingButton.Name = "InheritBindingButton";
            this.InheritBindingButton.Size = new System.Drawing.Size(146, 23);
            this.InheritBindingButton.TabIndex = 40;
            this.InheritBindingButton.Text = "Inherit Job from this";
            this.InheritBindingButton.UseVisualStyleBackColor = true;
            this.InheritBindingButton.Click += new System.EventHandler(this.InheritBindingButton_Click);
            // 
            // StartupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(445, 639);
            this.Controls.Add(this.InheritBindingButton);
            this.Controls.Add(this.RemoveSelectedJobBinding);
            this.Controls.Add(this.AddJobBinding);
            this.Controls.Add(this.JobBindingP);
            this.Controls.Add(this.JobBindingSelection);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.JenkinsURLTB);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.APITokenTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.JenkinsUsernameTextBox);
            this.Controls.Add(this.UsernameLabel);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "StartupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.JobBindingP.ResumeLayout(false);
            this.JobBindingP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label UsernameLabel;
        private System.Windows.Forms.TextBox JenkinsUsernameTextBox;
        private System.Windows.Forms.TextBox APITokenTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox JenkinsURLTB;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox DefaultProjectTB;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox ProjectRootDirTB;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox EngineRootDirTB;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox RepoRootDirTB;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox DelayTB;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox LaunchTokenTB;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox JobNameTB;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox JobBindingSelection;
        private System.Windows.Forms.Panel JobBindingP;
        private System.Windows.Forms.Button AddJobBinding;
        private System.Windows.Forms.Button RemoveSelectedJobBinding;
        private System.Windows.Forms.TextBox PerforceWorkspaceTB;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox SCMServerTB;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox SCMPasswordTB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox SCMUsernameTB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button InheritBindingButton;
    }
}