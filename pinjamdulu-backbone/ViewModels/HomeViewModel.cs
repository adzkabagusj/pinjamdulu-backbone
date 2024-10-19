using pinjamdulu_backbone.Helpers;
using pinjamdulu_backbone.Models;
using pinjamdulu_backbone.Services;
using pinjamdulu_backbone.Views;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace pinjamdulu_backbone.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly DatabaseService _databaseService;
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
        public ICommand NavigateToListingCommand {  get; }
        public ICommand GadgetSelectedCommand { get; }

        public HomeViewModel(NavigationService navigationService, User user)
        {
            _navigationService = navigationService;
            _databaseService = new DatabaseService();
            Gadgets = new ObservableCollection<Gadget>();

            SignOutCommand = new RelayCommand(SignOut);
            NavigateToListingCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ListingPage), user));
            GadgetSelectedCommand = new RelayCommand<Gadget>(OnGadgetSelected);
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
                // For now, we'll just show the title as requested
                System.Windows.MessageBox.Show($"Selected Gadget: {gadget.Title}");
                // Later, you can implement: _navigationService.NavigateTo("GadgetDetailPage", gadget);
            }
        }

        private void SignOut()
        {
            _navigationService.NavigateTo(typeof(LoginPage));
        }
    }
}