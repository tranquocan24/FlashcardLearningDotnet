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
                <h2>?? ??ng Nh?p</h2>
                <div class="form-group">
                    <label>Email</label>
                    <input type="email" id="loginEmail" placeholder="example@email.com">
                </div>
                <div class="form-group">
                    <label>M?t kh?u</label>
                    <input type="password" id="loginPassword" placeholder="Nh?p m?t kh?u">
                </div>
                <button class="btn btn-primary btn-full" onclick="handleLogin()">??ng Nh?p</button>
                <p class="text-center mt-2">
                    <a href="#/register" class="link">Ch?a có tài kho?n? ??ng ký ngay</a>
                </p>
            </div>
        </div>
    `;
}

function renderRegister() {
    document.getElementById('app').innerHTML = `
        <div class="auth-container">
            <div class="auth-box">
                <h2>?? ??ng Ký</h2>
                <div class="form-group">
                    <label>Tên ng??i dùng</label>
                    <input type="text" id="registerUsername" placeholder="Tên c?a b?n">
                </div>
                <div class="form-group">
                    <label>Email</label>
                    <input type="email" id="registerEmail" placeholder="example@email.com">
                </div>
                <div class="form-group">
                    <label>M?t kh?u</label>
                    <input type="password" id="registerPassword" placeholder="T?i thi?u 6 ký t?">
                </div>
                <button class="btn btn-primary btn-full" onclick="handleRegister()">??ng Ký</button>
                <p class="text-center mt-2">
                    <a href="#/login" class="link">?ã có tài kho?n? ??ng nh?p</a>
                </p>
            </div>
        </div>
    `;
}

async function handleLogin() {
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;
    
    if (!email || !password) {
        showAlert('Vui lòng ?i?n ??y ?? thông tin', 'error');
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
        showAlert('??ng nh?p thành công!', 'success');
        navigate('/dashboard');
    }
}

async function handleRegister() {
    const username = document.getElementById('registerUsername').value;
    const email = document.getElementById('registerEmail').value;
    const password = document.getElementById('registerPassword').value;
    
    if (!username || !email || !password) {
        showAlert('Vui lòng ?i?n ??y ?? thông tin', 'error');
        return;
    }
    
    if (password.length < 6) {
        showAlert('M?t kh?u ph?i có ít nh?t 6 ký t?', 'error');
        return;
    }
    
    const data = await apiCall('/Auth/register', {
        method: 'POST',
        body: JSON.stringify({ username, email, password })
    });
    
    if (data) {
        showAlert('??ng ký thành công! Hãy ??ng nh?p.', 'success');
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
            <h1>?? Dashboard</h1>
        </div>
        
        <div class="stats-grid">
            <div class="stat-card">
                <h3>${totalDecks}</h3>
                <p>B? th? c?a tôi</p>
            </div>
            <div class="stat-card success">
                <h3>${totalCards}</h3>
                <p>T?ng s? th?</p>
            </div>
            <div class="stat-card warning">
                <h3>${totalSessions}</h3>
                <p>L??t h?c</p>
            </div>
            <div class="stat-card danger">
                <h3>${avgScore}%</h3>
                <p>?i?m trung bình</p>
            </div>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3 class="card-title">?? L?ch s? h?c g?n ?ây</h3>
            </div>
            ${recentHistory.length === 0 ? 
                renderEmptyState('??', 'Ch?a có l?ch s? h?c t?p', 'Hãy b?t ??u h?c m?t b? th?!') :
                `<div class="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>B? th?</th>
                                <th>Ch? ??</th>
                                <th>?i?m s?</th>
                                <th>Th?i gian</th>
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
            <a href="#/decks" class="btn btn-primary">?? Xem t?t c? b? th?</a>
            <a href="#/profile" class="btn btn-secondary">?? Xem h? s?</a>
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
            <h1>?? B? Th? C?a Tôi</h1>
            <button class="btn btn-primary" onclick="showCreateDeckModal()">+ T?o B? Th? M?i</button>
        </div>
        
        ${!decks || decks.length === 0 ? 
            renderEmptyState('??', 'Ch?a có b? th? nào', 'T?o b? th? ??u tiên ?? b?t ??u h?c!') :
            `<div class="deck-grid">
                ${decks.map(deck => `
                    <div class="deck-item" onclick="navigate('/deck/${deck.id}')">
                        <h3>${deck.title}</h3>
                        <p>${deck.description || 'Không có mô t?'}</p>
                        <div class="deck-meta">
                            <div>
                                <span class="badge ${deck.isPublic ? 'badge-public' : 'badge-private'}">
                                    ${deck.isPublic ? '?? Công khai' : '?? Riêng t?'}
                                </span>
                                <span class="text-muted" style="margin-left:10px;">
                                    ${deck.flashcards?.length || 0} th?
                                </span>
                            </div>
                            <div class="deck-actions" onclick="event.stopPropagation()">
                                <button class="btn btn-sm btn-primary" onclick="navigate('/study/${deck.id}')">?? H?c</button>
                                <button class="btn btn-sm btn-secondary" onclick="showEditDeckModal('${deck.id}')">??</button>
                                <button class="btn btn-sm btn-danger" onclick="deleteDeck('${deck.id}')">???</button>
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
            <label>Tên b? th? *</label>
            <input type="text" id="deckTitle" placeholder="VD: T? v?ng IELTS">
        </div>
        <div class="form-group">
            <label>Mô t?</label>
            <textarea id="deckDescription" placeholder="Mô t? ng?n v? b? th?..."></textarea>
        </div>
        <div class="form-group">
            <div class="checkbox-group">
                <input type="checkbox" id="deckIsPublic">
                <label>Công khai cho m?i ng??i</label>
            </div>
        </div>
    `;
    
    openModal('T?o B? Th? M?i', modalContent, [
        { text: 'H?y', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'T?o M?i', class: 'btn-primary', onclick: 'createDeck()' }
    ]);
}

async function showEditDeckModal(deckId) {
    const deck = await apiCall(`/Decks/${deckId}`);
    if (!deck) return;
    
    const modalContent = `
        <div class="form-group">
            <label>Tên b? th? *</label>
            <input type="text" id="editDeckTitle" value="${deck.title}">
        </div>
        <div class="form-group">
            <label>Mô t?</label>
            <textarea id="editDeckDescription">${deck.description || ''}</textarea>
        </div>
        <div class="form-group">
            <div class="checkbox-group">
                <input type="checkbox" id="editDeckIsPublic" ${deck.isPublic ? 'checked' : ''}>
                <label>Công khai cho m?i ng??i</label>
            </div>
        </div>
    `;
    
    openModal('Ch?nh S?a B? Th?', modalContent, [
        { text: 'H?y', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'L?u', class: 'btn-primary', onclick: `updateDeck('${deckId}')` }
    ]);
}

async function createDeck() {
    const title = document.getElementById('deckTitle').value;
    const description = document.getElementById('deckDescription').value;
    const isPublic = document.getElementById('deckIsPublic').checked;
    
    if (!title) {
        showAlert('Vui lòng nh?p tên b? th?', 'error');
        return;
    }
    
    const data = await apiCall('/Decks', {
        method: 'POST',
        body: JSON.stringify({ title, description, isPublic })
    });
    
    if (data) {
        showAlert('T?o b? th? thành công!', 'success');
        closeModal();
        renderDecks();
    }
}

async function updateDeck(deckId) {
    const title = document.getElementById('editDeckTitle').value;
    const description = document.getElementById('editDeckDescription').value;
    const isPublic = document.getElementById('editDeckIsPublic').checked;
    
    if (!title) {
        showAlert('Vui lòng nh?p tên b? th?', 'error');
        return;
    }
    
    const data = await apiCall(`/Decks/${deckId}`, {
        method: 'PUT',
        body: JSON.stringify({ id: deckId, title, description, isPublic })
    });
    
    if (data) {
        showAlert('C?p nh?t thành công!', 'success');
        closeModal();
        renderDecks();
    }
}

async function deleteDeck(deckId) {
    if (!confirm('B?n có ch?c mu?n xóa b? th? này?')) return;
    
    const data = await apiCall(`/Decks/${deckId}`, {
        method: 'DELETE'
    });
    
    if (data) {
        showAlert('Xóa b? th? thành công!', 'success');
        renderDecks();
    }
}

// ============================
// DECK DETAIL PAGE
// ============================
async function renderDeckDetail(params) {
    const deckId = params.get('id');
    if (!deckId) {
        navigate('/decks');
        return;
    }
    
    const deck = await apiCall(`/Decks/${deckId}`);
    if (!deck) {
        navigate('/decks');
        return;
    }
    
    const cards = deck.flashcards || [];
    
    const content = `
        <div class="page-header">
            <div>
                <a href="#/decks" class="link">? Quay l?i</a>
                <h1>?? ${deck.title}</h1>
                <p class="text-muted">${deck.description || 'Không có mô t?'}</p>
            </div>
            <button class="btn btn-success" onclick="navigate('/study/${deck.id}')">?? B?t ??u h?c</button>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3 class="card-title">Flashcards (${cards.length})</h3>
                <button class="btn btn-primary btn-sm" onclick="showCreateFlashcardModal('${deckId}')">+ Thêm Th?</button>
            </div>
            
            ${cards.length === 0 ? 
                renderEmptyState('??', 'Ch?a có flashcard nào', 'Thêm flashcard ??u tiên ?? b?t ??u!') :
                `<div class="flashcard-list">
                    ${cards.map(card => `
                        <div class="flashcard-item">
                            <div class="flashcard-term">?? ${card.term}</div>
                            <div class="flashcard-definition">${card.definition}</div>
                            ${card.example ? `<div class="flashcard-example">?? ${card.example}</div>` : ''}
                            ${card.imageUrl ? `<img src="${card.imageUrl}" class="flashcard-image" alt="${card.term}">` : ''}
                            ${card.audioUrl ? `
                                <div class="audio-player">
                                    <button onclick="playAudio('${card.audioUrl}')">?? Phát âm</button>
                                    <span class="text-muted" style="font-size:0.8rem;">Auto-generated</span>
                                </div>
                            ` : ''}
                            <div class="flashcard-actions">
                                <button class="btn btn-sm btn-secondary" onclick="showEditFlashcardModal('${card.id}', '${deckId}')">?? S?a</button>
                                <button class="btn btn-sm btn-danger" onclick="deleteFlashcard('${card.id}', '${deckId}')">??? Xóa</button>
                            </div>
                        </div>
                    `).join('')}
                </div>`
            }
        </div>
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

function showCreateFlashcardModal(deckId) {
    const modalContent = `
        <div class="form-group">
            <label>Term (Thu?t ng?) *</label>
            <input type="text" id="cardTerm" placeholder="VD: Hello">
        </div>
        <div class="form-group">
            <label>Definition (??nh ngh?a) *</label>
            <input type="text" id="cardDefinition" placeholder="VD: Xin chào">
        </div>
        <div class="form-group">
            <label>Example (Ví d?)</label>
            <input type="text" id="cardExample" placeholder="VD: Hello, how are you?">
        </div>
        <div class="form-group">
            <label>Image URL</label>
            <input type="text" id="cardImageUrl" placeholder="https://example.com/image.jpg">
        </div>
        <div class="alert alert-info">
            <small>?? Audio s? t? ??ng t?o t? Term n?u có trong t? ?i?n</small>
        </div>
    `;
    
    openModal('Thêm Flashcard M?i', modalContent, [
        { text: 'H?y', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'T?o', class: 'btn-primary', onclick: `createFlashcard('${deckId}')` }
    ]);
}

async function showEditFlashcardModal(cardId, deckId) {
    const card = await apiCall(`/Flashcards/${cardId}`);
    if (!card) return;
    
    const modalContent = `
        <div class="form-group">
            <label>Term (Thu?t ng?) *</label>
            <input type="text" id="editCardTerm" value="${card.term}">
        </div>
        <div class="form-group">
            <label>Definition (??nh ngh?a) *</label>
            <input type="text" id="editCardDefinition" value="${card.definition}">
        </div>
        <div class="form-group">
            <label>Example (Ví d?)</label>
            <input type="text" id="editCardExample" value="${card.example || ''}">
        </div>
        <div class="form-group">
            <label>Image URL</label>
            <input type="text" id="editCardImageUrl" value="${card.imageUrl || ''}">
        </div>
    `;
    
    openModal('Ch?nh S?a Flashcard', modalContent, [
        { text: 'H?y', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'L?u', class: 'btn-primary', onclick: `updateFlashcard('${cardId}', '${deckId}')` }
    ]);
}

async function createFlashcard(deckId) {
    const term = document.getElementById('cardTerm').value;
    const definition = document.getElementById('cardDefinition').value;
    const example = document.getElementById('cardExample').value;
    const imageUrl = document.getElementById('cardImageUrl').value;
    
    if (!term || !definition) {
        showAlert('Vui lòng nh?p Term và Definition', 'error');
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
        showAlert('Thêm flashcard thành công!', 'success');
        closeModal();
        navigate(`/deck/${deckId}`);
    }
}

async function updateFlashcard(cardId, deckId) {
    const term = document.getElementById('editCardTerm').value;
    const definition = document.getElementById('editCardDefinition').value;
    const example = document.getElementById('editCardExample').value;
    const imageUrl = document.getElementById('editCardImageUrl').value;
    
    if (!term || !definition) {
        showAlert('Vui lòng nh?p Term và Definition', 'error');
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
        showAlert('C?p nh?t thành công!', 'success');
        closeModal();
        navigate(`/deck/${deckId}`);
    }
}

async function deleteFlashcard(cardId, deckId) {
    if (!confirm('B?n có ch?c mu?n xóa flashcard này?')) return;
    
    const data = await apiCall(`/Flashcards/${cardId}`, {
        method: 'DELETE'
    });
    
    if (data) {
        showAlert('Xóa flashcard thành công!', 'success');
        navigate(`/deck/${deckId}`);
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
        showAlert('B? th? này ch?a có flashcard nào!', 'error');
        navigate(`/deck/${deckId}`);
        return;
    }
    
    const content = `
        <div class="page-header">
            <div>
                <a href="#/deck/${deckId}" class="link">? Quay l?i</a>
                <h1>?? H?c: ${deck.title}</h1>
            </div>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3>Ch?n ch? ?? h?c</h3>
            </div>
            <div class="btn-group">
                <button class="btn btn-primary" onclick="startFlashcardMode('${deckId}', ${JSON.stringify(deck.flashcards).replace(/"/g, '&quot;')})">
                    ?? Flashcard
                </button>
                <button class="btn btn-success" onclick="startQuizMode('${deckId}', ${JSON.stringify(deck.flashcards).replace(/"/g, '&quot;')})">
                    ? Quiz
                </button>
                <button class="btn btn-warning" onclick="startMatchMode('${deckId}', ${JSON.stringify(deck.flashcards).replace(/"/g, '&quot;')})">
                    ?? Match Game
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
            <p class="text-center text-muted mb-2">Th? ${currentCardIndex + 1} / ${studyCards.length}</p>
            
            <div class="flip-card-container">
                <div class="flip-card" id="flipCard" onclick="document.getElementById('flipCard').classList.toggle('flipped')">
                    <div class="flip-card-front">
                        <div>
                            <h2>${card.term}</h2>
                            <p class="text-muted mt-2">Click ?? l?t th?</p>
                        </div>
                    </div>
                    <div class="flip-card-back">
                        <div>
                            <h3>${card.definition}</h3>
                            ${card.example ? `<p class="mt-2" style="font-size:1rem; font-style:italic;">${card.example}</p>` : ''}
                            ${card.audioUrl ? `
                                <button class="btn btn-success btn-sm mt-2" onclick="event.stopPropagation(); playAudio('${card.audioUrl}')">
                                    ?? Phát âm
                                </button>
                            ` : ''}
                        </div>
                    </div>
                </div>
            </div>
            
            <div class="study-controls">
                <button class="btn btn-secondary" onclick="prevCard()" ${currentCardIndex === 0 ? 'disabled' : ''}>
                    ? Tr??c
                </button>
                <button class="btn btn-primary" onclick="nextCard()">
                    ${currentCardIndex === studyCards.length - 1 ? 'Hoàn thành' : 'Ti?p theo ?'}
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
    
    // Create wrong answers from other cards
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
            <p class="text-center text-muted mb-2">Câu ${currentQuizIndex + 1} / ${quizCards.length}</p>
            <p class="text-center">?i?m: ${quizScore}</p>
            
            <div class="quiz-question">
                <h2 style="margin-bottom: 20px;">? ${card.term}</h2>
                <div class="quiz-options" id="quizOptions">
                    ${allAnswers.map((answer, idx) => `
                        <div class="quiz-option" onclick="selectQuizAnswer('${answer}', '${card.definition}', ${idx})">
                            ${answer}
                        </div>
                    `).join('')}
                </div>
                <div id="quizFeedback"></div>
                <button class="btn btn-primary btn-full mt-2" id="quizNextBtn" style="display:none;" onclick="nextQuiz()">
                    Ti?p theo ?
                </button>
            </div>
        </div>
    `;
}

function selectQuizAnswer(selected, correct, idx) {
    if (selectedAnswer !== null) return; // Already answered
    
    selectedAnswer = selected;
    const options = document.querySelectorAll('.quiz-option');
    
    if (selected === correct) {
        options[idx].classList.add('correct');
        quizScore++;
        document.getElementById('quizFeedback').innerHTML = `
            <div class="alert alert-success mt-2">? Chính xác!</div>
        `;
    } else {
        options[idx].classList.add('incorrect');
        // Highlight correct answer
        options.forEach((opt, i) => {
            if (opt.textContent.trim() === correct) {
                opt.classList.add('correct');
            }
        });
        document.getElementById('quizFeedback').innerHTML = `
            <div class="alert alert-error mt-2">? Sai r?i! ?áp án ?úng là: ${correct}</div>
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
    matchCards = JSON.parse(JSON.stringify(cards));
    matchDeckId = deckId;
    matchSelected = [];
    matchedPairs = 0;
    matchAttempts = 0;
    renderMatchView();
}

function renderMatchView() {
    // Create pairs: terms and definitions
    const terms = matchCards.map((c, i) => ({ id: i, text: c.term, type: 'term' }));
    const definitions = matchCards.map((c, i) => ({ id: i, text: c.definition, type: 'definition' }));
    
    const allCards = [...terms, ...definitions].sort(() => Math.random() - 0.5);
    
    document.getElementById('studyArea').innerHTML = `
        <div class="study-container">
            <div class="card-header">
                <h3>?? Match Game</h3>
                <div>
                    <span>?ã ghép: ${matchedPairs}/${matchCards.length}</span> |
                    <span>L??t ch?i: ${matchAttempts}</span>
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
                <p class="text-muted">Ch?n m?t Term và m?t Definition t??ng ?ng</p>
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
            // Unselect
            card.classList.remove('selected');
            matchSelected = [];
            return;
        }
        
        matchAttempts++;
        
        if (first.type !== type && first.id === id) {
            // Correct match
            card.classList.add('matched');
            first.element.classList.remove('selected');
            first.element.classList.add('matched');
            matchedPairs++;
            matchSelected = [];
            
            if (matchedPairs === matchCards.length) {
                setTimeout(() => {
                    finishStudySession('Match', matchCards.length - Math.floor(matchAttempts / 2), matchCards.length);
                }, 500);
            }
        } else {
            // Wrong match
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

// Finish Study Session
async function finishStudySession(mode, score, total) {
    const data = await apiCall('/StudySessions', {
        method: 'POST',
        body: JSON.stringify({
            deckId: studyDeckId || quizDeckId || matchDeckId,
            mode: mode,
            score: score,
            totalCards: total
        })
    });
    
    if (data) {
        const percentage = ((score / total) * 100).toFixed(0);
        const message = `
            <div class="text-center">
                <h2 style="font-size: 3rem; margin-bottom: 20px;">
                    ${percentage >= 80 ? '??' : percentage >= 50 ? '??' : '??'}
                </h2>
                <h3>Hoàn thành!</h3>
                <p style="font-size: 1.5rem; margin: 20px 0;">
                    <strong>${score}/${total}</strong> (${percentage}%)
                </p>
                <p class="text-muted">Ch? ??: ${mode}</p>
            </div>
        `;
        
        openModal('K?t qu? h?c t?p', message, [
            { text: 'Xem Leaderboard', class: 'btn-warning', onclick: `showLeaderboard('${studyDeckId || quizDeckId || matchDeckId}')` },
            { text: 'H?c l?i', class: 'btn-primary', onclick: `closeModal(); navigate('/study/${studyDeckId || quizDeckId || matchDeckId}')` },
            { text: 'V? Dashboard', class: 'btn-secondary', onclick: `closeModal(); navigate('/dashboard')` }
        ]);
    }
}

async function showLeaderboard(deckId) {
    const leaderboard = await apiCall(`/StudySessions/leaderboard/${deckId}`);
    
    if (!leaderboard || leaderboard.length === 0) {
        showAlert('Ch?a có d? li?u x?p h?ng', 'info');
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
    
    openModal('?? B?ng X?p H?ng', content, [
        { text: '?óng', class: 'btn-secondary', onclick: 'closeModal()' }
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
            <h1>?? H? S? C?a Tôi</h1>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3>Thông tin cá nhân</h3>
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
            <button class="btn btn-primary" onclick="updateProfile()">?? L?u thay ??i</button>
        </div>
        
        <div class="card mt-3">
            <div class="card-header">
                <h3>?? ??i m?t kh?u</h3>
            </div>
            <div class="form-group">
                <label>M?t kh?u c?</label>
                <input type="password" id="oldPassword">
            </div>
            <div class="form-group">
                <label>M?t kh?u m?i</label>
                <input type="password" id="newPassword">
            </div>
            <div class="form-group">
                <label>Xác nh?n m?t kh?u m?i</label>
                <input type="password" id="confirmPassword">
            </div>
            <button class="btn btn-warning" onclick="changePassword()">??i m?t kh?u</button>
        </div>
        
        <div class="card mt-3">
            <div class="card-header">
                <h3>?? Th?ng kê</h3>
            </div>
            <p>?? T?ng s? b? th?: <strong>${profile.deckCount}</strong></p>
            <p>?? Tham gia t?: <strong>${formatDate(profile.createdAt)}</strong></p>
            <div class="mt-2">
                <a href="#/history" class="btn btn-primary">?? Xem l?ch s? h?c t?p ??y ??</a>
            </div>
        </div>
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

async function updateProfile() {
    const username = document.getElementById('profileUsername').value;
    const avatarUrl = document.getElementById('profileAvatar').value;
    
    if (!username) {
        showAlert('Username không ???c ?? tr?ng', 'error');
        return;
    }
    
    const data = await apiCall('/Users/profile', {
        method: 'PUT',
        body: JSON.stringify({ username, avatarUrl })
    });
    
    if (data) {
        // Update stored user info
        const userInfo = auth.getUserInfo();
        userInfo.username = username;
        auth.setUserInfo(userInfo);
        
        showAlert('C?p nh?t h? s? thành công!', 'success');
        renderProfile();
    }
}

async function changePassword() {
    const oldPassword = document.getElementById('oldPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    
    if (!oldPassword || !newPassword || !confirmPassword) {
        showAlert('Vui lòng ?i?n ??y ?? thông tin', 'error');
        return;
    }
    
    if (newPassword !== confirmPassword) {
        showAlert('M?t kh?u m?i không kh?p!', 'error');
        return;
    }
    
    if (newPassword.length < 6) {
        showAlert('M?t kh?u m?i ph?i có ít nh?t 6 ký t?', 'error');
        return;
    }
    
    const data = await apiCall('/Users/change-password', {
        method: 'PUT',
        body: JSON.stringify({ oldPassword, newPassword })
    });
    
    if (data) {
        showAlert('??i m?t kh?u thành công!', 'success');
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
            <h1>?? L?ch S? H?c T?p</h1>
        </div>
        
        ${!history || history.length === 0 ? 
            renderEmptyState('??', 'Ch?a có l?ch s? h?c t?p', 'Hãy b?t ??u h?c m?t b? th?!') :
            `
            <div class="card">
                <div class="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>Th?i gian</th>
                                <th>B? th?</th>
                                <th>Ch? ??</th>
                                <th>?i?m s?</th>
                                <th>T? l?</th>
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
                    <h3>?? Th?ng kê t?ng quan</h3>
                </div>
                ${renderStudyStats(history)}
            </div>
            `
        }
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

function renderStudyStats(sessions) {
    if (!sessions || sessions.length === 0) return '<p>Ch?a có d? li?u</p>';
    
    const stats = calculateStudyStats(sessions);
    
    return `
        <div class="stats-grid">
            <div class="stat-card">
                <h3>${stats.totalSessions}</h3>
                <p>T?ng l??t h?c</p>
            </div>
            <div class="stat-card success">
                <h3>${stats.totalCards}</h3>
                <p>T?ng s? th? ?ã h?c</p>
            </div>
            <div class="stat-card warning">
                <h3>${stats.averageScore}%</h3>
                <p>?i?m trung bình</p>
            </div>
            <div class="stat-card danger">
                <h3>${stats.streak}</h3>
                <p>Chu?i ngày h?c</p>
            </div>
        </div>
        
        <div class="mt-3">
            <p><strong>?? Th?ng kê theo ch? ??:</strong></p>
            <ul>
                ${['Flashcard', 'Quiz', 'Match'].map(mode => {
                    const modeSessions = sessions.filter(s => s.mode === mode);
                    const count = modeSessions.length;
                    const avgScore = count > 0 
                        ? (modeSessions.reduce((sum, s) => sum + (s.score / s.totalCards * 100), 0) / count).toFixed(1)
                        : 0;
                    return `<li>${mode}: ${count} l??t (${avgScore}% trung bình)</li>`;
                }).join('')}
            </ul>
        </div>
    `;
}

// Helper function for grade calculation
function getGrade(percentage) {
    if (percentage >= 90) return { letter: 'A', color: '#28a745' };
    if (percentage >= 80) return { letter: 'B', color: '#17a2b8' };
    if (percentage >= 70) return { letter: 'C', color: '#ffc107' };
    if (percentage >= 60) return { letter: 'D', color: '#fd7e14' };
    return { letter: 'F', color: '#dc3545' };
}

// Helper for study stats calculation
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
    const bestScore = Math.max(...sessions.map(s => 
        s.totalCards > 0 ? (s.score / s.totalCards * 100) : 0
    ));
    
    // Calculate streak (consecutive days)
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
        bestScore: bestScore.toFixed(1),
        streak
    };
}

// ============================
// ADMIN PAGE
// ============================
async function renderAdmin() {
    const userInfo = auth.getUserInfo();
    
    if (userInfo?.role !== 'Admin') {
        showAlert('B?n không có quy?n truy c?p trang này', 'error');
        navigate('/dashboard');
        return;
    }
    
    const [users, allHistory] = await Promise.all([
        apiCall('/Users'),
        apiCall('/StudySessions/admin/all-history')
    ]);
    
    const content = `
        <div class="page-header">
            <h1>?? Admin Panel</h1>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3>?? Qu?n lý ng??i dùng</h3>
            </div>
            ${!users || users.length === 0 ? 
                renderEmptyState('??', 'Không có ng??i dùng', '') :
                `<div class="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>Username</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Ngày t?o</th>
                                <th>Thao tác</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${users.map(u => `
                                <tr>
                                    <td>${u.username}</td>
                                    <td>${u.email}</td>
                                    <td><span class="badge badge-primary">${u.role}</span></td>
                                    <td>${formatDate(u.createdAt)}</td>
                                    <td>
                                        <button class="btn btn-sm btn-danger" onclick="deleteUser('${u.id}')">Xóa</button>
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
                <h3>?? L?ch s? h?c t?p (Toàn h? th?ng)</h3>
            </div>
            ${!allHistory || allHistory.length === 0 ? 
                renderEmptyState('??', 'Ch?a có l?ch s?', '') :
                `<div class="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>Ng??i dùng</th>
                                <th>B? th?</th>
                                <th>Ch? ??</th>
                                <th>?i?m</th>
                                <th>Th?i gian</th>
                                <th>Thao tác</th>
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
                                        <button class="btn btn-sm btn-danger" onclick="deleteSession('${s.id}')">Xóa</button>
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

async function deleteUser(userId) {
    if (!confirm('B?n có ch?c mu?n xóa ng??i dùng này?')) return;
    
    const data = await apiCall(`/Users/${userId}`, {
        method: 'DELETE'
    });
    
    if (data) {
        showAlert('Xóa ng??i dùng thành công!', 'success');
        renderAdmin();
    }
}

async function deleteSession(sessionId) {
    if (!confirm('B?n có ch?c mu?n xóa session này?')) return;
    
    const data = await apiCall(`/StudySessions/admin/${sessionId}`, {
        method: 'DELETE'
    });
    
    if (data) {
        showAlert('Xóa session thành công!', 'success');
        renderAdmin();
    }
}

// ============================
// NOT FOUND PAGE
// ============================
function renderNotFound() {
    const content = renderEmptyState('?', '404 - Không tìm th?y trang', 'Trang b?n tìm ki?m không t?n t?i');
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
                    <h2>?? Flashcard</h2>
                    <div class="user-info">
                        <div>?? ${userInfo?.username || 'User'}</div>
                        <div style="font-size:0.75rem; opacity:0.7">${userInfo?.role || 'User'}</div>
                    </div>
                </div>
                <ul class="sidebar-menu">
                    <li><a href="#/dashboard" class="${currentHash.includes('dashboard') ? 'active' : ''}">?? Dashboard</a></li>
                    <li><a href="#/decks" class="${currentHash.includes('decks') || currentHash.includes('deck/') ? 'active' : ''}">?? My Decks</a></li>
                    <li><a href="#/history" class="${currentHash.includes('history') ? 'active' : ''}">?? Study History</a></li>
                    <li><a href="#/profile" class="${currentHash.includes('profile') ? 'active' : ''}">?? Profile</a></li>
                    ${isAdmin ? `<li><a href="#/admin" class="${currentHash.includes('admin') ? 'active' : ''}">?? Admin Panel</a></li>` : ''}
                    <li style="margin-top: 20px;"><a href="#/logout" style="color:#ff6b6b;">?? ??ng xu?t</a></li>
                </ul>
                <div style="position: absolute; bottom: 20px; left: 20px; right: 20px; font-size: 0.75rem; opacity: 0.5; text-align: center;">
                    <a href="/guide.html" target="_blank" style="color: white;">?? Guide</a> | 
                    <a href="/test-api.html" target="_blank" style="color: white;">?? API Test</a>
                </div>
            </aside>
            <main class="main-content">
                ${content}
            </main>
        </div>
    `;
}
