/**
 * Dark Mode Toggle Component
 * Handles dark mode switching and persistence
 */

// Check if dark mode is enabled in localStorage or system preference
function isDarkModeEnabled() {
  // Check localStorage first
  const storedPreference = localStorage.getItem('darkMode');
  if (storedPreference !== null) {
    return storedPreference === 'true';
  }
  
  // If no stored preference, check system preference
  return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
}

// Apply dark mode to the document
function applyDarkMode(isDark) {
  if (isDark) {
    document.documentElement.classList.add('dark');
  } else {
    document.documentElement.classList.remove('dark');
  }
  
  // Update localStorage
  localStorage.setItem('darkMode', isDark);
  
  // Update meta theme-color for mobile browsers
  const metaThemeColor = document.querySelector('meta[name="theme-color"]');
  if (metaThemeColor) {
    metaThemeColor.setAttribute('content', isDark ? '#1f2937' : '#ffffff');
  }
}

// Initialize dark mode
function initDarkMode() {
  // Apply initial dark mode state
  const isDark = isDarkModeEnabled();
  applyDarkMode(isDark);
  
  // Listen for system preference changes
  if (window.matchMedia) {
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
      // Only apply if user hasn't set a preference
      if (localStorage.getItem('darkMode') === null) {
        applyDarkMode(e.matches);
      }
    });
  }
  
  // Add toggle button to the DOM
  const header = document.querySelector('.clickup-header-content');
  if (header) {
    const toggleButton = document.createElement('button');
    toggleButton.id = 'darkModeToggle';
    toggleButton.className = 'p-2 rounded-md text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500';
    toggleButton.setAttribute('aria-label', 'Toggle dark mode');
    
    // Set initial icon
    updateToggleIcon(toggleButton, isDark);
    
    // Add click event
    toggleButton.addEventListener('click', () => {
      const newDarkMode = !document.documentElement.classList.contains('dark');
      applyDarkMode(newDarkMode);
      updateToggleIcon(toggleButton, newDarkMode);
    });
    
    // Add to header
    header.appendChild(toggleButton);
  }
}

// Update the toggle button icon
function updateToggleIcon(button, isDark) {
  if (isDark) {
    button.innerHTML = `
      <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
      </svg>
    `;
  } else {
    button.innerHTML = `
      <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
      </svg>
    `;
  }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', initDarkMode);

// Export for module usage
export { initDarkMode, isDarkModeEnabled, applyDarkMode }; 