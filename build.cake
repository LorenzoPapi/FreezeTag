#addin "nuget:?package=SharpZipLib&Version=1.3.0"
#addin "nuget:?package=Cake.Compression&Version=0.2.4"
#addin "nuget:?package=Cake.FileHelpers&Version=3.3.0"


var buildId = EnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? "0";
var buildVersion = EnvironmentVariable("IMPOSTOR_VERSION") ?? "1.0.0";
var buildBranch = EnvironmentVariable("APPVEYOR_REPO_BRANCH") ?? "dev";
var buildDir = MakeAbsolute(Directory("./build"));

var prNumber = EnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER");
var target = Argument("target", "Deploy");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// UTILS
//////////////////////////////////////////////////////////////////////

// Remove unnecessary files for packaging.
private void ImpostorPublish(string name, string project, string runtime, bool isServer = false) {
    var projBuildDir = buildDir.Combine(name + "_" + runtime);
    var projBuildName = name + "_" + buildVersion + "_" + runtime;

    DotNetCorePublish(project, new DotNetCorePublishSettings {
        Configuration = configuration,
        NoRestore = true,
        Framework = "net5.0",
        Runtime = runtime,
        SelfContained = false,
        PublishSingleFile = true,
        PublishTrimmed = false,
        OutputDirectory = projBuildDir
    });

    if (isServer) {
        CreateDirectory(projBuildDir.Combine("plugins"));
        CreateDirectory(projBuildDir.Combine("libraries"));

        if (runtime == "win-x64") {
            FileWriteText(projBuildDir.CombineWithFilePath("run.bat"), "@echo off\r\nImpostor.Server.exe\r\npause");
        }
    }

    if (runtime == "win-x64") {
        Zip(projBuildDir, buildDir.CombineWithFilePath(projBuildName + ".zip"));
    } else {
        GZipCompress(projBuildDir, buildDir.CombineWithFilePath(projBuildName + ".tar.gz"));
    }
}

private void ImpostorPublishNF(string name, string project) {
    var runtime = "win-x64";
    var projBuildDir = buildDir.Combine(name + "_" + runtime);
    var projBuildZip = buildDir.CombineWithFilePath(name + "_" + buildVersion + "_" + runtime + ".zip");

    DotNetCorePublish(project, new DotNetCorePublishSettings {
        Configuration = configuration,
        NoRestore = true,
        Framework = "net472",
        OutputDirectory = projBuildDir
    });

    Zip(projBuildDir, projBuildZip);
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() => {
        if (DirectoryExists(buildDir)) {
            DeleteDirectory(buildDir, new DeleteDirectorySettings {
                Recursive = true
            });
        }
    });

Task("Restore")
    .Does(() => {
        DotNetCoreRestore("FreezeTag.sln");
    });

Task("Patch")
    .WithCriteria(BuildSystem.AppVeyor.IsRunningOnAppVeyor)
    .Does(() => {
        ReplaceRegexInFiles("./**/*.csproj", @"<Version>.*?<\/Version>", "<Version>" + buildVersion + "</Version>");
        ReplaceRegexInFiles("./**/*.props", @"<Version>.*?<\/Version>", "<Version>" + buildVersion + "</Version>");
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Patch")
    .IsDependentOn("Restore")
    .Does(() => {
        // Only build artifacts if;
        // - buildBranch is master/dev
        // - it is not a pull request
        if (buildBranch == "master" && string.IsNullOrEmpty(prNumber)) {
            // Server.
            ImpostorPublish("Impostor-Server", "./Impostor.Server/Impostor.Server.csproj", "win-x64", true);
            ImpostorPublish("Impostor-Server", "./Impostor.Server/Impostor.Server.csproj", "osx-x64", true);
            ImpostorPublish("Impostor-Server", "./Impostor.Server/Impostor.Server.csproj", "linux-x64", true);
            ImpostorPublish("Impostor-Server", "./Impostor.Server/Impostor.Server.csproj", "linux-arm", true);
            ImpostorPublish("Impostor-Server", "./Impostor.Server/Impostor.Server.csproj", "linux-arm64", true);
        }
    });

Task("Deploy")
    .IsDependentOn("Build")
    .Does(() => {
        Information("Finished.");
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);