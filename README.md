# Arabic Street Names

A Cities: Skylines II code mod that draws Arabic road and district names correctly on the map: connected letters, right-to-left order, numbers and Latin words kept in place.

[العربية](#أسماء-الشوارع-العربية)

## Why
The game draws map labels with TextMeshPro, which has no Arabic shaping or bidirectional ordering: Arabic names come out as separate letters in reverse order. The game's interface (Gameface) handles Arabic, so only the map labels need fixing.

## How it works
- `Patches/LabelNamePatch.cs`: a Harmony transpiler replaces only the two calls that hand a name to the map label renderer (`NameSystem.GetRenderedLabelName` in `AggregateMeshSystem.UpdateLabelVertices` and `AreaBufferSystem.UpdateLabelVertices`). Other users of that method (menus, panels, other mods) are untouched.
- `Text/ArabicShaper.cs`: joins letters into their initial, medial, final and isolated presentation forms, including the lam-alef ligatures; optional diacritics.
- `Text/ArabicBidi.cs`: run-level bidirectional ordering (based on UAX #9): Arabic runs right to left, numbers and Latin words left to right, brackets mirrored.
- `Text/ArabicText.cs`: a cache, so each name is processed once.
- `Patches/LabelLayout.cs`: word spacing and letter size through TextMeshPro rich-text tags (`<space>`, `<size>`); defaults 10% and 60% (glyphs overlap at full size).
- `Rendering/LabelFont.cs`: checks the game fonts for the Arabic presentation forms, otherwise adds a Windows font (Auto / Arial / Tahoma / Segoe UI) as a fallback.
- Ships Harmony 2.3.3 (the game loads one Harmony for all mods; an older copy breaks mods built on 2.3).

Nothing is saved into the city: safe to add or remove at any time.

## Building
Requirements: Cities: Skylines II with the official modding toolchain installed (it sets the `CSII_*` environment variables) and the .NET SDK.

```
dotnet build ArabicStreetNames/ArabicStreetNames.csproj -c Release
```

The build deploys the mod to the game's local `Mods/ArabicStreetNames` folder. Close the game first.

## License
MIT, see [LICENSE](LICENSE). Harmony: MIT (Andreas Pardeike).

---

# أسماء الشوارع العربية

مود للعبة Cities: Skylines II يرسم أسماء الطرق والأحياء العربية على الخريطة بشكل صحيح: حروف متصلة، واتجاه من اليمين لليسار، والأرقام والكلمات اللاتينية في مكانها.

## ليش
اللعبة ترسم أسماء الخريطة بأداة ما تدعم وصل الحروف العربية ولا ترتيبها، فتطلع الحروف منفصلة ومعكوسة. واجهة اللعبة نفسها تدعم العربي، فالمشكلة في أسماء الخريطة بس.

## كيف يشتغل
- يعدّل فقط الاستدعاءين اللي يمررون الاسم لرسّام أسماء الخريطة، وما يلمس القوائم ولا اللوحات ولا المودات الثانية.
- يحوّل الحروف لأشكالها المتصلة (أول الكلمة ووسطها وآخرها ومنفردة) مع اللام ألف.
- يرتّب النص: العربي من اليمين لليسار، والأرقام واللاتيني من اليسار لليمين، ويعكس الأقواس.
- يعالج كل اسم مرة وحدة ويحفظ النتيجة.
- إعدادات المسافة بين الكلمات (افتراضياً 10%) وحجم الحروف (افتراضياً 60%).
- يستخدم خطوط اللعبة إذا فيها الحروف العربية، وإلا خط من ويندوز.

ما يحفظ شي داخل المدينة، فتقدر تضيفه أو تشيله في أي وقت.

## البناء
تحتاج اللعبة مع أدوات التعديل الرسمية و .NET SDK، ثم:

```
dotnet build ArabicStreetNames/ArabicStreetNames.csproj -c Release
```

## الرخصة
MIT، انظر [LICENSE](LICENSE).
