import api from "./axios";
import type { LoginDto, RegisterDto, AuthResponse } from "../models/auth";

export const authApi = {
  login: (data: LoginDto) =>
    api.post<AuthResponse>("/Auth/login", data),

  register: (data: RegisterDto) =>
    api.post<AuthResponse>("/Auth/register", data),

  me: () =>
    api.get("/Auth/me"),
};
