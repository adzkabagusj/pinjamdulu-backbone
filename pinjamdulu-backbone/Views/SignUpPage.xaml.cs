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
using pinjamdulu_backbone.ViewModels;
using System.Windows.Controls;

namespace pinjamdulu_backbone.Views
{
    /// <summary>
    /// Interaction logic for SignUpPage.xaml
    /// </summary>
    public partial class SignUpPage : Page
    {
        private SignUpViewModel _viewModel;

        public SignUpPage()
        {
            InitializeComponent();
            _viewModel = new SignUpViewModel(MainWindow.NavigationService);
            DataContext = _viewModel;

            // code behind: again, idk why this just isn't included in the ViewModel.
            PasswordBox.PasswordChanged += (s, e) =>
            {
                _viewModel.Password = PasswordBox.Password;
            };

            ConfirmPasswordBox.PasswordChanged += (s, e) =>
            {
                _viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            };
        }
    }
}
