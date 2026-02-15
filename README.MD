# Quik-R (Quick Reply Balloon)

## ไทย

### ภาพรวม
Quik-R คือโปรแกรม WPF สำหรับข้อความตอบกลับด่วนแบบบอลลูนลอย (Floating Balloons) บน Windows

ฟีเจอร์หลัก:
- Header ลอยด้านบนสุด
- บอลลูนข้อความเรียงลงล่างและเลื่อน (scroll) ได้
- ระบบ Aim เลือกหน้าต่างเป้าหมายเพื่อวางข้อความ
- ปุ่มวางทันที (`คลิกข้อความ`) และปุ่มวาง+ส่ง (`➤`)
- บันทึกข้อความ, ชื่อแสดงผล, สี, ความโปร่งใส ต่อบอลลูน
- System Tray (Open / Exit)

---

### ความต้องการระบบ
- Windows 10/11
- .NET SDK 8.0 (สำหรับ build จาก source)

---

### Build เป็น .exe (Portable)
รันที่โฟลเดอร์โปรเจกต์:

```powershell
Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" -ArgumentList 'publish "c:\Users\BDC\Desktop\QuickReply\QuickReply.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true' -Wait -NoNewWindow
```

ไฟล์ผลลัพธ์:
- `bin\Release\net8.0-windows\win-x64\publish\QuickReply.exe`

---

### Setup (ติดตั้ง)
มีตัวติดตั้งแบบ Wizard แล้ว:
- `dist\Quik-R-Setup.exe`

ถ้าต้องการ build ตัวติดตั้งใหม่:
- `setup\Build-Setup.ps1`

ไฟล์สคริปต์ installer:
- `setup\QuikR.iss`

ตัวเลือกเดิม (script install):
มีสคริปต์ติดตั้งให้แล้ว:
- `setup\Setup-QuikR.bat` (ดับเบิลคลิกได้)
- หรือ `setup\install.ps1`

สิ่งที่ setup ทำ:
- คัดลอกไฟล์ไป `LocalAppData\Programs\Quik-R`
- สร้าง Shortcut ที่ Desktop และ Start Menu

---

### Uninstall (ถอนการติดตั้ง)
รัน:
- `setup\uninstall.ps1`

สคริปต์จะ:
- ปิดโปรแกรม Quik-R (ถ้ากำลังเปิด)
- ลบไฟล์ติดตั้ง
- ลบ Desktop/Start Menu shortcuts

---

### ไอคอนโปรแกรม
ไฟล์ไอคอนหลัก:
- `Assets\app.ico`

ตอน build จะถูกใช้เป็น:
- ไอคอนของไฟล์ EXE
- ไอคอนใน System Tray

---

### อัปขึ้น GitHub (Checklist)
1) ติดตั้ง Git for Windows: https://git-scm.com/download/win
2) สร้าง repository ใหม่บน GitHub (เช่น `QuickReply`)
3) ที่โฟลเดอร์โปรเจกต์ รันคำสั่ง:

```powershell
git init
git add .
git commit -m "release: v1.0.0"
git branch -M main
git remote add origin https://github.com/<your-user>/<your-repo>.git
git push -u origin main
```

4) สร้าง GitHub Release (v1.0.0) และแนบไฟล์:
- `dist\Quik-R-Setup.exe`
- `bin\Release\net8.0-windows\win-x64\publish\QuickReply.exe`
- ใช้ข้อความจาก `RELEASE_NOTES_v1.0.0.md`

---

## English

### Overview
Quik-R is a WPF floating quick-reply app for Windows.

Key features:
- Always-on-top header
- Stacked balloon messages with scrolling
- Aim mode to select target window/chat input
- Quick paste (`click message`) and paste+send (`➤`)
- Per-balloon saved message, display name, color, and opacity
- System tray controls (Open / Exit)

---

### Requirements
- Windows 10/11
- .NET SDK 8.0 (for source build)

---

### Build .exe (Portable)
Run in project folder:

```powershell
Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" -ArgumentList 'publish "c:\Users\BDC\Desktop\QuickReply\QuickReply.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true' -Wait -NoNewWindow
```

Output:
- `bin\Release\net8.0-windows\win-x64\publish\QuickReply.exe`

---

### Setup (Install)
Wizard installer is now available:
- `dist\Quik-R-Setup.exe`

To rebuild installer:
- `setup\Build-Setup.ps1`

Installer script file:
- `setup\QuikR.iss`

Legacy script-based install is still available:
Included setup scripts:
- `setup\Setup-QuikR.bat` (double-click)
- or `setup\install.ps1`

Setup actions:
- Copies app files to `LocalAppData\Programs\Quik-R`
- Creates Desktop and Start Menu shortcuts

---

### Uninstall
Run:
- `setup\uninstall.ps1`

This script:
- Stops Quik-R process if running
- Removes installed files
- Removes Desktop/Start Menu shortcuts

---

### App Icon
Primary icon file:
- `Assets\app.ico`

Used for:
- EXE application icon
- System tray icon

---

### GitHub Upload (Checklist)
1) Install Git for Windows: https://git-scm.com/download/win
2) Create a new GitHub repository (e.g. `QuickReply`)
3) Run in project folder:

```powershell
git init
git add .
git commit -m "release: v1.0.0"
git branch -M main
git remote add origin https://github.com/<your-user>/<your-repo>.git
git push -u origin main
```

4) Create GitHub Release `v1.0.0` and upload assets:
- `dist\Quik-R-Setup.exe`
- `bin\Release\net8.0-windows\win-x64\publish\QuickReply.exe`
- Release notes from `RELEASE_NOTES_v1.0.0.md`
