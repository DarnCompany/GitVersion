using System.Text.RegularExpressions;
using GitVersion.Configuration;
using GitVersion.Extensions;

namespace GitVersion;

internal class SourceBranchFinder
{
    private readonly GitVersionConfiguration configuration;
    private readonly IEnumerable<IBranch> excludedBranches;

    public SourceBranchFinder(IEnumerable<IBranch> excludedBranches, GitVersionConfiguration configuration)
    {
        this.excludedBranches = excludedBranches.NotNull();
        this.configuration = configuration.NotNull();
    }

    public IEnumerable<IBranch> FindSourceBranchesOf(IBranch branch)
    {
        var predicate = new SourceBranchPredicate(branch, this.configuration);
        return this.excludedBranches.Where(predicate.IsSourceBranch);
    }

    private class SourceBranchPredicate
    {
        private readonly IBranch branch;
        private readonly IEnumerable<string> sourceBranchRegexes;

        public SourceBranchPredicate(IBranch branch, GitVersionConfiguration configuration)
        {
            this.branch = branch;
            this.sourceBranchRegexes = GetSourceBranchRegexes(branch, configuration);
        }

        public bool IsSourceBranch(INamedReference sourceBranchCandidate)
        {
            if (Equals(sourceBranchCandidate, this.branch))
                return false;

            var branchName = sourceBranchCandidate.Name.WithoutOrigin;

            return this.sourceBranchRegexes.Any(regex => Regex.IsMatch(branchName, regex));
        }

        private static IEnumerable<string> GetSourceBranchRegexes(INamedReference branch, GitVersionConfiguration configuration)
        {
            var currentBranchConfig = configuration.GetBranchConfiguration(branch.Name);
            if (currentBranchConfig.SourceBranches == null)
            {
                yield return ".*";
            }
            else
            {
                var branches = configuration.Branches;
                foreach (var sourceBranch in currentBranchConfig.SourceBranches)
                {
                    var regex = branches[sourceBranch].RegularExpression;
                    if (regex != null)
                        yield return regex;
                }
            }
        }
    }
}
