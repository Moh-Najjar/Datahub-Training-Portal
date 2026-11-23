import { RoleNames } from "../types";

export function canManageDepartments(role: string): boolean {
  return role === RoleNames.SuperAdmin;
}

export function canManageUsers(role: string): boolean {
  return role === RoleNames.SuperAdmin || role === RoleNames.DepartmentAdmin;
}

export function canManageEnrollments(role: string): boolean {
  return role === RoleNames.SuperAdmin || role === RoleNames.DepartmentAdmin;
}

export function canAuthorTrainings(role: string): boolean {
  return role === RoleNames.SuperAdmin || role === RoleNames.DepartmentAdmin;
}

export function canManageEvaluations(role: string): boolean {
  return role === RoleNames.SuperAdmin || role === RoleNames.DepartmentAdmin;
}
