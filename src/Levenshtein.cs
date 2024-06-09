using System;

// Non-exact match percentage is calculated using Levenshtein distance
namespace src {
    public class Levenshtein {
        public static double CalculateLevenshteinBlockString(string block, string str) 
        {
            int blockLength = block.Length;
            int strLength = str.Length;
        
            double maxPercentage = double.MinValue;

            for (int i = 0; i <= strLength - blockLength; i++)
            {
                string substring = str.Substring(i, blockLength);
                double distance = CalculateLevenshteinSimilarity(block, substring);
                if (distance > maxPercentage)
                {
                    maxPercentage = distance;
                }
            }

            return maxPercentage;
        }

        public static double CalculateLevenshteinSimilarity(string s1, string s2)
        {
            int distance = ComputeLevenshteinDistance(s1, s2);
            int maxLen = Math.Max(s1.Length, s2.Length);
            return (1.0 - (double)distance / maxLen) * 100;
        }

        public static int ComputeLevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
            {
                d[i, 0] = i;
            }
            for (int j = 0; j <= s2.Length; j++)
            {
                d[0, j] = j;
            }

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }
    }
}