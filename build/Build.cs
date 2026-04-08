using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using System;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions(
    "CI",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    InvokedTargets = new[] { nameof(Test) },
    AutoGenerate = false
)]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [Parameter] readonly bool IgnoreFailedSources = true;
    AbsolutePath AvaloniaProject => RootDirectory / "FastTelecom.AvaloniaUI" / "FastTelecom.AvaloniaUI.csproj";
    AbsolutePath PublishDir => RootDirectory / "publish";
    AbsolutePath ReleasesDir => RootDirectory / "releases";
    [Parameter("Version for Velopack packaging")]
    readonly string Version;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetClean(_ => _.SetProject(Solution));
            PublishDir.CreateOrCleanDirectory();
            ReleasesDir.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution)
                .SetIgnoreFailedSources(IgnoreFailedSources));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild());
        });

    Target Publish => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPublish(_ => _
                .SetProject(AvaloniaProject)
                .SetConfiguration(Configuration.Release)
                .SetRuntime("win-x64")
                .SetSelfContained(true)
                .SetOutput(PublishDir)
                .EnableNoRestore());
        });

    Target Pack => _ => _
        .DependsOn(Publish)
        .Requires(() => Version)
        .Executes(() =>
        {
            // Velopack package 
            ProcessTasks.StartProcess(
                "vpk",
                $"pack --packId FastTelecom --packVersion {Version} --packDir {PublishDir} --outputDir {ReleasesDir}",
                workingDirectory: RootDirectory
            ).AssertZeroExitCode();

            // Inno Setup installer
            var issFile = RootDirectory / "build" / "installer.iss";
            ProcessTasks.StartProcess(
                "iscc",
                $"/Qp /DMyAppVersion={Version} \"{issFile}\"",
                workingDirectory: RootDirectory
            ).AssertZeroExitCode();
        });
}
