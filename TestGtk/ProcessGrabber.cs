using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

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
            SetTimer();
        }

        public void Stop()
        {
            aTimer.Stop();
            aTimer.Dispose();
        }

        private void SetTimer()
        {
            aTimer = new Timer(1000);
            aTimer.Elapsed += GetData;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        
        private void GetData(object source, ElapsedEventArgs args)
        {
            //Console.WriteLine("ok");
            List<string> output = new List<string>();
            ProcessMod[] processes = ProcessMod.GetProcesses();
            IEnumerable<ProcessMod> processesSorted = processes.OrderByDescending(process => process.CpuUsage).Take(15);

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