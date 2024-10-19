using pinjamdulu_backbone.Models;
using pinjamdulu_backbone.ViewModels;
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

namespace pinjamdulu_backbone.Views
{
    /// <summary>
    /// Interaction logic for StripePayment.xaml
    /// </summary>
    public partial class StripePayment : Page
    {
        public StripePayment(User user, PaymentParameters rentData)
        {
            DataContext = new StripePaymentViewModel(MainWindow.NavigationService, user, rentData);
            InitializeComponent();
        }
    }
}
