using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace VoliPick
{
    public partial class VoliForm  
    {
        public HttpClient Client = new HttpClient();
        public Thread lcuConnectThread;
        public bool connected = false;
        public Process lcu;

        public void LcuConnectThread()
        {
            
            lcuConnectThread = new Thread(LcuConnect);
            lcuConnectThread.IsBackground = true;
            lcuConnectThread.Name = "lcuConnectThread";
            lcuConnectThread.Start();
        }
        public void LcuConnect()
        {
            while (!cbbChampList.IsHandleCreated && !btPickLock.IsHandleCreated) { Thread.Sleep(200); }
            cbbChampList.Invoke((Action)delegate () {
                cbbChampList.DataSource = null;
                cbbChampList.Text = "";
                cbbChampList.Enabled = false;
            });
            btPickLock.Invoke((Action)delegate () {
                btPickLock.Enabled = false;
                //btPickLock.Text = "Pick && Lock";
            });
            tooltipStatus.Text = "Status: Connecting to LoL..";

            lcu = null;
            while (lcu == null)
            {
                lcu = Process.GetProcessesByName("LeagueClientUx").FirstOrDefault();
                Thread.Sleep(1000);
            }
            
            // Tạo sự kiện khi lol tắt 
            lcu.EnableRaisingEvents = true;
            lcu.Exited += Lcu_Exited;
            LcuGetAuthInfo();
            LcuAuth(pass);

            AddChampToCBBThread();
            MatchThread();
            PickLockThreadSetting();
            if (switchPickLock == 2)
            {
                pickLockThread.Start();
            }
        }

        public void LcuGetAuthInfo()
        {
            var lcuConnect = new Process();
            lcuConnect.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/c WMIC PROCESS WHERE name='LeagueClientUx.exe' GET commandline",
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            lcuConnect.Start();
            string authInfo = lcuConnect.StandardOutput.ReadToEnd().Trim();
            lcuConnect.WaitForExit();
            lcuConnect.Close();
            port = new Regex("--app-port=(.*?)\"").Match(authInfo).Groups[1].Value;
            pass = new Regex("--remoting-auth-token=(.*?)\"").Match(authInfo).Groups[1].Value;
            GC.Collect();
            if (pass == null || port == null)
            {
                MyMessageBox(1, "Error 0x669966. Program will exit.", "VoliPick");
                Environment.Exit(0);
            }

            
        }

        public void LcuAuth(string pass)
        {
            // Encrypt pass and user
            string accountEncrypt = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"riot:{pass}"));

            // disable SSL
            ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => true;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;

            // Authorization
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accountEncrypt);

            // Qua bước xác thực => được thao tác với api => connect
            connected = true;
            GC.Collect();
        }

        public void Lcu_Exited(object sender, EventArgs e)
        {
            connected = false;
            //switchPickLock = 1;
            selectedIndx = cbbChampList.SelectedIndex;
            matchThread.Abort();
            if (pickLockThread.IsAlive)
                pickLockThread.Abort();
            lcuConnectThread.Abort();
            Thread.Sleep(500);
            LcuConnectThread();
        }
    }
}
