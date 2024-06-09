from faker import Faker
import random
import os
import mysql.connector
from datetime import datetime
from datetime import datetime

def alay_upper_lower(text):
    result = ''.join(random.choice([char.lower(), char.upper()]) for char in text)
    return result

def alay_numbers(text):
    replacement = {'a': '4', 'i': '1', 'e': '3', 'o': '0', 's': '5', 'g': '6', 't': '7'}
    result = ''.join(replacement.get(char.lower(), char) for char in text)
    return result

def alay_remove_vowels(text):
    vowels = "aeiouAEIOU"
    result = ''.join(char for char in text if char not in vowels or random.random() > 0.5)
    return result

def alay_abbreviation(text):
    words = text.split()
    result = ' '.join(alay_remove_vowels(word) for word in words)
    return result

def alay_combination(text):
    if random.random() > 0.75:
        result = alay_upper_lower(alay_numbers(alay_abbreviation(text)))
        return result
    else:
        return text

def generate_bahasa_alay(text):
    print("Kata orisinal:", text)
    print("Kombinasi huruf besar-kecil:", alay_upper_lower(text))
    print("Penggunaan angka:", alay_numbers(text))
    print("Penyingkatan:", alay_abbreviation(text))
    print("Kombinasi ketiganya:", alay_combination(text))

fake = Faker()

def generate_fake_data():
    nik = fake.random_number(digits=16)
    nama = fake.name()
    tempat_lahir = fake.city()
    tanggal_lahir = fake.date_of_birth()
    jenis_kelamin = random.choice(['Laki-Laki', 'Perempuan'])
    golongan_darah = random.choice(['A', 'B', 'AB', 'O']) + random.choice(['+', '-'])
    alamat = fake.address()
    agama = random.choice(['Islam', 'Kristen', 'Katolik', 'Hindu', 'Buddha', 'Konghucu'])
    status_perkawinan = random.choice(['Belum Menikah', 'Menikah', 'Cerai'])
    pekerjaan = fake.job()
    kewarganegaraan = fake.country()

    return [nik, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan]

def list_files(directory):
    files = [f for f in os.listdir(directory) if os.path.isfile(os.path.join(directory, f))]
    return files

def xor_encrypt_decrypt(text, key, length):
    encrypted = ''.join(chr(ord(char) ^ key) for char in text)
    return encrypted[:length]  # Ensure the encrypted string fits within the specified length

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

directory_path = r'../test/dataset'
files = list_files(directory_path)

source_conn = mysql.connector.connect(
    host="localhost",
    user="root",
    password="23)#)$",
    database="fingerprint_original"
)

source_cursor = source_conn.cursor()

source_cursor.execute("SELECT NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan FROM biodata")
source_data = source_cursor.fetchall()

source_cursor.execute("SELECT berkas_citra, nama FROM sidik_jari")
source_fingerprint_data = source_cursor.fetchall()

encryption_key = 129 

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
    host="localhost",
    user="root",
    password="23)#)$",
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