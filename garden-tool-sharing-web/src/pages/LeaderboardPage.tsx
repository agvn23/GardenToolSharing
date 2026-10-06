import { useEffect, useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { getLeaderboard, type LeaderboardEntry } from '../data/leaderboard';
import { errorMessage } from '../utils/problemDetails';

export function LeaderboardPage() {
  const { user } = useAuth();
  const [entries, setEntries] = useState<LeaderboardEntry[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getLeaderboard()
      .then((data) => { if (!cancelled) setEntries(data); })
      .catch((err) => { if (!cancelled) setError(errorMessage(err)); });
    return () => { cancelled = true; };
  }, []);

  return (
    <section>
      <h1>Top lenders</h1>
      <p style={{ color: '#555' }}>
        Ranked by the number of times each person's tools have been lent out.
      </p>

      {error && <p role="alert">{error}</p>}
      {!error && !entries && <p>Loading leaderboard…</p>}
      {!error && entries && entries.length === 0 && <p>No loans yet, so nobody is on the board.</p>}
      {!error && entries && entries.length > 0 && (
        <table style={{ borderCollapse: 'collapse', minWidth: '300px' }}>
          <thead>
            <tr>
              <th style={cell}>Rank</th>
              <th style={{ ...cell, textAlign: 'left' }}>Lender</th>
              <th style={cell}>Loans</th>
            </tr>
          </thead>
          <tbody>
            {entries.map((entry) => {
              const isMe = user?.id === entry.ownerId;
              return (
                <tr key={entry.ownerId} style={isMe ? { background: '#e7f3ec', fontWeight: 600 } : undefined}>
                  <td style={cell}>{entry.rank}</td>
                  <td style={{ ...cell, textAlign: 'left' }}>
                    {entry.ownerName}{isMe && ' (you)'}
                  </td>
                  <td style={cell}>{entry.totalLoans}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
    </section>
  );
}

const cell = { padding: '0.4rem 0.9rem', borderBottom: '1px solid #ddd', textAlign: 'center' } as const;