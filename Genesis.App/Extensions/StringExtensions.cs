using System;

namespace Genesis
{
    public static class StringExtensions
    {
        public static TEnum? ToEnum<TEnum>(this string strEnumValue)
            where TEnum: struct
        {
            if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
                return default(TEnum?);

            return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
        }
    }
}
