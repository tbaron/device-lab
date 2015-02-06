using System.Text;

namespace InfoSpace.DeviceLab
{
    /// <summary>
    /// A base-62 encoder implementation
    /// </summary>
    /// <remarks>
    /// Based on:
    /// https://gist.github.com/miguelhasse/555ca5d965547301280d
    /// "A Secure, Lossless, and Compressed Base62 Encoding" - http://www.opitz-online.com/dl/base62_encoding.pdf
    /// </remarks>
    public static class Base62Encoder
    {
        private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public static string ToBase62(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            var bitcount = bytes.Length * 8;

            for (int n = 0; n < bitcount; n += 6)
            {
                int index = 0;
                int i = 0;

                while (i < 6 && n + i < bitcount)
                {
                    index <<= 1;
                    index |= ((bytes[(n + i) / 8] & (0x80 >> (n + i) % 8)) > 0) ? 1 : 0;
                    i++;
                }

                if (i == 6 && (index & Alphabet.Length) == Alphabet.Length)
                {
                    index >>= 1;
                    n--;
                }

                sb.Append(Alphabet[index]);
            }

            return sb.ToString();
        }
    }
}
