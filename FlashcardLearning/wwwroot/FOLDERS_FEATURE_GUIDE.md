# ?? FOLDERS FEATURE - USER GUIDE

## ? ?ã hoàn thành tích h?p tính n?ng Folders

### ?? T?ng quan
Tính n?ng Folders cho phép b?n t? ch?c các Decks thành các th? m?c ?? qu?n lý d? dàng h?n.

### ?? Các tính n?ng chính

#### 1. **Qu?n lý Folders** (`#/folders`)
- ? Xem danh sách t?t c? folders
- ? T?o folder m?i (Tên + Mô t?)
- ? Xóa folder (Decks bên trong s? KHÔNG b? xóa, ch? tr? thành unassigned)
- ? Xem s? l??ng decks trong m?i folder
- ? Xem danh sách Unassigned Decks

#### 2. **Folder Detail** (`#/folder/{id}`)
- ? Xem t?t c? decks trong folder
- ? Di chuy?n deck ra kh?i folder
- ? Truy c?p nhanh vào các decks

#### 3. **Di chuy?n Deck vào Folder**
- ? T? trang My Decks: Nút "?? Move"
- ? T? Folder Detail: Nút "?? Move"
- ? Modal v?i dropdown ch?n folder ?ích
- ? Option "Remove from folder" ?? unassign

#### 4. **Tích h?p vào Create/Edit Deck**
- ? Thêm dropdown "Folder" khi t?o/s?a deck
- ? Có th? ch?n folder ngay khi t?o deck m?i

### ?? UI/UX Features
- ? **Folder Grid**: Hi?n th? folders d?ng card ??p m?t v?i gradient màu
- ? **Icon h? th?ng**: S? d?ng emoji ?? ?? ??? cho tr?c quan
- ? **Badge indicator**: Hi?n th? "?? In Folder" trên deck card
- ? **Empty states**: Thông báo rõ ràng khi ch?a có folder/deck
- ? **Responsive design**: Ho?t ??ng t?t trên mobile

### ?? Technical Implementation

#### **API Endpoints ???c s? d?ng:**
```javascript
GET  /api/Folders                    // L?y danh sách folders
POST /api/Folders                    // T?o folder m?i
GET  /api/Folders/{id}               // Chi ti?t folder + decks bên trong
DELETE /api/Folders/{id}             // Xóa folder
GET  /api/Folders/unassigned-decks   // L?y decks không có folder
PUT  /api/Decks/{id}                 // Update deck (v?i folderId)
```

#### **Files ?ã t?o/c?p nh?t:**
1. ? **wwwroot/js/folders.js** - Logic x? lý folders
2. ? **wwwroot/css/style.css** - Styles cho folders
3. ? **wwwroot/js/router.js** - Routes & navigation
4. ? **wwwroot/js/app.js** - Shared functions
5. ? **wwwroot/index.html** - Load scripts

### ?? Navigation Flow

```
Dashboard
  ??> Folders (/folders)
       ??> Create New Folder (Modal)
       ??> View Folder Detail (/folder/{id})
       ?    ??> Move Deck (Modal)
       ??> View Unassigned Decks

My Decks (/decks)
  ??> Create Deck (có option ch?n folder)
  ??> Edit Deck (có option ??i folder)
  ??> Move Deck (Modal ch?n folder)
```

### ?? User Stories

#### Story 1: T?o và qu?n lý folders
```
1. User click "Folders" trong sidebar
2. Click "+ Create New Folder"
3. Nh?p tên (required) và mô t? (optional)
4. Click "Create" ? Folder ???c t?o
5. Th?y folder m?i xu?t hi?n trong grid
```

#### Story 2: Di chuy?n deck vào folder
```
1. T? "My Decks", click nút "?? Move" trên m?t deck
2. Modal hi?n dropdown danh sách folders
3. Ch?n folder ?ích
4. Click "Move" ? Deck ???c chuy?n vào folder
5. Badge "?? In Folder" xu?t hi?n trên deck card
```

#### Story 3: Xem decks trong folder
```
1. T? "Folders", click vào m?t folder
2. Th?y danh sách t?t c? decks trong folder ?ó
3. Có th? click vào deck ?? xem chi ti?t
4. Có th? di chuy?n deck ra folder khác
```

#### Story 4: Xóa folder (decks gi? nguyên)
```
1. Click nút "Delete" trên folder card
2. Confirm dialog: "Decks s? không b? xóa"
3. Click OK ? Folder b? xóa
4. Decks t? ??ng thành "Unassigned"
5. Xem l?i ? ph?n "Unassigned Decks"
```

### ?? CSS Classes ???c thêm

```css
.folder-grid                 /* Grid layout cho folders */
.folder-item                 /* Card style cho folder */
.folder-icon                 /* Icon size cho folder emoji */
.folder-meta                 /* Metadata (deck count, date) */
.folder-actions              /* Buttons (View, Delete) */
.folder-detail-header        /* Header gradient cho folder detail */
.folder-badge                /* Badge hi?n th? s? decks */
.folder-select-container     /* Container cho dropdown */
.current-folder-badge        /* Badge "In Folder" trên deck card */
```

### ?? JavaScript Functions

#### folders.js
```javascript
renderFolders()              // Render folders list page
loadUnassignedDecks()        // Load decks không có folder
renderFolderDetail(params)   // Render folder detail page
showCreateFolderModal()      // Hi?n modal t?o folder
createFolder()               // X? lý t?o folder
deleteFolder(id)             // X? lý xóa folder
showMoveDeckModal(id, fid)   // Hi?n modal di chuy?n deck
moveDeck(id)                 // X? lý di chuy?n deck
populateFolderDropdown(sel)  // ?? data vào dropdown
```

### ?? Important Notes

1. **Xóa folder KHÔNG xóa decks**
   - Khi xóa folder, t?t c? decks bên trong s? tr? thành "Unassigned"
   - Backend t? ??ng set `folderId = null`

2. **FolderId là nullable**
   - Deck có th? có ho?c không có folder
   - `null` = deck không thu?c folder nào

3. **Permission**
   - User ch? th?y folders c?a chính mình
   - Admin có th? th?y t?t c? (n?u backend implement)

4. **Validation**
   - Folder name: Required, max 200 chars
   - Description: Optional, max 1000 chars

### ?? Testing Checklist

- [ ] T?o folder m?i thành công
- [ ] Xóa folder ? decks không b? m?t
- [ ] Di chuy?n deck vào folder
- [ ] Di chuy?n deck ra kh?i folder (unassign)
- [ ] T?o deck m?i v?i folder ngay t? ??u
- [ ] Edit deck ?? ??i folder
- [ ] Xem chi ti?t folder hi?n th? ?úng decks
- [ ] Unassigned decks hi?n th? ??y ??
- [ ] Navigation gi?a các trang ho?t ??ng t?t
- [ ] Modal ?óng/m? m??t mà
- [ ] Responsive trên mobile

### ?? Support

N?u có l?i ho?c c?n thêm tính n?ng:
1. Check Console (F12) xem có API errors
2. Verify backend API ho?t ??ng (test v?i Postman/Swagger)
3. Clear browser cache và refresh
4. Check Network tab xem request/response

### ?? Enjoy!

Bây gi? b?n có th? t? ch?c decks m?t cách khoa h?c h?n! ???
