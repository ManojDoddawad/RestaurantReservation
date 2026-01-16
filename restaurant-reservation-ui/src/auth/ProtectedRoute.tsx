import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext";
import type { JSX } from "react";

export const ProtectedRoute = ({
  children,
  roles,
}: {
  children: JSX.Element;
  roles?: string[];
}) => {
  const { user, loading } = useAuth();

  // 🔥 WAIT for auth initialization
  if (loading) {
    return <div className="p-6">Loading...</div>;
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (roles && !roles.includes(user.role)) {
    return <Navigate to="/" replace />;
  }

  return children;
};
