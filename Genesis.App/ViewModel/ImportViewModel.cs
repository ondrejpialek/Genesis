using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Genesis.Excel;
using Microsoft.Win32;
using System.Linq;
using System;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;

namespace Genesis.ViewModel
{

    public class ImportViewModel : ViewModelBase
    {
        public enum ImportType { Localities, Data };
        private static IEnumerable<ImportType> importTypes = new ImportType[] { ImportType.Localities, ImportType.Data };
        public IEnumerable<ImportType> ImportTypes { get { return importTypes; } }

        private GenesisContext context;
        private IExcelService excelService;

        public ObservableCollection<string> Sheets { get; private set; }
        public ObservableCollection<ColumnViewModel> Columns { get; protected set; }
        public ObservableCollection<ICellReader> Fields { get; protected set; }
        
        public ObservableCollection<Gene> Genes
        {
            get
            {
                if (context != null) {
                    context.Genes.ToList();
                    return context.Genes.Local;
                }
                return null;
            }
        }

        private readonly IEnumerable<ICellReader<Locality>> localityFields = new List<ICellReader<Locality>> { new CodeColumn(), new LatitudeColumn(), new LongitudeColumn(), new LocalityNameColumn() };
        private readonly ICollection<ICellReader<Mouse>> miceFields = new List<ICellReader<Mouse>> { new PINColumn(), new TraitColumn(), new SexColumn()};

        public ImportViewModel(IExcelService excelService)
        {
            context = new GenesisContext();
            this.excelService = excelService;

            Sheets = new ObservableCollection<string>();
            Columns = new ObservableCollection<ColumnViewModel>();

            Messenger.Default.Register<GenericMessage<Message>>(this, (m) =>
            {
                if (m.Target != this)

                switch (m.Content)
                {
                    case Message.Refresh:
                        Refresh();
                        break;
                }
            });

            Refresh();
        }

        private void Refresh()
        {
            if (context != null)
            {
                context.Dispose();
                context = new GenesisContext();
            }

            RaisePropertyChanged(() => Genes);

            ReloadFields();
        }

        private void ReloadFields()
        {
            if (SelectedImport == ImportType.Localities)
            {
                Fields = new ObservableCollection<ICellReader>(localityFields);
            }
            else
            {
                Fields = new ObservableCollection<ICellReader>(miceFields);
                Fields.Add(new MouseLocalityColumn(context.Localities.ToList()));
            }
            RaisePropertyChanged(() => Fields);
        }

        private ImportType selectedImport = ImportType.Localities;
        public ImportType SelectedImport
        {
            get
            {
                return selectedImport;
            }
            set
            {
                Set(() => SelectedImport, ref selectedImport, value);
                ReloadFields();
            }
        }

        private string filename = null;
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                Set(() => Filename, ref filename, value);
            }
        }

        private IExcelFile excelFile;

        private RelayCommand browse;
        public RelayCommand Browse
        {
            get
            {
                if (browse == null)
                {
                    browse = new RelayCommand(() =>
                    {
                        var dialog = new OpenFileDialog();
                        dialog.FileName = Filename;
                        dialog.Filter = null;
                        bool result = dialog.ShowDialog() ?? false;
                        if (result)
                        {
                            Filename = dialog.FileName;
                            if (excelFile != null)
                            {
                                excelFile.Dispose();
                                excelFile = null;
                            }
                            excelFile = excelService.Open(filename);
                            Sheets.Clear();
                            var worksheets = excelFile.Worksheets;
                            foreach (var sheet in worksheets)
                            {
                                Sheets.Add(sheet.Name);
                            }
                        }
                    });
                }

                return browse;
            }
        }

        IExcelWorksheet worksheet;

        private string sheet = null;
        public string Sheet
        {
            get
            {
                return sheet;
            }
            set
            {
                Set(() => Sheet, ref sheet, value);
                int index = Sheets.IndexOf(value);
                if ((excelFile != null) && index > -1)
                {
                    var worksheets = excelFile.Worksheets;
                    worksheet = worksheets[Sheets.IndexOf(value)];
                    LoadColumns();
                }
            }
        }

        private void LoadColumns()
        {
            var alphabet = Alphabet.GetAlphabet(worksheet.GetColCount());

            Columns.Clear();
            foreach (var letter in alphabet)
            {
                var column = worksheet.GetCellValueAsString(letter + "1");
                if (string.IsNullOrEmpty(column))
                    continue;
                Columns.Add(new ColumnViewModel(letter, column));
            }
        }

        private double progress = 0;
        public double Progress
        {
            get
            {
                return progress;
            }
            set
            {
                Set(() => Progress, ref progress, value);
            }
        }

        private RelayCommand import;
        public RelayCommand Import
        {
            get
            {
                if (import == null)
                {
                    import = new RelayCommand(() =>
                    {
                        if (SelectedImport == ImportType.Localities)
                        {
                            DoImportLocalities();
                        }
                        else
                        {
                            DoImportData();
                        }
                    });
                }

                return import;
            }
        }

        private void DoImportData()
        {
            ImportArgs<Mouse> importArgs = new ImportArgs<Mouse>();
            importArgs.Filename = filename;
            importArgs.WorkSheetName = sheet;
            importArgs.Columns.Clear();
            foreach (var columnViewModel in Columns.Where(c => c.Column != null))
            {
                columnViewModel.Column.Column = columnViewModel.Key;
                importArgs.Columns.Add((ICellReader<Mouse>)columnViewModel.Column);
            }

            var excelImport = new ExcelImport<Mouse>(context);
            excelImport.ProgressChanged += new EventHandler((o, e) =>
            {
                Progress = excelImport.Progress * 100;
            });

            excelImport.Finished += new EventHandler((o, e) =>
            {
                excelImport.Save();
                MessageBox.Show("Import complete");
            });
            excelImport.Cancelled += (o, e) =>
            {
                MessageBox.Show("Import cancelled.");
            };

            excelImport.Error += (o, e) =>
            {
                MessageBox.Show("ERROR!\n\n" + e.Error);
            };

            excelImport.Start(importArgs);
        }

        private void DoImportLocalities()
        {
            ImportArgs<Locality> importArgs = new ImportArgs<Locality>();
            importArgs.Filename = filename;
            importArgs.WorkSheetName = sheet;
            importArgs.Columns.Clear();
            foreach (var column in Columns.Where(c => c.Column != null))
            {
                column.Column.Column = column.Key;
                importArgs.Columns.Add((ICellReader<Locality>)column.Column);
            }

            var excelImport = new ExcelImport<Locality>(context);
            excelImport.ProgressChanged += new EventHandler((o, e) =>
            {
                Progress = excelImport.Progress * 100;
            });

            excelImport.Cancelled += (o, e) =>
            {
                MessageBox.Show("Import cancelled.");
            };

            excelImport.Error += (o, e) =>
            {
                MessageBox.Show("ERROR!\n\n" + e.Error);
            };

            excelImport.Finished += new EventHandler((o, e) =>
            {
                excelImport.Save();
                MessageBox.Show("Import complete");
            });
            excelImport.Start(importArgs);
        }

        public override void Cleanup()
        {
            if (excelFile != null)
            {
                excelFile.Dispose();
                excelFile = null;
            }
        }
    }
}