# نشر "Arabic Street Names" على Paradox Mods

> English summary at the end.

- المشروع: `ArabicStreetNames/ArabicStreetNames.csproj`.
- ملف النشر: `ArabicStreetNames/Properties/PublishConfiguration.xml`.
- الصورة المصغّرة الجديدة: `ArabicStreetNames/Properties/Thumbnail.png` (1024×1024، طريق عليه "طريق الملك فهد" بحروف متصلة).
- ملفات النشر (Publish Profiles) أُضيفت في `ArabicStreetNames/Properties/PublishProfiles/`.

## 1. لقطات الشاشة المطلوبة

| الملف | ماذا تُظهر |
|---|---|
| `ArabicStreetNames/Properties/Screenshot1.png` | منظر علوي لمدينة فيها عدة طرق بأسماء عربية على الخريطة (مثل: طريق الملك فهد، شارع العليا) بحروف متصلة واضحة. |
| `ArabicStreetNames/Properties/Screenshot2.png` | مقارنة "قبل / بعد": نفس الطريق والمود متوقف (حروف منفصلة معكوسة) ثم والمود يعمل. يمكنك دمج الصورتين جنباً إلى جنب في صورة واحدة. |
| `ArabicStreetNames/Properties/Screenshot3.png` | أسماء أحياء عربية، واسم يجمع العربي مع أرقام أو كلمة إنجليزية (مثل: طريق 40 أو شارع King Road) لإظهار الاتجاه الصحيح. |
| `ArabicStreetNames/Properties/Screenshot4.png` | صفحة إعدادات المود: التشغيل، التشكيل، المسافة بين الكلمات، حجم الحروف، الخط البديل وحالة الخط. |

نصائح:
- الدقة المقترحة **1920×1080** بصيغة PNG، وحجم كل صورة يُفضّل أن يكون أقل من 2–3 ميجابايت تقريباً.
- قرّب الكاميرا بما يكفي لقراءة الأسماء بسهولة.
- إن غيّرت عدد الصور فعدّل أسطر `<Screenshot>` في ملف النشر لتطابق الملفات الموجودة.

## 2. قبل النشر (قائمة التحقق)

- [ ] لا يوجد `ModId` في ملف النشر (المود جديد؛ المعرّف يُعطى عند أول نشر).
- [ ] `ModVersion` = `1.0.0` (يُستخدم أيضاً كرقم إصدار التجميعة الظاهر في الإعدادات) و `GameVersion` = `1.6.*`.
- [ ] `AccessLevel` = `Public` (أو `Unlisted` / `Private` / `Hidden` للتجربة).
- [ ] الصور الأربع موجودة.
- [ ] الملف `0Harmony.dll` (Harmony 2.3.3، رخصة MIT) يُنسخ مع المود — تأكد من وجوده في مجلد المود المحلي بعد البناء.
- [ ] جرّب في **لعبة نظيفة**: مدينة جديدة، سمِّ طرقاً وأحياء بالعربي، غيّر الإعدادات وتأكد أن الأسماء تُعاد رسمها، ثم احفظ وأعد التحميل، وجرّب إزالة المود والتأكد أن المدينة تفتح بشكل طبيعي.
- [ ] جرّبه أيضاً مع مود آخر يستخدم Harmony للتأكد من عدم التعارض.
- [ ] البناء بإعداد Release بدون أخطاء: `dotnet build ArabicStreetNames/ArabicStreetNames.csproj -c Release`.

## 3. تسجيل الدخول بحساب Paradox

أداة النشر تستخدم حساب Paradox المسجّل في اللعبة تلقائياً: شغّل اللعبة وسجّل الدخول مرة واحدة على الأقل ثم أغلقها. ويجب أن تكون أدوات التطوير مثبّتة من الإعدادات ← Modding.

## 4. أوامر النشر

```powershell
cd D:\CitiesSkylinesMegaMod\ArabicStreetNames\ArabicStreetNames

# أول نشر
dotnet publish ArabicStreetNames.csproj -c Release /p:PublishProfile=PublishNewMod

# إصدار جديد لاحقاً (ارفع ModVersion وحدّث ChangeLog)
dotnet publish ArabicStreetNames.csproj -c Release /p:PublishProfile=PublishNewVersion

# تحديث الوصف/الصور فقط
dotnet publish ArabicStreetNames.csproj -c Release /p:PublishProfile=UpdatePublishedConfiguration
```

بعد أول نشر ناجح تظهر رسالة `Mod published with Id=...` — تأكد أن `<ModId Value="الرقم" />` موجود في ملف النشر قبل الإصدارات اللاحقة.

---

## English summary

- Screenshots (1920×1080 PNG): `Properties/Screenshot1.png` city overview with Arabic road names; `Screenshot2.png` before/after (mod off vs on); `Screenshot3.png` district names and a name mixing Arabic with numbers/Latin; `Screenshot4.png` the options page.
- Checklist: no ModId on first publish, ModVersion 1.0.0, GameVersion 1.6.*, AccessLevel, 0Harmony.dll shipped, tested in a clean game and with another Harmony mod, safe removal checked.
- Log in to Paradox in the game once, then from `ArabicStreetNames/`: `dotnet publish ArabicStreetNames.csproj -c Release /p:PublishProfile=PublishNewMod` (first), `PublishNewVersion` (updates), `UpdatePublishedConfiguration` (metadata only). Publish profiles were added under `Properties/PublishProfiles`.
