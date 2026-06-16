<h1 align="center">🔍 Revit Visibility Diagnostic (R24)</h1>

<p align="center">
  <strong>เครื่องมือวินิจฉัยแบบ Real-time สุดทรงพลังสำหรับ Autodesk Revit 2024 ช่วยค้นหาสาเหตุว่าทำไมโมเดลถึงมองไม่เห็นใน View</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Revit%20API-2024-blue?style=for-the-badge&logo=autodesk" alt="Revit 2024">
  <img src="https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=c-sharp" alt="C#">
  <img src="https://img.shields.io/badge/UI-WPF-512BD4?style=for-the-badge" alt="WPF">
</p>

---

## 💡 ภาพรวม (Overview)
เคยสงสัยไหมครับว่า *"ทำไมฉันถึงมองไม่เห็นโมเดลชิ้นนี้ใน View?"* 
ปลั๊กอิน **Visibility Diagnostic** คือผู้ช่วยที่จะมาประหยัดเวลาของคุณ! ด้วยหน้าต่างวิเคราะห์แบบ Modeless ที่เป็นมิตรต่อผู้ใช้งาน คุณสามารถตรวจสอบสาเหตุทั้งหมดที่ทำให้โมเดลหายไปใน View นั้นๆ ได้แบบ Real-time 

บอกลาการคลิกสุ่มหาใน Visibility/Graphics (VV), View Ranges หรือ Filters แบบเดิมๆ ไปได้เลย!

## ✨ ฟีเจอร์หลัก (Key Features)
- 🚀 **วิเคราะห์แบบ Real-time:** ใช้งานผ่านหน้าต่าง WPF แบบ Modeless สามารถเปิดค้างไว้ระหว่างทำงานและคลิกเช็คโมเดลต่างๆ ได้ตลอดเวลา
- 🎯 **ค้นหา View ที่ต้องการได้ทันที:** มีช่อง ComboBox ที่รองรับระบบพิมพ์ค้นหา (Filter-as-you-type) ช่วยให้เข้าถึง View ที่ต้องการได้อย่างรวดเร็ว
- 🚦 **แสดงผลลัพธ์ด้วยสีสันชัดเจน:** อ่านค่าสถานะได้ทันที (🟢 สีเขียว = แสดงผลปกติ, 🔴 สีแดง = ถูกซ่อน/ถูกบล็อก)
- 🧩 **ตรวจสอบครอบคลุมทุกจุด:** เช็คเงื่อนไขเหล่านี้ให้อัตโนมัติ:
  - โมเดลถูกสั่งซ่อนถาวรอยู่หรือไม่? (`Hide in View`)
  - หมวดหมู่ (Category) หรือ Subcategory ถูกซ่อนไว้หรือไม่?
  - โมเดลหลุดกรอบ Crop Region ไปหรือเปล่า?
  - Workset ถูกปิด หรือถูกซ่อนใน View นี้หรือไม่?
  - โดน View Filters ทับซ้อนจนมองไม่เห็นหรือไม่?
  - การตั้งค่า Phase ส่งผลต่อการมองเห็นไหม?
  - หลุดระยะ View Range ไปหรือไม่? (สำหรับมุมมองแปลน)

## 🛠️ การติดตั้ง (Installation)
1. Build โปรเจกต์ผ่าน Visual Studio
2. คัดลอกไฟล์ `RevitVisibilityDiagnostic_R24.dll` ไปยังโฟลเดอร์ Add-ins ของ Revit
3. คัดลอกไฟล์ `RevitVisibilityDiagnostic_R24.addin` ไปที่โฟลเดอร์เดียวกัน:
   `%APPDATA%\Autodesk\Revit\Addins\2024`
4. ตรวจสอบให้แน่ใจว่าพาธ `<Assembly>` ในไฟล์ `.addin` ชี้ไปยังตำแหน่งไฟล์ `.dll` ของคุณอย่างถูกต้อง
5. ปิดแล้วเปิดโปรแกรม Revit ใหม่อีกครั้ง

## 🚀 วิธีใช้งาน (How to Use)
1. เปิด Autodesk Revit 2024 และเปิดโปรเจกต์ของคุณ
2. ไปที่แท็บ Ribbon: **`BIMTools`**
3. ในพาเนล **`Diagnostics`** ให้คลิกที่ปุ่ม **`Visibility Diagnostic R24`**
4. หน้าต่างเครื่องมือจะปรากฏขึ้น ให้กรอกรหัส **Element ID** ของโมเดลที่หายไป
5. พิมพ์ค้นหาและเลือก **Target View** ที่คุณกำลังเจอปัญหา
6. คลิกปุ่ม **`Check Visibility`** เพื่อดูสาเหตุที่แท้จริงว่าทำไมโมเดลถึงถูกซ่อนไว้!

## 🧑‍💻 ประวัติการพัฒนา (Development Log)
โปรเจกต์นี้มีไฟล์บันทึกการอัปเดตแบบละเอียด (`CODEX_UPDATE_LOG.md`) (ใช้เป็นการภายใน) เพื่อติดตามการสร้างฟีเจอร์, การแก้ไข UI และการตั้งค่าระบบต่างๆ ระหว่างการพัฒนา

---
*พัฒนาด้วย ❤️ เพื่อยกระดับการทำงานสาย BIM*
