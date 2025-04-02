document.addEventListener('DOMContentLoaded', function () {
    const columns = document.querySelectorAll('.kanban-column');
    columns.forEach(column => {
        new Sortable(column, {
            group: 'kanban',
            animation: 150,
            onEnd: function (evt) {
                // Log the move
                console.log(`Moved item from ${evt.from.id} to ${evt.to.id}`);
                
                // Collect the new order of tasks
                let order = [];
                evt.to.querySelectorAll('.card').forEach(card => {
                    order.push(card.getAttribute('data-id'));
                });

                // Send update to server
                fetch('/api/kanban/updateorder', {
                    method: 'POST',
                    headers: { 
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${localStorage.getItem('auth_token')}`
                    },
                    body: JSON.stringify({ 
                        order: order,
                        fromColumn: evt.from.id,
                        toColumn: evt.to.id
                    })
                })
                .then(response => response.json())
                .then(data => {
                    // Use Tailwind toast for success/error
                    if (data.success) {
                        window.TailwindUI.Toast.show({
                            message: 'Task order updated successfully',
                            type: 'success'
                        });
                    } else {
                        window.TailwindUI.Toast.show({
                            message: 'Failed to update task order',
                            type: 'error'
                        });
                    }
                })
                .catch(err => {
                    console.error("Error updating order:", err);
                    window.TailwindUI.Toast.show({
                        message: 'Network error updating task order',
                        type: 'error'
                    });
                });
            }
        });
    });
});