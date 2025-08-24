import React, { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

export default function SignalRDemo() {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [messages, setMessages] = useState<string[]>([]);
    const [input, setInput] = useState("");

    useEffect(() => {
        // Create connection
        const conn = new signalR.HubConnectionBuilder()
            .withUrl("app://events/demo", {
                transport: signalR.HttpTransportType.ServerSentEvents
            })
            .withAutomaticReconnect()
            .build();

        // Listen for messages
        conn.on("ReceiveMessage", (user, message) => {
            setMessages(prev => [...prev, `${user}: ${message}`]);
        });

        conn.start()
            .then(() => console.log("SignalR connected (SSE only)"))
            .catch(console.error);

        setConnection(conn);

        return () => {
            conn.stop();
        };
    }, []);

    const sendMessage = async () => {
        if (connection) {
            try {
                await connection.invoke("SendMessage", "ReactClient", input);
                setInput("");
            } catch (err) {
                console.error("Send failed:", err);
            }
        }
    };

    return (
        <div style={{ padding: "1rem", fontFamily: "sans-serif" }}>
            <h2>SignalR Demo (SSE only)</h2>
            <div>
                <input
                    type="text"
                    value={input}
                    onChange={e => setInput(e.target.value)}
                    placeholder="Type a message"
                />
                <button onClick={sendMessage}>Send</button>
            </div>
            <ul>
                {messages.map((m, i) => (
                    <li key={i}>{m}</li>
                ))}
            </ul>
        </div>
    );
}
