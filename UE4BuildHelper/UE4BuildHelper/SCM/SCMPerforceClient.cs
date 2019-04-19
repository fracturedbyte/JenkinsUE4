using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UE4BuildHelper.SCM
{
    class SCMPerforceClient : SCMClientBase
    {
        const string WholeDirectorySuffix = "...";

        SCMInfo SCMData;
        string WorkspaceName = null;

        private SCMPerforceClient()
        {

        }

        public SCMPerforceClient(SCMInfo InSCMData, string InWorkspaceName)
        {
            SCMData = InSCMData;
            WorkspaceName = InWorkspaceName;
        }

        private static bool P4JobNameToBuildAction(List<string> JobNames, out SCMCommitInfo.EBuildAction OutBuildAction, out SCMCommitInfo.EBuildSubAction OutBuildSubAction)
        {
            OutBuildAction = SCMCommitInfo.EBuildAction.EBA_Count;
            OutBuildSubAction = SCMCommitInfo.EBuildSubAction.EBSA_All;

            // Main Action
            if (JobNames.Contains("Build", StringComparer.OrdinalIgnoreCase))
            {
                OutBuildAction = SCMCommitInfo.EBuildAction.EBA_Build;
            }
            else if (JobNames.Contains("Rebuild", StringComparer.OrdinalIgnoreCase))
            {
                OutBuildAction = SCMCommitInfo.EBuildAction.EBA_Rebuild;
            }
            else if (JobNames.Contains("PackageOnly", StringComparer.OrdinalIgnoreCase))
            {
                OutBuildAction = SCMCommitInfo.EBuildAction.EBA_Package;
            }

            // Sub Action
            bool bShouldPerformPackage = false;
            if (JobNames.Contains("PackageWin64", StringComparer.OrdinalIgnoreCase))
            {
                OutBuildSubAction = SCMCommitInfo.EBuildSubAction.EBSA_Win64;

                bShouldPerformPackage = true;
            }
            else if (JobNames.Contains("PackageWinServer", StringComparer.OrdinalIgnoreCase))
            {
                // TODO: Server
            }
            else if (JobNames.Contains("PackageIOS", StringComparer.OrdinalIgnoreCase))
            {
                OutBuildSubAction = SCMCommitInfo.EBuildSubAction.EBSA_IOS;

                bShouldPerformPackage = true;
            }
            else if (JobNames.Contains("PackageAndroid", StringComparer.OrdinalIgnoreCase))
            {
                OutBuildSubAction = SCMCommitInfo.EBuildSubAction.EBSA_Android;

                bShouldPerformPackage = true;
            }

            // Set build by default
            if(OutBuildAction == SCMCommitInfo.EBuildAction.EBA_Count && bShouldPerformPackage)
            {
                OutBuildAction = SCMCommitInfo.EBuildAction.EBA_Build;
            }

            return OutBuildAction != SCMCommitInfo.EBuildAction.EBA_Count;
        }

        protected override string GetGOpts(bool bForceLog = false)
        {
            if (SCMData == null ||
                WorkspaceName == null ||
                SCMData.Credentials.UserName == null ||
                SCMData.Credentials.GetPassword() == null ||
                SCMData.ServerAddress == null)
            {
                return "";
            }

            string GlobalOptions = "p4 " +
                "-c " + WorkspaceName +
                " -u " + SCMData.Credentials.UserName +
                " -P " + SCMData.Credentials.GetPassword().ToUpper() +
                " -p " + SCMData.ServerAddress;

            if (!bForceLog && Logger.GetLogLevel() < ELogLevel.VeryVerbose)
            {
                GlobalOptions += " -q"; // Suppress logs 
            }

            return GlobalOptions;
        }

        private string[] AppendSuffix(string[] InDirectories, string Suffix, bool bCheckDir = false)
        {
            List<string> OutDirectories = new List<string>();

            foreach (string Dir in InDirectories)
            {
                if (bCheckDir && !Directory.Exists(Dir))
                {
                    continue;
                }

                OutDirectories.Add(Path.Combine(Dir, Suffix));
            }

            return OutDirectories.ToArray();
        }

        protected override bool ParseCommitInfosFromRawData(List<String> DataLines, out List<SCMCommitInfo> OutCommitInfos)
        {
            OutCommitInfos = null;

            if (DataLines != null)
            {
                List<String> CommitLines = new List<String>();
                OutCommitInfos = new List<SCMCommitInfo>();

                for (int i = 0; i < DataLines.Count; i++)
                {
                    string Str = DataLines[i];

                    if (Str != null && Str.Length > 0 && Str.StartsWith("change", StringComparison.OrdinalIgnoreCase))
                    {
                        for (int j = i + 1; j < DataLines.Count; j++)
                        {
                            string SubStr = DataLines[j];
                            if (SubStr.StartsWith("change", StringComparison.OrdinalIgnoreCase))
                            {
                                i = j - 1;
                                break;
                            }
                            else if (SubStr != null && SubStr.Length > 0)
                            {
                                CommitLines.Add(SubStr);
                            }
                        }

                        String Data = Str;
                        String Comment = "";

                        for (int j = 0; j < CommitLines.Count; j++)
                        {
                            if (CommitLines[j].Length > 0)
                            {
                                Comment += CommitLines[j] + "\n";
                            }
                        }

                        SCMCommitInfo CommitInfo = new SCMCommitInfo();

                        int ParsedRevision = -1;
                        string Instigator = null;
                        if (ParseCommitInfo(Data, out ParsedRevision, out Instigator) && CommitInfo.Init(ParsedRevision, Instigator, Comment))
                        {
                            OutCommitInfos.Add(CommitInfo);
                        }

                        CommitLines.Clear();
                    }
                }
                CommitLines.Clear();

                return true;
            }

            return false;
        }

        private bool ParseFixes(List<string> DataLines, List<SCMCommitInfo> CommitInfos)
        {
            Dictionary<int, List<string>> JobsByRevisions = new Dictionary<int, List<string>>();

            foreach (string Line in DataLines)
            {
                char[] Delims = { ' ' };

                string[] Tokens = Line.Split(Delims);

                string JobName = "";
                int Revision = -1;

                if (Tokens.Length > 1)
                {
                    JobName = Tokens[0];
                }

                if(Tokens.Length >= 5 && Tokens[3].Equals("change", StringComparison.OrdinalIgnoreCase))
                {
                    if(int.TryParse(Tokens[4], out Revision))
                    {
                        if(!JobsByRevisions.ContainsKey(Revision))
                        {
                            JobsByRevisions[Revision] = new List<string>();
                        }

                        JobsByRevisions[Revision].Add(JobName);
                    }
                }
            }

            bool bReturnResult = false;

            foreach (var Elem in JobsByRevisions)
            {
                SCMCommitInfo CommitInfo = CommitInfos.Find(info => info.Revision == Elem.Key);

                if(CommitInfo != null && Elem.Value != null)
                {
                    SCMCommitInfo.EBuildAction OutBuildAction;
                    SCMCommitInfo.EBuildSubAction OutBuildSubAction;
                    bool bResult = P4JobNameToBuildAction(Elem.Value, out OutBuildAction, out OutBuildSubAction);

                    if(bResult)
                    {
                        // Fully override already existing actions
                        CommitInfo.BuildAction = OutBuildAction;
                        CommitInfo.BuildSubAction = OutBuildSubAction;

                        if(Elem.Value.Contains("Upload", StringComparer.OrdinalIgnoreCase))
                        {
                            CommitInfo.bShouldUpload = true;
                        }

                        bReturnResult = true;
                    }
                }
            }

            return bReturnResult;
        }

        public override bool GetLog(int Revision, int ToRevision, string RepositoryRootDirectory, out List<SCMCommitInfo> OutCommitInfos, bool bVerbose = false)
        {
            OutCommitInfos = null;

            if (Revision <= 0 || ToRevision < 0)
            {
                return false;
            }

            string RootDirectory = Path.Combine(RepositoryRootDirectory, "...");

            if (!Directory.Exists(RootDirectory))
            {
                return false;
            }

            int ExitCode = -1;

            string RevisionRangeString = RootDirectory + "@" + Revision + "," +
                (ToRevision >= Revision ? ToRevision.ToString() : "#head");

            List<String> DataLines = Tools.ExecuteCommand(GetGOpts(true) + " changes -s submitted -l " + RevisionRangeString, out ExitCode, true);

            if (ParseCommitInfosFromRawData(DataLines, out OutCommitInfos))
            {
                if(ExitCode == 0)
                {
                    DataLines.Clear();
                    DataLines = Tools.ExecuteCommand(GetGOpts(true) + " fixes " + RevisionRangeString, out ExitCode, true);

                    ParseFixes(DataLines, OutCommitInfos);
                }

                return ExitCode == 0;
            }

            return false;
        }

        private SCMCommitInfo GetRevisionInfoInternal(string RepositoryRootDirectory, string RevisionTag)
        {
            string RootDirectory = Path.Combine(RepositoryRootDirectory, "...");

            if (!Directory.Exists(RootDirectory) || string.IsNullOrEmpty(RevisionTag))
            {
                return null;
            }

            List<SCMCommitInfo> CommitInfos;

            int ExitCode = -1;
            List<String> DataLines = Tools.ExecuteCommand(GetGOpts(true) + " changes -m1 " + RootDirectory + RevisionTag, out ExitCode, true);

            if (ExitCode == 0 &&
                ParseCommitInfosFromRawData(DataLines, out CommitInfos) &&
                CommitInfos != null && CommitInfos.Count > 0)
            {
                return CommitInfos[0];
            }

            return null;
        }

        public override SCMCommitInfo GetCurrentRevision(string RepositoryRootDirectory)
        {
            return GetRevisionInfoInternal(RepositoryRootDirectory, "#have");
        }

        public override SCMCommitInfo GetLatestRevision(string RepositoryRootDirectory)
        {
            return GetRevisionInfoInternal(RepositoryRootDirectory, "#head");
        }

        public override bool ParseCommitInfo(string InData, out int OutRevision, out string OutInstigator)
        {
            OutRevision = -1;
            OutInstigator = "";

            try
            {
                char[] DelimiterChars = { ' ' };
                string[] Tokens = InData.Split(DelimiterChars);

                for (int i = 0; i < Tokens.Length; i++)
                {
                    if (Tokens[i].Equals("Change", StringComparison.OrdinalIgnoreCase) && Tokens.Length > i + 1)
                    {
                        OutRevision = Int32.Parse(Tokens[i + 1]);
                    }
                    if (Tokens[i].Equals("by", StringComparison.OrdinalIgnoreCase) && Tokens.Length > i + 1)
                    {
                        string Instigator = Tokens[i + 1];

                        int SeparatorIndex = Instigator.IndexOf("@");

                        if (SeparatorIndex >= 0 && SeparatorIndex < Instigator.Length)
                        {
                            Instigator = Instigator.Remove(SeparatorIndex, Instigator.Length - SeparatorIndex);
                        }

                        OutInstigator = Instigator;
                    }

                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION While parsing SVN Commit: " + e);
            }

            return false;
        }

        public override bool UpdateToTheLatestRevision(string[] InDirectories)
        {
            if (InDirectories.Length > 0)
            {
                string DirectoriesArgs = CombineArrayToCmd(AppendSuffix(InDirectories, WholeDirectorySuffix), true);

                int ExitCode = -1;
                List<String> DataLines = Tools.ExecuteCommand(GetGOpts() + " sync " + DirectoriesArgs, out ExitCode, true);

                return ExitCode == 0;
            }

            return false;
        }

        public override bool ResolveConflicts(bool bTheirs)
        {
            int ExitCode = -1;

            string ResolveOptions = bTheirs ? "-at" : "-am";

            Tools.ExecuteCommand(GetGOpts() + " resolve " + ResolveOptions, out ExitCode, true);

            return ExitCode == 0;
        }

        public override bool CheckoutFiles(string[] InDirectories, string[] InExtensions)
        {
            if (InDirectories.Length > 0)
            {
                // Combine all extensions with all directories that contains binaries
                List<string> DirectoriesWithExtensions = new List<string>();

                foreach (string Ext in InExtensions)
                {
                    DirectoriesWithExtensions.AddRange(AppendSuffix(InDirectories, "*." + Ext, true));
                }
                //

                return CheckoutSpecific(DirectoriesWithExtensions.ToArray());
            }

            return false;
        }

        public override bool CheckoutSpecific(string[] InFiles)
        {
            if (InFiles.Length > 0)
            {
                string Args = CombineArrayToCmd(InFiles.ToArray());

                int ExitCode = -1;
                List<String> DataLines = Tools.ExecuteCommand(GetGOpts() + " edit " + Args, out ExitCode, true);

                return ExitCode == 0;
            }

            return false;
        }

        public override bool SupportsCheckout()
        {
            return true;
        }

        public override bool RevertFiles(string[] InDirectories, bool bUnchangedOnly)
        {
            if (InDirectories.Length > 0)
            {
                string DirectoriesArgs = CombineArrayToCmd(AppendSuffix(InDirectories, WholeDirectorySuffix), true);

                int ExitCode = -1;

                List<String> DataLines = Tools.ExecuteCommand(GetGOpts() + " revert -c default " +
                   (bUnchangedOnly ? "-a" : "") +
                   " " + DirectoriesArgs,
                   out ExitCode,
                   false);

                return ExitCode == 0;
            }

            return false;
        }

        public override bool CommitFiles(string[] InDirectories, string Comment, bool bRevertUnchanged)
        {
            if (InDirectories.Length > 0 && Comment.Length > 0)
            {
                int ExitCode = -1;

                string AdditionalParameters = "";

                if (bRevertUnchanged)
                {
                    AdditionalParameters += "-f revertunchanged";
                }

                // NOTE: We can submit whole changelist without setting specific directories because we checked out needed files before
                List<String> DataLines = Tools.ExecuteCommand(GetGOpts() + " submit " + AdditionalParameters + " " +
                   "-d \"" + Comment + "\"", out ExitCode, true);

                return ExitCode == 0;
            }

            return false;
        }

        public override bool HasPendingChanges()
        {
            int ExitCode = -1;

            Logger.WriteLine("Checking for changes...");

            List<String> DataLines = Tools.ExecuteCommand(GetGOpts(true) + " opened -c default -s -m 30 -u " + SCMData.Credentials.UserName, out ExitCode, true);

            return DataLines.Count > 0;
        }

        public override string GetBinariesTypeCommitDescription(SCMCommitInfo.EBuildSubAction BuildSubAction)
        {
            return "Changelist";
        }
    }
}
