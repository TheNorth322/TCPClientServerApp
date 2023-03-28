using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCPClientApp.Model;
using TCPClientApp.UI.ViewModels;

namespace TCPClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new TCPClientViewModel();
            (DataContext as ViewModelBase).MessageBoxRequest += ViewMessageBoxRequest;
            InitializeComponent();
        }

        private void ViewMessageBoxRequest(object sender, MessageBoxEventArgs e)
        {
            e.Show();
        }
    }
}