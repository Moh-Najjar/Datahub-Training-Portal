import { useEffect, useState, type FormEvent } from "react";
import { enrollmentsApi } from "../api/enrollments";
import { trainingsApi } from "../api/trainings";
import { usersApi } from "../api/users";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { useAuth } from "../context/AuthContext";
import { canManageEnrollments } from "../utils/permissions";
import { formatDate, formatEnrollmentStatus } from "../utils/format";
import { EnrollmentStatus, RoleNames, type EnrollmentDto, type TrainingDto, type UserDto } from "../types";

const statusOptions = [EnrollmentStatus.Assigned, EnrollmentStatus.InProgress, EnrollmentStatus.Completed];
const assignableRoles: string[] = [RoleNames.Employee, RoleNames.Trainee, RoleNames.Intern];

export function EnrollmentsPage() {
  const { user } = useAuth();
  const canManage = canManageEnrollments(user!.role);

  const [enrollments, setEnrollments] = useState<EnrollmentDto[]>([]);
  const [trainings, setTrainings] = useState<TrainingDto[]>([]);
  const [users, setUsers] = useState<UserDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ trainingId: "", userId: "" });

  const load = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const [enrollmentsResult, trainingsResult, usersResult] = await Promise.all([
        enrollmentsApi.getAll(),
        trainingsApi.getAll(),
        usersApi.getAssignable(),
      ]);
      setEnrollments(enrollmentsResult);
      setTrainings(trainingsResult);
      setUsers(usersResult.filter((u) => assignableRoles.includes(u.role)));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load enrollments.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleCreate = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    try {
      await enrollmentsApi.create(form);
      setForm({ trainingId: "", userId: "" });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create enrollment.");
    }
  };

  const handleStatusChange = async (enrollment: EnrollmentDto, status: EnrollmentStatus) => {
    setError(null);
    try {
      await enrollmentsApi.updateStatus(enrollment.id, { status });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update status.");
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this enrollment?")) return;
    setError(null);
    try {
      await enrollmentsApi.delete(id);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete enrollment.");
    }
  };

  if (!canManage) {
    return (
      <div>
        <div className="page-header">
          <h1>Enrollments</h1>
        </div>
        <p className="muted">Only SuperAdmin and DepartmentAdmin can manage training enrollments.</p>
      </div>
    );
  }

  return (
    <div>
      <div className="page-header">
        <h1>Enrollments</h1>
      </div>

      <ErrorBanner message={error} />

      {canManage && (
        <form className="card form-grid" onSubmit={handleCreate}>
          <label>
            Training
            <select
              value={form.trainingId}
              onChange={(e) => setForm({ ...form, trainingId: e.target.value })}
              required
            >
              <option value="" disabled>
                Select a training
              </option>
              {trainings.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.title}
                </option>
              ))}
            </select>
          </label>
          <label>
            User
            <select
              value={form.userId}
              onChange={(e) => setForm({ ...form, userId: e.target.value })}
              required
            >
              <option value="" disabled>
                Select a user
              </option>
              {users.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.fullName} {u.departmentName ? `(${u.departmentName})` : ""}
                </option>
              ))}
            </select>
          </label>
          <button type="submit">Assign training</button>
        </form>
      )}

      {isLoading ? (
        <p>Loading...</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Training</th>
              <th>User</th>
              <th>Assigned by</th>
              <th>Status</th>
              <th>Assigned</th>
              <th>Completed</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {enrollments.map((e) => (
              <tr key={e.id}>
                <td>{e.trainingTitle ?? "-"}</td>
                <td>{e.userName ?? "-"}</td>
                <td>{e.assignedByUserName ?? "-"}</td>
                <td>
                  <select
                    value={e.status}
                    onChange={(ev) => handleStatusChange(e, Number(ev.target.value) as EnrollmentStatus)}
                  >
                    {statusOptions.map((s) => (
                      <option key={s} value={s}>
                        {formatEnrollmentStatus(s)}
                      </option>
                    ))}
                  </select>
                </td>
                <td>{formatDate(e.assignedAt)}</td>
                <td>{formatDate(e.completedAt)}</td>
                <td>
                  <div className="row-actions">
                    <button className="danger" onClick={() => handleDelete(e.id)}>
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {enrollments.length === 0 && (
              <tr>
                <td colSpan={7} className="muted">
                  No enrollments yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
