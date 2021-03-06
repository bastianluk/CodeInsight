﻿using NodaTime;

namespace CodeInsight.Commits
{
    public class AuthorStats
    {
        public AuthorStats(
            string authorName,
            uint additions,
            uint deletions,
            Instant lastCommitAt,
            Instant firstCommitAt)
        {
            AuthorName = authorName;
            Additions = additions;
            Deletions = deletions;
            LastCommitAt = lastCommitAt;
            FirstCommitAt = firstCommitAt;
        }

        public string AuthorName { get; }

        public uint Additions { get; }

        public uint Deletions { get; }

        public int CodeChangeDiff => (int) Additions - (int) Deletions;

        public Instant LastCommitAt { get; }

        public Instant FirstCommitAt { get; }

        public Duration TimeSpentOnProject => LastCommitAt - FirstCommitAt;
        
        public static AuthorStats Combine(AuthorStats a, AuthorStats b)
        {
            return new AuthorStats(
                a.AuthorName,
                a.Additions+b.Additions,
                a.Deletions+b.Deletions,
                Instant.Max(a.LastCommitAt, b.LastCommitAt),
                Instant.Min(a.FirstCommitAt, b.FirstCommitAt) 
                );
        }
    }
}
