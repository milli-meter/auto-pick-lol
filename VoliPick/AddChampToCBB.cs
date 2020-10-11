using System;
using System.Threading;
using System.Threading.Tasks;

namespace VoliPick
{
    public partial class VoliForm
    {
        public Thread addChampToCBBThread;
        public ChampList champList;
        public void AddChampToCBBThread()
        {
            addChampToCBBThread = new Thread(AddChampToCBB);
            addChampToCBBThread.IsBackground = true;
            addChampToCBBThread.Name = "AddChampToCBBThread";
            addChampToCBBThread.Start();
        }
        public void AddChampToCBB()
        {
            //Console.Clear();
            Console.WriteLine(Thread.CurrentThread.Name);
            lcuConnectThread.Join();
            string json = null;
            while (json == null || json == "[]") { json = Request("Get", "/lol-champions/v1/owned-champions-minimal"); Thread.Sleep(500); }
            tooltipStatus.Text = "Status: Choose your champion";
            Action fun = () =>
            {
                Console.WriteLine("aaaaa");
            };
            cbbChampList.Invoke((Action)delegate ()
            {
                champList = new ChampList();
                champList.GetChampList(json);
                cbbChampList.Enabled = true;
                cbbChampList.DataSource = champList.ChampNameList;
                cbbChampList.Focus();
                cbbChampList.SelectedIndex = selectedIndx;
                cbbChampList.SelectionStart = 0;
                cbbChampList.SelectionLength = cbbChampList.Text.Length;
            });

            btPickLock.Invoke((Action)delegate ()
            {
                btPickLock.Enabled = true;
            });
            GC.Collect();
        }
    }
}
