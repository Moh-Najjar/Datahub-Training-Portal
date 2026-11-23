import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { authApi } from "../api/auth";
import { setAuthToken } from "../api/client";
import type { LoginResponse } from "../types";

interface AuthUser {
  userId: string;
  fullName: string;
  email: string;
  role: string;
  departmentId: string | null;
}

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const STORAGE_KEY = "datahub-training-portal-auth";

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function toAuthUser(response: LoginResponse): AuthUser {
  return {
    userId: response.userId,
    fullName: response.fullName,
    email: response.email,
    role: response.role,
    departmentId: response.departmentId,
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      try {
        const parsed = JSON.parse(stored) as LoginResponse;
        if (new Date(parsed.expiresAt) > new Date()) {
          setAuthToken(parsed.token);
          setUser(toAuthUser(parsed));
        } else {
          localStorage.removeItem(STORAGE_KEY);
        }
      } catch {
        localStorage.removeItem(STORAGE_KEY);
      }
    }
    setIsLoading(false);
  }, []);

  const login = async (email: string, password: string) => {
    const response = await authApi.login({ email, password });
    localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
    setAuthToken(response.token);
    setUser(toAuthUser(response));
  };

  const logout = () => {
    localStorage.removeItem(STORAGE_KEY);
    setAuthToken(null);
    setUser(null);
  };

  const value = useMemo(() => ({ user, isLoading, login, logout }), [user, isLoading]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
