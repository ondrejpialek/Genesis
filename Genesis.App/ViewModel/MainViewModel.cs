using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using Genesis.Views;
using GalaSoft.MvvmLight.Command;

namespace Genesis.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        private UserControl activeView;
        public UserControl ActiveView
        {
            get
            {
                return activeView;
            }
            set
            {
                Set(() => ActiveView, ref activeView, value);
            }
        }

        private RelayCommand displayData;
        public RelayCommand DisplayData
        {
            get
            {
                return displayData ?? (displayData = new RelayCommand(
                    () =>
                    {
                        ActiveView = new Data();
                    }));
            }
        }

        private RelayCommand displayMice;
        public RelayCommand DisplayMice
        {
            get
            {
                return displayMice ?? (displayMice = new RelayCommand(
                    () =>
                    {
                        ActiveView = new Mice();
                    }));
            }
        }

        private RelayCommand displayAnalysis;
        public RelayCommand DisplayAnalysis
        {
            get
            {
                return displayAnalysis ?? (displayAnalysis = new RelayCommand(
                    () =>
                    {
                        ActiveView = new Analyze();
                    }));
            }
        }

        private RelayCommand displaySettings;
        public RelayCommand DisplaySettings
        {
            get
            {
                return displaySettings ?? (displaySettings = new RelayCommand(
                    () =>
                    {
                        ActiveView = new Settings();
                    }));
            }
        }

        private RelayCommand displayImport;
        public RelayCommand DisplayImport
        {
            get
            {
                return displayImport ?? (displayImport = new RelayCommand(
                    () =>
                    {
                        ActiveView = new Import();
                    }));
            }
        }

        private RelayCommand displayMap;
        public RelayCommand DisplayMap
        {
            get
            {
                return displayMap ?? (displayMap = new RelayCommand(
                    () =>
                    {
                        ActiveView = new Map();
                    }));
            }
        }

        public MainViewModel()
        {
            ActiveView = new Data();
        }
    }
}