using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UE4BuildHelper.SCM
{
    class SCMSVNClient : SCMClientBase
    {
        protected override string GetGOpts(bool bForceLog = false)
        {
            string GlobalOptions = "svn";

            //if (!bForceLog && Logger.GetLogLevel() < ELogLevel.VeryVerbose)
            //{
            //    GlobalOptions += " -q"; // Suppress logs 
            //}

            return GlobalOptions;
        }

        public override bool SupprotsPartialUpdate()
        {
            return false;
        }

        public override bool ParseCommitInfo(string InData, out int OutRevision, out string OutInstigator)
        {
            OutRevision = -1;
            OutInstigator = null;

            try
            {
                char[] DelimiterChars = { '|' };
                string[] Tokens = InData.Split(DelimiterChars);

                if (Tokens.Length > 0)
                {
                    string RawRevision = Tokens[0].Remove(0, 1);
                    OutRevision = Int32.Parse(RawRevision);
                }
                if (Tokens.Length > 1)
                {
                    OutInstigator = Tokens[1].Replace(" ", "");
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION While parsing SVN Commit: " + e);
            }

            return false;
        }

        protected override bool ParseCommitInfosFromRawData(List<String> DataLines, out List<SCMCommitInfo> OutCommitInfos)
        {
            OutCommitInfos = null;

            if (DataLines != null)
            {
                List<String> CommitLines = new List<String>();
                OutCommitInfos = new List<SCMCommitInfo>();

                foreach (String s in DataLines)
                {
                    if (s == "------------------------------------------------------------------------")
                    {
                        if (CommitLines.Count > 0)
                        {
                            String Data = CommitLines[0];
                            String Comment = "";
                            List<SCMCommitInfo.ModifiedFileInfo> ModifiedFiles = null;

                            if (CommitLines.Count > 1)
                            {
                                for (int i = 1; i < CommitLines.Count; i++)
                                {
                                    if(CommitLines[i].Equals("changed paths:", StringComparison.OrdinalIgnoreCase))
                                    {
                                        for(int j = i + 1; j < CommitLines.Count; j++)
                                        {
                                            if (CommitLines[j].Length == 0)
                                            {
                                                i = j;
                                                break;
                                            }
                                            else
                                            {
                                                char[] DelimiterChars = { ' ' };
                                                string[] Tokens = CommitLines[j].Split(DelimiterChars, StringSplitOptions.RemoveEmptyEntries);

                                                if(Tokens.Length >= 2)
                                                {
                                                    if(ModifiedFiles == null)
                                                    {
                                                        ModifiedFiles = new List<SCMCommitInfo.ModifiedFileInfo>();
                                                    }

                                                    SCMCommitInfo.ModifiedFileInfo FileInfo = new SCMCommitInfo.ModifiedFileInfo();
                                                    FileInfo.Action = Tokens[0];
                                                    FileInfo.RelativePath = Tokens[1];

                                                    ModifiedFiles.Add(FileInfo);
                                                }
                                            }
                                        }
                                    }
                                    else if (CommitLines[i].Length > 0)
                                    {
                                        Comment += CommitLines[i] + "\n";
                                    }
                                }
                            }

                            SCMCommitInfo CommitInfo = new SCMCommitInfo();

                            int ParsedRevision = -1;
                            string Instigator = null;
                            if (ParseCommitInfo(Data, out ParsedRevision, out Instigator) && CommitInfo.Init(ParsedRevision, Instigator, Comment))
                            {
                                if(ModifiedFiles != null)
                                {
                                    CommitInfo.AppendModifiedFiles(ModifiedFiles);
                                }

                                OutCommitInfos.Add(CommitInfo);
                            }

                            CommitLines.Clear();
                        }
                    }
                    else if (s != null)
                    {
                        CommitLines.Add(s);
                    }
                }
                CommitLines.Clear();

                return true;
            }

            return false;
        }

        protected SCMCommitInfo ParseSingleCommitInfo(List<string> DataLines)
        {
            SCMCommitInfo CommitInfo = null;

            foreach (String s in DataLines)
            {
                if(CommitInfo == null)
                {
                    CommitInfo = new SCMCommitInfo();
                }

                if(s.StartsWith("Revision:"))
                {
                    int Revision = -1;

                    if (Int32.TryParse(s.Replace("Revision: ", ""), out Revision))
                    {
                        CommitInfo.Revision = Revision;
                    }
                }

                if (s.StartsWith("Last Changed Author:"))
                {
                    CommitInfo.Instigator = s.Replace("Last Changed Author: ", "");
                }
            }

            return CommitInfo;
        }

        public override bool GetLog(int Revision, int ToRevision, string RepositoryRootDirectory, out List<SCMCommitInfo> OutCommitInfos, bool bVerbose = false)
        {
            OutCommitInfos = null;

            if (Revision <= 0 || ToRevision < 0)
            {
                return false;
            }

            if (!Directory.Exists(RepositoryRootDirectory))
            {
                return false;
            }

            int ExitCode = -1;
            List<String> DataLines = Tools.ExecuteCommand(GetGOpts(true) + " log " +
                (bVerbose ? " -v " : "") +
                " -r " + Revision.ToString() + ":" + ((ToRevision == 0) ? "HEAD" : ToRevision.ToString()) +
                " " + RepositoryRootDirectory, out ExitCode, true);

            if (ParseCommitInfosFromRawData(DataLines, out OutCommitInfos))
            {
                return ExitCode == 0;
            }

            return false;
        }

        private SCMCommitInfo GetRevisionInfoInternal(string RepositoryRootDirectory, string RevisionTag)
        {
            if (!Directory.Exists(RepositoryRootDirectory) || string.IsNullOrEmpty(RevisionTag))
            {
                return null;
            }

            List<SCMCommitInfo> CommitInfos;

            int ExitCode = -1;
            List<String> DataLines = Tools.ExecuteCommand(GetGOpts(true) + " log -r " + RevisionTag + " " + RepositoryRootDirectory, out ExitCode, true);

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
            return GetRevisionInfoInternal(RepositoryRootDirectory, "BASE");
        }

        public override SCMCommitInfo GetLatestRevision(string RepositoryRootDirectory)
        {
            return GetRevisionInfoInternal(RepositoryRootDirectory, "HEAD");
        }

        public override bool UpdateToTheLatestRevision(string[] InDirectories)
        {
            if (InDirectories.Length > 0)
            {
                string DirectoriesArgs = CombineArrayToCmd(InDirectories, true);

                if (!string.IsNullOrEmpty(DirectoriesArgs))
                {
                    int ExitCode = -1;
                    List<String> DataLines = Tools.ExecuteCommand(GetGOpts() + " update " + DirectoriesArgs +
                        "--accept theirs-full", out ExitCode, true);

                    return ExitCode == 0;
                }
            }

            return false;
        }

        public override bool RevertFiles(string[] InDirectories, bool bUnchangedOnly)
        {
            if (InDirectories.Length > 0)
            {
                string DirectoriesArgs = CombineArrayToCmd(InDirectories, true);

                if (!string.IsNullOrEmpty(DirectoriesArgs))
                {
                    int ExitCode = -1;
                    List<String> DataLines = Tools.ExecuteCommand(GetGOpts() + " revert --depth=infinity " + DirectoriesArgs, out ExitCode, true);

                    return ExitCode == 0;
                }
            }

            return false;
        }

        public override bool CommitFiles(string[] InDirectories, string Comment, bool bRevertUnchanged)
        {
            if (InDirectories.Length > 0 && Comment.Length > 0)
            {
                string DirectoriesArgs = CombineArrayToCmd(InDirectories, true);

                if (!string.IsNullOrEmpty(DirectoriesArgs))
                {
                    int ExitCode = -1;
                    List<String> DataLines = Tools.ExecuteCommand(GetGOpts() + " commit -m " +
                        "\"" + Comment + "\"" + " " + DirectoriesArgs, out ExitCode, true);

                    return ExitCode == 0;
                }
            }

            return false;
        }

        public override bool HasPendingChanges()
        {
            return false;
        }

        public override string GetBinariesTypeCommitDescription(SCMCommitInfo.EBuildSubAction BuildSubAction)
        {
            string BinariesType = "";

            if (BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_OnlyProject)
            {
                BinariesType = "Project";
            }
            else if (BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_OnlyEngine)
            {
                BinariesType = "Engine";
            }
            else if (BuildSubAction == SCMCommitInfo.EBuildSubAction.EBSA_All)
            {
                BinariesType = "Engine and Project";
            }

            return BinariesType;
            ;
        }

        public override bool WriteChangelogToXML(string Filename, List<SCMCommitInfo> InCommits)
        {
            using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(Filename))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("log");

                foreach(SCMCommitInfo CommitInfo in InCommits)
                {
                    writer.WriteStartElement("logentry");
                    writer.WriteAttributeString("revision", CommitInfo.Revision.ToString());

                        writer.WriteElementString("author", CommitInfo.Instigator);

                        writer.WriteStartElement("paths");

                            foreach(SCMCommitInfo.ModifiedFileInfo ModifiedFile in CommitInfo.ModifiedFiles)
                            {
                                writer.WriteStartElement("path");

                                writer.WriteAttributeString("action", ModifiedFile.Action);
                                writer.WriteAttributeString("localPath", "");
                                writer.WriteAttributeString("kind", "file");
                                writer.WriteString(ModifiedFile.RelativePath);

                                writer.WriteEndElement(); // path
                            }
                        
                        writer.WriteEndElement(); // paths

                        writer.WriteElementString("msg", CommitInfo.Comment);

                    writer.WriteEndElement(); // logentry
                }

                writer.WriteEndElement(); // log
                writer.WriteEndDocument();
            }

            return true;
        }

        public override bool ResolveConflicts(bool bTheirs)
        {
            // Not supported

            return true;
        }

        public override bool CheckoutFiles(string[] InDirectories, string[] InExtensions)
        {
            // Not supported

            return true;
        }

        public override bool CheckoutSpecific(string[] InFiles)
        {
            // Not supported

            return true;
        }
    }
}
