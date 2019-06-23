﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CodeInsight.Domain.Commit;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using FuncSharp;
using NodaTime;

namespace CodeInsight.Commits
{
    public sealed class WeekStats
    {
        private readonly DataCube1<LocalDate, IImmutableSet<Commit>> Data;

        public WeekStats(
            DataCube1<LocalDate, IImmutableSet<Commit>> data
            )
        {
            this.Data = data;
        }

        public DataCube1<LocalDate, T> Map<T>(Func<OverTimeStats, T> project) =>
            Data.Map(ToStats).Map(project);

        public IOption<OverTimeStats> Get(LocalDate date) =>
            Data.Get(date).Map(ToStats);

        private OverTimeStats ToStats(IEnumerable<Commit> commits) =>
            commits
                .Select(c => OverTimeStats.FromCommits(c))
                .Aggregate(OverTimeStats.Combine);
    }
}