﻿using System;
using System.Drawing;
using System.Text;
using MySqlConnector;
using System.Collections.Generic;
using System.Linq;

namespace FingerprintMatchingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the path of the first fingerprint image:");
            string? imagePath1 = Console.ReadLine();

            Console.WriteLine("Enter algorithm (BM or KMP):");
            string? algorithm = Console.ReadLine();

            string[] imagePathsFromDatabase = GetImagePathsFromDatabase();

            try
            {
                string binaryString1 = ImageProcessor.ConvertImageToBinaryString(imagePath1);
                string asciiString1 = ImageProcessor.ConvertBinaryToAscii(binaryString1);
                string asciiBlock1 = ImageProcessor.ExtractCentralAsciiBlock(asciiString1, 30);

                int bestMatchPosition = -1;
                string bestMatchImagePath = string.Empty;
                double bestLevenshteinSimilarity = 0;

                foreach (string imagePath2 in imagePathsFromDatabase)
                {
                    string binaryString2 = ImageProcessor.ConvertImageToBinaryString(imagePath2);
                    string asciiString2 = ImageProcessor.ConvertBinaryToAscii(binaryString2);
                    
                    int matchPosition = -1;
                    if (algorithm == "BM")
                    {
                        BoyerMoore bm = new BoyerMoore(asciiBlock1);
                        matchPosition = bm.Search(asciiString2);
                    }
                    else if (algorithm == "KMP")
                    {
                        KMP kmp = new KMP(asciiBlock1);
                        matchPosition = kmp.Search(asciiString2);
                    }

                    if (matchPosition != -1)
                    {
                        bestMatchPosition = matchPosition;
                        bestMatchImagePath = imagePath2;
                        break; // Exact match found, exit loop
                    }
                    else
                    {
                        double levenshteinSimilarity = Levenshtein.CalculateLevenshteinBlockString(asciiBlock1, asciiString2);
                        if (levenshteinSimilarity > bestLevenshteinSimilarity)
                        {
                            bestLevenshteinSimilarity = levenshteinSimilarity;
                            bestMatchImagePath = imagePath2;
                        }
                    }
                }

                if (bestMatchPosition != -1)
                {
                    Console.WriteLine("Match found at position: " + bestMatchPosition);
                    Console.WriteLine("Matching image path: " + bestMatchImagePath);
                }
                else
                {
                    Console.WriteLine("No exact match found. Best similarity match:");
                    Console.WriteLine("Image path: " + bestMatchImagePath);
                    Console.WriteLine($"Levenshtein similarity percentage: {bestLevenshteinSimilarity:F2}%");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            Console.WriteLine("Nama Alay:");
            string? namaAlay = Console.ReadLine();
            if (!string.IsNullOrEmpty(namaAlay))
            {
                Console.WriteLine($"Fixed: {AlayFixer(namaAlay)}");
            }
        }

        public static string AlayFixer(string text)
        {
            Dictionary<char, char> numberSubs = new Dictionary<char, char>
            {
                {'1', 'i'}, {'4', 'a'}, {'6', 'g'}, {'0', 'o'}, {'3', 'e'}, {'7', 't'}, {'8', 'b'}, {'5', 's'}, {'9', 'p'}
            };

            string originalString = "Bintang Dwi Marthen";
            List<string> originalWords = originalString.ToLower().Split(' ').ToList();

            // Fix number substitutions and lowercasing
            char[] fixedTextChars = text.ToLower().ToCharArray();
            for (int i = 0; i < fixedTextChars.Length; i++)
            {
                if (numberSubs.ContainsKey(fixedTextChars[i]))
                {
                    fixedTextChars[i] = numberSubs[fixedTextChars[i]];
                }
            }
            string fixedText = new string(fixedTextChars);

            // Split text into words for abbreviation fixing
            List<string> words = fixedText.Split(' ').ToList();

            // Function to find the closest match for abbreviations
            string ClosestMatch(string word)
            {
                var distances = originalWords.Select(original => new { Original = original, Distance = Levenshtein.ComputeLevenshteinDistance(word, original) }).ToList();
                var closest = distances.OrderBy(x => x.Distance).First();
                return closest.Distance <= 2 ? closest.Original : word;
            }

            // Fix abbreviations using Levenshtein distance
            List<string> fixedWords = words.Select(word => ClosestMatch(word)).ToList();

            return string.Join(" ", fixedWords);
        }

        private static string[] GetImagePathsFromDatabase()
        {
            string connectionString = "server=localhost;user=root;password=12345;database=tubes3_stima24";
            string query = "SELECT berkas_citra FROM sidik_jari";

            List<string> imagePaths = new List<string>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string imagePath = reader.GetString("berkas_citra");
                            imagePaths.Add(imagePath);
                        }
                    }
                }
            }

            return imagePaths.ToArray();
        }
    }
}

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

public static class ImageProcessor
{
    public static string ConvertImageToBinaryString(string imagePath)
    {
        try
        {
            using (Bitmap bitmap = new Bitmap(imagePath))
            {
                StringBuilder binaryString = new StringBuilder();

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color pixelColor = bitmap.GetPixel(x, y);
                        int gray = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                        binaryString.Append(gray < 128 ? "0" : "1");
                    }
                }

                return binaryString.ToString();
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Error processing image at path {imagePath}: {ex.Message}");
        }
    }

    public static string ConvertBinaryToAscii(string binaryString)
    {
        StringBuilder asciiString = new StringBuilder();

        for (int i = 0; i < binaryString.Length; i += 8)
        {
            if (i + 8 <= binaryString.Length)
            {
                string byteString = binaryString.Substring(i, 8);
                int asciiValue = Convert.ToInt32(byteString, 2);
                asciiString.Append((char)asciiValue);
            }
        }

        return asciiString.ToString();
    }

    public static string ExtractCentralAsciiBlock(string asciiString, int length)
    {
        int center = asciiString.Length / 2;
        int start = Math.Max(0, center - length / 2);
        return asciiString.Substring(start, Math.Min(length, asciiString.Length - start));
    }
}