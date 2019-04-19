using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UE4BuildHelper
{
    public class Serialization
    {
        // Json serialization: Begin
        [DataContract]
        public class SerializableObject
        {
            [DataMember]
            public String ObjectType = null;

            public string Serialize()
            {
                ObjectType = this.ToString();

                try
                {
                    DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(this.GetType());

                    using (MemoryStream ms = new MemoryStream())
                    {
                        jsonFormatter.WriteObject(ms, this);
                        return Encoding.Default.GetString(ms.ToArray());
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteLine(e.ToString());
                }

                return "";
            }

            public static T Deserialize<T>(string data)
            {
                try
                {
                    DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(T));

                    using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(data)))
                    {
                        T info = (T)jsonFormatter.ReadObject(ms);

                        return info;
                    }
                } 
                catch (Exception e)
                {
                    Logger.WriteLine(e.ToString());
                }

                return default(T);
            }
        }

        [DataContract]
        public class JenkinsAuthResponse : SerializableObject
        {
            [DataMember]
            public String _class = null;

            [DataMember]
            public String crumb = null;

            [DataMember]
            public String crumbRequestField = null;
        }
        // Json serialization: End

        // Binary serialization: Begin
        [Serializable]
        public class BinaryObjectFile
        {
            static private string ResolveFilePath(string FileName, string Id)
            {
                string FileNameToSave = FileName;

                if(FileNameToSave.Length == 0)
                {
                    return null;
                }

                if (Id != null)
                {
                    FileNameToSave += "-" + Id;
                }

                FileNameToSave += ".bin";

                return Path.Combine(FileHelper.CacheDirectory, FileNameToSave);
            }

            private string ResolveFilePath(string Id)
            {
                return ResolveFilePath(this.GetType().Name, Id);
            }

            public void Save(string Id = null)
            {
                string FilePath = ResolveFilePath(Id);

                WriteBinaryObjectToFile(FilePath, this);
            }

            public static T Load<T>(string Id = null)
            {
                string FileName = typeof(T).Name;

                string FilePath = ResolveFilePath(FileName, Id);

                return (T)ReadBinaryObjectFromFile(FilePath);
            }

            public void RemoveFile(string Id = null)
            {
                string FilePath = ResolveFilePath(Id);

                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
        }

        [Serializable]
        public class JenkinsJobCache : BinaryObjectFile
        {
            public SCMCommitInfo ResolvedCommitInfo = null;
        }

        [Serializable]
        public class SCMCachedData : BinaryObjectFile
        {
            public SCMCommitInfo LastUpdateCommitInfo = null;

            public int LastCheckRevision = -1;
        }

        [Serializable]
        public class PackagedBuildInfo : BinaryObjectFile
        {
            public string PackagedBuildPath = "";

            public string ExtractBuildDirectoryName()
            {
                char[] DelimiterChars = { '/', '\\' };
                string[] Tokens = PackagedBuildPath.Split(DelimiterChars);

                if(Tokens.Length > 0)
                {
                    return Tokens.Last();
                }

                return null;
            }
        }


        public static bool WriteBinaryObjectToFile(string FileName, object InObject)
        {
            string FileDirectory = Path.GetDirectoryName(FileName);

            if(!Directory.Exists(FileDirectory))
            {
                Directory.CreateDirectory(FileDirectory);
            }

            if (Directory.Exists(FileDirectory) && InObject != null)
            {
                IFormatter BinFormatter = new BinaryFormatter();
                Stream FStream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                BinFormatter.Serialize(FStream, InObject);
                FStream.Close();

                return true;
            }

            return false;
        }

        public static object ReadBinaryObjectFromFile(string FileName)
        {
            if(File.Exists(FileName))
            {
                IFormatter BinFormatter = new BinaryFormatter();
                Stream FStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                object ResultObject = BinFormatter.Deserialize(FStream);
                FStream.Close();

                return ResultObject;
            }

            return null;
        }
        // Binary serialization: End
    }
}
