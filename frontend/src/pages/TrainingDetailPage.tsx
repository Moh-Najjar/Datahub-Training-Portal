import { useEffect, useState, type FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { trainingsApi } from "../api/trainings";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { useAuth } from "../context/AuthContext";
import { canAuthorTrainings } from "../utils/permissions";
import { formatTrainingStatus } from "../utils/format";
import { TrainingStatus, type TrainingDto } from "../types";

export function TrainingDetailPage() {
  const { trainingId } = useParams<{ trainingId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const canAuthor = canAuthorTrainings(user!.role);

  const [training, setTraining] = useState<TrainingDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [editForm, setEditForm] = useState<{ title: string; description: string; status: TrainingStatus }>({
    title: "",
    description: "",
    status: TrainingStatus.Draft,
  });

  const load = async () => {
    if (!trainingId) return;
    setIsLoading(true);
    setError(null);
    try {
      const trainingResult = await trainingsApi.getById(trainingId);
      setTraining(trainingResult);
      setEditForm({
        title: trainingResult.title,
        description: trainingResult.description,
        status: trainingResult.status,
      });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load training.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [trainingId]);

  const handleUpdate = async (event: FormEvent) => {
    event.preventDefault();
    if (!trainingId) return;
    setError(null);
    try {
      await trainingsApi.update(trainingId, editForm);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update training.");
    }
  };

  const handleDeleteTraining = async () => {
    if (!trainingId || !confirm("Delete this training?")) return;
    try {
      await trainingsApi.delete(trainingId);
      navigate("/trainings");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete training.");
    }
  };

  if (isLoading) return <p>Loading...</p>;

  return (
    <div>
      <Link to="/trainings" className="back-link">
        &larr; Back to trainings
      </Link>

      <ErrorBanner message={error} />

      {training && (
        <>
          <div className="training-hero">
            <div className="page-header">
              <h1>{training.title}</h1>
              <span className="badge">{formatTrainingStatus(training.status)}</span>
            </div>
            <p className="training-hero-description">{training.description}</p>
          </div>

          {canAuthor && (
            <form className="card training-edit-form" onSubmit={handleUpdate}>
              <label>
                Title
                <input
                  value={editForm.title}
                  onChange={(e) => setEditForm({ ...editForm, title: e.target.value })}
                  required
                />
              </label>
              <label>
                Description
                <textarea
                  value={editForm.description}
                  onChange={(e) => setEditForm({ ...editForm, description: e.target.value })}
                  rows={3}
                  required
                />
              </label>
              <div className="training-edit-row">
                <label className="training-edit-status">
                  Status
                  <select
                    value={editForm.status}
                    onChange={(e) =>
                      setEditForm({ ...editForm, status: Number(e.target.value) as TrainingStatus })
                    }
                  >
                    <option value={TrainingStatus.Draft}>Draft</option>
                    <option value={TrainingStatus.Published}>Published</option>
                    <option value={TrainingStatus.Archived}>Archived</option>
                  </select>
                </label>
                <div className="row-actions">
                  <button type="submit">Save changes</button>
                  <button type="button" className="danger" onClick={handleDeleteTraining}>
                    Delete training
                  </button>
                </div>
              </div>
            </form>
          )}

          <div className="training-nav-grid">
            <Link to={`/trainings/${trainingId}/content`} className="training-nav-card">
              <span className="training-nav-icon">📚</span>
              <div>
                <h3>Content</h3>
                <p className="muted">{training.materialCount} item(s) &middot; files, text, and links</p>
              </div>
              <span className="training-nav-arrow">&rarr;</span>
            </Link>

            <Link to={`/trainings/${trainingId}/assignments`} className="training-nav-card">
              <span className="training-nav-icon">📝</span>
              <div>
                <h3>Assignments</h3>
                <p className="muted">Deadlines and submissions</p>
              </div>
              <span className="training-nav-arrow">&rarr;</span>
            </Link>
          </div>
        </>
      )}
    </div>
  );
}
