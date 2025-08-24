import React, { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

export default function TodoStatusMessages() {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [messages, setMessages] = useState<string[]>([]);
    
    useEffect(() => {
        // Create connection
        const conn = new signalR.HubConnectionBuilder()
            .withUrl("app://events/todoStatuses", {
                transport: signalR.HttpTransportType.ServerSentEvents
            })
            .withAutomaticReconnect()
            .build();

        // Listen for messages
        conn.on("ReceiveStatus", (message) => {
            setMessages(prev => [...prev, message]);
        });

        conn.start()
            .then(() => console.log("SignalR connected (SSE only)"))
            .catch(console.error);

        setConnection(conn);

        return () => {
            conn.stop();
        };
    }, []);
    
    return (
        <div style={{ padding: "1rem", fontFamily: "sans-serif" }}>
            <h5>Populated via SignalR</h5>
            <ul>
                {messages.map((m, i) => (
                    <li key={i}>{m}</li>
                ))}
            </ul>
        </div>
    );
}