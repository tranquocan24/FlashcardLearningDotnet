# ?? AUTO-TRANSLATION SUGGESTION FEATURE

## ?? T?ng quan

Tính n?ng **G?i ý d?ch t? ??ng** (Auto-Translation Suggestion) giúp ng??i dùng t? ??ng l?y ngh?a ti?ng Vi?t khi t?o flashcard m?i, ti?t ki?m th?i gian tra t? ?i?n.

---

## ??? Ki?n trúc

### Backend: Repository & Service Pattern

```
Controller (DictionaryController)
    ?
Service (DictionaryService)
    ?
Repository (DictionaryRepository)
    ?
Database (DictionaryEntries Table)
    ?
External API (MyMemory Translation API)
```

---

## ?? C?u trúc File Backend

### 1. Entity: `DictionaryEntry.cs`

```plaintext
FlashcardLearning/Models/DictionaryEntry.cs
```

**Ch?c n?ng:** L?u cache t? ?ã d?ch (Key: Word, Value: Meaning)

**Properties:**
- `Id` (Guid): Primary Key
- `Word` (string): T? ti?ng Anh (lowercase)
- `Meaning` (string): Ngh?a ti?ng Vi?t
- `CachedAt` (DateTime): Th?i gian cache

### 2. Repository Layer

```plaintext
FlashcardLearning/Repositories/IDictionaryRepository.cs
FlashcardLearning/Repositories/DictionaryRepository.cs
```

**Methods:**
- `GetByWordAsync(string word)`: L?y t? v?ng t? cache
- `AddAsync(DictionaryEntry entry)`: Thêm t? m?i vào cache
- `ExistsAsync(string word)`: Ki?m tra t? có trong cache không

### 3. Service Layer

```plaintext
FlashcardLearning/Services/IDictionaryService.cs
FlashcardLearning/Services/DictionaryService.cs
```

**Methods:**
- `LookupWordAsync(string word)`: Tra c?u t? (Cache-first strategy)
- `GetAudioUrlAsync(string word)`: L?y URL audio (existing feature)

**Logic Flow:**
1. Check cache (Database) tr??c
2. N?u có ? Tr? v? ngay (Fast)
3. N?u không ? G?i MyMemory API
4. L?u vào cache cho l?n sau
5. Tr? v? k?t qu?

### 4. Controller

```plaintext
FlashcardLearning/Controllers/DictionaryController.cs
```

**Endpoint:**
- `GET /api/Dictionary/lookup?word={word}`

**Authorization:** Requires JWT Token

**Response:**
```json
{
  "word": "hello",
  "meaning": "xin chào",
  "success": true
}
```

### 5. Dependency Injection

```plaintext
FlashcardLearning/Program.cs
```

**Registered Services:**
```csharp
builder.Services.AddScoped<IDictionaryRepository, DictionaryRepository>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();
builder.Services.AddHttpClient<IDictionaryService, DictionaryService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});
```

---

## ?? C?u trúc Frontend (Vanilla JS)

### File: `wwwroot/js/router.js`

**Functions thêm m?i:**

#### 1. `debounce(func, delay)`
- T?o debounce th? công (không dùng th? vi?n)
- Delay: 1000ms (1 giây)

#### 2. `lookupWord(word)`
- G?i API `/api/Dictionary/lookup?word={word}`
- Ch? ?i?n ngh?a n?u ô Definition **R?NG**
- Hi?n th? loading indicator: "? ?ang tra c?u..."
- Green border khi thành công

#### 3. `debouncedLookup`
- Wrapper c?a `lookupWord` v?i debounce 1 giây

#### 4. S?a ??i `showCreateFlashcardModal()`
- Thêm event listener `input` cho ô Term
- G?i `debouncedLookup` khi user nh?p

---

## ?? Workflow hoàn ch?nh

### User Actions:
1. User click "Add Card" trong Deck Detail
2. Modal m? ra
3. User nh?p t? ti?ng Anh vào ô "Term"
4. Sau 1 giây không gõ ? Auto lookup
5. System ?i?n ngh?a ti?ng Vi?t vào ô "Definition" (n?u r?ng)
6. User có th? ch?nh s?a ho?c gi? nguyên
7. Click "Create" ? Flashcard ???c t?o

### Backend Flow:
```
User Input "hello"
    ?
[Debounce 1s]
    ?
Frontend: fetch('/api/Dictionary/lookup?word=hello')
    ?
Backend: DictionaryController.LookupWord()
    ?
DictionaryService.LookupWordAsync("hello")
    ?
Check DictionaryRepository.GetByWordAsync("hello")
    ?
    ?? Found in DB? ? Return t? cache (Fast!)
    ?
    ?? Not found?
        ?
        Call MyMemory API: https://api.mymemory.translated.net/get?q=hello&langpair=en|vi
        ?
        Parse response ? "xin chào"
        ?
        Save to DB (DictionaryRepository.AddAsync)
        ?
        Return "xin chào"
    ?
Frontend: Fill "xin chào" vào ô Definition
```

---

## ?? Testing Guide

### 1. Test Backend API (Swagger)

**Step 1:** M? Swagger
```
https://localhost:5001/swagger
```

**Step 2:** Authorize v?i JWT Token
- Click "Authorize" button
- Nh?p: `Bearer YOUR_TOKEN_HERE`

**Step 3:** Test endpoint
```
GET /api/Dictionary/lookup?word=hello
```

**Expected Response:**
```json
{
  "word": "hello",
  "meaning": "xin chào",
  "success": true
}
```

### 2. Test Frontend UI

**Step 1:** Login vào app
```
https://localhost:5001/
```

**Step 2:** Vào b?t k? Deck nào

**Step 3:** Click "Add Card"

**Step 4:** Nh?p t? ti?ng Anh (e.g. "hello")

**Step 5:** ??i 1 giây ? Ngh?a t? ??ng xu?t hi?n

**Step 6:** Ki?m tra:
- ? Placeholder ??i thành "? ?ang tra c?u..."
- ? Ngh?a ti?ng Vi?t t? ??ng ?i?n vào
- ? Border xanh lá xu?t hi?n (2 giây)

### 3. Test Edge Cases

#### Case 1: T? không t?n t?i
- Input: `asdfghjkl`
- Expected: Không ?i?n gì (không có ngh?a)

#### Case 2: User ?ã nh?p Definition tr??c
- Input Term: `hello`
- User ?ã nh?p Definition: `l?i chào`
- Expected: KHÔNG ghi ?è (gi? nguyên "l?i chào")

#### Case 3: Cache Hit (l?n 2)
- L?n 1: G?i API ? L?u vào DB
- L?n 2: Không g?i API ? L?y t? DB (nhanh h?n)

---

## ?? Database Schema

### Table: `DictionaryEntries`

| Column | Type | Nullable | Index |
|--------|------|----------|-------|
| Id | uniqueidentifier | No | PK |
| Word | nvarchar(200) | No | **Unique** |
| Meaning | nvarchar(1000) | No | |
| CachedAt | datetime2 | No | |

**Migration Name:** `AddDictionaryEntry`

**SQL Generated:**
```sql
CREATE TABLE [DictionaryEntries] (
    [Id] uniqueidentifier NOT NULL,
    [Word] nvarchar(200) NOT NULL,
    [Meaning] nvarchar(1000) NOT NULL,
    [CachedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DictionaryEntries] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_DictionaryEntries_Word] ON [DictionaryEntries] ([Word]);
```

---

## ?? Configuration

### External API: MyMemory Translated

**Base URL:**
```
https://api.mymemory.translated.net
```

**Endpoint:**
```
GET /get?q={word}&langpair=en|vi
```

**Response Format:**
```json
{
  "responseData": {
    "translatedText": "xin chào"
  }
}
```

**Free Tier Limits:**
- 500 requests/day
- 100 chars/request
- No API key required

---

## ?? Performance Optimizations

### 1. Cache Strategy
- ? Cache hit: ~5-10ms (Database query)
- ?? Cache miss: ~300-500ms (External API call)
- ?? Hit Rate: T?ng d?n theo th?i gian

### 2. Debounce
- ? Gi?m s? l??ng API calls
- ? Tránh spam requests khi user ?ang gõ
- ? Delay: 1000ms (t?i ?u gi?a UX và Performance)

### 3. Timeout Protection
- HttpClient timeout: 5 seconds
- API call timeout: 3 seconds
- Graceful fallback n?u API down

### 4. Unique Index
- Word column có unique index
- Tránh duplicate entries
- T?ng t?c ?? query

---

## ?? Error Handling

### Frontend:
- ? Network error ? Silent fail (không hi?n th? error)
- ? Empty result ? Không ?i?n gì
- ? API timeout ? Không block UI

### Backend:
- ? Invalid input ? 400 Bad Request
- ? API timeout ? Return empty string
- ? Database error ? Log + Return empty
- ? Duplicate word ? Catch unique constraint error

---

## ?? Future Enhancements

### Short-term:
1. ? Add pronunciation (IPA) field
2. ? Cache expiration (e.g. 30 days)
3. ? Batch translation API
4. ? User feedback (upvote/downvote translation)

### Long-term:
1. ?? Offline support (IndexedDB)
2. ?? Multiple language pairs (en-vi, vi-en, en-ja...)
3. ?? AI-powered translation (OpenAI GPT)
4. ?? Translation history per user
5. ?? Custom dictionary (user-defined)

---

## ?? Security

### API Authorization:
- ? JWT Token required
- ? No anonymous access
- ? Role-based access (User/Admin)

### Input Validation:
- ? Max length: 200 characters
- ? XSS protection (sanitize input)
- ? SQL Injection protection (EF Core parameterized queries)

### Rate Limiting (Recommended):
- ?? TODO: Add rate limiting middleware
- ?? TODO: Limit 10 requests/minute per user

---

## ?? Code Examples

### Backend Service:
```csharp
public async Task<string> LookupWordAsync(string word)
{
    var normalizedWord = word.Trim().ToLower();
    
    // 1. Check cache
    var cachedEntry = await _dictionaryRepository.GetByWordAsync(normalizedWord);
    if (cachedEntry != null)
    {
        return cachedEntry.Meaning;
    }
    
    // 2. Call external API
    var meaning = await FetchFromExternalApiAsync(normalizedWord);
    
    // 3. Save to cache
    if (!string.IsNullOrEmpty(meaning))
    {
        var newEntry = new DictionaryEntry
        {
            Word = normalizedWord,
            Meaning = meaning
        };
        await _dictionaryRepository.AddAsync(newEntry);
    }
    
    return meaning;
}
```

### Frontend Debounce:
```javascript
function debounce(func, delay) {
    let timeout;
    return function(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), delay);
    };
}

const debouncedLookup = debounce(lookupWord, 1000);
```

### Frontend Auto-fill:
```javascript
termInput.addEventListener('input', (e) => {
    const word = e.target.value;
    if (word && word.trim().length > 0) {
        debouncedLookup(word);
    }
});
```

---

## ? Checklist Hoàn thành

### Backend:
- ? Entity: DictionaryEntry
- ? Repository: IDictionaryRepository + DictionaryRepository
- ? Service: IDictionaryService + DictionaryService
- ? Controller: DictionaryController
- ? DI Registration: Program.cs
- ? Migration: AddDictionaryEntry
- ? Database Update: Applied

### Frontend:
- ? Debounce function
- ? LookupWord function
- ? Event listener: Term input
- ? UI feedback: Loading + Success

### Testing:
- ? Build successful
- ? Migration applied
- ? No compilation errors

---

## ?? K?t lu?n

Tính n?ng **Auto-Translation Suggestion** ?ã ???c tri?n khai thành công v?i:

- ? Ki?n trúc clean: Repository + Service Pattern
- ? Performance t?t: Cache-first strategy
- ? UX m??t mà: Debounce 1 giây
- ? Error handling: Graceful fallback
- ? Security: JWT Authentication

**Happy Coding! ??**

---

**Author:** FlashcardLearning Team  
**Date:** 2024-11-27  
**Version:** 1.0.0
