import { Navigate, Route, Routes } from "react-router-dom";
import { Layout } from "./components/Layout";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { LoginPage } from "./pages/LoginPage";
import { DepartmentsPage } from "./pages/DepartmentsPage";
import { UsersPage } from "./pages/UsersPage";
import { TrainingsPage } from "./pages/TrainingsPage";
import { TrainingDetailPage } from "./pages/TrainingDetailPage";
import { TrainingContentPage } from "./pages/TrainingContentPage";
import { MaterialEditPage } from "./pages/MaterialEditPage";
import { TrainingAssignmentsPage } from "./pages/TrainingAssignmentsPage";
import { AssignmentSubmissionsPage } from "./pages/AssignmentSubmissionsPage";
import { MaterialTextPage } from "./pages/MaterialTextPage";
import { EnrollmentsPage } from "./pages/EnrollmentsPage";
import { RankingsPage } from "./pages/RankingsPage";
import { UserEvaluationsPage } from "./pages/UserEvaluationsPage";

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          <Route index element={<Navigate to="/trainings" replace />} />
          <Route path="departments" element={<DepartmentsPage />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="trainings" element={<TrainingsPage />} />
          <Route path="trainings/:trainingId" element={<TrainingDetailPage />} />
          <Route path="trainings/:trainingId/content" element={<TrainingContentPage />} />
          <Route path="trainings/:trainingId/content/:materialId/edit" element={<MaterialEditPage />} />
          <Route path="trainings/:trainingId/assignments" element={<TrainingAssignmentsPage />} />
          <Route
            path="trainings/:trainingId/assignments/:assignmentId/submissions"
            element={<AssignmentSubmissionsPage />}
          />
          <Route
            path="trainings/:trainingId/materials/:materialId"
            element={<MaterialTextPage />}
          />
          <Route path="enrollments" element={<EnrollmentsPage />} />
          <Route path="rankings" element={<RankingsPage />} />
          <Route path="users/:userId/evaluations" element={<UserEvaluationsPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
