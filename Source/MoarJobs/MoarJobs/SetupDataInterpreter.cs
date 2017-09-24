using System;
using System.Collections.Generic;
using System.IO;

namespace MoarJobs
{
	public static class SetupDataInterpreter
	{
		public static SetupData Interpret(string fullFilePath, HugsLib.Utils.ModLogger logger)
		{
			SetupData setupData = new SetupData(logger);
			string[] blocks = File.ReadAllText(fullFilePath).Replace(Environment.NewLine, "\n").Split('~');
			List<Macro> macros = new List<Macro>(5);
			// Add special macros
			macros.Add(new Macro("INSERT_TILDE ~"));
			macros.Add(new Macro("INSERT_GRAVEACCENT `"));
			List<GroupEntry> entries = new List<GroupEntry>(40);
			// Ignore the first block (header)
			for (int blockIndex = 1; blockIndex < blocks.Length; blockIndex++)
			{
				string[] lines = PreprocessBlock(blocks[blockIndex], ref macros);
				// If this block is a comment type section
				if (lines == null)
				{
					continue;
				}
				Group currentGroupContext = null;
				Type currentEntryType = null;
				Dictionary<string, string[]> baseEntries = new Dictionary<string, string[]>(4);
				for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
				{
					string unparsedEntry = lines[lineIndex];
					// Check for group context change
					if (unparsedEntry.StartsWith("$"))
					{
						// Return to the block base group
						if (unparsedEntry.Length == 1)
						{
							currentGroupContext = setupData.FindGroup(lines[0]);
							if (currentGroupContext == null)
							{
								currentGroupContext = setupData.AddGroup(lines[0], null);
							}
						}
						// Change the group context to a sub-group
						else
						{
							Group group = setupData.FindGroup(currentGroupContext.FullName + "." + unparsedEntry.Substring(1));
							if (group == null)
							{
								group = setupData.AddGroup(unparsedEntry.Substring(1), currentGroupContext);
							}
							currentGroupContext = group;
						}
					}
					// Process abstract/meta entries
					else if (currentGroupContext == null)
					{
						// Set the type of entries to be proccessed
						if (unparsedEntry.StartsWith("Type="))
						{
							currentEntryType = 
								typeof(MoarJobsMod).Assembly.GetType("MoarJobs.GroupEntry_" + unparsedEntry.Split('=')[1]);
						}
						// Process abstract/base entries
						else
						{
							// TODO
						}
					}
					// Process all standard entries
					else
					{
						List<string> entry = new List<string>(unparsedEntry.Split(new char[] { '`' }, StringSplitOptions.RemoveEmptyEntries));
						// Apply base entry
						if (unparsedEntry.Contains('`' + "Base="))
						{
							string baseAttr = entry.Find(
								(string e) =>
								{
									return e.StartsWith("Base=");
								}
								);
							entry.Remove(baseAttr);
							foreach(string attribute in baseEntries[baseAttr.Split('=')[1]])
							{
								bool containsAttribute = false;
								foreach (string currAttr in entry)
								{
									if (currAttr.StartsWith(attribute.Split('=')[0]))
									{
										containsAttribute = true;
										break;
									}
								}
								if (!containsAttribute)
								{
									entry.Add(attribute);
								}
							}
						}
						// Create group entries
						GroupEntry newGroupEntry = null;
						if (currentEntryType == null)
						{
							logger.Warning("SetupDataInterpreter:No entry type set to use for generation group entry");
						}
						else if (currentEntryType == typeof(GroupEntry_FunctionDeclaration))
						{
							if (currentGroupContext.localName == "WorkTypeUtils")
							{
								newGroupEntry = new GroupEntry_FunctionDeclaration(entry.ToArray(), WorkTypeUtils.DoFunctionCall);
							}
							else if (currentGroupContext.localName == "WorkGiverUtils")
							{
								newGroupEntry = new GroupEntry_FunctionDeclaration(entry.ToArray(), WorkGiverUtils.DoFunctionCall);
							}
							else
							{
								logger.Warning("SetupDataInterpreter:Current group context does not correspond to a known class declaring a DoFunctionCall method");
							}
						}
						else if (currentEntryType == typeof(GroupEntry_FunctionCall))
						{
							// TODO
						}
						else
						{
							newGroupEntry = (GroupEntry)Activator.CreateInstance(currentEntryType, new object[] { entry.ToArray() });
						}
						if (newGroupEntry != null)
						{
							entries.Add(newGroupEntry);
						}
					}
				}
			}
			// Finalize all entries in the order they were created
			foreach (GroupEntry entry in entries)
			{
				entry.Finalize(setupData);
			}
			return setupData;
		}

		private static string[] PreprocessBlock(string block, ref List<Macro> macros)
		{
			if (block.StartsWith("Reference") || block.StartsWith("Comment"))
			{
				return null;
			}
			// Process macros for the current block
			foreach (Macro currentMacro in macros)
			{
				block = block.Replace(currentMacro.macroTag, currentMacro.macroValue);
			}
			List<string> lines = new List<string>(block.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries));
			lines.RemoveAll(
				(string line) =>
				{
					if (line.StartsWith("//"))
					{
						return true;
					}
					foreach (char c in line)
					{
						if (!char.IsWhiteSpace(c))
						{
							return false;
						}
					}
					return true;
				}
				);
			// Remove in-line comments
			for (int li = 0; li < lines.Count; li++)
			{
				if (lines[li].Contains("\\"))
				{
					lines[li] = lines[li].Split('\\')[0];
				}
			}
			// Remove leading & trailing whitespace
			for (int i = 0; i < lines.Count; i++)
			{
				lines[i] = lines[i].Trim();
			}
			// Add the new macros
			if (block.StartsWith("Macros"))
			{
				foreach (string macroLine in lines)
				{
					if (!macroLine.StartsWith("#"))
					{
						continue;
					}
					macros.Add(new Macro(macroLine));
				}
				return null;
			}
			return lines.ToArray();
		}

		private class Macro
		{
			public string macroTag;
			public string macroValue;

			public Macro(string macroLine)
			{
				macroTag = macroLine.Substring(macroLine.StartsWith("#") ? 1 : 0, macroLine.IndexOf(' '));
				macroValue = macroLine.Substring(macroLine.IndexOf(' ') + 1).Replace("\n","");
			}

			public override string ToString()
			{
				return "Macro:" + macroTag + " " + macroValue;
			}
		}
	}
}
