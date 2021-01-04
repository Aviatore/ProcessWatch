using System;
using System.Collections.Generic;
using System.Linq;
using Gdk;
using Gtk;
using TestGtk.Controller;
using TestGtk.Model;
using Application = Gtk.Application;
using Process = System.Diagnostics.Process;
using Window = Gtk.Window;


namespace TestGtk.View
{
    public class WindowBuilder
    {
        private ComboBox _cpuFiltrationDirectionComboBox;
        private ComboBox _memoryFiltrationDirectionComboBox;
        private ComboBox _memoryFiltrationUnitsComboBox;
        private double _currentScrollPosition;
        private Entry _cpuFiltrationEntry;
        private Entry _processNameEntry;
        private Entry _memoryFiltrationEntry;
        private Entry _processIdEntry;
        private HBox _cpuFiltrationHbox;
        private HBox _filtrationHBox;
        private HBox _memoryFiltrationHbox;
        private HBox _windowHBox;
        private Label _cpuFiltrationLabel;
        private List<int> _processIdToKill;
        private ScrolledWindow _scrolledWindow;
        private static ListStore _store;
        private static ProcessGrabber _processGrabber;
        private string[] _FiltrationDirectionOptions;
        private string[] _filtrationOptions;
        private string[] _memoryFiltrationDirectionUnits;
        private TreeView _treeView;
        private VBox _windowVBox;
        private Button _aboutButton;
        private Image _aboutIcon;
        private Button _filterButton;
        private TreeModelFilter _treeModelFilter;
        private string _textToFilter;
        private AboutDialog _aboutDialog;
        private Window _window;
        private ComboBox _filtrationCombo;
        private CellRendererText _cellRendererText;
        private Button _killButton;
        private TreeModelSort _treeModelSort;
        private TreeViewColumn _treeViewColumn;

        private int _columnFilter;

        public WindowBuilder()
        {
            _columnFilter = 0;
            _textToFilter = "";
            
            _processIdToKill = new List<int>();
            _store = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), 
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));

            Application.Init();

            _window = new Window ("Label sample");
            _window.Resize(1300, 600);
            _window.Title = "Process Watch";
            _window.SetIconFromFile("icons/processIconSmall.png");
            _window.BorderWidth = 5;
            _window.DeleteEvent += OnWindowClose;

            
            

            _aboutButton = new Button();
            _aboutIcon = new Image();
            _aboutIcon.Pixbuf = new Pixbuf("icons/information.png");
            _aboutButton.Image = _aboutIcon;
            _aboutButton.TooltipText = "About Process Watch";
            _aboutButton.Clicked += (sender, args) =>
            {
                _aboutDialog.Show();
            };
            
            _aboutDialog = CreateAboutDialog();

            _filterButton = new Button();
            _filterButton.Image = new Image(Stock.Find, IconSize.Button);
            _filterButton.TooltipText = "Filtration utilities";
            _filterButton.Clicked += (sender, args) =>
            {
                if (_filtrationHBox.IsVisible)
                    _filtrationHBox.Hide();
                else
                    _filtrationHBox.ShowAll();
            };
            
            _windowHBox = new HBox (false, 5);
            _windowHBox.PackEnd(_aboutButton, false, false, 0);
            _windowHBox.PackEnd(_filterButton, false, false, 0);

            _processNameEntry = new Entry();
            _processNameEntry.Changed += OnChanged;
            
            _processIdEntry = new Entry();
            _processIdEntry.Changed += OnChanged;
            _processIdEntry.TextInserted += OnlyNumerical;

            
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
            
            
            _memoryFiltrationEntry = new Entry();
            _memoryFiltrationEntry.MaxWidthChars = 7;
            _memoryFiltrationEntry.WidthChars = 7;
            _memoryFiltrationEntry.Changed += OnChanged;
            _memoryFiltrationEntry.TextInserted += OnlyNumerical;
            
            _memoryFiltrationDirectionComboBox = new ComboBox(_FiltrationDirectionOptions);
            _memoryFiltrationDirectionComboBox.Changed += OnChanged;
            
            _memoryFiltrationUnitsComboBox = new ComboBox(_memoryFiltrationDirectionUnits);
            _memoryFiltrationUnitsComboBox.Changed += OnChanged;
            
            _memoryFiltrationHbox = new HBox();
            _memoryFiltrationHbox.PackStart(_memoryFiltrationDirectionComboBox, false, false, 0);
            _memoryFiltrationHbox.PackStart(_memoryFiltrationEntry, false, false, 0);
            _memoryFiltrationHbox.PackStart(_memoryFiltrationUnitsComboBox, false, false, 0);

            
            _cpuFiltrationEntry = new Entry();
            _cpuFiltrationEntry.MaxWidthChars = 7;
            _cpuFiltrationEntry.WidthChars = 7;
            _cpuFiltrationEntry.Changed += OnChanged;
            _cpuFiltrationEntry.TextInserted += OnlyNumerical;
            
            _cpuFiltrationDirectionComboBox = new ComboBox(_FiltrationDirectionOptions);
            _cpuFiltrationDirectionComboBox.Changed += OnChanged;
            
            _cpuFiltrationLabel = new Label("%"); 
            
            _cpuFiltrationHbox = new HBox();
            _cpuFiltrationHbox.PackStart(_cpuFiltrationDirectionComboBox, false, false, 0);
            _cpuFiltrationHbox.PackStart(_cpuFiltrationEntry, false, false, 0);
            _cpuFiltrationHbox.PackStart(_cpuFiltrationLabel, false, false, 0);
            
            
            _filtrationOptions = new[]
            {
                "All processes",
                "Filter by PID",
                "Filter by Process Name",
                "Filter by Memory Usage",
                "Filter by CPU usage",
            };
            
            _filtrationCombo = new ComboBox(_filtrationOptions);
            _filtrationCombo.Changed += ComboOnChanged;
            
            _filtrationHBox = new HBox(false, 5);
            _filtrationHBox.PackStart(_filtrationCombo, false, false, 0);
            

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
            
            
            
            _treeModelFilter = new TreeModelFilter(_store, null);
            _treeModelFilter.VisibleFunc = Filter;

            _treeModelSort = new TreeModelSort(_treeModelFilter);
            _treeModelSort.SetSortFunc(0, WindowBuilderHelper.IdSortFunc);
            _treeModelSort.SetSortFunc(1, WindowBuilderHelper.ProcessNameSortFunc);
            _treeModelSort.SetSortFunc(2, WindowBuilderHelper.MemoryUsageSortFunc);
            _treeModelSort.SetSortFunc(3, WindowBuilderHelper.PrioritySortFunc);
            _treeModelSort.SetSortFunc(4, WindowBuilderHelper.UserCpuTimeSortFunc);
            _treeModelSort.SetSortFunc(5, WindowBuilderHelper.PrivilegedCpuTimeSortFunc);
            _treeModelSort.SetSortFunc(6, WindowBuilderHelper.TotalCpuTimeSortFunc);
            _treeModelSort.SetSortFunc(7, WindowBuilderHelper.CpuUsageSortFunc);
            _treeModelSort.SetSortFunc(8, WindowBuilderHelper.ThreadCountSortFunc);
            _treeModelSort.SetSortFunc(9, WindowBuilderHelper.StartTimeSortFunc);
            
            _treeView = new TreeView();
            _treeView.Model = _treeModelSort;
            _treeView.Selection.Mode = SelectionMode.Multiple;
            _treeView.Selection.Changed += OnSelectionChanged;
            
            _cellRendererText = new CellRendererText();
            _cellRendererText.Alignment = Pango.Alignment.Right;
            _cellRendererText.Xalign = 0.5f;
            
            for (int i = 0; i < 10; i++)
            {
                _treeViewColumn = new TreeViewColumn();
                _treeViewColumn.Clickable = true;
                _treeViewColumn.Resizable = true;
                _treeViewColumn.Title = columnLabels[i];
                _treeViewColumn.SortIndicator = true;
                _treeViewColumn.Alignment = 0.5f;
                _treeViewColumn.Expand = true;
                _treeViewColumn.SortColumnId = i;
                _treeViewColumn.PackStart(_cellRendererText, true);
                _treeViewColumn.AddAttribute(_cellRendererText, "text", i);

                switch (i)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        _treeViewColumn.SetCellDataFunc(_cellRendererText, WindowBuilderHelper.MemoryUsageFormatter);
                        break;
                    case 3:
                        break;
                    case 4:
                        _treeViewColumn.SetCellDataFunc(_cellRendererText, WindowBuilderHelper.UserCpuTimeFormatter);
                        break;
                    case 5:
                        _treeViewColumn.SetCellDataFunc(_cellRendererText, WindowBuilderHelper.PrivilegedCpuTimeFormatter);
                        break;
                    case 6:
                        _treeViewColumn.SetCellDataFunc(_cellRendererText, WindowBuilderHelper.TotalCpuTimeFormatter);
                        break;
                    case 7:
                        _treeViewColumn.SetCellDataFunc(_cellRendererText, WindowBuilderHelper.CpuUsageFormatter);
                        break;
                    case 8:
                        break;
                    case 9:
                        _treeViewColumn.SetCellDataFunc(_cellRendererText, WindowBuilderHelper.StartTimeFormatter);
                        break;
                }

                _treeView.AppendColumn(_treeViewColumn);
            }
            
            _scrolledWindow = new ScrolledWindow();
            _scrolledWindow.Add(_treeView);

            _killButton = new Button("Kill process");
            _killButton.Clicked += KillProcess;
            
            _windowVBox = new VBox (false, 5);
            _windowVBox.PackStart (_windowHBox, false, false, 0);
            _windowVBox.PackStart(_filtrationHBox, false, false, 0);
            _windowVBox.PackStart(_scrolledWindow, true, true, 0);
            _windowVBox.PackStart(_killButton, false, false, 0);
            
            
            _window.Add(_windowVBox);
            
            
            // Create an instance of the object Updater
            _processGrabber = new ProcessGrabber();
            _processGrabber.OnResult += (sender, processList) =>
            {
                Application.Invoke(delegate
                {
                    _currentScrollPosition = _treeView.Vadjustment.Value;
                    StoreClear();
                    LoadStore(processList);

                    _treeView.ShowAll();
                });
            };

            _treeView.Vadjustment.Changed += (sender, args) =>
            {
                _treeView.Vadjustment.Value = _currentScrollPosition;
            }; 
            
            
            _treeView.ShowAll();
            _window.ShowAll();
            _filtrationHBox.Hide();
            _processGrabber.Run();
        }
        
        private AboutDialog CreateAboutDialog()
        {
            AboutDialog aboutDialog = new AboutDialog();
            
            aboutDialog.Title = "About Process Watch";
            aboutDialog.Authors = new[]
            {
                "Wojciech Wesołowski",
                "Marek Krzysztofiak"
            };
            aboutDialog.Copyright = "Copyright \xa9 2021 Codecool";
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

            return aboutDialog;
        }

        private void ComboOnChanged(object sender, EventArgs args)
        {
            ComboBox combo = (ComboBox) sender;

            switch (_filtrationOptions[combo.Active])
            {
                case "All processes":
                    HideAllEntryWidgets();
                    break;
                case "Filter by PID":
                    _columnFilter = 0;
                    ShowFilterWidgets(_processIdEntry);
                    break;
                case "Filter by Process Name":
                    _columnFilter = 1;
                    ShowFilterWidgets(_processNameEntry);
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
        
        private void OnlyNumerical(object sender, TextInsertedArgs args)
        {
            Entry entry = (Entry) sender;

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

        private void HideAllEntryWidgets()
        {
            _processNameEntry.Text = "";
            _processIdEntry.Text = "";
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
        
        private void OnChanged(object sender, EventArgs args)
        {
            switch (_columnFilter)
            {
                case 0:
                    _textToFilter = _processIdEntry.Text;
                    break;
                case 1: 
                    _textToFilter = _processNameEntry.Text;
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
            
            _treeModelFilter.Refilter();
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

        private bool Filter(ITreeModel model, TreeIter iter)
        {
            try
            {
                string columnValue = model.GetValue(iter, _columnFilter).ToString();
                
                if (_textToFilter == "" || columnValue == "")
                    return true;

                if (_textToFilter.EndsWith('%'))
                {
                    string[] _textToFilterSplitted = _textToFilter.Split(" ");
                    double userInput = Convert.ToDouble(_textToFilterSplitted[0]);
                    double cpuUsage = Convert.ToDouble(columnValue);
                    
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

                    double memoryUsage = Convert.ToDouble(columnValue);
                

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

                if (columnValue.IndexOf(_textToFilter, StringComparison.CurrentCultureIgnoreCase) > -1)
                    return true;
                
                return false;
            }
            catch (NullReferenceException e)
            {
                return false;
            }
        }

        private void StoreClear()
        {
            TreeIter iter;
            _store.GetIterFirst(out iter);
            for (int i = 0; i < _store.IterNChildren(); i++)
            {
                _store.SetValues(iter, "", "", "", "", "", "", "", "", "", "");

                _store.IterNext(ref iter);
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
            _store.GetIterFirst(out iter);
            for (int i = 0; i < _store.IterNChildren(); i++)
            {
                if (element.Count - 0 > i)
                {
                    _store.SetValues(iter,
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
                    _store.IterNext(ref iter);
                }
                else
                {
                    _store.Remove(ref iter);
                }
                
                elementIndex++;
            }

            if (element.Count > elementIndex)
            {
                for (int i = elementIndex; i < element.Count; i++)
                {
                    _store.AppendValues(element[i].Id.ToString(),
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

        static void OnWindowClose (object obj, DeleteEventArgs args)
        {
            _processGrabber.Stop();
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
}