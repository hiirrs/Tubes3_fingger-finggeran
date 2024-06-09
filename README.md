# Fingerprint Detector

Repositori ini berisi kode untuk sistem deteksi individu berbasis biometrik melalui citra sidik jari dengan memanfaatkan pattern matching. Pattern matching yang dilakukan pada aplikasi ini memanfaatkan Algoritma Knuth Morris Pratt (KMP), Algoritma Boyer Moore (BM), dan Ekspresi Reguler (Regex).

## Penjelasan Singkat Implementasi

### Strategi Pencocokan
Dalam pencocokan sidik jari, pertama-tama, sidik jari dikonversi menjadi rangkaian bit terlebih dahulu. Rangkaian bit tersebut kemudian akan dikonversi menjadi karakter ASCII. Konversi bit menjadi ASCII dilakukan dengan mengambil bit pada suatu titik dan tujuh bit di bawahnya untuk dikonversi menjadi satu karakter ASCII. Sebagai contoh, hasil konversi 100 x 100 karakter bit akan berubah menjadi 93 x 100 karakter ASCII.

Setelah sidik jari masukan dikonversi menjadi ASCII, masukan akan dibandingkan dengan sidik jari pada basis data. Sidik jari pada basis data akan dikonversi menjadi ASCII juga. Untuk membandingkan kedua citra, 30 karakter ASCII pada bagian tengah sidik jari masukan dibandingkan dengan seluruh ASCII pada setiap sidik jari pada basis data dengan menggunakan algoritma BM atau KMP. Jika tidak ada citra yang cocok secara 100%, citra yang memiliki persentase kemiripan tertinggi (dihitung dengan Levenshtein Distance) akan ditampilkan jika melewati ambang batas tertentu.

### Algoritma Knuth Morris Pratt (KMP)
Algoritma Knuth-Morris-Pratt (KMP) adalah algoritma pencocokan string yang digunakan untuk menemukan pola dalam teks. Algoritma ini menghindari perbandingan yang tidak perlu dengan cara memanfaatkan informasi dari perbandingan sebelumnya. Proses pencocokan dilakukan dari kiri ke kanan, mirip dengan algoritma brute force, tetapi dengan pergeseran yang lebih baik saat terjadi ketidaksesuaian atau mismatch antara teks dan pola. Algoritma KMP menggunakan fungsi pinggiran (border function) yang disebut juga dengan fungsi gagal (failure function) untuk menentukan seberapa jauh pola dapat digeser saat terjadi ketidaksesuaian. Fungsi ini memproses pola untuk menemukan kecocokan antara awalan (prefix) dan akhiran (suffix) pola itu sendiri.

### Algoritma Boyer Moore (BM)
Algoritma Boyer-Moore (BM) adalah algoritma pencocokan string yang berjalan lebih efisien pada teks dengan alfabet besar. Algoritma ini menggunakan dua teknik utama: teknik kaca spion (looking-glass technique) dan teknik loncatan karakter (character-jump technique). Teknik kaca spion melakukan pencocokan dari belakang ke depan pada pola, dimulai dari karakter terakhir. Teknik loncatan karakter menggeser pola ke kanan berdasarkan kemunculan terakhir karakter yang tidak cocok dalam pola. Fungsi kemunculan terakhir (last occurrence function) dibangun selama preprocessing untuk setiap karakter dalam alfabet, memetakan indeks kemunculan terakhir karakter tersebut dalam pola.

### Ekspresi Reguler (Regex)
Algoritma Regex (Regular Expressions) adalah teknik untuk pencocokan pola menggunakan ekspresi reguler. Ekspresi reguler adalah urutan karakter yang mendefinisikan pola pencarian. Algoritma ini digunakan secara luas dalam berbagai aplikasi seperti editor teks, mesin pencari web, dan analisis data. Algoritma regex bekerja dengan membangun mesin keadaan hingga deterministik (DFA) atau nondeterministik (NFA) yang memproses teks untuk mencari pola yang sesuai dengan ekspresi reguler. Kompleksitas waktu pencocokan pola dengan ekspresi reguler tergantung pada implementasi spesifik dari mesin keadaan dan panjang serta kerumitan ekspresi reguler. Regex sangat fleksibel dan kuat, tetapi bisa menjadi kurang efisien untuk pola yang sangat kompleks atau teks yang sangat panjang. Pada aplikasi ini, Regex dimanfaatkan untuk mencocokkan biodata yang korup. Biodata korup tersebut berada dalam bentuk nama yang penulisannya "dialaykan". Penulisan nama "alay" dapat berupa penggunaan huruf kapital dan kecil, penggantian huruf dengan angka, dan penyingkatan kata.

## Program Requirements
1. .NET (Menjalankan program utama)
2. Python 3.11.9 (Memuat database)
3. python-dotenv (Mengatur environment variables di Python, jalankan perintah pip install python-dotenv untuk menginstall)
4. DotNetEnv (Mengatur environment variables di C#)

## Tata Cara Kompilasi dan Menjalankan Program
1. Clone repositori ini

2. Pada root folder, tambahkan file .env yang berisi variabel berikut:
```env
DB_SERVER=localhost # Ganti dengan nama server pada perangkat Anda
DB_USER=root # Ganti dengan nama pengguna pada perangkat Anda
DB_PASSWORD=12345 # Ganti dengan password pada perangkat Anda
DB_SOURCE=fingerprint_original # Nama basis data default untuk fingerprint yang belum dienkripsi
ENCRYPTION_KEY=129 # Kunci enkripsi/dekripsi data
```

3. Pindah ke direktori DatabaseLoader:
```powershell
cd DatabaseLoader
```

4. Jalankan perintah berikut untuk menginisialisasi skema basis data:
```powershell
dotnet run
```

5. Jika basis data sidik jari belum tersedia, jalankan perintah berikut untuk membuat mock data:
```powershell
python OriginalMockGenerator.py
```

6. Jalankan perintah berikut untuk mengenkripsi basis data:
```powershell
python SchemaConverter.py
```

7. Pindah ke direktori src:
```powershell
cd ../src
```

8. Jalankan perintah berikut untuk menjalankan program:
```powershell
dotnet run
```

9. Tampilan utama program akan muncul. Masukkan sidik jari yang ingin dicocokkan dan algoritma yang ingin digunakan. Setelah itu, tekan tombol Search untuk mendapatkan hasil pencocokan.


## Identitas Pembuat
<table>
    <tr>
        <td>No.</td>
        <td>Nama</td>
        <td>NIM</td>
    </tr>
    <tr>
        <td>1.</td>
        <td>Shazya Audrea Taufik</td>
        <td>13522063</td>
    </tr>
    <tr>
        <td>2.</td>
        <td>Zahira Dina Amalia</td>
        <td>13522085</td>
    </tr>
    <tr>
        <td>3.</td>
        <td>Muhammad Neo Cicero Koda</td>
        <td>13522108</td>
    </tr>
</table>
