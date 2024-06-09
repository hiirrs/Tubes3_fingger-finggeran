import os
import mysql.connector
from datetime import datetime
from dotenv import load_dotenv

def list_files(directory):
    files = [f for f in os.listdir(directory) if os.path.isfile(os.path.join(directory, f))]
    return files

def xor_encrypt_decrypt(text, key, length):
    encrypted = ''.join(chr(ord(char) ^ key) for char in text)
    return encrypted[:length]

def mod_encrypt_date(date_obj, key):
    year = (date_obj.year + key) % 9999
    month = (date_obj.month + key - 1) % 12 + 1 
    day = (date_obj.day + key - 1) % 31 + 1

    while True:
        try:
            encrypted_date = datetime(year, month, day).date()
            break
        except ValueError:
            day -= 1

    return encrypted_date

def mod_decrypt_date(date_obj, key):
    year = (date_obj.year - key) % 9999
    month = (date_obj.month - key - 1) % 12 + 1 
    day = (date_obj.day - key - 1) % 31 + 1 

    while True:
        try:
            decrypted_date = datetime(year, month, day).date()
            break
        except ValueError:
            day -= 1

    return decrypted_date

load_dotenv("../.env")

directory_path = r'../test/dataset'
files = list_files(directory_path)

source_conn = mysql.connector.connect(
    host=os.getenv("DB_SERVER"),
    user=os.getenv("DB_USER"),
    password=os.getenv("DB_PASSWORD"),
    database=os.getenv("DB_SOURCE")
)

source_cursor = source_conn.cursor()

source_cursor.execute("SELECT NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan FROM biodata")
source_data = source_cursor.fetchall()

source_cursor.execute("SELECT berkas_citra, nama FROM sidik_jari")
source_fingerprint_data = source_cursor.fetchall()

encryption_key = int(os.getenv("ENCRYPTION_KEY"))

encrypted_data = []
for row in source_data:
    encrypted_row = list(row)
    encrypted_row[0] = xor_encrypt_decrypt(str(encrypted_row[0]), encryption_key, 16) 
    encrypted_row[1] = xor_encrypt_decrypt(encrypted_row[1], encryption_key, 100) 
    encrypted_row[2] = xor_encrypt_decrypt(encrypted_row[2], encryption_key, 100) 
    encrypted_row[3] = mod_encrypt_date(encrypted_row[3], encryption_key)
    encrypted_row[5] = xor_encrypt_decrypt(encrypted_row[5], encryption_key, 200)
    encrypted_row[6] = xor_encrypt_decrypt(encrypted_row[6], encryption_key, 200)
    encrypted_row[7] = xor_encrypt_decrypt(encrypted_row[7], encryption_key, 50)
    encrypted_row[9] = xor_encrypt_decrypt(encrypted_row[9], encryption_key, 100)
    encrypted_row[10] = xor_encrypt_decrypt(encrypted_row[10], encryption_key, 50) 
    encrypted_data.append(tuple(encrypted_row))

encrypted_fingerprint_data = []
for row in source_fingerprint_data:
    encrypted_row = list(row)
    encrypted_row[0] = xor_encrypt_decrypt(row[0], encryption_key, 200)
    encrypted_row[1] = xor_encrypt_decrypt(row[1], encryption_key, 100)
    encrypted_fingerprint_data.append(tuple(encrypted_row))

dest_conn = mysql.connector.connect(
    host=os.getenv("DB_SERVER"),
    user=os.getenv("DB_USER"),
    password=os.getenv("DB_PASSWORD"),
    database="fingerprint"
)

dest_cursor = dest_conn.cursor()

sql_biodata = "INSERT INTO biodata (NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"
for row in encrypted_data:
    dest_cursor.execute(sql_biodata, row)

sql_sidik_jari = "INSERT INTO sidik_jari (berkas_citra, nama) VALUES (%s, %s)"
for row in encrypted_fingerprint_data:
    dest_cursor.execute(sql_sidik_jari, row)

dest_conn.commit()

source_cursor.close()
source_conn.close()
dest_cursor.close()
dest_conn.close()