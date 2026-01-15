// src/components/Layout.jsx
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Layout = ({ children }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="min-h-screen flex flex-col">
      <header className="bg-gray-800 text-white p-4">
        <div className="container mx-auto flex justify-between items-center">
          <Link to="/" className="text-xl font-bold">Restaurant Res</Link>
          <nav className="flex space-x-4">
            {user ? (
              <>
                <Link to="/menu" className="hover:text-gray-300">Menu</Link>
                <Link to="/book" className="hover:text-gray-300">Book</Link>
                <Link to="/reservations" className="hover:text-gray-300">My Reservations</Link>
                <button onClick={handleLogout} className="hover:text-gray-300">Logout</button>
              </>
            ) : (
              <>
                <Link to="/login" className="hover:text-gray-300">Login</Link>
                <Link to="/register" className="hover:text-gray-300">Register</Link>
              </>
            )}
          </nav>
        </div>
      </header>
      <main className="container mx-auto py-8 flex-grow">
        {children}
      </main>
      <footer className="bg-gray-800 text-white p-4 text-center">
        &copy; {new Date().getFullYear()} Restaurant Reservation System
      </footer>
    </div>
  );
};

export default Layout;
