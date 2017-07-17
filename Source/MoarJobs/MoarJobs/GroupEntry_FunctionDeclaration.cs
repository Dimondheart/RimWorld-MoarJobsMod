using System;
using System.Collections.Generic;

namespace MoarJobs
{
	public class GroupEntry_FunctionDeclaration : GroupEntry
	{
		public readonly Action<GroupEntry_FunctionCall, Dictionary<string, string>> functionCallHandler;
		public readonly Dictionary<string, string> parameterTypes;

		public GroupEntry_FunctionDeclaration(string[] entry, Action<GroupEntry_FunctionCall, Dictionary<string, string>> functionCallHandler) : base(entry)
		{
			this.functionCallHandler = functionCallHandler;
			parameterTypes = new Dictionary<string, string>(entry.Length - 1);
		}

		public override void Finalize(SetupData data)
		{
			// TODO
		}
	}
}
