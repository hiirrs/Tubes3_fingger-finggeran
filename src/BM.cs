using System;


namespace src {
    public class BoyerMoore
    {
        private readonly int[] _badCharacterShift;
        private readonly string _pattern;

        public BoyerMoore(string pattern)
        {
            _pattern = pattern;
            _badCharacterShift = BuildBadCharacterShift(pattern);
        }

        private int[] BuildBadCharacterShift(string pattern)
        {
            const int alphabetSize = 256;
            int[] badCharShift = new int[alphabetSize];
            for (int i = 0; i < alphabetSize; i++)
            {
                badCharShift[i] = pattern.Length;
            }
            for (int i = 0; i < pattern.Length - 1; i++)
            {
                badCharShift[pattern[i]] = pattern.Length - 1 - i;
            }
            return badCharShift;
        }

        public int Search(string text)
        {
            int m = _pattern.Length;
            int n = text.Length;
            int skip;
            for (int i = 0; i <= n - m; i += skip)
            {
                skip = 0;
                for (int j = m - 1; j >= 0; j--)
                {
                    if (_pattern[j] != text[i + j])
                    {
                        skip = Math.Max(1, _badCharacterShift[text[i + j]] - (m - 1 - j));
                        break;
                    }
                }
                if (skip == 0) return i;
            }
            return -1;
        }
    }
}