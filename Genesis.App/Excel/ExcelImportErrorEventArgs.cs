using System;

namespace Genesis.Excel
{
    public class ExcelImportErrorEventArgs: EventArgs {
        public ExcelImportErrorEventArgs(string error) {
            Error = error;
        }

        public string Error { get; }
    }
}