using System.Collections.Generic;
using CodeInsight.Library.DatePicker;
using CodeInsight.PullRequests;
using CodeInsight.Web.Common.Charts;
using FuncSharp;

namespace CodeInsight.Web.Models.PullRequest
{
    public sealed class PullRequestStatisticsViewModel : ChartsViewModel
    {        
        public PullRequestStatisticsViewModel(
            IntervalStatisticsConfiguration configuration,
            IReadOnlyList<Domain.PullRequest.PullRequest> pullRequests,
            IOption<string> error,
            IReadOnlyList<Chart> charts) 
            : base(charts)
        {
            Configuration = configuration;
            PullRequests = pullRequests;
            Error = error;
        }

        public IntervalStatisticsConfiguration Configuration { get; }
        
        public IReadOnlyList<Domain.PullRequest.PullRequest> PullRequests { get; }
        
        public IOption<string> Error { get; }
    }
}