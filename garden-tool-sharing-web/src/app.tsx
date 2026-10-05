import { Route, Routes } from 'react-router-dom';
import { AuthGuard } from './layouts/AuthGuard';
import { RootLayout } from './layouts/RootLayout';
import { LoginPage } from './pages/LoginPage';
import { useAuth } from './contexts/AuthContext';

// Temporary stand-in for "/" until the Dashboard slice replaces it - proves the full
// login -> protected route -> logout loop works end to end.
function Home() {
  const { user, logout } = useAuth();
  return (
    <main style={{ padding: '2rem', fontFamily: 'system-ui, sans-serif' }}>
      <h1>Garden Tool Sharing</h1>
      <p>
        Signed in as {user?.displayName} ({user?.email}).
      </p>
      <button onClick={logout}>Log out</button>
    </main>
  );
}

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<RootLayout />}>
        <Route element={<AuthGuard />}>
          <Route path="/" element={<Home />} />
        </Route>
      </Route>
    </Routes>
  );
}
