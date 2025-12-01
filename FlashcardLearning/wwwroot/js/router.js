// ============================
// SIMPLE SPA ROUTER
// ============================

const routes = {
    '/login': renderLogin,
    '/register': renderRegister,
    '/dashboard': renderDashboard,
    '/decks': renderDecks,
    '/deck': renderDeckDetail,
    '/study': renderStudy,
    '/folders': renderFolders,
    '/folder': renderFolderDetail,
    '/profile': renderProfile,
    '/history': renderHistory,
    '/admin': renderAdmin,
    '/logout': handleLogout
};

function navigate(path) {
    window.location.hash = path;
}

async function router() {
    let hash = window.location.hash.slice(1) || '/';
    
    // Parse route and params
    let [route, queryString] = hash.split('?');
    let params = new URLSearchParams(queryString);
    
    // Check authentication
    if (!auth.isAuthenticated() && route !== '/login' && route !== '/register') {
        window.location.hash = '#/login';
        return;
    }
    
    if (auth.isAuthenticated() && (route === '/login' || route === '/register' || route === '/')) {
        window.location.hash = '#/dashboard';
        return;
    }
    
    // Find route handler
    let routeHandler = routes[route];
    
    // Handle parameterized routes
    if (!routeHandler) {
        if (route.startsWith('/deck/')) {
            const id = route.split('/deck/')[1];
            params.set('id', id);
            routeHandler = renderDeckDetail;
        } else if (route.startsWith('/study/')) {
            const id = route.split('/study/')[1];
            params.set('id', id);
            routeHandler = renderStudy;
        } else if (route.startsWith('/folder/')) {
            const id = route.split('/folder/')[1];
            params.set('id', id);
            routeHandler = renderFolderDetail;
        }
    }
    
    if (routeHandler) {
        await routeHandler(params);
    } else {
        renderNotFound();
    }
}

window.addEventListener('hashchange', router);
window.addEventListener('load', router);

// ============================
// AUTH PAGES
// ============================
function renderLogin() {
    document.getElementById('app').innerHTML = `
        <div class="auth-container">
            <div class="auth-box">
                <h2>Login</h2>
                <div class="form-group">
                    <label>Email</label>
                    <input type="email" id="loginEmail" placeholder="example@email.com">
                </div>
                <div class="form-group">
                    <label>Password</label>
                    <input type="password" id="loginPassword" placeholder="Enter password">
                </div>
                <button class="btn btn-primary btn-full" onclick="handleLogin()">Login</button>
                <p class="text-center mt-2">
                    <a href="#/register" class="link">Don't have an account? Register now</a>
                </p>
            </div>
        </div>
    `;
}

function renderRegister() {
    document.getElementById('app').innerHTML = `
        <div class="auth-container">
            <div class="auth-box">
                <h2>Register</h2>
                <div class="form-group">
                    <label>Username</label>
                    <input type="text" id="registerUsername" placeholder="Your name">
                </div>
                <div class="form-group">
                    <label>Email</label>
                    <input type="email" id="registerEmail" placeholder="example@email.com">
                </div>
                <div class="form-group">
                    <label>Password</label>
                    <input type="password" id="registerPassword" placeholder="Minimum 6 characters">
                </div>
                <button class="btn btn-primary btn-full" onclick="handleRegister()">Register</button>
                <p class="text-center mt-2">
                    <a href="#/login" class="link">Already have an account? Login</a>
                </p>
            </div>
        </div>
    `;
}

async function handleLogin() {
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;
    
    if (!email || !password) {
        showAlert('Please fill in all fields', 'error');
        return;
    }
    
    const data = await apiCall('/Auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password })
    });
    
    if (data && data.token) {
        auth.setToken(data.token);
        auth.setUserInfo({
            userId: data.userId,
            username: data.username,
            role: data.role
        });
        showAlert('Login successful!', 'success');
        navigate('/dashboard');
    }
}

async function handleRegister() {
    const username = document.getElementById('registerUsername').value;
    const email = document.getElementById('registerEmail').value;
    const password = document.getElementById('registerPassword').value;
    
    if (!username || !email || !password) {
        showAlert('Please fill in all fields', 'error');
        return;
    }
    
    if (password.length < 6) {
        showAlert('Password must be at least 6 characters', 'error');
        return;
    }
    
    const data = await apiCall('/Auth/register', {
        method: 'POST',
        body: JSON.stringify({ username, email, password })
    });
    
    if (data) {
        showAlert('Registration successful! Please login.', 'success');
        navigate('/login');
    }
}

function handleLogout() {
    auth.logout();
}

// ============================
// DASHBOARD PAGE
// ============================
async function renderDashboard() {
    const [profile, decks, history] = await Promise.all([
        apiCall('/Users/profile'),
        apiCall('/Decks'),
        apiCall('/StudySessions/history')
    ]);
    
    const totalDecks = decks?.length || 0;
    const totalCards = decks?.reduce((sum, d) => sum + (d.flashcards?.length || 0), 0) || 0;
    const totalSessions = history?.length || 0;
    const avgScore = totalSessions > 0 
        ? (history.reduce((sum, s) => sum + (s.totalCards > 0 ? (s.score / s.totalCards) * 100 : 0), 0) / totalSessions).toFixed(1)
        : 0;
    
    const recentHistory = history?.slice(0, 5) || [];
    
    const content = `
        <div class="page-header">
            <h1>Dashboard</h1>
        </div>
        
        <div class="stats-grid">
            <div class="stat-card">
                <h3>${totalDecks}</h3>
                <p>My Decks</p>
            </div>
            <div class="stat-card success">
                <h3>${totalCards}</h3>
                <p>Total Cards</p>
            </div>
            <div class="stat-card warning">
                <h3>${totalSessions}</h3>
                <p>Study Sessions</p>
            </div>
            <div class="stat-card danger">
                <h3>${avgScore}%</h3>
                <p>Average Score</p>
            </div>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3 class="card-title">Recent Study History</h3>
            </div>
            ${recentHistory.length === 0 ? 
                renderEmptyState('No study history yet', 'Start studying a deck!') :
                `<div class="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>Deck</th>
                                <th>Mode</th>
                                <th>Score</th>
                                <th>Date</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${recentHistory.map(s => `
                                <tr>
                                    <td>${s.deckTitle}</td>
                                    <td><span class="badge badge-primary">${s.mode}</span></td>
                                    <td><strong>${s.score}/${s.totalCards}</strong> (${((s.score/s.totalCards)*100).toFixed(0)}%)</td>
                                    <td>${formatDate(s.dateStudied)}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>`
            }
        </div>
        
        <div class="btn-group">
            <a href="#/decks" class="btn btn-primary">View All Decks</a>
            <a href="#/profile" class="btn btn-secondary">View Profile</a>
        </div>
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

// ============================
// DECKS PAGE
// ============================
async function renderDecks() {
    const decks = await apiCall('/Decks');
    
    const content = `
        <div class="page-header">
            <h1>My Decks</h1>
            <div class="btn-group">
                <button class="btn btn-secondary" onclick="navigate('/folders')">Manage Folders</button>
                <button class="btn btn-primary" onclick="showCreateDeckModal()">+ Create New Deck</button>
            </div>
        </div>
        
        ${!decks || decks.length === 0 ? 
            renderEmptyState( 'No decks yet', 'Create your first deck to start learning!') :
            `<div class="deck-grid">
                ${decks.map(deck => `
                    <div class="deck-item" onclick="navigate('/deck/${deck.id}')">
                        <h3>${deck.title}</h3>
                        <p>${deck.description || 'No description'}</p>
                        ${deck.folderId ? '<div class="current-folder-badge">In Folder</div>' : ''}
                        <div class="deck-meta">
                            <div>
                                <span class="badge ${deck.isPublic ? 'badge-public' : 'badge-private'}">
                                    ${deck.isPublic ? 'Public' : 'Private'}
                                </span>
                                <span class="text-muted" style="margin-left:10px;">
                                    ${deck.flashcardCount || 0} cards
                                </span>
                            </div>
                            <div class="deck-actions" onclick="event.stopPropagation()">
                                <button class="btn btn-sm btn-success" onclick="navigate('/study/${deck.id}')">Study</button>
                                <button class="btn btn-sm btn-warning" onclick="showMoveDeckModal('${deck.id}', '${deck.folderId || ''}')">Folder</button>
                                <button class="btn btn-sm btn-secondary" onclick="showEditDeckModal('${deck.id}')">Edit</button>
                                <button class="btn btn-sm btn-danger" onclick="deleteDeck('${deck.id}')">Delete</button>
                            </div>
                        </div>
                    </div>
                `).join('')}
            </div>`
        }
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

function showCreateDeckModal() {
    const modalContent = `
        <div class="form-group">
            <label>Deck Name *</label>
            <input type="text" id="deckTitle" placeholder="e.g. IELTS Vocabulary">
        </div>
        <div class="form-group">
            <label>Description</label>
            <textarea id="deckDescription" placeholder="Brief description of the deck..."></textarea>
        </div>
        <div class="form-group">
            <label>Folder (Optional)</label>
            <select id="deckFolderId">
                <option value="">-- No Folder --</option>
            </select>
        </div>
        <div class="form-group">
            <div class="checkbox-group">
                <input type="checkbox" id="deckIsPublic">
                <label>Make public for everyone</label>
            </div>
        </div>
    `;
    
    openModal('Create New Deck', modalContent, [
        { text: 'Cancel', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'Create', class: 'btn-primary', onclick: 'createDeck()' }
    ]);
    
    // Load folders into dropdown
    populateFolderDropdown('deckFolderId');
}

async function showEditDeckModal(deckId) {
    const deck = await apiCall(`/Decks/${deckId}`);
    if (!deck) return;
    
    const modalContent = `
        <div class="form-group">
            <label>Deck Name *</label>
            <input type="text" id="editDeckTitle" value="${deck.title}">
        </div>
        <div class="form-group">
            <label>Description</label>
            <textarea id="editDeckDescription">${deck.description || ''}</textarea>
        </div>
        <div class="form-group">
            <label>Folder (Optional)</label>
            <select id="editDeckFolderId">
                <option value="">-- No Folder --</option>
            </select>
        </div>
        <div class="form-group">
            <div class="checkbox-group">
                <input type="checkbox" id="editDeckIsPublic" ${deck.isPublic ? 'checked' : ''}>
                <label>Make public for everyone</label>
            </div>
        </div>
    `;
    
    openModal('Edit Deck', modalContent, [
        { text: 'Cancel', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'Save', class: 'btn-primary', onclick: `updateDeck('${deckId}')` }
    ]);
    
    // Load folders into dropdown with current selection
    populateFolderDropdown('editDeckFolderId', deck.folderId);
}

async function createDeck() {
    const title = document.getElementById('deckTitle').value;
    const description = document.getElementById('deckDescription').value;
    const isPublic = document.getElementById('deckIsPublic').checked;
    const folderId = document.getElementById('deckFolderId').value || null;
    
    if (!title) {
        showAlert('Please enter deck name', 'error');
        return;
    }
    
    const data = await apiCall('/Decks', {
        method: 'POST',
        body: JSON.stringify({ title, description, isPublic, folderId })
    });
    
    if (data) {
        showAlert('Deck created successfully!', 'success');
        closeModal();
        renderDecks();
    }
}

async function updateDeck(deckId) {
    const title = document.getElementById('editDeckTitle').value;
    const description = document.getElementById('editDeckDescription').value;
    const isPublic = document.getElementById('editDeckIsPublic').checked;
    const folderId = document.getElementById('editDeckFolderId').value || null;
    
    if (!title) {
        showAlert('Please enter deck name', 'error');
        return;
    }
    
    const data = await apiCall(`/Decks/${deckId}`, {
        method: 'PUT',
        body: JSON.stringify({ title, description, isPublic, folderId })
    });
    
    if (data) {
        showAlert('Updated successfully!', 'success');
        closeModal();
        renderDecks();
    }
}

async function deleteDeck(deckId) {
    if (!confirm('Are you sure you want to delete this deck?')) return;
    
    const data = await apiCall(`/Decks/${deckId}`, {
        method: 'DELETE'
    });
    
    if (data) {
        showAlert('Deck deleted successfully!', 'success');
        renderDecks();
    }
}

// ============================
// DECK DETAIL PAGE
// ============================
let currentDeckId = null;

// ============================
// DEBOUNCE UTILITY
// ============================
function debounce(func, delay) {
    let timeout;
    return function(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), delay);
    };
}

// ============================
// AUTO-TRANSLATION LOOKUP
// ============================
async function lookupWord(word) {
    if (!word || word.trim().length === 0) return;
    
    try {
        // Show loading indicator
        const definitionInput = document.getElementById('cardDefinition');
        if (!definitionInput) return;
        
        // Only suggest if Definition field is EMPTY
        if (definitionInput.value && definitionInput.value.trim() !== '') {
            return;
        }
        
        // Add placeholder to show loading
        const originalPlaceholder = definitionInput.placeholder;
        definitionInput.placeholder = 'Looking up...';
        
        const data = await apiCall(`/Dictionary/lookup?word=${encodeURIComponent(word.trim())}`);
        
        // Restore placeholder
        definitionInput.placeholder = originalPlaceholder;
        
        if (data && data.success && data.meaning) {
            // Only fill if field is still EMPTY (user hasn't typed anything)
            if (!definitionInput.value || definitionInput.value.trim() === '') {
                definitionInput.value = data.meaning;
                definitionInput.style.borderColor = '#28a745'; // Green border
                
                // Reset border after 2s
                setTimeout(() => {
                    definitionInput.style.borderColor = '';
                }, 2000);
            }
        }
    } catch (error) {
        console.error('Translation lookup error:', error);
    }
}

// Debounced version (1 second delay)
const debouncedLookup = debounce(lookupWord, 1000);

async function renderDeckDetail(params) {
    const deckId = params.get('id');
    if (!deckId) {
        navigate('/decks');
        return;
    }
    
    currentDeckId = deckId;
    
    const deck = await apiCall(`/Decks/${deckId}`);
    if (!deck) {
        navigate('/decks');
        return;
    }
    
    const cards = deck.flashcards || [];
    
    const content = `
        <div class="page-header">
            <div>
                <a href="#/decks" class="link">Back</a>
                <h1>${deck.title}</h1>
                <p class="text-muted">${deck.description || 'No description'}</p>
            </div>
            <button class="btn btn-success" onclick="navigate('/study/${deck.id}')">Start Study</button>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3 class="card-title">Flashcards (${cards.length})</h3>
                <button class="btn btn-primary btn-sm" onclick="showCreateFlashcardModal('${deckId}')">+ Add Card</button>
            </div>
            
            ${cards.length === 0 ? 
                renderEmptyState('No flashcards yet', 'Add your first flashcard to get started!') :
                `<div class="flashcard-list">
                    ${cards.map(card => `
                        <div class="flashcard-item">
                            <div class="flashcard-term"><strong>${card.term}</strong></div>
                            <div class="flashcard-definition">${card.definition}</div>
                            ${card.example ? `<div class="flashcard-example"><em>${card.example}</em></div>` : ''}
                            ${card.imageUrl ? `<img src="${card.imageUrl}" class="flashcard-image" alt="${card.term}">` : ''}
                            ${card.audioUrl ? `
                                <div class="audio-player">
                                    <button onclick="playAudio('${card.audioUrl}')">Play Audio</button>
                                    <span class="text-muted" style="font-size:0.8rem;">Auto-generated</span>
                                </div>
                            ` : ''}
                            <div class="flashcard-actions">
                                <button class="btn btn-sm btn-secondary" onclick="showEditFlashcardModal('${card.id}', '${deckId}')">Edit</button>
                                <button class="btn btn-sm btn-danger" onclick="deleteFlashcard('${card.id}', '${deckId}')">Delete</button>
                            </div>
                        </div>
                    `).join('')}
                </div>`
            }
        </div>
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

async function refreshDeckDetail() {
    if (currentDeckId) {
        const params = new URLSearchParams();
        params.set('id', currentDeckId);
        await renderDeckDetail(params);
    }
}

function showCreateFlashcardModal(deckId) {
    const modalContent = `
        <div class="form-group">
            <label>Term (English) *</label>
            <input type="text" id="cardTerm" placeholder="e.g. Hello">
            <small class="text-muted">Enter an English word and wait 1 second for auto-suggestion</small>
        </div>
        <div class="form-group">
            <label>Definition (Vietnamese) *</label>
            <input type="text" id="cardDefinition" >
        </div>
        <div class="form-group">
            <label>Example</label>
            <input type="text" id="cardExample" placeholder="e.g. Hello, how are you?">
        </div>
        <div class="form-group">
            <label>Image URL</label>
            <input type="text" id="cardImageUrl" placeholder="https://example.com/image.jpg">
        </div>
        <div class="alert alert-info">
            <small>Audio will be auto-generated from Term (if available in dictionary)</small>
        </div>
    `;
    
    openModal('Add New Flashcard', modalContent, [
        { text: 'Cancel', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'Create', class: 'btn-primary', onclick: `createFlashcard('${deckId}')` }
    ]);
    
    // ==============================
    // ATTACH AUTO-TRANSLATION LISTENER
    // ==============================
    setTimeout(() => {
        const termInput = document.getElementById('cardTerm');
        if (termInput) {
            termInput.addEventListener('input', (e) => {
                const word = e.target.value;
                if (word && word.trim().length > 0) {
                    debouncedLookup(word);
                }
            });
        }
    }, 100); // Wait for modal to render
}

async function showEditFlashcardModal(cardId, deckId) {
    const card = await apiCall(`/Flashcards/${cardId}`);
    if (!card) return;
    
    const modalContent = `
        <div class="form-group">
            <label>Term *</label>
            <input type="text" id="editCardTerm" value="${card.term}">
        </div>
        <div class="form-group">
            <label>Definition *</label>
            <input type="text" id="editCardDefinition" value="${card.definition}">
        </div>
        <div class="form-group">
            <label>Example</label>
            <input type="text" id="editCardExample" value="${card.example || ''}">
        </div>
        <div class="form-group">
            <label>Image URL</label>
            <input type="text" id="editCardImageUrl" value="${card.imageUrl || ''}">
        </div>
    `;
    
    openModal('Edit Flashcard', modalContent, [
        { text: 'Cancel', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'Save', class: 'btn-primary', onclick: `updateFlashcard('${cardId}', '${deckId}')` }
    ]);
}

async function createFlashcard(deckId) {
    const term = document.getElementById('cardTerm').value;
    const definition = document.getElementById('cardDefinition').value;
    const example = document.getElementById('cardExample').value;
    const imageUrl = document.getElementById('cardImageUrl').value;
    
    if (!term || !definition) {
        showAlert('Please enter Term and Definition', 'error');
        return;
    }
    
    const data = await apiCall('/Flashcards', {
        method: 'POST',
        body: JSON.stringify({
            deckId,
            term,
            definition,
            example,
            imageUrl
        })
    });
    
    if (data) {
        showAlert('Flashcard added successfully!', 'success');
        closeModal();
        await refreshDeckDetail();
    }
}

async function updateFlashcard(cardId, deckId) {
    const term = document.getElementById('editCardTerm').value;
    const definition = document.getElementById('editCardDefinition').value;
    const example = document.getElementById('editCardExample').value;
    const imageUrl = document.getElementById('editCardImageUrl').value;
    
    if (!term || !definition) {
        showAlert('Please enter Term and Definition', 'error');
        return;
    }
    
    const data = await apiCall(`/Flashcards/${cardId}`, {
        method: 'PUT',
        body: JSON.stringify({
            id: cardId,
            deckId,
            term,
            definition,
            example,
            imageUrl
        })
    });
    
    if (data) {
        showAlert('Updated successfully!', 'success');
        closeModal();
        await refreshDeckDetail();
    }
}

async function deleteFlashcard(cardId, deckId) {
    if (!confirm('Are you sure you want to delete this flashcard?')) return;
    
    const data = await apiCall(`/Flashcards/${cardId}`, {
        method: 'DELETE'
    });
    
    if (data) {
        showAlert('Flashcard deleted successfully!', 'success');
        await refreshDeckDetail();
    }
}

// ============================
// STUDY PAGE
// ============================
async function renderStudy(params) {
    const deckId = params.get('id');
    if (!deckId) {
        navigate('/decks');
        return;
    }
    
    const deck = await apiCall(`/Decks/${deckId}`);
    if (!deck || !deck.flashcards || deck.flashcards.length === 0) {
        showAlert('This deck has no flashcards yet!', 'error');
        navigate(`/deck/${deckId}`);
        return;
    }
    
    const content = `
        <div class="page-header">
            <div>
                <a href="#/deck/${deckId}" class="link">Back</a>
                <h1>Study: ${deck.title}</h1>
            </div>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3>Choose Study Mode</h3>
            </div>
            <div class="btn-group">
                <button class="btn btn-primary" onclick="startFlashcardMode('${deckId}', ${JSON.stringify(deck.flashcards).replace(/"/g, '&quot;')})">
                    Flashcard Mode
                </button>
                <button class="btn btn-success" onclick="startQuizMode('${deckId}', ${JSON.stringify(deck.flashcards).replace(/"/g, '&quot;')})">
                    Quiz Mode
                </button>
                <button class="btn btn-warning" onclick="startMatchMode('${deckId}', ${JSON.stringify(deck.flashcards).replace(/"/g, '&quot;')})">
                    Match Game
                </button>
            </div>
        </div>
        
        <div id="studyArea"></div>
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

// Flashcard Mode
let currentCardIndex = 0;
let studyCards = [];
let studyDeckId = '';

function startFlashcardMode(deckId, cards) {
    // Reset all state variables
    studyCards = JSON.parse(JSON.stringify(cards));
    studyDeckId = deckId;
    currentCardIndex = 0;
    renderFlashcardView();
}

function renderFlashcardView() {
    if (currentCardIndex >= studyCards.length) {
        finishStudySession('Flashcard', studyCards.length, studyCards.length);
        return;
    }
    
    const card = studyCards[currentCardIndex];
    const progress = ((currentCardIndex + 1) / studyCards.length) * 100;
    
    document.getElementById('studyArea').innerHTML = `
        <div class="study-container">
            <div class="progress-bar">
                <div class="progress-fill" style="width: ${progress}%"></div>
            </div>
            <p class="text-center text-muted mb-2">Card ${currentCardIndex + 1} / ${studyCards.length}</p>
            
            <div class="flip-card-container">
                <div class="flip-card" id="flipCard" onclick="document.getElementById('flipCard').classList.toggle('flipped')">
                    <div class="flip-card-front">
                        <div>
                            <h2>${card.term}</h2>
                            <p class="text-muted mt-2">Click to flip</p>
                            ${card.audioUrl ? `
                                <button class="btn btn-success btn-sm mt-2" onclick="event.stopPropagation(); playAudio('${card.audioUrl}')">
                                    Play Audio
                                </button>
                            ` : ''}
                        </div>
                    </div>
                    <div class="flip-card-back">
                        <div>
                            <h3>${card.definition}</h3>
                            ${card.example ? `<p class="mt-2" style="font-size:1rem; font-style:italic;">${card.example}</p>` : ''}
                        </div>
                    </div>
                </div>
            </div>
            
            <div class="study-controls">
                <button class="btn btn-secondary" onclick="prevCard()" ${currentCardIndex === 0 ? 'disabled' : ''}>
                    Previous
                </button>
                <button class="btn btn-primary" onclick="nextCard()">
                    ${currentCardIndex === studyCards.length - 1 ? 'Finish' : 'Next'}
                </button>
            </div>
        </div>
    `;
}

function prevCard() {
    if (currentCardIndex > 0) {
        currentCardIndex--;
        renderFlashcardView();
    }
}

function nextCard() {
    currentCardIndex++;
    renderFlashcardView();
}

// Quiz Mode
let quizCards = [];
let quizDeckId = '';
let currentQuizIndex = 0;
let quizScore = 0;
let selectedAnswer = null;

function startQuizMode(deckId, cards) {
    // Reset all state variables
    quizCards = JSON.parse(JSON.stringify(cards));
    quizDeckId = deckId;
    currentQuizIndex = 0;
    quizScore = 0;
    selectedAnswer = null;
    renderQuizView();
}

function renderQuizView() {
    if (currentQuizIndex >= quizCards.length) {
        finishStudySession('Quiz', quizScore, quizCards.length);
        return;
    }
    
    const card = quizCards[currentQuizIndex];
    const progress = ((currentQuizIndex + 1) / quizCards.length) * 100;
    
    const otherCards = quizCards.filter((_, i) => i !== currentQuizIndex);
    const wrongAnswers = otherCards
        .sort(() => Math.random() - 0.5)
        .slice(0, 3)
        .map(c => c.definition);
    
    const allAnswers = [card.definition, ...wrongAnswers]
        .sort(() => Math.random() - 0.5);
    
    document.getElementById('studyArea').innerHTML = `
        <div class="study-container">
            <div class="progress-bar">
                <div class="progress-fill" style="width: ${progress}%"></div>
            </div>
            <p class="text-center text-muted mb-2">Question ${currentQuizIndex + 1} / ${quizCards.length}</p>
            <p class="text-center">Score: ${quizScore}</p>
            
            <div class="quiz-question">
                <h2 style="margin-bottom: 20px;">${card.term}</h2>
                <div class="quiz-options" id="quizOptions">
                    ${allAnswers.map((answer, idx) => `
                        <div class="quiz-option" onclick="selectQuizAnswer('${answer.replace(/'/g, "\\'")}', '${card.definition.replace(/'/g, "\\'")}', ${idx})">
                            ${answer}
                        </div>
                    `).join('')}
                </div>
                <div id="quizFeedback"></div>
                <button class="btn btn-primary btn-full mt-2" id="quizNextBtn" style="display:none;" onclick="nextQuiz()">
                    Next
                </button>
            </div>
        </div>
    `;
}

function selectQuizAnswer(selected, correct, idx) {
    if (selectedAnswer !== null) return;
    
    selectedAnswer = selected;
    const options = document.querySelectorAll('.quiz-option');
    
    if (selected === correct) {
        options[idx].classList.add('correct');
        quizScore++;
        document.getElementById('quizFeedback').innerHTML = `
            <div class="alert alert-success mt-2">Correct!</div>
        `;
    } else {
        options[idx].classList.add('incorrect');
        options.forEach((opt, i) => {
            if (opt.textContent.trim() === correct) {
                opt.classList.add('correct');
            }
        });
        document.getElementById('quizFeedback').innerHTML = `
            <div class="alert alert-error mt-2">Wrong! Correct answer: ${correct}</div>
        `;
    }
    
    document.getElementById('quizNextBtn').style.display = 'block';
}

function nextQuiz() {
    selectedAnswer = null;
    currentQuizIndex++;
    renderQuizView();
}

// Match Game Mode
let matchCards = [];
let matchDeckId = '';
let matchSelected = [];
let matchedPairs = 0;
let matchAttempts = 0;

function startMatchMode(deckId, cards) {
    // Reset all state variables
    matchCards = JSON.parse(JSON.stringify(cards));
    matchDeckId = deckId;
    matchSelected = [];
    matchedPairs = 0;
    matchAttempts = 0;
    renderMatchView();
}

function renderMatchView() {
    const terms = matchCards.map((c, i) => ({ id: i, text: c.term, type: 'term' }));
    const definitions = matchCards.map((c, i) => ({ id: i, text: c.definition, type: 'definition' }));
    
    const allCards = [...terms, ...definitions].sort(() => Math.random() - 0.5);
    
    document.getElementById('studyArea').innerHTML = `
        <div class="study-container">
            <div class="card-header">
                <h3>Match Game</h3>
                <div>
                    <span>Matched: ${matchedPairs}/${matchCards.length}</span> |
                    <span>Attempts: ${matchAttempts}</span>
                </div>
            </div>
            
            <div class="match-grid" id="matchGrid">
                ${allCards.map((item, idx) => `
                    <div class="match-card" data-id="${item.id}" data-type="${item.type}" id="match-${idx}" onclick="selectMatchCard(${idx}, ${item.id}, '${item.type}')">
                        ${item.text}
                    </div>
                `).join('')}
            </div>
            
            <div class="text-center mt-3">
                <p class="text-muted">Select a Term and matching Definition</p>
            </div>
        </div>
    `;
}

function selectMatchCard(idx, id, type) {
    const card = document.getElementById(`match-${idx}`);
    
    if (card.classList.contains('matched')) return;
    
    if (matchSelected.length === 0) {
        card.classList.add('selected');
        matchSelected.push({ idx, id, type, element: card });
    } else if (matchSelected.length === 1) {
        const first = matchSelected[0];
        
        if (first.idx === idx) {
            card.classList.remove('selected');
            matchSelected = [];
            return;
        }
        
        matchAttempts++;
        
        if (first.type !== type && first.id === id) {
            card.classList.add('matched');
            first.element.classList.remove('selected');
            first.element.classList.add('matched');
            matchedPairs++;
            matchSelected = [];
            
            if (matchedPairs === matchCards.length) {
                setTimeout(() => {
                    finishStudySession('Match', matchedPairs, matchCards.length);
                }, 500);
            }
        } else {
            card.classList.add('wrong');
            first.element.classList.add('wrong');
            
            setTimeout(() => {
                card.classList.remove('wrong');
                first.element.classList.remove('selected', 'wrong');
                matchSelected = [];
            }, 800);
        }
    }
}

async function finishStudySession(mode, score, total) {
    const currentDeckId = studyDeckId || quizDeckId || matchDeckId;
    
    const data = await apiCall('/StudySessions', {
        method: 'POST',
        body: JSON.stringify({
            deckId: currentDeckId,
            mode: mode,
            score: score,
            totalCards: total
        })
    });
    
    if (data) {
        const percentage = ((score / total) * 100).toFixed(0);
        const emoji = percentage >= 80 ? 'exellent' : percentage >= 50 ? 'good' : 'bad';
        const message = `
            <div class="text-center">
                <h2 style="font-size: 3rem; margin-bottom: 20px;">${emoji}</h2>
                <h3>Completed!</h3>
                <p style="font-size: 1.5rem; margin: 20px 0;">
                    <strong>${score}/${total}</strong> (${percentage}%)
                </p>
                <p class="text-muted">Mode: ${mode}</p>
            </div>
        `;
        
        openModal('Study Results', message, [
            { text: 'View Leaderboard', class: 'btn-warning', onclick: `showLeaderboard('${currentDeckId}')` },
            { text: 'Study Again', class: 'btn-primary', onclick: `restartStudySession('${currentDeckId}')` },
            { text: 'Go to Dashboard', class: 'btn-secondary', onclick: `closeModal(); navigate('/dashboard')` }
        ]);
    }
}

// New function to restart study session and clear the study area
function restartStudySession(deckId) {
    closeModal();
    
    // Clear all game state variables
    currentCardIndex = 0;
    studyCards = [];
    studyDeckId = '';
    currentQuizIndex = 0;
    quizScore = 0;
    selectedAnswer = null;
    quizCards = [];
    quizDeckId = '';
    matchCards = [];
    matchDeckId = '';
    matchSelected = [];
    matchedPairs = 0;
    matchAttempts = 0;
    
    // Clear the study area completely
    const studyArea = document.getElementById('studyArea');
    if (studyArea) {
        studyArea.innerHTML = '';
    }
    
    // Small delay to ensure clean state, then navigate
    setTimeout(() => {
        navigate(`/study/${deckId}`);
    }, 100);
}

// Alternative: Add a function to show leaderboard
async function showLeaderboard(deckId) {
    const leaderboard = await apiCall(`/StudySessions/leaderboard/${deckId}`);
    
    if (!leaderboard || leaderboard.length === 0) {
        showAlert('No leaderboard data yet', 'info');
        return;
    }
    
    const content = `
        <div>
            ${leaderboard.map((entry, idx) => `
                <div class="leaderboard-item">
                    <div class="leaderboard-rank">#${idx + 1}</div>
                    <img src="${entry.avatar || 'https://via.placeholder.com/50'}" class="leaderboard-avatar" alt="${entry.userName}">
                    <div class="leaderboard-info">
                        <div><strong>${entry.userName}</strong></div>
                        <div class="text-muted" style="font-size:0.85rem;">${formatDate(entry.date)}</div>
                    </div>
                    <div class="leaderboard-score">${entry.score}/${entry.totalCards}</div>
                </div>
            `).join('')}
        </div>
    `;
    
    openModal('Leaderboard', content, [
        { text: 'Close', class: 'btn-secondary', onclick: 'closeModal()' }
    ]);
}

// ============================
// PROFILE PAGE
// ============================
async function renderProfile() {
    const profile = await apiCall('/Users/profile');
    
    if (!profile) return;
    
    const content = `
        <div class="page-header">
            <h1>My Profile</h1>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3>Personal Information</h3>
            </div>
            ${profile.avatarUrl ? `
                <div style="text-align: center; margin-bottom: 20px;">
                    <img src="${profile.avatarUrl}" alt="Avatar" style="width: 100px; height: 100px; border-radius: 50%; object-fit: cover;">
                </div>
            ` : ''}
            <div class="form-group">
                <label>Username</label>
                <input type="text" id="profileUsername" value="${profile.username}">
            </div>
            <div class="form-group">
                <label>Email</label>
                <input type="email" value="${profile.email}" disabled>
            </div>
            <div class="form-group">
                <label>Avatar URL</label>
                <input type="text" id="profileAvatar" value="${profile.avatarUrl || ''}" placeholder="https://example.com/avatar.jpg">
            </div>
            <div class="form-group">
                <label>Role</label>
                <input type="text" value="${profile.role}" disabled>
            </div>
            <button class="btn btn-primary" onclick="updateProfile()">Save Changes</button>
        </div>
        
        <div class="card mt-3">
            <div class="card-header">
                <h3>Change Password</h3>
            </div>
            <div class="form-group">
                <label>Old Password</label>
                <input type="password" id="oldPassword">
            </div>
            <div class="form-group">
                <label>New Password</label>
                <input type="password" id="newPassword">
            </div>
            <div class="form-group">
                <label>Confirm New Password</label>
                <input type="password" id="confirmPassword">
            </div>
            <button class="btn btn-warning" onclick="changePassword()">Change Password</button>
        </div>
        
        <div class="card mt-3">
            <div class="card-header">
                <h3>Statistics</h3>
            </div>
            <p>Total Decks: <strong>${profile.deckCount}</strong></p>
            <p>Member Since: <strong>${formatDate(profile.createdAt)}</strong></p>
            <div class="mt-2">
                <a href="#/history" class="btn btn-primary">View Full Study History</a>
            </div>
        </div>
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

async function updateProfile() {
    const username = document.getElementById('profileUsername').value;
    const avatarUrl = document.getElementById('profileAvatar').value;
    
    if (!username) {
        showAlert('Username cannot be empty', 'error');
        return;
    }
    
    const data = await apiCall('/Users/profile', {
        method: 'PUT',
        body: JSON.stringify({ username, avatarUrl })
    });
    
    if (data) {
        const userInfo = auth.getUserInfo();
        userInfo.username = username;
        auth.setUserInfo(userInfo);
        
        showAlert('Profile updated successfully!', 'success');
        renderProfile();
    }
}

async function changePassword() {
    const oldPassword = document.getElementById('oldPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    
    if (!oldPassword || !newPassword || !confirmPassword) {
        showAlert('Please fill in all fields', 'error');
        return;
    }
    
    if (newPassword !== confirmPassword) {
        showAlert('New passwords do not match!', 'error');
        return;
    }
    
    if (newPassword.length < 6) {
        showAlert('New password must be at least 6 characters', 'error');
        return;
    }
    
    const data = await apiCall('/Users/change-password', {
        method: 'PUT',
        body: JSON.stringify({ oldPassword, newPassword })
    });
    
    if (data) {
        showAlert('Password changed successfully!', 'success');
        document.getElementById('oldPassword').value = '';
        document.getElementById('newPassword').value = '';
        document.getElementById('confirmPassword').value = '';
    }
}

// ============================
// HISTORY PAGE
// ============================
async function renderHistory() {
    const history = await apiCall('/StudySessions/history');
    
    const content = `
        <div class="page-header">
            <h1>Study History</h1>
        </div>
        
        ${!history || history.length === 0 ? 
            renderEmptyState('No study history yet', 'Start studying a deck!') :
            `
            <div class="card">
                <div class="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>Date</th>
                                <th>Deck</th>
                                <th>Mode</th>
                                <th>Score</th>
                                <th>Percentage</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${history.map(s => {
                                const percentage = ((s.score / s.totalCards) * 100).toFixed(0);
                                const grade = getGrade(parseInt(percentage));
                                return `
                                    <tr>
                                        <td>${formatDate(s.dateStudied)}</td>
                                        <td>
                                            <a href="#/deck/${s.deckId}" class="link">${s.deckTitle}</a>
                                        </td>
                                        <td><span class="badge badge-primary">${s.mode}</span></td>
                                        <td><strong>${s.score}/${s.totalCards}</strong></td>
                                        <td>
                                            <span style="color: ${grade.color}; font-weight: bold;">
                                                ${percentage}% (${grade.letter})
                                            </span>
                                        </td>
                                    </tr>
                                `;
                            }).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
            
            <div class="card mt-3">
                <div class="card-header">
                    <h3>Overall Statistics</h3>
                </div>
                ${renderStudyStats(history)}
            </div>
            `
        }
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

function renderStudyStats(sessions) {
    if (!sessions || sessions.length === 0) return '<p>No data available</p>';
    
    const stats = calculateStudyStats(sessions);
    
    return `
        <div class="stats-grid">
            <div class="stat-card">
                <h3>${stats.totalSessions}</h3>
                <p>Total Sessions</p>
            </div>
            <div class="stat-card success">
                <h3>${stats.totalCards}</h3>
                <p>Total Cards Studied</p>
            </div>
            <div class="stat-card warning">
                <h3>${stats.averageScore}%</h3>
                <p>Average Score</p>
            </div>
            <div class="stat-card danger">
                <h3>${stats.streak}</h3>
                <p>Study Streak (days)</p>
            </div>
        </div>
        
        <div class="mt-3">
            <p><strong>Statistics by Mode:</strong></p>
            <ul>
                ${['Flashcard', 'Quiz', 'Match'].map(mode => {
                    const modeSessions = sessions.filter(s => s.mode === mode);
                    const count = modeSessions.length;
                    const avgScore = count > 0 
                        ? (modeSessions.reduce((sum, s) => sum + (s.score / s.totalCards * 100), 0) / count).toFixed(1)
                        : 0;
                    return `<li>${mode}: ${count} sessions (${avgScore}% average)</li>`;
                }).join('')}}
            </ul>
        </div>
    `;
}

function getGrade(percentage) {
    if (percentage >= 90) return { letter: 'A', color: '#28a745' };
    if (percentage >= 80) return { letter: 'B', color: '#17a2b8' };
    if (percentage >= 70) return { letter: 'C', color: '#ffc107' };
    if (percentage >= 60) return { letter: 'D', color: '#fd7e14' };
    return { letter: 'F', color: '#dc3545' };
}

function calculateStudyStats(sessions) {
    if (!sessions || sessions.length === 0) {
        return {
            totalSessions: 0,
            totalCards: 0,
            averageScore: 0,
            bestScore: 0,
            streak: 0
        };
    }
    
    const totalSessions = sessions.length;
    const totalCards = sessions.reduce((sum, s) => sum + s.totalCards, 0);
    const totalCorrect = sessions.reduce((sum, s) => sum + s.score, 0);
    const averageScore = totalCards > 0 ? (totalCorrect / totalCards * 100).toFixed(1) : 0;
    
    let streak = 0;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    const sortedDates = [...new Set(
        sessions.map(s => {
            const d = new Date(s.dateStudied);
            d.setHours(0, 0, 0, 0);
            return d.getTime();
        })
    )].sort((a, b) => b - a);
    
    let currentDate = today.getTime();
    for (const sessionDate of sortedDates) {
        const diffDays = Math.floor((currentDate - sessionDate) / (1000 * 60 * 60 * 24));
        
        if (diffDays === 0 || diffDays === 1) {
            streak++;
            currentDate = sessionDate;
        } else {
            break;
        }
    }
    
    return {
        totalSessions,
        totalCards,
        averageScore,
        streak
    };
}

// ============================
// ADMIN PAGE
// ============================
async function renderAdmin() {
    const userInfo = auth.getUserInfo();
    
    if (userInfo?.role !== 'Admin') {
        showAlert('You do not have permission to access this page', 'error');
        navigate('/dashboard');
        return;
    }
    
    const [users, allHistory] = await Promise.all([
        apiCall('/Users'),
        apiCall('/StudySessions/admin/all-history')
    ]);
    
    const content = `
        <div class="page-header">
            <h1>Admin Panel</h1>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3>User Management</h3>
                <button class="btn btn-primary btn-sm" onclick="showAdminUpdateUserModal()">Update User</button>
            </div>
            ${!users || users.length === 0 ? 
                renderEmptyState('No users', '') :
                `<div class="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Username</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Created</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${users.map(u => `
                                <tr>
                                    <td><code style="font-size:0.75rem;">${u.id}</code></td>
                                    <td>${u.username}</td>
                                    <td>${u.email}</td>
                                    <td><span class="badge badge-primary">${u.role}</span></td>
                                    <td>${formatDate(u.createdAt)}</td>
                                    <td>
                                        <button class="btn btn-sm btn-warning" onclick="showQuickUpdateModal('${u.email}')">Edit</button>
                                        <button class="btn btn-sm btn-danger" onclick="deleteUser('${u.id}')">Delete</button>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>`
            }
        </div>
        
        <div class="card mt-3">
            <div class="card-header">
                <h3>System Study History</h3>
            </div>
            ${!allHistory || allHistory.length === 0 ? 
                renderEmptyState('No history', '') :
                `<div class="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>User</th>
                                <th>Deck</th>
                                <th>Mode</th>
                                <th>Score</th>
                                <th>Date</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${allHistory.slice(0, 20).map(s => `
                                <tr>
                                    <td>${s.user?.username || 'Unknown'}</td>
                                    <td>${s.deck?.title || 'Deleted'}</td>
                                    <td><span class="badge badge-primary">${s.mode}</span></td>
                                    <td>${s.score}/${s.totalCards}</td>
                                    <td>${formatDate(s.dateStudied)}</td>
                                    <td>
                                        <button class="btn btn-sm btn-danger" onclick="deleteSession('${s.id}')">Delete</button>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>`
            }
        </div>
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

// Show modal ?? Admin update user (form ??y ??)
function showAdminUpdateUserModal() {
    const modalContent = `
        <div class="form-group">
            <label>Current Email *</label>
            <input type="email" id="adminCurrentEmail" placeholder="user@example.com">
            <small class="text-muted">Email c?a user c?n c?p nh?t</small>
        </div>
        <div class="form-group">
            <label>New Email (optional)</label>
            <input type="email" id="adminNewEmail" placeholder="newemail@example.com">
            <small class="text-muted">?? tr?ng n?u không ??i email</small>
        </div>
        <div class="form-group">
            <label>New Password (optional)</label>
            <input type="password" id="adminNewPassword" placeholder="New password">
            <small class="text-muted">?? tr?ng n?u không ??i m?t kh?u</small>
        </div>
    `;
    
    openModal('Admin: Update User', modalContent, [
        { text: 'Cancel', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'Update', class: 'btn-primary', onclick: 'adminUpdateUser()' }
    ]);
}

// Quick update modal khi b?m Edit t? b?ng (t? ??ng ?i?n email)
function showQuickUpdateModal(currentEmail) {
    const modalContent = `
        <div class="form-group">
            <label>Current Email</label>
            <input type="email" id="adminCurrentEmail" value="${currentEmail}" readonly>
        </div>
        <div class="form-group">
            <label>New Email (optional)</label>
            <input type="email" id="adminNewEmail" placeholder="newemail@example.com">
        </div>
        <div class="form-group">
            <label>New Password (optional)</label>
            <input type="password" id="adminNewPassword" placeholder="New password">
            
        </div>
    `;
    
    openModal('Edit User: ' + currentEmail, modalContent, [
        { text: 'Cancel', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'Update', class: 'btn-warning', onclick: 'adminUpdateUser()' }
    ]);
}

async function adminUpdateUser() {
    const currentEmail = document.getElementById('adminCurrentEmail').value.trim();
    const newEmail = document.getElementById('adminNewEmail').value.trim();
    const newPassword = document.getElementById('adminNewPassword').value.trim();
    
    if (!currentEmail) {
        showAlert('Please enter current email', 'error');
        return;
    }
    
    if (!newEmail && !newPassword) {
        showAlert('Please enter at least New Email or New Password', 'error');
        return;
    }
    
    const data = await apiCall('/Users/admin/update-user', {
        method: 'PUT',
        body: JSON.stringify({ 
            email: currentEmail, 
            newEmail: newEmail || null, 
            newPassword: newPassword || null 
        })
    });
    
    if (data) {
        showAlert('User updated successfully!', 'success');
        closeModal();
        renderAdmin();
    }
}

async function deleteUser(userId) {
    if (!confirm('Are you sure you want to delete this user? This action cannot be undone.')) return;
    
    const data = await apiCall(`/Users/${userId}`, {
        method: 'DELETE'
    });
    
    if (data !== null) {
        showAlert('User deleted successfully!', 'success');
        renderAdmin();
    }
}

// ============================
// NOT FOUND PAGE
// ============================
function renderNotFound() {
    const content = renderEmptyState('404 - Page Not Found', 'The page you are looking for does not exist');
    document.getElementById('app').innerHTML = renderLayout(content);
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
                    <h2>Flashcard Learning</h2>
                    <div class="user-info">
                        <div>${userInfo?.username || 'User'}</div>
                        <div style="font-size:0.75rem; opacity:0.7">${userInfo?.role || 'User'}</div>
                    </div>
                </div>
                <ul class="sidebar-menu">
                    <li><a href="#/dashboard" class="${currentHash.includes('dashboard') ? 'active' : ''}">Dashboard</a></li>
                    <li><a href="#/decks" class="${currentHash.includes('decks') || currentHash.includes('deck/') ? 'active' : ''}">My Decks</a></li>
                    <li><a href="#/folders" class="${currentHash.includes('folder') ? 'active' : ''}">Folders</a></li>
                    <li><a href="#/history" class="${currentHash.includes('history') ? 'active' : ''}">Study History</a></li>
                    <li><a href="#/profile" class="${currentHash.includes('profile') ? 'active' : ''}">Profile</a></li>
                    ${isAdmin ? `<li><a href="#/admin" class="${currentHash.includes('admin') ? 'active' : ''}">Admin Panel</a></li>` : ''}
                    <li style="margin-top: 20px;"><a href="#/logout" style="color:#ff6b6b;">Logout</a></li>
                </ul>
                <div style="position: absolute; bottom: 20px; left: 20px; right: 20px; font-size: 0.75rem; opacity: 0.5; text-align: center;">
                    <a href="/guide.html" target="_blank" style="color: white;">Guide</a> | 
                    ${isAdmin ? `<a href="/admin-guide.html" target="_blank" style="color: #ffc107;">Admin Guide</a> | ` : ''}
                    <a href="/test-api.html" target="_blank" style="color: white;">API Test</a>
                </div>
            </aside>
            <main class="main-content">
                ${content}
            </main>
        </div>
    `;
}
