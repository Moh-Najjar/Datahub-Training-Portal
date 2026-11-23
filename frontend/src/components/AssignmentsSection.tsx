import { useEffect, useRef, useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { assignmentsApi } from "../api/assignments";
import { assignmentSubmissionsApi } from "../api/assignmentSubmissions";
import { ApiError } from "../api/client";
import { ErrorBanner } from "./ErrorBanner";
import { formatAssignmentStatus, formatDateTime, formatFileSize } from "../utils/format";
import { AssignmentStatus, type AssignmentAttachmentDto, type AssignmentDto, type AssignmentSubmissionDto } from "../types";

interface AssignmentsSectionProps {
  trainingId: string;
  canAuthor: boolean;
}

function toDatetimeLocalValue(iso: string): string {
  const date = new Date(iso);
  const localTime = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
  return localTime.toISOString().slice(0, 16);
}

function fromDatetimeLocalValue(value: string): string {
  return new Date(value).toISOString();
}

const statusBadgeClass: Record<AssignmentStatus, string> = {
  [AssignmentStatus.Upcoming]: "badge badge-upcoming",
  [AssignmentStatus.Open]: "badge badge-open",
  [AssignmentStatus.Closed]: "badge badge-closed",
};

export function AssignmentsSection({ trainingId, canAuthor }: AssignmentsSectionProps) {
  const [assignments, setAssignments] = useState<AssignmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [opensAt, setOpensAt] = useState("");
  const [closesAt, setClosesAt] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const load = async () => {
    setIsLoading(true);
    setError(null);
    try {
      setAssignments(await assignmentsApi.getForTraining(trainingId));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load assignments.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [trainingId]);

  const handleCreate = async (event: FormEvent) => {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);
    try {
      const files = fileInputRef.current?.files ? Array.from(fileInputRef.current.files) : [];
      await assignmentsApi.create(trainingId, {
        title,
        description,
        opensAt: fromDatetimeLocalValue(opensAt),
        closesAt: fromDatetimeLocalValue(closesAt),
        files,
      });
      setTitle("");
      setDescription("");
      setOpensAt("");
      setClosesAt("");
      if (fileInputRef.current) fileInputRef.current.value = "";
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create assignment.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="assignments-section">
      <h2>Assignments</h2>
      <ErrorBanner message={error} />

      {canAuthor && (
        <form className="card add-content-form" onSubmit={handleCreate}>
          <label>
            Title
            <input value={title} onChange={(e) => setTitle(e.target.value)} required />
          </label>
          <label>
            Description
            <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3} required />
          </label>
          <div className="form-grid">
            <label>
              Opens at
              <input type="datetime-local" value={opensAt} onChange={(e) => setOpensAt(e.target.value)} required />
            </label>
            <label>
              Closes at
              <input type="datetime-local" value={closesAt} onChange={(e) => setClosesAt(e.target.value)} required />
            </label>
          </div>
          <label>
            Attachments (optional)
            <input type="file" ref={fileInputRef} multiple />
          </label>
          <div className="add-content-actions">
            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Adding..." : "Add assignment"}
            </button>
          </div>
        </form>
      )}

      {isLoading ? (
        <p>Loading...</p>
      ) : assignments.length === 0 ? (
        <p className="muted">No assignments yet.</p>
      ) : (
        <div className="content-grid">
          {assignments.map((assignment) => (
            <AssignmentCard
              key={assignment.id}
              assignment={assignment}
              trainingId={trainingId}
              canAuthor={canAuthor}
              onChanged={load}
              onError={setError}
            />
          ))}
        </div>
      )}
    </div>
  );
}

interface AssignmentCardProps {
  assignment: AssignmentDto;
  trainingId: string;
  canAuthor: boolean;
  onChanged: () => void | Promise<void>;
  onError: (message: string) => void;
}

function AssignmentCard({ assignment, trainingId, canAuthor, onChanged, onError }: AssignmentCardProps) {
  const navigate = useNavigate();
  const [isEditing, setIsEditing] = useState(false);
  const [editTitle, setEditTitle] = useState(assignment.title);
  const [editDescription, setEditDescription] = useState(assignment.description);
  const [editOpensAt, setEditOpensAt] = useState(() => toDatetimeLocalValue(assignment.opensAt));
  const [editClosesAt, setEditClosesAt] = useState(() => toDatetimeLocalValue(assignment.closesAt));
  const [isSaving, setIsSaving] = useState(false);
  const addAttachmentInputRef = useRef<HTMLInputElement>(null);

  const [mySubmission, setMySubmission] = useState<AssignmentSubmissionDto | null>(null);
  const [isLoadingSubmission, setIsLoadingSubmission] = useState(!canAuthor);
  const [submissionText, setSubmissionText] = useState("");
  const [isSavingSubmission, setIsSavingSubmission] = useState(false);
  const submissionFileInputRef = useRef<HTMLInputElement>(null);

  const handleStartEdit = () => {
    setEditTitle(assignment.title);
    setEditDescription(assignment.description);
    setEditOpensAt(toDatetimeLocalValue(assignment.opensAt));
    setEditClosesAt(toDatetimeLocalValue(assignment.closesAt));
    setIsEditing(true);
  };

  const handleSaveEdit = async () => {
    setIsSaving(true);
    try {
      await assignmentsApi.update(trainingId, assignment.id, {
        title: editTitle,
        description: editDescription,
        opensAt: fromDatetimeLocalValue(editOpensAt),
        closesAt: fromDatetimeLocalValue(editClosesAt),
      });
      setIsEditing(false);
      await onChanged();
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to update assignment.");
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!confirm("Delete this assignment?")) return;
    try {
      await assignmentsApi.delete(trainingId, assignment.id);
      await onChanged();
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to delete assignment.");
    }
  };

  const handleAddAttachments = async () => {
    const files = addAttachmentInputRef.current?.files ? Array.from(addAttachmentInputRef.current.files) : [];
    if (files.length === 0) return;
    try {
      await assignmentsApi.addAttachments(trainingId, assignment.id, files);
      if (addAttachmentInputRef.current) addAttachmentInputRef.current.value = "";
      await onChanged();
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to add attachments.");
    }
  };

  const handleDeleteAttachment = async (attachmentId: string) => {
    if (!confirm("Remove this attachment?")) return;
    try {
      await assignmentsApi.deleteAttachment(trainingId, assignment.id, attachmentId);
      await onChanged();
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to remove attachment.");
    }
  };

  const handleDownloadAttachment = async (attachmentId: string, fileName: string) => {
    try {
      const { blob } = await assignmentsApi.downloadAttachment(trainingId, assignment.id, attachmentId);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = fileName;
      link.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to download attachment.");
    }
  };

  useEffect(() => {
    if (canAuthor) return;
    let cancelled = false;
    setIsLoadingSubmission(true);
    assignmentSubmissionsApi
      .getMine(trainingId, assignment.id)
      .then((submission) => {
        if (cancelled) return;
        setMySubmission(submission);
        setSubmissionText(submission?.textResponse ?? "");
      })
      .catch((err) => onError(err instanceof ApiError ? err.message : "Failed to load your submission."))
      .finally(() => {
        if (!cancelled) setIsLoadingSubmission(false);
      });
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [trainingId, assignment.id, canAuthor]);

  const handleSaveSubmission = async () => {
    setIsSavingSubmission(true);
    try {
      const files = submissionFileInputRef.current?.files ? Array.from(submissionFileInputRef.current.files) : [];
      const result = await assignmentSubmissionsApi.upsertMine(trainingId, assignment.id, submissionText, files);
      setMySubmission(result);
      if (submissionFileInputRef.current) submissionFileInputRef.current.value = "";
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to save your submission.");
    } finally {
      setIsSavingSubmission(false);
    }
  };

  const handleDeleteSubmissionAttachment = async (attachmentId: string) => {
    if (!confirm("Remove this attachment from your submission?")) return;
    try {
      await assignmentSubmissionsApi.deleteAttachment(trainingId, assignment.id, attachmentId);
      setMySubmission((prev) => (prev ? { ...prev, attachments: prev.attachments.filter((a) => a.id !== attachmentId) } : prev));
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to remove attachment.");
    }
  };

  const handleDownloadSubmissionAttachment = async (attachmentId: string, fileName: string) => {
    try {
      const { blob } = await assignmentSubmissionsApi.downloadAttachment(trainingId, assignment.id, attachmentId);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = fileName;
      link.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to download attachment.");
    }
  };

  if (isEditing) {
    return (
      <div className="content-card">
        <div className="edit-content-form">
          <label>
            Title
            <input value={editTitle} onChange={(e) => setEditTitle(e.target.value)} required />
          </label>
          <label>
            Description
            <textarea value={editDescription} onChange={(e) => setEditDescription(e.target.value)} rows={3} required />
          </label>
          <div className="form-grid">
            <label>
              Opens at
              <input type="datetime-local" value={editOpensAt} onChange={(e) => setEditOpensAt(e.target.value)} required />
            </label>
            <label>
              Closes at
              <input type="datetime-local" value={editClosesAt} onChange={(e) => setEditClosesAt(e.target.value)} required />
            </label>
          </div>
          <div className="row-actions">
            <button onClick={handleSaveEdit} disabled={isSaving}>
              {isSaving ? "Saving..." : "Save"}
            </button>
            <button type="button" className="secondary" onClick={() => setIsEditing(false)}>
              Cancel
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="content-card">
      <div className="content-card-header">
        <span className="content-card-title">{assignment.title}</span>
        <span className={statusBadgeClass[assignment.status]}>{formatAssignmentStatus(assignment.status)}</span>
      </div>

      <p className="assignment-description">{assignment.description}</p>
      <p className="muted assignment-window">
        Opens {formatDateTime(assignment.opensAt)} &middot; Closes {formatDateTime(assignment.closesAt)}
      </p>

      {assignment.attachments.length > 0 && (
        <ul className="assignment-attachments">
          {assignment.attachments.map((attachment) => (
            <li key={attachment.id}>
              <button
                type="button"
                className="link-button"
                onClick={() => handleDownloadAttachment(attachment.id, attachment.fileName)}
              >
                {attachment.fileName}
              </button>
              <span className="muted"> ({formatFileSize(attachment.fileSizeBytes)})</span>
              {canAuthor && (
                <button
                  type="button"
                  className="link-button link-button-danger"
                  onClick={() => handleDeleteAttachment(attachment.id)}
                >
                  Remove
                </button>
              )}
            </li>
          ))}
        </ul>
      )}

      {canAuthor && (
        <div className="assignment-add-attachment">
          <input type="file" ref={addAttachmentInputRef} multiple />
          <button type="button" className="secondary" onClick={handleAddAttachments}>
            Add attachment
          </button>
        </div>
      )}

      {!canAuthor && (
        <div className="assignment-submission">
          <h4>Your submission</h4>
          {isLoadingSubmission ? (
            <p className="muted">Loading...</p>
          ) : assignment.status === AssignmentStatus.Open ? (
            <>
              <textarea
                value={submissionText}
                onChange={(e) => setSubmissionText(e.target.value)}
                rows={3}
                placeholder="Write your response..."
              />
              <AttachmentList
                attachments={mySubmission?.attachments ?? []}
                onDownload={handleDownloadSubmissionAttachment}
                onRemove={handleDeleteSubmissionAttachment}
              />
              <input type="file" ref={submissionFileInputRef} multiple />
              <div className="row-actions">
                <button onClick={handleSaveSubmission} disabled={isSavingSubmission}>
                  {isSavingSubmission ? "Saving..." : mySubmission ? "Update submission" : "Submit"}
                </button>
              </div>
              {mySubmission && (
                <p className="muted">Last saved {formatDateTime(mySubmission.updatedAt ?? mySubmission.submittedAt)}</p>
              )}
            </>
          ) : mySubmission ? (
            <>
              <p className="assignment-description">
                {mySubmission.textResponse || <span className="muted">(no written response)</span>}
              </p>
              <AttachmentList attachments={mySubmission.attachments} onDownload={handleDownloadSubmissionAttachment} />
              <p className="muted">Submitted {formatDateTime(mySubmission.submittedAt)}</p>
            </>
          ) : (
            <p className="muted">
              {assignment.status === AssignmentStatus.Upcoming ? "Not open yet." : "You did not submit anything."}
            </p>
          )}
        </div>
      )}

      <div className="content-card-footer">
        <span>{assignment.createdByUserName ?? "-"}</span>
        {canAuthor && (
          <div className="row-actions">
            <button
              className="secondary"
              onClick={() => navigate(`/trainings/${trainingId}/assignments/${assignment.id}/submissions`)}
            >
              View submissions
            </button>
            <button className="secondary" onClick={handleStartEdit}>
              Edit
            </button>
            <button className="danger" onClick={handleDelete}>
              Delete
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

interface AttachmentListProps {
  attachments: AssignmentAttachmentDto[];
  onDownload: (attachmentId: string, fileName: string) => void;
  onRemove?: (attachmentId: string) => void;
}

function AttachmentList({ attachments, onDownload, onRemove }: AttachmentListProps) {
  if (attachments.length === 0) return null;

  return (
    <ul className="assignment-attachments">
      {attachments.map((attachment) => (
        <li key={attachment.id}>
          <button type="button" className="link-button" onClick={() => onDownload(attachment.id, attachment.fileName)}>
            {attachment.fileName}
          </button>
          <span className="muted"> ({formatFileSize(attachment.fileSizeBytes)})</span>
          {onRemove && (
            <button type="button" className="link-button link-button-danger" onClick={() => onRemove(attachment.id)}>
              Remove
            </button>
          )}
        </li>
      ))}
    </ul>
  );
}
