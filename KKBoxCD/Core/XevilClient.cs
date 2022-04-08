using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace KKBoxCD.Core
{
    public class XevilClient
    {
        #region Singleton

        private static readonly Lazy<XevilClient> Singleton = new Lazy<XevilClient>(() => new XevilClient());

        public static XevilClient Instance => Singleton.Value;

        #endregion

        private readonly List<string> PoolData;

        private readonly int MaxPoolSize = 1;

        protected XevilClient()
        {
            PoolData = new List<string>();
        }

        public void Start()
        {
            for (int i = 0; i < 1; i++)
            {
                new Thread(Work).Start();
            }
        }

        private void Work()
        {
            RestClient client = new RestClient(new RestClientOptions
            {
                Timeout = 10000
            });
            while (true)
            {
                if (PoolData.Count < MaxPoolSize)
                {
                    try
                    {
                        RestRequest request = new RestRequest("http://xmenxevil.zapto.org/Xevil/createTask", Method.Post);
                        string body = "{\"clientKey\":\"4b21537f9c9426b6e98213f27634202e\",\"task\":{\"websiteURL\":\"https://kkid.kkbox.com/login\",\"websiteKey\":\"6LcuGcoUAAAAAB8E-zI7hoiQ_fcudMnk9YVZtW4m\",\"minScore\":0.7,\"pageAction\":\"login\",\"type\":\"RecaptchaV3TaskProxyless\"}}";
                        request.AddHeader("Content-Type", "application/json");
                        request.AddBody(body, "application/json");
                        RestResponse response = client.ExecuteAsync(request).Result;
                        dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Content);

                        int task_id = data.taskId;
                        int timeout = 30000;
                        while (timeout > 0)
                        {
                            try
                            {
                                request = new RestRequest("http://xmenxevil.zapto.org/Xevil/getTaskResult", Method.Post);
                                body = string.Concat("{\"clientKey\":\"4b21537f9c9426b6e98213f27634202e\",\"taskId\":", task_id, "}");
                                request.AddHeader("Content-Type", "application/json");
                                request.AddBody(body, "application/json");
                                response = client.ExecuteAsync(request).Result;
                                data = JsonConvert.DeserializeObject<dynamic>(response.Content);

                                if (data != null)
                                {
                                    string status = data.status;
                                    if (status.Equals("ready"))
                                    {
                                        string token = data.solution.text;
                                        PoolData.Add(token);
                                        break;
                                    }
                                    else if (!status.Equals("processing"))
                                {
                                    break;
                                }
                                }
                            }
                            catch { }

                            Thread.Sleep(5000);
                            timeout -= 5000;
                        }
                    }
                    catch { }
                }
                Thread.Sleep(2000);
            }
        }

        public string Get(int timeout = 30000)
        {
            while (timeout > 0)
            {
                if (PoolData.Any())
                {
                    string token = PoolData[0];
                    PoolData.RemoveAt(0);
                    return token;
                }
                Thread.Sleep(5000);
                timeout -= 5000;
            }
            return null;
        }

        public void Push(string token)
        {
            PoolData.Add(token);
        }

        public int Size()
        {
            return PoolData.Count;
        }
    }
}
