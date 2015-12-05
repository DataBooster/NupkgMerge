// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://nupkgmerge.codeplex.com/

using CommandLine;
using CommandLine.Text;

namespace NuGetPackageMerge
{
	class CmdArguments
	{
		[Option('p', "primary", Required = true, HelpText = "Primary .nupkg file.")]
		public string PrimaryNupkg { get; set; }

		[Option('s', "second", Required = true, HelpText = "Second .nupkg file.")]
		public string SecondNupkg { get; set; }

		[Option('o', "out", Required = true, HelpText = "Output .nupkg file name.")]
		public string OutputNupkg { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this,
			  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}
