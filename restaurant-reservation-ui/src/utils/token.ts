import { jwtDecode } from "jwt-decode";


export interface JwtPayload {
  sub?: string;
  role?: string;
  email?: string;
}

export const getToken = () => localStorage.getItem("token");

export const setToken = (token: string) => {
  localStorage.setItem("token", token);
};

export const clearToken = () => {
  localStorage.removeItem("token");
};

export const decodeToken = (token: string): JwtPayload =>
  jwtDecode(token);
