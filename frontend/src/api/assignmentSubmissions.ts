import { apiClient, downloadFile } from "./client";
import type { AssignmentSubmissionDto, AssignmentSubmissionOverviewDto } from "../types";

export const assignmentSubmissionsApi = {
  getMine: (trainingId: string, assignmentId: string) =>
    apiClient.get<AssignmentSubmissionDto | null>(
      `/trainings/${trainingId}/assignments/${assignmentId}/submission`
    ),
  getAllForAssignment: (trainingId: string, assignmentId: string) =>
    apiClient.get<AssignmentSubmissionOverviewDto[]>(
      `/trainings/${trainingId}/assignments/${assignmentId}/submissions`
    ),
  upsertMine: (trainingId: string, assignmentId: string, textResponse: string, files: File[]) => {
    const form = new FormData();
    form.append("textResponse", textResponse);
    files.forEach((file) => form.append("attachments", file));
    return apiClient.putForm<AssignmentSubmissionDto>(
      `/trainings/${trainingId}/assignments/${assignmentId}/submission`,
      form
    );
  },
  deleteAttachment: (trainingId: string, assignmentId: string, attachmentId: string) =>
    apiClient.delete<void>(
      `/trainings/${trainingId}/assignments/${assignmentId}/submission/attachments/${attachmentId}`
    ),
  downloadAttachment: (trainingId: string, assignmentId: string, attachmentId: string) =>
    downloadFile(
      `/trainings/${trainingId}/assignments/${assignmentId}/submission/attachments/${attachmentId}/download`
    ),
};
