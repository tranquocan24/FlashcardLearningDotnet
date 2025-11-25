// ============================
// ADVANCED UTILITIES
// ============================

// Debounce function for search
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Copy to clipboard
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        showAlert('?ã copy vào clipboard!', 'success');
    }).catch(err => {
        console.error('Copy failed:', err);
    });
}

// Download JSON data
function downloadJSON(data, filename) {
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
}

// Format number with thousands separator
function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

// Calculate percentage
function calculatePercentage(value, total) {
    if (total === 0) return 0;
    return Math.round((value / total) * 100);
}

// Get grade from percentage
function getGrade(percentage) {
    if (percentage >= 90) return { letter: 'A', color: '#28a745' };
    if (percentage >= 80) return { letter: 'B', color: '#17a2b8' };
    if (percentage >= 70) return { letter: 'C', color: '#ffc107' };
    if (percentage >= 60) return { letter: 'D', color: '#fd7e14' };
    return { letter: 'F', color: '#dc3545' };
}

// Shuffle array
function shuffleArray(array) {
    const newArray = [...array];
    for (let i = newArray.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [newArray[i], newArray[j]] = [newArray[j], newArray[i]];
    }
    return newArray;
}

// Get time ago string
function timeAgo(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const seconds = Math.floor((now - date) / 1000);
    
    const intervals = {
        year: 31536000,
        month: 2592000,
        week: 604800,
        day: 86400,
        hour: 3600,
        minute: 60
    };
    
    for (const [unit, secondsInUnit] of Object.entries(intervals)) {
        const interval = Math.floor(seconds / secondsInUnit);
        if (interval >= 1) {
            return `${interval} ${unit}${interval > 1 ? 's' : ''} ago`;
        }
    }
    
    return 'just now';
}

// Validate email
function isValidEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

// Generate random color
function getRandomColor() {
    const colors = [
        '#4257b2', '#28a745', '#dc3545', '#ffc107', 
        '#17a2b8', '#6610f2', '#e83e8c', '#fd7e14'
    ];
    return colors[Math.floor(Math.random() * colors.length)];
}

// Text to Speech (for flashcard pronunciation)
function speak(text, lang = 'en-US') {
    if ('speechSynthesis' in window) {
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = lang;
        window.speechSynthesis.speak(utterance);
    } else {
        showAlert('Trình duy?t không h? tr? Text-to-Speech', 'error');
    }
}

// Full screen API
function toggleFullscreen(element) {
    if (!document.fullscreenElement) {
        element.requestFullscreen().catch(err => {
            console.error('Fullscreen error:', err);
        });
    } else {
        document.exitFullscreen();
    }
}

// Keyboard shortcuts handler
const keyboardShortcuts = {};

function registerShortcut(key, callback, description) {
    keyboardShortcuts[key] = { callback, description };
}

if (typeof document !== 'undefined') {
    document.addEventListener('keydown', (e) => {
        const key = e.key.toLowerCase();
        
        // Check if Ctrl/Cmd is pressed for combinations
        const modifier = e.ctrlKey || e.metaKey;
        
        if (modifier) {
            const shortcutKey = `ctrl+${key}`;
            if (keyboardShortcuts[shortcutKey]) {
                e.preventDefault();
                keyboardShortcuts[shortcutKey].callback();
            }
        } else if (keyboardShortcuts[key]) {
            // Only trigger if not typing in input
            if (!['INPUT', 'TEXTAREA'].includes(e.target.tagName)) {
                e.preventDefault();
                keyboardShortcuts[key].callback();
            }
        }
    });
}

// Export data to CSV
function exportToCSV(data, filename) {
    if (!data || data.length === 0) return;
    
    const headers = Object.keys(data[0]);
    const csvContent = [
        headers.join(','),
        ...data.map(row => headers.map(h => JSON.stringify(row[h] || '')).join(','))
    ].join('\n');
    
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
}

// Confetti effect for celebration
function showConfetti() {
    const colors = ['#ff0000', '#00ff00', '#0000ff', '#ffff00', '#ff00ff', '#00ffff'];
    const confettiCount = 50;
    
    for (let i = 0; i < confettiCount; i++) {
        const confetti = document.createElement('div');
        confetti.className = 'confetti';
        confetti.style.left = Math.random() * 100 + '%';
        confetti.style.background = colors[Math.floor(Math.random() * colors.length)];
        confetti.style.animationDelay = Math.random() * 0.5 + 's';
        confetti.style.animationDuration = (Math.random() * 2 + 2) + 's';
        
        document.body.appendChild(confetti);
        
        setTimeout(() => confetti.remove(), 3000);
    }
}

// Local storage with expiry
const storage = {
    set: (key, value, expiryMinutes = 60) => {
        const item = {
            value: value,
            expiry: new Date().getTime() + (expiryMinutes * 60 * 1000)
        };
        localStorage.setItem(key, JSON.stringify(item));
    },
    
    get: (key) => {
        const itemStr = localStorage.getItem(key);
        if (!itemStr) return null;
        
        const item = JSON.parse(itemStr);
        if (new Date().getTime() > item.expiry) {
            localStorage.removeItem(key);
            return null;
        }
        
        return item.value;
    },
    
    remove: (key) => {
        localStorage.removeItem(key);
    }
};

// Image validator
function isValidImageUrl(url) {
    return /\.(jpg|jpeg|png|gif|webp|svg)$/i.test(url);
}

// Audio validator
function isValidAudioUrl(url) {
    return /\.(mp3|wav|ogg|m4a)$/i.test(url);
}

// Generate UUID (for client-side temp IDs)
function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

// Parse JWT token
function parseJWT(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        
        return JSON.parse(jsonPayload);
    } catch (e) {
        console.error('Invalid token:', e);
        return null;
    }
}

// Check if token is expired
function isTokenExpired(token) {
    const decoded = parseJWT(token);
    if (!decoded || !decoded.exp) return true;
    
    const expiryTime = decoded.exp * 1000; // Convert to milliseconds
    return Date.now() >= expiryTime;
}

// Auto refresh token before expiry
function setupTokenRefresh() {
    const token = auth.getToken();
    if (!token) return;
    
    if (isTokenExpired(token)) {
        auth.logout();
        return;
    }
    
    const decoded = parseJWT(token);
    if (decoded && decoded.exp) {
        const expiryTime = decoded.exp * 1000;
        const timeUntilExpiry = expiryTime - Date.now();
        const refreshTime = timeUntilExpiry - (5 * 60 * 1000); // 5 minutes before expiry
        
        if (refreshTime > 0) {
            setTimeout(() => {
                showAlert('Phiên ??ng nh?p s?p h?t h?n. Vui lòng ??ng nh?p l?i.', 'info');
            }, refreshTime);
        }
    }
}

// Initialize on page load
if (auth.isAuthenticated()) {
    setupTokenRefresh();
}

// Intersection Observer for lazy loading
function setupLazyLoading() {
    const images = document.querySelectorAll('img[data-src]');
    
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.removeAttribute('data-src');
                observer.unobserve(img);
            }
        });
    });
    
    images.forEach(img => imageObserver.observe(img));
}

// Service Worker for offline support (optional)
if ('serviceWorker' in navigator) {
    // Uncomment to enable service worker
    /*
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/sw.js')
            .then(reg => console.log('SW registered:', reg))
            .catch(err => console.log('SW registration failed:', err));
    });
    */
}

// Print flashcards
function printFlashcards(cards) {
    const printWindow = window.open('', '', 'height=600,width=800');
    printWindow.document.write('<html><head><title>Flashcards</title>');
    printWindow.document.write('<style>');
    printWindow.document.write('body { font-family: Arial, sans-serif; padding: 20px; }');
    printWindow.document.write('.card { border: 2px solid #333; padding: 20px; margin: 20px 0; page-break-inside: avoid; }');
    printWindow.document.write('.term { font-size: 1.5rem; font-weight: bold; margin-bottom: 10px; }');
    printWindow.document.write('.definition { font-size: 1.2rem; color: #555; }');
    printWindow.document.write('</style></head><body>');
    
    cards.forEach(card => {
        printWindow.document.write(`
            <div class="card">
                <div class="term">${card.term}</div>
                <div class="definition">${card.definition}</div>
                ${card.example ? `<div style="margin-top:10px; font-style:italic;">${card.example}</div>` : ''}
            </div>
        `);
    });
    
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    printWindow.print();
}

// Share functionality
async function shareContent(title, text, url) {
    if (navigator.share) {
        try {
            await navigator.share({ title, text, url });
        } catch (err) {
            console.log('Share cancelled:', err);
        }
    } else {
        // Fallback: copy link
        copyToClipboard(url || window.location.href);
    }
}

// Notification API
function showNotification(title, options = {}) {
    if ('Notification' in window && Notification.permission === 'granted') {
        new Notification(title, options);
    } else if ('Notification' in window && Notification.permission !== 'denied') {
        Notification.requestPermission().then(permission => {
            if (permission === 'granted') {
                new Notification(title, options);
            }
        });
    }
}

// Study timer
class StudyTimer {
    constructor() {
        this.startTime = null;
        this.endTime = null;
    }
    
    start() {
        this.startTime = new Date();
    }
    
    stop() {
        this.endTime = new Date();
        return this.getDuration();
    }
    
    getDuration() {
        if (!this.startTime || !this.endTime) return 0;
        return Math.floor((this.endTime - this.startTime) / 1000); // seconds
    }
    
    getDurationFormatted() {
        const seconds = this.getDuration();
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    }
}

// Spaced repetition calculator (SM-2 algorithm simplified)
class SpacedRepetition {
    static getNextReviewDate(quality) {
        // quality: 0-5 (0=complete blackout, 5=perfect recall)
        const now = new Date();
        let daysToAdd = 0;
        
        if (quality >= 4) {
            daysToAdd = 7; // Review in 7 days
        } else if (quality >= 3) {
            daysToAdd = 3; // Review in 3 days
        } else if (quality >= 2) {
            daysToAdd = 1; // Review tomorrow
        } else {
            daysToAdd = 0; // Review today again
        }
        
        now.setDate(now.getDate() + daysToAdd);
        return now;
    }
}

// Form validation
function validateForm(formId) {
    const form = document.getElementById(formId);
    const inputs = form.querySelectorAll('input[required], textarea[required]');
    
    let isValid = true;
    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.style.borderColor = 'var(--danger-color)';
            isValid = false;
        } else {
            input.style.borderColor = 'var(--border-color)';
        }
    });
    
    return isValid;
}

// Dark mode toggle
function toggleDarkMode() {
    document.body.classList.toggle('dark-mode');
    const isDark = document.body.classList.contains('dark-mode');
    localStorage.setItem('darkMode', isDark);
}

// Initialize dark mode from localStorage
if (localStorage.getItem('darkMode') === 'true') {
    document.body.classList.add('dark-mode');
}

// Chart drawing helper (simple bar chart)
function drawBarChart(canvasId, data, labels) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    const width = canvas.width;
    const height = canvas.height;
    const barWidth = width / data.length;
    const maxValue = Math.max(...data);
    
    ctx.clearRect(0, 0, width, height);
    
    data.forEach((value, index) => {
        const barHeight = (value / maxValue) * (height - 40);
        const x = index * barWidth + 10;
        const y = height - barHeight - 20;
        
        // Draw bar
        ctx.fillStyle = '#4257b2';
        ctx.fillRect(x, y, barWidth - 20, barHeight);
        
        // Draw value
        ctx.fillStyle = '#333';
        ctx.font = '12px sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText(value, x + (barWidth - 20) / 2, y - 5);
        
        // Draw label
        if (labels && labels[index]) {
            ctx.fillText(labels[index], x + (barWidth - 20) / 2, height - 5);
        }
    });
}

// Create progress ring SVG
function createProgressRing(percentage, size = 120) {
    const radius = (size - 16) / 2;
    const circumference = 2 * Math.PI * radius;
    const offset = circumference - (percentage / 100) * circumference;
    
    return `
        <svg class="progress-ring" width="${size}" height="${size}">
            <circle class="progress-ring-bg" cx="${size/2}" cy="${size/2}" r="${radius}"></circle>
            <circle class="progress-ring-circle" cx="${size/2}" cy="${size/2}" r="${radius}"
                    stroke-dasharray="${circumference}" stroke-dashoffset="${offset}">
            </circle>
            <text x="50%" y="50%" text-anchor="middle" dy=".3em" 
                  style="font-size: ${size/4}px; font-weight: bold; fill: #333;">
                ${percentage}%
            </text>
        </svg>
    `;
}

// Auto-save functionality
class AutoSave {
    constructor(key, saveCallback, interval = 30000) {
        this.key = key;
        this.saveCallback = saveCallback;
        this.interval = interval;
        this.timerId = null;
        this.isDirty = false;
    }
    
    start() {
        this.timerId = setInterval(() => {
            if (this.isDirty) {
                this.save();
            }
        }, this.interval);
    }
    
    stop() {
        if (this.timerId) {
            clearInterval(this.timerId);
        }
    }
    
    markDirty() {
        this.isDirty = true;
    }
    
    async save() {
        if (this.saveCallback) {
            await this.saveCallback();
            this.isDirty = false;
            showAlert('?ã t? ??ng l?u', 'success');
        }
    }
}

// Error handler wrapper
async function handleAsync(asyncFn, errorMessage = '?ã có l?i x?y ra') {
    try {
        return await asyncFn();
    } catch (error) {
        console.error(error);
        showAlert(errorMessage + ': ' + error.message, 'error');
        return null;
    }
}

// Retry logic for failed requests
async function retryRequest(fn, maxRetries = 3, delay = 1000) {
    for (let i = 0; i < maxRetries; i++) {
        try {
            return await fn();
        } catch (error) {
            if (i === maxRetries - 1) throw error;
            await new Promise(resolve => setTimeout(resolve, delay * (i + 1)));
        }
    }
}

// Batch operations
async function batchProcess(items, processor, batchSize = 5) {
    const results = [];
    
    for (let i = 0; i < items.length; i += batchSize) {
        const batch = items.slice(i, i + batchSize);
        const batchResults = await Promise.all(batch.map(processor));
        results.push(...batchResults);
    }
    
    return results;
}

// Study statistics calculator
function calculateStudyStats(sessions) {
    if (!sessions || sessions.length === 0) {
        return {
            totalSessions: 0,
            totalCards: 0,
            averageScore: 0,
            bestScore: 0,
            totalTime: 0,
            streak: 0
        };
    }
    
    const totalSessions = sessions.length;
    const totalCards = sessions.reduce((sum, s) => sum + s.totalCards, 0);
    const totalCorrect = sessions.reduce((sum, s) => sum + s.score, 0);
    const averageScore = totalCards > 0 ? (totalCorrect / totalCards * 100).toFixed(1) : 0;
    const bestScore = Math.max(...sessions.map(s => 
        s.totalCards > 0 ? (s.score / s.totalCards * 100) : 0
    ));
    
    // Calculate streak (consecutive days studied)
    let streak = 0;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    const sortedDates = sessions
        .map(s => new Date(s.dateStudied))
        .sort((a, b) => b - a);
    
    let currentDate = new Date(today);
    for (const sessionDate of sortedDates) {
        sessionDate.setHours(0, 0, 0, 0);
        const diffDays = Math.floor((currentDate - sessionDate) / (1000 * 60 * 60 * 24));
        
        if (diffDays === 0 || diffDays === 1) {
            if (diffDays === 1) streak++;
            currentDate = new Date(sessionDate);
        } else {
            break;
        }
    }
    
    return {
        totalSessions,
        totalCards,
        averageScore,
        bestScore: bestScore.toFixed(1),
        streak
    };
}
