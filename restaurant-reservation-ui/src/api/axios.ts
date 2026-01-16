import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7050/api", // change if needed
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  res => res.data, // unwrap ApiResponse<T>
  err => Promise.reject(err.response?.data || err)
);

export default api;
