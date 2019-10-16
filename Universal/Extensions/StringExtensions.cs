namespace Universal.Extensions
{
    public static class StringExtensions
    {
        public static string ToLength(this string str, int length)
        {
            if (string.IsNullOrEmpty(str))
                str = "STRING NULL";
            return str.PadRight(length, '\0');
        }

        public static string CenterLength(this string str, int length)
        {
            var newString = str;

            while (newString.Length != length)
            {
                if (newString.Length % 2 == 0)
                {
                    newString = " " + newString;
                }
                else
                {
                    newString = newString + " ";
                }
            }

            return newString;
        }
    }
}
