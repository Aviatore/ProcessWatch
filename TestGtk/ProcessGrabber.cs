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

            Console.WriteLine($"ColumnToSort: {ColumnToSort[0]} {ColumnToSort[1]}");
            if (ColumnToSort[1] != null)
            {
                switch (ColumnToSort[0])
                {
                    case 0:
                        if (ColumnToSort[1] == 0)
                        {
                            OnResult?.Invoke(this, processes.OrderBy(process => process.Id).Select(element => element).ToList());
                            //processesSorted = processes.OrderBy(process => process.Id).Select(element => element).ToArray();
                        }
                        else if (ColumnToSort[1] == 1)
                        {
                            OnResult?.Invoke(this, processes.OrderBy(process => process.Id).Select(element => element).ToList());
                            //processesSorted = processes.OrderByDescending(process => process.Id).Select(element => element).ToArray();
                        }
                        return;
                        break;
                    case 1:
                        if (ColumnToSort[1] == 0)
                        {
                            OnResult?.Invoke(this, processes.OrderBy(process => process.ProcessName).Select(element => element).ToList());
                        }
                        else if (ColumnToSort[1] == 1)
                        {
                            OnResult?.Invoke(this, processes.OrderByDescending(process => process.ProcessName).Select(element => element).ToList());
                        }
                        return;
                        break;
                    case 2:
                        if (ColumnToSort[1] == 0)
                        {
                            OnResult?.Invoke(this, processes.OrderBy(process => process.WorkingSet64).Select(element => element).ToList());
                        }
                        else if (ColumnToSort[1] == 1)
                        {
                            OnResult?.Invoke(this, processes.OrderByDescending(process => process.WorkingSet64).Select(element => element).ToList());
                        }
                        return;
                        break;
                    case 3:
                        if (ColumnToSort[1] == 0)
                        {
                            OnResult?.Invoke(this, processes.OrderBy(process => process.PriorityClass).Select(element => element).ToList());
                        }
                        else if (ColumnToSort[1] == 1)
                        {
                            OnResult?.Invoke(this, processes.OrderByDescending(process => process.PriorityClass).Select(element => element).ToList());
                        }
                        return;
                        break;
                    case 4:
                        if (ColumnToSort[1] == 0)
                        {
                            OnResult?.Invoke(this, processes.OrderBy(process => process.UserProcessorTime).Select(element => element).ToList());
                        }
                        else if (ColumnToSort[1] == 1)
                        {
                            OnResult?.Invoke(this, processes.OrderByDescending(process => process.UserProcessorTime).Select(element => element).ToList());
                        }
                        return;
                        break;
                    case 5:
                        if (ColumnToSort[1] == 0)
                        {
                            OnResult?.Invoke(this, processes.OrderBy(process => process.PrivilegedProcessorTime).Select(element => element).ToList());
                        }
                        else if (ColumnToSort[1] == 1)
                        {
                            OnResult?.Invoke(this, processes.OrderByDescending(process => process.PrivilegedProcessorTime).Select(element => element).ToList());
                        }
                        return;
                        break;
                    case 6:
                        if (ColumnToSort[1] == 0)
                        {
                            OnResult?.Invoke(this, processes.OrderBy(process => process.TotalProcessorTime).Select(element => element).ToList());
                        }
                        else if (ColumnToSort[1] == 1)
                        {
                            OnResult?.Invoke(this, processes.OrderByDescending(process => process.TotalProcessorTime).Select(element => element).ToList());
                        }
                        return;
                        break;
                    case 7:
                        if (ColumnToSort[1] == 0)
                        {
                            Console.WriteLine("Cpu ascending");
                            OnResult?.Invoke(this, processes.OrderBy(process => process.CpuUsage).Select(element => element).ToList());
                        }
                        else if (ColumnToSort[1] == 1)
                        {
                            Console.WriteLine("Cpu descending");
                            OnResult?.Invoke(this, processes.OrderByDescending(process => process.CpuUsage).Select(element => element).ToList());
                        }
                        return;
                        break;
                    case 8:
                        if (ColumnToSort[1] == 0)
                        {
                            OnResult?.Invoke(this, processes.OrderBy(process => process.ThreadCount).Select(element => element).ToList());
                        }
                        else if (ColumnToSort[1] == 1)
                        {
                            OnResult?.Invoke(this, processes.OrderByDescending(process => process.ThreadCount).Select(element => element).ToList());
                        }
                        return;
                        break;
                    case 9:
                        if (ColumnToSort[1] == 0)
                        {
                            OnResult?.Invoke(this, processes.OrderBy(process => process.StartTime).Select(element => element).ToList());
                        }
                        else if (ColumnToSort[1] == 1)
                        {
                            OnResult?.Invoke(this, processes.OrderByDescending(process => process.StartTime).Select(element => element).ToList());
                        }
                        return;

                        break;
                    default:
                        Console.WriteLine("err");
                        break;
                }
            }
            else
            {
                //processesSorted = processes;
            }

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

            OnResult?.Invoke(this, processes.ToList());
        } 
    }
}