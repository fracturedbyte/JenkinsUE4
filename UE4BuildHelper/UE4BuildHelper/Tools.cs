using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UE4BuildHelper
{
    enum ELogLevel
    {
        None,
        Verbose,
        VeryVerbose
    }

    public enum ESCMType
    {
        None,
        Perforce,
        SVN
    }

    class Logger
    {
        private static string LogFileName = null;
        private static ELogLevel LogLevel = ELogLevel.None;

        private static string GetNowDateTime()
        {
            return DateTime.Now.ToShortDateString().Replace("/", "-").Replace(" ", "-").Replace(":", "-").Replace(".", "-") + "-" +
                     DateTime.Now.ToShortTimeString().Replace("/", "-").Replace(" ", "-").Replace(":", "-").Replace(".", "-");
        }

        public static ELogLevel GetLogLevel()
        {
            return LogLevel;
        }

        public static void Init(ELogLevel InLogLevel, string LogTag)
        {
            LogLevel = InLogLevel;

            if (FileHelper.LogsDirectory != null)
            {
                string LogBaseFileName = "";
#if DEBUG
                LogBaseFileName = "DEBUG-UE4BuildHelperLog-" + GetNowDateTime();
#else
                if (LogTag.Length > 0)
                {
                    LogBaseFileName = "UE4BuildHelperLog-" + LogTag;
                }
                else
                {
                    LogBaseFileName = "UE4BuildHelperLog-" + GetNowDateTime();
                }
#endif

                LogFileName = Path.Combine(Path.GetFullPath(FileHelper.LogsDirectory), LogBaseFileName + ".txt");
            }
        }

        public static void WriteLine(string Format, ELogLevel InLogLevel)
        {
            if(InLogLevel <= LogLevel && InLogLevel > ELogLevel.None)
            {
                WriteLine(Format);
            }
        }

        public static void WriteLine(string Format)
        {
            if(LogLevel > ELogLevel.None)
            {
                string Prefix = "";
#if DEBUG
                Prefix = "DEBUG-";
#endif

                Format = Prefix + GetNowDateTime() + ": " + Format;

                Console.WriteLine(Format);

                if (LogFileName != null)
                {
                    if (!File.Exists(LogFileName))
                    {
                        string DirectoryName = Path.GetDirectoryName(LogFileName);
                        if (!Directory.Exists(DirectoryName))
                        {
                            Directory.CreateDirectory(DirectoryName);

                            Logger.WriteLine("Log Directory created at: " + DirectoryName);
                        }

                        using (StreamWriter sw = File.CreateText(LogFileName))
                        {
                            sw.WriteLine(Format);
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = File.AppendText(LogFileName))
                        {
                            sw.WriteLine(Format);
                        }
                    }
                }
            }
        }

        public static void WError(string Format)
        {
            WriteLine("ERROR: " + Format);
        }

        public static void StdOut(string Format)
        {
            Console.WriteLine(Format);
        }
    }

    [Serializable]
    public class CredentialsInfo
    {
        public string UserName = null;
        private string Password = null;

        public string APIToken = null;

        public string GetBase64()
        {
            return Tools.Base64Encode(UserName + ":" + APIToken);
        }

        public void SetPassword(string InPassword)
        {
            Password = InPassword; // !string.IsNullOrEmpty(InPassword) ? Tools.CreateMD5(InPassword) : null;
        }

        public string GetPassword()
        {
            return Password;
        }
    }

    [Serializable]
    public class SCMInfo
    {
        public CredentialsInfo Credentials = new CredentialsInfo();

        //public string WorkspaceName = null;
        public string ServerAddress = null;
    }

    [Serializable]
    public class JenkinsJobInfo
    {
        public string Name = null;
        public string LaunchToken = null;
        public int Delay = 60;

        public string RepositoryRootDirectory = null;
        public string EngineDirectory = null;
        public string ProjectsDirectory = null;

        public string DefaultProjectName = null;

        public string WorkspaceName = null;

        private string ProjectDirectory = null;

        public SCMInfo SCMData = new SCMInfo();

        public JenkinsJobInfo Clone()
        {
            return MemberwiseClone() as JenkinsJobInfo;
        }

        public bool IsRepositoryEquals(JenkinsJobInfo InJobInfo)
        {
            return InJobInfo.RepositoryRootDirectory == RepositoryRootDirectory &&
                InJobInfo.EngineDirectory == EngineDirectory &&
                InJobInfo.ProjectsDirectory == ProjectsDirectory;
        }

        public string GetProjectDirectory(string OverrideProjectName = null)
        {
            if (ProjectDirectory == null || OverrideProjectName != null)
            {
                foreach (string Dir in Directory.GetDirectories(ProjectsDirectory))
                {
                    foreach (string FileName in Directory.GetFiles(Dir))
                    {
                        if(FileName.EndsWith(".uproject") && 
                            FileName.Contains((OverrideProjectName != null ? OverrideProjectName : DefaultProjectName) + ".uproject"))
                        {
                            ProjectDirectory = Dir;
                            return ProjectDirectory;
                        }
                    }
                }
            }

            return ProjectDirectory;
        }

        public bool IsSCMDataValid()
        {
            if (SCMData == null)
            {
                SCMData = new SCMInfo();
            }

            if (SCMData != null)
            {
                if (SCMData.Credentials == null)
                {
                    SCMData.Credentials = new CredentialsInfo();
                }

                if (SCMData.Credentials != null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public class Config : Serialization.BinaryObjectFile
    {
        public CredentialsInfo JenkinsCredentials = new CredentialsInfo();

        public string JenkinsServerURL = null;
        public List<JenkinsJobInfo> JenkinsJobs = new List<JenkinsJobInfo>();

        private static Config StaticInstance = null;
        public static Config Get()
        {
            return StaticInstance;
        }

        public static void Load()
        {
            StaticInstance = Serialization.BinaryObjectFile.Load<Config>();

            if(StaticInstance == null)
            {
                StaticInstance = new Config();

                JenkinsJobInfo JenkinsJob = new JenkinsJobInfo();

                StaticInstance.Save();
            }
        }

        public JenkinsJobInfo FindJenkinsJob(string JobName)
        {
            foreach (JenkinsJobInfo Job in JenkinsJobs)
            {
                if (Job.Name.Equals(JobName))
                {
                    return Job;
                }
            }

            return null;
        }
    }

    class GlobalParametersInfo
    {
        public string JenkinsJobName = null;
        public string BuildTag = null;
        public string ProjectOverride = null;

        GlobalParametersInfo()
        {

        }

        public GlobalParametersInfo(CommandlineHelper InCommandline)
        {
            if (InCommandline != null)
            {
                JenkinsJobName = InCommandline.GetString("j");
                BuildTag = InCommandline.GetString("t");
                ProjectOverride = string.IsNullOrEmpty(InCommandline.GetString("p")) ? null : InCommandline.GetString("p");
            }
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(JenkinsJobName) &&
                !string.IsNullOrEmpty(BuildTag);
        }
    }

    class Tools
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static List<string> ExecuteCommand(string CommandWitArgs, out int ExitCode, bool bPrintOutput = false)
        {
            Logger.WriteLine("Executing command: " + CommandWitArgs, ELogLevel.VeryVerbose);

            Process process = new Process();
            ProcessStartInfo startinfo = new ProcessStartInfo("cmd.exe", @"/C " + CommandWitArgs);
            startinfo.RedirectStandardOutput = true;
            startinfo.UseShellExecute = false;
            process.StartInfo = startinfo;

            List<string> DataLines = new List<string>();
            process.OutputDataReceived += delegate (object sendingProcess, DataReceivedEventArgs outLine)
            {
                if (outLine.Data != null)
                {
                    DataLines.Add(outLine.Data);

                    if(bPrintOutput)
                    {
                        Logger.WriteLine(outLine.Data, ELogLevel.VeryVerbose);
                    }
                }
            };
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            ExitCode = process.ExitCode;

            Logger.WriteLine("Command executed with result: " + ExitCode, ELogLevel.VeryVerbose);

            return DataLines;
        }

        public static string CreateMD5(string Input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] InputBytes = System.Text.Encoding.ASCII.GetBytes(Input);
                byte[] HashBytes = md5.ComputeHash(InputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < HashBytes.Length; i++)
                {
                    sb.Append(HashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static string BuildIniPath(string IniName, string ProjectRoot)
        {
            if (ProjectRoot != null)
            {
                return ProjectRoot + "/Config/" + IniName;
            }

            return null;
        }

        public static int ExtractBuildNumberFromTag(string BuildTag)
        {
            char[] Delims= { '-' };
            string[] Tokens = BuildTag.Split(Delims);

            if(Tokens.Length >= 3)
            {
                int OutValue = -1;
                if(int.TryParse(Tokens[2], out OutValue))
                {
                    return OutValue;
                }
            }

            return -1;
        }
    }

    class FileHelper
    {
        public static string ExecutableDirectory
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }

        public static string BuilderDirectory
        {
            get
            {
#if DEBUG
                return ExecutableDirectory + "/../../../../Jenkins/Builder/";
#else
                return ExecutableDirectory + "/../";
#endif
            }
        }

        public static string BatchFilesPath
        {
            get
            {
                return BuilderDirectory + "BatchFiles/";
            }
        }

        public static string LogsDirectory
        {
            get
            {
                return BuilderDirectory + "Logs/";
            }
        }

        public static string CacheDirectory
        {
            get
            {
                return BuilderDirectory + "Cache/";
            }
        }

        public static string BuildDLLBat = "BuildDLL.bat";
        public static string RebuildDLLBat = "RebuildDLL.bat";
        public static string BuildDedicatedServerBat = "BuildDedicatedServer.bat";
        public static string BuildStandaloneBat = "BuildStandalone.bat";
    }
}
