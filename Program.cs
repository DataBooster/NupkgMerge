// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://nupkgmerge.codeplex.com/

using CommandLine;

namespace NuGetPackageMerge
{
	class Program
	{
		static void Main(string[] args)
		{
			CmdArguments cmdArguments = new CmdArguments();

			if (Parser.Default.ParseArguments(args, cmdArguments))
			{
				NupkgMerge nupkgMerge = new NupkgMerge(cmdArguments.PrimaryNupkg);
				nupkgMerge.Merge(cmdArguments.SecondNupkg);
				nupkgMerge.Save(cmdArguments.OutputNupkg);
			}
		}
	}
}
