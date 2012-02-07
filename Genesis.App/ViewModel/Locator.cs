using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Ioc;
using Genesis.App.Data;
using Genesis.Excel;
using Microsoft.Practices.ServiceLocation;

namespace Genesis.ViewModel
{
    public class Locator
    {

        static Locator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<DataViewModel>();
            SimpleIoc.Default.Register<ImportViewModel>();
            SimpleIoc.Default.Register<AnalyzeViewModel>();
            SimpleIoc.Default.Register<IExcelService, ExcelService>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public SettingsViewModel Settings
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsViewModel>();
            }
        }

        public DataViewModel Data
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DataViewModel>();
            }
        }

        public AnalyzeViewModel Analyze
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AnalyzeViewModel>();
            }
        }

        public ImportViewModel Import
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ImportViewModel>();
            }
        }

    }
}
