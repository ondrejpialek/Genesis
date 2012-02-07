﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Genesis.Excel;
using Microsoft.Win32;
using System.Linq;
using System;
using System.Windows;

namespace Genesis.ViewModel
{

    public class ImportViewModel : ViewModelBase
    {
        private GenesisContext context;
        private IExcelService excelService;

        public ObservableCollection<string> Sheets { get; private set; }
        public ObservableCollection<ColumnViewModel> Columns { get; protected set; }
        public ObservableCollection<ICellReader> Fields { get; protected set; }
        public ObservableCollection<Gene> Genes { get; protected set; }

        private readonly IEnumerable<ICellReader<Locality>> localityFields = new List<ICellReader<Locality>> { new CodeColumn(), new LatitudeColumn(), new LongitudeColumn(), new LocalityNameColumn() };
        private readonly ICollection<ICellReader<Mouse>> miceFields = new List<ICellReader<Mouse>> { new PINColumn(), new TraitColumn(), new SexColumn()};

        public ImportViewModel(IExcelService excelService)
        {
            context = new GenesisContext();
            this.excelService = excelService;

            Sheets = new ObservableCollection<string>();
            Columns = new ObservableCollection<ColumnViewModel>();

            Genes = new ObservableCollection<Gene>(context.Genes);

            miceFields.Add(new MouseLocalityColumn(context.Localities));

            Fields = new ObservableCollection<ICellReader>(localityFields.Cast<ICellReader>().Union(miceFields));
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
                            excelFile = excelService.Open(filename);
                            Sheets.Clear();
                            foreach (var sheet in excelFile.Worksheets)
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
                worksheet = excelFile.Worksheets[Sheets.IndexOf(value)];
                LoadColumns();
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

        private RelayCommand importlocalities;
        public RelayCommand ImportLocalities
        {
            get
            {
                if (importlocalities == null)
                {
                    importlocalities = new RelayCommand(() =>
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

                        //excelImport.Started += new EventHandler(import_Started);
                        //excelImport.Saved += new EventHandler();
                        //excelImport.Cancelled += new EventHandler(import_Finished);
                        excelImport.Finished += new EventHandler((o, e) =>
                        {
                            excelImport.Save();
                            MessageBox.Show("Import complete");
                        });
                        excelImport.Start(importArgs);
                    });
                }

                return importlocalities;
            }
        }

        private RelayCommand importData;
        public RelayCommand ImportData
        {
            get
            {
                if (importData == null)
                {
                    importData = new RelayCommand(() =>
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
                        excelImport.Start(importArgs);
                    });
                }

                return importData;
            }
        }
    }
}