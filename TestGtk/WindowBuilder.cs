using System;
using System.Collections.Generic;
using System.Timers;
using Gtk;
using System.Diagnostics;


namespace TestGtk
{
    public class WindowBuilder
    {
        private static ProcessGrabber _updater;
        private static ListStore store;
        private TreeView tree;
        private ScrolledWindow scrolledWindow;
        private double _currentScrollPosition;
        private List<int> _processIdToKill;

        public WindowBuilder()
        {
            _processIdToKill = new List<int>();
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

            TreeModelSort sortable = new TreeModelSort(store);
            sortable.SetSortFunc(0, ProcessNameSortFunc);
            sortable.SetSortFunc(1, IdSortFunc);
            sortable.SetSortFunc(2, WorkingSetSortFunc);
            sortable.SetSortFunc(3, CpuUsageSortFunc);
            tree = new TreeView();
            tree.Model = sortable;
            
            for (int i = 0; i < 4; i++)
            {
                TreeViewColumn column = new TreeViewColumn();
                column.Clickable = true;
                column.Resizable = true;
                column.Title = columnLabels[i];
                column.SortIndicator = true;
                column.Alignment = 0.5f;
                column.Expand = true;
                column.SortColumnId = i;
                column.PackStart(render, true);
                column.AddAttribute(render, "text", i);
                
                if (i == 2)
                    column.SetCellDataFunc(render, WorkingSetFormatter);

                tree.AppendColumn(column);
            }
            
            scrolledWindow.Add(tree);
            //processesVBox.PackStart(tree, false, false, 0);
            
            frame.Add(scrolledWindow);

            vbox.PackStart(frame, false, false, 0);

            Button killButton = new Button("Kill process");
            killButton.Clicked += KillProcess;
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
            tree.Selection.Changed += OnSelectionChanged;

            tree.ShowAll();
            window.ShowAll();
        }

        private void WorkingSetFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            try
            {
                string data = model.GetValue(iter, 2).ToString();
                double dataDouble = Convert.ToDouble(data);
                if (data != "")
                {
                    ((CellRendererText) cell).Text = ProcessMod.FormatMemSize(dataDouble);
                }
            }
            catch (Exception e)
            {
                if (e is NullReferenceException || e is FormatException)
                {
                    ((CellRendererText) cell).Text = "";
                }
            }
        }

        private int ProcessNameSortFunc(ITreeModel model, TreeIter a, TreeIter b)
        {
            try
            {
                string s1 = (string) model.GetValue(a, 0);
                string s2 = (string) model.GetValue(b, 0);
                return String.Compare(s1, s2);
            }
            catch (NullReferenceException e)
            {
                return 0;
            }
        }
        
        private int IdSortFunc(ITreeModel model, TreeIter a, TreeIter b)
        {
            try
            {
                string val1 = model.GetValue(a, 1).ToString();
                string val2 = model.GetValue(b, 1).ToString();

                if (val1 == "" || val2 == "")
                    return 0;

                int s1 = Convert.ToInt32(val1);
                int s2 = Convert.ToInt32(val2);
                return s1.CompareTo(s2);
            }
            catch (NullReferenceException e)
            {
                return 0;
            }
        }
        
        private int WorkingSetSortFunc(ITreeModel model, TreeIter a, TreeIter b)
        {
            try
            {
                string val1 = model.GetValue(a, 2).ToString().Split(" ")[0];
                string val2 = model.GetValue(b, 2).ToString().Split(" ")[0];
                
                if (val1 == "" || val2 == "")
                    return 0;

                double s1 = Convert.ToDouble(val1);
                double s2 = Convert.ToDouble(val2);
                return s1.CompareTo(s2);
            }
            catch (NullReferenceException e)
            {
                return 0;
            }
        }
        
        private int CpuUsageSortFunc(ITreeModel model, TreeIter a, TreeIter b)
        {
            try
            {
                string val1 = model.GetValue(a, 3).ToString().Split(" ")[0];
                string val2 = model.GetValue(b, 3).ToString().Split(" ")[0];

                if (val1 == "" || val2 == "")
                    return 0;
                
                double s1 = Convert.ToDouble(val1);
                double s2 = Convert.ToDouble(val2);
                return s1.CompareTo(s2);
            }
            catch (NullReferenceException e)
            {
                return 0;
            }
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

        private void KillProcess(object o, EventArgs args)
        {
            foreach (var id in _processIdToKill)
            {
                Process process = Process.GetProcessById(id);
                Console.WriteLine($"{id} killed");
                process.Kill();
            }
            
            _updater.Run();
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
                        element[i].WorkingSet64.ToString(),
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
        
        void OnSelectionChanged(object o, EventArgs args)
        {
            TreeSelection selection = (TreeSelection)o;
            TreePath[] selectedRows = selection.GetSelectedRows();

            _processIdToKill.Clear();
            TreeIter iter;
            if (selectedRows.Length > 0)
            {
                for (int i = 0; i < selectedRows.Length; i++)
                {
                    store.GetIter(out iter, selectedRows[i]);
                    Console.WriteLine(store.GetValue(iter, 1));

                    int id;
                    int.TryParse(store.GetValue(iter, 1).ToString(), out id);
                    if (id != 0)
                    {
                        _processIdToKill.Add(id);
                    }
                }
                _updater.Stop();
            }
            else
            {
                Console.WriteLine("Node is null.");
            }

            foreach (var element in _processIdToKill)
            {
                Console.WriteLine($"{element} added.");
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