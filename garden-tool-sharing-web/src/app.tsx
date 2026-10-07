import { Navigate, Route, Routes } from 'react-router-dom';
import { AuthGuard } from './layouts/AuthGuard';
import { RootLayout } from './layouts/RootLayout';
import { LoginPage } from './pages/LoginPage';
import { ToolsPage } from './pages/ToolsPage';
import { NewToolPage } from './pages/NewToolPage';
import { LeaderboardPage } from './pages/LeaderboardPage';
import { RequestsPage } from './pages/RequestsPage';


// Temporary stand-in for "/" until the Dashboard slice replaces it - proves the full
// login -> protected route -> logout loop works end to end.


export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<AuthGuard />}>
        <Route element={<RootLayout />}>
          <Route path="/" element={<Navigate to="/tools" replace />} />
          <Route path="/tools" element={<ToolsPage />} />
          <Route path="/tools/new" element={<NewToolPage />} />
          <Route path="/leaderboard" element={<LeaderboardPage />} />
          <Route path="/requests" element={<RequestsPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/tools" replace />} />
    </Routes>
  );
}
