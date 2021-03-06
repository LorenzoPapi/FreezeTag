#addin "nuget:?package=SharpZipLib&Version=1.3.0"
#addin "nuget:?package=Cake.Compression&Version=0.2.4"
#addin "nuget:?package=Cake.FileHelpers&Version=3.3.0"

var buildId = EnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? "0";
var buildDir = MakeAbsolute(Directory("./build"));

private void FreezeTag() {
    var projBuildDir = buildDir.Combine("FreezeTag");
    var projBuildName = "FreezeTag_" + buildId;

    DotNetCorePublish("./FreezeTag/FreezeTag.csproj", new DotNetCorePublishSettings {
        Configuration = Argument("configuration", "Release"),
        SelfContained = true,
        OutputDirectory = projBuildDir
    });

    MoveFile(projBuildDir + "/FreezeTag.dll", buildDir + "/FreezeTag.dll");
}

private void ServerPublish(string runtime) {
    var projBuildDir = buildDir.Combine("Impostor-Server_" + runtime);
    var projBuildName = "Impostor-Server_1.2.2_" + buildId + "_" + runtime;

    DotNetCorePublish("./Impostor.Server/Impostor.Server.csproj", new DotNetCorePublishSettings {
        Configuration = Argument("configuration", "Release"),
        NoRestore = true,
        Framework = "net5.0",
        Runtime = runtime,
        SelfContained = false,
        PublishSingleFile = true,
        PublishTrimmed = false,
        OutputDirectory = projBuildDir
    });

    CreateDirectory(projBuildDir.Combine("plugins"));
    CreateDirectory(projBuildDir.Combine("libraries"));

    if (runtime == "win-x64") {
        FileWriteText(projBuildDir.CombineWithFilePath("run.bat"), "@echo off\r\nImpostor.Server.exe\r\npause");
        Zip(projBuildDir, buildDir.CombineWithFilePath(projBuildName + ".zip"));
    } else {
        GZipCompress(projBuildDir, buildDir.CombineWithFilePath(projBuildName + ".tar.gz"));
    }
}

Task("Build")
    .Does(() => {
        if (DirectoryExists(buildDir)) {
            DeleteDirectory(buildDir, new DeleteDirectorySettings {
                Recursive = true
            });
        }
        DotNetCoreRestore("FreezeTag.sln");
        FreezeTag();
        ServerPublish("win-x64");
        ServerPublish("osx-x64");
        ServerPublish("linux-x64");
        ServerPublish("linux-arm");
        ServerPublish("linux-arm64");
        Information("Finished building.");
    });

RunTarget(Argument("target", "Build"));