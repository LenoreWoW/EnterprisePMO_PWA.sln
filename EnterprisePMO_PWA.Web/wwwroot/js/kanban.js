document.addEventListener('DOMContentLoaded', function () {
    const columns = document.querySelectorAll('.kanban-column');
    columns.forEach(column => {
        new Sortable(column, {
            group: 'kanban',
            animation: 150,
            onEnd: function (evt) {
                console.log(`Moved item from ${evt.from.id} to ${evt.to.id}`);
                let order = [];
                evt.to.querySelectorAll('.card').forEach(card => {
                    order.push(card.getAttribute('data-id'));
                });
                fetch('/api/kanban/updateorder', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ order: order })
                })
                .then(response => response.json())
                .then(data => console.log("Order update response:", data))
                .catch(err => console.error("Error updating order:", err));
            }
        });
    });
});
