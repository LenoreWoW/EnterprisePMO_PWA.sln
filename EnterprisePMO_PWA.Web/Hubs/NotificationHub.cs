using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace EnterprisePMO_PWA.Web.Hubs
{
    public class NotificationHub : Hub
    {
        // Simple hub method to broadcast a notification to all clients
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
