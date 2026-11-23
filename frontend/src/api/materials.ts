import { apiClient, downloadFile } from "./client";
import type {
  CreateLinkMaterialRequest,
  CreateTextMaterialRequest,
  ReorderMaterialsRequest,
  TrainingMaterialDto,
  UpdateFileMaterialRequest,
} from "../types";

export interface FileGroupItemInput {
  file: File;
  title?: string;
}

function buildFileItemsForm(items: FileGroupItemInput[]): FormData {
  const form = new FormData();
  items.forEach((item) => {
    form.append("Files", item.file);
    form.append("FileTitles", item.title ?? "");
  });
  return form;
}

export const materialsApi = {
  getForTraining: (trainingId: string) =>
    apiClient.get<TrainingMaterialDto[]>(`/trainings/${trainingId}/materials`),
  addFileGroup: (trainingId: string, title: string | undefined, items: FileGroupItemInput[]) => {
    const form = buildFileItemsForm(items);
    if (title) form.append("title", title);
    return apiClient.postForm<TrainingMaterialDto>(`/trainings/${trainingId}/materials/file`, form);
  },
  addFileItems: (trainingId: string, materialId: string, items: FileGroupItemInput[]) =>
    apiClient.postForm<TrainingMaterialDto>(
      `/trainings/${trainingId}/materials/${materialId}/files`,
      buildFileItemsForm(items)
    ),
  updateFileItemTitle: (trainingId: string, materialId: string, fileItemId: string, title: string | undefined) =>
    apiClient.put<TrainingMaterialDto>(
      `/trainings/${trainingId}/materials/${materialId}/files/${fileItemId}`,
      { title }
    ),
  deleteFileItem: (trainingId: string, materialId: string, fileItemId: string) =>
    apiClient.delete<void>(`/trainings/${trainingId}/materials/${materialId}/files/${fileItemId}`),
  downloadFileItem: (trainingId: string, materialId: string, fileItemId: string) =>
    downloadFile(`/trainings/${trainingId}/materials/${materialId}/files/${fileItemId}/download`),
  addText: (trainingId: string, request: CreateTextMaterialRequest) =>
    apiClient.post<TrainingMaterialDto>(`/trainings/${trainingId}/materials/text`, request),
  addLink: (trainingId: string, request: CreateLinkMaterialRequest) =>
    apiClient.post<TrainingMaterialDto>(`/trainings/${trainingId}/materials/link`, request),
  updateText: (trainingId: string, materialId: string, request: CreateTextMaterialRequest) =>
    apiClient.put<TrainingMaterialDto>(`/trainings/${trainingId}/materials/${materialId}/text`, request),
  updateLink: (trainingId: string, materialId: string, request: CreateLinkMaterialRequest) =>
    apiClient.put<TrainingMaterialDto>(`/trainings/${trainingId}/materials/${materialId}/link`, request),
  updateFileTitle: (trainingId: string, materialId: string, request: UpdateFileMaterialRequest) =>
    apiClient.put<TrainingMaterialDto>(`/trainings/${trainingId}/materials/${materialId}/file`, request),
  delete: (trainingId: string, materialId: string) =>
    apiClient.delete<void>(`/trainings/${trainingId}/materials/${materialId}`),
  reorder: (trainingId: string, request: ReorderMaterialsRequest) =>
    apiClient.put<void>(`/trainings/${trainingId}/materials/reorder`, request),
};
