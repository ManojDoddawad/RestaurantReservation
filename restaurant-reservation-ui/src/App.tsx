import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/auth/Login";
import BookTable from "./pages/customer/BookTable";
import { ProtectedRoute } from "./auth/ProtectedRoute";
import AppLayout from "./components/layout/AppLayout";
import Tables from "./pages/admin/Tables";
import TableSchedule from "./pages/admin/TableSchedule";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public */}
        <Route path="/login" element={<Login />} />

        {/* Customer */}
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <AppLayout>
                <BookTable />
              </AppLayout>
            </ProtectedRoute>
          }
        />

        {/* Admin */}
        <Route
          path="/admin"
          element={
            <ProtectedRoute roles={["Admin"]}>
              <AppLayout>
                <Tables />
              </AppLayout>
            </ProtectedRoute>
          }
        />

        <Route
          path="/admin/schedule"
          element={
            <ProtectedRoute roles={["Admin"]}>
              <AppLayout>
                <TableSchedule />
              </AppLayout>
            </ProtectedRoute>
          }
        />

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
