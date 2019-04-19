using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UE4BuildHelper
{
    [Serializable]
    public class SCMCommitInfo
    {
        public enum EBuildAction
        {
            EBA_Build,
            EBA_Rebuild,

            EBA_Package,

            EBA_Count
        };

        public enum EBuildSubAction
        {
            EBSA_OnlyProject,
            EBSA_OnlyEngine,
            EBSA_All,

            EBSA_IOS,
            EBSA_Android,
            EBSA_Win64,

            EBSA_Count
        };

        public class ModifiedFileInfo
        {
            public string Action = "";
            public string RelativePath = "";
        }

        public int Revision = -1;
        public string Instigator = "";
        public string Comment { get; internal set; } = "";

        public List<ModifiedFileInfo> ModifiedFiles { get; internal set; }  = null;

        public EBuildAction BuildAction = EBuildAction.EBA_Count;
        public EBuildSubAction BuildSubAction = EBuildSubAction.EBSA_All;

        public string ProjectOverride = null;
        public bool bShouldUpload = false;


        public bool Init(int InRevision, string InInstigator ,string InComment)
        {
            if(InRevision <= 0)
            {
                Logger.WriteLine("SCM: Error: Can't init commit info with revision: " + InRevision);

                return false;
            }

            Revision = InRevision;
            Instigator = InInstigator;
            Comment = InComment;

            InitBuildActions();

            Logger.WriteLine("SCM: Commit info: " + InRevision + " by " + InInstigator + " with comment " + InComment, ELogLevel.VeryVerbose);
            Logger.WriteLine("SCM: Build actions: " + BuildAction + "," + BuildSubAction + " Project Override: " + ProjectOverride, ELogLevel.VeryVerbose);

            return true;
        }

        public void AppendModifiedFiles(List<ModifiedFileInfo> InFiles)
        {
            if(ModifiedFiles == null)
            {
                ModifiedFiles = InFiles;
            }
            else
            {
                ModifiedFiles.AddRange(InFiles);
            }
        }

        public override string ToString()
        {
            return "SCM: Commit info: " + Revision + " by " + Instigator + " with comment " + Comment + "\n" +
                "SCM: Build actions: " + BuildAction + "," + BuildSubAction;
        }

        bool InitBuildActions()
        {
            if (Comment.Length == 0)
            {
                BuildAction = EBuildAction.EBA_Count;
                BuildSubAction = EBuildSubAction.EBSA_Count;

                return false;
            }

            Logger.WriteLine("SCM: Getting build actions...", ELogLevel.VeryVerbose);

            try
            {
                int StartTagIndex = Comment.IndexOf('[');
                int EndTagIndex = Comment.IndexOf(']');

                if (StartTagIndex >= 0 && EndTagIndex >= 0)
                {
                    int AdjustStartTagIndex = StartTagIndex + 1;

                    string FullBuildTag = Comment.Substring(AdjustStartTagIndex, EndTagIndex - AdjustStartTagIndex);

                    char[] MajorDelims = { '+' };
                    char[] MinorDelims = { '|' };

                    string[] MajorTokens = FullBuildTag.Split(MajorDelims);

                    if (MajorTokens.Length > 0) // Build tag section
                    {
                        string BuildTags = MajorTokens[0];

                        string[] Tokens = BuildTags.Split(MinorDelims);

                        BuildAction = EBuildAction.EBA_Count;
                        BuildSubAction = EBuildSubAction.EBSA_All;

                        if (Tokens.Length > 0)  // Build Action
                        {
                            string LowerToken = Tokens[0].ToLower();

                            // DLL
                            if (LowerToken == "build")
                            {
                                BuildAction = EBuildAction.EBA_Build;
                            }
                            else if (LowerToken == "rebuild")
                            {
                                BuildAction = EBuildAction.EBA_Rebuild;
                            }
                            else if (LowerToken == "package")
                            {
                                BuildAction = EBuildAction.EBA_Package;
                            }
                        }

                        if (Tokens.Length > 1)  // Build Sub Action
                        {
                            string LowerToken = Tokens[1].ToLower();

                            // DLL
                            if (LowerToken == "project")
                            {
                                BuildSubAction = EBuildSubAction.EBSA_OnlyProject;
                            }
                            else if (LowerToken == "engine")
                            {
                                BuildSubAction = EBuildSubAction.EBSA_OnlyEngine;
                            }
                            // Package
                            else if (LowerToken == "ios")
                            {
                                BuildSubAction = EBuildSubAction.EBSA_IOS;
                            }
                            else if (LowerToken == "android")
                            {
                                BuildSubAction = EBuildSubAction.EBSA_Android;
                            }
                            else if (LowerToken == "win")
                            {
                                BuildSubAction = EBuildSubAction.EBSA_Win64;
                            }

                        }
                    }

                    if (MajorTokens.Length > 1) // Options section
                    {
                        string Options = MajorTokens[1];

                        string[] Tokens = Options.Split(MinorDelims);

                        if (Tokens.Length > 0)  // Project override
                        {
                            if (Tokens.Contains("upload") || Tokens.Contains("Upload"))
                            {
                                bShouldUpload = true;
                            }
                            else
                            {
                                ProjectOverride = Tokens[0];
                            }
                        }
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine("EXCEPTION While parsing build actions: " + e);
            }

            BuildAction = EBuildAction.EBA_Count;
            BuildSubAction = EBuildSubAction.EBSA_Count;

            return false;
        }

        public bool HasAnyActions()
        {
            return BuildAction != EBuildAction.EBA_Count;
        }

        public bool IsDLLBuild()
        {
            return BuildSubAction == EBuildSubAction.EBSA_OnlyProject || BuildSubAction == EBuildSubAction.EBSA_OnlyEngine ||
                BuildSubAction == EBuildSubAction.EBSA_All;
        }

        public bool IsPackage()
        {
            return BuildSubAction == EBuildSubAction.EBSA_IOS || BuildSubAction == EBuildSubAction.EBSA_Android ||
               BuildSubAction == EBuildSubAction.EBSA_Win64;
        }

        public bool MatchProject(string InProjectName)
        {
            return InProjectName == ProjectOverride;
        }
    }

    class SCMClientBase
    {
        protected virtual string GetGOpts(bool bForceLog = false)
        {
            return "";
        }

        protected virtual bool ParseCommitInfosFromRawData(List<String> DataLines, out List<SCMCommitInfo> OutCommitInfos)
        {
            OutCommitInfos = null;

            return false;
        }

        protected virtual string CombineArrayToCmd(string[] Args, bool bCheckDirectories = false)
        {
            string Cmd = "";

            foreach (string Arg in Args)
            {
                if (bCheckDirectories && !Directory.Exists(Arg))
                {
                    continue;
                }

                Cmd += Arg + " ";
            }

            return Cmd;
        }

        /** 
            Revision - Min Revision [1;inf]
            ToRevision - Max Revision [0;inf]
                0 - Head revision
             */
        public virtual bool GetLog(int Revision, int ToRevision, string RepositoryRootDirectory, out List<SCMCommitInfo> OutCommitInfos, bool bVerbose = false)
        {
            OutCommitInfos = null;

            return false;
        }

        public virtual SCMCommitInfo GetCurrentRevision(string RepositoryRootDirectory)
        {
            return null;
        }

        public virtual SCMCommitInfo GetLatestRevision(string RepositoryRootDirectory)
        {
            return null;
        }

        public virtual bool ParseCommitInfo(string InData, out int OutRevision, out string OutInstigator)
        {
            OutRevision = -1;
            OutInstigator = "";

            return false;
        }

        public virtual bool UpdateToTheLatestRevision(string[] InDirectories)
        {
            return false;
        }

        public virtual bool ResolveConflicts(bool bTheirs)
        {
            return false;
        }

        public virtual bool CheckoutFiles(string[] InDirectories, string[] InExtensions)
        {
            return false;
        }

        public virtual bool CheckoutSpecific(string[] InFiles)
        {
            return false;
        }

        public virtual bool RevertFiles(string[] InDirectories, bool bUnchangedOnly)
        {
            return false;
        }

        public virtual bool CommitFiles(string[] InDirectories, string Comment, bool bRevertUnchanged)
        {
            return false;
        }

        public virtual bool HasPendingChanges()
        {
            return false;
        }

        public virtual string GetBinariesTypeCommitDescription(SCMCommitInfo.EBuildSubAction BuildSubAction)
        {
            return "";
        }

        public virtual bool SupprotsPartialUpdate()
        {
            return true;
        }

        public virtual bool SupportsCheckout()
        {
            return false;
        }

        public virtual bool WriteChangelogToXML(string Filename, List<SCMCommitInfo> InCommits)
        {
            return false;
        }
    }

    class SCMProcessor
    {
        private SCMClientBase ClientInstance = null;
        public ESCMType SCMType { get; internal set; } = ESCMType.None;

        public void Init(SCMInfo SCMData, ESCMType InSCMType, string WorkspaceName)
        {
            SCMType = InSCMType;

            switch (InSCMType)
            {
                case ESCMType.Perforce:
                    ClientInstance = new SCM.SCMPerforceClient(SCMData, WorkspaceName);
                    break;
                case ESCMType.SVN:
                    ClientInstance = new SCM.SCMSVNClient();
                    break;
            }
        }

        public bool ParseChangelist(int Revision, int ToRevision, string RepositoryRootDirectory, out List<SCMCommitInfo> OutCommits, bool bVerbose = false)
        {
            OutCommits = null;

            if (ClientInstance != null)
            {
                return ClientInstance.GetLog(Revision, ToRevision, RepositoryRootDirectory, out OutCommits, bVerbose);   
            }

            return false;
        }

        public SCMCommitInfo GetCurrentRevisionInfo(string RepositoryRootDirectory)
        {
            if (ClientInstance != null)
            {
                return ClientInstance.GetCurrentRevision(RepositoryRootDirectory);
            }

            return null;
        }

        public SCMCommitInfo GetLatestRevisionInfo(string RepositoryRootDirectory)
        {
            if (ClientInstance != null)
            {
                return ClientInstance.GetLatestRevision(RepositoryRootDirectory);
            }

            return null;
        }

        public bool UpdateToTheLatestRevision(string EngineRoot, string ProjectRoot, string RepositoryRoot)
        {
            if (ClientInstance != null)
            {
                List<string> DirectoriesToUpdate = new List<string>();

                if(!ClientInstance.SupprotsPartialUpdate() && RepositoryRoot != null && Directory.Exists(RepositoryRoot))
                {
                    DirectoriesToUpdate.Add(RepositoryRoot);
                }
                else if(Directory.Exists(ProjectRoot) && Directory.Exists(EngineRoot))
                {
                    DirectoriesToUpdate.AddRange(new List<string>(){ EngineRoot, ProjectRoot });
                }
                else
                {
                    return false;
                }

                return ClientInstance.UpdateToTheLatestRevision(DirectoriesToUpdate.ToArray());       
            }

            return false;
        }

        public bool ResolveConflicts(string EngineRoot, string ProjectRoot, bool bTheirs = true)
        {
            if (ClientInstance != null)
            {
                return ClientInstance.ResolveConflicts(bTheirs);
            }

            return false;
        }

        public bool CheckoutBinaries(string EngineRoot, string ProjectRoot, string[] InExtensions)
        {
            if (ClientInstance != null)
            {
                // Directories that contains binaries
                string[] DirectoriesToCheckout = {
                    Path.Combine(ProjectRoot, "Binaries", "Win64"),
                    Path.Combine(ProjectRoot, "Plugins", "..."),

                    Path.Combine(EngineRoot, "Binaries", "Win64"),
                    Path.Combine(EngineRoot, "Binaries", "Win64", "Android"),
                    Path.Combine(EngineRoot, "Binaries", "Win64", "IOS"),
                    Path.Combine(EngineRoot, "Binaries", "Win64", "HTML5"),
                    Path.Combine(EngineRoot, "Binaries", "DotNET"),
                    Path.Combine(EngineRoot, "Binaries", "DotNET", "IOS"),
                    Path.Combine(EngineRoot, "Plugins", "...")
                };
                //

                return ClientInstance.CheckoutFiles(DirectoriesToCheckout, InExtensions);
            }

            return false;
        }

        public bool CheckoutSpecificFiles(string[] Files)
        {
            if (ClientInstance != null)
            {
                return ClientInstance.CheckoutSpecific(Files);
            }

            return false;
        }

        public bool RevertFiles(string EngineRoot, string ProjectRoot, bool bUnchangedOnly)
        {
            if (ClientInstance != null && Directory.Exists(ProjectRoot) && Directory.Exists(EngineRoot))
            {
                string[] DirectoriesToRevert = { EngineRoot, ProjectRoot };

                return ClientInstance.RevertFiles(DirectoriesToRevert, bUnchangedOnly);
            }

            return false;
        }

        public bool CommitBinaries(string EngineRoot, string ProjectRoot, SCMCommitInfo ResolvedCommitInfo, bool bRevertUnchanged)
        {
            if(ClientInstance != null && Directory.Exists(ProjectRoot) && Directory.Exists(EngineRoot) && ResolvedCommitInfo != null)
            {
                if (!bRevertUnchanged && !ClientInstance.HasPendingChanges())
                {
                    Logger.WriteLine("Nothing to commit.");

                    return true;
                }

                // Directories that contains binaries
                string ProjectBinariesDir = Path.Combine(ProjectRoot, "Binaries", "Win64");
                string ProjectPluginsDir = Path.Combine(ProjectRoot, "Plugins");

                string EngineBinariesDir = Path.Combine(EngineRoot, "Binaries", "Win64");
                string EnginePluginsDir = Path.Combine(EngineRoot, "Plugins");

                List<string> DirectoriesToSubmit = new List<string>();
                string BinariesTypeDesctiption = ClientInstance.GetBinariesTypeCommitDescription(ResolvedCommitInfo.BuildSubAction);

                if (ResolvedCommitInfo.BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_All || 
                    ResolvedCommitInfo.BuildAction == SCMCommitInfo.EBuildAction.EBA_Rebuild)   // Forcing full commit if this is full rebuild
                {
                    DirectoriesToSubmit.Add(EngineBinariesDir);
                    DirectoriesToSubmit.Add(EnginePluginsDir);

                    DirectoriesToSubmit.Add(ProjectBinariesDir);
                    DirectoriesToSubmit.Add(ProjectPluginsDir);
                }
                else if (ResolvedCommitInfo.BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_OnlyProject)
                {
                    DirectoriesToSubmit.Add(ProjectBinariesDir);
                    DirectoriesToSubmit.Add(ProjectPluginsDir);
                }
                else if (ResolvedCommitInfo.BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_OnlyEngine)
                {
                    DirectoriesToSubmit.Add(EngineBinariesDir);
                    DirectoriesToSubmit.Add(EnginePluginsDir);
                }

                string BuildType = "Unknown";
                if(ResolvedCommitInfo.BuildAction == SCMCommitInfo.EBuildAction.EBA_Build)
                {
                    BuildType = "Build";
                }
                else if (ResolvedCommitInfo.BuildAction == SCMCommitInfo.EBuildAction.EBA_Rebuild)
                {
                    BuildType = "Full Rebuild";
                }

                string Comment = "[BUILDMACHINE COMMIT] | -Reason: Binaries " + BuildType + ". | -Commit filter: " + BinariesTypeDesctiption + " | -Build instigator: " +
                    ResolvedCommitInfo.Instigator + " | -Update revision: " + ResolvedCommitInfo.Revision;

                return ClientInstance.CommitFiles(DirectoriesToSubmit.ToArray(), Comment, bRevertUnchanged);
            }

            return false;
        }

        public SCMCommitInfo ResolveCommonCommitInfo(List<SCMCommitInfo> CommitsList, bool bApplyHeadRevision, string ProjectFilter)
        {
            SCMCommitInfo ResolvedCommitInfo = null;

            if (CommitsList != null)
            {
                // Sort by revisions
                CommitsList.Sort(delegate (SCMCommitInfo Left, SCMCommitInfo Right)
                {
                    return Left.Revision.CompareTo(Right.Revision);
                });
                //

                foreach (SCMCommitInfo CommitInfo in CommitsList)
                {
                    if (CommitInfo != null && CommitInfo.HasAnyActions() && CommitInfo.MatchProject(ProjectFilter))
                    {
                        if (ResolvedCommitInfo == null)
                        {
                            ResolvedCommitInfo = CommitInfo;
                        }

                        if (CommitInfo.BuildAction > ResolvedCommitInfo.BuildAction)
                        {
                            ResolvedCommitInfo.BuildAction = CommitInfo.BuildAction;
                        }

                        if ((ResolvedCommitInfo.BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_OnlyProject &&
                            CommitInfo.BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_OnlyEngine) ||
                            (ResolvedCommitInfo.BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_OnlyEngine &&
                            CommitInfo.BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_OnlyProject))
                        {
                            ResolvedCommitInfo.BuildSubAction = SCMCommitInfo.EBuildSubAction.EBSA_All;
                        }

                        if (CommitInfo.BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_All)
                        {
                            ResolvedCommitInfo.BuildSubAction = SCMCommitInfo.EBuildSubAction.EBSA_All;
                        }
                    }
                    else if (CommitInfo == null)
                    {
                        Console.WriteLine("SCM: CommitInfo is null!");
                    }
                }

                if(bApplyHeadRevision && CommitsList.Count > 0 && ResolvedCommitInfo != null)
                {
                    ResolvedCommitInfo.Revision = CommitsList[CommitsList.Count - 1].Revision;
                }
            }

            return ResolvedCommitInfo;
        }

        public bool SupportsCheckout()
        {
            if(ClientInstance != null)
            {
                return ClientInstance.SupportsCheckout();
            }

            return false;
        }

        public bool WriteChangelogToXML(string Filename, List<SCMCommitInfo> InCommits)
        {
            if (ClientInstance != null)
            {
                return ClientInstance.WriteChangelogToXML(Filename, InCommits);
            }

            return false;
        }
    }
}
