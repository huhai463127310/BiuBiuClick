using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Generic;

namespace BiuBiuClick
{
class KeyController
    {
        private Dispatcher dispatcher;
        private  delegate void SendKeysToProcessDelegate(Process p, string key);
        private static object objLock = new object();

        public KeyController(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        class SendKeysToProcessThreadMain
        {
            private readonly Process p;
            private readonly string key;
            private readonly Dispatcher dispatcher;
            private readonly SendKeysToProcessDelegate d;

            public SendKeysToProcessThreadMain(Dispatcher dispatcher, SendKeysToProcessDelegate d, Process p, string key)
            {
                this.dispatcher = dispatcher;
                this.d = d;
                this.p = p;
                this.key = key;
            }

            public void Run()
            {
                /*this.dispatcher.BeginInvoke(
                    d, new object[]{ p, key}
                );*/
                d(p, key);
            }
        }

        public void sendKeyToProcessDefault(string processName, string key)
        {
            Process p = Process.GetProcessesByName(processName).FirstOrDefault();
            sendKeyToProcess(p, key);
        }

        public int sendKeyToProcessAll(string processName, string key)
        {
            Process[] ps = Process.GetProcessesByName(processName);
            if (ps.Length > 0) {
                List<Thread> threadList = new List<Thread>();
                Stopwatch sw = new Stopwatch();
                long[] timeSpans = new long[ps.Length];
                for(int i = 0; i < ps.Length; i++)
                {
                    Process p = ps[i];
                    sw.Start();
                    sendKeyToProcess(p, key);
                    sw.Stop();
                    timeSpans[i] = sw.ElapsedMilliseconds;                    
                }
                for (int i = 0; i < ps.Length; i++)
                {
                    Process p = ps[i];
                    Console.WriteLine("send key [{0}] to process {1}, cost {2} ms", key, p.Id, timeSpans[i]);
                }
                    
                /*foreach (Process p in ps)
                {
                    Console.WriteLine("found process " + processName + ". handle id is " + p.MainWindowHandle + ". process id is " + p.Id);
                    SendKeysToProcessDelegate d = new SendKeysToProcessDelegate(sendKeyToProcess);
                    SendKeysToProcessThreadMain tm = new SendKeysToProcessThreadMain(dispatcher, d, p, key); 
                    Thread thread = new Thread(new ThreadStart(tm.Run));
                    threadList.Add(thread);
                }
                foreach (Thread thread in threadList)
                {
                    thread.Start();
                    Console.WriteLine("start send keys thread " + thread.ManagedThreadId);
                }
                foreach (Thread thread in threadList)
                {
                    thread.Join();
                }*/
            }
            return ps.Length;
        }

        public void sendKeyToProcess(Process p, string key)
        {
            if (p != null)
            {
                IntPtr h = p.MainWindowHandle;
                
                //bool showWindowResult = WindowHelper.ShowWindow(h, WindowHelper.SHOW_WINDOW_CMD.SW_RESTORE);
                int setForegroundWindow = WindowHelper.SetForegroundWindow(h);
                //Console.WriteLine("handle id is " + h + ", process id is " + p.Id + ", showWindowResult = " + showWindowResult + ", setForegroundWindow=" + setForegroundWindow);
                              
                string[] keyParts = key.Split(',');
                foreach (string keyPart in keyParts)
                {
                    //Console.WriteLine("send key part [" + keyPart + "] to process " + p.ProcessName + "  which process id is " + p.Id + " handle id is " + h + ". current thread is " + Thread.CurrentThread.ManagedThreadId);
                    SendKeys.SendWait(keyPart);
                    SendKeys.Flush();
                }
                //Console.WriteLine("send key [" + key + "] to process " + p.ProcessName + "  which process id is " + p.Id + " handle id is " + h + ". current thread is " + Thread.CurrentThread.ManagedThreadId);
                
            }
        }

    }

}
