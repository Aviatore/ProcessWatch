using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using GLib;
using Gtk;
using Application = Gtk.Application;
using Timer = System.Timers.Timer;

namespace TestGtk
{
    class Program
    {
        private static Timer _timer = new Timer();
        static ThreadNotify notify;
        private static Updater _updater;
        private static NodeStore store;
        
        static void Main(string[] args)
        {
            Widget widget;
            Window window;
            HBox hbox;
            VBox vbox;
            Frame frame;
            Label label;
            store = new NodeStore(typeof(MyTreeNode));

            Application.Init ();


            window = new Window ("Label sample");
            window.Resize(400, 600);

            window.DeleteEvent += delete_event;

            window.Title = "Label";
            window.BorderWidth = 5;
            
            hbox = new HBox (false, 5);
            vbox = new VBox (false, 5);

            Button button = new Button("Start");
            button.Clicked += Start;
            
            Button button2 = new Button("Stop");
            button2.Clicked += Stop;
            
            hbox.PackStart(button, false, false, 0);
            hbox.PackStart(button2, false, false, 0);
            
            window.Add (vbox);
            vbox.PackStart (hbox, false, false, 0);

            frame = new Frame ("Processes");
            VBox processesVBox = new VBox(false, 5);
            
            NodeView view = new NodeView();
            string[] columnLabels = new string[]
            {
                "Process name",
                "Process Id",
                "WorkingSet64",
                "CPU usage"
            };
            
            CellRendererText render = new CellRendererText();
            render.Alignment = Pango.Alignment.Center;
            
            for (int i = 0; i < 4; i++)
            {
                TreeViewColumn column = new TreeViewColumn();
                column.Clickable = true;
                column.Resizable = true;
                column.Title = columnLabels[i];
                column.SortIndicator = true;
                column.Alignment = 0.5f;
                column.Expand = true;
                column.PackStart(render, true);
                column.AddAttribute(render, "text", i);

                //view.AppendColumn(columnLabels[i], render, "text", i);
                view.AppendColumn(column);
            }
            
            processesVBox.PackStart(view, false, false, 0);

            frame.Add(processesVBox);

            vbox.PackStart(frame, false, false, 0);
            
            //frame.Add (label);

            //notify = new ThreadNotify(new ReadyEvent(() => { Updater(vbox);}));
            //Thread thr = new Thread(new ThreadStart(() => { Updater(vbox);}));

            _updater = new Updater();
            _updater.OnResult += (sender, list) =>
            {
                Application.Invoke(delegate
                {
                    store = null;
                    store = new NodeStore(typeof(MyTreeNode));

                    foreach (var element in list)
                    {
                        store.AddNode(new MyTreeNode(element.ProcessName, element.Id.ToString(), element.WorkingSet64.ToString(), element.CpuUsage.ToString()));
                    }

                    view.NodeStore = store;

                    
                    
                    /*
                    foreach (var element in processesVBox.Children)
                    {
                        processesVBox.Remove(element);
                    }

                    foreach (var element in list)
                    {
                        label = new Label(element);
                        processesVBox.PackStart(label, false, false, 0);
                    }
                    */
                    
                    window.ShowAll();
                    view.ShowAll();
                });
            };

/*
            _timer.Interval = 1000;
            //_timer.AutoReset = false;
            _timer.Elapsed += (sender, eventArgs) =>
            {
                Updater(vbox);
            };
            _timer.Start();
*/
            for (int i = 0; i < 15; i ++)
            {
                store.AddNode(new MyTreeNode());
            }

            view.NodeStore = store;
            view.ShowAll();
            window.ShowAll();

            //thr.Start();
            Application.Run();
        }

        static void Start(object sender, EventArgs args)
        {
            _updater.Run();
        }
        
        static void Stop(object sender, EventArgs args)
        {
            _updater.Stop();
        }

        static void delete_event (object obj, DeleteEventArgs args)
        {
            _updater.Stop();
            Application.Quit();
        }
 
        static void exitbutton_event (object obj, EventArgs args)
        {
            Application.Quit();
        }
    }

    public class Updater
    {
        public event EventHandler<List<ProcessMod>> OnResult;
        private Timer aTimer;
        
        public Updater()
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
            Console.WriteLine("ok");
            List<string> output = new List<string>();
            ProcessMod[] processes = ProcessMod.GetProcesses();
            IEnumerable<ProcessMod> processesSorted = processes.OrderByDescending(process => process.CpuUsage).Take(15);

            foreach (var process in processesSorted)
            {
                string data =
                    $"{process.Id.ToString()}\t{process.ProcessName}\t{process.CpuUsage.ToString():0.#}%\t{ProcessMod.FormatMemSize(process.WorkingSet64)}";
                output.Add(data);
            }
            
            OnResult?.Invoke(this, processesSorted.ToList());
        }   
    }

    [TreeNode(ListOnly = true)]
    public class MyTreeNode : TreeNode
    {
        [TreeNodeValue(Column = 0)]
        public string ProcessName { get; set; }
        
        [TreeNodeValue(Column = 1)]
        public string Id { get; set; }
        
        [TreeNodeValue(Column = 2)]
        public string WorkingSet64 { get; set; }
        
        [TreeNodeValue(Column = 3)]
        public string CpuUsage { get; set; }
        
        public MyTreeNode(string processName, string id, string workingSet64, string cpuUsage)
        {
            ProcessName = processName;
            Id = id;
            WorkingSet64 = workingSet64;
            CpuUsage = cpuUsage;
        }

        public MyTreeNode()
        {
            ProcessName = "";
            Id = "";
            WorkingSet64 = "";
            CpuUsage = "";
        }
    }
}