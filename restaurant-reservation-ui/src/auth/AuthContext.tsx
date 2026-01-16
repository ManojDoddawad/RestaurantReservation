import { createContext, useContext, useEffect, useState } from "react";
import { authApi } from "../api/auth.api";
import { getToken, setToken, clearToken } from "../utils/token";

interface AuthUser {
  userId: number;
  username: string;
  email: string;
  role: string;
}

/* ✅ ADD THIS INTERFACE HERE */
interface AuthContextType {
  user: AuthUser | null;
  loading: boolean;
  login: (token: string) => Promise<void>;
  logout: () => void;
}

/* ✅ CREATE CONTEXT USING THE INTERFACE */
const AuthContext = createContext<AuthContextType>({} as AuthContextType);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  const loadUser = async () => {
    try {
      const res = await authApi.me();
      setUser(res.data);
    } catch (err) {
      console.warn("Auth init failed", err);
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  /* ✅ login returns Promise<void> */
  const login = async (token: string): Promise<void> => {
    setToken(token);
    await loadUser();
  };

  const logout = () => {
    clearToken();
    setUser(null);
  };

  useEffect(() => {
    if (getToken()) {
      loadUser();
    } else {
      setLoading(false);
    }
  }, []);

  /* ✅ PROVIDER VALUE GOES HERE */
  return (
    <AuthContext.Provider value={{ user, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

/* ✅ CUSTOM HOOK */
export const useAuth = () => useContext(AuthContext);
