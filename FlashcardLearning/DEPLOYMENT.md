# ?? FLASHCARD LEARNING APP - DEPLOYMENT GUIDE

## ?? T?ng quan

?ng d?ng h?c flashcard v?i tính n?ng auto-generate audio, h? tr? 3 ch? ?? h?c (Flashcard/Quiz/Match), leaderboard và progress tracking.

## ??? Yêu c?u h? th?ng

- .NET 8.0 SDK
- SQL Server (LocalDB ho?c SQL Server Express)
- Trình duy?t hi?n ??i (Chrome, Edge, Firefox, Safari)
- Port 5000 (HTTP) ho?c 5001 (HTTPS) kh? d?ng

## ?? Cài ??t & Ch?y

### B??c 1: Clone Repository
```bash
git clone https://github.com/tranquocan24/FlashcardLearningDotnet.git
cd FlashcardLearningDotnet/FlashcardLearning
```

### B??c 2: C?u hình Database
M? `appsettings.json` và c?p nh?t connection string:

```json
{
  "ConnectionStrings": {
    "DBConnection": "Server=(localdb)\\mssqllocaldb;Database=FlashcardLearningDb;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

### B??c 3: Ch?y Migration
```bash
dotnet ef database update
```

### B??c 4: Ch?y ?ng d?ng
```bash
dotnet run
```

Ho?c t? Visual Studio: `F5` ho?c `Ctrl + F5`

### B??c 5: Truy c?p
- **Main App:** https://localhost:5001/ ho?c https://localhost:5001/index.html
- **Welcome Page:** https://localhost:5001/welcome.html
- **Tutorial:** https://localhost:5001/tutorial.html
- **API Testing:** https://localhost:5001/test-api.html
- **Swagger:** https://localhost:5001/swagger

## ?? C?u trúc Frontend

```
wwwroot/
??? index.html           ? Main SPA Application
??? welcome.html         ? Landing page v?i navigation
??? tutorial.html        ? H??ng d?n chi ti?t t?ng b??c
??? guide.html          ? API Documentation ??y ??
??? test-api.html       ? Tool test t?t c? API endpoints
??? demo.html           ? Demo UI components
??? start.html          ? Quick start guide
??? README.md           ? Frontend documentation
?
??? css/
?   ??? style.css       ? Main stylesheet
?   ??? components.css  ? Additional components & utilities
?
??? js/
    ??? app.js          ? Core utilities (API, Auth, UI)
    ??? router.js       ? SPA Router & Page renderers
    ??? utils.js        ? Advanced utilities & helpers
```

## ?? Các trang Frontend

| Trang | URL | M?c ?ích |
|-------|-----|----------|
| Welcome | `/welcome.html` | Landing page, ?i?u h??ng ??n các trang khác |
| Main App | `/index.html` ho?c `/` | ?ng d?ng SPA chính (Login ? Dashboard ? Decks ? Study...) |
| Tutorial | `/tutorial.html` | H??ng d?n s? d?ng chi ti?t cho end-user |
| API Guide | `/guide.html` | Documentation cho developers |
| API Testing | `/test-api.html` | Tool test API tr?c ti?p |
| UI Demo | `/demo.html` | Demo t?t c? UI components |
| Quick Start | `/start.html` | H??ng d?n nhanh 5 phút |

## ?? T?o Admin User

Sau khi ??ng ký user thông th??ng, ch?y SQL query:

```sql
UPDATE Users 
SET Role = 'Admin' 
WHERE Email = 'admin@example.com'
```

Ho?c dùng SQL Server Management Studio / Azure Data Studio.

## ?? Test Workflow

### Workflow 1: UI Testing (Recommended cho End Users)
1. M? https://localhost:5001/
2. Register ? Login
3. T?o Deck ? Thêm Flashcards
4. Study v?i 3 modes
5. Check History & Leaderboard
6. Update Profile

### Workflow 2: API Testing (Recommended cho Developers)
1. M? https://localhost:5001/test-api.html
2. Register ? Login (token t? ??ng l?u)
3. Test t?ng endpoint v?i form inputs
4. Xem response real-time
5. Copy ID t? response ?? test ti?p

### Workflow 3: Swagger Testing
1. M? https://localhost:5001/swagger
2. Click "Authorize" và nh?p token
3. Test các endpoints
4. Xem schema & validation rules

## ?? Auto Audio Feature

### Cách ho?t ??ng:
1. User t?o flashcard v?i `term` là t? ti?ng Anh
2. Backend g?i Free Dictionary API: `https://api.dictionaryapi.dev/api/v2/entries/en/{term}`
3. Parse response l?y link audio MP3
4. L?u vào `Flashcard.AudioUrl`
5. Frontend hi?n th? button "?? Phát âm"

### Test v?i các t? có audio:
? hello, world, computer, book, apple, water, happy, beautiful, friend, language

### L?u ý:
- Không ph?i t? nào c?ng có audio trong Free Dictionary API
- N?u không tìm th?y, `AudioUrl` s? là `null` ho?c `""`
- Frontend có fallback: dùng Web Speech API ?? ??c

## ?? Database Schema

### Tables:
- **Users:** Id, Username, Email, Password (hashed), Role, AvatarUrl, CreatedAt
- **Decks:** Id, Title, Description, IsPublic, UserId, CreatedAt
- **Flashcards:** Id, Term, Definition, Example, ImageUrl, AudioUrl, DeckId
- **StudySessions:** Id, UserId, DeckId, Mode, Score, TotalCards, DateStudied

### Relationships:
- User 1:N Decks
- User 1:N StudySessions
- Deck 1:N Flashcards
- Deck 1:N StudySessions

## ?? Security

- ? JWT Authentication (Bearer token)
- ? Password hashing v?i BCrypt
- ? Role-based authorization (User/Admin)
- ? HTTPS enforcement
- ? Input validation v?i Data Annotations
- ? CORS configuration (if needed)

## ?? Frontend Architecture

### SPA Router
- Hash-based routing (`#/dashboard`, `#/decks`, etc.)
- Route guards (redirect to login if not authenticated)
- Dynamic content loading
- No page refresh

### State Management
- LocalStorage cho token & user info
- In-memory state cho study sessions
- Auto-sync v?i backend

### API Communication
- Centralized `apiCall()` function
- Auto-attach JWT token
- Error handling (401, 403, 404...)
- Loading indicators

## ?? Responsive Design

- ? Desktop: Full sidebar + main content
- ? Tablet: Collapsible sidebar
- ? Mobile: Hamburger menu (planned)
- ? Print-friendly styles

## ?? Troubleshooting

### Issue: 401 Unauthorized
**Cause:** Token invalid or expired
**Fix:** Logout and login again

### Issue: CORS Error
**Cause:** Backend not configured for CORS
**Fix:** Add to Program.cs:
```csharp
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder => {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
app.UseCors();
```

### Issue: Static files not serving
**Cause:** Missing middleware
**Fix:** Ensure Program.cs has:
```csharp
app.UseDefaultFiles();
app.UseStaticFiles();
```

### Issue: Audio not playing
**Cause:** Dictionary API down or word not found
**Fix:** Use common English words or check console for errors

## ?? Performance Tips

1. **Lazy load images:** Use `data-src` attribute
2. **Debounce search:** Already implemented in utils.js
3. **Cache API responses:** Use localStorage with expiry
4. **Minimize re-renders:** Only update changed elements
5. **Compress assets:** Minify CSS/JS for production

## ?? Deployment

### Deploy Backend (Azure App Service)
```bash
dotnet publish -c Release
# Upload to Azure App Service
```

### Update Connection String (Production)
```json
{
  "ConnectionStrings": {
    "DBConnection": "Server=tcp:your-server.database.windows.net,1433;Database=FlashcardDB;User ID=your-user;Password=your-password;Encrypt=True;"
  }
}
```

### Frontend (Already bundled with backend)
- No build step needed (pure HTML/CSS/JS)
- Files served from `wwwroot/`
- CDN not required

## ?? Support & Contact

- **GitHub:** https://github.com/tranquocan24/FlashcardLearningDotnet
- **Issues:** https://github.com/tranquocan24/FlashcardLearningDotnet/issues

## ?? License

[Add your license here]

## ????? Author

Tran Quoc An

---

**Made with ?? using ASP.NET Core 8.0 & Vanilla JavaScript**
