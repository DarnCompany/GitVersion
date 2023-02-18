using Common.Utilities;

namespace Docker.Tasks;

[TaskName(nameof(DockerTest))]
[TaskDescription("Test the docker images containing the GitVersion Tool")]
[TaskArgument(Arguments.DockerRegistry, Constants.DockerHub, Constants.GitHub)]
[TaskArgument(Arguments.DockerDotnetVersion, Constants.Version60, Constants.Version70)]
[TaskArgument(Arguments.DockerDistro, Constants.Alpine315, Constants.Debian11, Constants.Ubuntu2204)]
[TaskArgument(Arguments.Architecture, Constants.Amd64, Constants.Arm64)]
[IsDependentOn(typeof(DockerBuild))]
public class DockerTest : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        var shouldRun = true;
        shouldRun &= context.ShouldRun(context.IsDockerOnLinux, $"{nameof(DockerTest)} works only on Docker on Linux agents.");

        return shouldRun;
    }

    public override void Run(BuildContext context)
    {
        foreach (var dockerImage in context.Images)
        {
            if (context.SkipImageForDocker(dockerImage)) continue;
            context.DockerTestImage(dockerImage);
        }
    }
}
