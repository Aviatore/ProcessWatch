using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly AboutDialog _aboutDialog;
        private readonly ComboBox _cpuFiltrationDirectionComboBox;
        private readonly ComboBox _memoryFiltrationDirectionComboBox;
        private readonly ComboBox _memoryFiltrationUnitsComboBox;
        private double _currentScrollPosition;
        private readonly Entry _cpuFiltrationEntry;
        private readonly Entry _memoryFiltrationEntry;
        private readonly Entry _processIdEntry;
        private readonly Entry _processNameEntry;
        private readonly HBox _cpuFiltrationHbox;
        private readonly HBox _filtrationHBox;
        private readonly HBox _memoryFiltrationHbox;
        private int _columnFilter;
        private readonly List<int> _processIdToKill;
        private static ListStore _listStore;
        private static ProcessGrabber _processGrabber;
        private readonly string[] _filtrationDirectionOptions;
        private readonly string[] _filtrationOptions;
        private readonly string[] _memoryFiltrationDirectionUnits;
        private string _textToFilter;
        private readonly TreeModelFilter _treeModelFilter;
        private readonly Window _window;

        public WindowBuilder()
        {
            _columnFilter = 0;
            _textToFilter = "";
            
            _processIdToKill = new List<int>();
            _listStore = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), 
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));

            Application.Init();

            _window = new Window ("Label sample");
            _window.Resize(1300, 600);
            _window.Title = "Process Watch";
            _window.SetIconFromFile("icons/processIconSmall.png");
            _window.BorderWidth = 5;
            _window.DeleteEvent += OnWindowClose;
            
            var aboutButton = new Button();
            var aboutIcon = new Image();
            aboutIcon.Pixbuf = new Pixbuf("icons/information.png");
            aboutButton.Image = aboutIcon;
            aboutButton.TooltipText = "About Process Watch";
            aboutButton.Clicked += (sender, args) =>
            {
                _aboutDialog.Show();
            };
            
            _aboutDialog = CreateAboutDialog();

            var filterButton = new Button();
            filterButton.Image = new Image(Stock.Find, IconSize.Button);
            filterButton.TooltipText = "Filtration utilities";
            filterButton.Clicked += (sender, args) =>
            {
                if (_filtrationHBox.IsVisible)
                    _filtrationHBox.Hide();
                else
                    _filtrationHBox.ShowAll();
            };
            
            var windowHBox = new HBox (false, 5);
            windowHBox.PackEnd(aboutButton, false, false, 0);
            windowHBox.PackEnd(filterButton, false, false, 0);

            _processNameEntry = new Entry();
            _processNameEntry.Changed += OnChanged;
            
            _processIdEntry = new Entry();
            _processIdEntry.Changed += OnChanged;
            _processIdEntry.TextInserted += OnlyNumerical;

            // String values for the combobox - filtration direction
            _filtrationDirectionOptions = new[]
            {
                ">",
                "≥",
                "=",
                "≤",
                "<"
            };

            // String values for the combobox - memory usage units
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
            
            _memoryFiltrationDirectionComboBox = new ComboBox(_filtrationDirectionOptions);
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
            
            _cpuFiltrationDirectionComboBox = new ComboBox(_filtrationDirectionOptions);
            _cpuFiltrationDirectionComboBox.Changed += OnChanged;
            
            var cpuFiltrationLabel = new Label("%"); 
            
            _cpuFiltrationHbox = new HBox();
            _cpuFiltrationHbox.PackStart(_cpuFiltrationDirectionComboBox, false, false, 0);
            _cpuFiltrationHbox.PackStart(_cpuFiltrationEntry, false, false, 0);
            _cpuFiltrationHbox.PackStart(cpuFiltrationLabel, false, false, 0);
            
            
            _filtrationOptions = new[]
            {
                "All processes",
                "Filter by PID",
                "Filter by Process Name",
                "Filter by Memory Usage",
                "Filter by CPU usage",
            };
            
            var filtrationCombo = new ComboBox(_filtrationOptions);
            filtrationCombo.Changed += ComboOnChanged;
            
            _filtrationHBox = new HBox(false, 5);
            _filtrationHBox.PackStart(filtrationCombo, false, false, 0);
            

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


            _treeModelFilter = new TreeModelFilter(_listStore, null);
            _treeModelFilter.VisibleFunc = Filter;

            var treeModelSort = new TreeModelSort(_treeModelFilter);
            treeModelSort.SetSortFunc(0, WindowBuilderHelper.IdSortFunc);
            treeModelSort.SetSortFunc(1, WindowBuilderHelper.ProcessNameSortFunc);
            treeModelSort.SetSortFunc(2, WindowBuilderHelper.MemoryUsageSortFunc);
            treeModelSort.SetSortFunc(3, WindowBuilderHelper.PrioritySortFunc);
            treeModelSort.SetSortFunc(4, WindowBuilderHelper.UserCpuTimeSortFunc);
            treeModelSort.SetSortFunc(5, WindowBuilderHelper.PrivilegedCpuTimeSortFunc);
            treeModelSort.SetSortFunc(6, WindowBuilderHelper.TotalCpuTimeSortFunc);
            treeModelSort.SetSortFunc(7, WindowBuilderHelper.CpuUsageSortFunc);
            treeModelSort.SetSortFunc(8, WindowBuilderHelper.ThreadCountSortFunc);
            treeModelSort.SetSortFunc(9, WindowBuilderHelper.StartTimeSortFunc);
            
            var treeView = new TreeView();
            treeView.Model = treeModelSort;
            treeView.Selection.Mode = SelectionMode.Multiple;
            treeView.Selection.Changed += OnSelectionChanged;
            
            // Create a CellRendererText responsible for proper rendering cell data
            var cellRendererText = new CellRendererText();
            cellRendererText.Alignment = Pango.Alignment.Right;
            cellRendererText.Xalign = 0.5f;
            
            // Load the _treeView with TreeViewColumns
            for (int i = 0; i < 10; i++)
            {
                var treeViewColumn = new TreeViewColumn();
                treeViewColumn.Clickable = true;
                treeViewColumn.Resizable = true;
                treeViewColumn.Title = columnLabels[i];
                treeViewColumn.SortIndicator = true;
                treeViewColumn.Alignment = 0.5f;
                treeViewColumn.Expand = true;
                treeViewColumn.SortColumnId = i;
                treeViewColumn.PackStart(cellRendererText, true);
                treeViewColumn.AddAttribute(cellRendererText, "text", i);

                switch (i)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        treeViewColumn.SetCellDataFunc(cellRendererText, WindowBuilderHelper.MemoryUsageFormatter);
                        break;
                    case 3:
                        break;
                    case 4:
                        treeViewColumn.SetCellDataFunc(cellRendererText, WindowBuilderHelper.UserCpuTimeFormatter);
                        break;
                    case 5:
                        treeViewColumn.SetCellDataFunc(cellRendererText, WindowBuilderHelper.PrivilegedCpuTimeFormatter);
                        break;
                    case 6:
                        treeViewColumn.SetCellDataFunc(cellRendererText, WindowBuilderHelper.TotalCpuTimeFormatter);
                        break;
                    case 7:
                        treeViewColumn.SetCellDataFunc(cellRendererText, WindowBuilderHelper.CpuUsageFormatter);
                        break;
                    case 8:
                        break;
                    case 9:
                        treeViewColumn.SetCellDataFunc(cellRendererText, WindowBuilderHelper.StartTimeFormatter);
                        break;
                }

                treeView.AppendColumn(treeViewColumn);
            }
            
            // Create a scrollable window
            var scrolledWindow = new ScrolledWindow();
            scrolledWindow.Add(treeView);

            var killButton = new Button("Kill process");
            killButton.Clicked += KillProcess;
            
            var windowVBox = new VBox (false, 5);
            windowVBox.PackStart (windowHBox, false, false, 0);
            windowVBox.PackStart(_filtrationHBox, false, false, 0);
            windowVBox.PackStart(scrolledWindow, true, true, 0);
            windowVBox.PackStart(killButton, false, false, 0);
            
            _window.Add(windowVBox);
            
            // Create an instance of the object Updater
            _processGrabber = new ProcessGrabber();
            // Add a callback executed when _processGrabber takes process data.
            // The callback clears the _treeView content and loads new data
            // Before clearing the _treeView content the callback saves the current scroll position
            _processGrabber.OnResult += (sender, processList) =>
            {
                Application.Invoke(delegate
                {
                    _currentScrollPosition = treeView.Vadjustment.Value;
                    StoreClear();
                    LoadStore(processList);

                    treeView.ShowAll();
                });
            };

            // Add a callback executed after 'Changed' event raised after changing the position of the _treeView
            // When the _treeView content is reloaded the previous scroll position is updated
            treeView.Vadjustment.Changed += (sender, args) =>
            {
                treeView.Vadjustment.Value = _currentScrollPosition;
            }; 
            
            treeView.ShowAll();
            _window.ShowAll();
            
            // Hide widgets related to process filtration
            _filtrationHBox.Hide();
            
            // Start the Timer process responsible for grabbing process data periodically 
            _processGrabber.Run();
        }
        
        /// <summary>
        /// Function that creates the dialog containing information about the program.
        /// </summary>
        /// <returns>AboutDialog instance</returns>
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

        /// <summary>
        /// Callback executed after Changed event raised by _filtrationCombo
        /// </summary>
        /// <param name="sender">_filtrationCombo object</param>
        /// <param name="args">Event arguments</param>
        private void ComboOnChanged(object sender, EventArgs args)
        {
            ComboBox combo = (ComboBox) sender;

            // combo.Active returns the index of the actually selected item
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

        /// <summary>
        /// Hide all filtration entry widgets except the specified one
        /// </summary>
        /// <param name="widget">The widget to be shown</param>
        private void ShowFilterWidgets(Widget widget)
        {
            HideAllEntryWidgets();
            _filtrationHBox.PackStart(widget, false, false, 0);
            _filtrationHBox.ShowAll();
        }
        
        /// <summary>
        /// Callback method executed on TextInserted event by Entry widget to force only digits
        /// </summary>
        /// <param name="sender">Entry widget</param>
        /// <param name="args">Text inserted arguments</param>
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

        /// <summary>
        /// Hide all Entry widgets and clear their input values.
        /// </summary>
        private void HideAllEntryWidgets()
        {
            _processNameEntry.Text = "";
            _processIdEntry.Text = "";
            _memoryFiltrationEntry.Text = "";
            _cpuFiltrationEntry.Text = "";
            
            // Loop through all _filtrationHBox children and remove all of them but 'GtkComboBox'
            _filtrationHBox.Foreach(widget =>
            {
                if (widget.Name != "GtkComboBox")
                {
                    _filtrationHBox.Remove(widget);
                }
            });
        }
        
        /// <summary>
        /// Callback executed after changing a value within any filtration Entry field. 
        /// </summary>
        /// <param name="sender">Widget that call the 'Changed' event</param>
        /// <param name="args">Event arguments</param>
        private void OnChanged(object sender, EventArgs args)
        {
            // _columnFilter - id of the column by which the filtration will be performed
            switch (_columnFilter)
            {
                // Assign a filtration text to the global variable _textToFilter
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
            
            // Execute filtration callback
            _treeModelFilter.Refilter();
        }

        /// <summary>
        /// Converts formatted memory size to bytes.
        /// </summary>
        /// <param name="size">Memory usage expressed in the specified unit. The size is provided as a string.</param>
        /// <param name="unit">Unit of the memory size.</param>
        /// <returns>Double - raw memory usage in bytes</returns>
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

        /// <summary>
        /// Method executed during filtration (after executing the _treeModelFilter.Refilter() method)
        /// </summary>
        /// <param name="model">TreeModel object</param>
        /// <param name="iter">TreeIter</param>
        /// <returns>'True' if the column value should be kept or 'false' if the column value should hidden</returns>
        private bool Filter(ITreeModel model, TreeIter iter)
        {
            try
            {
                string columnValue = model.GetValue(iter, _columnFilter).ToString();
                
                if (_textToFilter == "" || columnValue == "")
                    return true;

                // Filtration rule if the text to be filtered ends with percent character (CPU usage) 
                if (_textToFilter.EndsWith('%'))
                {
                    string[] textToFilterSplitted = _textToFilter.Split(" ");
                    double userInput = Convert.ToDouble(textToFilterSplitted[0]);
                    double cpuUsage = Convert.ToDouble(columnValue);
                    
                    switch (_filtrationDirectionOptions[_cpuFiltrationDirectionComboBox.Active])
                    {
                        case ">":
                            return cpuUsage > userInput;
                        case "≥":
                            return cpuUsage >= userInput;
                        case "=":
                            return Math.Abs(cpuUsage - userInput) < 0.1;
                        case "≤":
                            return cpuUsage <= userInput;
                        case "<":
                            return cpuUsage < userInput;
                        default:
                            return true;
                    }
                }
                
                // Filtration rule if the text to be filtered ends with 'B' character (memory usage)
                if (_textToFilter.EndsWith('B'))
                {
                    string[] textToFilterSplitted = _textToFilter.Split(" ");
                    string memSize = textToFilterSplitted[0];
                    string memUnit = textToFilterSplitted[1];

                    double memoryUsage = Convert.ToDouble(columnValue);
                

                    switch (_filtrationDirectionOptions[_memoryFiltrationDirectionComboBox.Active])
                    {
                        case ">":
                            return memoryUsage > MemSizeToRaw(memSize, memUnit);
                        case "≥":
                            return memoryUsage >= MemSizeToRaw(memSize, memUnit);
                        case "=":
                            return Math.Abs(memoryUsage - MemSizeToRaw(memSize, memUnit)) < 100;
                        case "≤":
                            return memoryUsage <= MemSizeToRaw(memSize, memUnit);
                        case "<":
                            return memoryUsage < MemSizeToRaw(memSize, memUnit);
                        default:
                            return true;
                    }
                }

                // Filtration rule if the text to be filtered is a simple string (PID or process name)
                if (columnValue != null && columnValue.IndexOf(_textToFilter, StringComparison.CurrentCultureIgnoreCase) > -1)
                    return true;
                
                return false;
            }
            catch (NullReferenceException e)
            {
                return false;
            }
        }

        /// <summary>
        /// Clear _listStore rows by filling columns with empty strings.
        /// </summary>
        private void StoreClear()
        {
            TreeIter iter;
            _listStore.GetIterFirst(out iter);
            
            for (int i = 0; i < _listStore.IterNChildren(); i++)
            {
                _listStore.SetValues(iter, "", "", "", "", "", "", "", "", "", "");

                _listStore.IterNext(ref iter);
            }
        }
        
        /// <summary>
        /// Load process data into _listStore.
        /// </summary>
        /// <param name="element">List of ProcessMod objects containing data about all processes</param>
        private void LoadStore(List<ProcessMod> element)
        {
            // Index of _listStore row
            int elementIndex = 0;
            
            TreeIter iter;
            _listStore.GetIterFirst(out iter);
            
            for (int i = 0; i < _listStore.IterNChildren(); i++)
            {
                // If the current row index is less than the total number of processes
                // load process data into the current _listStore row
                if (element.Count > i)
                {
                    _listStore.SetValues(iter,
                        element[i].Id.ToString(CultureInfo.InvariantCulture),
                        element[i].ProcessName,
                        element[i].WorkingSet64.ToString(),
                        element[i].PriorityClass,
                        element[i].UserProcessorTime.ToString(CultureInfo.InvariantCulture),
                        element[i].PrivilegedProcessorTime.ToString(CultureInfo.InvariantCulture),
                        element[i].TotalProcessorTime.ToString(CultureInfo.InvariantCulture),
                        element[i].CpuUsage.ToString(CultureInfo.InvariantCulture),
                        element[i].ThreadCount.ToString(),
                        element[i].StartTime.ToString()
                    );
                    _listStore.IterNext(ref iter);
                }
                // If the number of rows is higher than the total number of processes
                // remove empty _listStore row
                else
                {
                    _listStore.Remove(ref iter);
                }
                
                elementIndex++;
            }

            // If the total process number is higher than the number of rows in _listStore
            // add new rows and fill them with process data
            if (element.Count > elementIndex)
            {
                for (int i = elementIndex; i < element.Count; i++)
                {
                    _listStore.AppendValues(element[i].Id.ToString(CultureInfo.InvariantCulture),
                        element[i].ProcessName,
                        element[i].WorkingSet64.ToString(),
                        element[i].PriorityClass,
                        element[i].UserProcessorTime.ToString(CultureInfo.InvariantCulture),
                        element[i].PrivilegedProcessorTime.ToString(CultureInfo.InvariantCulture),
                        element[i].TotalProcessorTime.ToString(CultureInfo.InvariantCulture),
                        element[i].CpuUsage.ToString(CultureInfo.InvariantCulture),
                        element[i].ThreadCount.ToString(),
                        element[i].StartTime.ToString());
                }
            }
        }

        /// <summary>
        /// A method called on click event of 'Kill process' button.
        /// Kill all processes which IDs are stored in the list _processIdToKill.
        /// </summary>
        /// <param name="o">Reference to the button object</param>
        /// <param name="args">Event arguments</param>
        private void KillProcess(object o, EventArgs args)
        {
            // KillDialog - a dialog that pops up to confirm process assassination
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

                // Add a callback to click event on any button within KillDialog
                killDialog.Response += (o1, responseArgs) =>
                {
                    switch (responseArgs.ResponseId)
                    {
                        // Click on YES button
                        case ResponseType.Yes:
                            foreach (var id in _processIdToKill)
                            {
                                Process process = Process.GetProcessById(id);
                                process.Kill();
                                
                                Console.WriteLine($"{id.ToString()} killed");
                                
                                // Cleaning up
                                process.Dispose();
                            }

                            break;
                        // Click on NO button
                        case ResponseType.No:
                            Console.WriteLine("Abort killing.");
                            break;
                    }
                };
                
                killDialog.Run();
            }
        }

        /// <summary>
        /// Callback method executed after closing the main window.
        /// </summary>
        /// <param name="obj">Window object</param>
        /// <param name="args">Delete event arguments</param>
        static void OnWindowClose (object obj, DeleteEventArgs args)
        {
            // Stop Timer process responsible for grabbing process data periodically 
            _processGrabber.Stop();
            
            Application.Quit();
        }
        
        /// <summary>
        /// Callback method executed when row/rows is/are selected
        /// </summary>
        /// <param name="o">TreeSelection object</param>
        /// <param name="args">Event arguments</param>
        void OnSelectionChanged(object o, EventArgs args)
        {
            TreeSelection selection = (TreeSelection)o;
            
            ITreeModel filtered;
            TreePath[] selectedRows = selection.GetSelectedRows(out filtered);

            // Clear a list storing PID to kill
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

        /// <summary>
        /// Method to start application main loop
        /// </summary>
        public void Run()
        {
            Application.Run();
        }
    }
}