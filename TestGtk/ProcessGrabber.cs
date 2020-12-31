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
        public int?[] ColumnToSort { get; set; }
        public event EventHandler<List<ProcessMod>> OnResult;
        private Timer aTimer;
        
        public ProcessGrabber()
        {
            ColumnToSort = new int?[2];
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
            //IEnumerable<ProcessMod> processesSorted = processes.OrderByDescending(process => process.CpuUsage);
            //IEnumerable<ProcessMod> processesSorted = null;
            //ProcessMod[] processesSorted = processes.OrderBy(process => process.Id).Select(element => element).ToArray();

            OnResult?.Invoke(this, processes.OrderBy(process => process.Id).Select(element => element).ToList());
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

            //OnResult?.Invoke(this, processes.ToList());
            
            OnResult?.Invoke(this, processes.OrderByDescending(process => process.CpuUsage).Select(element => element).ToList());
        } 
    }
}