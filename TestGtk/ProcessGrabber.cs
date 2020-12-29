using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading;
using Gtk;
using Timer = System.Timers.Timer;

namespace TestGtk
{
    public class ProcessGrabber
    {
        public event EventHandler<List<ProcessMod>> OnResult;
        private Timer aTimer;
        
        public ProcessGrabber()
        {
            
        }

        public void Run()
        {
            Thread t = new Thread(new ThreadStart(GetData2));
            t.Start();
            
            SetTimer();
        }

        public void Stop()
        {
            aTimer.Stop();
            aTimer.Dispose();
        }

        private void SetTimer()
        {
            aTimer = new Timer(3000);
            aTimer.Elapsed += GetData;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        
        private void GetData(object source=null, ElapsedEventArgs args=null)
        {
            //Console.WriteLine("ok");
            List<string> output = new List<string>();
            ProcessMod[] processes = ProcessMod.GetProcesses();
            //IEnumerable<ProcessMod> processesSorted = processes.OrderByDescending(process => process.CpuUsage).Take(15);
            IEnumerable<ProcessMod> processesSorted = processes.OrderByDescending(process => process.CpuUsage);

            foreach (var process in processesSorted)
            {
                string data =
                    $"{process.Id.ToString()}\t{process.ProcessName}\t{process.CpuUsage:0.#}%\t{ProcessMod.FormatMemSize(process.WorkingSet64)}";
                output.Add(data);
            }
            
            OnResult?.Invoke(this, processesSorted.ToList());
        }
        
        public void GetData2()
        {
            //Console.WriteLine("ok");
            List<string> output = new List<string>();
            ProcessMod[] processes = ProcessMod.GetProcesses();
            //IEnumerable<ProcessMod> processesSorted = processes.OrderByDescending(process => process.CpuUsage).Take(15);
            IEnumerable<ProcessMod> processesSorted = processes.OrderByDescending(process => process.CpuUsage);

            foreach (var process in processesSorted)
            {
                string data =
                    $"{process.Id.ToString()}\t{process.ProcessName}\t{process.CpuUsage:0.#}%\t{ProcessMod.FormatMemSize(process.WorkingSet64)}";
                output.Add(data);
            }

            OnResult?.Invoke(this, processesSorted.ToList());
        } 
    }
}