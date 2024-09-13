import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Route, Routes, Link, Navigate } from 'react-router-dom';
import './App.css';
import RPSLSGame from './components/RPSSLGame';
import MultiplayerGame from './components/MultiplayerGame';
import Register from './components/Register';
import Login from './components/Login';
import NewLobby from './components/NewLobby';

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [token, setToken] = useState<string | null>(null);
  const [username, setUsername] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedToken = localStorage.getItem('token');
    const storedUsername = localStorage.getItem('username');
    debugger;
    if (storedToken && storedUsername) {
      setToken(storedToken);
      setUsername(storedUsername);
      setIsLoggedIn(true);
    }
    setIsLoading(false);
  }, []);

  const handleLogin = (newToken: string, newUsername: string) => {
    setIsLoggedIn(true);
    setToken(newToken);
    setUsername(newUsername);
    setIsLoggedIn(true);
    localStorage.setItem('token', newToken);
    localStorage.setItem('username', newUsername);
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setToken(null);
    localStorage.removeItem('token');
    localStorage.removeItem('username');
  };
  
  if (isLoading) {
    return <div>Loading...</div>; 
  }

  return (
    <Router>
      <div className="flex flex-col min-h-screen">
        <header className="bg-gray-800 text-white p-4">
          <h1 className="text-2xl text-center">Rock Paper Scissors Lizard Spock</h1>
          <nav className="mt-2 text-center">
            <Link to="/" className="mr-4">Home</Link>
            {isLoggedIn ? (
              <>
                <Link to="/lobby" className="mr-4">Lobby</Link>
                <button onClick={handleLogout}>Logout</button>
                <span className="ml-4">Welcome, {username}!</span>
              </>
            ) : (
              <>
                <Link to="/login" className="mr-4">Login</Link>
              </>
            )}
          </nav>
        </header>
        <main className="flex-grow p-4 overflow-auto">
          <Routes>
            <Route path="/" element={<RPSLSGame />} />
            <Route path="/login" element={isLoggedIn ? <Navigate to="/lobby" /> : <Login onLogin={handleLogin} />} />
            <Route path="/register" element={isLoggedIn ? <Navigate to="/lobby" /> : <Register />} />
            <Route 
              path="/lobby" 
              element={isLoggedIn ? <NewLobby token={token} /> : <Navigate to="/login" />} 
            />            
            <Route 
              path="/room/:roomId" 
              element={isLoggedIn ? <MultiplayerGame token={token} currentUserId={username} /> : <Navigate to="/login" />} 
            />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;
