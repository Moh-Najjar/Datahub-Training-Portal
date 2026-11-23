import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { assignmentsApi } from "../api/assignments";
import { assignmentSubmissionsApi } from "../api/assignmentSubmissions";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { formatAssignmentStatus, formatDateTime, formatFileSize } from "../utils/format";
import { AssignmentStatus, type AssignmentDto, type AssignmentSubmissionOverviewDto } from "../types";

const statusBadgeClass: Record<AssignmentStatus, string> = {
  [AssignmentStatus.Upcoming]: "badge badge-upcoming",
  [AssignmentStatus.Open]: "badge badge-open",
  [AssignmentStatus.Closed]: "badge badge-closed",
};

export function AssignmentSubmissionsPage() {
  const { trainingId, assignmentId } = useParams<{ trainingId: string; assignmentId: string }>();

  const [assignment, setAssignment] = useState<AssignmentDto | null>(null);
  const [overview, setOverview] = useState<AssignmentSubmissionOverviewDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!trainingId || !assignmentId) return;
    setIsLoading(true);
    setError(null);
    Promise.all([
      assignmentsApi.getForTraining(trainingId),
      assignmentSubmissionsApi.getAllForAssignment(trainingId, assignmentId),
    ])
      .then(([assignments, submissions]) => {
        setAssignment(assignments.find((a) => a.id === assignmentId) ?? null);
        setOverview(submissions);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load submissions."))
      .finally(() => setIsLoading(false));
  }, [trainingId, assignmentId]);

  const handleDownload = async (attachmentId: string, fileName: string) => {
    if (!trainingId || !assignmentId) return;
    setError(null);
    try {
      const { blob } = await assignmentSubmissionsApi.downloadAttachment(trainingId, assignmentId, attachmentId);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = fileName;
      link.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to download attachment.");
    }
  };

  const submittedCount = overview.filter((entry) => entry.submission).length;

  return (
    <div>
      <Link to={`/trainings/${trainingId}/assignments`} className="back-link">
        &larr; Back to assignments
      </Link>

      <ErrorBanner message={error} />

      {isLoading ? (
        <p>Loading...</p>
      ) : assignment ? (
        <>
          <div className="page-header">
            <h1>{assignment.title}</h1>
            <span className={statusBadgeClass[assignment.status]}>{formatAssignmentStatus(assignment.status)}</span>
          </div>
          <p className="muted">
            Opens {formatDateTime(assignment.opensAt)} &middot; Closes {formatDateTime(assignment.closesAt)}
            {" · "}
            {submittedCount}/{overview.length} submitted
          </p>

          {overview.length === 0 ? (
            <p className="muted">No one is enrolled in this training yet.</p>
          ) : (
            <ul className="submission-overview-list">
              {overview.map((entry) => (
                <li key={entry.userId} className="submission-overview-item">
                  <div className="submission-overview-header">
                    <span className="content-card-title">{entry.userName}</span>
                    <span className={entry.submission ? "badge badge-open" : "badge badge-closed"}>
                      {entry.submission ? "Submitted" : "Not submitted"}
                    </span>
                  </div>
                  {entry.submission && (
                    <>
                      <p className="assignment-description">
                        {entry.submission.textResponse || <span className="muted">(no written response)</span>}
                      </p>
                      {entry.submission.attachments.length > 0 && (
                        <ul className="assignment-attachments">
                          {entry.submission.attachments.map((attachment) => (
                            <li key={attachment.id}>
                              <button
                                type="button"
                                className="link-button"
                                onClick={() => handleDownload(attachment.id, attachment.fileName)}
                              >
                                {attachment.fileName}
                              </button>
                              <span className="muted"> ({formatFileSize(attachment.fileSizeBytes)})</span>
                            </li>
                          ))}
                        </ul>
                      )}
                      <p className="muted">
                        Submitted {formatDateTime(entry.submission.submittedAt)}
                        {entry.submission.updatedAt && ` · updated ${formatDateTime(entry.submission.updatedAt)}`}
                      </p>
                    </>
                  )}
                </li>
              ))}
            </ul>
          )}
        </>
      ) : (
        <p className="muted">This assignment could not be found.</p>
      )}
    </div>
  );
}
