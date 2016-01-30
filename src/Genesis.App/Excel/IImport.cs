using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis.Excel
{
    public interface IImport<in TArgs>
    {
        event EventHandler ProgressChanged;
        event EventHandler Started;
        event EventHandler Finished;
        event EventHandler Saved;
        event EventHandler Cancelled;

        void Cancel();
        void Start(TArgs Args);
        void Save();

        ImportState State { get; }
        double Progress { get; }
    }
}
