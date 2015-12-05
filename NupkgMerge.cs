// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://nupkgmerge.codeplex.com/

using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NuGet;

namespace NuGetPackageMerge
{
	class NupkgMerge
	{
		private readonly PackageBuilder _Builder;

		public NupkgMerge(string primaryNupkg)
		{
			_Builder = new PackageBuilder();
			Merge(primaryNupkg, true);
		}

		private static void ReplaceMetadata(PackageBuilder builder, IPackage primarySource)
		{
			if (!string.IsNullOrEmpty(primarySource.Id))
				builder.Id = primarySource.Id;
			if (primarySource.Version != null)
				builder.Version = primarySource.Version;
			if (!string.IsNullOrEmpty(primarySource.Title))
				builder.Title = primarySource.Title;
			if (primarySource.IconUrl != null)
				builder.IconUrl = primarySource.IconUrl;
			if (primarySource.LicenseUrl != null)
				builder.LicenseUrl = primarySource.LicenseUrl;
			if (primarySource.ProjectUrl != null)
				builder.ProjectUrl = primarySource.ProjectUrl;
			if (primarySource.RequireLicenseAcceptance)
				builder.RequireLicenseAcceptance = primarySource.RequireLicenseAcceptance;
			if (primarySource.DevelopmentDependency)
				builder.DevelopmentDependency = primarySource.DevelopmentDependency;
			if (!string.IsNullOrEmpty(primarySource.Description))
				builder.Description = primarySource.Description;
			if (!string.IsNullOrEmpty(primarySource.Summary))
				builder.Summary = primarySource.Summary;
			if (!string.IsNullOrEmpty(primarySource.ReleaseNotes))
				builder.ReleaseNotes = primarySource.ReleaseNotes;
			if (!string.IsNullOrEmpty(primarySource.Copyright))
				builder.Copyright = primarySource.Copyright;
			if (!string.IsNullOrEmpty(primarySource.Language))
				builder.Language = primarySource.Language;
			if (primarySource.MinClientVersion != null)
				builder.MinClientVersion = primarySource.MinClientVersion;
			if (!string.IsNullOrEmpty(primarySource.Tags))
				builder.Tags.AddRange(SplitTags(primarySource.Tags));
			builder.Authors.AddRange(primarySource.Authors.Except(builder.Authors));
			builder.Owners.AddRange(primarySource.Owners.Except(builder.Owners));

			MergeDependencySets(builder.DependencySets, primarySource);
			MergeFrameworkReferences(builder.FrameworkReferences, primarySource.FrameworkAssemblies);
			MergePackageAssemblyReferences(builder.PackageAssemblyReferences, primarySource.PackageAssemblyReferences);
		}

		private static void MergeMetadata(PackageBuilder builder, IPackage secondSource)
		{
			if (string.IsNullOrEmpty(builder.Id) && !string.IsNullOrEmpty(secondSource.Id))
				builder.Id = secondSource.Id;
			if (builder.Version == null && secondSource.Version != null)
				builder.Version = secondSource.Version;
			if (string.IsNullOrEmpty(builder.Title) && !string.IsNullOrEmpty(secondSource.Title))
				builder.Title = secondSource.Title;
			if (builder.IconUrl == null && secondSource.IconUrl != null)
				builder.IconUrl = secondSource.IconUrl;
			if (builder.LicenseUrl == null && secondSource.LicenseUrl != null)
				builder.LicenseUrl = secondSource.LicenseUrl;
			if (builder.ProjectUrl == null && secondSource.ProjectUrl != null)
				builder.ProjectUrl = secondSource.ProjectUrl;
			if (!builder.RequireLicenseAcceptance && secondSource.RequireLicenseAcceptance)
				builder.RequireLicenseAcceptance = secondSource.RequireLicenseAcceptance;
			if (!builder.DevelopmentDependency && secondSource.DevelopmentDependency)
				builder.DevelopmentDependency = secondSource.DevelopmentDependency;
			if (string.IsNullOrEmpty(builder.Description) && !string.IsNullOrEmpty(secondSource.Description))
				builder.Description = secondSource.Description;
			if (string.IsNullOrEmpty(builder.Summary) && !string.IsNullOrEmpty(secondSource.Summary))
				builder.Summary = secondSource.Summary;
			if (string.IsNullOrEmpty(builder.ReleaseNotes) && !string.IsNullOrEmpty(secondSource.ReleaseNotes))
				builder.ReleaseNotes = secondSource.ReleaseNotes;
			if (string.IsNullOrEmpty(builder.Copyright) && !string.IsNullOrEmpty(secondSource.Copyright))
				builder.Copyright = secondSource.Copyright;
			if (string.IsNullOrEmpty(builder.Language) && !string.IsNullOrEmpty(secondSource.Language))
				builder.Language = secondSource.Language;
			if (builder.MinClientVersion == null && secondSource.MinClientVersion != null)
				builder.MinClientVersion = secondSource.MinClientVersion;
			if (!string.IsNullOrEmpty(secondSource.Tags))
				builder.Tags.AddRange(SplitTags(secondSource.Tags).Except(builder.Tags));
			builder.Authors.AddRange(secondSource.Authors.Except(builder.Authors));
			builder.Owners.AddRange(secondSource.Owners.Except(builder.Owners));

			MergeDependencySets(builder.DependencySets, secondSource);
			MergeFrameworkReferences(builder.FrameworkReferences, secondSource.FrameworkAssemblies);
			MergePackageAssemblyReferences(builder.PackageAssemblyReferences, secondSource.PackageAssemblyReferences);
		}

		private static void MergeDependencySets(Collection<PackageDependencySet> dependencySets, IPackage secondSource)
		{
			var secondDependencies = TargetDependencies(secondSource);

			if (dependencySets.Count == 0)
				dependencySets.AddRange(secondDependencies);
			else
			{
				PackageDependencySet oldDepSet;

				foreach (var newDepSet in secondDependencies)
				{
					oldDepSet = dependencySets.FirstOrDefault(d => d.TargetFramework == newDepSet.TargetFramework);

					if (oldDepSet == null)
						dependencySets.Add(newDepSet);
					else
					{
						foreach (var dep in newDepSet.Dependencies)
							if (!oldDepSet.Dependencies.Any(d => d.Id.Equals(dep.Id, StringComparison.OrdinalIgnoreCase)))
								oldDepSet.Dependencies.Add(dep);
					}
				}
			}
		}

		private static IEnumerable<PackageDependencySet> TargetDependencies(IPackage source)
		{
			List<FrameworkName> supportedFrameworks = source.GetSupportedFrameworks().ToList();

			var dependencySets = source.DependencySets.Where(d => d.Dependencies.Count > 0);

			if (supportedFrameworks.Count == 1)
				return dependencySets.Select(d => TargetDependency(d, supportedFrameworks[0]));
			else
				return dependencySets;
		}

		private static PackageDependencySet TargetDependency(PackageDependencySet packageDependencySet, FrameworkName targetFramework)
		{
			if (packageDependencySet.TargetFramework == null && packageDependencySet.Dependencies.Count > 0)
				return new PackageDependencySet(targetFramework, packageDependencySet.Dependencies);
			else
				return packageDependencySet;
		}

		private static void MergeFrameworkReferences(Collection<FrameworkAssemblyReference> frameworkReferences, IEnumerable<FrameworkAssemblyReference> secondFrameworkReferences)
		{
			if (frameworkReferences.Count == 0)
				frameworkReferences.AddRange(secondFrameworkReferences);
			else
			{
				foreach (var fr in secondFrameworkReferences)
					if (!frameworkReferences.Any(f => f.AssemblyName.Equals(fr.AssemblyName, StringComparison.OrdinalIgnoreCase)))
						frameworkReferences.Add(fr);
			}
		}

		private static void MergePackageAssemblyReferences(ICollection<PackageReferenceSet> packageAssemblyReferences, IEnumerable<PackageReferenceSet> secondPackageAssemblyReferences)
		{
			if (packageAssemblyReferences.Count == 0)
				packageAssemblyReferences.AddRange(secondPackageAssemblyReferences);
			else
			{
				PackageReferenceSet oldPrSet;

				foreach (var newPrSet in secondPackageAssemblyReferences)
				{
					oldPrSet = packageAssemblyReferences.FirstOrDefault(p => p.TargetFramework == newPrSet.TargetFramework);

					if (oldPrSet == null)
						packageAssemblyReferences.Add(newPrSet);
					else
					{
						foreach (var r in newPrSet.References)
							if (!oldPrSet.References.Contains(r, StringComparer.OrdinalIgnoreCase))
								oldPrSet.References.Add(r);
					}
				}
			}
		}

		public void Merge(string secondNupkg, bool replaceMetadata = false)
		{
			var secondPackage = new ZipPackage(secondNupkg);

			if (replaceMetadata)
				ReplaceMetadata(_Builder, secondPackage);
			else
				MergeMetadata(_Builder, secondPackage);

			foreach (var packageFile in secondPackage.GetFiles())
				if (!_Builder.Files.Any(e => e.Path.Equals(packageFile.Path, StringComparison.OrdinalIgnoreCase)))
					_Builder.Files.Add(packageFile);
		}

		public void Save(string outputNupkg)
		{
			using (Stream stream = File.Create(outputNupkg))
			{
				_Builder.Save(stream);
			}
		}

		private static IEnumerable<string> SplitTags(string tags)
		{
			return (tags == null) ? Enumerable.Empty<string>() : tags.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
