/**
 * Bootstrap to Tailwind CSS Converter Script
 * 
 * This script helps automate the conversion of common Bootstrap classes to Tailwind CSS
 * It can be run on HTML files to perform basic class replacements
 * 
 * Usage: 
 * node bootstrap-to-tailwind-converter.js <file-path>
 */

const fs = require('fs');
const path = require('path');

// Class mappings from Bootstrap to Tailwind
const classMappings = {
  // Typography
  'h1': 'text-4xl font-bold',
  'h2': 'text-3xl font-bold',
  'h3': 'text-2xl font-bold',
  'h4': 'text-xl font-bold',
  'h5': 'text-lg font-bold',
  'h6': 'text-base font-bold',
  'lead': 'text-xl',
  'small': 'text-sm',
  'text-muted': 'text-gray-500',
  'font-weight-bold': 'font-bold',
  'font-italic': 'italic',
  'text-uppercase': 'uppercase',
  'text-capitalize': 'capitalize',
  'text-center': 'text-center',
  'text-right': 'text-right',
  'text-left': 'text-left',
  'text-primary': 'text-blue-600',
  'text-secondary': 'text-gray-600',
  'text-success': 'text-green-600',
  'text-danger': 'text-red-600',
  'text-warning': 'text-yellow-600',
  'text-info': 'text-blue-500',
  'text-white': 'text-white',
  'text-dark': 'text-gray-900',
  'text-light': 'text-gray-100',
  
  // Containers and Layout
  'container': 'container mx-auto px-4',
  'container-fluid': 'w-full px-4',
  'row': 'flex flex-wrap -mx-4',
  
  // Grid Columns
  'col': 'flex-1 px-4',
  'col-1': 'w-1/12 px-4',
  'col-2': 'w-1/6 px-4',
  'col-3': 'w-1/4 px-4',
  'col-4': 'w-1/3 px-4',
  'col-5': 'w-5/12 px-4',
  'col-6': 'w-1/2 px-4',
  'col-7': 'w-7/12 px-4',
  'col-8': 'w-2/3 px-4',
  'col-9': 'w-3/4 px-4',
  'col-10': 'w-5/6 px-4', 
  'col-11': 'w-11/12 px-4',
  'col-12': 'w-full px-4',
  
  // Responsive Grid Columns
  'col-sm-1': 'sm:w-1/12 px-4',
  'col-sm-2': 'sm:w-1/6 px-4',
  'col-sm-3': 'sm:w-1/4 px-4',
  'col-sm-4': 'sm:w-1/3 px-4',
  'col-sm-5': 'sm:w-5/12 px-4',
  'col-sm-6': 'sm:w-1/2 px-4',
  'col-sm-7': 'sm:w-7/12 px-4',
  'col-sm-8': 'sm:w-2/3 px-4',
  'col-sm-9': 'sm:w-3/4 px-4',
  'col-sm-10': 'sm:w-5/6 px-4',
  'col-sm-11': 'sm:w-11/12 px-4',
  'col-sm-12': 'sm:w-full px-4',
  
  'col-md-1': 'md:w-1/12 px-4',
  'col-md-2': 'md:w-1/6 px-4',
  'col-md-3': 'md:w-1/4 px-4',
  'col-md-4': 'md:w-1/3 px-4',
  'col-md-5': 'md:w-5/12 px-4',
  'col-md-6': 'md:w-1/2 px-4',
  'col-md-7': 'md:w-7/12 px-4',
  'col-md-8': 'md:w-2/3 px-4',
  'col-md-9': 'md:w-3/4 px-4',
  'col-md-10': 'md:w-5/6 px-4',
  'col-md-11': 'md:w-11/12 px-4',
  'col-md-12': 'md:w-full px-4',
  
  'col-lg-1': 'lg:w-1/12 px-4',
  'col-lg-2': 'lg:w-1/6 px-4',
  'col-lg-3': 'lg:w-1/4 px-4',
  'col-lg-4': 'lg:w-1/3 px-4',
  'col-lg-5': 'lg:w-5/12 px-4',
  'col-lg-6': 'lg:w-1/2 px-4',
  'col-lg-7': 'lg:w-7/12 px-4',
  'col-lg-8': 'lg:w-2/3 px-4',
  'col-lg-9': 'lg:w-3/4 px-4',
  'col-lg-10': 'lg:w-5/6 px-4',
  'col-lg-11': 'lg:w-11/12 px-4',
  'col-lg-12': 'lg:w-full px-4',
  
  'col-xl-1': 'xl:w-1/12 px-4',
  'col-xl-2': 'xl:w-1/6 px-4',
  'col-xl-3': 'xl:w-1/4 px-4',
  'col-xl-4': 'xl:w-1/3 px-4',
  'col-xl-5': 'xl:w-5/12 px-4',
  'col-xl-6': 'xl:w-1/2 px-4',
  'col-xl-7': 'xl:w-7/12 px-4',
  'col-xl-8': 'xl:w-2/3 px-4',
  'col-xl-9': 'xl:w-3/4 px-4',
  'col-xl-10': 'xl:w-5/6 px-4',
  'col-xl-11': 'xl:w-11/12 px-4',
  'col-xl-12': 'xl:w-full px-4',
  
  // Buttons
  'btn': 'py-2 px-4 rounded font-medium focus:outline-none focus:ring-2 focus:ring-offset-2',
  'btn-primary': 'bg-blue-600 hover:bg-blue-700 text-white focus:ring-blue-500',
  'btn-secondary': 'bg-gray-600 hover:bg-gray-700 text-white focus:ring-gray-500',
  'btn-success': 'bg-green-600 hover:bg-green-700 text-white focus:ring-green-500',
  'btn-danger': 'bg-red-600 hover:bg-red-700 text-white focus:ring-red-500',
  'btn-warning': 'bg-yellow-500 hover:bg-yellow-600 text-white focus:ring-yellow-500',
  'btn-info': 'bg-blue-400 hover:bg-blue-500 text-white focus:ring-blue-400',
  'btn-light': 'bg-gray-100 hover:bg-gray-200 text-gray-800 focus:ring-gray-100',
  'btn-dark': 'bg-gray-800 hover:bg-gray-900 text-white focus:ring-gray-800',
  'btn-outline-primary': 'bg-transparent hover:bg-blue-600 text-blue-600 hover:text-white border border-blue-600',
  'btn-outline-secondary': 'bg-transparent hover:bg-gray-600 text-gray-600 hover:text-white border border-gray-600',
  'btn-outline-success': 'bg-transparent hover:bg-green-600 text-green-600 hover:text-white border border-green-600',
  'btn-outline-danger': 'bg-transparent hover:bg-red-600 text-red-600 hover:text-white border border-red-600',
  'btn-outline-warning': 'bg-transparent hover:bg-yellow-500 text-yellow-500 hover:text-white border border-yellow-500',
  'btn-outline-info': 'bg-transparent hover:bg-blue-400 text-blue-400 hover:text-white border border-blue-400',
  'btn-outline-light': 'bg-transparent hover:bg-gray-100 text-gray-100 hover:text-gray-800 border border-gray-100',
  'btn-outline-dark': 'bg-transparent hover:bg-gray-800 text-gray-800 hover:text-white border border-gray-800',
  'btn-sm': 'py-1 px-3 text-sm',
  'btn-lg': 'py-3 px-5 text-lg',
  'btn-block': 'w-full',
  
  // Forms
  'form-group': 'mb-4',
  'form-control': 'block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500',
  'form-control-lg': 'text-lg px-4 py-3',
  'form-control-sm': 'text-sm px-2 py-1',
  'form-check': 'flex items-center',
  'form-check-input': 'h-4 w-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500',
  'form-check-label': 'ml-2 block text-gray-900',
  'form-text': 'mt-1 text-sm text-gray-500',
  'form-select': 'block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500',
  'input-group': 'flex rounded-md shadow-sm',
  'input-group-text': 'inline-flex items-center px-3 py-2 border border-r-0 border-gray-300 bg-gray-50 text-gray-500',
  'is-invalid': 'border-red-500 focus:ring-red-500 focus:border-red-500',
  'invalid-feedback': 'mt-1 text-sm text-red-600',
  
  // Cards
  'card': 'bg-white rounded-lg border overflow-hidden shadow-sm',
  'card-header': 'px-4 py-5 sm:px-6 border-b border-gray-200 bg-gray-50',
  'card-body': 'p-4 sm:p-6',
  'card-footer': 'px-4 py-4 sm:px-6 bg-gray-50 border-t border-gray-200',
  'card-title': 'text-lg font-medium text-gray-900',
  'card-subtitle': 'text-sm font-medium text-gray-500',
  'card-text': 'mt-2 text-gray-600',
  'card-link': 'text-blue-600 hover:text-blue-800',
  'card-img-top': 'w-full',
  
  // Alerts
  'alert': 'p-4 rounded-md border',
  'alert-primary': 'bg-blue-50 border-blue-400 text-blue-700',
  'alert-secondary': 'bg-gray-50 border-gray-400 text-gray-700',
  'alert-success': 'bg-green-50 border-green-400 text-green-700',
  'alert-danger': 'bg-red-50 border-red-400 text-red-700',
  'alert-warning': 'bg-yellow-50 border-yellow-400 text-yellow-700',
  'alert-info': 'bg-blue-50 border-blue-400 text-blue-700',
  'alert-light': 'bg-gray-50 border-gray-300 text-gray-700',
  'alert-dark': 'bg-gray-700 border-gray-600 text-white',
  
  // Badges
  'badge': 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
  'badge-primary': 'bg-blue-100 text-blue-800',
  'badge-secondary': 'bg-gray-100 text-gray-800',
  'badge-success': 'bg-green-100 text-green-800',
  'badge-danger': 'bg-red-100 text-red-800',
  'badge-warning': 'bg-yellow-100 text-yellow-800',
  'badge-info': 'bg-blue-100 text-blue-800',
  'badge-light': 'bg-gray-100 text-gray-500',
  'badge-dark': 'bg-gray-800 text-white',
  'badge-pill': 'rounded-full',
  
  // Tables
  'table': 'min-w-full divide-y divide-gray-200',
  'table-striped': '', // Need to apply odd:bg-gray-50 to rows
  'table-bordered': 'border',
  'table-hover': '', // Need to apply hover:bg-gray-50 to rows
  'table-sm': 'text-sm',
  'thead-light': 'bg-gray-50',
  'table-responsive': 'overflow-x-auto',
  
  // Utilities
  'd-flex': 'flex',
  'd-inline-flex': 'inline-flex',
  'flex-row': 'flex-row',
  'flex-column': 'flex-col',
  'justify-content-start': 'justify-start',
  'justify-content-center': 'justify-center',
  'justify-content-end': 'justify-end',
  'justify-content-between': 'justify-between',
  'justify-content-around': 'justify-around',
  'align-items-start': 'items-start',
  'align-items-center': 'items-center',
  'align-items-end': 'items-end',
  'align-self-start': 'self-start',
  'align-self-center': 'self-center',
  'align-self-end': 'self-end',
  
  // Spacing
  'm-0': 'm-0',
  'm-1': 'm-1',
  'm-2': 'm-2',
  'm-3': 'm-3',
  'm-4': 'm-4',
  'm-5': 'm-5',
  
  'mt-0': 'mt-0',
  'mt-1': 'mt-1',
  'mt-2': 'mt-2',
  'mt-3': 'mt-3',
  'mt-4': 'mt-4',
  'mt-5': 'mt-5',
  
  'mb-0': 'mb-0',
  'mb-1': 'mb-1',
  'mb-2': 'mb-2',
  'mb-3': 'mb-3',
  'mb-4': 'mb-4',
  'mb-5': 'mb-5',
  
  'ml-0': 'ml-0',
  'ml-1': 'ml-1',
  'ml-2': 'ml-2',
  'ml-3': 'ml-3',
  'ml-4': 'ml-4',
  'ml-5': 'ml-5',
  
  'mr-0': 'mr-0',
  'mr-1': 'mr-1',
  'mr-2': 'mr-2',
  'mr-3': 'mr-3',
  'mr-4': 'mr-4',
  'mr-5': 'mr-5',
  
  'mx-0': 'mx-0',
  'mx-1': 'mx-1',
  'mx-2': 'mx-2',
  'mx-3': 'mx-3',
  'mx-4': 'mx-4',
  'mx-5': 'mx-5',
  'mx-auto': 'mx-auto',
  
  'my-0': 'my-0',
  'my-1': 'my-1',
  'my-2': 'my-2',
  'my-3': 'my-3',
  'my-4': 'my-4',
  'my-5': 'my-5',
  'my-auto': 'my-auto',
  
  'p-0': 'p-0',
  'p-1': 'p-1',
  'p-2': 'p-2',
  'p-3': 'p-3',
  'p-4': 'p-4',
  'p-5': 'p-5',
  
  'pt-0': 'pt-0',
  'pt-1': 'pt-1',
  'pt-2': 'pt-2',
  'pt-3': 'pt-3',
  'pt-4': 'pt-4',
  'pt-5': 'pt-5',
  
  'pb-0': 'pb-0',
  'pb-1': 'pb-1',
  'pb-2': 'pb-2',
  'pb-3': 'pb-3',
  'pb-4': 'pb-4',
  'pb-5': 'pb-5',
  
  'pl-0': 'pl-0',
  'pl-1': 'pl-1',
  'pl-2': 'pl-2',
  'pl-3': 'pl-3',
  'pl-4': 'pl-4',
  'pl-5': 'pl-5',
  
  'pr-0': 'pr-0',
  'pr-1': 'pr-1',
  'pr-2': 'pr-2',
  'pr-3': 'pr-3',
  'pr-4': 'pr-4',
  'pr-5': 'pr-5',
  
  'px-0': 'px-0',
  'px-1': 'px-1',
  'px-2': 'px-2',
  'px-3': 'px-3',
  'px-4': 'px-4',
  'px-5': 'px-5',
  
  'py-0': 'py-0',
  'py-1': 'py-1',
  'py-2': 'py-2',
  'py-3': 'py-3',
  'py-4': 'py-4',
  'py-5': 'py-5',
  
  // Display
  'd-none': 'hidden',
  'd-block': 'block',
  'd-inline': 'inline',
  'd-inline-block': 'inline-block',
  'd-table': 'table',
  'd-table-row': 'table-row',
  'd-table-cell': 'table-cell',
  
  // Width and Height
  'w-25': 'w-1/4',
  'w-50': 'w-1/2',
  'w-75': 'w-3/4',
  'w-100': 'w-full',
  'w-auto': 'w-auto',
  
  'h-25': 'h-1/4',
  'h-50': 'h-1/2',
  'h-75': 'h-3/4',
  'h-100': 'h-full',
  'h-auto': 'h-auto',
  
  // Position
  'position-static': 'static',
  'position-relative': 'relative',
  'position-absolute': 'absolute',
  'position-fixed': 'fixed',
  'position-sticky': 'sticky',
  
  // Background colors
  'bg-primary': 'bg-blue-600',
  'bg-secondary': 'bg-gray-600',
  'bg-success': 'bg-green-600',
  'bg-danger': 'bg-red-600',
  'bg-warning': 'bg-yellow-500',
  'bg-info': 'bg-blue-400',
  'bg-light': 'bg-gray-100',
  'bg-dark': 'bg-gray-800',
  'bg-white': 'bg-white',
  'bg-transparent': 'bg-transparent',
  
  // Borders
  'border': 'border',
  'border-top': 'border-t',
  'border-right': 'border-r',
  'border-bottom': 'border-b',
  'border-left': 'border-l',
  'border-0': 'border-0',
  'border-top-0': 'border-t-0',
  'border-right-0': 'border-r-0',
  'border-bottom-0': 'border-b-0',
  'border-left-0': 'border-l-0',
  'border-primary': 'border-blue-600',
  'border-secondary': 'border-gray-600',
  'border-success': 'border-green-600',
  'border-danger': 'border-red-600',
  'border-warning': 'border-yellow-500',
  'border-info': 'border-blue-400',
  'border-light': 'border-gray-100',
  'border-dark': 'border-gray-800',
  'border-white': 'border-white',
  
  // Rounded corners
  'rounded': 'rounded',
  'rounded-sm': 'rounded-sm',
  'rounded-lg': 'rounded-lg',
  'rounded-top': 'rounded-t',
  'rounded-right': 'rounded-r',
  'rounded-bottom': 'rounded-b',
  'rounded-left': 'rounded-l',
  'rounded-circle': 'rounded-full',
  'rounded-pill': 'rounded-full',
  'rounded-0': 'rounded-none',
  
  // Shadows
  'shadow-sm': 'shadow-sm',
  'shadow': 'shadow',
  'shadow-lg': 'shadow-lg',
  'shadow-none': 'shadow-none',
  
  // Visibility
  'visible': 'visible',
  'invisible': 'invisible',
  
  // Others
  'sr-only': 'sr-only',
  'fixed-top': 'fixed top-0 left-0 right-0 z-50',
  'fixed-bottom': 'fixed bottom-0 left-0 right-0 z-50',
  'sticky-top': 'sticky top-0 z-50',
  'float-left': 'float-left',
  'float-right': 'float-right',
  'float-none': 'float-none',
  'clearfix': 'clearfix',
  'overflow-auto': 'overflow-auto',
  'overflow-hidden': 'overflow-hidden',
  'text-nowrap': 'whitespace-nowrap',
  'text-truncate': 'truncate',
  'font-weight-normal': 'font-normal',
  'font-weight-light': 'font-light',
  'font-weight-lighter': 'font-extralight',
  'font-weight-bold': 'font-bold',
  'font-weight-bolder': 'font-extrabold',
  'font-italic': 'italic',
  'text-decoration-none': 'no-underline',
  'text-break': 'break-words',
  'user-select-all': 'select-all',
  'user-select-auto': 'select-auto',
  'user-select-none': 'select-none'
};

// Special case replacements that need more complex transformations
const specialCases = [
  // Table striping - applies to tr elements usually
  {
    regex: /<tr\b([^>]*)>/g,
    replacement: (match, attributes) => {
      // Add odd:bg-gray-50 for table striping if table-striped was applied to the table
      if (attributes.includes('table-striped')) {
        return `<tr${attributes} class="odd:bg-gray-50">`;
      }
      return match;
    }
  },
  // Table hover - applies to tr elements usually
  {
    regex: /<tr\b([^>]*)>/g,
    replacement: (match, attributes) => {
      // Add hover:bg-gray-50 for table hover if table-hover was applied to the table
      if (attributes.includes('table-hover')) {
        return `<tr${attributes} class="hover:bg-gray-50">`;
      }
      return match;
    }
  }
];

/**
 * Convert a single class name from Bootstrap to Tailwind
 * @param {string} className - The Bootstrap class name
 * @returns {string} The Tailwind equivalent
 */
function convertClass(className) {
  return classMappings[className] || className;
}

/**
 * Convert Bootstrap class strings to Tailwind
 * @param {string} classStr - Space-separated Bootstrap classes
 * @returns {string} Space-separated Tailwind classes
 */
function convertClassString(classStr) {
  if (!classStr) return '';
  
  // Split classes and convert each one
  const classes = classStr.split(/\s+/);
  const convertedClasses = classes.map(cls => convertClass(cls));
  
  // Return the joined string
  return convertedClasses.join(' ');
}

/**
 * Process class attributes in HTML content
 * @param {string} content - HTML content
 * @returns {string} Processed HTML content
 */
function processClassAttributes(content) {
  // Regular expression to find class attributes
  const classRegex = /class=["']([^"']*)["']/g;
  
  // Replace each class attribute with converted classes
  return content.replace(classRegex, (match, classStr) => {
    const convertedClasses = convertClassString(classStr);
    return `class="${convertedClasses}"`;
  });
}

/**
 * Apply special case transformations
 * @param {string} content - HTML content
 * @returns {string} Processed HTML content
 */
function applySpecialCases(content) {
  let result = content;
  
  // Apply each special case
  specialCases.forEach(({ regex, replacement }) => {
    result = result.replace(regex, replacement);
  });
  
  return result;
}

/**
 * Convert a file from Bootstrap to Tailwind
 * @param {string} filePath - Path to the file
 * @returns {string} Converted file content
 */
function convertFile(filePath) {
  // Check file exists
  if (!fs.existsSync(filePath)) {
    console.error(`File not found: ${filePath}`);
    process.exit(1);
  }
  
  // Read file content
  const content = fs.readFileSync(filePath, 'utf8');
  
  // Process the content
  let processedContent = processClassAttributes(content);
  processedContent = applySpecialCases(processedContent);
  
  // Update layout reference if this is a view
  if (filePath.endsWith('.cshtml')) {
    processedContent = processedContent.replace(
      /Layout\s*=\s*["']_Layout["']/g, 
      'Layout = "_TailwindLayout"'
    );
    processedContent = processedContent.replace(
      /Layout\s*=\s*["']_AuthLayout["']/g, 
      'Layout = "_TailwindLayout"'
    );
  }
  
  return processedContent;
}

/**
 * Save converted content to a file
 * @param {string} filePath - Path to the original file
 * @param {string} content - Converted content
 */
function saveConvertedFile(filePath, content) {
  const dir = path.dirname(filePath);
  const basename = path.basename(filePath, path.extname(filePath));
  const ext = path.extname(filePath);
  
  // Create new file with .tailwind extension
  const newFilePath = path.join(dir, `${basename}.tailwind${ext}`);
  
  // Write content to new file
  fs.writeFileSync(newFilePath, content);
  
  console.log(`Converted file saved to: ${newFilePath}`);
}

/**
 * Main function
 */
function main() {
  // Get file path from command line
  const args = process.argv.slice(2);
  if (args.length === 0) {
    console.error('Please provide a file path');
    process.exit(1);
  }
  
  const filePath = args[0];
  
  // Convert file
  const convertedContent = convertFile(filePath);
  
  // Save converted file
  saveConvertedFile(filePath, convertedContent);
}

// Run the script
main();