import { apiClient } from "./client";
import type { CreateTrainingRequest, ReorderTrainingsRequest, TrainingDto, UpdateTrainingRequest } from "../types";

export const trainingsApi = {
  getAll: (departmentId?: string) =>
    apiClient.get<TrainingDto[]>(departmentId ? `/trainings?departmentId=${departmentId}` : "/trainings"),
  getById: (id: string) => apiClient.get<TrainingDto>(`/trainings/${id}`),
  create: (request: CreateTrainingRequest) => apiClient.post<TrainingDto>("/trainings", request),
  update: (id: string, request: UpdateTrainingRequest) =>
    apiClient.put<TrainingDto>(`/trainings/${id}`, request),
  delete: (id: string) => apiClient.delete<void>(`/trainings/${id}`),
  reorder: (request: ReorderTrainingsRequest) => apiClient.put<void>("/trainings/reorder", request),
};
