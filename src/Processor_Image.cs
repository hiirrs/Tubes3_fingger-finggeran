using System;
using System.Drawing;
using System.Text;
using System.Collections.Generic;


namespace src {
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

        public static bool checkEmptyPattern(string pattern)
        {
            foreach (char c in pattern)
            {
                if ((int) c != 255)
                {
                    return false;
                }
            }
            return true;
        }
    }
}