import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { canManageDepartments, canManageEnrollments, canManageEvaluations, canManageUsers } from "../utils/permissions";
import { Logo } from "./Logo";

export function Layout() {
  const { user, logout } = useAuth();

  if (!user) return null;

  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="app-header-title">
          <Logo height={26} />
          <span className="app-header-product">Training Portal</span>
        </div>
        <nav className="app-nav">
          <NavLink to="/trainings">Trainings</NavLink>
          {canManageEnrollments(user.role) && <NavLink to="/enrollments">Enrollments</NavLink>}
          {canManageEvaluations(user.role) && <NavLink to="/rankings">Rankings</NavLink>}
          {canManageUsers(user.role) && <NavLink to="/users">Users</NavLink>}
          {canManageDepartments(user.role) && <NavLink to="/departments">Departments</NavLink>}
        </nav>
        <div className="app-header-user">
          <span>
            {user.fullName} <em>({user.role})</em>
          </span>
          <button onClick={logout}>Log out</button>
        </div>
      </header>
      <main className="app-content">
        <Outlet />
      </main>
    </div>
  );
}
