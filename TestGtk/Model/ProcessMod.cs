using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace TestGtk.Model
{
    public class ProcessMod
    {
        public string ProcessName { get;  set; }
        public double CpuUsage { get;  set; }
        public double Id { get;  set; }
        public string PriorityClass { get; set; }
        public double UserProcessorTime { get; set; }
        public double PrivilegedProcessorTime { get; set; }
        public int ThreadCount { get; set; }
        
        public long WorkingSet64 { get;  set; }
        public double TotalProcessorTime { get;  set; }
        public long StartTime { get; set; } 
        
        public ProcessMod()
        {
            
        }

        /// <summary>
        /// A method that manages grabbing data from all processes.
        /// </summary>
        /// <returns>An array of ProcessMod objects</returns>
        public static ProcessMod[] GetProcesses()
        {
            Process[] processes = Process.GetProcesses();
            
            // The list is initialized with the specific size to solve memory allocation issues
            List<ProcessMod> proc = new List<ProcessMod>(processes.Length);
            
            List<Task<ProcessMod>> tasks = new List<Task<ProcessMod>>(processes.Length);
            
            // Load the list 'tasks' with the async method 'GetCpuUsageAsync' for each process
            foreach (var process in processes)
            {
                tasks.Add(GetCpuUsageAsync(process));
            }

            // Launch all tasks asynchronously
            Task task = Task.WhenAll(tasks);
            
            // Wait until all tasks finished
            task.Wait();

            // Load the list 'proc' with 'ProcessMod' objects returned by each task
            for (int i = 0; i < tasks.Count; i++)
            {
                proc.Add(tasks[i].Result);
            }

            // Cleaning up
            foreach (var process in processes)
            {
                process.Dispose();
            }
            
            return proc.ToArray();
        }

        /// <summary>
        /// Get data about every process and calculate CPU usage in %.
        /// All grabbed data are stored in a new instance of the ProcessMod class.
        /// </summary>
        /// <param name="proc">An instance of the Process class</param>
        /// <returns>An instance of the Task class that returns a ProcessMod object</returns>
        private static async Task<ProcessMod> GetCpuUsageAsync(Process proc)
        {
            DateTime startTime = DateTime.UtcNow;
            var startCpuUsage = proc.TotalProcessorTime;
            
            // Wait 500 ms
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
            tmp.PriorityClass = proc.PriorityClass.ToString();
            tmp.UserProcessorTime = proc.UserProcessorTime.Milliseconds;
            tmp.PrivilegedProcessorTime = proc.PrivilegedProcessorTime.Milliseconds;
            tmp.TotalProcessorTime = proc.TotalProcessorTime.Milliseconds;
            tmp.ThreadCount = proc.Threads.Count;
            tmp.StartTime = proc.StartTime.Ticks;
            //Console.WriteLine(tmp.CpuUsage);
            return tmp;
        }

        /// <summary>
        /// Format CPU usage
        /// </summary>
        /// <param name="cpuUsage">CPU usage in %, but not formatted</param>
        /// <returns>Formatted CPU usage suffixed with percent character.</returns>
        public static string FormatCpuUsage(double cpuUsage)
        {
            return $"{cpuUsage:0.#} %";
        }
        
        /// <summary>
        /// Format memory usage
        /// </summary>
        /// <param name="size">Memory usage in bytes</param>
        /// <returns>Formatted memory usage suffixed with appropriate unit.</returns>
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
            return $"{Math.Round(d, 2).ToString(CultureInfo.CurrentCulture)} {unit[i]}";
            //return (string.Format("{0} {1}", Math.Round(d, 2), unit[i]));
        }
        
        /// <summary>
        /// Format the start time of the process
        /// </summary>
        /// <param name="ticks">Total number of ticks that passed during process launch</param>
        /// <returns>Properly formatted time</returns>
        public static string FormatTime(long ticks)
        {
            DateTime dateTime = new DateTime(ticks);

            return dateTime.ToString("G", CultureInfo.CreateSpecificCulture("pl-PL"));
        }
        
        /// <summary>
        /// Format the CPU time
        /// </summary>
        /// <param name="size">Raw CPU time in ms</param>
        /// <returns>Formatted time suffixed with 'ms'</returns>
        public static string FormatTimeMs(double size)
        {
            return $"{size.ToString(CultureInfo.InvariantCulture)} ms";
        }
    }
}