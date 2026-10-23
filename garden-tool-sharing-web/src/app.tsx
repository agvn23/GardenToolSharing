import { Navigate, Route, Routes } from 'react-router-dom';
import { AuthGuard } from './layouts/AuthGuard';
import { RootLayout } from './layouts/RootLayout';
import { LoginPage } from './pages/LoginPage';
import { ToolsPage } from './pages/ToolsPage';
import { NewToolPage } from './pages/NewToolPage';
import { LeaderboardPage } from './pages/LeaderboardPage';
import { RequestsPage } from './pages/RequestsPage';
import { RequestAccessPage } from './pages/RequestAccessPage';
import { HiddenToolsPage } from './pages/HiddenToolsPage';
import { EditToolPage } from './pages/EditToolPage';
import { RegisterPage } from './pages/RegisterPage';
import { NoticeBoardPage } from './pages/NoticeBoardPage';
import { NoticePage } from './pages/NoticePage';


// Temporary stand-in for "/" until the Dashboard slice replaces it - proves the full
// login -> protected route -> logout loop works end to end.


export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route element={<AuthGuard />}>
        <Route element={<RootLayout />}>
          <Route path="/" element={<Navigate to="/tools" replace />} />
          <Route path="/tools" element={<ToolsPage />} />
          <Route path="/tools/new" element={<NewToolPage />} />
          <Route path="/leaderboard" element={<LeaderboardPage />} />
          <Route path="/requests" element={<RequestsPage />} />
          <Route path="/tools/:id/request" element={<RequestAccessPage />} />
          <Route path="/tools/hidden" element={<HiddenToolsPage />} />
          <Route path="/tools/:id/edit" element={<EditToolPage />} />
          <Route path="/notices" element={<NoticeBoardPage />} />
          <Route path="/notices/:id" element={<NoticePage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/tools" replace />} />
    </Routes>
  );
}
