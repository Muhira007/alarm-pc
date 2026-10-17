# Alarm Giliran — Imran & Lukman

Aplikasi Windows untuk membagi waktu bermain laptop secara bergantian dengan durasi yang sama.

## Cara memakai

1. Buka **AlarmGiliran.exe** di folder `dist`, atau shortcut **Alarm Giliran** di desktop.
2. Pilih **20 mnt** atau **30 mnt**. Untuk durasi lain, buka **Pengaturan**, isi **1–180 menit**, lalu **Simpan**.
3. Pilih anak yang bermain pertama melalui **Ganti pemain pertama**. Awalnya Imran.
4. Tekan **Mulai giliran**. Jendela boleh dikecilkan; alarm tetap berjalan.
5. Saat waktu habis, pop-up besar muncul di depan dengan teks **Saatnya giliran: Lukman** atau **Imran**, dan nada berulang.
6. Serahkan laptop, lalu tekan **Stop**. Nada berhenti, pop-up hilang, dan durasi penuh untuk anak berikutnya langsung dimulai. Proses ini terus berulang.
7. Untuk selesai, buka **Pengaturan → Keluar aplikasi**. Jika alarm sedang tampil, tekan Stop terlebih dahulu.

## Perilaku aplikasi

- Tidak ada tombol X di jendela utama atau pop-up. Alt+F4 dan perintah tutup biasa ditolak; keluar melalui Pengaturan.
- Pop-up hanya memiliki satu tombol: **Stop**. Waktu anak berikutnya mulai setelah Stop ditekan, sehingga waktu menunggu serah-terima tidak memotong gilirannya.
- Perubahan durasi saat sesi berjalan berlaku mulai giliran berikutnya. Giliran yang sedang berjalan tetap mendapat waktu semula.
- Durasi dan pilihan anak pertama tersimpan di `%LOCALAPPDATA%\AlarmGiliran\settings.json`.
- Membuka aplikasi lagi setelah keluar memulai sesi baru. Aplikasi tidak berjalan otomatis saat Windows dinyalakan.
- Waktu sleep ikut dihitung. Jika waktu habis saat laptop sleep, alarm muncul setelah laptop bangun; aplikasi tidak membangunkan laptop.
- Suara mengikuti volume Windows. Pastikan laptop tidak mute.
- Pop-up memakai jendela topmost. Untuk game dengan mode fullscreen eksklusif, gunakan mode **borderless/windowed** agar pengingat dapat tampil di depan game.
- Ini pengingat giliran, bukan pengunci Windows: proses tetap dapat dihentikan melalui Task Manager atau saat Windows dimatikan. Menu Pengaturan tidak memakai PIN.

## Menjalankan dan membangun

Paket `dist\AlarmGiliran.exe` mandiri untuk Windows x64, tanpa instalasi .NET tambahan, akses admin, atau internet saat digunakan. Simpan aplikasi pada lokasi tetap agar shortcut tidak terputus.

Kode sumber menggunakan C# / WPF dan .NET 10. Untuk pengembangan, pasang .NET 10 SDK di Windows:

```powershell
dotnet run --project AlarmGiliran.csproj
dotnet run --project tests/TimerChecks.csproj -c Release
dotnet run --project tests/UiChecks.csproj -c Release
.\Publish.ps1
```

Tes mencakup batas waktu, giliran berulang, penundaan menekan Stop, durasi setelah perubahan pengaturan, sleep melewati batas waktu, validasi input, tombol mulai/Stop, pencegahan tutup, serta penerimaan pemutaran suara oleh Windows. Tes komponen menghasilkan render tampilan di folder `artifacts`.

Pengujian melalui klik desktop langsung belum dilakukan karena koneksi alat computer-use tidak tersedia pada sesi pembuatan. Render UI dan pengujian komponen telah dilakukan; visibilitas di atas game tertentu dan suara yang terdengar perlu dicoba pada game serta perangkat audio yang digunakan.
