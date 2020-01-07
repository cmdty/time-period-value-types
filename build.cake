var target = Argument<string>("Target", "Default");
var configuration = Argument<string>("Configuration", "Release");
bool publishWithoutBuild = Argument<bool>("PublishWithoutBuild", false);
string nugetPrereleaseTextPart = Argument<string>("PrereleaseText", "alpha");

var artifactsDirectory = Directory("./artifacts");
var samplesDirectory = Directory("./samples");
string testResultDir = "./temp/";
var isRunningOnBuildServer = !BuildSystem.IsLocalBuild;

var msBuildSettings = new DotNetCoreMSBuildSettings();

if (HasArgument("PrereleaseNumber"))
{
    msBuildSettings.WithProperty("PrereleaseNumber", Argument<string>("PrereleaseNumber"));
    msBuildSettings.WithProperty("VersionSuffix", nugetPrereleaseTextPart + Argument<string>("PrereleaseNumber"));
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

Task("Add-NuGetSource")
    .Does(() =>
    {
		if (isRunningOnBuildServer)
		{
			// Get the access token
			string accessToken = EnvironmentVariable("SYSTEM_ACCESSTOKEN");
			if (string.IsNullOrEmpty(accessToken))
			{
				throw new InvalidOperationException("Could not resolve SYSTEM_ACCESSTOKEN.");
			}

			NuGetRemoveSource("Cmdty", "https://pkgs.dev.azure.com/cmdty/_packaging/cmdty/nuget/v3/index.json");

			// Add the authenticated feed source
			NuGetAddSource(
				"Cmdty",
				"https://pkgs.dev.azure.com/cmdty/_packaging/cmdty/nuget/v3/index.json",
				new NuGetSourcesSettings
				{
					UserName = "VSTS",
					Password = accessToken
				});
		}
		else
		{
			Information("Not running on build so no need to add Cmdty NuGet source");
		}
    });

Task("Build-Samples")
    .IsDependentOn("Add-NuGetSource")
	.Does(() =>
{
	var dotNetCoreSettings = new DotNetCoreBuildSettings()
        {
            Configuration = configuration,
        };
	DotNetCoreBuild("samples/Cmdty.TimePeriodValueTypes.Samples.sln", dotNetCoreSettings);
});

Task("Verify-TryDotNetDocs")
    .IsDependentOn("Build-Samples")
	.Does(() =>
{
	StartProcessThrowOnError("dotnet", $"try verify {samplesDirectory}");
});

private void StartProcessThrowOnError(string applicationName, params string[] processArgs)
{
    var argsBuilder = new ProcessArgumentBuilder();
    foreach(string processArg in processArgs)
    {
        argsBuilder.Append(processArg);
    }
    int exitCode = StartProcess(applicationName, new ProcessSettings {Arguments = argsBuilder});
    if (exitCode != 0)
        throw new ApplicationException($"Starting {applicationName} in new process returned non-zero exit code of {exitCode}");
}

using System.Reflection;
private string GetAssemblyVersion(string configuration, string workingDirectory)
{
	// TODO find better way of doing this!
	string assemblyPath = System.IO.Path.Combine(workingDirectory, "src", "Cmdty.TimePeriodValueTypes", "bin", configuration, "netstandard2.0", "Cmdty.TimePeriodValueTypes.dll");
	Assembly assembly = Assembly.LoadFrom(assemblyPath);
	string productVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
	return productVersion;
}

Task("Pack-NuGet")
	.IsDependentOn("Build-Samples")
	.IsDependentOn("Test-C#")
	.IsDependentOn("Clean-Artifacts")
	.Does(() =>
{
	var dotNetPackSettings = new DotNetCorePackSettings()
                {
                    Configuration = configuration,
                    OutputDirectory = artifactsDirectory,
                    NoRestore = true,
                    MSBuildSettings = msBuildSettings
                };
	DotNetCorePack("src/Cmdty.TimePeriodValueTypes/Cmdty.TimePeriodValueTypes.csproj", dotNetPackSettings);
});

Task("Default")
	.IsDependentOn("Verify-TryDotNetDocs")
	.IsDependentOn("Pack-NuGet");

RunTarget(target);