// ============================
// FOLDERS MODULE
// ============================

// ============================
// FOLDERS LIST PAGE
// ============================
async function renderFolders() {
    const folders = await apiCall('/Folders');
    
    const content = `
        <div class="page-header">
            <h1>My Folders</h1>
            <button class="btn btn-primary" onclick="showCreateFolderModal()">+ Create New Folder</button>
        </div>
        
        <div class="card mb-3">
            <div class="alert alert-info" style="margin: 0;">
                <strong>What are folders?</strong> Organize your decks into folders for better management. 
                Deleting a folder will NOT delete the decks inside - they'll just become unassigned.
            </div>
        </div>
        
        ${!folders || folders.length === 0 ? 
            renderEmptyState('No folders yet', 'Create your first folder to organize your decks!') :
            `<div class="folder-grid">
                ${folders.map(folder => `
                    <div class="folder-item" onclick="navigate('/folder/${folder.id}')">
                        <div class="folder-icon"></div>
                        <h3>${folder.name}</h3>
                        <p>${folder.description || 'No description'}</p>
                        <div class="folder-meta">
                            <span>${folder.deckCount || 0} decks</span>
                            <span>${formatDate(folder.createdAt).split(',')[0]}</span>
                        </div>
                        <div class="folder-actions" onclick="event.stopPropagation()">
                            <button class="btn-view" onclick="navigate('/folder/${folder.id}')">View</button>
                            <button class="btn-delete" onclick="deleteFolder('${folder.id}')">Delete</button>
                        </div>
                    </div>
                `).join('')}
            </div>`
        }
        
        <div class="card mt-3">
            <div class="card-header">
                <h3>Unassigned Decks</h3>
                <button class="btn btn-sm btn-primary" onclick="navigate('/decks')">View All Decks</button>
            </div>
            <div id="unassignedDecks">Loading...</div>
        </div>
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
    
    // Load unassigned decks
    loadUnassignedDecks();
}

// ============================
// LOAD UNASSIGNED DECKS
// ============================
async function loadUnassignedDecks() {
    const decks = await apiCall('/Folders/unassigned-decks');
    const container = document.getElementById('unassignedDecks');
    
    if (!container) return;
    
    if (!decks || decks.length === 0) {
        container.innerHTML = '<p class="text-muted text-center" style="padding: 20px;">All decks are organized in folders! ?</p>';
        return;
    }
    
    container.innerHTML = `
        <div class="deck-grid">
            ${decks.map(deck => `
                <div class="deck-item" onclick="navigate('/deck/${deck.id}')">
                    <h3>${deck.title}</h3>
                    <p>${deck.description || 'No description'}</p>
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
                            <button class="btn btn-sm btn-primary" onclick="showMoveDeckModal('${deck.id}', null)">Move</button>
                        </div>
                    </div>
                </div>
            `).join('')}
        </div>
    `;
}

// ============================
// FOLDER DETAIL PAGE
// ============================
async function renderFolderDetail(params) {
    const folderId = params.get('id');
    if (!folderId) {
        navigate('/folders');
        return;
    }
    
    const folder = await apiCall(`/Folders/${folderId}`);
    if (!folder) {
        navigate('/folders');
        return;
    }
    
    const decks = folder.decks || [];
    
    const content = `
        <div class="page-header">
            <div>
                <a href="#/folders" class="link">Back to Folders</a>
            </div>
        </div>
        
        <div class="folder-detail-header">
            <h1>${folder.name}</h1>
            <p>${folder.description || 'No description'}</p>
            <div class="folder-badge">
                ${decks.length} deck${decks.length !== 1 ? 's' : ''}
            </div>
        </div>
        
        <div class="card">
            <div class="card-header">
                <h3 class="card-title">Decks in this Folder</h3>
                <button class="btn btn-primary btn-sm" onclick="navigate('/decks')">+ Add Deck</button>
            </div>
            
            ${decks.length === 0 ? 
                renderEmptyState('No decks in this folder', 'Move decks here to organize them!') :
                `<div class="deck-grid">
                    ${decks.map(deck => `
                        <div class="deck-item" onclick="navigate('/deck/${deck.id}')">
                            <h3>${deck.title}</h3>
                            <p>${deck.description || 'No description'}</p>
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
                                    <button class="btn btn-sm btn-primary" onclick="navigate('/study/${deck.id}')">Study</button>
                                    <button class="btn btn-sm btn-secondary" onclick="showMoveDeckModal('${deck.id}', '${folderId}')">Move</button>
                                </div>
                            </div>
                        </div>
                    `).join('')}
                </div>`
            }
        </div>
    `;
    
    document.getElementById('app').innerHTML = renderLayout(content);
}

// ============================
// CREATE FOLDER MODAL
// ============================
function showCreateFolderModal() {
    const modalContent = `
        <div class="form-group">
            <label>Folder Name *</label>
            <input type="text" id="folderName" placeholder="e.g. IELTS Preparation" maxlength="200">
        </div>
        <div class="form-group">
            <label>Description</label>
            <textarea id="folderDescription" placeholder="Brief description of the folder..." maxlength="1000"></textarea>
        </div>
    `;
    
    openModal('Create New Folder', modalContent, [
        { text: 'Cancel', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: '? Create', class: 'btn-primary', onclick: 'createFolder()' }
    ]);
}

// ============================
// CREATE FOLDER
// ============================
async function createFolder() {
    const name = document.getElementById('folderName').value.trim();
    const description = document.getElementById('folderDescription').value.trim();
    
    if (!name) {
        showAlert('Please enter folder name', 'error');
        return;
    }
    
    if (name.length > 200) {
        showAlert('Folder name cannot exceed 200 characters', 'error');
        return;
    }
    
    const data = await apiCall('/Folders', {
        method: 'POST',
        body: JSON.stringify({ name, description })
    });
    
    if (data) {
        showAlert('? Folder created successfully!', 'success');
        closeModal();
        renderFolders();
    }
}

// ============================
// DELETE FOLDER
// ============================
async function deleteFolder(folderId) {
    if (!confirm('Are you sure you want to delete this folder?\n\nDon\'t worry: Decks inside will NOT be deleted, they\'ll just become unassigned.')) {
        return;
    }
    
    const data = await apiCall(`/Folders/${folderId}`, {
        method: 'DELETE'
    });
    
    if (data) {
        showAlert('? Folder deleted successfully!', 'success');
        renderFolders();
    }
}

// ============================
// SHOW MOVE DECK MODAL
// ============================
async function showMoveDeckModal(deckId, currentFolderId) {
    // Load folders for dropdown
    const folders = await apiCall('/Folders');
    
    if (!folders || folders.length === 0) {
        showAlert('Please create a folder first!', 'error');
        return;
    }
    
    const modalContent = `
        <div class="folder-select-container">
            <label>Move deck to folder:</label>
            <select id="targetFolderId" class="form-control">
                <option value="">-- Remove from folder (Unassign) --</option>
                ${folders.map(f => `
                    <option value="${f.id}" ${f.id === currentFolderId ? 'selected' : ''}>
                        ${f.name} ${f.id === currentFolderId ? '(Current)' : ''}
                    </option>
                `).join('')}
            </select>
        </div>
        
        <div class="alert alert-info" style="margin-top: 15px;">
            <small>
                <strong>Tip:</strong> Select a folder to organize your deck, or choose "Remove from folder" to unassign it.
            </small>
        </div>
    `;
    
    openModal('Move Deck', modalContent, [
        { text: 'Cancel', class: 'btn-secondary', onclick: 'closeModal()' },
        { text: 'Move', class: 'btn-primary', onclick: `moveDeck('${deckId}')` }
    ]);
}

// ============================
// MOVE DECK TO FOLDER
// ============================
async function moveDeck(deckId) {
    const targetFolderId = document.getElementById('targetFolderId').value;
    
    // Get current deck info
    const deck = await apiCall(`/Decks/${deckId}`);
    if (!deck) {
        closeModal();
        return;
    }
    
    // Update deck with new folderId
    const data = await apiCall(`/Decks/${deckId}`, {
        method: 'PUT',
        body: JSON.stringify({
            title: deck.title,
            description: deck.description,
            isPublic: deck.isPublic,
            folderId: targetFolderId || null
        })
    });
    
    if (data) {
        const message = targetFolderId 
            ? '? Deck moved to folder successfully!' 
            : '? Deck removed from folder!';
        showAlert(message, 'success');
        closeModal();
        
        // Refresh current view
        const currentHash = window.location.hash;
        if (currentHash.includes('/folders')) {
            renderFolders();
        } else if (currentHash.includes('/folder/')) {
            router();
        } else if (currentHash.includes('/decks')) {
            renderDecks();
        }
    }
}

// ============================
// POPULATE FOLDER DROPDOWN
// (Helper function for other modals)
// ============================
async function populateFolderDropdown(selectId, currentFolderId = null) {
    const folders = await apiCall('/Folders');
    const selectElement = document.getElementById(selectId);
    
    if (!selectElement) return;
    
    selectElement.innerHTML = `
        <option value="">-- No Folder (Unassigned) --</option>
        ${folders ? folders.map(f => `
            <option value="${f.id}" ${f.id === currentFolderId ? 'selected' : ''}>
                ${f.name}
            </option>
        `).join('') : ''}
    `;
}
