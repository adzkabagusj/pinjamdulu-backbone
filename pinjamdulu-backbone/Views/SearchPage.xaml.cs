using pinjamdulu_backbone.Models;
using pinjamdulu_backbone.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        public SearchPage(SearchParameters searchparams)
        {
            DataContext = new SearchPageViewModel(MainWindow.NavigationService, searchparams);
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void RatingValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers and one decimal point
            Regex regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            string newText = ((TextBox)sender).Text + e.Text;

            if (!regex.IsMatch(newText))
            {
                e.Handled = true;
                return;
            }

            // Check if the value is within 0-5 range
            if (float.TryParse(newText, out float value))
            {
                e.Handled = value < 0 || value > 5;
            }
        }

        private void ConditionValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            // Check if the value is within 1-10 range
            string newText = ((TextBox)sender).Text + e.Text;
            if (int.TryParse(newText, out int value))
            {
                e.Handled = value < 1 || value > 10;
            }
        }

        private void OnCategoryChecked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked == true)
            {
                var viewModel = (SearchPageViewModel)DataContext;
                viewModel.SelectedCategory = radioButton.Tag?.ToString();
            }
        }
    }
}
