using System;
using System.Timers;
using Gtk;

namespace TestGtk
{
    public class WindowBuilder
    {
        private static ProcessGrabber _updater;
        private static NodeStore store;

        public WindowBuilder()
        {
            store = new NodeStore(typeof(MyTreeNode));
            Window window;

            Application.Init ();
            
            window = new Window ("Label sample");
            window.Resize(400, 600);
            window.Title = "Label";
            window.BorderWidth = 5;
            
            window.DeleteEvent += delete_event;
            
            HBox hbox = new HBox (false, 5);
            
            VBox vbox = new VBox (false, 5);
            
            Button button = new Button("Start");
            button.Clicked += Start;
            
            Button button2 = new Button("Stop");
            button2.Clicked += Stop;
            
            hbox.PackStart(button, false, false, 0);
            hbox.PackStart(button2, false, false, 0);
            
            window.Add (vbox);
            vbox.PackStart (hbox, false, false, 0);
            
            Frame frame = new Frame ("Processes");
            VBox processesVBox = new VBox(false, 5);
            
            string[] columnLabels = {
                "Process name",
                "Process Id",
                "WorkingSet64",
                "CPU usage"
            };
            
            CellRendererText render = new CellRendererText();
            render.Alignment = Pango.Alignment.Right;
            render.Xalign = 0.5f;
            
            NodeView view = new NodeView();
            
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
                
                view.AppendColumn(column);
            }
            
            processesVBox.PackStart(view, false, false, 0);
            
            frame.Add(processesVBox);

            vbox.PackStart(frame, false, false, 0);
            
            // Create an instance of the object Updater
            _updater = new ProcessGrabber();
            _updater.OnResult += (sender, list) =>
            {
                Application.Invoke(delegate
                {
                    store = null;
                    store = new NodeStore(typeof(MyTreeNode));

                    foreach (var element in list)
                    {
                        store.AddNode(new MyTreeNode(element.ProcessName, element.Id.ToString(), ProcessMod.FormatMemSize(element.WorkingSet64), ProcessMod.FormatCpuUsage(element.CpuUsage)));
                    }

                    view.NodeStore = store;

                    window.ShowAll();
                    view.ShowAll();
                });
            };
            
            // Fill up TreeView with empty data
            // This prevents a problem with the empty display after launching the program
            for (int i = 0; i < 15; i ++)
            {
                store.AddNode(new MyTreeNode());
            }

            view.NodeStore = store;
            view.ShowAll();
            window.ShowAll();
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
        
        public void Run()
        {
            Application.Run();
        }
    }
}