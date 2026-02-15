# Quik-R v1.0.1

วันที่ปล่อย: 2026-02-15  
Release date: 2026-02-15

## ไทย

อัปเดตเวอร์ชันนี้เน้นแก้ไขความเสถียรในการทำงานจริง โดยเฉพาะการทำงานร่วมกับ System Tray และการคงข้อมูลบอลลูนหลังปิดโปรแกรม

### สิ่งที่เปลี่ยนแปลง
- แก้ปัญหาไอคอนใน System Tray ไม่แสดงผล
- ปรับพฤติกรรมปุ่มปิด (`X`) บน Header ให้ปิดโปรแกรมทั้งหมด รวมถึงปิดจาก Tray ด้วย
- แก้ปัญหาข้อมูลบอลลูนหายหลังปิดโปรแกรมและเปิดใหม่ (ข้อมูลเดิมจะยังคงอยู่)
- ป้องกันการเปิดโปรแกรมซ้ำแล้วเกิดไอคอน Tray ซ้ำหลายตัว

### หมายเหตุ
- รีลีสนี้เป็นการปรับปรุงคุณภาพและความเสถียร ไม่ได้เพิ่มฟีเจอร์ใหม่

---

## English

This update focuses on day-to-day reliability, especially System Tray behavior and balloon data persistence across restarts.

### Changes
- Fixed an issue where the System Tray icon did not appear
- Updated Header close (`X`) behavior to fully exit the app, including Tray instance
- Fixed balloon data being lost after closing and reopening the app
- Prevented duplicate Tray icons when launching the app multiple times

### Notes
- This release is focused on stability and quality improvements, with no new features added