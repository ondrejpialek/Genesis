﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Genesis;

namespace Genesis.Excel
{
    public class ExcelImportErrorEventArgs: EventArgs {
        private string error;

        public ExcelImportErrorEventArgs(string error) {
            this.error = error;
        }

        public string Error {
            get { return error; }
        }
    }

    public class ExcelImport<TEntity> : IImport<ImportArgs<TEntity>>
        where TEntity : class, new()
    {

        private GenesisContext context;
        private readonly Func<DbSet<TEntity>, IEnumerable<TEntity>> currentDataLoad;
        private readonly Action<GenesisContext> warmupAction;

        private DbSet<TEntity> repository;

        private CancellationTokenSource cancellationTokenSource;

        private InternalImporter<TEntity> importer;

        public event EventHandler ProgressChanged;

        public event EventHandler Started;

        public event EventHandler Finished;

        public event EventHandler Saved;

        public event EventHandler Cancelled;

        public event EventHandler<ExcelImportErrorEventArgs> Error;

        ///TODO: concurrent dictionary on errors
        //http://msdn.microsoft.com/en-us/library/ee378677.aspx

        public ExcelImport(GenesisContext context, Func<DbSet<TEntity>, IEnumerable<TEntity>> currentDataLoad, Action<GenesisContext> warmupAction)
            : base()
        {
            this.context = context;
            this.currentDataLoad = currentDataLoad;
            this.warmupAction = warmupAction;
            this.repository = context.Set<TEntity>();

            progress = 0;
        }

        public void Cancel()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
                importer = null;
                OnCancelled();
            }
        }

        protected virtual void OnCancelled()
        {
            State = ImportState.Cancelled;
            if (Cancelled != null)
                Cancelled(this, EventArgs.Empty);
        }

        public void Start(ImportArgs<TEntity> args)
        {
            OnRunning();

            ArgsParser<TEntity> parser = new ArgsParser<TEntity>(args);
            cancellationTokenSource = new CancellationTokenSource();
            importer = new InternalImporter<TEntity>(context, currentDataLoad, warmupAction, TaskScheduler.FromCurrentSynchronizationContext(), cancellationTokenSource.Token);
            importer.ProgressUpdate = step => Progress += step;
            importer.CompletedAction = OnFinished;
            importer.CancelledAction = OnCancelled;
            importer.ErrorAction = OnError;
            importer.Start(parser);
        }

        private void OnRunning()
        {
            State = ImportState.Running;
            if (Started != null)
                Started(this, EventArgs.Empty);
        }

        protected virtual void OnFinished()
        {
            Progress = 1;
            State = ImportState.Done;
            if (Finished != null)
                Finished(this, EventArgs.Empty);
        }

        protected virtual void OnError(string error)
        {
            State = ImportState.Error;
            if (Error != null)
                Error(this, new ExcelImportErrorEventArgs(error));
        }

        public void Save()
        {
            if (importer != null)
            {
                importer.Save();
                importer = null;
                OnSaved();
            }
        }

        protected virtual void OnSaved()
        {
            State = ImportState.Saved;
            if (Saved != null)
                Saved(this, EventArgs.Empty);
        }

        public ImportState State { get; protected set;  }

        private double progress;
        public double Progress
        {
            get
            {
                return progress;
            }

            protected set
            {
                progress = value;
                if (ProgressChanged != null)
                {
                    ProgressChanged.Invoke(this, new EventArgs());
                }
            }
        }
    }
}
