using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using pinjamdulu_backbone.Models;
using pinjamdulu_backbone.ViewModels;
using System.Windows.Controls;

namespace pinjamdulu_backbone.Views
{
    public partial class ListingPage : Page
    {
        public ListingPage(User user)
        {
            InitializeComponent();
            DataContext = new ListingViewModel(MainWindow.NavigationService, user);
        }
    }
}
