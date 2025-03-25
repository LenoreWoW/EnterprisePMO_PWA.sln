import { HubConnectionBuilder } from "https://cdn.jsdelivr.net/npm/@microsoft/signalr@9.0.0/dist/esm/index.js";

const connection = new HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", (message) => {
    console.log("Notification received:", message);
    alert("Notification: " + message);
});

connection.start()
    .then(() => console.log("SignalR connection established."))
    .catch(err => console.error("Error starting SignalR connection:", err));
