import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import Sample from "./Sample";


const sampleRoot = document.getElementById('sample-root');

if (sampleRoot) {
    const root = ReactDOM.createRoot(sampleRoot);
    
    root.render(<Sample />);
}

const defaultRoot = document.getElementById('root');

if (defaultRoot) {
    const root = ReactDOM.createRoot(defaultRoot);

    root.render(
        <React.StrictMode>
            <App />
        </React.StrictMode>
    );
}

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
