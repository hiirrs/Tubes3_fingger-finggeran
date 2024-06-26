﻿using System;
using MySqlConnector;
using DotNetEnv;

// Load the schema
class SchemaLoader
{
    static void Main()
    {
        Env.Load("../.env");

        string server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
        string user = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
        string password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "12345";
        string database1 = "fingerprint";
        string database2 = Environment.GetEnvironmentVariable("DB_SOURCE") ?? "fingerprint_original";

        string connectionString = $"Server={server};User ID={user};Password={password};";

        try
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                CreateDatabase(conn, database1);
                CreateDatabase(conn, database2);

                LoadSchema(conn, database1);
                LoadSchema(conn, database2);

                Console.WriteLine("Databases and tables created successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void CreateDatabase(MySqlConnection conn, string database)
    {
        using (var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{database}`;", conn))
        {
            cmd.ExecuteNonQuery();
        }
    }

    static void LoadSchema(MySqlConnection conn, string database)
    {
        conn.ChangeDatabase(database);

        string sqlDump = @"
        -- MySQL dump 10.13  Distrib 8.0.36, for Linux (x86_64)
        --
        -- Host: localhost    Database: tubes3_stima24
        -- ------------------------------------------------------
        -- Server version	8.0.36-0ubuntu0.22.04.1

        /*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
        /*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
        /*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
        /*!50503 SET NAMES utf8mb4 */;
        /*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
        /*!40103 SET TIME_ZONE='+00:00' */;
        /*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
        /*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
        /*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
        /*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

        --
        -- Table structure for table `biodata`
        --

        DROP TABLE IF EXISTS `biodata`;
        /*!40101 SET @saved_cs_client     = @@character_set_client */;
        /*!50503 SET character_set_client = utf8mb4 */;
        CREATE TABLE `biodata` (
        `NIK` varchar(16) NOT NULL,
        `nama` varchar(100) DEFAULT NULL,
        `tempat_lahir` varchar(50) DEFAULT NULL,
        `tanggal_lahir` date DEFAULT NULL,
        `jenis_kelamin` enum('Laki-Laki','Perempuan') DEFAULT NULL,
        `golongan_darah` varchar(5) DEFAULT NULL,
        `alamat` varchar(255) DEFAULT NULL,
        `agama` varchar(50) DEFAULT NULL,
        `status_perkawinan` enum('Belum Menikah','Menikah','Cerai') DEFAULT NULL,
        `pekerjaan` varchar(100) DEFAULT NULL,
        `kewarganegaraan` varchar(50) DEFAULT NULL,
        PRIMARY KEY (`NIK`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
        /*!40101 SET character_set_client = @saved_cs_client */;

        --
        -- Dumping data for table `biodata`
        --

        LOCK TABLES `biodata` WRITE;
        /*!40000 ALTER TABLE `biodata` DISABLE KEYS */;
        /*!40000 ALTER TABLE `biodata` ENABLE KEYS */;
        UNLOCK TABLES;

        --
        -- Table structure for table `sidik_jari`
        --

        DROP TABLE IF EXISTS `sidik_jari`;
        /*!40101 SET @saved_cs_client     = @@character_set_client */;
        /*!50503 SET character_set_client = utf8mb4 */;
        CREATE TABLE `sidik_jari` (
        `berkas_citra` text,
        `nama` varchar(100) DEFAULT NULL
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
        /*!40101 SET character_set_client = @saved_cs_client */;

        --
        -- Dumping data for table `sidik_jari`
        --

        LOCK TABLES `sidik_jari` WRITE;
        /*!40000 ALTER TABLE `sidik_jari` DISABLE KEYS */;
        /*!40000 ALTER TABLE `sidik_jari` ENABLE KEYS */;
        UNLOCK TABLES;
        /*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

        /*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
        /*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
        /*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
        /*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
        /*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
        /*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
        /*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

        -- Dump completed on 2024-05-04 15:57:34
        ";

        foreach (var sql in sqlDump.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!string.IsNullOrWhiteSpace(sql))
            {
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
