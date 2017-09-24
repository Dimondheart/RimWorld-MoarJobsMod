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
			name = entry[0].Split('(')[0];
			this.functionDeclaration = functionDeclaration;
			parameters = new Dictionary<string, string>(functionDeclaration.parameterOrder.Length);
		}

		public override void Finalize(SetupData data)
		{
			int start = entry[0].IndexOf('(');
			int length = entry[0].LastIndexOf(')') - start;
			string[] eachParam = entry[0].Substring(start, length).Split(',');
			for (int i = 0; i < eachParam.Length; i++)
			{
				parameters[functionDeclaration.parameterOrder[i]] = eachParam[i];
				// TODO validate value
			}
		}
	}
}
