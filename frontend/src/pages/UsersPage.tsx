import { useEffect, useState, type FormEvent } from "react";
import { usersApi } from "../api/users";
import { departmentsApi } from "../api/departments";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { useAuth } from "../context/AuthContext";
import { canManageUsers } from "../utils/permissions";
import { AllRoles, RoleNames, type DepartmentDto, type UserDto } from "../types";

const departmentAdminRoles: string[] = [RoleNames.Intern, RoleNames.Trainee, RoleNames.Employee];

const emptyCreateForm = { fullName: "", email: "", password: "", role: "", departmentId: "" };

export function UsersPage() {
  const { user } = useAuth();
  const canManage = canManageUsers(user!.role);
  const isDepartmentAdmin = user!.role === RoleNames.DepartmentAdmin;
  const isScopedToOwnDepartment = isDepartmentAdmin;
  const assignableRoles = isDepartmentAdmin ? departmentAdminRoles : AllRoles;

  const [users, setUsers] = useState<UserDto[]>([]);
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [createForm, setCreateForm] = useState({ ...emptyCreateForm, role: assignableRoles[0] as string });
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editForm, setEditForm] = useState({ fullName: "", email: "", role: "", isActive: true, departmentId: "" });

  const load = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const [usersResult, departmentsResult] = await Promise.all([
        usersApi.getAll(),
        departmentsApi.getAll(),
      ]);
      setUsers(usersResult);
      setDepartments(departmentsResult);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load users.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleCreate = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    try {
      await usersApi.create({
        fullName: createForm.fullName,
        email: createForm.email,
        password: createForm.password,
        role: createForm.role,
        departmentId: isScopedToOwnDepartment ? null : createForm.departmentId || null,
      });
      setCreateForm({ ...emptyCreateForm, role: assignableRoles[0] as string });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create user.");
    }
  };

  const startEdit = (u: UserDto) => {
    setEditingId(u.id);
    setEditForm({
      fullName: u.fullName,
      email: u.email,
      role: u.role,
      isActive: u.isActive,
      departmentId: u.departmentId ?? "",
    });
  };

  const handleUpdate = async (event: FormEvent) => {
    event.preventDefault();
    if (!editingId) return;
    setError(null);
    try {
      await usersApi.update(editingId, {
        fullName: editForm.fullName,
        email: editForm.email,
        role: editForm.role,
        isActive: editForm.isActive,
        departmentId: isScopedToOwnDepartment ? null : editForm.departmentId || null,
      });
      setEditingId(null);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update user.");
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this user?")) return;
    setError(null);
    try {
      await usersApi.delete(id);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete user.");
    }
  };

  return (
    <div>
      <div className="page-header">
        <h1>Users</h1>
      </div>

      <ErrorBanner message={error} />

      {canManage && (
        <form className="card form-grid" onSubmit={handleCreate}>
          <label>
            Full name
            <input
              value={createForm.fullName}
              onChange={(e) => setCreateForm({ ...createForm, fullName: e.target.value })}
              required
            />
          </label>
          <label>
            Email
            <input
              type="email"
              value={createForm.email}
              onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })}
              required
            />
          </label>
          <label>
            Password
            <input
              type="password"
              value={createForm.password}
              onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })}
              minLength={8}
              required
            />
          </label>
          <label>
            Role
            <select
              value={createForm.role}
              onChange={(e) => setCreateForm({ ...createForm, role: e.target.value })}
            >
              {assignableRoles.map((role) => (
                <option key={role} value={role}>
                  {role}
                </option>
              ))}
            </select>
          </label>
          {!isScopedToOwnDepartment && (
            <label>
              Department
              <select
                value={createForm.departmentId}
                onChange={(e) => setCreateForm({ ...createForm, departmentId: e.target.value })}
              >
                <option value="">None</option>
                {departments.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.name}
                  </option>
                ))}
              </select>
            </label>
          )}
          <button type="submit">Add user</button>
        </form>
      )}

      {isLoading ? (
        <p>Loading...</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Department</th>
              <th>Active</th>
              {canManage && <th></th>}
            </tr>
          </thead>
          <tbody>
            {users.map((u) =>
              editingId === u.id ? (
                <tr key={u.id}>
                  <td colSpan={6}>
                    <form className="card user-edit-form" onSubmit={handleUpdate}>
                      <div className="form-grid">
                        <label>
                          Name
                          <input
                            value={editForm.fullName}
                            onChange={(e) => setEditForm({ ...editForm, fullName: e.target.value })}
                            required
                          />
                        </label>
                        <label>
                          Email
                          <input
                            type="email"
                            value={editForm.email}
                            onChange={(e) => setEditForm({ ...editForm, email: e.target.value })}
                            required
                          />
                        </label>
                        <label>
                          Role
                          <select
                            value={editForm.role}
                            onChange={(e) => setEditForm({ ...editForm, role: e.target.value })}
                          >
                            {assignableRoles.map((role) => (
                              <option key={role} value={role}>
                                {role}
                              </option>
                            ))}
                          </select>
                        </label>
                        {!isScopedToOwnDepartment && (
                          <label>
                            Department
                            <select
                              value={editForm.departmentId}
                              onChange={(e) => setEditForm({ ...editForm, departmentId: e.target.value })}
                            >
                              <option value="">None</option>
                              {departments.map((d) => (
                                <option key={d.id} value={d.id}>
                                  {d.name}
                                </option>
                              ))}
                            </select>
                          </label>
                        )}
                      </div>

                      <label className="checkbox-field">
                        <input
                          type="checkbox"
                          checked={editForm.isActive}
                          onChange={(e) => setEditForm({ ...editForm, isActive: e.target.checked })}
                        />
                        Active
                      </label>

                      <div className="row-actions">
                        <button type="submit">Save</button>
                        <button type="button" className="secondary" onClick={() => setEditingId(null)}>
                          Cancel
                        </button>
                      </div>
                    </form>
                  </td>
                </tr>
              ) : (
                <tr key={u.id}>
                  <td>{u.fullName}</td>
                  <td>{u.email}</td>
                  <td>{u.role}</td>
                  <td>{u.departmentName ?? "-"}</td>
                  <td>{u.isActive ? "Yes" : "No"}</td>
                  {canManage && (
                    <td>
                      <div className="row-actions">
                        <button className="secondary" onClick={() => startEdit(u)}>
                          Edit
                        </button>
                        <button className="danger" onClick={() => handleDelete(u.id)}>
                          Delete
                        </button>
                      </div>
                    </td>
                  )}
                </tr>
              ),
            )}
            {users.length === 0 && (
              <tr>
                <td colSpan={6} className="muted">
                  No users yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
