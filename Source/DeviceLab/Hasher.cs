namespace InfoSpace.DeviceLab
{
    public static class Hasher
    {
        public static int Base
        {
            get { return 7; }
        }

        public static int Combine(int a, int b)
        {
            return a * 31 + b;
        }

        public static int Combine(int a, int b, int c)
        {
            return Combine(Combine(a, b), c);
        }

        public static int Combine(params int[] values)
        {
            var result = values[0];

            for (var i = 1; i < values.Length; i++)
            {
                result = result * 31 + values[i];
            }

            return result;
        }

        public static int SafeHash(object obj)
        {
            return obj != null ? obj.GetHashCode() : 0;
        }
    }
}
