﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genesis;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Text;

namespace Genesis.Excel
{
    internal class InternalImporter<TEntity>
        where TEntity: class, new()
    {
        #region Properties

        public Action<double> ProgressUpdate { get; set; }

        public Action CompletedAction { get; set; }

        public Action CancelledAction { get; set; }

        public Action<string> ErrorAction { get; set; }

        #endregion

        #region Dependencies

        private TaskScheduler synchronizedScheduler;

        private readonly Func<DbSet<TEntity>, IEnumerable<TEntity>> currentDataLoad;
        private readonly Action<GenesisContext> warmupAction;

        private CancellationToken cancellationToken;

        #endregion

        #region Fields

        private double step;

        private WorksheetReader<TEntity> worksheetReader;
        private GenesisContext context;

        #endregion

        public InternalImporter(GenesisContext context, Func<DbSet<TEntity>, IEnumerable<TEntity>> currentDataLoad, Action<GenesisContext> warmupAction, TaskScheduler synchronizedScheduler, CancellationToken cancellationToken)
        {
            this.synchronizedScheduler = synchronizedScheduler;
            this.context = context;
            this.currentDataLoad = currentDataLoad;
            this.warmupAction = warmupAction;
            this.cancellationToken = cancellationToken;
        }

        public void Start(ArgsParser<TEntity> parser)
        {
            Task.Factory.StartNew(() =>
            {
                using (IExcelFile file = parser.GetExcelFile())
                {
                    var excelWorksheet = parser.GetExcelWorksheet(file);
                    var rowReader = parser.GetRowReader();
                    worksheetReader = new WorksheetReader<TEntity>(excelWorksheet, rowReader);
                    try
                    {
                        var import = Task.Factory.StartNew(Import, cancellationToken);
                        var cont = import.ContinueWith(x =>
                        {
                            CompletedAction?.Invoke();
                        }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, synchronizedScheduler);
                        
                        cont.Wait();
                    }
                    catch (AggregateException ae)
                    {
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }

                        var exceptions = ae.Flatten();
                        if (exceptions.InnerExceptions.Any(x => x is TaskCanceledException))
                        {
                            Task.Factory.StartNew(() =>
                            {
                                CancelledAction?.Invoke();
                            }, CancellationToken.None, TaskCreationOptions.None, synchronizedScheduler);
                            exceptions.Handle(e => e is TaskCanceledException);
                        }
                        else if (ErrorAction != null)
                        {
                            var validations = exceptions.InnerExceptions.OfType<DbEntityValidationException>().SelectMany(v => v.EntityValidationErrors).GroupBy(e => e.Entry);
                            var sb = new StringBuilder();
                            foreach (var entry in validations)
                            {
                                sb.Append(entry.Key.Entity + ":\n");
                                foreach (var validation in entry)
                                {
                                    foreach(var error in validation.ValidationErrors) {
                                        sb.Append(" - " + error.PropertyName + ": " + error.ErrorMessage + "\n");
                                    } 
                                }
                            }

                            var errors = exceptions.InnerExceptions.Where(e => !(e is DbEntityValidationException)).Select(e =>
                                $"{e.Message} ({e.InnerException?.Message ?? "no more info"})");
                            var msg = sb + string.Join("\n", errors);
                            Task.Factory.StartNew(() =>
                            {
                                ErrorAction.Invoke(msg);
                            }, CancellationToken.None, TaskCreationOptions.None, synchronizedScheduler);
                        }                        
                    }
                }
            }, cancellationToken);
        }

        public void Save()
        {
            context.ChangeTracker.DetectChanges();
            context.SaveChanges();
            context.Configuration.AutoDetectChangesEnabled = true;
            context.Configuration.ValidateOnSaveEnabled = true;
        }

        private void Import()
        {
            step = (double)1 / worksheetReader.GetRecordCount();

            warmupAction(context);
            var repository = context.Set<TEntity>();
            var records = currentDataLoad(repository).ToList();

            context.Configuration.AutoDetectChangesEnabled = false;
            context.Configuration.ValidateOnSaveEnabled = false;

            var imports = worksheetReader.Records;

            var o = Observable.Create<Tuple<RowApplicator<TEntity>, TEntity>>(observer => {
                foreach (var import in imports)
                {
                    TEntity record = default(TEntity);
                    for(int i = 0; i < records.Count; i++) {
                        if (import.Matches(records[i])) {
                            record = records[i];
                            records.RemoveAt(i);
                            break;
                        }
                    }
                    if (record == null)
                    {
                        record = repository.Create();
                        repository.Add(record);
                    }

                    observer.OnNext(new Tuple<RowApplicator<TEntity>, TEntity>(import, record));
                }

                observer.OnCompleted();

                return () => { };
            });

            o.ObserveOn(Scheduler.TaskPool).Select(x => Update(x.Item1, x.Item2)).ToList().Wait();
        }

        private TEntity Update(RowApplicator<TEntity> applicator, TEntity entity) {
            applicator.Apply(entity);
            if (ProgressUpdate != null)
                Task.Factory.StartNew(() => ProgressUpdate.Invoke(step), cancellationToken, TaskCreationOptions.None, synchronizedScheduler);
            return entity;
        }
    }
}
