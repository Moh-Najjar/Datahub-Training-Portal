import { apiClient } from "./client";
import type { LoginRequest, LoginResponse } from "../types";

export const authApi = {
  login: (request: LoginRequest) => apiClient.post<LoginResponse>("/auth/login", request),
};
