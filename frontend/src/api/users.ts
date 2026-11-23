import { apiClient } from "./client";
import type { CreateUserRequest, UpdateUserRequest, UserDto } from "../types";

export const usersApi = {
  getAll: (departmentId?: string) =>
    apiClient.get<UserDto[]>(departmentId ? `/users?departmentId=${departmentId}` : "/users"),
  getAssignable: () => apiClient.get<UserDto[]>("/users/assignable"),
  getById: (id: string) => apiClient.get<UserDto>(`/users/${id}`),
  create: (request: CreateUserRequest) => apiClient.post<UserDto>("/users", request),
  update: (id: string, request: UpdateUserRequest) => apiClient.put<UserDto>(`/users/${id}`, request),
  delete: (id: string) => apiClient.delete<void>(`/users/${id}`),
};
