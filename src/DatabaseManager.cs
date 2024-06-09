using System;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection.Metadata.Ecma335;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using DotNetEnv;

namespace src {
    public static class DatabaseManager
    {
        private static string connectionString;

        static DatabaseManager()
        {
            Env.Load("../.env");

            connectionString = $"server={Environment.GetEnvironmentVariable("DB_SERVER")};" +
                               $"user={Environment.GetEnvironmentVariable("DB_USER")};" +
                               $"password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
                               $"database=fingerprint";
        }
        
        // get images from database
        public static string[] GetImagePathsFromDatabase()
        {
            string query = "SELECT berkas_citra FROM sidik_jari";
            List<string> imagePaths = new List<string>();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand(query, connection);
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
            return imagePaths.ToArray();
        }

        public static Biodata GetBiodataForName(string name, List<string> correctNames)
        {
            string query = "SELECT * FROM biodata";
            
            // List<Biodata> biodataList = new List<Biodata>();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                Biodata data = new Biodata();
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", name);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string namaHasil = DecryptXOR(reader.GetString("nama"), 129);
                        namaHasil = AlayFixer.FixAlayText(namaHasil, correctNames);
                        if (namaHasil == name) {
                            data.NIK = DecryptXOR(reader.GetString("NIK"), 129);
                            data.Nama = namaHasil;
                            data.TempatLahir = DecryptXOR(reader.GetString("tempat_lahir"), 129);
                            data.TanggalLahir = DecryptDate(reader.GetDateTime("tanggal_lahir"), 129);
                            data.JenisKelamin = reader.GetString("jenis_kelamin");
                            data.GolonganDarah = DecryptXOR(reader.GetString("golongan_darah"), 129);
                            data.Alamat = DecryptXOR(reader.GetString("alamat"), 129);
                            data.Agama = DecryptXOR(reader.GetString("agama"), 129);
                            data.StatusPerkawinan = reader.GetString("status_perkawinan");
                            data.Pekerjaan = DecryptXOR(reader.GetString("pekerjaan"), 129);
                            data.Kewarganegaraan = DecryptXOR(reader.GetString("kewarganegaraan"), 129);
                        };
                    }
                }
                return data;
            }
        }

        public static string GetNameFromImagePath(string imagePath)
        {
            imagePath =  DecryptXOR(imagePath, 129);
            string query = "SELECT nama FROM sidik_jari WHERE berkas_citra = @ImagePath";
            string name = "";
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
            return name;
        }

        // getting names from database
        public static List<string> GetCorrectNamesFromDatabase()
        {
            string query = "SELECT nama FROM sidik_jari";
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

        public static List<string> GetAlayNamesFromDatabase()
        {
            string query = "SELECT nama FROM biodata";
            List<string> alayNames = new List<string>();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string alayName = reader.GetString("nama");
                            alayNames.Add(alayName);
                        }
                    }
                }
            }
            return alayNames;
        }

        public static string FindAlayName(Dictionary<string, string> dictionary, string alayName)
        {
            if (dictionary.TryGetValue(alayName, out string foundName))
            {
                return foundName;
            }
            return null;
        }

        // decryption algorithms
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

    }
}
