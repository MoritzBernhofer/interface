import { MemoryRouter as Router, Route, Routes } from 'react-router-dom';
import './App.css';
import { useEffect } from 'react';
import { sendMessage } from './services/ipc';

function Hello() {
  useEffect(() => {
    if (window.test_api) {
      sendMessage("servas")
    }
  }, []);

  return <div>hello component</div>;
}

export default function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Hello />} />
      </Routes>
    </Router>
  );
}
