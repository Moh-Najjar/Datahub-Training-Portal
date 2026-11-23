import { useEffect, useRef, useState, type FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { materialsApi } from "../api/materials";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { RichTextEditor } from "../components/RichTextEditor";
import { formatFileSize } from "../utils/format";
import { MaterialType, type MaterialFileItemDto, type TrainingMaterialDto } from "../types";

function isBlankHtml(html: string): boolean {
  const div = document.createElement("div");
  div.innerHTML = html;
  return (div.textContent ?? "").trim().length === 0;
}

interface LinkRow {
  id: string;
  url: string;
  title: string;
}

function newLinkRow(): LinkRow {
  return { id: crypto.randomUUID(), url: "", title: "" };
}

export function MaterialEditPage() {
  const { trainingId, materialId } = useParams<{ trainingId: string; materialId: string }>();
  const navigate = useNavigate();

  const [material, setMaterial] = useState<TrainingMaterialDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [linkRows, setLinkRows] = useState<LinkRow[]>([newLinkRow()]);
  const [isSaving, setIsSaving] = useState(false);

  const newFileInputRef = useRef<HTMLInputElement>(null);
  const [newFileTitle, setNewFileTitle] = useState("");
  const [isAddingFile, setIsAddingFile] = useState(false);

  const load = () => {
    if (!trainingId || !materialId) return;
    setIsLoading(true);
    setError(null);
    materialsApi
      .getForTraining(trainingId)
      .then((materials) => {
        const found = materials.find((m) => m.id === materialId) ?? null;
        setMaterial(found);
        if (found) {
          setTitle(found.title);
          setContent(found.textContent ?? "");
          setLinkRows(
            found.linkItems.length > 0
              ? found.linkItems.map((l) => ({ id: l.id, url: l.url, title: l.title ?? "" }))
              : [newLinkRow()]
          );
        }
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load content."))
      .finally(() => setIsLoading(false));
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [trainingId, materialId]);

  const updateLinkRow = (id: string, patch: Partial<LinkRow>) => {
    setLinkRows((rows) => rows.map((r) => (r.id === id ? { ...r, ...patch } : r)));
  };

  const handleSave = async (event: FormEvent) => {
    event.preventDefault();
    if (!trainingId || !materialId || !material) return;

    if (material.type === MaterialType.Text && isBlankHtml(content)) {
      setError("Content cannot be empty.");
      return;
    }

    setIsSaving(true);
    setError(null);
    try {
      if (material.type === MaterialType.Text) {
        await materialsApi.updateText(trainingId, materialId, { title, content });
      } else if (material.type === MaterialType.Link) {
        const links = linkRows
          .filter((r) => r.url.trim().length > 0)
          .map((r) => ({ url: r.url, title: r.title || undefined }));
        if (links.length === 0) {
          setError("Add at least one link.");
          setIsSaving(false);
          return;
        }
        await materialsApi.updateLink(trainingId, materialId, { title, links });
      } else {
        await materialsApi.updateFileTitle(trainingId, materialId, { title });
      }
      navigate(`/trainings/${trainingId}/content`);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update content.");
    } finally {
      setIsSaving(false);
    }
  };

  const handleAddFile = async () => {
    if (!trainingId || !materialId) return;
    const file = newFileInputRef.current?.files?.[0];
    if (!file) return;

    setIsAddingFile(true);
    setError(null);
    try {
      await materialsApi.addFileItems(trainingId, materialId, [{ file, title: newFileTitle || undefined }]);
      setNewFileTitle("");
      if (newFileInputRef.current) newFileInputRef.current.value = "";
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to add file.");
    } finally {
      setIsAddingFile(false);
    }
  };

  const handleRenameFileItem = async (fileItemId: string, newTitle: string) => {
    if (!trainingId || !materialId) return;
    setError(null);
    try {
      await materialsApi.updateFileItemTitle(trainingId, materialId, fileItemId, newTitle || undefined);
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to rename file.");
    }
  };

  const handleRemoveFileItem = async (fileItemId: string) => {
    if (!trainingId || !materialId || !confirm("Remove this file?")) return;
    setError(null);
    try {
      await materialsApi.deleteFileItem(trainingId, materialId, fileItemId);
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to remove file.");
    }
  };

  const handleDownloadFileItem = async (fileItemId: string, fileName: string) => {
    if (!trainingId || !materialId) return;
    setError(null);
    try {
      const { blob } = await materialsApi.downloadFileItem(trainingId, materialId, fileItemId);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = fileName;
      link.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to download file.");
    }
  };

  return (
    <div>
      <Link to={`/trainings/${trainingId}/content`} className="back-link">
        &larr; Back to content
      </Link>

      <ErrorBanner message={error} />

      {isLoading ? (
        <p>Loading...</p>
      ) : material ? (
        <>
          <div className="page-header">
            <h1>Edit content</h1>
          </div>

          <form className="card edit-content-form" onSubmit={handleSave}>
            <label>
              Title
              <input
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                required={material.type !== MaterialType.File}
              />
            </label>

            {material.type === MaterialType.Text && (
              <label>
                Content
                <RichTextEditor value={content} onChange={setContent} />
              </label>
            )}

            {material.type === MaterialType.Link && (
              <div className="multi-item-editor">
                {linkRows.map((row) => (
                  <div className="multi-item-row" key={row.id}>
                    <input
                      type="url"
                      value={row.url}
                      onChange={(e) => updateLinkRow(row.id, { url: e.target.value })}
                      placeholder="https://..."
                    />
                    <input
                      value={row.title}
                      onChange={(e) => updateLinkRow(row.id, { title: e.target.value })}
                      placeholder="Title (optional)"
                    />
                    {linkRows.length > 1 && (
                      <button
                        type="button"
                        className="link-button link-button-danger"
                        onClick={() => setLinkRows((rows) => rows.filter((r) => r.id !== row.id))}
                      >
                        Remove
                      </button>
                    )}
                  </div>
                ))}
                <button
                  type="button"
                  className="secondary"
                  onClick={() => setLinkRows((rows) => [...rows, newLinkRow()])}
                >
                  Add another link
                </button>
              </div>
            )}

            <div className="row-actions">
              <button type="submit" disabled={isSaving}>
                {isSaving ? "Saving..." : "Save"}
              </button>
              <button
                type="button"
                className="secondary"
                onClick={() => navigate(`/trainings/${trainingId}/content`)}
              >
                Cancel
              </button>
            </div>
          </form>

          {material.type === MaterialType.File && (
            <div className="card">
              <h3>Files</h3>
              {material.fileItems.length === 0 ? (
                <p className="muted">No files yet.</p>
              ) : (
                <ul className="assignment-attachments">
                  {material.fileItems.map((item) => (
                    <FileItemRow
                      key={item.id}
                      item={item}
                      onDownload={() => handleDownloadFileItem(item.id, item.title || item.fileName)}
                      onRename={(newTitle) => handleRenameFileItem(item.id, newTitle)}
                      onRemove={() => handleRemoveFileItem(item.id)}
                    />
                  ))}
                </ul>
              )}

              <div className="multi-item-row">
                <input type="file" ref={newFileInputRef} />
                <input
                  value={newFileTitle}
                  onChange={(e) => setNewFileTitle(e.target.value)}
                  placeholder="Title (optional)"
                />
                <button type="button" className="secondary" onClick={handleAddFile} disabled={isAddingFile}>
                  {isAddingFile ? "Adding..." : "Add file"}
                </button>
              </div>
            </div>
          )}
        </>
      ) : (
        <p className="muted">This content could not be found.</p>
      )}
    </div>
  );
}

interface FileItemRowProps {
  item: MaterialFileItemDto;
  onDownload: () => void;
  onRename: (title: string) => void;
  onRemove: () => void;
}

function FileItemRow({ item, onDownload, onRename, onRemove }: FileItemRowProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [title, setTitle] = useState(item.title ?? "");

  if (isEditing) {
    return (
      <li>
        <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder={item.fileName} />
        <button
          type="button"
          className="link-button"
          onClick={() => {
            onRename(title);
            setIsEditing(false);
          }}
        >
          Save
        </button>
        <button type="button" className="link-button" onClick={() => setIsEditing(false)}>
          Cancel
        </button>
      </li>
    );
  }

  return (
    <li>
      <button type="button" className="link-button" onClick={onDownload}>
        {item.title || item.fileName}
      </button>
      <span className="muted"> ({formatFileSize(item.fileSizeBytes)})</span>
      <button type="button" className="link-button" onClick={() => setIsEditing(true)}>
        Rename
      </button>
      <button type="button" className="link-button link-button-danger" onClick={onRemove}>
        Remove
      </button>
    </li>
  );
}
