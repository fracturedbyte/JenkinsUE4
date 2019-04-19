using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;


namespace UE4BuildHelper
{
    class HTTPClient
    {
        private static string GetJenkinsCrumb(CredentialsInfo InCredentials)
        {
            if (InCredentials != null)
            {
                StringContent Content = new StringContent("");

                using (HttpClient Client = new HttpClient())
                {
                    Client.DefaultRequestHeaders.Add("Authorization", "Basic " + Config.Get().JenkinsCredentials.GetBase64());

                    var Result = Client.PostAsync(Config.Get().JenkinsServerURL + "/crumbIssuer/api/json", Content).Result;
                    string ResultContent = Result.Content.ReadAsStringAsync().Result;

                    Logger.WriteLine("GetJenkinsCrumb Response: " + ResultContent, ELogLevel.VeryVerbose);

                    Serialization.JenkinsAuthResponse Response = Serialization.SerializableObject.Deserialize<Serialization.JenkinsAuthResponse>(ResultContent);

                    if (Response != null)
                    {
                        return Response.crumb;
                    }
                    else
                    {
                        Logger.WError("HTTPClient Can't get Jenkins crumb: Response is null");
                    }
                }
            }
            else
            {
                Logger.WError("HTTPClient Can't get Jenkins crumb: Credentials is null");
            }

            return "";
        }

        public static void TriggerJenkinsParametrizedBuild(string JobName, CredentialsInfo InCredentials, string Token, int DelaySec = 0, 
            ESCMType SCMType = ESCMType.None, string ProjectOverride = null,
            List<KeyValuePair<string, string>> AdditionalArguments = null)
        {
            if (JobName != null && InCredentials != null && !string.IsNullOrEmpty(Config.Get().JenkinsServerURL))
            {
                List<KeyValuePair<string, string>> Values = new List<KeyValuePair<string, string>>();
                Values.Add(new KeyValuePair<string, string>("token", Token));

                if (DelaySec > 0)
                {
                    Values.Add(new KeyValuePair<string, string>("delay", DelaySec + "sec"));
                }

                if (SCMType != ESCMType.None)
                {
                    Values.Add(new KeyValuePair<string, string>("SCMType", SCMType.ToString()));
                }

                if(ProjectOverride != null)
                {
                    Values.Add(new KeyValuePair<string, string>("ProjectName", ProjectOverride));
                }

                if(AdditionalArguments != null)
                {
                    Values.AddRange(AdditionalArguments);
                }

                FormUrlEncodedContent Content = new FormUrlEncodedContent(Values);

                using (HttpClient Client = new HttpClient())
                {
                    Client.DefaultRequestHeaders.Add("Authorization", "Basic " + Config.Get().JenkinsCredentials.GetBase64());

                    var Result = Client.PostAsync(Config.Get().JenkinsServerURL + "/job/" + JobName + "/buildWithParameters", Content).Result;
                    string ResultContent = Result.Content.ReadAsStringAsync().Result;

                    Logger.WriteLine("HTTPClient Trigger Jenkins build result: " + ResultContent);
                }
            }
            else
            {
                Logger.WError("HTTPClient Can't trigger Jenkins build: Credentials or URL is null");
            }
        }

        public static void TriggerJenkinsParametrizedBuild(string JobName, string Token, List<KeyValuePair<string, string>> AdditionalArguments)
        {
            CredentialsInfo Credentials = new CredentialsInfo();

            TriggerJenkinsParametrizedBuild(JobName, Credentials, Token, 0, ESCMType.None, null, AdditionalArguments);
        }
    }
}
