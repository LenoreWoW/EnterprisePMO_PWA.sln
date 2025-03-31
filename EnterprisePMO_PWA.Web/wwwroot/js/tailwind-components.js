/**
 * Tailwind Components - UI helpers for Tailwind CSS
 * This file helps replace Bootstrap JavaScript functionality for Tailwind UI components
 */

/**
 * Modal component
 */
const Modal = {
    /**
     * Show a modal dialog
     * @param {string} modalId - The ID of the modal element
     */
    show: function(modalId) {
      const modal = document.getElementById(modalId);
      if (!modal) return;
      
      // Set display properties
      modal.classList.remove('hidden');
      document.body.classList.add('overflow-hidden');
      
      // Add fade-in animation
      setTimeout(() => {
        modal.querySelector('.modal-content')?.classList.add('opacity-100', 'translate-y-0');
        modal.querySelector('.modal-backdrop')?.classList.add('opacity-50');
      }, 10);
      
      // Add close handlers
      const closeButtons = modal.querySelectorAll('[data-dismiss="modal"]');
      closeButtons.forEach(button => {
        button.addEventListener('click', () => this.hide(modalId));
      });
      
      // Close on backdrop click
      modal.addEventListener('click', (e) => {
        if (e.target === modal) this.hide(modalId);
      });
      
      // Close on escape key
      document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') this.hide(modalId);
      });
      
      // Trigger event
      modal.dispatchEvent(new CustomEvent('modal:shown'));
    },
    
    /**
     * Hide a modal dialog
     * @param {string} modalId - The ID of the modal element
     */
    hide: function(modalId) {
      const modal = document.getElementById(modalId);
      if (!modal) return;
      
      // Remove animation classes
      modal.querySelector('.modal-content')?.classList.remove('opacity-100', 'translate-y-0');
      modal.querySelector('.modal-backdrop')?.classList.remove('opacity-50');
      
      // Hide after animation
      setTimeout(() => {
        modal.classList.add('hidden');
        document.body.classList.remove('overflow-hidden');
        
        // Trigger event
        modal.dispatchEvent(new CustomEvent('modal:hidden'));
      }, 300);
    }
  };
  
  /**
   * Toast notification component
   */
  const Toast = {
    /**
     * Show a toast notification
     * @param {Object} options - Toast options
     * @param {string} options.message - Message to show
     * @param {string} options.type - Type of toast (success, error, warning, info)
     * @param {number} options.duration - Duration in ms (default: 3000)
     */
    show: function({ message, type = 'info', duration = 3000 }) {
      // Create container if it doesn't exist
      let container = document.getElementById('toast-container');
      if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'fixed bottom-0 right-0 p-4 z-50 flex flex-col items-end space-y-2';
        document.body.appendChild(container);
      }
      
      // Create toast element
      const toastId = `toast-${Date.now()}`;
      const toast = document.createElement('div');
      toast.id = toastId;
      toast.className = `transform transition-all duration-300 translate-x-full opacity-0 max-w-sm 
                        flex items-center p-4 rounded-lg shadow-lg ${this._getTypeClasses(type)}`;
      
      toast.innerHTML = `
        <div class="flex-shrink-0 mr-3">
          ${this._getTypeIcon(type)}
        </div>
        <div class="flex-1">
          <p class="text-sm font-medium">${message}</p>
        </div>
        <div class="ml-4 flex-shrink-0 flex">
          <button type="button" class="inline-flex text-gray-400 focus:outline-none focus:text-gray-500 transition ease-in-out duration-150">
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"/>
            </svg>
          </button>
        </div>
      `;
      
      // Add to container
      container.appendChild(toast);
      
      // Animate in
      setTimeout(() => {
        toast.classList.remove('translate-x-full', 'opacity-0');
      }, 10);
      
      // Set up auto dismiss
      const dismissTimer = setTimeout(() => {
        this._dismiss(toast);
      }, duration);
      
      // Set up manual dismiss
      toast.querySelector('button').addEventListener('click', () => {
        clearTimeout(dismissTimer);
        this._dismiss(toast);
      });
      
      return toastId;
    },
    
    /**
     * Dismiss a toast notification
     * @param {HTMLElement} toast - The toast element to dismiss
     * @private
     */
    _dismiss: function(toast) {
      toast.classList.add('translate-x-full', 'opacity-0');
      
      setTimeout(() => {
        toast.remove();
        
        // Remove container if empty
        const container = document.getElementById('toast-container');
        if (container && container.children.length === 0) {
          container.remove();
        }
      }, 300);
    },
    
    /**
     * Get CSS classes for toast type
     * @param {string} type - Toast type
     * @returns {string} CSS classes
     * @private
     */
    _getTypeClasses: function(type) {
      switch (type) {
        case 'success':
          return 'bg-green-50 text-green-800 border-l-4 border-green-400';
        case 'error':
          return 'bg-red-50 text-red-800 border-l-4 border-red-400';
        case 'warning':
          return 'bg-yellow-50 text-yellow-800 border-l-4 border-yellow-400';
        case 'info':
        default:
          return 'bg-blue-50 text-blue-800 border-l-4 border-blue-400';
      }
    },
    
    /**
     * Get icon SVG for toast type
     * @param {string} type - Toast type
     * @returns {string} SVG markup
     * @private
     */
    _getTypeIcon: function(type) {
      switch (type) {
        case 'success':
          return `<svg class="h-6 w-6 text-green-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>`;
        case 'error':
          return `<svg class="h-6 w-6 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>`;
        case 'warning':
          return `<svg class="h-6 w-6 text-yellow-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                  </svg>`;
        case 'info':
        default:
          return `<svg class="h-6 w-6 text-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>`;
      }
    }
  };
  
  /**
   * Dropdown component
   */
  const Dropdown = {
    /**
     * Initialize all dropdowns on the page
     */
    init: function() {
      const toggles = document.querySelectorAll('[data-toggle="dropdown"]');
      
      toggles.forEach(toggle => {
        const target = document.getElementById(toggle.getAttribute('data-target'));
        if (!target) return;
        
        toggle.addEventListener('click', (e) => {
          e.preventDefault();
          e.stopPropagation();
          
          this.toggle(target);
        });
      });
      
      // Close dropdowns when clicking outside
      document.addEventListener('click', () => {
        document.querySelectorAll('.dropdown-menu.block').forEach(menu => {
          this.hide(menu);
        });
      });
    },
    
    /**
     * Toggle dropdown visibility
     * @param {HTMLElement} dropdown - The dropdown element
     */
    toggle: function(dropdown) {
      if (dropdown.classList.contains('hidden')) {
        this.show(dropdown);
      } else {
        this.hide(dropdown);
      }
    },
    
    /**
     * Show dropdown
     * @param {HTMLElement} dropdown - The dropdown element
     */
    show: function(dropdown) {
      // Hide any open dropdowns
      document.querySelectorAll('.dropdown-menu:not(.hidden)').forEach(menu => {
        if (menu !== dropdown) this.hide(menu);
      });
      
      // Show this dropdown
      dropdown.classList.remove('hidden');
      dropdown.classList.add('block');
      
      // Add animation classes
      dropdown.classList.add('opacity-100', 'translate-y-0');
      dropdown.classList.remove('opacity-0', 'translate-y-1');
      
      // Trigger event
      dropdown.dispatchEvent(new CustomEvent('dropdown:shown'));
    },
    
    /**
     * Hide dropdown
     * @param {HTMLElement} dropdown - The dropdown element
     */
    hide: function(dropdown) {
      dropdown.classList.add('opacity-0', 'translate-y-1');
      dropdown.classList.remove('opacity-100', 'translate-y-0');
      
      setTimeout(() => {
        dropdown.classList.remove('block');
        dropdown.classList.add('hidden');
        
        // Trigger event
        dropdown.dispatchEvent(new CustomEvent('dropdown:hidden'));
      }, 100);
    }
  };
  
  /**
   * Tooltip component
   */
  const Tooltip = {
    /**
     * Initialize all tooltips on the page
     */
    init: function() {
      const tooltipTriggers = document.querySelectorAll('[data-tooltip]');
      
      tooltipTriggers.forEach(trigger => {
        const content = trigger.getAttribute('data-tooltip');
        const position = trigger.getAttribute('data-tooltip-position') || 'top';
        
        // Create tooltip element
        const tooltip = document.createElement('div');
        tooltip.className = `hidden absolute z-10 px-3 py-2 text-sm font-medium text-white bg-gray-900 rounded-md shadow-sm 
                            opacity-0 transition-opacity duration-300 tooltip`;
        tooltip.textContent = content;
        
        // Add arrow
        const arrow = document.createElement('div');
        arrow.className = 'absolute w-3 h-3 bg-gray-900 transform rotate-45';
        tooltip.appendChild(arrow);
        
        // Position arrow based on tooltip position
        switch (position) {
          case 'bottom':
            arrow.classList.add('top-0', '-translate-y-1/2', 'left-1/2', '-translate-x-1/2');
            break;
          case 'left':
            arrow.classList.add('right-0', 'translate-x-1/2', 'top-1/2', '-translate-y-1/2');
            break;
          case 'right':
            arrow.classList.add('left-0', '-translate-x-1/2', 'top-1/2', '-translate-y-1/2');
            break;
          case 'top':
          default:
            arrow.classList.add('bottom-0', 'translate-y-1/2', 'left-1/2', '-translate-x-1/2');
            break;
        }
        
        // Append tooltip to body
        document.body.appendChild(tooltip);
        
        // Show tooltip on hover
        trigger.addEventListener('mouseenter', () => {
          const triggerRect = trigger.getBoundingClientRect();
          
          // Position tooltip based on trigger position
          switch (position) {
            case 'bottom':
              tooltip.style.left = `${triggerRect.left + triggerRect.width / 2 - tooltip.offsetWidth / 2}px`;
              tooltip.style.top = `${triggerRect.bottom + 8}px`;
              break;
            case 'left':
              tooltip.style.right = `${window.innerWidth - triggerRect.left + 8}px`;
              tooltip.style.top = `${triggerRect.top + triggerRect.height / 2 - tooltip.offsetHeight / 2}px`;
              break;
            case 'right':
              tooltip.style.left = `${triggerRect.right + 8}px`;
              tooltip.style.top = `${triggerRect.top + triggerRect.height / 2 - tooltip.offsetHeight / 2}px`;
              break;
            case 'top':
            default:
              tooltip.style.left = `${triggerRect.left + triggerRect.width / 2 - tooltip.offsetWidth / 2}px`;
              tooltip.style.top = `${triggerRect.top - tooltip.offsetHeight - 8}px`;
              break;
          }
          
          // Show tooltip
          tooltip.classList.remove('hidden');
          setTimeout(() => {
            tooltip.classList.add('opacity-100');
          }, 10);
        });
        
        // Hide tooltip on mouse leave
        trigger.addEventListener('mouseleave', () => {
          tooltip.classList.remove('opacity-100');
          setTimeout(() => {
            tooltip.classList.add('hidden');
          }, 300);
        });
      });
    }
  };
  
  /**
   * Form validation helpers
   */
  const Form = {
    /**
     * Initialize form validation
     * @param {string} formId - ID of the form element
     * @param {Object} options - Validation options
     */
    initValidation: function(formId, options = {}) {
      const form = document.getElementById(formId);
      if (!form) return;
      
      const validationOptions = {
        errorClass: 'border-red-500',
        successClass: 'border-green-500',
        errorMessageClass: 'text-sm text-red-500 mt-1',
        ...options
      };
      
      // Handle submit event
      form.addEventListener('submit', (e) => {
        if (!this.validateForm(form, validationOptions)) {
          e.preventDefault();
          e.stopPropagation();
        }
      });
      
      // Add validation on input change
      form.querySelectorAll('input, select, textarea').forEach(input => {
        input.addEventListener('blur', () => {
          this.validateInput(input, validationOptions);
        });
        
        input.addEventListener('input', () => {
          this.validateInput(input, validationOptions);
        });
      });
    },
    
    /**
     * Validate entire form
     * @param {HTMLFormElement} form - Form element
     * @param {Object} options - Validation options
     * @returns {boolean} Valid status
     */
    validateForm: function(form, options = {}) {
      let isValid = true;
      
      form.querySelectorAll('input, select, textarea').forEach(input => {
        if (!this.validateInput(input, options)) {
          isValid = false;
        }
      });
      
      // Add was-validated class for styling
      form.classList.add('was-validated');
      
      return isValid;
    },
    
    /**
     * Validate a single input
     * @param {HTMLInputElement} input - Input element
     * @param {Object} options - Validation options
     * @returns {boolean} Valid status
     */
    validateInput: function(input, options = {}) {
      // Skip disabled, hidden, or readonly inputs
      if (input.disabled || input.type === 'hidden' || input.readOnly) {
        return true;
      }
      
      const errorClass = options.errorClass || 'border-red-500';
      const successClass = options.successClass || 'border-green-500';
      const errorMessageClass = options.errorMessageClass || 'text-sm text-red-500 mt-1';
      
      // Get validation message
      let message = '';
      let isValid = true;
      
      // Check validity using constraint validation API
      if (!input.validity.valid) {
        isValid = false;
        
        if (input.validity.valueMissing) {
          message = input.getAttribute('data-required-message') || 'This field is required';
        } else if (input.validity.typeMismatch) {
          message = input.getAttribute('data-type-message') || 'Please enter a valid value';
        } else if (input.validity.patternMismatch) {
          message = input.getAttribute('data-pattern-message') || 'Value does not match the required pattern';
        } else if (input.validity.tooLong || input.validity.tooShort) {
          message = input.getAttribute('data-length-message') || `Please enter a value between ${input.minLength} and ${input.maxLength} characters`;
        } else if (input.validity.rangeUnderflow || input.validity.rangeOverflow) {
          message = input.getAttribute('data-range-message') || `Please enter a value between ${input.min} and ${input.max}`;
        } else {
          message = input.validationMessage;
        }
      }
      
      // Extra validation for confirmed password
      if (input.getAttribute('data-matches')) {
        const matchInputId = input.getAttribute('data-matches');
        const matchInput = document.getElementById(matchInputId);
        
        if (matchInput && input.value !== matchInput.value) {
          isValid = false;
          message = input.getAttribute('data-matches-message') || 'Values do not match';
        }
      }
      
      // Custom validation function
      if (input.getAttribute('data-validate-fn')) {
        try {
          const validateFn = new Function('value', input.getAttribute('data-validate-fn'));
          const result = validateFn(input.value);
          
          if (result !== true) {
            isValid = false;
            message = result || 'Invalid value';
          }
        } catch (error) {
          console.error('Custom validation error:', error);
        }
      }
      
      // Remove old error message and classes
      const errorElement = input.parentNode.querySelector(`.${errorMessageClass.split(' ')[0]}`);
      if (errorElement) {
        errorElement.remove();
      }
      
      input.classList.remove(errorClass, successClass);
      
      // Add appropriate class and error message
      if (!isValid) {
        input.classList.add(errorClass);
        
        // Add error message if not already present
        if (message) {
          const errorDiv = document.createElement('div');
          errorDiv.className = errorMessageClass;
          errorDiv.textContent = message;
          
          // Insert after input
          input.parentNode.insertBefore(errorDiv, input.nextSibling);
        }
      } else {
        input.classList.add(successClass);
      }
      
      return isValid;
    }
  };
  
  // Export to window
  window.TailwindUI = {
    Modal,
    Toast,
    Dropdown,
    Tooltip,
    Form
  };
  
  // Initialize components on DOM load
  document.addEventListener('DOMContentLoaded', function() {
    Dropdown.init();
    Tooltip.init();
    
    // Initialize forms with data-validate attribute
    document.querySelectorAll('form[data-validate]').forEach(form => {
      Form.initValidation(form.id);
    });
  });