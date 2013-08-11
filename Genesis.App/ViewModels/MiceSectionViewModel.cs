using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Genesis.Views;

namespace Genesis.ViewModels
{
    public class MiceSectionViewModel : Screen, ISectionViewModel
    {
        private GenesisContext context;

        public MiceSectionViewModel()
        {
            DisplayName = "Data";
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (context != null)
                context.Dispose();
            context = new GenesisContext();

            NotifyOfPropertyChange(() => Mice);
            NotifyOfPropertyChange(() => Genes);
        }

        public ObservableCollection<Mouse> Mice
        {
            get
            {
                if (context != null)
                {
                    context.Mice
                        .OrderBy(m => m.Locality == null ? string.Empty : m.Locality.Name)
                        .ThenBy(m => m.Sex).ThenBy(m => m.Name).Load();
                    return context.Mice.Local;
                }
                return null;
            }
        }

        DataGrid grid;
        private RelayCommand<DataGrid> dataGridLoaded;
        public RelayCommand<DataGrid> DataGridLoaded
        {
            get
            {
                return dataGridLoaded ?? (dataGridLoaded = new RelayCommand<DataGrid>(g =>
                {
                    grid = g;
                }));
            }
        }

        public ObservableCollection<Gene> Genes
        {
            get
            {
                if (context != null)
                {
                    context.Genes.Load();
                    return context.Genes.Local;
                }
                return null;
            }
        }

        private MouseToAllelesConverter mouseToAllelesConverter;

        private RelayCommand<Gene> addGeneColumn;
        public RelayCommand<Gene> AddColumn
        {
            get
            {
                return addGeneColumn ?? (addGeneColumn = new RelayCommand<Gene>(gene =>
                {
                    if (grid != null)
                    {
                        var col = new DataGridTextColumn() { Header = gene.Name };
                        grid.Columns.Add(col);
                        var binding = new Binding();
                        binding.Converter = mouseToAllelesConverter ?? (mouseToAllelesConverter = new MouseToAllelesConverter());
                        binding.ConverterParameter = gene;
                        col.Binding = binding;
                    }
                }));
            }
        }

        private RelayCommand<Gene> removeGeneColumn;
        public RelayCommand<Gene> RemoveColumn
        {
            get
            {
                return removeGeneColumn ?? (removeGeneColumn = new RelayCommand<Gene>(gene =>
                {
                    if (grid != null)
                    {
                        var col = grid.Columns.Where(c => string.Equals(c.Header, gene.Name)).FirstOrDefault();
                        if (col != null)
                        {
                            grid.Columns.Remove(col);
                        }
                    }
                }));
            }
        }
    }
}
