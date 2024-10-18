﻿using pinjamdulu_backbone.Helpers;
using pinjamdulu_backbone.Models;
using pinjamdulu_backbone.Services;
using pinjamdulu_backbone.Views;
using System.Windows.Input;

namespace pinjamdulu_backbone.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;

        public ICommand SignOutCommand { get; }
        public ICommand NavigateToListingCommand {  get; }

        public HomeViewModel(NavigationService navigationService, User user)
        {
            _navigationService = navigationService;
            SignOutCommand = new RelayCommand(SignOut);
            NavigateToListingCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ListingPage), user));
        }

        private void SignOut()
        {
            _navigationService.NavigateTo(typeof(LoginPage));
        }
    }
}