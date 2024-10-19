using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using pinjamdulu_backbone.Models;
using pinjamdulu_backbone.Services;
using pinjamdulu_backbone.Helpers;
using pinjamdulu_backbone.Views;

namespace pinjamdulu_backbone.ViewModels
{
    public class NavigationParameters
    {
        public User User { get; set; }
        public Gadget Gadget { get; set; }

        public NavigationParameters(User user, Gadget gadget)
        {
            User = user;
            Gadget = gadget;
        }
    }
    public class HomeViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private ObservableCollection<Gadget> _gadgets;
        private bool _isLoading;

        public ObservableCollection<Gadget> Gadgets
        {
            get => _gadgets;
            set
            {
                _gadgets = value;
                OnPropertyChanged(nameof(Gadgets));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand SignOutCommand { get; }
        public ICommand NavigateToListingCommand { get; }
        public ICommand GadgetSelectedCommand { get; }

        public HomeViewModel(NavigationService navigationService, User user)
        {
            _navigationService = navigationService;
            _currentUser = user;
            _databaseService = new DatabaseService();
            Gadgets = new ObservableCollection<Gadget>();

            // Initialize commands
            SignOutCommand = new RelayCommand(SignOut);
            NavigateToListingCommand = new RelayCommand(NavigateToListing);
            GadgetSelectedCommand = new RelayCommand<Gadget>(OnGadgetSelected);

            // Load gadgets when view model is created
            LoadGadgetsAsync();
        }

        private async void LoadGadgetsAsync()
        {
            try
            {
                IsLoading = true;
                var gadgets = await _databaseService.GetRandomGadgets();
                Gadgets.Clear();
                foreach (var gadget in gadgets)
                {
                    Gadgets.Add(gadget);
                }
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                System.Diagnostics.Debug.WriteLine($"Error loading gadgets: {ex.Message}");
                // Optionally show error message to user
                // System.Windows.MessageBox.Show($"Error loading gadgets: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnGadgetSelected(Gadget gadget)
        {
            if (gadget != null)
            {
                var navigationParams = new NavigationParameters(_currentUser, gadget);
                _navigationService.NavigateTo(typeof(GadgetDetail), navigationParams.User, navigationParams.Gadget);
            }
        }

        private void NavigateToListing()
        {
            _navigationService.NavigateTo(typeof(ListingPage), _currentUser);
        }

        private void SignOut()
        {
            _navigationService.NavigateTo(typeof(LoginPage));
        }
    }
}