// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://nupkgmerge.codeplex.com/

using System.Linq;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace NuGetPackageMerge
{
	class CmdArguments
	{
		[Option('p', "primary", Required = true, HelpText = "\tSpecifies the primary .nupkg file.")]
		public string PrimaryNupkg { get; set; }

		[Option('s', "second", Required = true, HelpText = "\tSpecifies the second .nupkg file.")]
		public string SecondNupkg { get; set; }

		[Option('o', "out", Required = true, HelpText = "\tSpecifies the output .nupkg filename.")]
		public string OutputNupkg { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this,
			  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}

		public bool Parse(string[] args)
		{
			NormalizeArguments(args);
			return Parser.Default.ParseArguments(args, this);
		}

		private static void NormalizeArguments(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith("/"))
					args[i] = "-" + args[i].TrimStart('/');

				if (args[i].StartsWith("-") && args[i].Length > 2)
					if (args[i][1] != '-' && args[i][2] != '"')
						args[i] = "-" + args[i];
			}
		}
	}
}
