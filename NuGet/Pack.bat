@ECHO OFF
CD /D %~dp0

IF NOT EXIST nupkg MKDIR nupkg
..\.nuget\NuGet.exe pack ..\NuGetPackageMerge.csproj -Tool -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg %*
