namespace URLShortenerService.Hash
{
    public class UrlHashingService
    {
        /// <summary>
        ///  Naive bijective functions for encoding/decoding Database ID 
        ///  Pre defined Alphabet for values to use in short URL codes
        /// </summary>
        public static readonly string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public static readonly int Base = Alphabet.Length;

        public UrlHashingService() {}

        /// <summary>
        /// Encodes database ID to Shortened URL string
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Endoded string</returns>
        public string Encode(int i)
        {
            if (i == 0) return Alphabet[0].ToString();

            var s = string.Empty;

            while (i > 0)
            {
                s += Alphabet[i % Base];
                i = i / Base;
            }

            return string.Join(string.Empty, s.Reverse());
        }

        /// <summary>
        /// Decodes short URL string to DB Id for lookup
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Returns DB id for lookup</returns>
        public int Decode(string s)
        {
            var i = 0;

            foreach (var c in s)
            {
                i = (i * Base) + Alphabet.IndexOf(c);
            }

            return i;
        }
    }
}
