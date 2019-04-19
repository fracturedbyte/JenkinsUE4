using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UE4BuildHelper
{
    class UEBuildAutomation
    {
        private SCMProcessor SCM;

        public void Launch(ESCMType InSCMType, string JenkinsJobName)
        {
            if (!string.IsNullOrEmpty(JenkinsJobName))
            {
                JenkinsJobInfo JobInfo = Config.Get().FindJenkinsJob(JenkinsJobName);

                if (JobInfo != null && JobInfo.IsSCMDataValid())
                {
                    SCM = new SCMProcessor();

                    SCM.Init(JobInfo.SCMData, InSCMType, JobInfo.WorkspaceName);
                }
            }
            else
            {
                Logger.WError("Failed to initialize Source Code Manager processor! Jenkins Job Name is empty ([-j] argument expected)");
            }
        }

        public int ProcessSCMTrigger(int Change, string DLLJobName, string PackageJobName, ESCMType SCMType, string UserName)
        {
            JenkinsJobInfo DLLJobInfo = Config.Get().FindJenkinsJob(DLLJobName);
            JenkinsJobInfo PackageJobInfo = Config.Get().FindJenkinsJob(PackageJobName);

            if(DLLJobInfo != null && PackageJobInfo != null && !DLLJobInfo.IsRepositoryEquals(PackageJobInfo))
            {
                Logger.WError("Job '" + DLLJobInfo.Name + "' has different repository with job '" + PackageJobInfo.Name + "'. Can't trigger build.");

                return -1;
            }

            if (DLLJobInfo != null && SCM != null)
            {
                // Do not handle if triggered by this user
                if (DLLJobInfo.IsSCMDataValid() && DLLJobInfo.SCMData.Credentials.UserName.Equals(UserName, StringComparison.OrdinalIgnoreCase))
                {
                    return 0;
                }

                Logger.WriteLine("Looking for build marks...");

                List<SCMCommitInfo> Commits = null;

                if (SCM.ParseChangelist(Change, Change, DLLJobInfo.RepositoryRootDirectory, out Commits))
                {
                    if (Commits.Count > 0 && Commits[0].HasAnyActions())
                    {
                        SCMCommitInfo CommitInfo = Commits[0];

                        Logger.WriteLine("Triggering Jenkins Job...");

                        string ProjectOverride = CommitInfo.ProjectOverride;

                        // Validate Project override
                        if(ProjectOverride != null)
                        {
                            if(!Directory.Exists(DLLJobInfo.GetProjectDirectory(ProjectOverride)))
                            {
                                ProjectOverride = null;
                            }
                        }
                        //

                        if (CommitInfo.IsDLLBuild())
                        {
                            HTTPClient.TriggerJenkinsParametrizedBuild(DLLJobInfo.Name, DLLJobInfo.SCMData.Credentials, DLLJobInfo.LaunchToken, DLLJobInfo.Delay, SCMType, ProjectOverride);
                        }
                        else if(CommitInfo.IsPackage())
                        {
                            string BuildPlatform = null;
                            bool bFullRebuild = CommitInfo.BuildAction == SCMCommitInfo.EBuildAction.EBA_Rebuild;
                            bool bSkipCook = CommitInfo.BuildAction == SCMCommitInfo.EBuildAction.EBA_Package;

                            switch(CommitInfo.BuildSubAction)
                            {
                                case SCMCommitInfo.EBuildSubAction.EBSA_IOS:
                                    BuildPlatform = "IOS";
                                    break;
                                case SCMCommitInfo.EBuildSubAction.EBSA_Android:
                                    BuildPlatform = "Android";
                                    break;
                                case SCMCommitInfo.EBuildSubAction.EBSA_Win64:
                                    BuildPlatform = "Win64";
                                    break;
                                default:
                                    break;
                            }
  
                            if (!string.IsNullOrEmpty(BuildPlatform))
                            {
                                List<KeyValuePair<string, string>> AdditionalArguments = new List<KeyValuePair<string, string>>();

                                AdditionalArguments.Add(new KeyValuePair<string, string>("Platform", BuildPlatform));
                                AdditionalArguments.Add(new KeyValuePair<string, string>("bFullRebuild", (bFullRebuild ? "true" : "false")));
                                AdditionalArguments.Add(new KeyValuePair<string, string>("bSkipCook", (bSkipCook ? "true" : "false")));
                                AdditionalArguments.Add(new KeyValuePair<string, string>("bUpload", (CommitInfo.bShouldUpload ? "true" : "false")));

                                HTTPClient.TriggerJenkinsParametrizedBuild(PackageJobInfo.Name, PackageJobInfo.SCMData.Credentials, PackageJobInfo.LaunchToken,
                                    0, SCMType, ProjectOverride);   // Forcing zero delay because ini files may be incorrect if some build was triggered after this from web view
                            }
                        }
                    }
                }
            }

            // Always return successful result
            return 0;
        }

        public int ProcessUpdate(GlobalParametersInfo GlobalParameters)
        {
            if (GlobalParameters == null)
            {
                Logger.WError("Global parameters is null!");
                return -1;
            }

            JenkinsJobInfo JobInfo = Config.Get().FindJenkinsJob(GlobalParameters.JenkinsJobName);

            if (JobInfo != null && SCM != null)
            {
                string ProjectDirectory = JobInfo.GetProjectDirectory(GlobalParameters.ProjectOverride);

                Logger.WriteLine("Updating Source Code...");

                if (SCM.UpdateToTheLatestRevision(JobInfo.EngineDirectory, ProjectDirectory, JobInfo.RepositoryRootDirectory))
                {
                    // Write commit info of the current revision in to the cache
                    SCMCommitInfo CurrentRevisionInfo = SCM.GetCurrentRevisionInfo(JobInfo.RepositoryRootDirectory);
                    if (CurrentRevisionInfo != null)
                    {
                        Serialization.SCMCachedData SCMCache = Serialization.SCMCachedData.Load<Serialization.SCMCachedData>(GlobalParameters.JenkinsJobName);

                        if (SCMCache == null)
                        {
                            SCMCache = new Serialization.SCMCachedData();
                        }

                        if (SCMCache != null)
                        {
                            SCMCache.LastUpdateCommitInfo = CurrentRevisionInfo;

                            SCMCache.Save(GlobalParameters.JenkinsJobName);
                        }
                    }
                    //

                    Logger.WriteLine("Resolving conflicts...");

                    if (SCM.ResolveConflicts(JobInfo.EngineDirectory, ProjectDirectory, true))
                    {
                        Logger.WriteLine("Source Code updated successfully.");

                        return 0;
                    }
                    else
                    {
                        Logger.WError("Can't resolve conflicts.");
                    }      
                }
                else
                {
                    Logger.WError("Can't update SCM to the latest revision.");
                }
            }

            return -1;
        }

        public int ResolveAndCacheSourceCommits(GlobalParametersInfo GlobalParameters, bool bWriteChangelog, string WorkspaceRoot)
        {
            if (GlobalParameters == null)
            {
                Logger.WError("Global parameters is null!");
                return -1;
            }

            JenkinsJobInfo JobInfo = Config.Get().FindJenkinsJob(GlobalParameters.JenkinsJobName);

            if (JobInfo != null && SCM != null)
            {
                Logger.WriteLine("Resolving commit info...");

                int InitialChangelistNumber = -1;

                Serialization.SCMCachedData SCMCache = Serialization.SCMCachedData.Load<Serialization.SCMCachedData>(GlobalParameters.JenkinsJobName);
                if (SCMCache != null && SCMCache.LastUpdateCommitInfo.Revision > 0)
                {
                    // Check if our initial revision is valid
                    if (SCMCache.LastCheckRevision != SCMCache.LastUpdateCommitInfo.Revision && SCMCache.LastCheckRevision > 0)
                    {
                        InitialChangelistNumber = SCMCache.LastUpdateCommitInfo.Revision + 1;

                        SCMCommitInfo LatestRevisionInfo = SCM.GetLatestRevisionInfo(JobInfo.RepositoryRootDirectory);
                        if (LatestRevisionInfo != null && InitialChangelistNumber > LatestRevisionInfo.Revision)
                        {
                            InitialChangelistNumber = -1;
                        }

                        Logger.WriteLine("Cached Initial revision: " + InitialChangelistNumber);
                    }
                    else
                    {
                        Logger.WriteLine("Previous check revision the same with the last update revision. Adjusting revision...");
                    }

                    SCMCache.LastCheckRevision = SCMCache.LastUpdateCommitInfo.Revision;
                    SCMCache.Save(GlobalParameters.JenkinsJobName);
                    //
                }

                // Get Initial revision if it does not cached
                if (InitialChangelistNumber <= 0)
                {
                    Logger.WriteLine("Initial revision not found in the cache. Initializing from the last 15 changes.");

                    SCMCommitInfo CurrentRevisionInfo = SCM.GetCurrentRevisionInfo(JobInfo.RepositoryRootDirectory);
                    if (CurrentRevisionInfo != null)
                    {
                        InitialChangelistNumber = CurrentRevisionInfo.Revision - 15;
                    }

                    if (InitialChangelistNumber <= 0)
                    {
                        InitialChangelistNumber = 1;
                    }

                    Logger.WriteLine("Initial revision found: " + InitialChangelistNumber);
                }
                //


                List<SCMCommitInfo> Commits = null;

                if (SCM.ParseChangelist(InitialChangelistNumber, 0, JobInfo.RepositoryRootDirectory, out Commits, bWriteChangelog))
                {
                    if(bWriteChangelog && WorkspaceRoot != null)
                    {
                        string JobPath = WorkspaceRoot.Replace("workspace", "jobs");

                        int BuildNumber = Tools.ExtractBuildNumberFromTag(GlobalParameters.BuildTag);

                        if (BuildNumber >= 0)
                        {
                            string BuildInfoPath = Path.Combine(JobPath, "builds", BuildNumber.ToString());

                            if (Directory.Exists(BuildInfoPath))
                            {
                                string ChangelogFile = Path.Combine(BuildInfoPath, "changelog.xml");

                                Logger.WriteLine("Writing changelog: " + ChangelogFile);

                                SCM.WriteChangelogToXML(ChangelogFile, Commits);
                            }
                            else
                            {
                                Logger.WError("Can't write changelog by path: " + BuildInfoPath);
                            }
                        }
                        else
                        {
                            Logger.WError("Can't write changelog with build number: " + BuildNumber);
                        }

                        return 0;
                    }

                    SCMCommitInfo ResolvedCommitInfo = SCM.ResolveCommonCommitInfo(Commits, true, GlobalParameters.ProjectOverride);

                    if(ResolvedCommitInfo == null)
                    {
                        Logger.WError("Can't resolve commit info because revisions with build tags not found!");

                        return -1;
                    }

                    Logger.WriteLine("Commit info resolved: " + ResolvedCommitInfo.ToString());

                    // Write resolved commit info in to the cache
                    Serialization.JenkinsJobCache JobCache = Serialization.JenkinsJobCache.Load<Serialization.JenkinsJobCache>(GlobalParameters.BuildTag);

                    if (JobCache == null)
                    {
                        JobCache = new Serialization.JenkinsJobCache();
                    }

                    if (JobCache != null)
                    {
                        JobCache.ResolvedCommitInfo = ResolvedCommitInfo;

                        JobCache.Save(GlobalParameters.BuildTag);
                    }
                    //

                    return 0;
                }
            }

            Logger.WError("Can't resolve commit info!");

            return -1;
        }

        public int CheckoutBinaries(GlobalParametersInfo GlobalParameters, string[] InExtensions)
        {
            if (GlobalParameters == null)
            {
                Logger.WError("Global parameters is null!");
                return -1;
            }

            JenkinsJobInfo JobInfo = Config.Get().FindJenkinsJob(GlobalParameters.JenkinsJobName);

            if (JobInfo != null && SCM != null)
            {
                string ProjectDirectory = JobInfo.GetProjectDirectory(GlobalParameters.ProjectOverride);

                Logger.WriteLine("Checking out binaries...");

                if (SCM.CheckoutBinaries(JobInfo.EngineDirectory, ProjectDirectory, InExtensions))
                {
                    return 0;
                }
                else
                {
                    Logger.WError("Can't checkout binaries.");
                }
            }

            return -1;
        }

        public int ProcessBinaryBuild(GlobalParametersInfo GlobalParameters)
        {
            if (GlobalParameters == null)
            {
                Logger.WError("Global parameters is null!");
                return -1;
            }

            JenkinsJobInfo JobInfo = Config.Get().FindJenkinsJob(GlobalParameters.JenkinsJobName);

            if (JobInfo != null)
            {
                Serialization.JenkinsJobCache JobCache = Serialization.JenkinsJobCache.Load<Serialization.JenkinsJobCache>(GlobalParameters.BuildTag);

                string ResolvedBatchFile = (JobCache != null && JobCache.ResolvedCommitInfo != null) ? (JobCache.ResolvedCommitInfo.BuildAction == SCMCommitInfo.EBuildAction.EBA_Rebuild ? 
                    FileHelper.RebuildDLLBat: FileHelper.BuildDLLBat) : FileHelper.BuildDLLBat;

                string AbsoluteBatchPath = Path.GetFullPath(Path.Combine(FileHelper.BatchFilesPath, ResolvedBatchFile));

                Logger.StdOut("Building DLL with batch file: " + AbsoluteBatchPath);

                if (File.Exists(AbsoluteBatchPath))
                {
                    Process BuildProcess = new Process();
                    ProcessStartInfo StartInfo = new ProcessStartInfo("cmd.exe", @"/C " + AbsoluteBatchPath + " " +
                    JobInfo.EngineDirectory + " " +
                    JobInfo.GetProjectDirectory(GlobalParameters.ProjectOverride) + " " +
                    JobInfo.DefaultProjectName);

                    StartInfo.RedirectStandardOutput = true;
                    StartInfo.UseShellExecute = false;
                    BuildProcess.StartInfo = StartInfo;

                    bool bHasUnhandledErrors = false;

                    BuildProcess.OutputDataReceived += delegate (object SendingProcess, DataReceivedEventArgs OutLine)
                    {
                        if (OutLine.Data != null)
                        {
                            Logger.WriteLine("UE4: " + OutLine.Data);

                            // When build generates error: ERROR: System.UnauthorizedAccessException: Access to the path '' is denied. - ExitCode is 0.
                            if (OutLine.Data.StartsWith("ERROR:"))
                            {
                                bHasUnhandledErrors = true;

                                BuildProcess.Kill();
                            }
                        }
                    };

                    BuildProcess.Start();
                    BuildProcess.BeginOutputReadLine();
                    BuildProcess.WaitForExit();

                    Logger.StdOut("Build DLL completed with result: " + BuildProcess.ExitCode);

                    return bHasUnhandledErrors ? (-1) : BuildProcess.ExitCode;
                }
                else
                {
                    Logger.WError("Build.bat does not exist.");
                }
            }

            Logger.WError("Error of executing commandlet: Jenkins job info not found in the default configuration: " + GlobalParameters.JenkinsJobName);

            return -1;
        }

        public int RevertSCM(GlobalParametersInfo GlobalParameters, bool bRevertUnchangedOnly)
        {
            if (GlobalParameters == null)
            {
                Logger.WError("Global parameters is null!");
                return -1;
            }

            JenkinsJobInfo JobInfo = Config.Get().FindJenkinsJob(GlobalParameters.JenkinsJobName);

            if (JobInfo != null && SCM != null)
            {
                string ProjectDirectory = JobInfo.GetProjectDirectory(GlobalParameters.ProjectOverride);

                Logger.WriteLine("Reverting files...");

                if (SCM.RevertFiles(JobInfo.EngineDirectory, ProjectDirectory, bRevertUnchangedOnly))
                {
                    Logger.WriteLine("Files reverted successfully");

                    return 0;
                }
                else
                {
                    Logger.WError("Revert failed.");
                }
            }

            Logger.WError("Can't revert files");

            return -1;
        }

        public int ProcessCommitDLL(GlobalParametersInfo GlobalParameters, bool bRevertUnchanged)
        {
            if (GlobalParameters == null)
            {
                Logger.WError("Global parameters is null!");
                return -1;
            }

            // Read Resolved Commit Info from job cache
            Serialization.JenkinsJobCache JobCache = Serialization.JenkinsJobCache.Load<Serialization.JenkinsJobCache>(GlobalParameters.BuildTag);
            SCMCommitInfo ResolvedCommitInfo = null;

            if(JobCache != null)
            {
                ResolvedCommitInfo = JobCache.ResolvedCommitInfo;
            }
            //

            JenkinsJobInfo JobInfo = Config.Get().FindJenkinsJob(GlobalParameters.JenkinsJobName);

            if (JobInfo != null && ResolvedCommitInfo != null && SCM != null)
            {
                string ProjectDirectory = JobInfo.GetProjectDirectory(GlobalParameters.ProjectOverride);

                Logger.WriteLine("Committing binaries...");

                if (SCM.CommitBinaries(JobInfo.EngineDirectory, ProjectDirectory, ResolvedCommitInfo, bRevertUnchanged))
                {
                    Logger.WriteLine("Binaries committed successfully.");

                    return 0;
                }
                else
                {
                    Logger.WError("Commit failed.");
                }
            }

            Logger.WError("Can't commit binaries.");

            return 0;
        }

        private string ConfigureIni(GlobalParametersInfo GlobalParameters, JenkinsJobInfo JobInfo, string JenkinsWorkspaceRoot, string TargetDirectory)
        {
            string AdditionalCommandline = "";

            if (JobInfo != null && JenkinsWorkspaceRoot != null)
            {
                string ProjectDirectory = JobInfo.GetProjectDirectory(GlobalParameters.ProjectOverride);

                Console.WriteLine("Configure custom ini files.");

                string PathToConfigOverride = Path.Combine(JenkinsWorkspaceRoot, "Upload", "Config");

                // Override Ini
                if (Directory.Exists(PathToConfigOverride))
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(PathToConfigOverride);
                    FileInfo[] Files = DirInfo.GetFiles("*.ini");

                    // Force unlock files if using perforce
                    // We did not use checkout because someone can use this files
                    if (SCM.SupportsCheckout())
                    {
                        foreach (FileInfo Entry in Files)
                        {
                            string FileName = Tools.BuildIniPath(Entry.Name, ProjectDirectory);

                            if (File.Exists(FileName))
                            {
                                FileAttributes Attributes = File.GetAttributes(FileName);

                                if ((Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                {
                                    Attributes = Attributes & ~FileAttributes.ReadOnly;
                                    File.SetAttributes(FileName, Attributes);

                                    Logger.WriteLine("Unlocking ini file: " + FileName);
                                }
                            }
                        }
                    }
                    //

                    foreach (FileInfo Entry in Files)
                    {
                        string FileName = Tools.BuildIniPath(Entry.Name, ProjectDirectory);

                        Console.WriteLine("\t Replacing ini: " + FileName);

                        if(File.Exists(FileName))
                        {
                            if (SCM.SupportsCheckout())
                            {
                                string BackupFileName = FileName + ".backup";

                                Console.WriteLine("\t Backing up ini to: " + BackupFileName);

                                if (File.Exists(BackupFileName))
                                {
                                    File.Delete(BackupFileName);
                                }

                                File.Move(FileName, BackupFileName);  
                            }
                            else
                            {
                                File.Delete(FileName);
                            }
                        }

                        File.Move(Path.Combine(PathToConfigOverride, Entry.Name), FileName);
                    }
                }
                //

                string DebugGameIniFileName = Tools.BuildIniPath("DefaultGame.ini", ProjectDirectory);
                if (File.Exists(DebugGameIniFileName))
                {
                    External.IniParser Parser = new External.IniParser(DebugGameIniFileName);

                    if (Parser.Read("bGenerateChunks", "/Script/UnrealEd.ProjectPackagingSettings").ToLower() == "true" && TargetDirectory != null)
                    {
                        string HttpChunkInstallDataVersion = Parser.Read("HttpChunkInstallDataVersion", "/Script/UnrealEd.ProjectPackagingSettings");
                        string HttpChunkInstallDataDirectory = TargetDirectory + "\\ChunkInstall";
                        AdditionalCommandline += "-manifests -createchunkinstall -chunkinstalldirectory=" +
                            HttpChunkInstallDataDirectory + " -chunkinstallversion=" + HttpChunkInstallDataVersion + " ";
                    }

                    if (Parser.Read("ForDistribution", "/Script/UnrealEd.ProjectPackagingSettings").ToLower() == "true")
                    {
                        AdditionalCommandline += "-distribution ";
                    }
                }
            }

            Console.WriteLine("Ini files configurated successfully.");

            return AdditionalCommandline;
        }

        public int PerformPackageProject(GlobalParametersInfo GlobalParameters, 
            string BuildConfiguration, string Platform, string WorkspaceRoot,
            bool bFullRebuild, bool bDistribution, bool bSkipCook)
        {
            if (GlobalParameters == null)
            {
                Logger.WError("Global parameters is null!");
                return -1;
            }

            if(string.IsNullOrEmpty(BuildConfiguration) ||
                string.IsNullOrEmpty(Platform) ||
                string.IsNullOrEmpty(WorkspaceRoot))
            {
                Logger.WError("Parameters is null!");
                return -1;
            }

            JenkinsJobInfo JobInfo = Config.Get().FindJenkinsJob(GlobalParameters.JenkinsJobName);

            if (JobInfo != null)
            {
                string ResolvedBatchFile = null;
                string ResolvedPlatform = null;

                if (Platform.Equals("IOS"))
                {
                    ResolvedBatchFile = FileHelper.BuildStandaloneBat;
                    ResolvedPlatform = "IOS";
                }
                else if (Platform.Equals("Win64Server"))
                {
                    ResolvedBatchFile = FileHelper.BuildDedicatedServerBat;
                    ResolvedPlatform = "Win64";
                }
                else if (Platform.Equals("Win64"))
                {
                    ResolvedBatchFile = FileHelper.BuildStandaloneBat;
                    ResolvedPlatform = "Win64";
                }
                else if (Platform.Equals("Android"))
                {
                    ResolvedBatchFile = FileHelper.BuildStandaloneBat;
                    ResolvedPlatform = "Android";

                    // TODO: Config
                   // AdditionalCommandline += "-cookflavor=ETC2 ";
                    //
                }
                else if (Platform.Equals("Win32Server"))
                {
                    ResolvedBatchFile = FileHelper.BuildDedicatedServerBat;
                    ResolvedPlatform = "Win32";
                }
                else if (Platform.Equals("Win32"))
                {
                    ResolvedBatchFile = FileHelper.BuildStandaloneBat;
                    ResolvedPlatform = "Win32";
                }
                else
                {
                    Logger.WError("Unknown platform.");
                    return -1;
                }

                if (!File.Exists(Path.Combine(FileHelper.BatchFilesPath, ResolvedBatchFile)))
                {
                    Logger.WError("Error while Build: Build.bat does not exist.");
                    return -1;
                }

                string BuildsRoot = Path.Combine(WorkspaceRoot, "Builds");

                // Get current revision and author
                SCMCommitInfo CurrentRevisionInfo = SCM.GetCurrentRevisionInfo(JobInfo.RepositoryRootDirectory);
                //

                string TargetDirectory = Path.Combine(BuildsRoot, GlobalParameters.BuildTag +
                    (CurrentRevisionInfo != null ? ("-by-" + CurrentRevisionInfo.Instigator + "-" + CurrentRevisionInfo.Revision) : ""));

                if (!Directory.Exists(TargetDirectory))
                {
                    Directory.CreateDirectory(TargetDirectory);
                }

                // Configure additional commandline
                string AdditionalCommandline = ConfigureIni(GlobalParameters, JobInfo, WorkspaceRoot, TargetDirectory);

                if (bFullRebuild)
                {
                    AdditionalCommandline += "-clean ";
                }

                if (bDistribution && !AdditionalCommandline.Contains("-distribution"))
                {
                    AdditionalCommandline += "-distribution ";
                }

                if(bSkipCook)
                {
                    AdditionalCommandline += "-skipcook -skipstage ";
                }
                //

                string AbsoluteBatchPath = Path.GetFullPath(Path.Combine(FileHelper.BatchFilesPath, ResolvedBatchFile));

                Logger.StdOut("Building with batch file: " + AbsoluteBatchPath);

                if (File.Exists(AbsoluteBatchPath))
                {
                    string Commandline = JobInfo.EngineDirectory + " " +
                        JobInfo.GetProjectDirectory(GlobalParameters.ProjectOverride) + " " +
                        TargetDirectory + " " +
                        (GlobalParameters.ProjectOverride == null ? JobInfo.DefaultProjectName : GlobalParameters.ProjectOverride) + " " +
                        ResolvedPlatform + " " +
                        BuildConfiguration + " " +
                        "\"" + AdditionalCommandline + "\"";

                    Console.WriteLine("Commandline was initialized: '" + Commandline + "'");

                    Process BuildProcess = new Process();
                    ProcessStartInfo StartInfo = new ProcessStartInfo("cmd.exe", @"/C " + AbsoluteBatchPath + " " + Commandline);

                    StartInfo.RedirectStandardOutput = true;
                    StartInfo.UseShellExecute = false;
                    BuildProcess.StartInfo = StartInfo;

                    BuildProcess.OutputDataReceived += delegate (object SendingProcess, DataReceivedEventArgs OutLine)
                    {
                        if (OutLine.Data != null)
                        {
                            Logger.WriteLine("UE4: " + OutLine.Data);
                        }
                    };

                    BuildProcess.Start();
                    BuildProcess.BeginOutputReadLine();
                    BuildProcess.WaitForExit();

                    if(BuildProcess.ExitCode != 0)
                    {
                        Logger.WError("Error while Building.");

                        if (Directory.Exists(TargetDirectory))
                        {
                            Directory.Delete(TargetDirectory);
                        }

                        return -1;
                    }

                    if (Directory.Exists(TargetDirectory))
                    {
                        string[] Directories = Directory.GetDirectories(TargetDirectory);
                        if (Directories.Length < 1) // TODO: test this
                        {
                            Logger.WError("Output files does not exists.");

                            if (Directory.Exists(TargetDirectory))
                            {
                                Directory.Delete(TargetDirectory);
                            }

                            return -1;
                        }
                    }

                    // Save build path to cache
                    Serialization.PackagedBuildInfo PackagedBuildCache = Serialization.PackagedBuildInfo.Load<Serialization.PackagedBuildInfo>(GlobalParameters.JenkinsJobName);

                    if (PackagedBuildCache == null)
                    {
                        PackagedBuildCache = new Serialization.PackagedBuildInfo();
                    }

                    if (PackagedBuildCache != null)
                    {
                        PackagedBuildCache.PackagedBuildPath = TargetDirectory;

                        PackagedBuildCache.Save(GlobalParameters.JenkinsJobName);
                    }
                    //

                    Logger.WriteLine("Step Build completed successfully.");

                    return 0;
                }
            }

            return -1;
        }

        public int UploadBuild(GlobalParametersInfo GlobalParameters, string UploaderJobName)
        {
            Serialization.PackagedBuildInfo PackagedBuildCache = Serialization.PackagedBuildInfo.Load<Serialization.PackagedBuildInfo>(GlobalParameters.JenkinsJobName);
            JenkinsJobInfo JobInfo = Config.Get().FindJenkinsJob(GlobalParameters.JenkinsJobName);

            if (PackagedBuildCache != null && JobInfo != null && !string.IsNullOrEmpty(UploaderJobName))
            {
                string IOSBuildPath = Path.Combine(PackagedBuildCache.PackagedBuildPath, "IOS");
                string BuildDirectoryName = PackagedBuildCache.ExtractBuildDirectoryName();

                if(string.IsNullOrEmpty(BuildDirectoryName))
                {
                    Logger.WError("Build Directory Name is null.");
                    return -1;
                }

                string FileURL = null;

                if (Directory.Exists(IOSBuildPath))
                {
                    string[] BuildFiles = Directory.GetFiles(IOSBuildPath, "*.ipa");
                    
                    if(BuildFiles.Length > 0)
                    {
                        string IPAFilePath = Path.GetFileName(BuildFiles[0]);

                        FileURL = Config.Get().JenkinsServerURL + "/job/" + GlobalParameters.JenkinsJobName +
                            "/ws/Builds/" + BuildDirectoryName + "/IOS/" + IPAFilePath;
                    }
                }
                else
                {
                    // TODO: Add other platforms
                }

                if (!string.IsNullOrEmpty(FileURL))
                {
                    List<KeyValuePair<string, string>> AdditionalArguments = new List<KeyValuePair<string, string>>();

                    AdditionalArguments.Add(new KeyValuePair<string, string>("FileLink", FileURL));

                    Logger.WriteLine("Launching Uploader job with file: " + FileURL);

                    HTTPClient.TriggerJenkinsParametrizedBuild(UploaderJobName, JobInfo.LaunchToken, AdditionalArguments);

                    return 0;
                }
                else
                {
                    Logger.WError("Can't launch Uploader job because file link is empty");
                }
            }

            return -1;
        }

        public int Cleanup(GlobalParametersInfo GlobalParameters, string WorkspaceRoot)
        {
            if(GlobalParameters == null)
            {
                Logger.WError("Global parameters is null!");
                return -1;
            }

            // Revert ini without checking out
            if (SCM.SupportsCheckout())
            {
                JenkinsJobInfo JobInfo = Config.Get().FindJenkinsJob(GlobalParameters.JenkinsJobName);

                if (JobInfo != null)
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(Tools.BuildIniPath("", JobInfo.GetProjectDirectory(GlobalParameters.ProjectOverride)));
                    FileInfo[] Files = DirInfo.GetFiles("*.backup");

                    foreach (FileInfo Entry in Files)
                    {
                        string WorkingCopyFile = Entry.FullName.Replace(".backup", "");

                        if (File.Exists(WorkingCopyFile))
                        {
                            Logger.WriteLine("Reverting backed up ini: " + Entry.ToString() + " to " + WorkingCopyFile);

                            File.Delete(WorkingCopyFile);

                            File.Move(Entry.FullName, WorkingCopyFile);
                        }
                    }
                }
            }
            //

            Serialization.JenkinsJobCache JobCache = new Serialization.JenkinsJobCache();

            Logger.WriteLine("Cleaning cache...");

            if (JobCache != null)
            {
                JobCache.RemoveFile(GlobalParameters.BuildTag);
            }

            if(!string.IsNullOrEmpty(WorkspaceRoot))
            {
                // Clear ini cache
                string PathToConfigOverride = Path.Combine(WorkspaceRoot, "Upload", "Config");

                // Override Ini
                if (Directory.Exists(PathToConfigOverride))
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(PathToConfigOverride);
                    FileInfo[] Files = DirInfo.GetFiles("*.ini");

                    foreach (FileInfo Entry in Files)
                    {
                        string FilePath = Path.Combine(PathToConfigOverride, Entry.Name);

                        Console.WriteLine("\t Deleting ini" + FilePath);

                        File.Delete(FilePath);
                    }
                }
                //
            }

            return 0;
        }
    }
}
