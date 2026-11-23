import {
  AssignmentStatusLabels,
  EnrollmentStatusLabels,
  TrainingStatusLabels,
  type AssignmentStatus,
  type EnrollmentStatus,
  type TrainingStatus,
} from "../types";

export function formatTrainingStatus(status: TrainingStatus): string {
  return TrainingStatusLabels[status];
}

export function formatEnrollmentStatus(status: EnrollmentStatus): string {
  return EnrollmentStatusLabels[status];
}

export function formatAssignmentStatus(status: AssignmentStatus): string {
  return AssignmentStatusLabels[status];
}

export function formatDate(value: string | null | undefined): string {
  if (!value) return "-";
  return new Date(value).toLocaleDateString();
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) return "-";
  return new Date(value).toLocaleString();
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
