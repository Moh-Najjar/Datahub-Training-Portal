import { useEffect, useState, type FormEvent } from "react";
import { departmentsApi } from "../api/departments";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { useAuth } from "../context/AuthContext";
import { canManageDepartments } from "../utils/permissions";
import type { DepartmentDto } from "../types";

export function DepartmentsPage() {
  const { user } = useAuth();
  const canManage = canManageDepartments(user!.role);

  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [newName, setNewName] = useState("");
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editingName, setEditingName] = useState("");

  const load = async () => {
    setIsLoading(true);
    setError(null);
    try {
      setDepartments(await departmentsApi.getAll());
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load departments.");
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
      await departmentsApi.create({ name: newName });
      setNewName("");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create department.");
    }
  };

  const startEdit = (department: DepartmentDto) => {
    setEditingId(department.id);
    setEditingName(department.name);
  };

  const handleUpdate = async (event: FormEvent) => {
    event.preventDefault();
    if (!editingId) return;
    setError(null);
    try {
      await departmentsApi.update(editingId, { name: editingName });
      setEditingId(null);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update department.");
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this department?")) return;
    setError(null);
    try {
      await departmentsApi.delete(id);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete department.");
    }
  };

  return (
    <div>
      <div className="page-header">
        <h1>Departments</h1>
      </div>

      <ErrorBanner message={error} />

      {canManage && (
        <form className="card form-grid" onSubmit={handleCreate}>
          <label>
            New department name
            <input value={newName} onChange={(e) => setNewName(e.target.value)} required />
          </label>
          <button type="submit">Add department</button>
        </form>
      )}

      {isLoading ? (
        <p>Loading...</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Users</th>
              <th>Trainings</th>
              {canManage && <th></th>}
            </tr>
          </thead>
          <tbody>
            {departments.map((d) =>
              editingId === d.id ? (
                <tr key={d.id}>
                  <td colSpan={4}>
                    <form className="form-grid" onSubmit={handleUpdate}>
                      <label>
                        Name
                        <input
                          value={editingName}
                          onChange={(e) => setEditingName(e.target.value)}
                          required
                        />
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
                <tr key={d.id}>
                  <td>{d.name}</td>
                  <td>{d.userCount}</td>
                  <td>{d.trainingCount}</td>
                  {canManage && (
                    <td>
                      <div className="row-actions">
                        <button className="secondary" onClick={() => startEdit(d)}>
                          Edit
                        </button>
                        <button className="danger" onClick={() => handleDelete(d.id)}>
                          Delete
                        </button>
                      </div>
                    </td>
                  )}
                </tr>
              ),
            )}
            {departments.length === 0 && (
              <tr>
                <td colSpan={4} className="muted">
                  No departments yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
