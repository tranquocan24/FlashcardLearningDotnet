# ?? How to Clear Browser Cache

## ? Problem
You see Vietnamese text or old content even after updating the code.

## ? Solutions

### Method 1: Hard Reload (Fastest)
**Windows/Linux:**
- Press `Ctrl + Shift + R`
- Or `Ctrl + F5`

**Mac:**
- Press `Cmd + Shift + R`
- Or `Cmd + Option + R`

### Method 2: Clear Cache via DevTools
1. Open DevTools (`F12`)
2. Right-click the **Refresh button** 
3. Select "**Empty Cache and Hard Reload**"

### Method 3: Clear All Browser Cache
**Chrome:**
1. Press `Ctrl + Shift + Delete`
2. Select "Cached images and files"
3. Click "Clear data"

**Edge:**
1. Press `Ctrl + Shift + Delete`
2. Select "Cached images and files"
3. Click "Clear now"

### Method 4: Use Incognito/Private Mode
- **Chrome**: `Ctrl + Shift + N`
- **Edge**: `Ctrl + Shift + P`
- **Firefox**: `Ctrl + Shift + P`

### Method 5: Disable Cache (For Development)
1. Open DevTools (`F12`)
2. Go to **Network** tab
3. Check "**Disable cache**"
4. Keep DevTools open while browsing

## ?? Quick Test
After clearing cache:
1. Go to `https://localhost:{port}/`
2. Login or Register
3. Check if you see:
   - "My Decks" (not "B? th? c?a tôi")
   - "Total Cards" (not "T?ng s? th?")
   - "Study Sessions" (not "L??t h?c")
   - "Average Score" (not "?i?m trung bình")

## ? All Text Should Be in English Now!

The following files have been updated to English:
- ? `/wwwroot/index.html` - Changed `lang="en"` and added cache busting
- ? `/wwwroot/js/router.js` - All UI text converted to English
- ? `/wwwroot/js/app.js` - Error messages and helpers in English
- ? `/wwwroot/welcome.html` - Fully translated

## ?? Still Seeing Vietnamese?
If you still see Vietnamese text after clearing cache:

1. **Check the URL bar** - Make sure you're on the right page
2. **Check file version** - Open DevTools > Sources tab > Check if `router.js?v=2` is loaded
3. **Check console** - Look for any JavaScript errors
4. **Try another browser** - Test in Chrome, Edge, or Firefox

## ?? Files with Cache Busting
The following files now include `?v=2` parameter to force reload:
```html
<link rel="stylesheet" href="/css/style.css?v=2">
<script src="/js/app.js?v=2"></script>
<script src="/js/router.js?v=2"></script>
```

## ?? Ready to Go!
Once cache is cleared, you should see a fully English interface! ??
