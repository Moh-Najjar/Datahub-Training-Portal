import { useEffect, useRef, useState, type DragEvent, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { trainingsApi } from "../api/trainings";
import { departmentsApi } from "../api/departments";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { useAuth } from "../context/AuthContext";
import { canAuthorTrainings } from "../utils/permissions";
import { formatDate, formatTrainingStatus } from "../utils/format";
import type { DepartmentDto, TrainingDto } from "../types";

const emptyForm = { title: "", description: "", departmentId: "" };

export function TrainingsPage() {
  const { user } = useAuth();
  const canAuthor = canAuthorTrainings(user!.role);

  const [trainings, setTrainings] = useState<TrainingDto[]>([]);
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState(emptyForm);
  const draggedId = useRef<string | null>(null);
  const [isSavingOrder, setIsSavingOrder] = useState(false);

  const load = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const trainingsResult = await trainingsApi.getAll();
      setTrainings(trainingsResult);

      if (canAuthor) {
        setDepartments(await departmentsApi.getAll());
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load trainings.");
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
      await trainingsApi.create(form);
      setForm(emptyForm);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create training.");
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this training?")) return;
    setError(null);
    try {
      await trainingsApi.delete(id);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete training.");
    }
  };

  const handleDragStart = (id: string) => {
    draggedId.current = id;
  };

  const handleDragOver = (event: DragEvent<HTMLTableRowElement>, overId: string) => {
    event.preventDefault();
    const sourceId = draggedId.current;
    if (!sourceId || sourceId === overId) return;

    setTrainings((current) => {
      const sourceIndex = current.findIndex((t) => t.id === sourceId);
      const targetIndex = current.findIndex((t) => t.id === overId);
      if (sourceIndex === -1 || targetIndex === -1) return current;

      const next = [...current];
      const [moved] = next.splice(sourceIndex, 1);
      next.splice(targetIndex, 0, moved);
      return next;
    });
  };

  const handleDrop = async () => {
    draggedId.current = null;
    setIsSavingOrder(true);
    setError(null);
    try {
      await trainingsApi.reorder({ orderedIds: trainings.map((t) => t.id) });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save the new order.");
      await load();
    } finally {
      setIsSavingOrder(false);
    }
  };

  if (isLoading) {
    return (
      <div>
        <div className="page-header">
          <h1>Trainings</h1>
        </div>
        <p>Loading...</p>
      </div>
    );
  }

  if (!canAuthor) {
    return (
      <div>
        <div className="page-header">
          <h1>My Trainings</h1>
        </div>

        <ErrorBanner message={error} />

        {trainings.length === 0 ? (
          <p className="muted">You have not been assigned any trainings yet.</p>
        ) : (
          <div className="training-grid">
            {trainings.map((t) => (
              <Link to={`/trainings/${t.id}`} key={t.id} className="training-card">
                <div className="training-card-header">
                  <h3>{t.title}</h3>
                  <span className="badge">{formatTrainingStatus(t.status)}</span>
                </div>
                <p className="training-card-description">{t.description}</p>
                <div className="training-card-footer">
                  <span className="muted">{t.departmentName ?? "-"}</span>
                  <span className="muted">{t.materialCount} content item(s)</span>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    );
  }

  return (
    <div>
      <div className="page-header">
        <h1>Trainings</h1>
      </div>

      <ErrorBanner message={error} />

      <form className="card form-grid" onSubmit={handleCreate}>
        <label>
          Title
          <input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} required />
        </label>
        <label>
          Description
          <input
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            required
          />
        </label>
        <label>
          Department
          <select
            value={form.departmentId}
            onChange={(e) => setForm({ ...form, departmentId: e.target.value })}
            required
          >
            <option value="" disabled>
              Select a department
            </option>
            {departments.map((d) => (
              <option key={d.id} value={d.id}>
                {d.name}
              </option>
            ))}
          </select>
        </label>
        <button type="submit">Add training</button>
      </form>

      <p className="muted">Drag rows by the handle to reorder trainings.{isSavingOrder && " Saving order..."}</p>

      <table>
        <thead>
          <tr>
            <th></th>
            <th>Title</th>
            <th>Department</th>
            <th>Trainer</th>
            <th>Status</th>
            <th>Content</th>
            <th>Enrollments</th>
            <th>Created</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {trainings.map((t) => (
            <tr
              key={t.id}
              onDragOver={(e) => handleDragOver(e, t.id)}
              onDrop={handleDrop}
            >
              <td
                className="drag-handle"
                draggable
                onDragStart={() => handleDragStart(t.id)}
                title="Drag to reorder"
              >
                &#8942;&#8942;
              </td>
              <td>
                <Link to={`/trainings/${t.id}`}>{t.title}</Link>
              </td>
              <td>{t.departmentName ?? "-"}</td>
              <td>{t.trainerName ?? "-"}</td>
              <td>
                <span className="badge">{formatTrainingStatus(t.status)}</span>
              </td>
              <td>{t.materialCount}</td>
              <td>{t.enrollmentCount}</td>
              <td>{formatDate(t.createdAt)}</td>
              <td>
                <div className="row-actions">
                  <Link to={`/trainings/${t.id}`}>
                    <button className="secondary">Open</button>
                  </Link>
                  <button className="danger" onClick={() => handleDelete(t.id)}>
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          ))}
          {trainings.length === 0 && (
            <tr>
              <td colSpan={9} className="muted">
                No trainings yet.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
