from faker import Faker
import random
import os
import mysql.connector

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
    kewarganegaraan = random.choice(['Indonesia', 'France', 'Netherlands', 'Italy', 'USA', 'Malaysia', 'Singapore'])

    return [nik, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan]

def list_files(directory):
    files = [f for f in os.listdir(directory) if os.path.isfile(os.path.join(directory, f))]
    return files

directory_path = r'D:\Akademik\SEMESTER 4\STIMA\TUBES\Tubes3_Fingerfingeran\test\dataset'
files = list_files(directory_path)

conn = mysql.connector.connect(
    host="localhost",
    user="root",
    password="23)#)$",
    database="fingerprint_original"
)

cursor = conn.cursor()

k = 0
for i in range(1000):
    sql = "INSERT INTO biodata (NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"
    fake_biodata = generate_fake_data()
    original_name = fake_biodata[1]
    fake_biodata[1] = alay_combination(original_name)
    cursor.execute(sql, fake_biodata)
    if i < 600:
        for j in range(10):
            sql = "INSERT INTO sidik_jari (berkas_citra, nama) VALUES (%s, %s)"
            fake_sidik = ("../test/dataset/" + files[k], original_name)
            cursor.execute(sql, fake_sidik)
            k += 1

conn.commit()