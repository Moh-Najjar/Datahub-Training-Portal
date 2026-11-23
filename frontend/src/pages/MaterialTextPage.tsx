import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { materialsApi } from "../api/materials";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import type { TrainingMaterialDto } from "../types";
import "react-quill-new/dist/quill.snow.css";

export function MaterialTextPage() {
  const { trainingId, materialId } = useParams<{ trainingId: string; materialId: string }>();

  const [material, setMaterial] = useState<TrainingMaterialDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!trainingId || !materialId) return;

    setIsLoading(true);
    setError(null);
    materialsApi
      .getForTraining(trainingId)
      .then((materials) => {
        setMaterial(materials.find((m) => m.id === materialId) ?? null);
      })
      .catch((err) => {
        setError(err instanceof ApiError ? err.message : "Failed to load content.");
      })
      .finally(() => setIsLoading(false));
  }, [trainingId, materialId]);

  return (
    <div>
      <Link to={`/trainings/${trainingId}`} className="back-link">
        &larr; Back to training
      </Link>

      <ErrorBanner message={error} />

      {isLoading ? (
        <p>Loading...</p>
      ) : material ? (
        <article className="material-text-page">
          <h1>{material.title}</h1>
          <div className="material-text-body ql-container ql-snow">
            <div className="ql-editor" dangerouslySetInnerHTML={{ __html: material.textContent ?? "" }} />
          </div>
        </article>
      ) : (
        <p className="muted">This content could not be found.</p>
      )}
    </div>
  );
}
