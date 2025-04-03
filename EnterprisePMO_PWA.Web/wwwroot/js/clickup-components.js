/**
 * ClickUp UI Components
 * Handles specific UI components and interactions
 */

document.addEventListener('DOMContentLoaded', function() {
    // Initialize Kanban board
    initKanban();
    
    // Initialize modals
    initModals();
    
    // Initialize tabs
    initTabs();
    
    // Initialize file uploads
    initFileUploads();
});

/**
 * Initialize Kanban board functionality
 */
function initKanban() {
    const kanbanBoard = document.querySelector('.kanban-board');
    
    if (kanbanBoard) {
        const columns = kanbanBoard.querySelectorAll('.kanban-column');
        const tasks = kanbanBoard.querySelectorAll('.kanban-task');
        
        // Initialize drag and drop for tasks
        tasks.forEach(task => {
            task.setAttribute('draggable', 'true');
            
            task.addEventListener('dragstart', handleDragStart);
            task.addEventListener('dragend', handleDragEnd);
        });
        
        // Initialize drop zones for columns
        columns.forEach(column => {
            column.addEventListener('dragover', handleDragOver);
            column.addEventListener('dragleave', handleDragLeave);
            column.addEventListener('drop', handleDrop);
        });
    }
}

/**
 * Handle drag start event
 */
function handleDragStart(e) {
    e.target.classList.add('dragging');
    e.dataTransfer.setData('text/plain', e.target.id);
}

/**
 * Handle drag end event
 */
function handleDragEnd(e) {
    e.target.classList.remove('dragging');
}

/**
 * Handle drag over event
 */
function handleDragOver(e) {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
    e.currentTarget.classList.add('drag-over');
}

/**
 * Handle drag leave event
 */
function handleDragLeave(e) {
    e.currentTarget.classList.remove('drag-over');
}

/**
 * Handle drop event
 */
async function handleDrop(e) {
    e.preventDefault();
    e.currentTarget.classList.remove('drag-over');
    
    const taskId = e.dataTransfer.getData('text/plain');
    const task = document.getElementById(taskId);
    const newStatus = e.currentTarget.getAttribute('data-status');
    
    if (task && newStatus) {
        try {
            // Update task status via API
            await updateTaskStatus(taskId, newStatus);
            
            // Move task to new column
            e.currentTarget.querySelector('.kanban-tasks').appendChild(task);
            
            // Show success notification
            showNotification('Task status updated successfully', 'success');
        } catch (error) {
            console.error('Error updating task status:', error);
            showNotification('Failed to update task status', 'error');
        }
    }
}

/**
 * Update task status via API
 */
async function updateTaskStatus(taskId, newStatus) {
    const response = await fetch(`/api/tasks/${taskId}/status`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': document.querySelector('meta[name="csrf-token"]').getAttribute('content')
        },
        body: JSON.stringify({ status: newStatus })
    });
    
    if (!response.ok) {
        throw new Error('Failed to update task status');
    }
    
    return response.json();
}

/**
 * Initialize modals
 */
function initModals() {
    const modalTriggers = document.querySelectorAll('[data-modal-target]');
    
    modalTriggers.forEach(trigger => {
        const targetId = trigger.getAttribute('data-modal-target');
        const modal = document.getElementById(targetId);
        
        if (modal) {
            // Open modal
            trigger.addEventListener('click', () => {
                modal.classList.remove('hidden');
                document.body.classList.add('overflow-hidden');
            });
            
            // Close modal
            const closeButtons = modal.querySelectorAll('[data-modal-close]');
            closeButtons.forEach(button => {
                button.addEventListener('click', () => {
                    modal.classList.add('hidden');
                    document.body.classList.remove('overflow-hidden');
                });
            });
            
            // Close on outside click
            modal.addEventListener('click', (e) => {
                if (e.target === modal) {
                    modal.classList.add('hidden');
                    document.body.classList.remove('overflow-hidden');
                }
            });
            
            // Close on escape key
            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape' && !modal.classList.contains('hidden')) {
                    modal.classList.add('hidden');
                    document.body.classList.remove('overflow-hidden');
                }
            });
        }
    });
}

/**
 * Initialize tabs
 */
function initTabs() {
    const tabContainers = document.querySelectorAll('.clickup-tabs');
    
    tabContainers.forEach(container => {
        const tabs = container.querySelectorAll('[data-tab]');
        const contents = container.querySelectorAll('[data-tab-content]');
        
        tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                const targetId = tab.getAttribute('data-tab');
                
                // Update active tab
                tabs.forEach(t => t.classList.remove('clickup-tab-active'));
                tab.classList.add('clickup-tab-active');
                
                // Show target content
                contents.forEach(content => {
                    if (content.getAttribute('data-tab-content') === targetId) {
                        content.classList.remove('hidden');
                    } else {
                        content.classList.add('hidden');
                    }
                });
            });
        });
    });
}

/**
 * Initialize file uploads
 */
function initFileUploads() {
    const fileInputs = document.querySelectorAll('.clickup-file-upload');
    
    fileInputs.forEach(input => {
        const dropZone = input.closest('.clickup-drop-zone');
        const preview = input.closest('.clickup-file-preview');
        
        if (dropZone) {
            // Handle drag and drop
            dropZone.addEventListener('dragover', (e) => {
                e.preventDefault();
                dropZone.classList.add('clickup-drop-zone-active');
            });
            
            dropZone.addEventListener('dragleave', () => {
                dropZone.classList.remove('clickup-drop-zone-active');
            });
            
            dropZone.addEventListener('drop', (e) => {
                e.preventDefault();
                dropZone.classList.remove('clickup-drop-zone-active');
                
                const files = e.dataTransfer.files;
                handleFiles(files, input, preview);
            });
            
            // Handle file input change
            input.addEventListener('change', (e) => {
                handleFiles(e.target.files, input, preview);
            });
        }
    });
}

/**
 * Handle uploaded files
 */
function handleFiles(files, input, preview) {
    if (files.length > 0) {
        // Update file input
        input.files = files;
        
        // Show preview if available
        if (preview) {
            preview.innerHTML = Array.from(files).map(file => `
                <div class="clickup-file-item">
                    <div class="clickup-file-icon">
                        <i class="bi bi-file-earmark"></i>
                    </div>
                    <div class="clickup-file-info">
                        <div class="clickup-file-name">${file.name}</div>
                        <div class="clickup-file-size">${formatFileSize(file.size)}</div>
                    </div>
                    <button type="button" class="clickup-file-remove" onclick="removeFile(this)">
                        <i class="bi bi-x"></i>
                    </button>
                </div>
            `).join('');
            
            preview.classList.remove('hidden');
        }
    }
}

/**
 * Remove file from upload
 */
function removeFile(button) {
    const fileItem = button.closest('.clickup-file-item');
    const preview = fileItem.closest('.clickup-file-preview');
    const input = preview.closest('.clickup-file-upload-container').querySelector('.clickup-file-upload');
    
    // Clear file input
    input.value = '';
    
    // Remove preview
    fileItem.remove();
    
    // Hide preview container if empty
    if (preview.children.length === 0) {
        preview.classList.add('hidden');
    }
}

/**
 * Format file size
 */
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

/**
 * Show notification
 */
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `fixed bottom-4 right-4 p-4 rounded-md shadow-lg z-50 animate-fadeIn ${
        type === 'success' ? 'bg-green-500 text-white' :
        type === 'error' ? 'bg-red-500 text-white' :
        type === 'warning' ? 'bg-yellow-500 text-white' :
        'bg-blue-500 text-white'
    }`;
    
    // Add message
    notification.textContent = message;
    
    // Add to DOM
    document.body.appendChild(notification);
    
    // Remove after 3 seconds
    setTimeout(() => {
        notification.classList.add('opacity-0');
        setTimeout(() => {
            notification.remove();
        }, 300);
    }, 3000);
} 