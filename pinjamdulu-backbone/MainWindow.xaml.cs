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
using pinjamdulu_backbone.Views;
using pinjamdulu_backbone.Services;
using NavigationService = pinjamdulu_backbone.Services.NavigationService;

namespace pinjamdulu_backbone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static NavigationService NavigationService { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            NavigationService = new NavigationService(MainFrame);
            NavigationService.NavigateTo(typeof(LoginPage)); // first page that shows up when application starts is Login Page.
        }
    }
}