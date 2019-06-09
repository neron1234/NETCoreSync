﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Microsoft.EntityFrameworkCore;
using NETCoreSyncMobileSample.Models;
using NETCoreSyncMobileSample.Services;
using NETCoreSync;

namespace NETCoreSyncMobileSample.ViewModels
{
    public class EmployeeItemViewModel : BaseViewModel
    {
        private readonly INavigation navigation;
        private readonly DatabaseService databaseService;

        private List<bool> isActiveItems;
        public List<bool> IsActiveItems
        {
            get { return isActiveItems; }
            set { SetProperty(ref isActiveItems, value); }
        }

        private List<Department> departmentItems;
        public List<Department> DepartmentItems
        {
            get { return departmentItems; }
            set { SetProperty(ref departmentItems, value); }
        }

        public EmployeeItemViewModel(INavigation navigation, DatabaseService databaseService)
        {
            this.navigation = navigation;
            this.databaseService = databaseService;

            IsActiveItems = new List<bool>() { true, false };
            DepartmentItems = new List<Department>();
            DepartmentItems.Add(new Department() { Id = Guid.Empty.ToString(), Name = "[None]" });
            using (var databaseContext = databaseService.GetDatabaseContext())
            {
                DepartmentItems.AddRange(databaseService.GetDepartments(databaseContext).AsNoTracking().ToList());
            }
        }

        private bool isNewData;
        public bool IsNewData
        {
            get { return isNewData; }
            set { SetProperty(ref isNewData, value); }
        }

        private Employee data;
        public Employee Data
        {
            get { return data; }
            set { SetProperty(ref data, value); }
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            Data = (Employee)initData;
            if (Data == null)
            {
                Title = $"Add {nameof(Employee)}";
                isNewData = true;
                Data = new Employee();
                Data.Id = Guid.NewGuid().ToString();
                Data.Birthday = DateTime.Now;
            }
            else
            {
                Title = $"Edit {nameof(Employee)}";
            }
            Data.Department = DepartmentItems.Where(w => w.Id == Guid.Empty.ToString()).First();
            if (!string.IsNullOrEmpty(Data.DepartmentId))
            {
                Data.Department = DepartmentItems.Where(w => w.Id == Data.DepartmentId).FirstOrDefault();
            }
        }

        public ICommand SaveCommand => new Command(async () =>
        {
            string departmentId = null;
            if (Data.Department != null) departmentId = Data.Department.Id;
            if (departmentId == Guid.Empty.ToString()) departmentId = null;
            Data.Department = null;
            Data.DepartmentId = departmentId;

            using (var databaseContext = databaseService.GetDatabaseContext())
            {
                Data.LastUpdated = SyncEngine.GetNowTicks();
                if (IsNewData)
                {
                    databaseContext.Add(Data);
                }
                else
                {
                    databaseContext.Update(Data);
                }
                await databaseContext.SaveChangesAsync();
            }

            await navigation.PopAsync();
        });

        public ICommand DeleteCommand => new Command(async () =>
        {
            if (IsNewData) return;

            using (var databaseContext = databaseService.GetDatabaseContext())
            {
                Data.Deleted = SyncEngine.GetNowTicks();
                databaseContext.Update(Data);
                //databaseContext.Remove(Data);
                await databaseContext.SaveChangesAsync();
            }

            await navigation.PopAsync();
        });
    }
}
