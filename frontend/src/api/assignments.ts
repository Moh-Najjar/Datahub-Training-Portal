import { apiClient, downloadFile } from "./client";
import type { AssignmentDto, UpdateAssignmentRequest } from "../types";

export interface CreateAssignmentInput {
  title: string;
  description: string;
  opensAt: string;
  closesAt: string;
  files: File[];
}

function buildAssignmentForm(input: Omit<CreateAssignmentInput, "files"> & { files?: File[] }): FormData {
  const form = new FormData();
  form.append("title", input.title);
  form.append("description", input.description);
  form.append("opensAt", input.opensAt);
  form.append("closesAt", input.closesAt);
  (input.files ?? []).forEach((file) => form.append("attachments", file));
  return form;
}

export const assignmentsApi = {
  getForTraining: (trainingId: string) =>
    apiClient.get<AssignmentDto[]>(`/trainings/${trainingId}/assignments`),
  create: (trainingId: string, input: CreateAssignmentInput) =>
    apiClient.postForm<AssignmentDto>(`/trainings/${trainingId}/assignments`, buildAssignmentForm(input)),
  update: (trainingId: string, assignmentId: string, request: UpdateAssignmentRequest) =>
    apiClient.put<AssignmentDto>(`/trainings/${trainingId}/assignments/${assignmentId}`, request),
  delete: (trainingId: string, assignmentId: string) =>
    apiClient.delete<void>(`/trainings/${trainingId}/assignments/${assignmentId}`),
  addAttachments: (trainingId: string, assignmentId: string, files: File[]) => {
    const form = new FormData();
    files.forEach((file) => form.append("files", file));
    return apiClient.postForm<AssignmentDto>(
      `/trainings/${trainingId}/assignments/${assignmentId}/attachments`,
      form
    );
  },
  deleteAttachment: (trainingId: string, assignmentId: string, attachmentId: string) =>
    apiClient.delete<void>(`/trainings/${trainingId}/assignments/${assignmentId}/attachments/${attachmentId}`),
  downloadAttachment: (trainingId: string, assignmentId: string, attachmentId: string) =>
    downloadFile(`/trainings/${trainingId}/assignments/${assignmentId}/attachments/${attachmentId}/download`),
};
