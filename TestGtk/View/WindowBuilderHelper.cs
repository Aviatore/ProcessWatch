using System;
using Gtk;
using TestGtk.Model;

namespace TestGtk.View
{
    public class WindowBuilderHelper
    {
        
        
        public static void MemoryUsageFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
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
        
        public static void UserCpuTimeFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
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
        
        public static void PrivilegedCpuTimeFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
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
        
        public static void TotalCpuTimeFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
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
        
        public static void CpuUsageFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
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
        
        public static void StartTimeFormatter(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
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
        
        public static int IdSortFunc(ITreeModel model, TreeIter a, TreeIter b)
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
        
        public static int ProcessNameSortFunc(ITreeModel model, TreeIter a, TreeIter b)
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
        
        public static int MemoryUsageSortFunc(ITreeModel model, TreeIter a, TreeIter b)
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
        
        public static int PrioritySortFunc(ITreeModel model, TreeIter a, TreeIter b)
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
        
        public static int UserCpuTimeSortFunc(ITreeModel model, TreeIter a, TreeIter b)
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
        
        public static int PrivilegedCpuTimeSortFunc(ITreeModel model, TreeIter a, TreeIter b)
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
        
        public static int TotalCpuTimeSortFunc(ITreeModel model, TreeIter a, TreeIter b)
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
        
        public static int CpuUsageSortFunc(ITreeModel model, TreeIter a, TreeIter b)
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
        
        public static int ThreadCountSortFunc(ITreeModel model, TreeIter a, TreeIter b)
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
        
        public static int StartTimeSortFunc(ITreeModel model, TreeIter a, TreeIter b)
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