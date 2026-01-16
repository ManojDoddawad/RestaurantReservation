import { useState } from "react";
import { authApi } from "../../api/auth.api";
import { useAuth } from "../../auth/AuthContext";
import { useNavigate } from "react-router-dom";

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const submit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault(); // 🔥 THIS IS NON-NEGOTIABLE
    e.stopPropagation(); // 🔥 EXTRA SAFETY

    try {
      const res = await authApi.login({ username, password });
      await login(res.data.token);

      navigate("/", { replace: true }); // 🔥 FORCE SPA NAVIGATION
    } catch (err) {
      alert("Invalid credentials");
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center">
      <form
        onSubmit={submit}
        className="w-80 space-y-4"
        noValidate // 🔥 IMPORTANT
      >
        <h2 className="text-2xl font-bold">Login</h2>

        <input
          className="border p-2 w-full"
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />

        <input
          type="password"
          className="border p-2 w-full"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />

        <button
          type="submit" // 🔥 EXPLICIT
          className="bg-black text-white w-full p-2"
        >
          Login
        </button>
      </form>
    </div>
  );
}
