using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Genesis.Views;

namespace Genesis.ViewModels
{
    public class MiceViewModel : ViewModelBase
    {
        private GenesisContext context;

        public MiceViewModel()
        {
            Messenger.Default.Register<GenericMessage<Message>>(this, m =>
            {
                if (m.Target != this)
                    return;

                switch (m.Content)
                {
                    case Message.Refresh:
                        Refresh();
                        break;
                }
            });
        }

        private void Refresh()
        {
            if (context != null)
                context.Dispose();
            context = new GenesisContext();

            RaisePropertyChanged(() => Mice);
            RaisePropertyChanged(() => Genes);
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
