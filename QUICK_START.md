# Quick Start - نظام حجز الغرف

## التغييرات الرئيسية

### 1. الصفحات الجديدة
- ✅ `VacantRoomsDialog.razor` - عرض الغرف الشاغرة
- ✅ `BookingConfirmationDialog.razor` - تأكيد الحجز والدفع

### 2. الصفحات المحدثة
- ✅ `MyRoom.razor` - صفحة عرض المساكن (محدثة بالكامل)

### 3. Backend Updates

#### DTOs:
- ✅ `ResidenceDto.cs` → أضيف `MaxRoomsCount`

#### Handlers:
- ✅ `CreateRoomCommandHandler.cs` → أضيف `MonthlyRent` في الإنشاء
- ✅ `UpdateRoomCommandHandler.cs` → أضيف `MonthlyRent` في التحديث
- ✅ `GetAllRoomsQueryHandler.cs` → أضيف `MonthlyRent` في الإرجاع

#### Services:
- ✅ `ReservationService.cs` → تحديث `AvailableCapacity` عند الحجز
- ✅ `MapsterConfiguration.cs` → إضافة mappings للـ DTOs

## الميزات المنفذة

### ✅ 1. عرض المساكن مع نسبة الإشغال
- عرض العدد الحالي والأقصى للغرف (مثل: 5/10)
- عرض المقاعد المتاحة/الإجمالي
- شريط تقدم ملون حسب نسبة الإشغال
- حالة التوفر (متاح/ممتلئ)

### ✅ 2. مربع حوار الغرف الشاغرة
- عرض الغرف الشاغرة فقط
- عرض الغرف التي لها سعر محدد فقط
- تنبيه إذا لم يتم تحديد السعر من قبل الأدمن

### ✅ 3. نظام الحجز والدفع الوهمي
- اختيار فترة الحجز
- حساب التكلفة الإجمالية تلقائياً
- نموذج دفع وهمي (رقم البطاقة، CVV، تاريخ الانتهاء)
- معالجة الحجز والدفع
- عرض رقم الحجز والدفع عند النجاح

### ✅ 4. تحديد سعر الغرفة من قبل الأدمن
- يجب على الأدمن تحديد `MonthlyRentAmount` و `MonthlyRentCurrency`
- الغرف بدون سعر لا تظهر في قائمة الحجز
- يتم استخدام السعر في حساب التكلفة الإجمالية

### ✅ 5. تحديث نسبة الإشغال
#### عند إضافة غرفة:
```
CurrentRoomsCount++
TotalCapacity += TotalBeds
AvailableCapacity += TotalBeds
```

#### عند حذف غرفة:
```
CurrentRoomsCount--
TotalCapacity -= TotalBeds
AvailableCapacity -= TotalBeds
```

#### عند الحجز:
```
room.OccupiedBeds++
residence.AvailableCapacity--
```

## كيفية الاختبار

1. انتقل إلى `/student/room`
2. شاهد قائمة المساكن مع نسبة الإشغال
3. اضغط على "عرض الغرف المتاحة" لسكن معين
4. اختر غرفة (يجب أن يكون لها سعر محدد)
5. حدد تاريخ البداية والنهاية
6. انظر إلى إجمالي التكلفة المحسوبة
7. أدخل بيانات بطاقة وهمية:
   - رقم البطاقة: 1234 5678 9012 3456
   - تاريخ الانتهاء: 12/25
   - CVV: 123
8. اضغط "تأكيد الحجز والدفع"
9. شاهد رسالة النجاح مع رقم الحجز والدفع

## ملاحظات مهمة

⚠️ **الغرف بدون سعر لن تظهر للحجز**
⚠️ **نظام الدفع وهمي - أي بيانات ستعمل**
⚠️ **تأكد من تحديد سعر الغرفة عند الإنشاء/التعديل**

## التأكد من السعر

عند إنشاء غرفة جديدة في الإدارة:
1. افتح نموذج إنشاء الغرفة
2. تأكد من ملء `Monthly Rent Amount` (مثل: 500)
3. تأكد من ملء `Monthly Rent Currency` (مثل: LYD)
4. احفظ الغرفة

الآن ستظهر الغرفة في قائمة الغرف المتاحة للطلاب!

## البنية

```
MyRoom.razor
    ↓ (عند الضغط على "عرض الغرف المتاحة")
VacantRoomsDialog.razor
    ↓ (عند الضغط على "احجز الآن")
BookingConfirmationDialog.razor
    ↓ (عند الضغط على "تأكيد الحجز والدفع")
ReservationService.ReserveRoomAsync()
    ↓
DummyPaymentService.ProcessDummyPaymentAsync()
    ↓
Create Reservation + Update Room/Residence
    ↓
Success!
```

## الملفات المهمة

**Frontend:**
- `/Components/Pages/Students/MyRoom.razor`
- `/Components/Pages/Dialogs/VacantRoomsDialog.razor`
- `/Components/Pages/Dialogs/BookingConfirmationDialog.razor`

**Backend:**
- `/SRMS.Application/Residences/DTOs/ResidenceDto.cs`
- `/SRMS.Application/Rooms/CreateRoom/CreateRoomCommandHandler.cs`
- `/SRMS.Application/Rooms/UpdateRoom/UpdateRoomCommandHandler.cs`
- `/SRMS.Infrastructure/Configurations/Services/ReservationService.cs`
- `/SRMS.Infrastructure/Configurations/Services/DummyPaymentService.cs`
