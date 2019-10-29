using System;

namespace Universal.Extensions
{
    public static class ArrayHelper
    {
        // Performance-oriented algorithm selection
        public static T[] SelfSetToDefaults<T>(this T[] sourceArray)
        {
            if (sourceArray.Length <= 76)
            {
                for (var i = 0; i < sourceArray.Length; i++)
                {
                    sourceArray[i] = default(T);
                }
            }
            else
            { // 77+
                Array.Clear(
                    array: sourceArray,
                    index: 0,
                    length: sourceArray.Length);
            }
            return sourceArray;
        }
    }
}
