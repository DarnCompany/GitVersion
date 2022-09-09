using GitVersion.Common;
using GitVersion.Configuration;
using GitVersion.Extensions;
using GitVersion.Model.Configuration;

namespace GitVersion.VersionCalculation;

/// <summary>
/// Version is extracted from the name of the branch.
/// BaseVersionSource is the commit where the branch was branched from its parent.
/// Does not increment.
/// </summary>
public class VersionInBranchNameVersionStrategy : VersionStrategyBaseWithInheritSupport
{
    public VersionInBranchNameVersionStrategy(IRepositoryStore repositoryStore, Lazy<GitVersionContext> versionContext)
        : base(repositoryStore, versionContext)
    {
    }

    public override IEnumerable<BaseVersion> GetVersions(IBranch branch, EffectiveConfiguration configuration)
    {
        string nameWithoutOrigin = NameWithoutOrigin(branch);
        if (Context.FullConfiguration.IsReleaseBranch(nameWithoutOrigin))
        {
            var versionInBranch = GetVersionInBranch(branch.Name.Friendly, Context.FullConfiguration.TagPrefix);
            if (versionInBranch != null)
            {
                var commitBranchWasBranchedFrom = RepositoryStore.FindCommitBranchWasBranchedFrom(branch, Context.FullConfiguration);
                var branchNameOverride = Context.CurrentBranch.Name.Friendly.RegexReplace("[-/]" + versionInBranch.Item1, string.Empty);
                yield return new BaseVersion("Version in branch name", false, versionInBranch.Item2, commitBranchWasBranchedFrom.Commit, branchNameOverride);
            }
        }
    }

    private static Tuple<string, SemanticVersion>? GetVersionInBranch(string branchName, string? tagPrefixRegex)
    {
        var branchParts = branchName.Split('/', '-');
        foreach (var part in branchParts)
        {
            if (SemanticVersion.TryParse(part, tagPrefixRegex, out var semanticVersion))
            {
                return Tuple.Create(part, semanticVersion);
            }
        }

        return null;
    }

    private static string NameWithoutOrigin(IBranch branch) => branch.IsRemote && branch.Name.Friendly.StartsWith("origin/")
        ? branch.Name.Friendly.Substring("origin/".Length)
        : branch.Name.Friendly;
}
