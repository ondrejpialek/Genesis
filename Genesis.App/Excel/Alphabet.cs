using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis.Excel
{
    public static class Alphabet
    {
        public static IEnumerable<string> GetAlphabet()
        {
            long i = 0L;
            while (true)
                yield return Encode(i++);
        }

        public static IEnumerable<string> GetAlphabet(Int64 numberOfElements)
        {
            long i = 0L;
            while (i < numberOfElements)
                yield return Encode(i++);
        }

        private static char[] chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static string Encode(Int64 value)
        {
            int count = chars.Length;
            string returnValue = null;
            do
            {
                returnValue = chars[value % count] + returnValue;
                value /= count;
            } while (value-- != 0);
            return returnValue;
        }
    }
}
