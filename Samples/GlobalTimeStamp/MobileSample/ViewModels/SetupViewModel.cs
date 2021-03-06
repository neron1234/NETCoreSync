﻿using System;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using MobileSample.Models;
using MobileSample.Services;

namespace MobileSample.ViewModels
{
    public class SetupViewModel : BaseViewModel
    {
        private readonly INavigation navigation;
        private readonly DatabaseService databaseService;

        public SetupViewModel(INavigation navigation, DatabaseService databaseService)
        {
            this.navigation = navigation;
            this.databaseService = databaseService;
            Title = MainMenuItem.GetMenus().Where(w => w.Id == MenuItemType.Setup).First().Title;

            SynchronizationId = databaseService.GetSynchronizationId();
            if (!string.IsNullOrEmpty(SynchronizationId)) IsSynchronizationIdSet = true;
        }

        private string synchronizationId;
        public string SynchronizationId
        {
            get { return synchronizationId; }
            set { SetProperty(ref synchronizationId, value); }
        }

        private bool isSynchronizationIdSet;
        public bool IsSynchronizationIdSet
        {
            get { return isSynchronizationIdSet; }
            set { SetProperty(ref isSynchronizationIdSet, value); }
        }

        public ICommand SetSynchronizationIdCommand => new Command(async () =>
        {
            if (string.IsNullOrEmpty(SynchronizationId))
            {
                await Application.Current.MainPage.DisplayAlert("Empty Synchronization ID", "Please specify Synchronization ID", "OK");
                return;
            }

            if (IsSynchronizationIdSet)
            {
                bool isAccept = await Application.Current.MainPage.DisplayAlert("Reset Database", "Changing Synchronization ID will reset the database. Continue?", "Yes", "No");
                if (!isAccept) return;
            }

            using (var databaseContext = databaseService.GetDatabaseContext())
            {
                databaseContext.Employees.RemoveRange(databaseContext.Employees);
                databaseContext.Departments.RemoveRange(databaseContext.Departments);
                databaseContext.Configurations.RemoveRange(databaseContext.Configurations);
                await databaseContext.SaveChangesAsync();

                databaseService.SetSynchronizationId(SynchronizationId);
            }

            await Application.Current.MainPage.DisplayAlert("Success", "Synchronization ID is successfully set", "OK");
            Application.Current.MainPage = new Views.MainPage();
        });

        public ICommand DumpLogCommand => new Command(async () =>
        {
            databaseService.DumpLog();

            await Application.Current.MainPage.DisplayAlert("Success", "Database Contents is dumped to Debug Log", "OK");
        });

        public ICommand DateTimeCommand => new Command(async () =>
        {
            string text = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
            await Application.Current.MainPage.DisplayAlert("Date Time", $"The system's Date and Time is: {text}. Make sure this is correct and accurate according to the real world clock time.", "OK");
        });
    }
}
