// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://nupkgmerge.codeplex.com/

using System;
using CommandLine;

namespace NuGetPackageMerge
{
	class Program
	{
		static void Main(string[] args)
		{
			CmdArguments cmdArguments = new CmdArguments();

			if (cmdArguments.Parse(args))
			{
				NupkgMerge nupkgMerge = new NupkgMerge(cmdArguments.PrimaryNupkg);
				nupkgMerge.Merge(cmdArguments.SecondNupkg);
				nupkgMerge.Save(cmdArguments.OutputNupkg);

				Console.WriteLine("Successfully merged '{0}' with '{1}' into new package '{2}'.",
					cmdArguments.PrimaryNupkg, cmdArguments.SecondNupkg, cmdArguments.OutputNupkg);
			}
		}
	}
}
