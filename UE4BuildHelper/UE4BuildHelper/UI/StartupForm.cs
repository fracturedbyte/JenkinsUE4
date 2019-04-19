using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UE4BuildHelper.UI
{
    public partial class StartupForm : Form
    {
        private Config ConfigObject = null;

        public StartupForm()
        {
            InitializeComponent();
        }

        public void LoadConfig(Config InConfig)
        {
            ConfigObject = InConfig;

            if (ConfigObject != null)
            {
                JenkinsUsernameTextBox.Text = ConfigObject.JenkinsCredentials.UserName;
                APITokenTextBox.Text = ConfigObject.JenkinsCredentials.APIToken;

                JenkinsURLTB.Text = ConfigObject.JenkinsServerURL;

                FillJobSelection();
            }
        }

        private void FillJobSelection(bool bSelectLast = false)
        {
            JobBindingSelection.Items.Clear();

            if (ConfigObject != null)
            {
                foreach (JenkinsJobInfo JobInfo in ConfigObject.JenkinsJobs)
                {
                    JobBindingSelection.Items.Add(JobInfo.Name);
                }
            }

            if(JobBindingSelection.Items.Count > 0)
            {
                JobBindingSelection.SelectedIndex = bSelectLast ? (JobBindingSelection.Items.Count - 1) : 0;
            }

            UpdateJobBindingPanel();
        }

        private void SelectJobBinding(string JobName)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob(JobName);

                if (JobInfo != null)
                {
                    JobNameTB.Text = JobInfo.Name;
                    LaunchTokenTB.Text = JobInfo.LaunchToken;
                    DelayTB.Text = JobInfo.Delay.ToString();

                    RepoRootDirTB.Text = JobInfo.RepositoryRootDirectory;
                    EngineRootDirTB.Text = JobInfo.EngineDirectory;
                    ProjectRootDirTB.Text = JobInfo.ProjectsDirectory;
                    DefaultProjectTB.Text = JobInfo.DefaultProjectName;

                    PerforceWorkspaceTB.Text = JobInfo.WorkspaceName;

                    if (JobInfo.SCMData != null)
                    {
                        if (JobInfo.SCMData.Credentials != null)
                        {
                            SCMUsernameTB.Text = JobInfo.SCMData.Credentials.UserName;
                            SCMPasswordTB.Text = JobInfo.SCMData.Credentials.GetPassword();
                        }

                        SCMServerTB.Text = JobInfo.SCMData.ServerAddress;
                    }
                }
            }
        }

        private void UpdateJobBindingPanel()
        {
            if(JobBindingSelection.Items.Count == 0)
            {
                JobBindingP.Visible = false;
            }
            else
            {
                JobBindingP.Visible = true;
            }
        }

        private void JenkinsUsernameTextBox_TextChanged(object sender, EventArgs e)
        {
            if(ConfigObject != null)
            {
                ConfigObject.JenkinsCredentials.UserName = JenkinsUsernameTextBox.Text;

                ConfigObject.Save();
            }
        }

        private void APITokenTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                ConfigObject.JenkinsCredentials.APIToken = APITokenTextBox.Text;

                ConfigObject.Save();
            }
        }

        private void JenkinsURLTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                ConfigObject.JenkinsServerURL = JenkinsURLTB.Text;

                ConfigObject.Save();
            }
        }

        private string GetPathFromSelectionDialog()
        {
            using (FolderBrowserDialog FolderBrowser = new FolderBrowserDialog())
            {
                DialogResult Result = FolderBrowser.ShowDialog();

                if (Result == DialogResult.OK && !string.IsNullOrWhiteSpace(FolderBrowser.SelectedPath))
                {
                    return FolderBrowser.SelectedPath;
                }
            }

            return "";
        }

        private void LaunchTokenTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if (JobInfo != null)
                {
                    JobInfo.LaunchToken = LaunchTokenTB.Text;

                    ConfigObject.Save();
                }
            }
        }

        private void DelayTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if (JobInfo != null)
                {
                    JobInfo.Delay = DelayTB.Text.Length > 0 ? int.Parse(DelayTB.Text) : 0;

                    ConfigObject.Save();
                }
            }
        }

        private void RepoRootDirTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if (JobInfo != null)
                {
                    JobInfo.RepositoryRootDirectory = RepoRootDirTB.Text;

                    ConfigObject.Save();
                }
            }
        }

        private void EngineRootDirTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if (JobInfo != null)
                {
                    JobInfo.EngineDirectory = EngineRootDirTB.Text;

                    ConfigObject.Save();
                }
            }
        }

        private void ProjectRootDirTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if (JobInfo != null)
                {
                    JobInfo.ProjectsDirectory = ProjectRootDirTB.Text;

                    ConfigObject.Save();
                }
            }
        }

        private void DefaultProjectTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if (JobInfo != null)
                {
                    JobInfo.DefaultProjectName = DefaultProjectTB.Text;

                    ConfigObject.Save();
                }
            }
        }

        private void PerforceWorkspaceTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if (JobInfo != null)
                {
                    JobInfo.WorkspaceName = PerforceWorkspaceTB.Text;

                    ConfigObject.Save();
                }
            }
        }

        private void SCMUsernameTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if (JobInfo != null && JobInfo.IsSCMDataValid())
                {
                    JobInfo.SCMData.Credentials.UserName = SCMUsernameTB.Text;

                    ConfigObject.Save();
                }
            }
        }

        private void SCMPasswordTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if (JobInfo != null && 
                    JobInfo.IsSCMDataValid() && 
                    SCMPasswordTB.Text != JobInfo.SCMData.Credentials.GetPassword())
                {
                    JobInfo.SCMData.Credentials.SetPassword(SCMPasswordTB.Text);

                    ConfigObject.Save();
                }
            }
        }

        private void SCMServerTB_TextChanged(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if (JobInfo != null && JobInfo.IsSCMDataValid())
                {
                    JobInfo.SCMData.ServerAddress = SCMServerTB.Text;

                    ConfigObject.Save();
                }
            }
        }

        private void RepoRootDirTB_Enter(object sender, EventArgs e)
        {
            string Path = GetPathFromSelectionDialog();

            if (!string.IsNullOrEmpty(Path))
            {
                RepoRootDirTB.Text = Path;
            }

            RepoRootDirTB_TextChanged(null, null);
        }

        private void EngineRootDirTB_Enter(object sender, EventArgs e)
        {
            string Path = GetPathFromSelectionDialog();

            if (!string.IsNullOrEmpty(Path))
            {
                EngineRootDirTB.Text = Path;
            }

            EngineRootDirTB_TextChanged(null, null);
        }

        private void ProjectRootDirTB_Enter(object sender, EventArgs e)
        {
            string Path = GetPathFromSelectionDialog();

            if (!string.IsNullOrEmpty(Path))
            {
                ProjectRootDirTB.Text = Path;
            }

            ProjectRootDirTB_TextChanged(null, null);
        }

        private void JobBindingSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectJobBinding((string)JobBindingSelection.SelectedItem);
        }

        private void AddJobBinding_Click(object sender, EventArgs e)
        {
            InputBoxForm InputBox = new InputBoxForm();

            InputBox.Init("Job name:");
            InputBox.ShowDialog();

            string JobName = InputBox.GetInput();

            if (ConfigObject != null && 
                !string.IsNullOrEmpty(JobName) && 
                !string.IsNullOrWhiteSpace(JobName) && 
                !JobName.Contains(" ") &&
                ConfigObject.FindJenkinsJob(JobName) == null)
            {
                JenkinsJobInfo JobInfo = new JenkinsJobInfo();

                JobInfo.Name = JobName;

                ConfigObject.JenkinsJobs.Add(JobInfo);

                ConfigObject.Save();

                FillJobSelection(true);
            }
        }

        private void RemoveSelectedJobBinding_Click(object sender, EventArgs e)
        {
            if (ConfigObject != null)
            {
                JenkinsJobInfo JobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

                if(JobInfo != null)
                {
                    ConfigObject.JenkinsJobs.Remove(JobInfo);

                    ConfigObject.Save();

                    FillJobSelection();
                }
            }
        }

        private void InheritBindingButton_Click(object sender, EventArgs e)
        {
            JenkinsJobInfo BaseJobInfo = ConfigObject.FindJenkinsJob((string)JobBindingSelection.SelectedItem);

            if (BaseJobInfo != null)
            {
                InputBoxForm InputBox = new InputBoxForm();

                InputBox.Init("Job name:");
                InputBox.ShowDialog();

                string JobName = InputBox.GetInput();

                if (ConfigObject != null &&
                    !string.IsNullOrEmpty(JobName) &&
                    !string.IsNullOrWhiteSpace(JobName) &&
                    !JobName.Contains(" ") &&
                    ConfigObject.FindJenkinsJob(JobName) == null)
                {
                    JenkinsJobInfo JobInfo = BaseJobInfo.Clone();

                    JobInfo.Name = JobName;

                    ConfigObject.JenkinsJobs.Add(JobInfo);

                    ConfigObject.Save();

                    FillJobSelection(true);
                }
            }
        }
    }
}
