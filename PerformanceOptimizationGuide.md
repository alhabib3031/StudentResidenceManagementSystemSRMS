# دليل تحسين أداء استعلامات قواعد البيانات (.NET & EF Core)

تم إعداد هذا الدليل بناءً على تحليل الأخطاء الحالية (SqlException: Insufficient system memory)، لتوضيح المفاهيم التي تسبب بطء النظام واستهلاك الذاكرة، وكيفية تجنبها.

## 1. التقييم في الذاكرة مقابل التقييم في قاعدة البيانات
**Client-side Evaluation vs Server-side Evaluation**

### المشكلة:
عندما تقوم بجلب جدول كامل من قاعدة البيانات (مثلاً `GetAllAsync()`) ثم تقوم بعمل `Where` أو `Select` أو `Join` في كود الـ C#، فإنك تجبر السيرفر على:
1. نقل كمية ضخمة من البيانات عبر الشبكة.
2. حجز مساحة ضخمة في الذاكرة (RAM) لتخزين هذه البيانات.
3. معالجة البيانات ببطء مقارنة بمحرك SQL المخصص لذلك.

وهذا هو السبب الرئيسي لرسالة الخطأ: `Insufficient system memory`.

### الحل الصحيح (Projection):
يجب إرسال الاستعلام "مفصلاً" لقاعدة البيانات لترجع فقط البيانات المطلوبة.
```csharp
// ❌ خطأ (يقتل الذاكرة)
var allStudents = await _repo.GetAllAsync(); // يجلب 1000 طالب بكل بياناتهم
var names = allStudents.Select(s => s.Name).ToList();

// ✅ صح (سريع جداً)
var names = await _context.Students
    .Select(s => s.Name) // SQL ينفذ: SELECT Name FROM Students
    .ToListAsync();
```

### مصادر للتعلم:
- [Client vs. Server Evaluation (Microsoft Docs)](https://learn.microsoft.com/en-us/ef/core/querying/client-eval)
- [EF Core Query Performance](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying)

---

## 2. مشكلة N+1 (The N+1 Problem)

### المشكلة:
تحدث عندما تجلب قائمة (مثلاً 10 شكاوى)، ثم تقوم بعمل استعلام جديد لكل عنصر لجلب بيانات مرتبطة (مثلاً اسم الطالب).
- استعلام 1 لجلب الشكاوى.
- 10 استعلامات لجلب الطلاب.
المجموع: 11 استعلام. لو كان عندك 1000 شكوى، سيصبح 1001 استعلام!

### الحل (Eager Loading):
استخدام `Include` لجلب البيانات المرتبطة في استعلام واحد.
```csharp
// ❌ خطأ
foreach (var complaint in complaints) {
    complaint.Student = _repo.GetStudent(complaint.StudentId);
}

// ✅ صح
var complaints = _context.Complaints.Include(c => c.Student).ToList();
```

### مصادر للتعلم:
- [N+1 Problem in EF Core](https://code-maze.com/entity-framework-core-n-plus-one-problem/)

---

## 3. تتبع الكائنات (Tracking vs NoTracking)

### المشكلة:
بشكل افتراضي، EF Core يقوم بمراقبة أي كائن تقوم بجلبه لتتمكن من تعديله وحفظه (`SaveChanges`). هذه المراقبة تستهلك ذاكرة ووقت معالجة. في صفحات العرض (القراءة فقط)، هذه المراقبة عبء لا فائدة منه.

### الحل:
استخدام `.AsNoTracking()` مع أي استعلام للقراءة فقط.
```csharp
// ✅ للقراءة فقط
var users = await _context.Users.AsNoTracking().ToListAsync();
```

### مصادر للتعلم:
- [Tracking vs. No-Tracking Queries](https://learn.microsoft.com/en-us/ef/core/querying/tracking)

---

## 4. نصيحة معمارية: نمط المستودع (Repository Pattern)

في Clean Architecture، استخدام `GetAllAsync` التي ترجع `List` أو `IEnumerable` غالباً ما يكون فخاً للأداء.
الحل الأفضل هو أن يدعم المستودع `IQueryable` أو استخدام مواصفات (Specifications) أو إنشاء "Read Services" منفصلة تستخدم `Dapper` أو `EF Core` بذكاء لعرض البيانات.

