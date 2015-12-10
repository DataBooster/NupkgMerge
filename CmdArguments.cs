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
		[Option('p', "primary", Required = true, HelpText = "\tSpecifies primary .nupkg file.")]
		public string PrimaryNupkg { get; set; }

		[Option('s', "second", Required = true, HelpText = "\tSpecifies second .nupkg file.")]
		public string SecondNupkg { get; set; }

		[Option('o', "out", Required = true, HelpText = "\tSpecifies output .nupkg file name.")]
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
			return Parser.Default.ParseArguments(NormalizeArguments(args).ToArray(), this);
		}

		private static char[] _nameValueSeparators = new char[] { ':', '=' };

		private static IEnumerable<string> NormalizeArguments(string[] args)
		{
			string arg;
			int delimit;

			for (int i = 0; i < args.Length; i++)
			{
				arg = args[i];

				if (arg.StartsWith("/"))
					arg = "-" + arg.TrimStart('/');

				if (arg.StartsWith("-") && arg.Length > 2)
				{
					if (arg[1] != '-')
						arg = "-" + arg;

					delimit = arg.IndexOfAny(_nameValueSeparators, 2);
					if (delimit > 1 && delimit < arg.Length - 1)
					{
						yield return arg.Substring(0, delimit);
						yield return arg.Substring(delimit + 1);
						continue;
					}
				}

				yield return arg;
			}
		}
	}
}
