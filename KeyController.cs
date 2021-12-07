using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace BiuBiuClick
{
    class KeyController
    {
       

        public void sendKeyToProcessDefault(string processName, string key)
        {
            Process p = Process.GetProcessesByName(processName).FirstOrDefault();
            sendKeyToProcess(p, key);
        }

        public void sendKeyToProcessAll(string processName, string key)
        {
            Process[] ps = Process.GetProcessesByName(processName);
            foreach (Process p in ps)
            {
                sendKeyToProcess(p, key);
            }            
        }

        public void sendKeyToProcess(Process p, string key)
        {
            if (p != null)
            {
                IntPtr h = p.MainWindowHandle;
                WindowHelper.SetForegroundWindow(h);
                string[] keyParts = key.Split(',');
                foreach (string keyPart in keyParts)
                {
                    System.Windows.Forms.SendKeys.SendWait(keyPart);                    
                }
                Console.WriteLine("send key {" + key + "} to process " + p.ProcessName + "  which process id is " + h);
            }
        }

        public static void Main(string[] args)
        {
            KeyController kc = new KeyController();
            kc.sendKeyToProcessAll("PotPlayerMini", " ");
        }
    }

}
