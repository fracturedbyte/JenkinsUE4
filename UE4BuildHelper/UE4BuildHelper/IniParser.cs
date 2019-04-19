using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace External
{
    public class IniParser
    {
        string Path;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniParser(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? EXE + ".ini").FullName.ToString();
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        private class IniValue
        {
            public String Key = null;
            public String Value = null;

            public IniValue(String inKey, String inValue)
            {
                Key = inKey;
                Value = inValue;
            }

            public override string ToString()
            {
                return Key + "=" + Value;
            }
        }

        public void WriteArray(string Key, string Value, string Section = "Unknown")
        {
            StreamReader sr = new StreamReader(Path);
  
            Dictionary<string, List<IniValue>> iniFileMap = new Dictionary<string, List<IniValue>>();
            string currentSection = null;

            while (!sr.EndOfStream)
            {
                string str = sr.ReadLine();
                
                if (str == null)
                    break;

                if (str.Length == 0)
                    continue;

                if(str.StartsWith("["))
                {
                    currentSection = str.Replace("[", "").Replace("]", "");

                    if (!iniFileMap.ContainsKey(currentSection))
                    {
                        iniFileMap.Add(currentSection, new List<IniValue>());
                    }
                }

                if(iniFileMap.ContainsKey(currentSection))
                {
                    int separatorIndex = str.IndexOf("=");

                    if (separatorIndex > 0)
                    {
                        iniFileMap[currentSection].Add(new IniValue(str.Substring(0, separatorIndex), str.Substring(separatorIndex + 1, str.Length - separatorIndex - 1)));
                    }
                }
            }

            sr.Close();

            if(iniFileMap.ContainsKey(Section))
            {
                iniFileMap[Section].Add(new IniValue("+" + Key, Value));
            }

            using (StreamWriter sw = new StreamWriter(Path))
            {
                foreach (KeyValuePair<string, List<IniValue>> entry in iniFileMap)
                {
                    sw.WriteLine("[" + entry.Key + "]");

                    foreach(IniValue iniValue in entry.Value)
                    {
                        sw.WriteLine(iniValue.ToString());
                    }

                    sw.WriteLine("");
                }
            }
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
