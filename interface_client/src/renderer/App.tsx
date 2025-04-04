import { MemoryRouter as Router, Route, Routes } from 'react-router-dom';
import './App.css';

function Hello() {
  window.test_api.ipcRenderer.sendMessage('sendMessage', 'hello');

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
