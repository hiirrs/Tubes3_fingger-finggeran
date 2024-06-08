using System;
// using MySqlConnector;
using System.Collections.Generic;
using System.Diagnostics;

namespace FingerprintMatchingApp{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine("Enter the path of the first fingerprint image:");
            string? imagePath1 = Console.ReadLine();

            Console.WriteLine("Enter algorithm (BM or KMP):");
            string? algorithm = Console.ReadLine();

            string[] imagePathsFromDatabase = DatabaseManager.GetImagePathsFromDatabase();
            List<string> databaseName = DatabaseManager.GetAlayNamesFromDatabase();
            List<string> correctNames = DatabaseManager.GetCorrectNamesFromDatabase();

            Dictionary<string, string> nameMap = new Dictionary<string, string>();

            foreach (string nama2 in databaseName)
            {
                Console.WriteLine(nama2);
                string fixedName = AlayFixer.FixAlayText(nama2, correctNames);
                if (!nameMap.ContainsKey(nama2)) // Check if the key already exists
                {
                    Console.WriteLine(fixedName);
                    nameMap.Add(nama2, fixedName);
                }
                else
                {
                    // Optionally, handle the case where the key already exists, such as updating the value or logging a message
                    Console.WriteLine($"Skipping duplicate entry for: {nama2}");
                }
            }
            Console.WriteLine($"Finish Mapping");

            string name = "";

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
                        foreach (string asciiString2 in asciiStrings2) {
                            matchPosition = bm.Search(asciiString2);
                            if (matchPosition != -1) {
                                break;
                            }
                        }
                    }
                    else if (algorithm == "KMP")
                    {
                        KMP kmp = new KMP(asciiBlock1);
                        foreach (string asciiString2 in asciiStrings2) {
                            matchPosition = kmp.Search(asciiString2);
                            if (matchPosition != -1) {
                                break;
                            }
                        }
                    }

                    if (matchPosition != -1)
                    {
                        bestMatchPosition = matchPosition;
                        bestMatchImagePath = imagePath2;
                        break; // Exact match found, exit loop
                    }
                    else
                    {
                        double levenshteinSimilarity = 0;
                        foreach (string asciiString2 in asciiStrings2) {
                            levenshteinSimilarity = Levenshtein.CalculateLevenshteinBlockString(asciiBlock1, asciiString2);
                        }
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
                    name = DatabaseManager.GetNameFromImagePath(bestMatchImagePath);
                    
                }
                else
                {
                    Console.WriteLine("No exact match found. Best similarity match:");
                    Console.WriteLine("Image path: " + bestMatchImagePath);
                    Console.WriteLine($"Levenshtein similarity percentage: {bestLevenshteinSimilarity:F2}%");
                    name = DatabaseManager.GetNameFromImagePath(bestMatchImagePath);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            
            string keyNameToDatabase = DatabaseManager.FindAlayName(nameMap, name);
    
            List<Biodata> biodata = DatabaseManager.GetBiodataForName(keyNameToDatabase);
            stopwatch.Stop();
            Console.WriteLine($"Biodata for {keyNameToDatabase}:");
            foreach (var data in biodata)
            {
                Console.WriteLine($"{data.NIK}, {data.Nama}, {data.TempatLahir}, {data.TanggalLahir.ToString("yyyy-MM-dd")}, {data.JenisKelamin}, {data.GolonganDarah}, {data.Alamat}, {data.Agama}, {data.StatusPerkawinan}, {data.Pekerjaan}, {data.Kewarganegaraan}");
            }
            
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
        }

    }
}