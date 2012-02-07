using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genesis;

namespace Genesis.Excel
{
    internal class InternalImporter<TEntity>
        where TEntity: class, new()
    {
        #region Properties

        public Action<double> ProgressUpdate { get; set; }

        public Action CompletedAction { get; set; }

        public Action CancelledAction { get; set; }

        #endregion

        #region Dependencies

        private TaskScheduler synchronizedScheduler;

        private DbSet<TEntity> repository;

        private CancellationToken cancellationToken;

        #endregion

        #region Fields

        private double step;

        private IEnumerable<TEntity> results;

        private WorksheetReader<TEntity> worksheetReader;

        #endregion

        public InternalImporter(DbSet<TEntity> repository, TaskScheduler synchronizedScheduler, CancellationToken cancellationToken)
        {
            this.synchronizedScheduler = synchronizedScheduler;
            this.repository = repository;
            this.cancellationToken = cancellationToken;
        }

        public void Start(ArgsParser<TEntity> parser)
        {
            Task.Factory.StartNew(() =>
            {
                using (ExcelFile file = parser.GetExcelFile())
                {
                    ExcelWorksheet excelWorksheet = parser.GetExcelWorksheet(file);
                    RowReader<TEntity> rowReader = parser.GetRowReader();
                    worksheetReader = new WorksheetReader<TEntity>(excelWorksheet, rowReader);
                    try
                    {
                        Task.Factory
                            .StartNew(Import, cancellationToken)
                            .ContinueWith(x =>
                            {
                                if (CompletedAction != null)
                                    CompletedAction.Invoke();
                            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, synchronizedScheduler)
                            .Wait();
                    }
                    catch (AggregateException ae)
                    {
                        var exceptions = ae.Flatten();
                        if (exceptions.InnerExceptions.First(x => x is TaskCanceledException) != null) {
                            Task.Factory.StartNew(() =>
                                {
                                    if (CancelledAction != null)
                                        CancelledAction.Invoke();
                                }, CancellationToken.None, TaskCreationOptions.None, synchronizedScheduler);
                        }
                        exceptions.Handle(e => e is TaskCanceledException);
                    }
                }
            }, cancellationToken);
        }

        public void Save()
        {
            /*repository.
            foreach (TEntity entity in results)
            {
                repository. (entity);
            }*/
            //repository.
        }

        private void Import()
        {
            var records = new List<TEntity>(repository);
            var imports = worksheetReader.Records;

            IDictionary<RowApplicator<TEntity>, TEntity> data = new Dictionary<RowApplicator<TEntity>, TEntity>();

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
            /*
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

                data.Add(import, record);
            }*/

            results = o.ObserveOn(Scheduler.TaskPool).Select(x => Update(x.Item1, x.Item2)).ToList().First();

            step = (double)1 / data.Count;

            //results = data.ToList().AsParallel().WithCancellation(cancellationToken).Select(x => Update(x.Key, x.Value)).ToList();
        }

        private TEntity Update(RowApplicator<TEntity> applicator, TEntity entity) {
            applicator.Apply(entity);
            if (ProgressUpdate != null)
                Task.Factory.StartNew(() => ProgressUpdate.Invoke(step), cancellationToken, TaskCreationOptions.None, synchronizedScheduler);
            return entity;
        }
    }
}
