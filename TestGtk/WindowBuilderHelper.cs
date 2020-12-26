using Gtk;

namespace TestGtk
{
    public class WindowBuilderHelper
    {
        
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