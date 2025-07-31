using Microsoft.Win32;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Windows.Media.Animation;

namespace WpfApp1
{

    public class ListItems : INotifyPropertyChanged
    {
        private string filename;
        private bool isDuplicate;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDuplicate
        {
            get {  return isDuplicate; }
            set { 
                isDuplicate = value;
                NotifyPropertyChanged();
            }
        }

        public string Filename
        {
            get { return filename; }
            set { 
                filename = value;
                NotifyPropertyChanged();
            }
        }

        public ListItems()
        {
            filename = "";
            isDuplicate = false;
        }

        public ListItems(string inName)
        {
            filename = inName;
            isDuplicate = false;
        }

        public override string ToString()
        {
            return filename;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName ="")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    public class MatchCheck
    {
        private string filename;
        private List<int> indexes;
        private byte[] hash;
        public MatchCheck()
        {
            this.indexes = new List<int>();
            this.filename = "";
            hash = [];
        }

        public MatchCheck(string inName, int inIndex)
        {
            this.filename = inName;
            this.indexes = [inIndex];
            hash = [];
        }

        public MatchCheck(byte[] inHash, int inIndex)
        {
            this.filename = "";
            this.hash = inHash;
            this.indexes = [inIndex];
        }

        public void setFilename(string inName)
        {
            this.filename = inName;
        }

        public void addIndex(int index)
        {
            indexes.Add(index);
        }

        public byte[] getHash()
        {
            return hash;
        }

        public string getFilename()
        {
            return this.filename;
        }

        public List<int> getIndexes()
        {
            return this.indexes;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool scanned;
        private List<ListItems> itemsInDir;
        private List<MatchCheck> duplicateItems;

        private HashSet<string> duplicateFileNames;

        public MainWindow()
        {
            InitializeComponent();
            this.scanned = false;
            this.itemsInDir = new List<ListItems>();
            this.duplicateItems = new List<MatchCheck>();
            this.duplicateFileNames = new HashSet<string>();
        }


        private void ButtonPickDirectory_Click(object sender, RoutedEventArgs e)
        {
            this.itemsInDir.Clear();
            lstFiles.ItemsSource = new List<ListItems>();
            var folderDialog = new OpenFolderDialog
            {
                Title = "Select Folder to Scan",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;

                var enumOptions = new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true
                };

                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    var filesEnum = Directory.EnumerateFiles(folderName, "*", enumOptions).AsParallel();

                    foreach(var file in filesEnum)
                    {
                        this.itemsInDir.Add(new ListItems(file.ToString()));
                    }

                    lstFiles.ItemsSource = this.itemsInDir;
                }
                finally
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                    this.Title = $"Displaying Contents of: {folderName}";
                }

                this.scanned = false;

            }
        }

        private void ctxOpen_Click(object sender, RoutedEventArgs e)
        { 
            if (lstFiles.SelectedItem != null)
            {
                string txt = lstFiles.SelectedItem.ToString();

                string argument = "/select, \"" + txt + "\"";

                Process.Start("explorer.exe", argument);
            }

        }

        private void ctxMatch_Click(object sender, RoutedEventArgs e)
        {
            List<string> items = new List<string>();
            string? fileName = lstFiles.SelectedItem.ToString();

            

                if (fileName != null)
                {

                    byte[] currHash = [];
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(fileName))
                        {
                            currHash = md5.ComputeHash(stream);
                        }
                    }

                foreach (var item in duplicateItems)
                    {
                        if (item.getFilename() == System.IO.Path.GetFileName(fileName))
                        {
                            foreach (var tmp in this.itemsInDir)
                            {
                                if (tmp.Filename.Contains(item.getFilename()))
                                {
                                    items.Add(tmp.Filename);
                                }
                            }
                        } else
                    {
                        if (item.getHash().SequenceEqual(currHash))
                        {
                            foreach(var idx in item.getIndexes())
                            {
                                items.Add(this.itemsInDir[idx].Filename);
                            }
                        }
                    }



                    }


                }

            Window1 matchWin = new Window1(items);
            matchWin.Show();
        }

        private void GetDuplicates(List<MatchCheck> matches)
        {

            List<MatchCheck> duplicates = new List<MatchCheck>();


            foreach(var item in matches)
            {
                var indexes = item.getIndexes();

                if (indexes.Count() > 1)
                {
                    duplicates.Add(item);
                    foreach(var idx in indexes)
                    {
                        this.itemsInDir[idx].IsDuplicate = true;
                    }
                }
            }

            if (duplicates != null)
            {
                this.duplicateItems.AddRange(duplicates);
            }

        }

        private void ButtonScan_Click(object sender, RoutedEventArgs e)
        {
            bool nameCheck = (bool)chkNameMatches.IsChecked;
            bool contentCheck = (bool)chkContentMatches.IsChecked;
            bool exists = false;

            List<MatchCheck> nameMatches = new List<MatchCheck>();

            if (nameCheck)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                foreach (var item in lstFiles.Items)
                {
                    foreach (var tmp in nameMatches)
                    {
                        if (System.IO.Path.GetFileName(item.ToString()).Equals(tmp.getFilename()))
                        {
                            tmp.addIndex(lstFiles.Items.IndexOf(item));
                            exists = true;
                        }
                    }
                    if (!exists)
                    {
                        nameMatches.Add(new MatchCheck(System.IO.Path.GetFileName(item.ToString()), lstFiles.Items.IndexOf(item)));
                    }
                }

                GetDuplicates(nameMatches);

            }
            if (contentCheck)
            {

                var matches = new List<MatchCheck>();

                foreach (var item in this.itemsInDir)
                {
                    var currItem = item.Filename;
                    byte[] currHash = [];
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(currItem))
                        {
                            currHash = md5.ComputeHash(stream);
                        }
                    }

                    exists = false;

                    foreach (var match in matches)
                    {
                        if (match.getHash().SequenceEqual(currHash))
                        {
                            match.addIndex(this.itemsInDir.IndexOf(item));
                            exists = true;
                        }
                    }
                    if (!exists)
                    {
                        matches.Add(new MatchCheck(currHash, this.itemsInDir.IndexOf(item)));
                    }
                }

                GetDuplicates(matches);

            }

            Mouse.OverrideCursor = Cursors.Arrow;
            lstFiles.ItemsSource = this.itemsInDir;


        }
    }
}