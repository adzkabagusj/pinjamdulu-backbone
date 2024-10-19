using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Linq;
using pinjamdulu_backbone.Models;
using pinjamdulu_backbone.Services;
using pinjamdulu_backbone.Helpers;
using Stripe;
using pinjamdulu_backbone.Views;

namespace pinjamdulu_backbone.ViewModels
{
    public class StripePaymentViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NavigationService _navigationService;

        private User _currentUser;
        private Guid _gadgetId;
        private DateTime _rentEndDate;
        private decimal _totalPrice;

        public decimal TotalPrice => _totalPrice;

        public ICommand PayCommand { get; }
        public ICommand goBack { get; }

        public StripePaymentViewModel(NavigationService navigationService, User user, PaymentParameters rentData)
        {
            _databaseService = new DatabaseService();
            _navigationService = navigationService;

            PayCommand = new RelayCommand(async () => await ProcessPaymentAsync());
            goBack = new RelayCommand(() => _navigationService.GoBack());

            _currentUser = user;
            _gadgetId = rentData.GadgetId;
            _rentEndDate = rentData.RentEndDate;
            _totalPrice = rentData.TotalPrice;

            // Configure Stripe with secret key
            StripeConfiguration.ApiKey = ConfigurationHelper.GetStripeSecretKey();
        }

        private string GetSelectedPaymentMethod()
        {
            var selectedIndex = System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var window = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
                if (window != null)
                {
                    var comboBox = window.FindName("TestScenarioComboBox") as ComboBox;
                    return comboBox?.SelectedIndex ?? 0;
                }
                return 0;
            });

            switch (selectedIndex)
            {
                case 1:
                    return "pm_card_visa_chargeDeclined";
                case 2:
                    return "pm_card_visa_authenticationRequired";
                default:
                    return "pm_card_visa";
            }
        }

        private async Task ProcessPaymentAsync()
        {
            try
            {
                // Convert price to IDR integer (Stripe expects amounts without decimals for IDR)
                // For example: 1000000.50 IDR should be 1000000
                var amountInIDR = (long)Math.Round(_totalPrice*10);

                var paymentIntentOptions = new PaymentIntentCreateOptions
                {
                    Amount = amountInIDR,  // No need to multiply by 100 for IDR
                    Currency = "idr",      // Set currency to IDR
                    PaymentMethod = GetSelectedPaymentMethod(),
                    Confirm = true,
                    PaymentMethodTypes = new List<string> { "card" }
                };

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

                if (paymentIntent.Status == "succeeded")
                {
                    // Create booking
                    var booking = new Booking
                    {
                        BookingId = Guid.NewGuid(),
                        GadgetId = _gadgetId,
                        BorrowerId = _currentUser.UserId,
                        LenderId = await _databaseService.GetGadgetOwnerId(_gadgetId),
                        BookingDate = DateTime.Now,
                        RentalStartDate = DateTime.Now,
                        RentalEndDate = _rentEndDate
                    };

                    await _databaseService.CreateBooking(booking);

                    // Create payment record
                    var payment = new Payment
                    {
                        PaymentId = Guid.NewGuid(),
                        BookingId = booking.BookingId,
                        Amount = _totalPrice,
                        PaymentMethod = "Stripe",
                        PaymentStatus = true,
                        TransactionDate = DateTime.Now
                    };

                    await _databaseService.CreatePayment(payment);

                    MessageBox.Show($"Payment successful, gadget {await _databaseService.GetGadgetTitle(_gadgetId)} is rented successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    _navigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("Payment failed. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (StripeException e)
            {
                MessageBox.Show($"Payment failed: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occurred: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}