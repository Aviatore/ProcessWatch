using System;
using System.Collections.Generic;
using System.Timers;
using Gtk;

namespace TestGtk
{
    public class WindowBuilder
    {
        private static ProcessGrabber _updater;
        private static ListStore store;
        private TreeView tree;
        private ScrolledWindow scrolledWindow;
        private double _currentScrollPosition;

        public WindowBuilder()
        {
            store = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string));
            Window window;

            Application.Init ();
            
            window = new Window ("Label sample");
            window.Resize(500, 600);
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

            scrolledWindow = new ScrolledWindow();
            scrolledWindow.HeightRequest = 400;
            
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
            
            tree = new TreeView();
            tree.Model = store;
            
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

                tree.AppendColumn(column);
            }
            
            scrolledWindow.Add(tree);
            //processesVBox.PackStart(tree, false, false, 0);
            
            frame.Add(scrolledWindow);

            vbox.PackStart(frame, false, false, 0);

            Button killButton = new Button("Kill process");
            vbox.PackStart(killButton, false, false, 0);
            
            // Create an instance of the object Updater
            _updater = new ProcessGrabber();
            _updater.OnResult += (sender, list) =>
            {
                Application.Invoke(delegate
                {
                    _currentScrollPosition = tree.Vadjustment.Value;
                    //store.Clear();
                    StoreClear();
                    LoadStore(list);

                    /*
                    foreach (var element in list)
                    {
                        store.AppendValues(element.ProcessName, element.Id.ToString(),
                            ProcessMod.FormatMemSize(element.WorkingSet64),
                            ProcessMod.FormatCpuUsage(element.CpuUsage));
                    }
                    */
                    window.ShowAll();
                    tree.ShowAll();
                });
            };

            tree.Vadjustment.Changed += (sender, args) =>
            {
                tree.Vadjustment.Value = _currentScrollPosition;
            }; 
            
            // Fill up TreeView with empty data
            // This prevents a problem with the empty display after launching the program
            for (int i = 0; i < 15; i ++)
            {
                store.AppendValues("", "", "", "");
            }
            
            //tree.NodeSelection.Changed += OnSelectionChanged;
            //tree.NodeStore = store;
            tree.Selection.Mode = SelectionMode.Multiple;
            
            tree.ShowAll();
            window.ShowAll();
        }

        private void StoreClear()
        {
            TreeIter iter;
            store.GetIterFirst(out iter);
            for (int i = 0; i < store.IterNChildren(); i++)
            {
                store.SetValues(iter, "", "", "", "");

                store.IterNext(ref iter);
            }
        }

        private void LoadStore(List<ProcessMod> element)
        {
            int elementIndex = 0;
            TreeIter iter;
            store.GetIterFirst(out iter);
            for (int i = 0; i < store.IterNChildren(); i++)
            {
                if (element.Count - 1 > i)
                {
                    store.SetValues(iter, element[i].ProcessName, element[i].Id.ToString(),
                        ProcessMod.FormatMemSize(element[i].WorkingSet64),
                        ProcessMod.FormatCpuUsage(element[i].CpuUsage));
                    
                    store.IterNext(ref iter);
                }
                else
                {
                    store.Remove(ref iter);
                }
                
                elementIndex++;
            }

            if (element.Count > elementIndex)
            {
                for (int i = elementIndex; i < element.Count; i++)
                {
                    store.AppendValues(element[i].ProcessName, element[i].Id.ToString(),
                        ProcessMod.FormatMemSize(element[i].WorkingSet64),
                        ProcessMod.FormatCpuUsage(element[i].CpuUsage));
                }
            }
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
        
        static void OnSelectionChanged(object o, EventArgs args)
        {
            NodeSelection selection = (NodeSelection)o;
            MyTreeNode node = (MyTreeNode) selection.SelectedNode;
            if (node != null)
            {
                Console.WriteLine(node.ProcessName);
                _updater.Stop();
            }
            else
            {
                Console.WriteLine("Node is null.");
            }
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