using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
                string txt = lstMatchingFiles.SelectedItem.ToString();

                string argument = "/select, \"" + txt + "\"";

                Process.Start("explorer.exe", argument);
            }
        }
    }
}
