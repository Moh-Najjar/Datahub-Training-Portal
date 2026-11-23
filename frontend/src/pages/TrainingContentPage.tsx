import { useEffect, useRef, useState, type DragEvent, type FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import { trainingsApi } from "../api/trainings";
import { materialsApi } from "../api/materials";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { RichTextEditor } from "../components/RichTextEditor";
import { useAuth } from "../context/AuthContext";
import { canAuthorTrainings } from "../utils/permissions";
import { formatFileSize } from "../utils/format";
import {
  MaterialType,
  type MaterialFileItemDto,
  type TrainingDto,
  type TrainingMaterialDto,
} from "../types";

const materialTypeLabels: Record<MaterialType, string> = {
  [MaterialType.Text]: "Text",
  [MaterialType.Link]: "Link",
  [MaterialType.File]: "File",
};

const materialTypeIcons: Record<MaterialType, string> = {
  [MaterialType.Text]: "📝",
  [MaterialType.Link]: "🔗",
  [MaterialType.File]: "📎",
};

function isBlankHtml(html: string): boolean {
  const div = document.createElement("div");
  div.innerHTML = html;
  return (div.textContent ?? "").trim().length === 0;
}

interface FileRow {
  id: string;
  file: File | null;
  title: string;
}

interface LinkRow {
  id: string;
  url: string;
  title: string;
}

function newFileRow(): FileRow {
  return { id: crypto.randomUUID(), file: null, title: "" };
}

function newLinkRow(): LinkRow {
  return { id: crypto.randomUUID(), url: "", title: "" };
}

export function TrainingContentPage() {
  const { trainingId } = useParams<{ trainingId: string }>();
  const { user } = useAuth();
  const canAuthor = canAuthorTrainings(user!.role);

  const [training, setTraining] = useState<TrainingDto | null>(null);
  const [materials, setMaterials] = useState<TrainingMaterialDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isSubmittingMaterial, setIsSubmittingMaterial] = useState(false);
  const draggedMaterialId = useRef<string | null>(null);
  const [isSavingMaterialOrder, setIsSavingMaterialOrder] = useState(false);

  const [materialType, setMaterialType] = useState<MaterialType>(MaterialType.File);
  const [materialTitle, setMaterialTitle] = useState("");
  const [materialContent, setMaterialContent] = useState("");
  const [fileRows, setFileRows] = useState<FileRow[]>([newFileRow()]);
  const [linkRows, setLinkRows] = useState<LinkRow[]>([newLinkRow()]);

  const load = async () => {
    if (!trainingId) return;
    setIsLoading(true);
    setError(null);
    try {
      const [trainingResult, materialsResult] = await Promise.all([
        trainingsApi.getById(trainingId),
        materialsApi.getForTraining(trainingId),
      ]);
      setTraining(trainingResult);
      setMaterials(materialsResult);
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

  const resetMaterialForm = () => {
    setMaterialTitle("");
    setMaterialContent("");
    setFileRows([newFileRow()]);
    setLinkRows([newLinkRow()]);
  };

  const updateFileRow = (id: string, patch: Partial<FileRow>) => {
    setFileRows((rows) => rows.map((r) => (r.id === id ? { ...r, ...patch } : r)));
  };

  const updateLinkRow = (id: string, patch: Partial<LinkRow>) => {
    setLinkRows((rows) => rows.map((r) => (r.id === id ? { ...r, ...patch } : r)));
  };

  const handleAddMaterial = async (event: FormEvent) => {
    event.preventDefault();
    if (!trainingId) return;

    setIsSubmittingMaterial(true);
    setError(null);
    try {
      if (materialType === MaterialType.File) {
        const items = fileRows
          .filter((r) => r.file !== null)
          .map((r) => ({ file: r.file!, title: r.title || undefined }));
        if (items.length === 0) {
          setError("Add at least one file.");
          return;
        }
        await materialsApi.addFileGroup(trainingId, materialTitle || undefined, items);
      } else if (materialType === MaterialType.Text) {
        if (isBlankHtml(materialContent)) {
          setError("Content cannot be empty.");
          return;
        }
        await materialsApi.addText(trainingId, { title: materialTitle, content: materialContent });
      } else {
        const links = linkRows
          .filter((r) => r.url.trim().length > 0)
          .map((r) => ({ url: r.url, title: r.title || undefined }));
        if (links.length === 0) {
          setError("Add at least one link.");
          return;
        }
        await materialsApi.addLink(trainingId, { title: materialTitle, links });
      }
      resetMaterialForm();
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to add content.");
    } finally {
      setIsSubmittingMaterial(false);
    }
  };

  const handleDownloadFileItem = async (material: TrainingMaterialDto, item: MaterialFileItemDto) => {
    if (!trainingId) return;
    setError(null);
    try {
      const { blob, fileName } = await materialsApi.downloadFileItem(trainingId, material.id, item.id);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = fileName || item.fileName;
      link.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to download file.");
    }
  };

  const handleDeleteMaterial = async (materialId: string) => {
    if (!trainingId || !confirm("Delete this content?")) return;
    setError(null);
    try {
      await materialsApi.delete(trainingId, materialId);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete content.");
    }
  };

  const handleMaterialDragStart = (id: string) => {
    draggedMaterialId.current = id;
  };

  const handleMaterialDragOver = (event: DragEvent<HTMLDivElement>, overId: string) => {
    event.preventDefault();
    const sourceId = draggedMaterialId.current;
    if (!sourceId || sourceId === overId) return;

    setMaterials((current) => {
      const sourceIndex = current.findIndex((m) => m.id === sourceId);
      const targetIndex = current.findIndex((m) => m.id === overId);
      if (sourceIndex === -1 || targetIndex === -1) return current;

      const next = [...current];
      const [moved] = next.splice(sourceIndex, 1);
      next.splice(targetIndex, 0, moved);
      return next;
    });
  };

  const handleMaterialDrop = async () => {
    draggedMaterialId.current = null;
    if (!trainingId) return;

    setIsSavingMaterialOrder(true);
    setError(null);
    try {
      await materialsApi.reorder(trainingId, { orderedIds: materials.map((m) => m.id) });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save the new order.");
      await load();
    } finally {
      setIsSavingMaterialOrder(false);
    }
  };

  if (isLoading) return <p>Loading...</p>;

  return (
    <div>
      <Link to={`/trainings/${trainingId}`} className="back-link">
        &larr; Back to training
      </Link>

      <ErrorBanner message={error} />

      {training && (
        <>
          <div className="page-header">
            <h1>{training.title}</h1>
          </div>

          {canAuthor && (
            <form className="card add-content-form" onSubmit={handleAddMaterial}>
              <div className="content-type-picker">
                {(
                  [
                    [MaterialType.File, "📎", "File"],
                    [MaterialType.Text, "📝", "Text"],
                    [MaterialType.Link, "🔗", "Link"],
                  ] as const
                ).map(([type, icon, label]) => (
                  <button
                    key={type}
                    type="button"
                    className={materialType === type ? "content-type-option active" : "content-type-option"}
                    onClick={() => setMaterialType(type)}
                  >
                    <span>{icon}</span> {label}
                  </button>
                ))}
              </div>

              <label>
                Title
                <input
                  value={materialTitle}
                  onChange={(e) => setMaterialTitle(e.target.value)}
                  placeholder={materialType === MaterialType.File ? "Defaults to the first file's name if left blank" : undefined}
                  required={materialType !== MaterialType.File}
                />
              </label>

              {materialType === MaterialType.File && (
                <div className="multi-item-editor">
                  {fileRows.map((row) => (
                    <div className="multi-item-row" key={row.id}>
                      <input
                        type="file"
                        onChange={(e) => updateFileRow(row.id, { file: e.target.files?.[0] ?? null })}
                      />
                      <input
                        value={row.title}
                        onChange={(e) => updateFileRow(row.id, { title: e.target.value })}
                        placeholder="Title (optional)"
                      />
                      {fileRows.length > 1 && (
                        <button
                          type="button"
                          className="link-button link-button-danger"
                          onClick={() => setFileRows((rows) => rows.filter((r) => r.id !== row.id))}
                        >
                          Remove
                        </button>
                      )}
                    </div>
                  ))}
                  <button type="button" className="secondary" onClick={() => setFileRows((rows) => [...rows, newFileRow()])}>
                    Add another file
                  </button>
                </div>
              )}

              {materialType === MaterialType.Text && (
                <label>
                  Content
                  <RichTextEditor
                    value={materialContent}
                    onChange={setMaterialContent}
                    placeholder="Write the training content here..."
                  />
                </label>
              )}

              {materialType === MaterialType.Link && (
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
                  <button type="button" className="secondary" onClick={() => setLinkRows((rows) => [...rows, newLinkRow()])}>
                    Add another link
                  </button>
                </div>
              )}

              <div className="add-content-actions">
                <button type="submit" disabled={isSubmittingMaterial}>
                  {isSubmittingMaterial ? "Adding..." : "Add content"}
                </button>
              </div>
            </form>
          )}

          {materials.length === 0 ? (
            <p className="muted">No content added yet.</p>
          ) : (
            <>
              {canAuthor && (
                <p className="muted">
                  Drag the handle to reorder content.{isSavingMaterialOrder && " Saving order..."}
                </p>
              )}
              <div className="content-grid">
                {materials.map((m) => (
                  <div
                    className="content-card"
                    key={m.id}
                    onDragOver={canAuthor ? (e) => handleMaterialDragOver(e, m.id) : undefined}
                    onDrop={canAuthor ? handleMaterialDrop : undefined}
                  >
                    <div className="content-card-header">
                      {canAuthor && (
                        <span
                          className="drag-handle"
                          draggable
                          onDragStart={() => handleMaterialDragStart(m.id)}
                          title="Drag to reorder"
                        >
                          &#8942;&#8942;
                        </span>
                      )}
                      <span className="content-card-icon">{materialTypeIcons[m.type]}</span>
                      <span className="content-card-title">{m.title}</span>
                      <span className="badge">{materialTypeLabels[m.type]}</span>
                    </div>

                    {m.type === MaterialType.Text && (
                      <Link to={`/trainings/${trainingId}/materials/${m.id}`} className="content-card-text-link">
                        <div
                          className="content-card-text clickable"
                          title="Click to read the full text"
                          dangerouslySetInnerHTML={{ __html: m.textContent ?? "" }}
                        />
                        <span className="content-card-expand-hint">Read full text &rarr;</span>
                      </Link>
                    )}

                    {m.type === MaterialType.Link && m.linkItems.length > 0 && (
                      <ul className="assignment-attachments">
                        {m.linkItems.map((item) => (
                          <li key={item.id}>
                            <a className="content-card-link" href={item.url} target="_blank" rel="noreferrer">
                              {item.title || item.url} &#8599;
                            </a>
                          </li>
                        ))}
                      </ul>
                    )}

                    {m.type === MaterialType.File && m.fileItems.length > 0 && (
                      <ul className="assignment-attachments">
                        {m.fileItems.map((item) => (
                          <li key={item.id}>
                            <button
                              type="button"
                              className="link-button"
                              onClick={() => handleDownloadFileItem(m, item)}
                            >
                              {item.title || item.fileName}
                            </button>
                            <span className="muted"> ({formatFileSize(item.fileSizeBytes)})</span>
                          </li>
                        ))}
                      </ul>
                    )}

                    <div className="content-card-footer">
                      <span>{m.uploadedByUserName ?? "-"}</span>
                      <div className="row-actions">
                        {canAuthor && (
                          <Link to={`/trainings/${trainingId}/content/${m.id}/edit`}>
                            <button type="button" className="secondary">
                              Edit
                            </button>
                          </Link>
                        )}
                        {canAuthor && (
                          <button className="danger" onClick={() => handleDeleteMaterial(m.id)}>
                            Delete
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}
        </>
      )}
    </div>
  );
}
