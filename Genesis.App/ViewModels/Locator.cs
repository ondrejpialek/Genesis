using GalaSoft.MvvmLight.Ioc;
using Genesis.Excel;
using Microsoft.Practices.ServiceLocation;

namespace Genesis.ViewModels
{
    public class Locator
    {
        static Locator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<DataViewModel>();
            SimpleIoc.Default.Register<MiceViewModel>();
            SimpleIoc.Default.Register<ImportViewModel>();
            SimpleIoc.Default.Register<AnalyzeViewModel>();
            SimpleIoc.Default.Register<FstViewModel>();
            SimpleIoc.Default.Register<IExcelService, ExcelService>();
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

        public FstViewModel Fst
        {
            get
            {
                return ServiceLocator.Current.GetInstance<FstViewModel>();
            }
        }

        public MiceViewModel Mice
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MiceViewModel>();
            }
        }

        public void Cleanup()
        {
            Import.Cleanup();
        }
    }
}