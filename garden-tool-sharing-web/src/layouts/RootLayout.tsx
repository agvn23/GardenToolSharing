import { Outlet } from 'react-router-dom';

// Deliberately bare for now - just renders child routes. Real nav/header chrome is added
// once there's an actual Dashboard to navigate to, rather than designing it against nothing.
export function RootLayout() {
  return <Outlet />;
}
