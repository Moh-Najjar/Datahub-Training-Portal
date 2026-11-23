import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { trainingsApi } from "../api/trainings";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/ErrorBanner";
import { AssignmentsSection } from "../components/AssignmentsSection";
import { useAuth } from "../context/AuthContext";
import { canAuthorTrainings } from "../utils/permissions";
import type { TrainingDto } from "../types";

export function TrainingAssignmentsPage() {
  const { trainingId } = useParams<{ trainingId: string }>();
  const { user } = useAuth();
  const canAuthor = canAuthorTrainings(user!.role);

  const [training, setTraining] = useState<TrainingDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!trainingId) return;
    setIsLoading(true);
    setError(null);
    trainingsApi
      .getById(trainingId)
      .then(setTraining)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load training."))
      .finally(() => setIsLoading(false));
  }, [trainingId]);

  if (isLoading) return <p>Loading...</p>;

  return (
    <div>
      <Link to={`/trainings/${trainingId}`} className="back-link">
        &larr; Back to training
      </Link>

      <ErrorBanner message={error} />

      {training && trainingId && (
        <>
          <div className="page-header">
            <h1>{training.title}</h1>
          </div>

          <AssignmentsSection trainingId={trainingId} canAuthor={canAuthor} />
        </>
      )}
    </div>
  );
}
