import { QueryClient } from '@tanstack/react-query';

// Singleton: imported wherever a query/mutation needs the client, instead of each file
// constructing its own (which would give every component a separate, disconnected cache).
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
    },
  },
});
