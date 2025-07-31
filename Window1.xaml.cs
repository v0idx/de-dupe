using System.Diagnostics;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }
        
        public Window1(List<string> listContent)
        {
            InitializeComponent();
            lstMatchingFiles.ItemsSource = listContent;
        }
        
        private void ctxOpen_Click(object sender, RoutedEventArgs e)
        {
            if (lstMatchingFiles.SelectedItem != null)
            {
                string? txt = lstMatchingFiles.SelectedItem.ToString();

                string argument = "/select, \"" + txt + "\"";

                Process.Start("explorer.exe", argument);
            }
        }
    }
}
