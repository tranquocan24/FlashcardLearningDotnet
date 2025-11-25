# ?? Flashcard Learning App - Frontend Documentation

## ?? T?ng quan

Frontend ???c xây d?ng hoàn toàn b?ng **HTML, CSS, JavaScript thu?n** (Vanilla JS) - không s? d?ng framework ?? d? dàng test và customize.

## ?? C?u trúc file

```
wwwroot/
??? index.html              # Entry point - SPA Router
??? guide.html             # H??ng d?n s? d?ng & API Documentation
??? test-api.html          # Trang test t?t c? API endpoints
??? demo.html              # Demo UI components
??? css/
?   ??? style.css          # Main styles
?   ??? components.css     # Additional components & utilities
??? js/
    ??? app.js             # Core utilities (API, Auth, UI helpers)
    ??? router.js          # SPA Router & All page renderers
    ??? utils.js           # Advanced utilities & helpers
```

## ?? Cách s? d?ng

### 1. Ch?y ?ng d?ng

Sau khi ch?y backend API (dotnet run), truy c?p:

- **Main App:** `https://localhost:{port}/` ho?c `https://localhost:{port}/index.html`
- **Quick Guide:** `https://localhost:{port}/guide.html`
- **API Testing:** `https://localhost:{port}/test-api.html`
- **UI Demo:** `https://localhost:{port}/demo.html`

### 2. Flow s? d?ng c? b?n

1. **??ng ký tài kho?n** (Register)
2. **??ng nh?p** (Login) ? Nh?n JWT token
3. **T?o Deck** (My Decks)
4. **Thêm Flashcards** vào Deck
5. **H?c t?p** v?i 3 ch? ??: Flashcard / Quiz / Match
6. **Xem l?ch s? & Leaderboard**
7. **Qu?n lý Profile**

## ?? Các tính n?ng

### ?? Authentication
- ? Register (??ng ký)
- ? Login (??ng nh?p v?i JWT)
- ? Auto logout khi token h?t h?n
- ? Protected routes

### ?? Decks Management
- ? Xem t?t c? decks (c?a mình + public decks)
- ? T?o deck m?i (Public/Private)
- ? S?a deck
- ? Xóa deck
- ? View deck details v?i danh sách flashcards

### ?? Flashcards Management
- ? Thêm flashcard m?i
- ? **Auto-generate Audio** t? Free Dictionary API
- ? S?a flashcard
- ? Xóa flashcard
- ? Upload image (via URL)
- ? Phát audio pronunciation

### ?? Study Modes

#### 1. Flashcard Mode
- L?t th? ?? xem ??nh ngh?a
- Di chuy?n qua l?i gi?a các th?
- Phát audio pronunciation
- Progress tracking

#### 2. Quiz Mode
- Tr?c nghi?m 4 ?áp án
- Highlight ?úng/sai
- Tính ?i?m t? ??ng
- Hi?n th? k?t qu?

#### 3. Match Game
- Ghép Term v?i Definition
- ??m s? l??t th?
- Hi?u ?ng animation
- Tính ?i?m theo ?? chính xác

### ?? Progress Tracking
- ? L?u k?t qu? h?c t?p (Study Sessions)
- ? Xem l?ch s? h?c c?a mình
- ? Xem b?ng x?p h?ng theo Deck
- ? Th?ng kê: T?ng l??t h?c, ?i?m trung bình, chu?i ngày h?c

### ?? User Profile
- ? Xem thông tin cá nhân
- ? C?p nh?t username
- ? C?p nh?t avatar URL
- ? ??i m?t kh?u
- ? Xem th?ng kê cá nhân

### ?? Admin Panel
- ? Xem t?t c? users
- ? Xóa user (không th? t? xóa mình)
- ? Xem toàn b? l?ch s? h?c t?p h? th?ng
- ? Xóa study sessions
- ? Qu?n lý t?t c? decks & flashcards

## ?? Các trang test

### 1. `/test-api.html` - API Testing Dashboard

Trang này cho phép test **T?T C?** API endpoints m?t cách tr?c quan:

**Tính n?ng:**
- Form input cho m?i API endpoint
- T? ??ng thêm JWT token vào headers
- Display response real-time
- Test c? success và error cases

**Cách dùng:**
1. ??ng ký/??ng nh?p ?? l?y token
2. Token t? ??ng l?u vào localStorage
3. Click vào các button ?? test t?ng API
4. Xem response ? ph?n "API Response"

### 2. `/guide.html` - Quick Start Guide

**Bao g?m:**
- H??ng d?n t?ng b??c (Step-by-step)
- T?t c? API endpoints v?i method & body m?u
- Sample test data
- Checklist ??y ?? các tính n?ng c?n test
- Tips & Tricks

### 3. `/demo.html` - UI Components Playground

**Demo t?t c?:**
- Buttons (Primary, Success, Warning, Danger...)
- Badges & Chips
- Alerts
- Cards & Grid layouts
- Flip cards
- Quiz options
- Match game cards
- Progress bars
- Statistics cards
- Tables
- Modals

## ?? Tùy ch?nh CSS

### Variables (trong `/css/style.css`)

```css
:root {
    --primary-color: #4257b2;
    --danger-color: #dc3545;
    --success-color: #28a745;
    /* ... */
}
```

B?n có th? thay ??i màu ch? ??o b?ng cách s?a các CSS variables này.

### Responsive Design
- ? Desktop first
- ? Tablet support (768px)
- ? Mobile support
- ? Sidebar collapse on mobile

### Accessibility
- ? Keyboard navigation support
- ? Focus indicators
- ? High contrast mode
- ? Reduced motion support
- ? Screen reader friendly

## ?? Test Scenarios

### Scenario 1: User Flow (Basic)
1. Register: `test1@example.com` / `123456`
2. Login
3. Create deck: "English Vocabulary"
4. Add 5 flashcards v?i các t? ti?ng Anh (?? test auto audio)
5. Study v?i Flashcard mode
6. Study v?i Quiz mode
7. Study v?i Match mode
8. Check history
9. Check leaderboard

### Scenario 2: Multi-user Testing
1. T?o 3 users khác nhau
2. M?i user t?o deck riêng
3. User 1 t?o public deck
4. User 2 xem ???c public deck c?a User 1
5. User 2 không xem ???c private deck c?a User 1
6. C? 3 users h?c cùng 1 public deck
7. Check leaderboard

### Scenario 3: Admin Testing
1. T?o user v?i role Admin (trong DB)
2. Login as Admin
3. Xem t?t c? users
4. Xem t?t c? study sessions
5. Xóa user khác (không th? t? xóa)
6. S?a/xóa deck c?a user khác

### Scenario 4: Audio Auto-generation
1. T?o flashcard v?i term: "hello"
2. Check response có `audioUrl` không
3. Click button phát audio
4. Test v?i các t?: world, computer, book, apple

## ?? Keyboard Shortcuts (Planned)

- `Ctrl + N`: New Deck
- `Space`: Flip card (trong study mode)
- `Arrow Left/Right`: Previous/Next card
- `Esc`: Close modal
- `/`: Focus search

## ?? Debug Tips

### Xem API Calls
1. M? DevTools (F12)
2. Tab Network
3. Filter: Fetch/XHR
4. Xem Request Headers, Payload, Response

### Xem Token
```javascript
// Trong console:
localStorage.getItem('token')
```

### Test v?i Postman/Insomnia
Copy các endpoint t? `guide.html` và test tr?c ti?p

## ?? Checklist ??y ??

### Authentication ?
- [x] Register new user
- [x] Login with credentials
- [x] JWT token storage
- [x] Auto logout on 401
- [x] Token in Authorization header

### Decks CRUD ?
- [x] Get all decks (own + public)
- [x] Get single deck by ID
- [x] Create new deck
- [x] Update deck (title, description, isPublic)
- [x] Delete deck
- [x] Permission checks (owner/admin only)

### Flashcards CRUD ?
- [x] Get flashcard by ID
- [x] Create flashcard
- [x] Auto-generate audio URL
- [x] Update flashcard
- [x] Delete flashcard
- [x] Display image
- [x] Play audio

### Study Sessions ?
- [x] Flashcard mode (flip cards)
- [x] Quiz mode (multiple choice)
- [x] Match game (pair matching)
- [x] Save session results
- [x] Get study history
- [x] Get leaderboard by deck
- [x] Admin: Get all sessions
- [x] Admin: Delete session

### User Profile ?
- [x] Get profile info
- [x] Update username
- [x] Update avatar URL
- [x] Change password
- [x] Display statistics

### Admin Features ?
- [x] Get all users
- [x] Delete user (not self)
- [x] View all study sessions
- [x] Delete any session
- [x] Manage any deck/flashcard

## ?? UI/UX Features

- ? Responsive design
- ? Smooth animations
- ? Loading indicators
- ? Toast notifications
- ? Modal dialogs
- ? Empty states
- ? Error handling
- ? Form validation
- ? Flip card animation
- ? Progress bars
- ? Leaderboard design
- ? Statistics dashboard

## ?? External Dependencies

**NONE!** 100% Vanilla JS/CSS

Ch? s? d?ng:
- Browser APIs (Fetch, LocalStorage, Audio)
- CSS3 Animations
- No jQuery, no React, no frameworks!

## ?? Notes

- Token ???c l?u trong `localStorage` v?i key `token`
- User info ???c l?u trong `localStorage` v?i key `userInfo`
- T?t c? API calls ??u t? ??ng thêm Authorization header
- Audio auto-generation: Ch? work v?i t? ti?ng Anh có trong Free Dictionary API

## ?? Learning Path

1. **Beginner:** Ch? dùng main app (index.html)
2. **Intermediate:** Dùng test-api.html ?? hi?u rõ API flow
3. **Advanced:** Xem source code trong /js ?? h?c cách build SPA

## ?? Contributing

?? thêm tính n?ng m?i:
1. Thêm route vào `routes` object trong `router.js`
2. T?o render function cho page m?i
3. Thêm menu item trong `renderLayout()`
4. Style trong `style.css` ho?c `components.css`

---

**Made with ?? using Vanilla JavaScript**
