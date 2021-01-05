using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using TestGtk.Model;
using Timer = System.Timers.Timer;

namespace TestGtk.Controller
{
    public class ProcessGrabber
    {
        private Thread _thread;
        private Timer _aTimer;
        public event EventHandler<List<ProcessMod>> OnResult;

        public ProcessGrabber()
        {

        }

        public void Run()
        {
            _thread = new Thread(new ThreadStart(GetDataExecute));
            _thread.Start();
            
            SetTimer();
        }

        public void Stop()
        {
            _aTimer.Stop();
            _aTimer.Dispose();
        }

        private void SetTimer()
        {
            _aTimer = new Timer(3000);
            _aTimer.Elapsed += GetData;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;
        }
        
        private void GetData(object source=null, ElapsedEventArgs args=null)
        {
            Console.WriteLine("tick");
            //GetDataExecute();
            ProcessMod[] processes = ProcessMod.GetProcesses();

            OnResult?.Invoke(this, processes.ToList());
        }

        private void GetDataExecute()
        {
            ProcessMod[] processes = ProcessMod.GetProcesses();

            OnResult?.Invoke(this, processes.ToList());
        }
    }
}