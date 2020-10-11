using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VoliPick
{
    public partial class VoliForm
    {
        
        public string pass = null, port = null;
        public int champId = -1;
        public bool isAutoMatch = true;
        public Thread matchThread;
        public Thread pickLockThread;
        
        public bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public void PickLockThreadSetting()
        {
            pickLockThread = new Thread(LockAndPickChamp);
            pickLockThread.IsBackground = true;
        }
        public void PickLockThread()
        {
            pickLockThread.Start();
        }
        public void LockAndPickChamp()
        {
            int actionId;
            while (true)
            {
                actionId = GetActionId();
                if (actionId != -1)
                {
                    Request("Patch", $"/lol-champ-select/v1/session/actions/{actionId}",
                        "{\"championId\":" + champId + "}");
                    Thread.Sleep(200);
                    Request("Post", $"/lol-champ-select/v1/session/actions/{actionId}/complete");
                    tooltipStatus.Text = "Status: Enjoy your champion";
                }
                Thread.Sleep(300);
            }
        }
        public int GetActionId()
        {
            // lay so thu tu pick tuong
            int getPlayerId = -1;
            bool isComplete = true;
            bool isInProgress = false;
            string type = "";
            var responeChampSlect = Request("Get", $"/lol-champ-select/v1/session");
            if (responeChampSlect == null)
                return -1;
            try
            {
                dynamic jsonChampSlect = JObject.Parse(responeChampSlect);
                string getLocalPlayerCellId = jsonChampSlect.localPlayerCellId;
                string actorCellId;
                //int actionIndex = jsonChampSlect.actions.cout - 1;
                //MessageBox.Show(jsonChampSlect.actions.cout - 1);
                for (int i = 0; i < 23; i++)
                {
                    actorCellId = jsonChampSlect.actions[0][i].actorCellId;
                    if (actorCellId == getLocalPlayerCellId)
                    {
                        getPlayerId = jsonChampSlect.actions[0][i].id;
                        isComplete = jsonChampSlect.actions[0][i].completed;
                        type = jsonChampSlect.actions[0][i].type;
                        isInProgress = jsonChampSlect.actions[0][i].isInProgress;
                    }

                }
            }
            catch { }

            if (getPlayerId != -1 && isComplete == false && type == "pick" && isInProgress == true)
                return getPlayerId;
            return -1;
        }
        public void MatchThread()
        {
            matchThread = new Thread(FindMatch);
            matchThread.IsBackground = true;
            matchThread.Start();
        }
        public void DeclineMatch()
        {
            Request("Post", $"/lol-matchmaking/v1/ready-check/decline");
        }
        public void AcceptMatch()
        {
            Request("Post", $"/lol-matchmaking/v1/ready-check/accept");
        }
        public void FindMatch()
        {
            addChampToCBBThread.Join();
            string respones = null;
            while (true)
            {
                if (ckbAccessMatch.Checked)
                {
                    respones = Request("Get", $"/lol-matchmaking/v1/ready-check");
                    if (respones != null)
                    {
                        dynamic JsonParse = JObject.Parse(respones);
                        if (JsonParse.state == "InProgress" && JsonParse.playerResponse == "None")
                        {
                            AcceptMatch();
                            tooltipStatus.Text = "Status: Accept match";
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public string Request(string method, string uri, string body = null)
        {
            if (!connected) return null;

            try
            {
                var content = (body != null) ?
                    new StringContent(body, System.Text.Encoding.UTF8, "application/json") : null;

                var response = Client.SendAsync(new HttpRequestMessage(
                    new HttpMethod(method), "https://127.0.0.1:" + port + uri)
                {
                    Content = content
                }).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    return responseContent.ReadAsStringAsync().Result;
                }
            }
            catch { }

            return null;
        }
    }
}
