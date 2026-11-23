import { apiClient } from "./client";
import type { CreateEnrollmentRequest, EnrollmentDto, UpdateEnrollmentStatusRequest } from "../types";

export const enrollmentsApi = {
  getAll: (params?: { trainingId?: string; userId?: string }) => {
    const query = new URLSearchParams();
    if (params?.trainingId) query.set("trainingId", params.trainingId);
    if (params?.userId) query.set("userId", params.userId);
    const qs = query.toString();
    return apiClient.get<EnrollmentDto[]>(qs ? `/enrollments?${qs}` : "/enrollments");
  },
  getById: (id: string) => apiClient.get<EnrollmentDto>(`/enrollments/${id}`),
  create: (request: CreateEnrollmentRequest) => apiClient.post<EnrollmentDto>("/enrollments", request),
  updateStatus: (id: string, request: UpdateEnrollmentStatusRequest) =>
    apiClient.put<EnrollmentDto>(`/enrollments/${id}/status`, request),
  delete: (id: string) => apiClient.delete<void>(`/enrollments/${id}`),
};
