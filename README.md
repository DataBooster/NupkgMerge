This is a utility for merging two NuGet packages _(two .NET Framework Versions)_ into a single nupkg package _(Multi-TargetFrameworks)_.

### +Usage+

**NupkgMerge**.exe **-p**{"[rimary](rimary)"} "+primary.nupkg+" **-s**{"[econd](econd)"} "+second.nupkg+" **-o**{"[ut](ut)"} "+output.nupkg+"

* **-P**{"[rimary](rimary)"}
: Specifies the primary nupkg file to merge from. _(Higher Priority)_
* **-S**{"[econd](econd)"}
: Specifies the second nupkg file to merge from. _(Lower Priority)_ Only those items_(metadata/files)_ which are not in the primary nupkg file will be included in the output nupkg file.
* **-O**{"[ut](ut)"}
: Specifies the file name of new nupkg package for the merged output.


To merge more than two packages into a single package, please execute NupkgMerge in iterations. _(Pass the previous output file into the next iteration's primary file argument)_

For source package created from a **csproj** or **vbproj** file, the auto-generated flat list of package dependencies will be **grouped** into a corresponding targetFramework group _(if the source package supports a unique .NET Framework)_ while merging.
