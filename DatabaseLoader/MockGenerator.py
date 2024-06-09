from faker import Faker
import random
import os
import mysql.connector
from datetime import datetime
from dotenv import load_dotenv

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

conn = mysql.connector.connect(
    host=os.getenv("DB_SERVER"),
    user=os.getenv("DB_USER"),
    password=os.getenv("DB_PASSWORD"),
    database="fingerprint"
)

cursor = conn.cursor()

encryption_key = int(os.getenv("ENCRYPTION_KEY"))

k = 0
num_biodata = 600

for i in range(num_biodata):
    sql_biodata = "INSERT INTO biodata (NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"
    fake_biodata = generate_fake_data()
    original_name = fake_biodata[1]

    fake_biodata[0] = xor_encrypt_decrypt(str(fake_biodata[0]), encryption_key, 16)
    fake_biodata[1] = xor_encrypt_decrypt(alay_combination(original_name), encryption_key, 100) 
    fake_biodata[2] = xor_encrypt_decrypt(fake_biodata[2], encryption_key, 100) 
    fake_biodata[5] = xor_encrypt_decrypt(fake_biodata[5], encryption_key, 200)
    fake_biodata[6] = xor_encrypt_decrypt(fake_biodata[6], encryption_key, 200) 
    fake_biodata[7] = xor_encrypt_decrypt(fake_biodata[7], encryption_key, 50)
    fake_biodata[9] = xor_encrypt_decrypt(fake_biodata[9], encryption_key, 100) 
    fake_biodata[10] = xor_encrypt_decrypt(fake_biodata[10], encryption_key, 50)

    encrypted_date = mod_encrypt_date(fake_biodata[3], encryption_key)
    fake_biodata[3] = encrypted_date

    cursor.execute(sql_biodata, fake_biodata)
    
    for j in range(10):
        if k >= len(files):
            k = 0 
        sql_sidik_jari = "INSERT INTO sidik_jari (berkas_citra, nama) VALUES (%s, %s)"
        fake_sidik = (xor_encrypt_decrypt("../test/dataset/" + files[k], encryption_key, 200), xor_encrypt_decrypt(original_name, encryption_key, 100))  # Encrypt name
        cursor.execute(sql_sidik_jari, fake_sidik)
        k += 1

conn.commit()
cursor.close()
conn.close()