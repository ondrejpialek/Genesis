using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis.Excel
{
    public static class Alphabet
    {
        private static int currentSize = 0;
        private static List<string> currentAlphabet;

        public static IEnumerable<string> GetAlphabet()
        {
            var i = 0;
            while (true)
                yield return Encode(i++);
        }

        public static IEnumerable<string> GetAlphabet(int numberOfElements)
        {
            var i = 0;
            while (i < numberOfElements)
                yield return Encode(i++);
        }

        public static string GetExcelColumn(int index)
        {
            if (index >= currentSize)
            {
                if (currentSize == 0)
                    currentSize++;
                while (index >= currentSize)
                    currentSize = currentSize * 2;
                currentAlphabet = GetAlphabet(currentSize).ToList();
            }
            return currentAlphabet[index];
        }

        private static readonly char[] Chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static string Encode(int value)
        {
            var count = Chars.Length;
            string returnValue = null;
            do
            {
                returnValue = Chars[value % count] + returnValue;
                value /= count;
            } while (value-- != 0);
            return returnValue;
        }
    }
}
