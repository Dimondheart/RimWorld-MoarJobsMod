using System;
using System.Collections.Generic;

namespace MoarJobs
{
	public class GroupEntry_FunctionCall : GroupEntry
	{
		public readonly GroupEntry_FunctionDeclaration functionDeclaration;
		public readonly Dictionary<string, string> parameters;

		public GroupEntry_FunctionCall(string[] entry, GroupEntry_FunctionDeclaration functionDeclaration) : base(entry)
		{
			this.functionDeclaration = functionDeclaration;
			parameters = new Dictionary<string, string>(functionDeclaration.parameterTypes.Count);
		}

		public override void Finalize(SetupData data)
		{
			// TODO
		}
	}
}
