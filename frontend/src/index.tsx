import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import Sample from "./Sample";

const queryParams = new URLSearchParams(window.location.search);
const viewName = queryParams.get("viewName") ?? "default";

const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement);

root.render(
    <React.StrictMode>
        {viewName.toLowerCase() === "sample" ? <Sample /> : <App />}
    </React.StrictMode>
);

reportWebVitals();
