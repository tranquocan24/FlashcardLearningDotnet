// ============================
// GLOBAL CONFIGURATION
// ============================
const API_URL = '/api';

// ============================
// LOCAL STORAGE HELPERS
// ============================
const auth = {
    getToken: () => localStorage.getItem('token'),
    setToken: (token) => localStorage.setItem('token', token),
    removeToken: () => localStorage.removeItem('token'),
    getUserInfo: () => {
        const user = localStorage.getItem('userInfo');
        return user ? JSON.parse(user) : null;
    },
    setUserInfo: (user) => localStorage.setItem('userInfo', JSON.stringify(user)),
    removeUserInfo: () => localStorage.removeItem('userInfo'),
    isAuthenticated: () => !!localStorage.getItem('token'),
    logout: () => {
        auth.removeToken();
        auth.removeUserInfo();
        window.location.hash = '#/login';
    }
};

// ============================
// API HELPERS
// ============================
async function apiCall(endpoint, options = {}) {
    const token = auth.getToken();
    
    const config = {
        headers: {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` }),
            ...options.headers
        },
        ...options
    };

    try {
        showLoading();
        const response = await fetch(API_URL + endpoint, config);
        
        // Handle unauthorized
        if (response.status === 401) {
            auth.logout();
            return null;
        }

        // Handle forbidden
        if (response.status === 403) {
            showAlert('You do not have permission to perform this action', 'error');
            return null;
        }

        // Handle not found
        if (response.status === 404) {
            showAlert('Data not found', 'error');
            return null;
        }

        // Handle no content (successful delete/update)
        if (response.status === 204) {
            return { success: true };
        }

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || data.title || 'API Error');
        }

        return data;
    } catch (error) {
        console.error('API Error:', error);
        showAlert(error.message || 'An error occurred', 'error');
        return null;
    } finally {
        hideLoading();
    }
}

// ============================
// UI HELPERS
// ============================
function showLoading() {
    let loader = document.getElementById('loading');
    if (loader) loader.style.display = 'flex';
}

function hideLoading() {
    let loader = document.getElementById('loading');
    if (loader) loader.style.display = 'none';
}

function showAlert(message, type = 'info') {
    const alertClass = type === 'error' ? 'alert-error' : 
                       type === 'success' ? 'alert-success' : 'alert-info';
    
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert ${alertClass}`;
    alertDiv.textContent = message;
    alertDiv.style.position = 'fixed';
    alertDiv.style.top = '20px';
    alertDiv.style.right = '20px';
    alertDiv.style.zIndex = '10000';
    alertDiv.style.minWidth = '300px';
    alertDiv.style.animation = 'slideInRight 0.3s';
    
    document.body.appendChild(alertDiv);
    
    setTimeout(() => {
        alertDiv.remove();
    }, 3000);
}

// ============================
// MODAL HELPERS
// ============================
function openModal(title, content, footerButtons = []) {
    const modal = document.createElement('div');
    modal.className = 'modal-overlay';
    modal.id = 'modalOverlay';
    
    const buttonsHTML = footerButtons.map(btn => 
        `<button class="btn ${btn.class || 'btn-primary'}" onclick="${btn.onclick}">${btn.text}</button>`
    ).join('');
    
    modal.innerHTML = `
        <div class="modal">
            <div class="modal-header">
                <h3>${title}</h3>
                <button class="modal-close" onclick="closeModal()">&times;</button>
            </div>
            <div class="modal-body">
                ${content}
            </div>
            ${footerButtons.length ? `<div class="modal-footer">${buttonsHTML}</div>` : ''}
        </div>
    `;
    
    document.body.appendChild(modal);
    
    // Close on outside click
    modal.addEventListener('click', (e) => {
        if (e.target === modal) closeModal();
    });
}

function closeModal() {
    const modal = document.getElementById('modalOverlay');
    if (modal) modal.remove();
}

// ============================
// DATE FORMATTING
// ============================
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// ============================
// RENDER LAYOUT
// ============================
function renderLayout(content) {
    const userInfo = auth.getUserInfo();
    const isAdmin = userInfo?.role === 'Admin';
    const currentHash = window.location.hash;
    
    return `
        <div class="app-container">
            <aside class="sidebar">
                <div class="sidebar-header">
                    <h2>?? Flashcard</h2>
                    <div class="user-info">
                        <div>?? ${userInfo?.username || 'User'}</div>
                        <div style="font-size:0.75rem; opacity:0.7">${userInfo?.role || 'User'}</div>
                    </div>
                </div>
                <ul class="sidebar-menu">
                    <li><a href="#/dashboard" class="${currentHash.includes('dashboard') ? 'active' : ''}">?? Dashboard</a></li>
                    <li><a href="#/decks" class="${currentHash.includes('decks') || currentHash.includes('deck/') ? 'active' : ''}">?? My Decks</a></li>
                    <li><a href="#/folders" class="${currentHash.includes('folder') ? 'active' : ''}">?? Folders</a></li>
                    <li><a href="#/history" class="${currentHash.includes('history') ? 'active' : ''}">?? Study History</a></li>
                    <li><a href="#/profile" class="${currentHash.includes('profile') ? 'active' : ''}">?? Profile</a></li>
                    ${isAdmin ? `<li><a href="#/admin" class="${currentHash.includes('admin') ? 'active' : ''}">?? Admin Panel</a></li>` : ''}
                    <li style="margin-top: 20px;"><a href="#/logout" style="color:#ff6b6b;">?? Logout</a></li>
                </ul>
            </aside>
            <main class="main-content">
                ${content}
            </main>
        </div>
    `;
}

// ============================
// EMPTY STATE
// ============================
function renderEmptyState(icon, title, description) {
    return `
        <div class="empty-state">
            <div class="empty-state-icon">${icon}</div>
            <h3>${title}</h3>
            <p>${description}</p>
        </div>
    `;
}

// ============================
// AUDIO PLAYER
// ============================
function playAudio(url) {
    if (!url) {
        showAlert('No audio file available', 'error');
        return;
    }
    
    const audio = new Audio(url);
    audio.play().catch(err => {
        console.error('Audio error:', err);
        showAlert('Cannot play audio', 'error');
    });
}
