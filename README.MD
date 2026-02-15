# Quik-R (Quick Reply Balloon)

วันที่อัปเดต / Updated: 2026-02-15

## ไทย

### แนะนำโปรแกรม
Quik-R เป็นโปรแกรมสำหรับส่งข้อความตอบกลับแบบกึ่งอัตโนมัติ (Quick Reply) เหมาะสำหรับใช้งานกับไลน์ OA โดยภายในโปรแกรมจะมี “บอลลูนข้อความ” ที่บันทึกข้อความไว้ล่วงหน้า

• เมื่อคลิกที่บอลลูน ระบบจะวางข้อความที่บันทึกไว้ลงในช่องแชทโดยอัตโนมัติ  
• หากคลิกที่ปุ่ม ➤ ระบบจะวางข้อความและส่งทันที

หมายเหตุ: ก่อนเริ่มใช้งานทุกครั้ง จำเป็นต้องกดปุ่ม ◎ เพื่อกำหนดตำแหน่ง (เล็งเป้า) ไปที่ช่องแชทก่อน เพื่อให้โปรแกรมทราบตำแหน่งช่องแชทบนหน้าจอและทำงานได้อย่างถูกต้อง

### ฟีเจอร์หลัก
- บอลลูนข้อความลอย (Floating balloons)
- Header ลอยบนสุด พร้อมปุ่มเพิ่ม, ค้นหา, Aim, ย่อเข้า Tray, ปิดโปรแกรม
- รองรับลากสลับลำดับบอลลูนด้วยปุ่ม `☰`
- ปรับชื่อแสดงผล, สี, ความโปร่งใส ได้ในหน้าต่าง Edit
- ค้นหาได้ทั้งชื่อแสดงผลและเนื้อหาข้อความ
- บันทึกข้อมูลถาวร (ข้อความ/ชื่อ/สี/ความโปร่งใส/ลำดับ)
- System Tray menu: Open / Exit
- ถ้าเปิดโปรแกรมซ้ำ ระบบจะไม่เปิดซ้ำ และจะดึงหน้าต่างเดิมขึ้นด้านหน้าอัตโนมัติ

### ความต้องการระบบ
- Windows 10/11
- .NET SDK 8.0 (สำหรับการ build จาก source)

### วิธีรันจาก source
```powershell
Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" -ArgumentList 'run --project "c:\Users\BDC\Desktop\QuickReply\QuickReply.csproj"' -Wait -NoNewWindow
```

### Build เป็นไฟล์ .exe (Portable)
```powershell
Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" -ArgumentList 'publish "c:\Users\BDC\Desktop\QuickReply\QuickReply.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true' -Wait -NoNewWindow
```

ผลลัพธ์:
- `bin\Release\net8.0-windows\win-x64\publish\QuickReply.exe`

### Setup แบบ Wizard (แนะนำ)
- ไฟล์ติดตั้ง: `dist\Quik-R-Setup.exe`
- ใช้งาน: ดับเบิลคลิกเพื่อติดตั้งแบบ Wizard

### สร้าง Setup.exe ใหม่
```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "setup\Build-Setup.ps1"
```

### Uninstall
- ถ้าติดตั้งผ่าน Wizard: ใช้ Apps & Features ของ Windows หรือ Uninstall จาก Start Menu
- ถ้าใช้ script setup เดิม: รัน `setup\uninstall.ps1`

### ไอคอนโปรแกรม
- ไฟล์หลัก: `Assets\app.ico`
- ใช้เป็นไอคอน EXE และไอคอนใน System Tray

---

## English

### About
Quik-R is a semi-automatic quick reply app designed for LINE OA. It provides pre-saved message balloons for faster chat responses.

• Click a balloon to paste the saved message into the chat box automatically.  
• Click ➤ to paste and send immediately.

Note: Before using Quik-R, click ◎ to set (aim) the target chat input location, so the app knows where to focus and paste.

### Key Features
- Floating quick-reply balloons
- Always-on-top header with Add, Search, Aim, Collapse-to-Tray, and Close
- Reorder balloons with `☰`
- Edit display name, color, and opacity in the Edit popup
- Search by both display name and message content
- Persistent balloon data (text/name/style/order)
- System tray menu: Open / Exit
- Single-instance behavior: launching again brings the existing window to front

### Requirements
- Windows 10/11
- .NET SDK 8.0 (for source build)

### Run from source
```powershell
Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" -ArgumentList 'run --project "c:\Users\BDC\Desktop\QuickReply\QuickReply.csproj"' -Wait -NoNewWindow
```

### Build portable .exe
```powershell
Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" -ArgumentList 'publish "c:\Users\BDC\Desktop\QuickReply\QuickReply.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true' -Wait -NoNewWindow
```

Output:
- `bin\Release\net8.0-windows\win-x64\publish\QuickReply.exe`

### Wizard Setup (recommended)
- Installer file: `dist\Quik-R-Setup.exe`
- Double-click to install via wizard

### Rebuild Setup.exe
```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "setup\Build-Setup.ps1"
```

### Uninstall
- If installed via Wizard: use Windows Apps & Features or Start Menu uninstall entry
- If installed via legacy script: run `setup\uninstall.ps1`

### App Icon
- Main icon file: `Assets\app.ico`
- Used for EXE icon and system tray icon
