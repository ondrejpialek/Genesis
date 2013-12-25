using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Genesis.Excel;
using Microsoft.Win32;

namespace Genesis.ViewModels
{

    public class ImportSectionViewModel : SectionViewModel
    {
        public enum ImportType { Localities, Data };
        private static IEnumerable<ImportType> importTypes = new ImportType[] { ImportType.Localities, ImportType.Data };
        public IEnumerable<ImportType> ImportTypes { get { return importTypes; } }

        private GenesisContext context;
        private IExcelService excelService;

        public ObservableCollection<string> Sheets { get; private set; }
        public ObservableCollection<ColumnViewModel> Columns { get; protected set; }
        public ObservableCollection<ICellReader> Fields { get; protected set; }

        private ObservableCollection<Gene> genes;
        public ObservableCollection<Gene> Genes
        {
            get
            {
                if (genes == null)
                {
                    if (context != null) {
                        context.Genes.Include(g => g.Alleles).Include(g => g.Chromosome).Load();
                        genes = context.Genes.Local;
                    }
                }

                return genes;
            }
        }

        private readonly IEnumerable<ICellReader<Locality>> localityFields = new List<ICellReader<Locality>> { new CodeColumn(), new LatitudeColumn(), new LongitudeColumn(), new LocalityNameColumn() };
        private readonly ICollection<ICellReader<Mouse>> miceFields = new List<ICellReader<Mouse>> { new PINColumn(), new TraitColumn(), new SexColumn()};

        public ImportSectionViewModel(IExcelService excelService)
        {
            DisplayName = "Import";
            Order = 50;

            context = new GenesisContext();
            this.excelService = excelService;

            Sheets = new ObservableCollection<string>();
            Columns = new ObservableCollection<ColumnViewModel>();
        }

        protected override void OnActivate()
        {
            if (context != null)
            {
                context.Dispose();
                context = new GenesisContext();
            }

            NotifyOfPropertyChange(() => Genes);

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
                Fields.Add(new MouseLocalityColumn(context.Localities.Include(l => l.Mice).ToList()));
            }

            NotifyOfPropertyChange(() => Fields);
            ApplyConventions();
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
                this.Set(() => SelectedImport, ref selectedImport, value);
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
                this.Set(() => Filename, ref filename, value);
            }
        }

        private IExcelFile excelFile;

        public void Browse()
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
                this.Set(() => Sheet, ref sheet, value);
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
                Columns.Add(new ColumnViewModel(letter, column, Genes));
            }

            ApplyConventions();
        }

        private void ApplyConventions()
        {
            foreach (var field in Fields)
            {
                foreach (var column in Columns)
                {
                    if (column.Name.Equals(field.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        column.Column = field;
                        break;
                    }
                }
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
                this.Set(() => Progress, ref progress, value);
            }
        }

        public void Import()
        {
            if (SelectedImport == ImportType.Localities)
            {
                DoImportLocalities();
            }
            else
            {
                DoImportData();
            }
        }

        private void DoImportData()
        {
            var importArgs = new ImportArgs<Mouse>();
            importArgs.Filename = filename;
            importArgs.WorkSheetName = sheet;
            importArgs.Columns.Clear();
            foreach (var columnViewModel in Columns.Where(c => c.Column != null))
            {
                columnViewModel.Column.Column = columnViewModel.Key;
                importArgs.Columns.Add((ICellReader<Mouse>)columnViewModel.Column);
            }

            var excelImport = new ExcelImport<Mouse>(context, mice => mice.Include("Alleles.Allele").Include("Locality"), c => c.Localities.Include("Mice").Load());
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
            var importArgs = new ImportArgs<Locality>();
            importArgs.Filename = filename;
            importArgs.WorkSheetName = sheet;
            importArgs.Columns.Clear();
            foreach (var column in Columns.Where(c => c.Column != null))
            {
                column.Column.Column = column.Key;
                importArgs.Columns.Add((ICellReader<Locality>)column.Column);
            }

            var excelImport = new ExcelImport<Locality>(context, locality => locality, _ => { });
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

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            if (close)
            {
                if (excelFile != null)
                {
                    excelFile.Dispose();
                    excelFile = null;
                } 
            }
        }
    }
}