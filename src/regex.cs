using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using F23.StringSimilarity;


class Program
{
   static void Main()
   {
       List<string> testCases = new List<string>
       {
           "bintanG Dwl mArthen",
           "B1nt4n6 Dw1 M4rthen",
           "Bntng Dw Mrthen",
           "b1ntN6 Dw mrthn"
       };


       foreach (string testCase in testCases)
       {
           Console.WriteLine($"Original: {testCase} -> Fixed: {AlayFixer(testCase)}");
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
       string fixedText = Regex.Replace(text, "[143678059]", match => numberSubs[match.Value[0]].ToString());
       fixedText = fixedText.ToLower();


       // Split text into words for abbreviation fixing
       List<string> words = fixedText.Split(' ').ToList();


       // Function to find the closest match for abbreviations
       string ClosestMatch(string word)
       {
           Levenshtein levenshtein = new Levenshtein();
           var distances = originalWords.Select(original => new { Original = original, Distance = levenshtein.Distance(word, original) }).ToList();
           var closest = distances.OrderBy(x => x.Distance).First();
           return closest.Distance <= 2 ? closest.original : word;
       }


       // Fix abbreviations using Levenshtein distance
       List<string> fixedWords = words.Select(word => ClosestMatch(word)).ToList();


       return string.Join(" ", fixedWords);
   }
}