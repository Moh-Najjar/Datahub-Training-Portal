import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { rankingsApi } from "../api/rankings";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { useAuth } from "../context/AuthContext";
import { canManageEvaluations } from "../utils/permissions";
import { formatDate } from "../utils/format";
import type { RankingEntryDto } from "../types";

export function RankingsPage() {
  const { user } = useAuth();
  const canManage = canManageEvaluations(user!.role);

  const [rankings, setRankings] = useState<RankingEntryDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!canManage) return;
    setIsLoading(true);
    setError(null);
    rankingsApi
      .getAll()
      .then(setRankings)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load rankings."))
      .finally(() => setIsLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  if (!canManage) {
    return (
      <div>
        <div className="page-header">
          <h1>Rankings</h1>
        </div>
        <p className="muted">Only SuperAdmin and DepartmentAdmin can view rankings.</p>
      </div>
    );
  }

  return (
    <div>
      <div className="page-header">
        <h1>Rankings</h1>
      </div>
      <p className="muted">Ranked by each person's most recent evaluation total.</p>

      <ErrorBanner message={error} />

      {isLoading ? (
        <p>Loading...</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>#</th>
              <th>Name</th>
              <th>Role</th>
              <th>Department</th>
              <th>Total</th>
              <th>Evaluations</th>
              <th>Last evaluated</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {rankings.map((entry, index) => (
              <tr key={entry.userId}>
                <td>{entry.latestTotal !== null ? index + 1 : "-"}</td>
                <td>{entry.userName}</td>
                <td>{entry.role}</td>
                <td>{entry.departmentName ?? "-"}</td>
                <td>
                  {entry.latestTotal !== null ? (
                    `${entry.latestTotal} of ${entry.latestMaxTotal}`
                  ) : (
                    <span className="muted">Not evaluated</span>
                  )}
                </td>
                <td>{entry.evaluationCount}</td>
                <td>{formatDate(entry.latestEvaluatedAt)}</td>
                <td>
                  <div className="row-actions">
                    <Link to={`/users/${entry.userId}/evaluations`}>
                      <button className="secondary" type="button">
                        Evaluate
                      </button>
                    </Link>
                  </div>
                </td>
              </tr>
            ))}
            {rankings.length === 0 && (
              <tr>
                <td colSpan={8} className="muted">
                  No trainees, employees, or interns found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
