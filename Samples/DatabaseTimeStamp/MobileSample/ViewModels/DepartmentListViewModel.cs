﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using MobileSample.Models;
using MobileSample.Services;
using MobileSample.Views;

namespace MobileSample.ViewModels
{
    public class DepartmentListViewModel : BaseViewModel
    {
        private readonly INavigation navigation;
        private readonly DatabaseService databaseService;

        public DepartmentListViewModel(INavigation navigation, DatabaseService databaseService)
        {
            this.navigation = navigation;
            this.databaseService = databaseService;
        }

        private IEnumerable<Department> items;
        public IEnumerable<Department> Items
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        private Department selectedItem;
        public Department SelectedItem
        {
            get { return selectedItem; }
            set
            {
                SetProperty(ref selectedItem, value);
                if (selectedItem == null) return;
                DepartmentItemPage page = new DepartmentItemPage(selectedItem.Id);
                navigation.PushAsync(page);
                SelectedItem = null;
            }
        }

        protected override void ViewAppearing(object sender, EventArgs e)
        {
            base.ViewAppearing(sender, e);
            Title = MainMenuItem.GetMenus().Where(w => w.Id == MenuItemType.DepartmentList).First().Title;
            Items = databaseService.GetDepartments();
        }

        public ICommand AddCommand => new Command(async () => 
        {
            DepartmentItemPage page = new DepartmentItemPage(null);
            await navigation.PushAsync(page);
        });
    }
}
