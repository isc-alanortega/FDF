namespace PCG_FDF.Utility
{
    public static class Extensions
    {
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] enumArray = (T[])Enum.GetValues(src.GetType());
            int nextIndex = Array.IndexOf(enumArray, src) + 1;
            return (enumArray.Length == nextIndex) ? src : enumArray[nextIndex];
        }

        public static T Previous<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] enumArray = (T[])Enum.GetValues(src.GetType());
            int previousIndex = Array.IndexOf(enumArray, src) - 1;
            return (-1 == previousIndex) ? src : enumArray[previousIndex];
        }
    }
}
