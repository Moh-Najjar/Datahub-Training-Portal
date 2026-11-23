import { apiClient } from "./client";
import type { CreateDepartmentRequest, DepartmentDto, UpdateDepartmentRequest } from "../types";

export const departmentsApi = {
  getAll: () => apiClient.get<DepartmentDto[]>("/departments"),
  getById: (id: string) => apiClient.get<DepartmentDto>(`/departments/${id}`),
  create: (request: CreateDepartmentRequest) => apiClient.post<DepartmentDto>("/departments", request),
  update: (id: string, request: UpdateDepartmentRequest) =>
    apiClient.put<DepartmentDto>(`/departments/${id}`, request),
  delete: (id: string) => apiClient.delete<void>(`/departments/${id}`),
};
