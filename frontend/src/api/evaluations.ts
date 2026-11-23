import { apiClient } from "./client";
import type { CreateEvaluationRequest, EvaluationDto, UpdateEvaluationRequest } from "../types";

export const evaluationsApi = {
  getForUser: (userId: string) => apiClient.get<EvaluationDto[]>(`/users/${userId}/evaluations`),
  create: (userId: string, request: CreateEvaluationRequest) =>
    apiClient.post<EvaluationDto>(`/users/${userId}/evaluations`, request),
  update: (userId: string, evaluationId: string, request: UpdateEvaluationRequest) =>
    apiClient.put<EvaluationDto>(`/users/${userId}/evaluations/${evaluationId}`, request),
  delete: (userId: string, evaluationId: string) =>
    apiClient.delete<void>(`/users/${userId}/evaluations/${evaluationId}`),
};
