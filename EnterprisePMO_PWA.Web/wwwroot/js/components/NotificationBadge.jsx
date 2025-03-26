import React, { useState, useEffect } from 'react';
import { Bell } from 'lucide-react';
import NotificationsPanel from './NotificationsPanel';

const NotificationBadge = ({ userId }) => {
  const [unreadCount, setUnreadCount] = useState(0);
  const [isNotificationPanelOpen, setIsNotificationPanelOpen] = useState(false);
  const [loading, setLoading] = useState(false);

  // Fetch unread notification count when component mounts and periodically
  useEffect(() => {
    // Fetch initial count
    fetchUnreadCount();

    // Set up polling interval for real-time updates (every 30 seconds)
    const interval = setInterval(fetchUnreadCount, 30000);

    // Clean up on unmount
    return () => clearInterval(interval);
  }, []);

  const fetchUnreadCount = async () => {
    try {
      setLoading(true);
      const response = await fetch('/api/notifications/unread/count');
      if (!response.ok) {
        throw new Error('Failed to fetch notification count');
      }
      const data = await response.json();
      setUnreadCount(data.count || 0);
    } catch (err) {
      console.error('Error fetching unread notifications', err);
    } finally {
      setLoading(false);
    }
  };

  const toggleNotificationPanel = () => {
    setIsNotificationPanelOpen(!isNotificationPanelOpen);
    
    // Reset badge count when opening panel
    if (!isNotificationPanelOpen) {
      // We don't immediately reset the count to give the user a moment to see the notifications
      // This will be updated when they mark items as read or when the panel closes
    }
  };

  const handlePanelClose = () => {
    setIsNotificationPanelOpen(false);
    
    // Refresh the unread count when panel closes (in case users read notifications)
    fetchUnreadCount();
  };

  return (
    <div className="relative">
      <button 
        className="relative flex items-center justify-center p-2 rounded-full text-gray-700 hover:bg-gray-100 focus:outline-none"
        onClick={toggleNotificationPanel}
      >
        <Bell className="h-6 w-6" />
        
        {unreadCount > 0 && (
          <span className="absolute top-0 right-0 flex items-center justify-center h-5 w-5 text-xs font-bold text-white bg-red-500 rounded-full">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>
      
      {/* Notification Panel */}
      <NotificationsPanel 
        userId={userId}
        isOpen={isNotificationPanelOpen}
        onClose={handlePanelClose}
      />
    </div>
  );
};

export default NotificationBadge;