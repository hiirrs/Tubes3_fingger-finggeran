using System;
using System.Drawing;
using System.Text;
using MySqlConnector;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection.Metadata.Ecma335;

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
            List<string> correctNames = GetCorrectNamesFromDatabase();

            string name = "";

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                List<string> binaryStrings1 = ImageProcessor.ConvertImageToBinaryString2(imagePath1);
                List<string> asciiStrings1 = ImageProcessor.ConvertBinaryToAscii2(binaryStrings1);
                string asciiBlock1 = ImageProcessor.ExtractCentralAsciiBlock2(asciiStrings1, 30);

                int bestMatchPosition = -1;
                string bestMatchImagePath = string.Empty;
                double bestLevenshteinSimilarity = 0;

                foreach (string imagePath2 in imagePathsFromDatabase)
                {
                    List<string> binaryStrings2 = ImageProcessor.ConvertImageToBinaryString2(imagePath2);
                    List<string> asciiStrings2 = ImageProcessor.ConvertBinaryToAscii2(binaryStrings2);

                    int matchPosition = -1;
                    if (algorithm == "BM")
                    {
                        BoyerMoore bm = new BoyerMoore(asciiBlock1);
                        foreach (string asciiString2 in asciiStrings2)
                        {
                            matchPosition = bm.Search(asciiString2);
                            if (matchPosition != -1)
                            {
                                break;
                            }
                        }
                    }
                    else if (algorithm == "KMP")
                    {
                        KMP kmp = new KMP(asciiBlock1);
                        foreach (string asciiString2 in asciiStrings2)
                        {
                            matchPosition = kmp.Search(asciiString2);
                            if (matchPosition != -1)
                            {
                                break;
                            }
                        }
                    }

                    if (matchPosition != -1)
                    {
                        bestMatchPosition = matchPosition;
                        bestMatchImagePath = imagePath2;
                        break;
                    }
                    else
                    {
                        double levenshteinSimilarity = 0;
                        foreach (string asciiString2 in asciiStrings2)
                        {
                            levenshteinSimilarity = Levenshtein.CalculateLevenshteinBlockString(asciiBlock1, asciiString2);
                            if (levenshteinSimilarity > bestLevenshteinSimilarity)
                            {
                                bestLevenshteinSimilarity = levenshteinSimilarity;
                                bestMatchImagePath = imagePath2;
                            }
                        }
                        
                    }
                }

                if (bestMatchPosition != -1)
                {
                    Console.WriteLine("Match found at position: " + bestMatchPosition);
                    Console.WriteLine("Matching image path: " + bestMatchImagePath);
                    name = GetNameFromImagePath(bestMatchImagePath);
                }
                else
                {
                    Console.WriteLine("No exact match found. Best similarity match:");
                    Console.WriteLine("Image path: " + bestMatchImagePath);
                    Console.WriteLine($"Levenshtein similarity percentage: {bestLevenshteinSimilarity:F2}%");
                    name = GetNameFromImagePath(bestMatchImagePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }


            List<Biodata> biodata = GetBiodataForName(name, correctNames);
            Console.WriteLine($"Biodata for {name}:");
            foreach (var data in biodata)
            {
                Console.WriteLine($"{data.NIK}, {data.Nama}, {data.TempatLahir}, {data.TanggalLahir.ToString("yyyy-MM-dd")}, {data.JenisKelamin}, {data.GolonganDarah}, {data.Alamat}, {data.Agama}, {data.StatusPerkawinan}, {data.Pekerjaan}, {data.Kewarganegaraan}");
            }

            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            Console.WriteLine("Elapsed Time: {0} hours, {1} minutes, {2} seconds, {3} milliseconds",
                elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);
        }

        public static class AlayFixer
        {
            public static string FixAlayText(string text, List<string> correctNames)
            {
                Dictionary<char, char> numberSubs = new Dictionary<char, char>
                {
                    {'1', 'i'}, {'4', 'a'}, {'6', 'g'}, {'0', 'o'}, {'3', 'e'}, {'7', 't'}, {'8', 'b'}, {'5', 's'}, {'9', 'p'}
                };

                string fixedText = Regex.Replace(text, "[143678059]", match => numberSubs[match.Value[0]].ToString());
                fixedText = fixedText.ToLower();

                string ClosestMatch(string input, List<string> names)
                {
                    var similarities = names.Select(name => new
                    {
                        Name = name,
                        Similarity = Levenshtein.CalculateLevenshteinSimilarity(input, name.ToLower())
                    }).ToList();

                    var closest = similarities.OrderByDescending(x => x.Similarity).First();
                    return closest.Name;
                }

                string correctedText = ClosestMatch(fixedText, correctNames);

                return correctedText;
            }
        }

        private static string[] GetImagePathsFromDatabase()
        {
            string connectionString = "server=localhost;user=root;password=shzyt2929;database=tes";
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
                            imagePath = DecryptXOR(imagePath, 129);
                            imagePaths.Add(imagePath);
                        }
                    }
                }
            }

            return imagePaths.ToArray();
        }

        private static List<Biodata> GetBiodataForName(string name, List<string> correctNames)
        {
            string connectionString = "server=localhost;user=root;password=shzyt2929;database=tes";
            string query = "SELECT * FROM biodata";
            List<Biodata> biodataList = new List<Biodata>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            String namaHasil = DecryptXOR(reader.GetString("nama"), 129);
                            namaHasil = AlayFixer.FixAlayText(namaHasil, correctNames);
                            if (namaHasil == name){
                                Biodata data = new Biodata
                                {
                                    NIK = DecryptXOR(reader.GetString("NIK"), 129),
                                    Nama = namaHasil,
                                    TempatLahir = DecryptXOR(reader.GetString("tempat_lahir"), 129),
                                    TanggalLahir = DecryptDate(reader.GetDateTime("tanggal_lahir"), 129),
                                    JenisKelamin = reader.GetString("jenis_kelamin"),
                                    GolonganDarah = DecryptXOR(reader.GetString("golongan_darah"), 129),
                                    Alamat = DecryptXOR(reader.GetString("alamat"), 129),
                                    Agama =  DecryptXOR(reader.GetString("agama"), 129),
                                    StatusPerkawinan = reader.GetString("status_perkawinan"),
                                    Pekerjaan = DecryptXOR(reader.GetString("pekerjaan"), 129),
                                    Kewarganegaraan = DecryptXOR(reader.GetString("kewarganegaraan"), 129)
                                };
                                biodataList.Add(data);
                            } 
                        }
                    }
                }
            }
            return biodataList;
        }

        private static List<string> GetCorrectNamesFromDatabase()
        {
            string connectionString = "server=localhost;user=root;password=shzyt2929;database=tes";
            string query = "SELECT DISTINCT nama FROM sidik_jari";

            List<string> correctNames = new List<string>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string correctName = reader.GetString("nama");
                            correctName = DecryptXOR(correctName, 129);
                            correctNames.Add(correctName);
                        }
                    }
                }
            }

            return correctNames;
        }

        public static string GetNameFromImagePath(string imagePath)
        {
            string connectionString = "server=localhost;user=root;password=shzyt2929;database=tes";
            string query = "SELECT nama FROM sidik_jari WHERE berkas_citra = @imagePath";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@imagePath", imagePath);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return DecryptXOR(reader.GetString("nama"), 129);
                        }
                    }
                }
            }

            return null;
        }

        private static string DecryptXOR(string encryptedText, byte key)
        {
            StringBuilder decryptedText = new StringBuilder();
            foreach (char c in encryptedText)
            {
                decryptedText.Append((char)(c ^ key));
            }
            return decryptedText.ToString();
        }

        private static DateTime DecryptDate(DateTime encryptedDate, int key)
        {
            return encryptedDate.AddYears(-key);
        }

        public static class ImageProcessor
        {
            public static List<string> ConvertImageToBinaryString2(string imagePath)
            {
                try
                {
                    using (Bitmap bitmap = new Bitmap(imagePath))
                    {
                        List<string> binaryStrings = new List<string>();

                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            StringBuilder rowString = new StringBuilder();

                            for (int x = 0; x < bitmap.Width; x++)
                            {
                                Color pixelColor = bitmap.GetPixel(x, y);
                                int gray = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                                rowString.Append(gray < 128 ? "0" : "1");
                            }

                            binaryStrings.Add(rowString.ToString());
                        }

                        return binaryStrings;
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error processing image at path {imagePath}: {ex.Message}");
                }
            }

            public static List<string> ConvertBinaryToAscii2(List<string> binaryStrings)
            {
                int rows = binaryStrings.Count;
                int cols = binaryStrings[0].Length;
                List<string> asciiStrings = new List<string>();

                for (int y = 0; y < rows - 7; y++)
                {
                    StringBuilder rowString = new StringBuilder();

                    for (int x = 0; x < cols; x++)
                    {
                        StringBuilder binaryChunk = new StringBuilder();
                        for (int i = 0; i < 8; i++)
                        {
                            binaryChunk.Append(binaryStrings[y + i][x]);
                        }

                        char asciiChar = (char)Convert.ToInt32(binaryChunk.ToString(), 2);
                        rowString.Append(asciiChar);
                    }

                    asciiStrings.Add(rowString.ToString());
                }

                return asciiStrings;
            }

            public static string ExtractCentralAsciiBlock2(List<string> asciiStrings, int length)
            {
                int middleIndex = asciiStrings.Count / 2;
                string middleString = asciiStrings[middleIndex];
                int center = middleString.Length / 2;
                int start = Math.Max(0, center - length / 2);
                return middleString.Substring(start, Math.Min(length, middleString.Length - start));
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

    public class Levenshtein
    {
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

    public class Biodata
    {
        public string NIK { get; set; }
        public string Nama { get; set; }
        public string TempatLahir { get; set; }
        public DateTime TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string GolonganDarah { get; set; }
        public string Alamat { get; set; }
        public string Agama { get; set; }
        public string StatusPerkawinan { get; set; }
        public string Pekerjaan { get; set; }
        public string Kewarganegaraan { get; set; }
    }
}