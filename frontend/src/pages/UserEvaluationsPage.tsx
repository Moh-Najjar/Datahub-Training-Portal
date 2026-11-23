import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { evaluationsApi } from "../api/evaluations";
import { usersApi } from "../api/users";
import { enrollmentsApi } from "../api/enrollments";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { formatDate, formatDateTime } from "../utils/format";
import type { EnrollmentDto, EvaluationCategoryScoreInput, EvaluationDto, UserDto } from "../types";

const DEFAULT_CATEGORIES: EvaluationCategoryScoreInput[] = [
  { categoryName: "Presentation Skills", criteria: "Clarity of explanations", score: 5 },
  {
    categoryName: "Technical Understanding",
    criteria:
      "Demonstrates understanding of data engineering concepts, architecture, SQL, ETL, Python, data modeling.",
    score: 5,
  },
  {
    categoryName: "Ownership & Accountability",
    criteria: "Takes responsibility for assigned tasks; follows through to completion; communicates blockers promptly",
    score: 5,
  },
  { categoryName: "Time Management", criteria: "Delivers demos within allocated time and meets deadlines.", score: 5 },
  { categoryName: "English Fluency (Verbal)", criteria: "Smooth speech and good vocabulary.", score: 5 },
  {
    categoryName: "AI Usage Ethics",
    criteria: "Uses AI to accelerate work, not replace thinking; explains AI-generated code; debugs manually",
    score: 5,
  },
  {
    categoryName: "Receptiveness to Feedback",
    criteria: "Accepts criticism without defensiveness; applies feedback consistently in future work.",
    score: 5,
  },
  { categoryName: "Going Beyond Expectations", criteria: "Delivers bonus work unasked", score: 5 },
  { categoryName: "Documentation Quality", criteria: "Produces clear technical documentation/Solution", score: 5 },
  {
    categoryName: "Curiosity & Continuous Learning",
    criteria: "Actively explores topics beyond assigned work; asks insightful questions;",
    score: 5,
  },
];

function toDateInputValue(iso: string): string {
  return iso.slice(0, 10);
}

function todayDateInputValue(): string {
  return new Date().toISOString().slice(0, 10);
}

interface CategoryEditorTableProps {
  categories: EvaluationCategoryScoreInput[];
  onUpdate: (index: number, patch: Partial<EvaluationCategoryScoreInput>) => void;
  onRemove: (index: number) => void;
}

function CategoryEditorTable({ categories, onUpdate, onRemove }: CategoryEditorTableProps) {
  return (
    <div className="evaluation-table-wrap">
      <table className="evaluation-table">
        <thead>
          <tr>
            <th>Category</th>
            <th>Evaluation criteria</th>
            <th>Score (1-10)</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {categories.map((c, index) => (
            <tr key={index}>
              <td>
                <input
                  value={c.categoryName}
                  onChange={(e) => onUpdate(index, { categoryName: e.target.value })}
                  placeholder="Category name"
                  required
                />
              </td>
              <td>
                <input
                  value={c.criteria}
                  onChange={(e) => onUpdate(index, { criteria: e.target.value })}
                  placeholder="Evaluation criteria"
                />
              </td>
              <td>
                <input
                  type="number"
                  min={1}
                  max={10}
                  value={c.score}
                  onChange={(e) => onUpdate(index, { score: Number(e.target.value) })}
                  required
                />
              </td>
              <td>
                <button type="button" className="link-button link-button-danger" onClick={() => onRemove(index)}>
                  Remove
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function UserEvaluationsPage() {
  const { userId } = useParams<{ userId: string }>();

  const [targetUser, setTargetUser] = useState<UserDto | null>(null);
  const [evaluations, setEvaluations] = useState<EvaluationDto[]>([]);
  const [enrollments, setEnrollments] = useState<EnrollmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [isAddFormOpen, setIsAddFormOpen] = useState(false);
  const [evaluatedAt, setEvaluatedAt] = useState(todayDateInputValue());
  const [enrollmentId, setEnrollmentId] = useState("");
  const [categories, setCategories] = useState<EvaluationCategoryScoreInput[]>(DEFAULT_CATEGORIES);
  const [isSaving, setIsSaving] = useState(false);

  const load = async () => {
    if (!userId) return;
    setIsLoading(true);
    setError(null);
    try {
      const [userResult, evaluationsResult, enrollmentsResult] = await Promise.all([
        usersApi.getById(userId),
        evaluationsApi.getForUser(userId),
        enrollmentsApi.getAll({ userId }),
      ]);
      setTargetUser(userResult);
      setEvaluations(evaluationsResult);
      setEnrollments(enrollmentsResult);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load evaluations.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userId]);

  const openAddForm = () => {
    setEvaluatedAt(todayDateInputValue());
    setEnrollmentId("");
    setCategories(
      evaluations.length > 0
        ? evaluations[0].categories.map((c) => ({ categoryName: c.categoryName, criteria: c.criteria, score: 5 }))
        : DEFAULT_CATEGORIES
    );
    setIsAddFormOpen(true);
  };

  const updateCategory = (index: number, patch: Partial<EvaluationCategoryScoreInput>) => {
    setCategories((current) => current.map((c, i) => (i === index ? { ...c, ...patch } : c)));
  };

  const addCategory = () => {
    setCategories((current) => [...current, { categoryName: "", criteria: "", score: 5 }]);
  };

  const removeCategory = (index: number) => {
    setCategories((current) => current.filter((_, i) => i !== index));
  };

  const handleCreate = async () => {
    if (!userId) return;
    if (categories.length === 0) {
      setError("Add at least one category.");
      return;
    }

    setIsSaving(true);
    setError(null);
    try {
      await evaluationsApi.create(userId, {
        enrollmentId: enrollmentId || undefined,
        evaluatedAt: new Date(evaluatedAt).toISOString(),
        categories,
      });
      setIsAddFormOpen(false);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save evaluation.");
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (evaluationId: string) => {
    if (!userId || !confirm("Delete this evaluation?")) return;
    setError(null);
    try {
      await evaluationsApi.delete(userId, evaluationId);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete evaluation.");
    }
  };

  const addFormTotal = categories.reduce((sum, c) => sum + (Number.isFinite(c.score) ? c.score : 0), 0);

  return (
    <div>
      <Link to="/rankings" className="back-link">
        &larr; Back to rankings
      </Link>

      <ErrorBanner message={error} />

      {isLoading ? (
        <p>Loading...</p>
      ) : (
        <>
          <div className="page-header">
            <h1>{targetUser?.fullName ?? "Evaluations"}</h1>
            {!isAddFormOpen && <button onClick={openAddForm}>Add evaluation</button>}
          </div>

          {isAddFormOpen && (
            <div className="card evaluation-form">
              <label>
                Evaluated at
                <input type="date" value={evaluatedAt} onChange={(e) => setEvaluatedAt(e.target.value)} required />
              </label>

              {enrollments.length > 0 && (
                <label>
                  Training
                  <select value={enrollmentId} onChange={(e) => setEnrollmentId(e.target.value)}>
                    <option value="">General (home department)</option>
                    {enrollments.map((en) => (
                      <option key={en.id} value={en.id}>
                        {en.trainingTitle ?? "Untitled training"}
                      </option>
                    ))}
                  </select>
                </label>
              )}

              <CategoryEditorTable categories={categories} onUpdate={updateCategory} onRemove={removeCategory} />

              <div className="row-actions">
                <button type="button" className="secondary" onClick={addCategory}>
                  Add category
                </button>
              </div>

              <p className="evaluation-total">
                Total: {addFormTotal} of {categories.length * 10}
              </p>

              <div className="row-actions">
                <button onClick={handleCreate} disabled={isSaving}>
                  {isSaving ? "Saving..." : "Save evaluation"}
                </button>
                <button type="button" className="secondary" onClick={() => setIsAddFormOpen(false)}>
                  Cancel
                </button>
              </div>
            </div>
          )}

          {evaluations.length === 0 ? (
            <p className="muted">No evaluations yet.</p>
          ) : (
            <div className="evaluation-history">
              {evaluations.map((evaluation) => (
                <EvaluationCard
                  key={evaluation.id}
                  evaluation={evaluation}
                  userId={userId!}
                  onChanged={load}
                  onDelete={() => handleDelete(evaluation.id)}
                  onError={setError}
                />
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}

interface EvaluationCardProps {
  evaluation: EvaluationDto;
  userId: string;
  onChanged: () => void | Promise<void>;
  onDelete: () => void;
  onError: (message: string) => void;
}

function EvaluationCard({ evaluation, userId, onChanged, onDelete, onError }: EvaluationCardProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [evaluatedAt, setEvaluatedAt] = useState(() => toDateInputValue(evaluation.evaluatedAt));
  const [categories, setCategories] = useState<EvaluationCategoryScoreInput[]>(() =>
    evaluation.categories.map((c) => ({ categoryName: c.categoryName, criteria: c.criteria, score: c.score }))
  );
  const [isSaving, setIsSaving] = useState(false);

  const startEdit = () => {
    setEvaluatedAt(toDateInputValue(evaluation.evaluatedAt));
    setCategories(
      evaluation.categories.map((c) => ({ categoryName: c.categoryName, criteria: c.criteria, score: c.score }))
    );
    setIsEditing(true);
  };

  const updateCategory = (index: number, patch: Partial<EvaluationCategoryScoreInput>) => {
    setCategories((current) => current.map((c, i) => (i === index ? { ...c, ...patch } : c)));
  };

  const addCategory = () => {
    setCategories((current) => [...current, { categoryName: "", criteria: "", score: 5 }]);
  };

  const removeCategory = (index: number) => {
    setCategories((current) => current.filter((_, i) => i !== index));
  };

  const handleSave = async () => {
    if (categories.length === 0) {
      onError("Add at least one category.");
      return;
    }

    setIsSaving(true);
    try {
      await evaluationsApi.update(userId, evaluation.id, {
        evaluatedAt: new Date(evaluatedAt).toISOString(),
        categories,
      });
      setIsEditing(false);
      await onChanged();
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to update evaluation.");
    } finally {
      setIsSaving(false);
    }
  };

  const total = categories.reduce((sum, c) => sum + (Number.isFinite(c.score) ? c.score : 0), 0);

  if (isEditing) {
    return (
      <div className="card evaluation-form">
        <label>
          Evaluated at
          <input type="date" value={evaluatedAt} onChange={(e) => setEvaluatedAt(e.target.value)} required />
        </label>

        <CategoryEditorTable categories={categories} onUpdate={updateCategory} onRemove={removeCategory} />

        <div className="row-actions">
          <button type="button" className="secondary" onClick={addCategory}>
            Add category
          </button>
        </div>

        <p className="evaluation-total">
          Total: {total} of {categories.length * 10}
        </p>

        <div className="row-actions">
          <button onClick={handleSave} disabled={isSaving}>
            {isSaving ? "Saving..." : "Save evaluation"}
          </button>
          <button type="button" className="secondary" onClick={() => setIsEditing(false)}>
            Cancel
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="card">
      <div className="content-card-header">
        <span className="content-card-title">Evaluated {formatDate(evaluation.evaluatedAt)}</span>
        <span className="badge">
          Total: {evaluation.total} of {evaluation.categories.length * 10}
        </span>
      </div>
      <p className="muted">
        By {evaluation.evaluatedByUserName ?? "-"}
        {evaluation.trainingTitle && ` · ${evaluation.trainingTitle}`}
        {evaluation.departmentName && ` (${evaluation.departmentName})`}
        {evaluation.updatedAt && ` · updated ${formatDateTime(evaluation.updatedAt)}`}
      </p>

      <div className="evaluation-table-wrap">
        <table className="evaluation-table">
          <thead>
            <tr>
              <th>Category</th>
              <th>Evaluation criteria</th>
              <th>Score</th>
            </tr>
          </thead>
          <tbody>
            {evaluation.categories.map((c) => (
              <tr key={c.id}>
                <td>{c.categoryName}</td>
                <td>{c.criteria}</td>
                <td>{c.score}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="row-actions">
        <button className="secondary" onClick={startEdit}>
          Edit
        </button>
        <button className="danger" onClick={onDelete}>
          Delete
        </button>
      </div>
    </div>
  );
}
