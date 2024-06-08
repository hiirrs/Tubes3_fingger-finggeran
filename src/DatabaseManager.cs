using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace FingerprintMatchingApp
{
    public static class DatabaseManager
    {
        private static string connectionString = "server=localhost;user=root;password=23)#)$;database=tubes3_stima24";

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
                        imagePaths.Add(imagePath);
                    }
                }
            }
            return imagePaths.ToArray();
        }

        public static List<Biodata> GetBiodataForName(string name)
        {
            string query = "SELECT * FROM biodata WHERE nama = @name";
            List<Biodata> biodataList = new List<Biodata>();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", name);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Biodata data = new Biodata
                        {
                            NIK = reader.GetString("NIK"),
                            Nama = reader.GetString("nama"),
                            TempatLahir = reader.GetString("tempat_lahir"),
                            TanggalLahir = reader.GetDateTime("tanggal_lahir"),
                            JenisKelamin = reader.GetString("jenis_kelamin"),
                            GolonganDarah = reader.GetString("golongan_darah"),
                            Alamat = reader.GetString("alamat"),
                            Agama = reader.GetString("agama"),
                            StatusPerkawinan = reader.GetString("status_perkawinan"),
                            Pekerjaan = reader.GetString("pekerjaan"),
                            Kewarganegaraan = reader.GetString("kewarganegaraan")
                        };
                        biodataList.Add(data);
                    }
                }
            }
            return biodataList;
        }

        public static string GetNameFromImagePath(string imagePath)
        {
            string query = "SELECT nama FROM sidik_jari WHERE berkas_citra = @ImagePath";
            string name = "";
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@ImagePath", imagePath);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        name = reader["nama"] != null ? reader.GetString("nama") : "No name found";
                    }
                    else
                    {
                        name = "No record found";
                    }
                }
            }
            return name;
        }

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

    }
}
