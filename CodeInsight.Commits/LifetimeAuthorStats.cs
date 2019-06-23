﻿using System;
using NodaTime;
using Octokit;
using Commit = CodeInsight.Domain.Commit.Commit;

namespace CodeInsight.Commits
{
    public class LifetimeAuthorStats
    {
        public string AuthorName { get; }

        public uint Additions { get; }

        public uint Deletions { get; }

        public uint CodeChangeDiff { get { return Additions - Deletions; } }

        public Instant LastCommitAt { get; }

        public Instant FirstCommitAt { get; }

        public Duration TimeSpendOnProject { get { return LastCommitAt - FirstCommitAt; } }

        public LifetimeAuthorStats (
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

        public static LifetimeAuthorStats Combine(LifetimeAuthorStats a, LifetimeAuthorStats b)
        {
            return new LifetimeAuthorStats(
                a.AuthorName,
                a.Additions+b.Additions,
                a.Deletions+b.Deletions,
                Instant.Max(a.LastCommitAt, b.LastCommitAt),
                Instant.Min(a.FirstCommitAt, b.FirstCommitAt) 
                );
        }
    }
}