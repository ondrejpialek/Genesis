using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Genesis.Excel;
using Microsoft.Win32;

namespace Genesis.ViewModels.Import
{
    public class ImportSectionViewModel : SectionViewModel
    {
        public enum ImportType
        {
            Localities,
            Data
        }

        private static readonly IEnumerable<ImportType> importTypes = new[] {ImportType.Localities, ImportType.Data};

        private GenesisContext context;

        private IExcelFile excelFile;

        private readonly IExcelService excelService;

        private string filename;

        private double progress;

        private ImportType selectedImportType = ImportType.Localities;

        private string sheet;

        private IExcelWorksheet worksheet;

        public ImportSectionViewModel(IExcelService excelService)
        {
            DisplayName = "Import";
            Order = 50;

            context = new GenesisContext();
            this.excelService = excelService;

            Sheets = new ObservableCollection<string>();
            Columns = new ObservableCollection<ColumnViewModel>();
        }

        public IEnumerable<ImportType> ImportTypes
        {
            get { return importTypes; }
        }

        public ObservableCollection<string> Sheets { get; }
        public ObservableCollection<ColumnViewModel> Columns { get; protected set; }

        public ImportType SelectedImportType
        {
            get { return selectedImportType; }
            set
            {
                this.Set(() => SelectedImportType, ref selectedImportType, value);
                LoadColumns();
            }
        }

        public string Filename
        {
            get { return filename; }
            set { this.Set(() => Filename, ref filename, value); }
        }

        public string Sheet
        {
            get { return sheet; }
            set
            {
                this.Set(() => Sheet, ref sheet, value);
                var index = Sheets.IndexOf(value);
                if ((excelFile != null) && index > -1)
                {
                    var worksheets = excelFile.Worksheets;
                    worksheet = worksheets[Sheets.IndexOf(value)];
                    LoadColumns();
                }
            }
        }

        public double Progress
        {
            get { return progress; }
            set { this.Set(() => Progress, ref progress, value); }
        }

        protected override void OnActivate()
        {
            if (context != null)
            {
                context.Dispose();
                context = new GenesisContext();
                context.Traits.Load();
            }

            LoadColumns();
        }

        public void Browse()
        {
            var dialog = new OpenFileDialog();
            dialog.FileName = Filename;
            dialog.Filter = null;
            var result = dialog.ShowDialog() ?? false;
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

        /// <summary>
        /// Recreates the rows in the editor based on the columns of the current sheet.
        /// The actual view model for the row will match the currently selected import type.
        /// </summary>
        private void LoadColumns()
        {
            Columns.Clear();

            if (worksheet == null)
                return;

            var columnNames = worksheet.GetColumns();
            for(var i = 0; i < columnNames.Length; i++)
            {
                var name = columnNames[i];

                if (string.IsNullOrEmpty(name))
                    continue;

                if (SelectedImportType == ImportType.Localities)
                {
                    Columns.Add(new LocalitySheetColumnViewModel(i, name));
                }
                else
                {
                    Columns.Add(new MiceSheetColumnViewModel(i, name, context));
                }
            }

            NotifyOfPropertyChange(() => Columns);
        }

        public void Import()
        {
            if (SelectedImportType == ImportType.Localities)
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
            var importArgs = new ImportArgs<Mouse>
            {
                Filename = filename,
                WorkSheetName = sheet
            };
            importArgs.Columns.Clear();

            foreach (var columnViewModel in Columns)
            {
                var reader = columnViewModel.GetCellReader();
                if (reader != null)
                {
                    importArgs.Columns.Add((ICellReader<Mouse>) reader);
                }
            }

            var excelImport = new ExcelImport<Mouse>(context,
                mice => mice.Include(m => m.Records).Include(m => m.Locality),
                c => c.Localities.Include(l => l.Mice).Load());
            excelImport.ProgressChanged += (o, e) => { Progress = excelImport.Progress*100; };

            excelImport.Finished += (o, e) =>
            {
                excelImport.Save();
                MessageBox.Show("Import complete");
            };
            excelImport.Cancelled += (o, e) => { MessageBox.Show("Import cancelled."); };

            excelImport.Error += (o, e) => { MessageBox.Show("ERROR!\n\n" + e.Error); };

            excelImport.Start(importArgs);
        }

        private void DoImportLocalities()
        {
            var importArgs = new ImportArgs<Locality>
            {
                Filename = filename,
                WorkSheetName = sheet
            };
            importArgs.Columns.Clear();
            foreach (var columnViewModel in Columns)
            {
                var reader = columnViewModel.GetCellReader();
                if (reader != null)
                {
                    importArgs.Columns.Add((ICellReader<Locality>)reader);
                }
            }

            var excelImport = new ExcelImport<Locality>(context, locality => locality, _ => { });
            excelImport.ProgressChanged += (o, e) => { Progress = excelImport.Progress*100; };

            excelImport.Cancelled += (o, e) => { MessageBox.Show("Import cancelled."); };

            excelImport.Error += (o, e) => { MessageBox.Show("ERROR!\n\n" + e.Error); };

            excelImport.Finished += (o, e) =>
            {
                excelImport.Save();
                MessageBox.Show("Import complete");
            };
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