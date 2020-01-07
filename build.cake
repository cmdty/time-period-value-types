var target = Argument<string>("Target", "Default");
var configuration = Argument<string>("Configuration", "Release");
bool publishWithoutBuild = Argument<bool>("PublishWithoutBuild", false);
string nugetPrereleaseTextPart = Argument<string>("PrereleaseText", "alpha");

var artifactsDirectory = Directory("./artifacts");
string testResultDir = "./temp/";

var msBuildSettings = new DotNetCoreMSBuildSettings();

if (HasArgument("BuildNumber"))
{
    msBuildSettings.WithProperty("BuildNumber", Argument<string>("BuildNumber"));
    msBuildSettings.WithProperty("VersionSuffix", nugetPrereleaseTextPart + Argument<string>("BuildNumber"));
}

if (HasArgument("VersionPrefix"))
{
    msBuildSettings.WithProperty("VersionPrefix", Argument<string>("VersionPrefix"));
}

Task("Clean-Artifacts")
    .Does(() =>
{
    CleanDirectory(artifactsDirectory);
});

Task("Build")
    .Does(() =>
{
    var dotNetCoreSettings = new DotNetCoreBuildSettings()
            {
                Configuration = configuration,
                MSBuildSettings = msBuildSettings
            };
    DotNetCoreBuild("Cmdty.TimePeriodValueTypes.sln", dotNetCoreSettings);
});

Task("Test-C#")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("Cleaning test output directory");
    CleanDirectory(testResultDir);

    var projects = GetFiles("./tests/**/*.Test.csproj");
    
    foreach(var project in projects)
    {
        Information("Testing project " + project);
        DotNetCoreTest(
            project.ToString(),
            new DotNetCoreTestSettings()
            {
                ArgumentCustomization = args=>args.Append($"/p:CollectCoverage=true /p:CoverletOutputFormat=cobertura"),
                Logger = "trx",
                ResultsDirectory = testResultDir,
                Configuration = configuration,
                NoBuild = true
            });
    }
});


RunTarget(target);