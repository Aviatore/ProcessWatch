using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TestGtk
{
    public class ProcessMod
    {
        public string ProcessName { get;  set; }
        public double CpuUsage { get;  set; }
        public double Id { get;  set; }
        public long WorkingSet64 { get;  set; }
        public TimeSpan TotalProcessorTime { get;  set; }
        
        public ProcessMod()
        {
            
        }

        public static ProcessMod[] GetProcesses()
        {
            List<ProcessMod> proc = new List<ProcessMod>();
            
            Process[] processes = Process.GetProcesses();
            
            List<Task<ProcessMod>> tasks = new List<Task<ProcessMod>>();
            
            foreach (var process in processes)
            {
                tasks.Add(GetCpuUsageAsync(process));
            }

            Task task = Task.WhenAll(tasks);
            task.Wait();

            for (int i = 0; i < tasks.Count; i++)
            {
                proc.Add(tasks[i].Result);
            }

            foreach (var process in processes)
            {
                process.Dispose();
            }

            return proc.ToArray();
        }

        private static async Task<ProcessMod> GetCpuUsageAsync(Process proc)
        {
            DateTime startTime = DateTime.UtcNow;
            var startCpuUsage = proc.TotalProcessorTime;
        
            await Task.Delay(500);
        
            DateTime endTime = DateTime.UtcNow;
            
            var endCpuUsage = proc.TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            ProcessMod tmp = new ProcessMod();
            tmp.ProcessName = proc.ProcessName;
            tmp.Id = proc.Id;
            tmp.WorkingSet64 = proc.WorkingSet64;
            tmp.CpuUsage = cpuUsageTotal * 100 * 4;
            //Console.WriteLine(tmp.CpuUsage);
            
            return tmp;
        }

        public static string FormatCpuUsage(double cpuUsage)
        {
            return $"{cpuUsage:0.#}%";
        }
        
        public static string FormatMemSize(double size)
        {
            double d = (double)size;
            int i = 0;
            while ((d > 1024) && (i < 5))
            {
                d /= 1024;
                i++;
            }
            string[] unit = { "B", "KB", "MB", "GB", "TB" };
            return $"{Math.Round(d, 2)} {unit[i]}";
            //return (string.Format("{0} {1}", Math.Round(d, 2), unit[i]));
        }
    }
}