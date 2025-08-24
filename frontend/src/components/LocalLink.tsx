import React from 'react';

export interface ILocalLinkProps {
    fileName: string;
    text: string;
}

export const LocalLink: React.FC<ILocalLinkProps> = ({ fileName, text }) => {
    const handleClick = async (e: React.MouseEvent) => {
        e.preventDefault(); // stop <a> from navigating

        try {
            const response = await fetch("app://api/api/AppShell/new-window", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({"FileName": fileName})
            });

            if (!response.ok) console.error("Failed to open new window", response.statusText);
        } catch (err) {
            console.error("Error calling API:", err);
        }
    };

    return <a href="#" onClick={handleClick}>{text}</a>;
};