
namespace src {
    public class KMP
    {
        private readonly int[] _lps;
        private readonly string _pattern;

        public KMP(string pattern)
        {
            _pattern = pattern;
            _lps = BuildLPSArray(pattern);
        }

        private int[] BuildLPSArray(string pattern)
        {
            int[] lps = new int[pattern.Length];
            int length = 0;
            int i = 1;

            lps[0] = 0;

            while (i < pattern.Length)
            {
                if (pattern[i] == pattern[length])
                {
                    length++;
                    lps[i] = length;
                    i++;
                }
                else
                {
                    if (length != 0)
                    {
                        length = lps[length - 1];
                    }
                    else
                    {
                        lps[i] = 0;
                        i++;
                    }
                }
            }

            return lps;
        }

        public int Search(string text)
        {
            int m = _pattern.Length;
            int n = text.Length;
            int i = 0;
            int j = 0;

            while (i < n)
            {
                if (_pattern[j] == text[i])
                {
                    j++;
                    i++;
                }

                if (j == m)
                {
                    return i - j;
                }
                else if (i < n && _pattern[j] != text[i])
                {
                    if (j != 0)
                    {
                        j = _lps[j - 1];
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            return -1;
        }
    }

}