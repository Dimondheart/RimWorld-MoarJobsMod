using System;
using System.Collections.Generic;
using System.IO;

namespace MoarJobs
{
	public static class SetupDataInterpreter
	{
		public static SetupData Interpret(string fullFilePath)
		{
			string[] blocks = File.ReadAllText(fullFilePath).Replace("\n\n", "\n").Split('~');
			SetupData setupData = new SetupData();
			List<Macro> macros = new List<Macro>(5);
			// Add special macros
			macros.Add(new Macro("INSERT_TILDE ~"));
			macros.Add(new Macro("INSERT_GRAVEACCENT `"));
			// Make macros
			string[] macroLines = blocks[1].Split('\n');
			foreach (string macroLine in macroLines)
			{
				macros.Add(new Macro(macroLine));
			}
			// Ignore the first 2 blocks (header & macros)
			for (int blockIndex = 2; blockIndex < blocks.Length; blockIndex++)
			{
				// Process macros for the current block
				foreach (Macro currentMacro in macros)
				{
					while (blocks[blockIndex].Contains(currentMacro.macroTag))
					{
						// TODO
						break;
					}
				}
			}
			return setupData;
		}

		private class Macro
		{
			public string macroTag;
			public string macroValue;

			public Macro(string macroLine)
			{
				// TODO
				macroTag = "";
				macroValue = "";
			}
		}
	}
}
