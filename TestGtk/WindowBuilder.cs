using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Cairo;
using Gdk;
using Gtk;
using GLib;
using Application = Gtk.Application;
using Process = System.Diagnostics.Process;
using Window = Gtk.Window;


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
        private string[] _filtrationOptions;
        private HBox _filtrationHBox;
        private Entry _entry;
        private Entry _numericalEntry;

        private HBox _memoryFiltrationHbox;
        private Entry _memoryFiltrationEntry;
        private ComboBox _memoryFiltrationDirectionComboBox;
        private ComboBox _memoryFiltrationUnitsComboBox;
        private string[] _FiltrationDirectionOptions;
        private string[] _memoryFiltrationDirectionUnits;
        
        private HBox _cpuFiltrationHbox;
        private Entry _cpuFiltrationEntry;
        private ComboBox _cpuFiltrationDirectionComboBox;
        private Label _cpuFiltrationLabel;
        
        
        //private SpinButton _numericalEntry;
        private StringBuilder _searchPattern;
        private TreeModelFilter _filter;
        private string _textToFilter;

        private int _columnFilter;
        //private bool FilterById(ITreeModel model, TreeIter iter)

        public WindowBuilder()
        {
            _columnFilter = 0;
            _textToFilter = "";
            _searchPattern = new StringBuilder();
            _updater = new ProcessGrabber();
            _processIdToKill = new List<int>();
            store = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), 
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
            Window window;
            

            Application.Init ();
            
            window = new Window ("Label sample");
            
            AboutDialog aboutDialog;
            aboutDialog = new AboutDialog();
            aboutDialog.Title = "About Process Watch";
            aboutDialog.Authors = new[]
            {
                "Wojciech Wesołowski",
                "Marek Krzysztofiak"
            };
            aboutDialog.Copyright = "Copyright \xa9 2012 Codecool";
            aboutDialog.Version = "v.1.0";
            aboutDialog.ProgramName = "Process Watch";
            aboutDialog.Response += (o, args) =>
            {
                switch (args.ResponseId)
                {
                    case ResponseType.DeleteEvent:
                        aboutDialog.Hide();
                        break;
                    case ResponseType.Cancel:
                        aboutDialog.Hide();
                        break;
                }
            };
            aboutDialog.Comments = "Process Watch is a simple and intuitive process manager\n" +
                                   "that allows to inspect all running processes and eventually kill them.";
            aboutDialog.Logo = new Pixbuf("processIconSmall.png");
            aboutDialog.TransientFor = window;


            window.Resize(500, 600);
            window.Title = "Process Watch";
            window.SetIconFromFile("processIconSmall.png");
            window.BorderWidth = 5;
            
            window.DeleteEvent += delete_event;
            
            HBox hbox = new HBox (false, 5);
            
            VBox vbox = new VBox (false, 5);
            
            Button button = new Button("Start");
            button.Clicked += Start;
            
            Button button2 = new Button("Stop");
            button2.Clicked += Stop;

            Button aboutButton = new Button();
            Image aboutIcon = new Image();
            aboutIcon.Pixbuf = new Pixbuf("information.png");
            
            //aboutButton.Image = new Image(Stock.Info, IconSize.Button);
            aboutButton.Image = aboutIcon;
            aboutButton.Clicked += (sender, args) =>
            {
                aboutDialog.Show();
            }; 
            
            
            hbox.PackStart(button, false, false, 0);
            hbox.PackStart(button2, false, false, 0);
            hbox.PackEnd(aboutButton, false, false, 0);

            _filtrationHBox = new HBox(false, 5);
            _entry = new Entry();
            _entry.Changed += OnChanged;
            _numericalEntry = new Entry();
            //_numericalEntry = new SpinButton(1, 9999999, 1);
            _numericalEntry.Changed += OnChanged;
            _numericalEntry.TextInserted += OnlyNumerical;

            _filtrationOptions = new[]
            {
                "All processes",
                "Filter by PID",
                "Filter by Process Name",
                "Filter by Memory Usage",
                "Filter by CPU usage",
            };

            _FiltrationDirectionOptions = new[]
            {
                ">",
                "≥",
                "=",
                "≤",
                "<"
            };

            _memoryFiltrationDirectionUnits = new[]
            {
                "B",
                "KB",
                "MB",
                "GB"
            };

            _memoryFiltrationHbox = new HBox();
            _memoryFiltrationEntry = new Entry();
            _memoryFiltrationEntry.MaxWidthChars = 7;
            _memoryFiltrationEntry.WidthChars = 7;
            _memoryFiltrationEntry.Changed += OnChanged;
            _memoryFiltrationEntry.TextInserted += OnlyNumerical;
            _memoryFiltrationDirectionComboBox = new ComboBox(_FiltrationDirectionOptions);
            _memoryFiltrationDirectionComboBox.Changed += OnChanged;
            _memoryFiltrationUnitsComboBox = new ComboBox(_memoryFiltrationDirectionUnits);
            _memoryFiltrationUnitsComboBox.Changed += OnChanged;
            _memoryFiltrationHbox.PackStart(_memoryFiltrationDirectionComboBox, false, false, 0);
            _memoryFiltrationHbox.PackStart(_memoryFiltrationEntry, false, false, 0);
            _memoryFiltrationHbox.PackStart(_memoryFiltrationUnitsComboBox, false, false, 0);

            _cpuFiltrationHbox = new HBox();
            _cpuFiltrationEntry = new Entry();
            _cpuFiltrationEntry.MaxWidthChars = 7;
            _cpuFiltrationEntry.WidthChars = 7;
            _cpuFiltrationEntry.Changed += OnChanged;
            _cpuFiltrationEntry.TextInserted += OnlyNumerical;
            _cpuFiltrationDirectionComboBox = new ComboBox(_FiltrationDirectionOptions);
            _cpuFiltrationDirectionComboBox.Changed += OnChanged;
            _cpuFiltrationLabel = new Label("%"); 
            _cpuFiltrationHbox.PackStart(_cpuFiltrationDirectionComboBox, false, false, 0);
            _cpuFiltrationHbox.PackStart(_cpuFiltrationEntry, false, false, 0);
            _cpuFiltrationHbox.PackStart(_cpuFiltrationLabel, false, false, 0);
            
            ComboBox filtrationCombo = new ComboBox(_filtrationOptions);
            filtrationCombo.Changed += ComboOnChanged;
            _filtrationHBox.PackStart(filtrationCombo, false, false, 0);

            window.Add (vbox);
            vbox.PackStart (hbox, false, false, 0);
            vbox.PackStart(_filtrationHBox, false, false, 0);
            
            Frame frame = new Frame ("Processes");

            scrolledWindow = new ScrolledWindow();
            scrolledWindow.HeightRequest = 400;
            
            VBox processesVBox = new VBox(false, 5);
            
            string[] columnLabels = {
                "PID",
                "Process name",
                "Memory usage",
                "Priority",
                "User CPU Time",
                "Privileged CPU Time",
                "Total CPU Time",
                "CPU usage",
                "Threads",
                "Start Time"
            };
            
            CellRendererText render = new CellRendererText();
            render.Alignment = Pango.Alignment.Right;
            render.Xalign = 0.5f;

            //_filter = new TreeModelFilter(sortable, null);
            _filter = new TreeModelFilter(store, null);
            _filter.VisibleFunc = FilterByName;

            TreeModelSort sortable = new TreeModelSort(_filter);
            sortable.SetSortFunc(0, IdSortFunc);
            sortable.SetSortFunc(1, ProcessNameSortFunc);
            sortable.SetSortFunc(2, WorkingSetSortFunc);
            sortable.SetSortFunc(3, PrioritySortFunc);
            sortable.SetSortFunc(4, UserCpuTimeSortFunc);
            sortable.SetSortFunc(5, PrivilegedCpuTimeSortFunc);
            sortable.SetSortFunc(6, TotalCpuTimeSortFunc);
            sortable.SetSortFunc(7, CpuUsageSortFunc);
            sortable.SetSortFunc(8, ThreadCountFunc);
            sortable.SetSortFunc(9, StartTimeSortFunc);
            
            tree = new TreeView();
            tree.Model = sortable;
            //tree.Model = _filter;

            _updater.ColumnToSort[1] = null;
            for (int i = 0; i < 10; i++)
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

                var i1 = i;
                column.Clicked += (sender, args) =>
                {
                    Console.WriteLine(column.SortOrder);
                    _updater.ColumnToSort[0] = i1;
                    _updater.ColumnToSort[1] = SortOrderToInt();
                }; 

                switch (i)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        column.SetCellDataFunc(render, WorkingSetFormatter);
                        break;
                    case 3:
                        break;
                    case 4:
                        column.SetCellDataFunc(render, UserCpuTimeFormatter);
                        break;
                    case 5:
                        column.SetCellDataFunc(render, PrivilegedCpuTimeFormatter);
                        break;
                    case 6:
                        column.SetCellDataFunc(render, TotalCpuTimeFormatter);
                        break;
                    case 7:
                        column.SetCellDataFunc(render, CpuUsageFormatter);
                        break;
                    case 8:
                        break;
                    case 9:
                        column.SetCellDataFunc(render, StartTimeFormatter);
                        break;
                }

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
                store.AppendValues("", "", "", "", "", "", "", "", "", "");
            }
            
            //tree.NodeSelection.Changed += OnSelectionChanged;
            //tree.NodeStore = store;
            tree.Selection.Mode = SelectionMode.Multiple;
            tree.Selection.Changed += OnSelectionChanged;

            tree.ShowAll();
            window.ShowAll();
        }

        private void ComboOnChanged(object sender, EventArgs args)
        {
            ComboBox combo = (ComboBox) sender;

            switch (_filtrationOptions[combo.Active])
            {
                case "All processes":
                    FilterShowAll();
                    break;
                case "Filter by PID":
                    _columnFilter = 0;
                    //FilterByIdShowEntry();
                    ShowFilterWidgets(_numericalEntry);
                    break;
                case "Filter by Process Name":
                    _columnFilter = 1;
                    //FilterByNameShowEntry();
                    ShowFilterWidgets(_entry);
                    break;
                case "Filter by Memory Usage":
                    _columnFilter = 2;
                    ShowFilterWidgets(_memoryFiltrationHbox);
                    break;
                case "Filter by CPU usage":
                    _columnFilter = 7;
                    ShowFilterWidgets(_cpuFiltrationHbox);
                    break;
            }
        }

        private void ShowFilterWidgets(Widget widget)
        {
            HideAllEntryWidgets();
            _filtrationHBox.PackStart(widget, false, false, 0);
            _filtrationHBox.ShowAll();
        }

        private void FilterByNameShowEntry()
        {
            HideAllEntryWidgets();
            _filtrationHBox.PackStart(_entry, false, false, 0);
            _filtrationHBox.ShowAll();
        }
        
        private void FilterByIdShowEntry()
        {
            HideAllEntryWidgets();
            //_numericalEntry.KeyPressEvent += OnlyNumerical;
            
            _filtrationHBox.PackStart(_numericalEntry, false, false, 0);
            _filtrationHBox.ShowAll();
        }

        //[ConnectBefore]
        private void OnlyNumerical(object sender, TextInsertedArgs args)
        {
            Entry entry = (Entry) sender;
            Console.WriteLine($"input: {args.Position} real: {entry.Text} newText: {args.NewText}");
            
            if (args.NewText.Length == 1)
            {
                char inputKey = Convert.ToChar(args.NewText);
                int inputKeyPosition = Convert.ToInt32(args.Position);

                if (!Char.IsNumber(inputKey) && inputKey != '.')
                {
                    entry.Text = entry.Text.Remove(inputKeyPosition - 1);
                }
            }
        }

        private void FilterShowAll()
        {
            HideAllEntryWidgets();
        }

        private void HideAllEntryWidgets()
        {
            _entry.Text = "";
            _numericalEntry.Text = "";
            _memoryFiltrationEntry.Text = "";
            _cpuFiltrationEntry.Text = "";
            
            _filtrationHBox.Foreach(widget =>
            {
                if (widget.Name != "GtkComboBox")
                {
                    _filtrationHBox.Remove(widget);
                }
            });
        }

        //[ConnectBefore]
        private void OnChanged(object sender, EventArgs args)
        {
            switch (_columnFilter)
            {
                case 0:
                    _textToFilter = _numericalEntry.Text;
                    break;
                case 1: 
                    _textToFilter = _entry.Text;
                    break;
                case 2:
                    if (_memoryFiltrationUnitsComboBox.Active > -1 && _memoryFiltrationDirectionComboBox.Active > -1 && 
                        _memoryFiltrationEntry.Text != "" && Char.IsNumber(_memoryFiltrationEntry.Text.Last()))
                        _textToFilter =
                            $"{_memoryFiltrationEntry.Text} {_memoryFiltrationDirectionUnits[_memoryFiltrationUnitsComboBox.Active]}";
                    else
                        _textToFilter = "";
                    break;
                case 7:
                    if (_cpuFiltrationDirectionComboBox.Active > -1 && _cpuFiltrationEntry.Text != "" && Char.IsNumber(_cpuFiltrationEntry.Text.Last()))
                        _textToFilter =
                            $"{_cpuFiltrationEntry.Text} %";
                    else
                        _textToFilter = "";
                    break;
            }
            
            _filter.Refilter();
        }

        private double FormatMemSize(string size)
        {
            double d = Convert.ToDouble(size);
            
            int i = 0;
            while ((d > 1024) && (i < 5))
            {
                d /= 1024;
                i++;
            }

            return Math.Round(d, 2);
        }
        
        private double MemSizeToRaw(string size, string unit)
        {
            string[] units = { "TB", "GB", "MB", "KB", "B" };

            double sizeDouble = Convert.ToDouble(size);
            
            int i = Array.IndexOf(units, unit);
            while (i < units.Length - 1)
            {
                sizeDouble *= 1024;
                i++;
            }

            return sizeDouble;
        }

        private bool FilterByName(ITreeModel model, TreeIter iter)
        {
            try
            {
                string processName = model.GetValue(iter, _columnFilter).ToString();
                
                if (_textToFilter == "" || processName == "")
                    return true;

                if (_textToFilter.EndsWith('%'))
                {
                    string[] _textToFilterSplitted = _textToFilter.Split(" ");
                    double userInput = Convert.ToDouble(_textToFilterSplitted[0]);
                    double cpuUsage = Convert.ToDouble(processName);
                    
                    switch (_FiltrationDirectionOptions[_cpuFiltrationDirectionComboBox.Active])
                    {
                        case ">":
                            return cpuUsage > userInput;
                        case "≥":
                            return cpuUsage >= userInput;
                        case "=":
                            return cpuUsage == userInput;
                        case "≤":
                            return cpuUsage <= userInput;
                        case "<":
                            return cpuUsage < userInput;
                        default:
                            return true;
                    }
                }
                
                if (_textToFilter.EndsWith('B'))
                {
                    string[] _textToFilterSplitted = _textToFilter.Split(" ");
                    string memSize = _textToFilterSplitted[0];
                    string memUnit = _textToFilterSplitted[1];
                    Console.WriteLine($"memSize: {memSize}");

                    double memoryUsage = Convert.ToDouble(processName);
                

                    switch (_FiltrationDirectionOptions[_memoryFiltrationDirectionComboBox.Active])
                    {
                        case ">":
                            return memoryUsage > MemSizeToRaw(memSize, memUnit);
                        case "≥":
                            return memoryUsage >= MemSizeToRaw(memSize, memUnit);
                        case "=":
                            return memoryUsage == MemSizeToRaw(memSize, memUnit);
                        case "≤":
                            return memoryUsage <= MemSizeToRaw(memSize, memUnit);
                        case "<":
                            return memoryUsage < MemSizeToRaw(memSize, memUnit);
                        default:
                            return true;
                    }
                }

                if (processName.IndexOf(_textToFilter) > -1)
                    return true;
                
                return false;
            }
            catch (NullReferenceException e)
            {
                return false;
            }
        }
        
        private bool FilterById(ITreeModel model, TreeIter iter)
        {
            try
            {
                string processName = model.GetValue(iter, 0).ToString();

                if (_entry.Text == "")
                    return true;

                if (processName.IndexOf(_entry.Text) > -1)
                    return true;
                
                return false;
            }
            catch (NullReferenceException e)
            {
                return false;
            }
        }

        private int? SortOrderToInt(SortType? sortType=null)
        {
            if (sortType == SortType.Ascending)
                return 0;
            
            if (sortType == SortType.Descending && _updater.ColumnToSort[1] == 0)
                return 1;

            if (_updater.ColumnToSort[1] == null)
            {
                return 0;
            }

            if (_updater.ColumnToSort[1] == 0)
            {
                return 1;
            }
            
            return null;
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
        
        private void StartTimeFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            try
            {
                string data = model.GetValue(iter, 9).ToString();
                long dataLong = Convert.ToInt64(data);
                if (data != "")
                {
                    ((CellRendererText) cell).Text = ProcessMod.FormatTime(dataLong);
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
        
        private void UserCpuTimeFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            try
            {
                string data = model.GetValue(iter, 4).ToString();
                long dataLong = Convert.ToInt64(data);
                if (data != "")
                {
                    ((CellRendererText) cell).Text = ProcessMod.FormatTimeMs(dataLong);
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
        
        private void PrivilegedCpuTimeFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            try
            {
                string data = model.GetValue(iter, 5).ToString();
                long dataLong = Convert.ToInt64(data);
                if (data != "")
                {
                    ((CellRendererText) cell).Text = ProcessMod.FormatTimeMs(dataLong);
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
        
        private void TotalCpuTimeFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            try
            {
                string data = model.GetValue(iter, 6).ToString();
                long dataLong = Convert.ToInt64(data);
                if (data != "")
                {
                    ((CellRendererText) cell).Text = ProcessMod.FormatTimeMs(dataLong);
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
        
        private void CpuUsageFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            try
            {
                string data = model.GetValue(iter, 7).ToString();
                double dataDouble = Convert.ToDouble(data);
                if (data != "")
                {
                    ((CellRendererText) cell).Text = ProcessMod.FormatCpuUsage(dataDouble);
                    //((CellRendererText) cell).Text = dataDouble.ToString();
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
                string val1 = (string) model.GetValue(a, 1);
                string val2 = (string) model.GetValue(b, 1);
                
                if (val1 == "" || val2 == "")
                    return 1;
                
                return String.Compare(val1, val2);
            }
            catch (NullReferenceException e)
            {
                return 0;
            }
        }
        
        private int PrioritySortFunc(ITreeModel model, TreeIter a, TreeIter b)
        {
            try
            {
                string val1 = (string) model.GetValue(a, 3);
                string val2 = (string) model.GetValue(b, 3);
                
                if (val1 == "" || val2 == "")
                    return 1;
                
                return String.Compare(val1, val2);
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
                string val1 = model.GetValue(a, 7).ToString();
                string val2 = model.GetValue(b, 7).ToString();

                if (val1 == "" || val2 == "")
                    return 1;

                double s1 = Convert.ToDouble(val1);
                double s2 = Convert.ToDouble(val2);
                return s1.CompareTo(s2);
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
                string val1 = model.GetValue(a, 0).ToString();
                string val2 = model.GetValue(b, 0).ToString();

                if (val1 == "" || val2 == "")
                    return 1;

                int s1 = Convert.ToInt32(val1);
                int s2 = Convert.ToInt32(val2);
                return s1.CompareTo(s2);
            }
            catch (NullReferenceException e)
            {
                return 0;
            }
        }
        
        private int StartTimeSortFunc(ITreeModel model, TreeIter a, TreeIter b)
        {
            try
            {
                string val1 = model.GetValue(a, 9).ToString();
                string val2 = model.GetValue(b, 9).ToString();

                if (val1 == "" || val2 == "")
                    return 1;

                long s1 = Convert.ToInt64(val1);
                long s2 = Convert.ToInt64(val2);
                return s1.CompareTo(s2);
            }
            catch (NullReferenceException e)
            {
                return 0;
            }
        }
        
        private int ThreadCountFunc(ITreeModel model, TreeIter a, TreeIter b)
        {
            try
            {
                string val1 = model.GetValue(a, 8).ToString();
                string val2 = model.GetValue(b, 8).ToString();

                if (val1 == "" || val2 == "")
                    return 1;

                int s1 = Convert.ToInt32(val1);
                int s2 = Convert.ToInt32(val2);
                return s1.CompareTo(s2);
            }
            catch (NullReferenceException e)
            {
                return 0;
            }
        }
        
        private int UserCpuTimeSortFunc(ITreeModel model, TreeIter a, TreeIter b)
        {
            try
            {
                string val1 = model.GetValue(a, 4).ToString();
                string val2 = model.GetValue(b, 4).ToString();

                if (val1 == "" || val2 == "")
                    return 1;

                int s1 = Convert.ToInt32(val1);
                int s2 = Convert.ToInt32(val2);
                return s1.CompareTo(s2);
            }
            catch (NullReferenceException e)
            {
                return 0;
            }
        }
        
        private int PrivilegedCpuTimeSortFunc(ITreeModel model, TreeIter a, TreeIter b)
        {
            try
            {
                string val1 = model.GetValue(a, 5).ToString();
                string val2 = model.GetValue(b, 5).ToString();

                if (val1 == "" || val2 == "")
                    return 1;

                int s1 = Convert.ToInt32(val1);
                int s2 = Convert.ToInt32(val2);
                return s1.CompareTo(s2);
            }
            catch (NullReferenceException e)
            {
                return 0;
            }
        }
        
        private int TotalCpuTimeSortFunc(ITreeModel model, TreeIter a, TreeIter b)
        {
            /*
            if (_updater.ColumnToSort[0] == 6)
            {
                Console.WriteLine("wait");
                return 0;
            }
            */
            try
            {
                string val1 = model.GetValue(a, 6).ToString();
                string val2 = model.GetValue(b, 6).ToString();

                if (val1 == "" || val2 == "")
                    return 1;

                int s1 = Convert.ToInt32(val1);
                int s2 = Convert.ToInt32(val2);
                
                //Console.WriteLine($"{val1} {val2} {s1.CompareTo(s2).ToString()}");
                
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
                    return 1;

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
                store.SetValues(iter, "", "", "", "", "", "", "", "", "", "");

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
                    store.SetValues(iter,
                        element[i].Id.ToString(),
                        element[i].ProcessName,
                        element[i].WorkingSet64.ToString(),
                        element[i].PriorityClass,
                        element[i].UserProcessorTime.ToString(),
                        element[i].PrivilegedProcessorTime.ToString(),
                        element[i].TotalProcessorTime.ToString(),
                        element[i].CpuUsage.ToString(),
                        element[i].ThreadCount.ToString(),
                        element[i].StartTime.ToString()
                    );
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
                    store.AppendValues(element[i].Id.ToString(),
                        element[i].ProcessName,
                        element[i].WorkingSet64.ToString(),
                        element[i].PriorityClass,
                        element[i].UserProcessorTime.ToString(),
                        element[i].PrivilegedProcessorTime.ToString(),
                        element[i].TotalProcessorTime.ToString(),
                        element[i].CpuUsage.ToString(),
                        element[i].ThreadCount.ToString(),
                        element[i].StartTime.ToString());
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