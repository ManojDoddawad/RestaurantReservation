import { useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <nav className="bg-black text-white px-6 py-3 flex justify-between items-center">
      <div className="font-bold text-lg">🍽️ Restaurant Reservation</div>

      <div className="flex items-center gap-6">
        {user?.role === "Admin" && (
          <button
            onClick={() => navigate("/admin/schedule")}
            className="hover:underline"
          >
            Table Schedule
          </button>
        )}

        <span className="text-sm">
          {user?.username} ({user?.role})
        </span>

        <button
          onClick={handleLogout}
          className="bg-red-600 px-3 py-1 rounded text-sm"
        >
          Logout
        </button>
      </div>
    </nav>
  );
}
