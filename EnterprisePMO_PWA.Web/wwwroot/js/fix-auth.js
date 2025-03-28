// Save as fix-auth.js in your wwwroot/js folder
(function() {
    const token = localStorage.getItem('auth_token');
    
    if (token && !localStorage.getItem('user_info')) {
      // Decode the token to get user data
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        
        // Create user info object
        const userInfo = {
          id: payload.nameid,
          username: payload.unique_name,
          role: payload.role
        };
        
        // Store it
        localStorage.setItem('user_info', JSON.stringify(userInfo));
        console.log('User info restored:', userInfo);
        
        // Reload page to apply changes
        setTimeout(() => window.location.reload(), 500);
      } catch (e) {
        console.error('Failed to decode token:', e);
      }
    }
  })();