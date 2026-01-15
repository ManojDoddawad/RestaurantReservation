// src/context/AuthContext.jsx
import { createContext, useContext, useState, useEffect } from 'react';
import { auth } from '../services/api';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check for existing token on app load
    const token = localStorage.getItem('token');
    if (token) {
      // Verify token and get user data
      // You might want to add a /me endpoint to your API for this
      setUser({ token });
    }
    setLoading(false);
  }, []);

  const login = async (credentials) => {
    const response = await auth.login(credentials);
    localStorage.setItem('token', response.data.token);
    setUser(response.data);
    return response;
  };

  const register = async (userData) => {
    const response = await auth.register(userData);
    localStorage.setItem('token', response.data.token);
    setUser(response.data);
    return response;
  };

  const logout = () => {
    localStorage.removeItem('token');
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, login, register, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
