using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gdk;
using Gtk;
using Application = Gtk.Application;
using Process = System.Diagnostics.Process;
using Window = Gtk.Window;


namespace TestGtk
{
    public class WindowBuilder
    {
        private ComboBox _cpuFiltrationDirectionComboBox;
        private ComboBox _memoryFiltrationDirectionComboBox;
        private ComboBox _memoryFiltrationUnitsComboBox;
        private double _currentScrollPosition;
        private Entry _cpuFiltrationEntry;
        private Entry _entry;
        private Entry _memoryFiltrationEntry;
        private Entry _numericalEntry;
        private HBox _cpuFiltrationHbox;
        private HBox _filtrationHBox;
        private HBox _memoryFiltrationHbox;
        private Label _cpuFiltrationLabel;
        private List<int> _processIdToKill;
        private ScrolledWindow scrolledWindow;
        private static ListStore store;
        private static ProcessGrabber _updater;
        private string[] _FiltrationDirectionOptions;
        private string[] _filtrationOptions;
        private string[] _memoryFiltrationDirectionUnits;
        private TreeView tree;
        
        private TreeModelFilter _filter;
        private string _textToFilter;
        
        private Window _window;

        private int _columnFilter;

        public WindowBuilder()
        {
            _columnFilter = 0;
            _textToFilter = "";
            _updater = new ProcessGrabber();
            _processIdToKill = new List<int>();
            store = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), 
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));

            Application.Init();
            
            _window = new Window ("Label sample");

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
            aboutDialog.Logo = new Pixbuf("icons/processIconSmall.png");
            aboutDialog.TransientFor = _window;


            _window.Resize(1300, 600);
            _window.Title = "Process Watch";
            _window.SetIconFromFile("icons/processIconSmall.png");
            _window.BorderWidth = 5;
            
            _window.DeleteEvent += delete_event;
            
            HBox hbox = new HBox (false, 5);
            
            VBox vbox = new VBox (false, 5);

            Button aboutButton = new Button();
            Image aboutIcon = new Image();
            aboutIcon.Pixbuf = new Pixbuf("icons/information.png");
            
            aboutButton.Image = aboutIcon;
            aboutButton.TooltipText = "About Process Watch";
            aboutButton.Clicked += (sender, args) =>
            {
                aboutDialog.Show();
            };

            Button filterButton = new Button();
            filterButton.Image = new Image(Stock.Find, IconSize.Button);
            filterButton.TooltipText = "Filtration utilities";
            
            hbox.PackEnd(aboutButton, false, false, 0);
            hbox.PackEnd(filterButton, false, false, 0);

            _filtrationHBox = new HBox(false, 5);
            
            filterButton.Clicked += (sender, args) =>
            {
                if (_filtrationHBox.IsVisible)
                    _filtrationHBox.Hide();
                else
                    _filtrationHBox.ShowAll();
            };
            
            _entry = new Entry();
            _entry.Changed += OnChanged;
            _numericalEntry = new Entry();
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

            _window.Add (vbox);
            vbox.PackStart (hbox, false, false, 0);
            vbox.PackStart(_filtrationHBox, false, false, 0);
            
            Frame frame = new Frame ("Processes");

            scrolledWindow = new ScrolledWindow();

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
                    //Console.WriteLine(column.SortOrder);
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

            vbox.PackStart(scrolledWindow, true, true, 0);

            Button killButton = new Button("Kill process");
            killButton.Clicked += KillProcess;
            vbox.PackStart(killButton, false, false, 0);
            
            // Create an instance of the object Updater
            _updater.OnResult += (sender, list) =>
            {
                Application.Invoke(delegate
                {
                    _currentScrollPosition = tree.Vadjustment.Value;
                    StoreClear();
                    LoadStore(list);

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
            
            tree.Selection.Mode = SelectionMode.Multiple;
            tree.Selection.Changed += OnSelectionChanged;
            
            

            tree.ShowAll();
            _window.ShowAll();
            _filtrationHBox.Hide();
            _updater.Run();
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
                    ShowFilterWidgets(_numericalEntry);
                    break;
                case "Filter by Process Name":
                    _columnFilter = 1;
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

            _filtrationHBox.PackStart(_numericalEntry, false, false, 0);
            _filtrationHBox.ShowAll();
        }

        //[ConnectBefore]
        private void OnlyNumerical(object sender, TextInsertedArgs args)
        {
            Entry entry = (Entry) sender;
            //Console.WriteLine($"input: {args.Position} real: {entry.Text} newText: {args.NewText}");
            
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

                if (processName.IndexOf(_textToFilter, StringComparison.CurrentCultureIgnoreCase) > -1)
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
            try
            {
                string val1 = model.GetValue(a, 6).ToString();
                string val2 = model.GetValue(b, 6).ToString();

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
            using (KillDialog killDialog = new KillDialog(_window, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, null))
            {
                int processesToKillCount = _processIdToKill.Count;

                if (processesToKillCount == 0)
                    return;
                if (processesToKillCount == 1)
                {
                    int processId = _processIdToKill[0];
                    Process process = Process.GetProcessById(processId);
                    killDialog.Text =
                        $"Are you sure you want to end the selected process \"{process.ProcessName}\" (PID: {processId.ToString()})?";
                    process.Dispose();
                }
                else
                {
                    killDialog.Text =
                        $"Are you sure you want to end the {processesToKillCount.ToString()} selected processes?";
                }

                killDialog.Response += (o1, responseArgs) =>
                {
                    switch (responseArgs.ResponseId)
                    {
                        case ResponseType.Yes:
                            foreach (var id in _processIdToKill)
                            {
                                Process process = Process.GetProcessById(id);
                                Console.WriteLine($"{id.ToString()} killed");
                                process.Kill();
                                process.Dispose();
                            }

                            break;
                        case ResponseType.No:
                            Console.WriteLine("Abort killing.");
                            break;
                    }
                };
                
                killDialog.Run();
            }
        }

        private void LoadStore(List<ProcessMod> element)
        {
            int elementIndex = 0;
            TreeIter iter;
            store.GetIterFirst(out iter);
            for (int i = 0; i < store.IterNChildren(); i++)
            {
                if (element.Count - 0 > i)
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
            ITreeModel filtered;
            TreePath[] selectedRows = selection.GetSelectedRows(out filtered);
            

            _processIdToKill.Clear();
            TreeIter iter;
            if (selectedRows.Length > 0)
            {
                for (int i = 0; i < selectedRows.Length; i++)
                {
                    filtered.GetIter(out iter, selectedRows[i]);

                    int id;
                    int.TryParse(filtered.GetValue(iter, 0).ToString(), out id);
                    if (id != 0)
                    {
                        _processIdToKill.Add(id);
                    }
                }
            }
            else
            {
                Console.WriteLine("Node is null.");
            }
        }

        public void Run()
        {
            Application.Run();
        }
    }

    class KillDialog : MessageDialog
    {
        private string _secondaryText = "Killing a process may destroy data, break the session or introduce a security risk. Only unresponsive processes should be killed.";
        public KillDialog(Window window, DialogFlags flag, MessageType messageType, ButtonsType buttonType, string format) : base(window, flag, messageType, buttonType, format)
        {
            SecondaryText = _secondaryText;
        }
    }
}