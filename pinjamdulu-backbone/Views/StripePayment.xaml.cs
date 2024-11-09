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
    /// Interaction logic for StripePayment.xaml
    /// </summary>
    public partial class StripePayment : Page
    {
        public StripePayment(User user, PaymentParameters rentData)
        {
            DataContext = new StripePaymentViewModel(MainWindow.NavigationService, user, rentData);
            InitializeComponent();
        }

        private void NumberOnlyPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void CardNumberTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Replace(" ", "");
            if (text.Length > 0)
            {
                // Format card number in groups of 4
                text = string.Join(" ", Regex.Matches(text, ".{1,4}").Cast<Match>().Select(m => m.Value));
                if (text != textBox.Text)
                {
                    textBox.Text = text;
                    textBox.CaretIndex = text.Length;
                }
            }
        }

        private void ExpiryTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Replace("/", "");
            if (text.Length > 0)
            {
                // Format expiry as MM/YY
                if (text.Length >= 2)
                {
                    text = text.Insert(2, "/");
                }
                if (text != textBox.Text)
                {
                    textBox.Text = text;
                    textBox.CaretIndex = text.Length;
                }
            }
        }
    }
}
