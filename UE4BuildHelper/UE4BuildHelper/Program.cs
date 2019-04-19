using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UE4BuildHelper
{
    class CommandlineHelper
    {
        private Dictionary<string, List<string>> ParsedArgs = new Dictionary<string, List<string>>();

        public CommandlineHelper()
        {

        }

        public CommandlineHelper(string[] InArgs)
        {
            string CurrentKey = null;

            foreach (string Arg in InArgs)
            {
                string PureArg = Arg.Replace('\n'.ToString(), "").Replace('\r'.ToString(), "").Replace('\t'.ToString(), "");
                
                if (PureArg.StartsWith("-"))
                {
                    CurrentKey = PureArg.ToLower().Replace("-", "");

                    ParsedArgs[CurrentKey] = new List<string>();
                }
                else if (CurrentKey != null && ParsedArgs.ContainsKey(CurrentKey) && ParsedArgs[CurrentKey] != null)
                {
                    ParsedArgs[CurrentKey].Add(PureArg);
                }
            }
        }

        public bool HasArgument(string Key)
        {
            return ParsedArgs != null && Key != null && ParsedArgs.ContainsKey(Key.ToLower());
        }

        public string GetString(string Key)
        {
            Key = Key.ToLower();

            if (HasArgument(Key) && ParsedArgs[Key] != null && ParsedArgs[Key].Count > 0)
            {
                return ParsedArgs[Key].ElementAt(0);
            }

            return "";
        }

        public bool GetBool(string Key)
        {
            return GetString(Key).ToLower() == "true";
        }

        public int GetInt(string Key, int DefaultValue = -1)
        {
            return HasArgument(Key) ? int.Parse(GetString(Key)) : -1;
        }

        public List<string> GetArgumentsArray(string Key)
        {
            Key = Key.ToLower();

            if (HasArgument(Key) && ParsedArgs[Key] != null)
            {
                return ParsedArgs[Key];
            }

            return null;
        }

        public void PrintArgs()
        {
            string OutStr = "\n-----------------------\nCommandline input args:\n";

            foreach (var Entry in ParsedArgs)
            {
                string ListOfArgs = "";

                foreach(string Arg in Entry.Value)
                {
                    ListOfArgs += Arg + "; ";
                }

                OutStr += "\t" + Entry.Key + ": " + ListOfArgs + "\n";
            }

            OutStr += "-----------------------";

            Logger.WriteLine(OutStr);
        }

        public bool HasAnyArgs()
        {
            return ParsedArgs.Count > 0;
        }
    }

    class Program
    {
        private const string MajorParameter_HandleSCMTrigger = "handleSCMtrigger";
        private const string MajorParameter_UpdateSCM = "updatescm";
        private const string MajorParameter_ResolveSourceMarks = "resolvesourcemarks";
        private const string MajorParameter_CheckoutBinaries = "checkoutbin";
        private const string MajorParameter_PerformBuild = "performbuild";
        private const string MajorParameter_RevertSCM = "revertscm";
        private const string MajorParameter_CommitDLL = "commitdll";
        private const string MajorParameter_PerformPackage = "performpackage";
        private const string MajorParameter_UploadBuild = "uploadbuild";
        private const string MajorParameter_Cleanup = "cleanup";

        // Debug
        private const string MajorParameter_DebugFullPipeline = "debugfullpipeline";
        //

        private static UEBuildAutomation AutomationTool = null;

        static int HandleExecutionCommand(CommandlineHelper InCommandline)
        {
            int ExecutionResult = 0;

            if (AutomationTool != null && InCommandline.HasAnyArgs())
            {
                // Get required arguments
                GlobalParametersInfo GlobalParameters = new GlobalParametersInfo(InCommandline);
                //

                if (InCommandline.HasArgument(MajorParameter_HandleSCMTrigger))
                {
                    int ChangeNumber = InCommandline.GetInt("c", -1);
                    string UserName = InCommandline.GetString("u");

                    string DLLJobName = GlobalParameters.JenkinsJobName;
                    string PackageJobName = InCommandline.GetString("jp");

                    ESCMType SCMType = ESCMType.None;
                    Enum.TryParse<ESCMType>(InCommandline.GetString("s"), true, out SCMType);

                    if (ChangeNumber > 0 && !string.IsNullOrEmpty(DLLJobName) && !string.IsNullOrEmpty(PackageJobName) && SCMType != ESCMType.None)
                    {
                        if (SCMType == ESCMType.Perforce && string.IsNullOrEmpty(UserName))
                        {
                            Logger.WError("Missing arguments: [-u] arguments expected for Perforce.");
                        }

                        ExecutionResult = AutomationTool.ProcessSCMTrigger(ChangeNumber, DLLJobName, PackageJobName, SCMType, UserName);
                    }
                    else
                    {
                        Logger.WError("Invalid arguments: [-c; -j; -u] arguments expected.");
                    }
                }
                else if (GlobalParameters.IsValid())
                {
                    if (InCommandline.HasArgument(MajorParameter_UpdateSCM))
                    {
                        ExecutionResult = AutomationTool.ProcessUpdate(GlobalParameters);
                    }
                    else if (InCommandline.HasArgument(MajorParameter_ResolveSourceMarks))
                    {
                        bool bWriteChangelog = InCommandline.HasArgument("cl");
                        string WorkspaceRoot = null;

                        if(bWriteChangelog)
                        {
                            WorkspaceRoot = InCommandline.GetString("w");

                            if(string.IsNullOrEmpty(WorkspaceRoot))
                            {
                                Logger.WError("Changelog won't be written because -w option is not specified. (-w option is required with -cl argument)");
                            }
                        }

                        ExecutionResult = AutomationTool.ResolveAndCacheSourceCommits(GlobalParameters, bWriteChangelog, WorkspaceRoot);
                    }
                    else if (InCommandline.HasArgument(MajorParameter_CheckoutBinaries))
                    {
                        List<string> Values = InCommandline.GetArgumentsArray("e");

                        string[] AllExtensions = Values != null ? Values.ToArray() : null;

                        if (AllExtensions != null && AllExtensions.Length > 0)
                        {
                            ExecutionResult = AutomationTool.CheckoutBinaries(GlobalParameters, AllExtensions);
                        }
                        else
                        {
                            Logger.WError("Invalid arguments: [-e] arguments expected.");
                        }
                    }
                    else if (InCommandline.HasArgument(MajorParameter_PerformBuild))
                    {
                        ExecutionResult = AutomationTool.ProcessBinaryBuild(GlobalParameters);
                    }
                    else if (InCommandline.HasArgument(MajorParameter_RevertSCM))
                    {
                        bool bRevertUnchangedOnly = InCommandline.HasArgument("u");

                        ExecutionResult = AutomationTool.RevertSCM(GlobalParameters, bRevertUnchangedOnly);
                    }
                    else if (InCommandline.HasArgument(MajorParameter_CommitDLL))
                    {
                        bool bRevertUnchanged = InCommandline.HasArgument("ur");

                        ExecutionResult = AutomationTool.ProcessCommitDLL(GlobalParameters, bRevertUnchanged);
                    }
                    else if (InCommandline.HasArgument(MajorParameter_Cleanup))
                    {
                        string WorkspaceRoot = InCommandline.GetString("w");

                        ExecutionResult = AutomationTool.Cleanup(GlobalParameters, WorkspaceRoot);
                    }
                    else if (InCommandline.HasArgument(MajorParameter_PerformPackage))
                    {
                        // Required
                        string BuildConfiguration = InCommandline.GetString("configuration");
                        string Platform = InCommandline.GetString("platform");
                        string WorkspaceRoot = InCommandline.GetString("w");

                        // Optional
                        bool bFullRebuild = InCommandline.GetBool("fullrebuild");
                        bool bDistributiion = InCommandline.GetBool("distribution");
                        bool bSkipCook = InCommandline.GetBool("skipcook");

                        if (!string.IsNullOrEmpty(BuildConfiguration) &&
                            !string.IsNullOrEmpty(Platform) &&
                            !string.IsNullOrEmpty(WorkspaceRoot))
                        {
                            ExecutionResult = AutomationTool.PerformPackageProject(GlobalParameters, BuildConfiguration, Platform, WorkspaceRoot, 
                                bFullRebuild, bDistributiion, bSkipCook);
                        }
                        else
                        {
                            Logger.WError("Invalid arguments.");
                        }
                    }
                    else if (InCommandline.HasArgument(MajorParameter_UploadBuild))
                    {
                        string UploaderJobName = InCommandline.GetString("uj");
                        ExecutionResult = AutomationTool.UploadBuild(GlobalParameters, UploaderJobName);
                    }
                    // Debug
                    else if (InCommandline.HasArgument(MajorParameter_DebugFullPipeline))
                    {
                        int ChangeNumber = InCommandline.GetInt("c", -1);

                        if (ChangeNumber > 0)
                        {
                            ExecutionResult = AutomationTool.ProcessUpdate(GlobalParameters);

                            Serialization.JenkinsJobCache JobCache = Serialization.JenkinsJobCache.Load<Serialization.JenkinsJobCache>(GlobalParameters.BuildTag);

                            if (JobCache == null)
                            {
                                JobCache = new Serialization.JenkinsJobCache();

                                if (JobCache != null)
                                {
                                    SCMCommitInfo CommitInfo = new SCMCommitInfo();

                                    CommitInfo.BuildAction = SCMCommitInfo.EBuildAction.EBA_Build;
                                    CommitInfo.BuildSubAction = SCMCommitInfo.EBuildSubAction.EBSA_OnlyProject;

                                    CommitInfo.Instigator = "User1";
                                    CommitInfo.Revision = 109;

                                    JobCache.ResolvedCommitInfo = CommitInfo;

                                    JobCache.Save(GlobalParameters.BuildTag);
                                }
                            }

                            if (ExecutionResult == 0)
                            {
                                string[] BinExtensions = { "exe", "dll", "exp", "lib", "target", "modules", "version", "pdb" };
                                ExecutionResult = AutomationTool.CheckoutBinaries(GlobalParameters, BinExtensions);
                            }

                            if (ExecutionResult == 0)
                            {
                                ExecutionResult = AutomationTool.ProcessBinaryBuild(GlobalParameters);
                            }

                            if (ExecutionResult == 0)
                            {
                                //ExecutionResult = AutomationTool.RevertSCM(GlobalParameters, true);
                            }

                            if (ExecutionResult == 0)
                            {
                                ExecutionResult = AutomationTool.ProcessCommitDLL(GlobalParameters, true);
                            }

                            AutomationTool.Cleanup(GlobalParameters, null);
                        }
                        else
                        {
                            Logger.WError("Invalid arguments: [-c] arguments expected.");
                        }
                    }
                }
                else
                {
                    Logger.WError("Required global arguments expected: [-j; -t; -s] arguments expected.");

                    return -1;
                }
                //

                // Force cleanup if something fails
                if (ExecutionResult != 0)
                {
                    string WorkspaceRoot = InCommandline.GetString("w");

                    Logger.WError("Operation executed with error. Cleaning up...");

                    AutomationTool.Cleanup(GlobalParameters, WorkspaceRoot);
                }

            }

            return ExecutionResult;
        }

        public static void ShowStartupWindow()
        {
            Thread UIThread = new Thread(delegate ()
            {
                UI.StartupForm StartupWindow = new UI.StartupForm();

                StartupWindow.LoadConfig(Config.Get());

                Application.EnableVisualStyles();
                Application.Run(StartupWindow);

            });

            UIThread.SetApartmentState(ApartmentState.STA);
            UIThread.Start();

            UIThread.Join();
        }

        /**
            INPUT ARGUMENTS:

            Required global arguments:
                -j Jenkins Job Name
                -t Tag of Jenkins build
                -s Source Code Management type (Perforce, SVN)
                -p Project override. Must be empty if building default project

            Jenkins Build Trigger:
                -handleSCMtrigger
                -c [Revision]
                -jp (Optional) Package Job
                -u Username who triggered build
                
            Update SCM:
                -updatescm Updates SCM, automatically resolving all conflicts to "Theirs"
                
            Resolve and cache commit info for binaries build
                -resolvesourcemarks
                -cl Write full changelog between revisions to the current Job
                    -w Workspace path (using only with -cl option)

            Checkout Binaries if SCM supports it:
                -checkoutbin
                -e List of extensions of the files that should be checked out (Example: "-e exe lib target")   

            Build DLL:
                -performbuild

            Revert Files:
                -revertscm 
                -u Revert unchanged only

            Commit DLL:
                -commitdll
                -ur

            Perform full build of the project
                -performpackage
                -configuration [STR] Configuration of the build (e.g. DebugGame, Development, Shipping)
                -platform [STR] Platform to build (e.g. IOS, Win64)
                -w [STR] Jenkins workspace root
                -fullrebuild [INT] (Optional) Is full rebuild
                -distribution [INT] (Optional) Is distribution

            Upload build
                -uploadbuild
                -uj Uploader Job Name

            Debug:
                -debugfullpipeline
                -c [Revision]

            Other:
                -suppresslogs - disable logs
                -veryverbose - all possible logs 
                -cleanup - remove all temp files of current job
                    -w (Optional) - workspace root

            Logs:
                Verbose (Default) - this program logs and UE4 logs (special flag not needed in the commandline arguments)
                VeryVerbose - this program logs, UE4 logs, Perforce output and other console commands with enabled output
             
        */

        static int Main(string[] args)
        {
            CommandlineHelper Commandline = new CommandlineHelper(args);

            Config.Load();

            if (!Commandline.HasAnyArgs())
            {
                ShowStartupWindow();
            }
            else
            {
                Logger.Init(Commandline.HasArgument("suppresslogs") ? ELogLevel.None :
                    (Commandline.HasArgument("veryverbose") ? ELogLevel.VeryVerbose : ELogLevel.Verbose), Commandline.GetString("t"));

                Commandline.PrintArgs();

                AutomationTool = new UEBuildAutomation();

                // Get SCM Type
                ESCMType SCMType = ESCMType.None;
                if (!Enum.TryParse<ESCMType>(Commandline.GetString("s"), true, out SCMType))
                {
                    Logger.WError("Parameter [-s] not found in the commandline. Using Perforce as default SCM Type");
                    SCMType = ESCMType.Perforce;
                }
                //

                AutomationTool.Launch(SCMType, Commandline.GetString("j"));

                int ExecutionResult = HandleExecutionCommand(Commandline);

                Logger.StdOut("Execution completed with result: " + ExecutionResult);

                return ExecutionResult;
            }

            return 0;
        }
    }
}
