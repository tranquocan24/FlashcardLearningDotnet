# ?? TESTING GUIDE - Flashcard Learning App

## ?? T?ng quan

H??ng d?n test ??y ?? t?t c? ch?c n?ng c?a ?ng d?ng Flashcard Learning.

## ?? Test Scenarios

---

## ?? Scenario 1: Authentication Flow

### Test Case 1.1: Register New User
**Steps:**
1. M? https://localhost:5001/
2. Click "Ch?a có tài kho?n? ??ng ký ngay"
3. ?i?n:
   - Username: `testuser1`
   - Email: `test1@example.com`
   - Password: `123456`
4. Click "??ng ký"

**Expected Result:**
- ? Alert "??ng ký thành công! Hãy ??ng nh?p."
- ? T? ??ng chuy?n v? trang login
- ? User ???c t?o trong database

**API Endpoint:** `POST /api/Auth/register`

---

### Test Case 1.2: Login with Valid Credentials
**Steps:**
1. ? trang login, ?i?n:
   - Email: `test1@example.com`
   - Password: `123456`
2. Click "??ng nh?p"

**Expected Result:**
- ? Alert "??ng nh?p thành công!"
- ? Token ???c l?u vào localStorage
- ? Redirect ??n Dashboard
- ? Sidebar hi?n th? username

**API Endpoint:** `POST /api/Auth/login`

---

### Test Case 1.3: Login with Invalid Credentials
**Steps:**
1. Nh?p email: `test1@example.com`
2. Nh?p password SAI: `wrongpassword`
3. Click "??ng nh?p"

**Expected Result:**
- ? Alert "Email or password is wrong"
- ? Không ???c login
- ? V?n ? trang login

---

### Test Case 1.4: Auto Logout on Token Expiry
**Steps:**
1. Login thành công
2. Trong Console (F12): `localStorage.removeItem('token')`
3. Reload page ho?c navigate ??n b?t k? trang nào

**Expected Result:**
- ? T? ??ng redirect v? trang login
- ? Alert "Phiên ??ng nh?p h?t h?n"

---

## ?? Scenario 2: Decks Management

### Test Case 2.1: Create Public Deck
**Steps:**
1. Login thành công
2. Navigate ??n "My Decks"
3. Click "? T?o B? Th? M?i"
4. ?i?n:
   - Tên: `T? v?ng IELTS`
   - Mô t?: `100 t? v?ng thi?t y?u cho IELTS`
   - ? Check "Công khai cho m?i ng??i"
5. Click "T?o M?i"

**Expected Result:**
- ? Deck ???c t?o thành công
- ? Alert "T?o b? th? thành công!"
- ? Deck hi?n th? trong danh sách v?i badge "?? Công khai"
- ? UserId = current user ID

**API Endpoint:** `POST /api/Decks`

---

### Test Case 2.2: Create Private Deck
**Steps:**
1. T?o deck m?i
2. ? Không check "Công khai"
3. Submit

**Expected Result:**
- ? Deck v?i badge "?? Riêng t?"

---

### Test Case 2.3: View All Decks (Own + Public)
**Steps:**
1. Login v?i User 1
2. T?o 1 public deck, 1 private deck
3. Logout
4. Login v?i User 2
5. Navigate "My Decks"

**Expected Result:**
- ? User 2 th?y:
  - ? Public deck c?a User 1
  - ? Private decks c?a chính User 2
  - ? KHÔNG th?y private deck c?a User 1

**API Endpoint:** `GET /api/Decks`

---

### Test Case 2.4: Update Deck
**Steps:**
1. Click icon ?? trên m?t deck
2. ??i title thành `T? v?ng IELTS - Updated`
3. Click "L?u"

**Expected Result:**
- ? Deck title ???c c?p nh?t
- ? Alert "C?p nh?t thành công!"

**API Endpoint:** `PUT /api/Decks/{id}`

---

### Test Case 2.5: Delete Deck
**Steps:**
1. Click icon ??? trên deck
2. Confirm dialog

**Expected Result:**
- ? Deck b? xóa
- ? T?t c? flashcards trong deck c?ng b? xóa (cascade)
- ? Alert "Xóa b? th? thành công!"

**API Endpoint:** `DELETE /api/Decks/{id}`

---

### Test Case 2.6: Permission Check - Cannot Edit Others' Deck
**Steps:**
1. Login User 1, t?o private deck
2. Note deck ID
3. Logout, login User 2
4. Manually navigate: `#/deck/{deck-id-of-user-1}`

**Expected Result:**
- ? 403 Forbidden
- ? Alert "B?n không có quy?n..."

---

## ?? Scenario 3: Flashcards Management

### Test Case 3.1: Create Flashcard with Auto Audio
**Steps:**
1. Vào m?t deck
2. Click "? Thêm Th?"
3. ?i?n:
   - Term: `hello`
   - Definition: `xin chào`
   - Example: `Hello, how are you?`
4. Click "T?o"

**Expected Result:**
- ? Flashcard ???c t?o
- ? **AudioUrl ???c t? ??ng fill** (check response ho?c DB)
- ? Button "?? Phát âm" hi?n th?
- ? Click button ? audio phát thành công

**API Endpoint:** `POST /api/Flashcards`

---

### Test Case 3.2: Create Flashcard with Image
**Steps:**
1. T?o flashcard
2. ?i?n Image URL: `https://upload.wikimedia.org/wikipedia/commons/thumb/6/6a/JavaScript-logo.png/200px-JavaScript-logo.png`
3. Submit

**Expected Result:**
- ? Flashcard hi?n th? v?i ?nh
- ? ?nh load ?úng

---

### Test Case 3.3: Test Auto Audio v?i nhi?u t?
**Test Terms:**
```
hello ? ? Có audio
world ? ? Có audio
computer ? ? Có audio
asdfghjkl ? ? Không có audio (t? không t?n t?i)
```

**Expected Result:**
- ? Các t? h?p l? có audio
- ? T? không h?p l?: AudioUrl = null/empty

---

### Test Case 3.4: Update Flashcard
**Steps:**
1. Click ?? S?a trên flashcard
2. ??i Definition
3. Click "L?u"

**Expected Result:**
- ? Flashcard ???c c?p nh?t
- ? Alert "C?p nh?t thành công!"

**API Endpoint:** `PUT /api/Flashcards/{id}`

---

### Test Case 3.5: Delete Flashcard
**Steps:**
1. Click ??? Xóa
2. Confirm

**Expected Result:**
- ? Flashcard b? xóa kh?i deck
- ? Alert "Xóa flashcard thành công!"

**API Endpoint:** `DELETE /api/Flashcards/{id}`

---

## ?? Scenario 4: Study Modes

### Test Case 4.1: Flashcard Mode
**Steps:**
1. Vào deck có ít nh?t 5 flashcards
2. Click "?? B?t ??u h?c"
3. Click "?? Flashcard"
4. Click vào th? ?? l?t
5. Click "Ti?p theo" ?? xem th? ti?p
6. Hoàn thành t?t c? th?

**Expected Result:**
- ? Th? l?t ???c (flip animation)
- ? Progress bar t?ng d?n
- ? Audio button ho?t ??ng
- ? Sau th? cu?i ? Modal "Hoàn thành!"
- ? Session ???c t? ??ng l?u v?i Mode = "Flashcard"

**API Endpoint:** `POST /api/StudySessions`

---

### Test Case 4.2: Quiz Mode
**Steps:**
1. Click "? Quiz"
2. ??c câu h?i (Term)
3. Ch?n 1 trong 4 ?áp án
4. Click "Ti?p theo"
5. Hoàn thành quiz

**Expected Result:**
- ? 4 ?áp án random (1 ?úng + 3 sai)
- ? Ch?n ?úng ? màu xanh, alert "? Chính xác!"
- ? Ch?n sai ? màu ??, highlight ?áp án ?úng
- ? ?i?m s? t?ng khi ch?n ?úng
- ? K?t qu? cu?i: X/Y ?i?m

---

### Test Case 4.3: Match Game
**Steps:**
1. Click "?? Match Game"
2. Click 1 Term
3. Click Definition t??ng ?ng
4. Ghép h?t t?t c? c?p

**Expected Result:**
- ? Ghép ?úng ? 2 th? chuy?n màu xanh
- ? Ghép sai ? 2 th? rung (shake animation)
- ? ??m s? l??t th?
- ? Hoàn thành ? ?i?m = s? c?p ?úng

---

## ?? Scenario 5: Progress Tracking

### Test Case 5.1: View Study History
**Steps:**
1. Hoàn thành ít nh?t 3 sessions
2. Navigate "?? Study History"

**Expected Result:**
- ? Hi?n th? table v?i t?t c? sessions
- ? Thông tin: Th?i gian, Deck, Mode, ?i?m, T? l?
- ? Th?ng kê: T?ng l??t, ?i?m TB, Chu?i ngày
- ? Stats theo mode (Flashcard/Quiz/Match)

**API Endpoint:** `GET /api/StudySessions/history`

---

### Test Case 5.2: View Leaderboard
**Setup:**
- User 1: H?c deck A, ?i?m 10/10
- User 2: H?c deck A, ?i?m 8/10
- User 3: H?c deck A, ?i?m 9/10

**Steps:**
1. Sau khi hoàn thành session
2. Click "Xem Leaderboard" trong modal k?t qu?

**Expected Result:**
- ? X?p h?ng: User 1 (#1), User 3 (#2), User 2 (#3)
- ? Hi?n th?: Rank, Avatar, Username, Score, Date

**API Endpoint:** `GET /api/StudySessions/leaderboard/{deckId}`

---

## ?? Scenario 6: User Profile

### Test Case 6.1: Update Profile
**Steps:**
1. Navigate "?? Profile"
2. ??i username: `NewUsername`
3. Thêm Avatar URL: `https://i.pravatar.cc/150?img=3`
4. Click "?? L?u thay ??i"

**Expected Result:**
- ? Profile updated
- ? Avatar hi?n th? trong sidebar
- ? Alert "C?p nh?t h? s? thành công!"

**API Endpoint:** `PUT /api/Users/profile`

---

### Test Case 6.2: Change Password
**Steps:**
1. ? trang Profile
2. ?i?n:
   - Old Password: `123456`
   - New Password: `newpass123`
   - Confirm: `newpass123`
3. Click "??i m?t kh?u"

**Expected Result:**
- ? Alert "??i m?t kh?u thành công!"
- ? Form clear
- ? Logout và login l?i v?i password m?i ? OK

**API Endpoint:** `PUT /api/Users/change-password`

---

### Test Case 6.3: Change Password with Wrong Old Password
**Steps:**
1. Nh?p Old Password SAI
2. Submit

**Expected Result:**
- ? Alert "M?t kh?u c? không chính xác."
- ? Password không ??i

---

### Test Case 6.4: Change Password Validation
**Steps:**
1. New Password: `123` (< 6 chars)
2. Submit

**Expected Result:**
- ? Alert "M?t kh?u m?i ph?i có ít nh?t 6 ký t?"

---

## ?? Scenario 7: Admin Features

**Setup:** T?o admin user b?ng SQL:
```sql
UPDATE Users SET Role = 'Admin' WHERE Email = 'admin@example.com'
```

### Test Case 7.1: Admin View All Users
**Steps:**
1. Login as Admin
2. Navigate "?? Admin Panel"

**Expected Result:**
- ? Table hi?n th? t?t c? users
- ? Columns: Username, Email, Role, Created Date, Actions

**API Endpoint:** `GET /api/Users`

---

### Test Case 7.2: Admin Delete User
**Steps:**
1. ? Admin Panel
2. Click "Xóa" trên m?t user (không ph?i admin)
3. Confirm

**Expected Result:**
- ? User b? xóa
- ? Alert "Xóa ng??i dùng thành công!"
- ? T?t c? decks & sessions c?a user ?ó c?ng b? xóa (cascade)

**API Endpoint:** `DELETE /api/Users/{id}`

---

### Test Case 7.3: Admin Cannot Delete Self
**Steps:**
1. Click "Xóa" trên chính user admin ?ang login

**Expected Result:**
- ? Alert "B?n không th? t? xóa tài kho?n c?a chính mình."

---

### Test Case 7.4: Admin View All Study Sessions
**Steps:**
1. Scroll xu?ng "L?ch s? h?c t?p (Toàn h? th?ng)"

**Expected Result:**
- ? Table v?i sessions c?a T?T C? users
- ? Columns: User, Deck, Mode, Score, Time, Actions

**API Endpoint:** `GET /api/StudySessions/admin/all-history`

---

### Test Case 7.5: Non-Admin Cannot Access Admin Panel
**Steps:**
1. Login as regular user
2. Manually navigate: `#/admin`

**Expected Result:**
- ? Alert "B?n không có quy?n truy c?p trang này"
- ? Redirect v? Dashboard

---

## ?? Scenario 8: Audio Features

### Test Case 8.1: Auto Audio Generation
**Steps:**
1. T?o flashcard v?i term: `hello`
2. Submit (không ?i?n AudioUrl)
3. Check response ho?c DB

**Expected Result:**
- ? `audioUrl` field có giá tr? (VD: `https://api.dictionaryapi.dev/media/.../en-us.mp3`)
- ? Button "?? Phát âm" hi?n th?

---

### Test Case 8.2: Play Audio
**Steps:**
1. ? deck detail, click "?? Phát âm" trên flashcard có audio
2. Ho?c trong Study mode, click audio button

**Expected Result:**
- ? Audio phát ra (nghe ???c gi?ng ??c "hello")
- ? Không có error trong console

---

### Test Case 8.3: No Audio for Invalid Word
**Steps:**
1. T?o flashcard v?i term: `asdfghjkl` (t? vô ngh?a)
2. Submit

**Expected Result:**
- ? Flashcard v?n ???c t?o
- ? AudioUrl = null ho?c ""
- ? Button audio không hi?n th?

---

### Test Case 8.4: Test Multiple Words
**Test v?i list:**
```
? computer ? Có audio
? book ? Có audio
? apple ? Có audio
? xyz123 ? Không có audio
```

---

## ?? Scenario 9: Leaderboard

**Setup:**
- User A: H?c deck X, score 10/10
- User B: H?c deck X, score 9/10
- User C: H?c deck X, score 8/10

### Test Case 9.1: View Leaderboard
**Steps:**
1. Hoàn thành session
2. Click "Xem Leaderboard"

**Expected Result:**
- ? Rank #1: User A (10/10)
- ? Rank #2: User B (9/10)
- ? Rank #3: User C (8/10)
- ? Hi?n th? avatar, username, score, date

**API Endpoint:** `GET /api/StudySessions/leaderboard/{deckId}`

---

## ?? Scenario 10: Full User Journey

### Complete Flow (20 phút)
1. ? Register user: `alice@example.com`
2. ? Login
3. ? View Dashboard (empty state)
4. ? Create deck: "English Vocabulary" (Public)
5. ? Add 10 flashcards v?i English words
6. ? Verify auto audio on at least 5 cards
7. ? Study with Flashcard mode ? Finish
8. ? Study with Quiz mode ? Score 8/10
9. ? Study with Match game ? Finish
10. ? View Study History (3 sessions)
11. ? View Leaderboard
12. ? Update profile (username, avatar)
13. ? Change password
14. ? Logout & login v?i password m?i
15. ? Update deck to Private
16. ? Create user 2: `bob@example.com`
17. ? Login as Bob ? Verify không th?y deck private c?a Alice
18. ? Bob study public deck c?a Alice
19. ? Check leaderboard có c? Alice và Bob
20. ? Delete 1 flashcard, delete 1 deck

---

## ?? Scenario 11: Responsive & UI Testing

### Test Case 11.1: Desktop View
**Steps:**
1. M? ? desktop browser (>1024px)

**Expected Result:**
- ? Sidebar c? ??nh bên trái
- ? Main content responsive
- ? Grid layout 3 columns cho decks

---

### Test Case 11.2: Mobile View
**Steps:**
1. Resize browser < 768px
2. Ho?c test trên mobile device

**Expected Result:**
- ? Sidebar thành full width
- ? Grid layout 1 column
- ? Buttons full width
- ? Touch-friendly (padding l?n)

---

### Test Case 11.3: Flip Card Animation
**Steps:**
1. Vào Study Flashcard mode
2. Click vào th?

**Expected Result:**
- ? Smooth 3D flip animation
- ? Front ? Back transition m??t
- ? Click l?i ? flip v?

---

## ?? API Testing (test-api.html)

### Test Case 12.1: Use API Testing Tool
**Steps:**
1. M? https://localhost:5001/test-api.html
2. Register user
3. Login ? Token hi?n th?
4. Test m?i section:
   - Decks: Create, Get, Update, Delete
   - Flashcards: Create, Get, Update, Delete
   - Sessions: Create, Get History, Get Leaderboard
   - Users: Get Profile, Update, Change Password
   - Admin: Get All Users, Delete User, Get All Sessions

**Expected Result:**
- ? M?i request hi?n th? response trong "API Response" box
- ? Status codes ?úng (200, 201, 204, 400, 401, 403, 404)
- ? Token t? ??ng attach vào headers

---

## ?? Test Checklist Summary

### Authentication ?
- [x] Register new user
- [x] Login valid credentials
- [x] Login invalid credentials
- [x] Auto logout on 401
- [x] Token storage & retrieval

### Decks ?
- [x] Create public deck
- [x] Create private deck
- [x] View all decks (own + public)
- [x] View single deck
- [x] Update deck
- [x] Delete deck
- [x] Permission: Cannot edit others' private deck
- [x] Admin can edit/delete any deck

### Flashcards ?
- [x] Create flashcard
- [x] Auto audio generation
- [x] Play audio
- [x] Update flashcard
- [x] Delete flashcard
- [x] Image upload (URL)
- [x] Permission checks

### Study Modes ?
- [x] Flashcard mode works
- [x] Quiz mode works
- [x] Match game works
- [x] Session auto-save
- [x] Progress tracking
- [x] Score calculation

### Progress & Leaderboard ?
- [x] View study history
- [x] View statistics
- [x] View leaderboard
- [x] Leaderboard sorting correct
- [x] Multi-user leaderboard

### Profile ?
- [x] View profile
- [x] Update username
- [x] Update avatar
- [x] Change password
- [x] Password validation

### Admin ?
- [x] View all users
- [x] Delete user
- [x] Cannot delete self
- [x] View all sessions
- [x] Delete session
- [x] Access control (non-admin blocked)

### UI/UX ?
- [x] Responsive design
- [x] Animations smooth
- [x] Loading indicators
- [x] Toast notifications
- [x] Modal dialogs
- [x] Empty states
- [x] Error handling

---

## ?? Known Issues / Future Improvements

### Current Limitations:
- ?? No "Forgot Password" feature
- ?? Audio only works for English words in dictionary
- ?? No image upload to server (URL only)
- ?? No real-time updates (need refresh)
- ?? No social features (comments, likes)

### Planned Features:
- ?? Password reset via email
- ?? Search & filter decks
- ?? Export/import decks (JSON, CSV)
- ?? Spaced repetition algorithm
- ?? Mobile app (React Native)
- ?? Dark mode toggle
- ?? Multi-language support

---

## ?? Test Coverage Goal

| Category | Target | Status |
|----------|--------|--------|
| Authentication | 100% | ? |
| CRUD Operations | 100% | ? |
| Permissions | 100% | ? |
| Study Modes | 100% | ? |
| Audio Feature | 80% | ? |
| UI/UX | 95% | ? |
| Admin Features | 100% | ? |

---

## ?? Testing Best Practices

1. **Test in order:** Auth ? Decks ? Flashcards ? Study ? Profile
2. **Use DevTools:** Monitor Network tab for API calls
3. **Check Console:** Look for errors or warnings
4. **Test edge cases:** Empty inputs, invalid IDs, wrong permissions
5. **Multi-user testing:** Create multiple accounts
6. **Cross-browser:** Test on Chrome, Edge, Firefox
7. **Mobile testing:** Resize or use device emulator

---

## ?? Report Template

```markdown
## Bug Report

**Date:** [Date]
**Tester:** [Your Name]
**Environment:** [Browser, OS, .NET version]

**Test Case:** [Test Case ID]
**Expected:** [What should happen]
**Actual:** [What actually happened]
**Steps to Reproduce:**
1. Step 1
2. Step 2
3. ...

**Screenshots:** [If applicable]
**Console Errors:** [Copy from DevTools]
**API Response:** [Copy from Network tab]
```

---

? **Happy Testing!** ??
