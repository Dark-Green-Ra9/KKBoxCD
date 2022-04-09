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

        private readonly List<Recaptcha> PoolData;

        public int PoolSize { get; set; } = 5;

        public int ThreadSize { get; set; } = 2;

        public int SolvingSize { get; private set; } = 0;

        protected XevilClient()
        {
            PoolData = new List<Recaptcha>();
        }

        public void Start()
        {
            for (int i = 0; i < ThreadSize; i++)
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
                if (PoolData.Count + SolvingSize < PoolSize)
                {
                    try
                    {
                        SolvingSize++;
                        RestRequest request = new RestRequest("http://xmenxevil.zapto.org/Xevil/createTask", Method.Post);
                        string body = "{\"clientKey\":\"4b21537f9c9426b6e98213f27634202e\",\"task\":{\"websiteURL\":\"https://kkid.kkbox.com/login\",\"websiteKey\":\"6LcuGcoUAAAAAB8E-zI7hoiQ_fcudMnk9YVZtW4m\",\"minScore\":0.9,\"pageAction\":\"login\",\"type\":\"RecaptchaV3TaskProxyless\"}}";
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
                                
                                string status = data.status;
                                if (status.Equals("ready"))
                                {
                                    Recaptcha recaptcha = new Recaptcha()
                                    {
                                        Token = data.solution.text,
                                        Time = DateTime.Now
                                    };
                                    PoolData.Add(recaptcha);
                                    break;
                                }
                                else if (!status.Equals("processing"))
                                {
                                    break;
                                }
                            }
                            catch { }

                            Thread.Sleep(3000);
                            timeout -= 3000;
                        }
                    }
                    catch { }
                    finally
                    {
                        if (SolvingSize > 0)
                        {
                            SolvingSize--;
                        }
                    }
                }
                Thread.Sleep(2000);
            }
        }

        public Recaptcha Get()
        {
            if (PoolData.Any())
            {
                Recaptcha recaptcha = PoolData[0];
                PoolData.RemoveAt(0);
                return recaptcha;
            }
            return null;
        }

        public void Push(Recaptcha token)
        {
            PoolData.Add(token);
        }

        public int Size()
        {
            return PoolData.Count;
        }
    }

    public class Recaptcha
    {
        public string Token { get; set; } = string.Empty;

        public DateTime Time { get; set; } = DateTime.MinValue;

        public bool IsExpired()
        {
            return DateTime.Now.Subtract(Time).TotalSeconds >= 100;
        }

        public void WaitForCanUseIt()
        {
            int sec = (int)DateTime.Now.Subtract(Time).TotalSeconds;
            if (sec < 6)
            {
                Thread.Sleep((6 - sec) * 1000);
            }
        }
    }
}
