export const RoleNames = {
  SuperAdmin: "SuperAdmin",
  DepartmentAdmin: "DepartmentAdmin",
  Trainee: "Trainee",
  Employee: "Employee",
  Intern: "Intern",
} as const;

export type Role = (typeof RoleNames)[keyof typeof RoleNames];

export const AllRoles: Role[] = Object.values(RoleNames);

export const TrainingStatus = {
  Draft: 0,
  Published: 1,
  Archived: 2,
} as const;

export type TrainingStatus = (typeof TrainingStatus)[keyof typeof TrainingStatus];

export const TrainingStatusLabels: Record<TrainingStatus, string> = {
  [TrainingStatus.Draft]: "Draft",
  [TrainingStatus.Published]: "Published",
  [TrainingStatus.Archived]: "Archived",
};

export const EnrollmentStatus = {
  Assigned: 0,
  InProgress: 1,
  Completed: 2,
} as const;

export type EnrollmentStatus = (typeof EnrollmentStatus)[keyof typeof EnrollmentStatus];

export const EnrollmentStatusLabels: Record<EnrollmentStatus, string> = {
  [EnrollmentStatus.Assigned]: "Assigned",
  [EnrollmentStatus.InProgress]: "In Progress",
  [EnrollmentStatus.Completed]: "Completed",
};

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  userId: string;
  fullName: string;
  email: string;
  role: string;
  departmentId: string | null;
}

export interface DepartmentDto {
  id: string;
  name: string;
  userCount: number;
  trainingCount: number;
}

export interface CreateDepartmentRequest {
  name: string;
}

export interface UpdateDepartmentRequest {
  name: string;
}

export interface UserDto {
  id: string;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  departmentId: string | null;
  departmentName: string | null;
  createdAt: string;
}

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
  role: string;
  departmentId: string | null;
}

export interface UpdateUserRequest {
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  departmentId: string | null;
}

export interface TrainingDto {
  id: string;
  title: string;
  description: string;
  status: TrainingStatus;
  sortOrder: number;
  departmentId: string;
  departmentName: string | null;
  trainerId: string | null;
  trainerName: string | null;
  materialCount: number;
  enrollmentCount: number;
  createdAt: string;
}

export interface CreateTrainingRequest {
  title: string;
  description: string;
  departmentId: string;
}

export interface UpdateTrainingRequest {
  title: string;
  description: string;
  status: TrainingStatus;
}

export interface ReorderTrainingsRequest {
  orderedIds: string[];
}

export const MaterialType = {
  Text: 0,
  Link: 1,
  File: 2,
} as const;

export type MaterialType = (typeof MaterialType)[keyof typeof MaterialType];

export interface MaterialFileItemDto {
  id: string;
  title: string | null;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
}

export interface MaterialLinkItemDto {
  id: string;
  title: string | null;
  url: string;
}

export interface TrainingMaterialDto {
  id: string;
  trainingId: string;
  type: MaterialType;
  title: string;
  sortOrder: number;
  textContent: string | null;
  fileItems: MaterialFileItemDto[];
  linkItems: MaterialLinkItemDto[];
  uploadedByUserId: string;
  uploadedByUserName: string | null;
  createdAt: string;
}

export interface CreateTextMaterialRequest {
  title: string;
  content: string;
}

export interface LinkItemInput {
  title?: string;
  url: string;
}

export interface CreateLinkMaterialRequest {
  title: string;
  links: LinkItemInput[];
}

export interface UpdateFileMaterialRequest {
  title: string;
}

export interface ReorderMaterialsRequest {
  orderedIds: string[];
}

export const AssignmentStatus = {
  Upcoming: 0,
  Open: 1,
  Closed: 2,
} as const;

export type AssignmentStatus = (typeof AssignmentStatus)[keyof typeof AssignmentStatus];

export const AssignmentStatusLabels: Record<AssignmentStatus, string> = {
  [AssignmentStatus.Upcoming]: "Upcoming",
  [AssignmentStatus.Open]: "Open",
  [AssignmentStatus.Closed]: "Closed",
};

export interface AssignmentAttachmentDto {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
}

export interface AssignmentDto {
  id: string;
  trainingId: string;
  title: string;
  description: string;
  opensAt: string;
  closesAt: string;
  status: AssignmentStatus;
  createdByUserId: string;
  createdByUserName: string | null;
  createdAt: string;
  attachments: AssignmentAttachmentDto[];
}

export interface UpdateAssignmentRequest {
  title: string;
  description: string;
  opensAt: string;
  closesAt: string;
}

export interface AssignmentSubmissionDto {
  id: string;
  assignmentId: string;
  userId: string;
  textResponse: string;
  submittedAt: string;
  updatedAt: string | null;
  attachments: AssignmentAttachmentDto[];
}

export interface AssignmentSubmissionOverviewDto {
  userId: string;
  userName: string;
  submission: AssignmentSubmissionDto | null;
}

export interface EvaluationCategoryScoreDto {
  id: string;
  categoryName: string;
  criteria: string;
  score: number;
}

export interface EvaluationDto {
  id: string;
  userId: string;
  userName: string;
  evaluatedByUserId: string;
  evaluatedByUserName: string | null;
  enrollmentId: string | null;
  trainingId: string | null;
  trainingTitle: string | null;
  departmentName: string | null;
  evaluatedAt: string;
  total: number;
  createdAt: string;
  updatedAt: string | null;
  categories: EvaluationCategoryScoreDto[];
}

export interface EvaluationCategoryScoreInput {
  categoryName: string;
  criteria: string;
  score: number;
}

export interface CreateEvaluationRequest {
  enrollmentId?: string | null;
  evaluatedAt: string;
  categories: EvaluationCategoryScoreInput[];
}

export interface UpdateEvaluationRequest {
  evaluatedAt: string;
  categories: EvaluationCategoryScoreInput[];
}

export interface RankingEntryDto {
  userId: string;
  userName: string;
  role: string;
  departmentName: string | null;
  latestTotal: number | null;
  latestMaxTotal: number | null;
  latestEvaluatedAt: string | null;
  evaluationCount: number;
}

export interface EnrollmentDto {
  id: string;
  trainingId: string;
  trainingTitle: string | null;
  userId: string;
  userName: string | null;
  assignedByUserId: string;
  assignedByUserName: string | null;
  status: EnrollmentStatus;
  assignedAt: string;
  completedAt: string | null;
}

export interface CreateEnrollmentRequest {
  trainingId: string;
  userId: string;
}

export interface UpdateEnrollmentStatusRequest {
  status: EnrollmentStatus;
}

export interface ProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
}
