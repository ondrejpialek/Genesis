using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesis.Extensions
{
    public static class StreamWriterExtensions
    {
        public static async Task WriteCSVAsync(this StreamWriter writer, params string[] values)
        {
            var value = string.Join(",", values);
            await writer.WriteAsync(value);
        }

        public static async Task WriteCSVLineAsync(this StreamWriter writer, params string[] values)
        {
            await writer.WriteCSVAsync(values);
            await writer.WriteLineAsync();
        }
    }
}
