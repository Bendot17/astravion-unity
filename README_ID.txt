ASTRAVION — UNITY 3D PROJECT STARTER (V10)

Nama game: ASTRAVION
Tagline: LEGENDS OF ETERNAL WAR
Kredit: Developed by Suhendra
Target: Android, landscape
Unity: 6000.3.28f1 (Unity 6.3 LTS)

APA YANG ADA DI PROYEK INI
- Proyek Unity dengan scene arena dibuat otomatis saat Unity Editor mengimpor proyek.
- Arena 3D prototipe dengan tiga jalur, sungai, hutan, markas, menara, dan core.
- KAEL dibuat dari objek 3D dasar, kamera mengikuti pemain.
- HUD mobile, joystick sentuh, serang, Dash, Flame Arc, Ultimate Dragon Wrath.
- Bot sederhana dan minion lane.
- Pengaturan Android landscape dan ARM64.

PENTING — STATUS
Ini adalah fondasi/prototipe Unity 3D, BUKAN MOBA online 5v5 siap rilis.
Model KAEL, pohon, menara, dan naga masih dibuat dari primitive Unity; bukan model/animasi
profesional final. Bot dan minion masih AI sederhana. Multiplayer, matchmaking, login game,
server authoritative, anti-cheat, toko coin/diamond, pembelian Google Play, room custom,
spectator, turnamen dan wasit belum diimplementasikan di proyek Unity ini.
Jangan menganggap APK sudah dibuat: ZIP ini adalah sumber proyek Unity.

KARENA PENGEMBANGAN DILAKUKAN DARI HP ANDROID
Unity Editor resmi berjalan pada workstation/laptop Windows, macOS, atau Linux; bukan
editor penuh yang bisa dijalankan di HP Android. Jadi alur HP-only yang direncanakan:
1. Ekstrak ZIP.
2. Buat repositori GitHub BARU bernama astravion-unity (jangan menimpa repo lobby V8).
3. Unggah isi folder proyek ini ke ROOT repo: folder Assets, Packages, ProjectSettings.
   Pastikan ProjectSettings/ProjectVersion.txt ada di root repo.
4. Hubungkan repo ke Unity Dashboard > DevOps > Build Automation.
5. Pilih branch main, Unity version 6000.3.28f1 (atau versi yang persis sama), target Android.
6. Jalankan build cloud dan periksa hasil/log. Ketersediaan, batas pemakaian, dan biaya layanan
   cloud harus diperiksa di Dashboard sebelum menyetujui pembayaran/menambahkan metode bayar.

CATATAN CLOUD BUILD
Unity Build Automation dapat mengambil proyek dari GitHub dan membuat build Android di cloud.
Namun konfigurasi akun, hak akses repo, pengaturan target, dan kemungkinan biaya layanan tetap
harus diselesaikan di Unity Dashboard. Jangan bagikan password, OTP, token GitHub, atau kunci
penandatanganan Android ke chat.

JANGAN UNGGAH FOLDER Library, Temp, obj, atau Build ke GitHub. Folder tersebut dibuat Unity.
