using Gtk;


using TestGtk.View;

namespace TestGtk
{
    class Program
    {
        static void Main(string[] args)
        {
            WindowBuilder windowBuilder = new WindowBuilder();
            //windowBuilder.Run();
            Application.Run();
        }
    }
}

/*
public class TreeViewExample
{
    public static void Main ()
    {
        Gtk.Application.Init ();
        new TreeViewExample ();
        Gtk.Application.Run ();
    }
 
    Gtk.Entry filterEntry;
 
    Gtk.TreeModelFilter filter;
 
    public TreeViewExample ()
    {
        // Create a Window
        Gtk.Window window = new Gtk.Window ("TreeView Example");
        window.SetSizeRequest (500,200);
 
        // Create an Entry used to filter the tree
        filterEntry = new Gtk.Entry ();
 
        // Fire off an event when the text in the Entry changes
        filterEntry.Changed += OnFilterEntryTextChanged;
 
        // Create a nice label describing the Entry
        Gtk.Label filterLabel = new Gtk.Label ("Artist Search:");
 
        // Put them both into a little box so they show up side by side
        Gtk.HBox filterBox = new Gtk.HBox ();
        filterBox.PackStart (filterLabel, false, false, 5);
        filterBox.PackStart (filterEntry, true, true, 5);
 
        // Create our TreeView
        Gtk.TreeView tree = new Gtk.TreeView ();
 
        // Create a box to hold the Entry and Tree
        Gtk.VBox box = new Gtk.VBox ();
 
        // Add the widgets to the box
        box.PackStart (filterBox, false, false, 5);
        box.PackStart (tree, true, true, 5);
 
        window.Add (box);
 
        // Create a column for the artist name
        Gtk.TreeViewColumn artistColumn = new Gtk.TreeViewColumn ();
        artistColumn.Title = "Artist";
 
        // Create the text cell that will display the artist name
        Gtk.CellRendererText artistNameCell = new Gtk.CellRendererText ();
 
        // Add the cell to the column
        artistColumn.PackStart (artistNameCell, true);
 
        // Create a column for the song title
        Gtk.TreeViewColumn songColumn = new Gtk.TreeViewColumn ();
        songColumn.Title = "Song Title";
 
        // Do the same for the song title column
        Gtk.CellRendererText songTitleCell = new Gtk.CellRendererText ();
        songColumn.PackStart (songTitleCell, true);
 
        // Add the columns to the TreeView
        tree.AppendColumn (artistColumn);
        tree.AppendColumn (songColumn);
 
        // Tell the Cell Renderers which items in the model to display
        artistColumn.AddAttribute (artistNameCell, "text", 0);
        songColumn.AddAttribute (songTitleCell, "text", 1);
 
        // Create a model that will hold two strings - Artist Name and Song Title
        Gtk.ListStore musicListStore = new Gtk.ListStore (typeof (string), typeof (string));
 
        // Add some data to the store
        musicListStore.AppendValues ("BT", "Circles");
        musicListStore.AppendValues ("Daft Punk", "Technologic");
        musicListStore.AppendValues ("Daft Punk", "Digital Love");
        musicListStore.AppendValues ("The Crystal Method", "PHD");
        musicListStore.AppendValues ("The Crystal Method", "Name of the game");
        musicListStore.AppendValues ("The Chemical Brothers", "Galvanize");
 
        // Instead of assigning the ListStore model directly to the TreeStore, we create a TreeModelFilter
        // which sits between the Model (the ListStore) and the View (the TreeView) filtering what the model sees.
        // Some may say that this is a "Controller", even though the name and usage suggests that it is still part of
        // the Model.
        filter = new Gtk.TreeModelFilter (musicListStore, null);
 
        // Specify the function that determines which rows to filter out and which ones to display
        filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTree);
 
        // Assign the filter as our tree's model
        tree.Model = filter;
 
        // Show the window and everything on it
        window.ShowAll ();
    }
 
    private void OnFilterEntryTextChanged (object o, System.EventArgs args)
    {
        // Since the filter text changed, tell the filter to re-determine which rows to display
        filter.Refilter ();
    }
 
    private bool FilterTree (ITreeModel model, Gtk.TreeIter iter)
    {
        string artistName = model.GetValue (iter, 0).ToString ();
 
        if (filterEntry.Text == "")
            return true;
 
        if (artistName.IndexOf (filterEntry.Text) > -1)
            return true;
        else
            return false;
    }
}
*/