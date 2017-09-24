using System;
using System.Collections.Generic;

namespace MoarJobs
{
	public class GroupEntry_FunctionDeclaration : GroupEntry
	{
		public readonly Action<GroupEntry_FunctionCall, Dictionary<string, string>> functionCallHandler;
		public readonly string[] parameterOrder;
		public readonly Dictionary<string, string> parameterType;

		public GroupEntry_FunctionDeclaration(string[] entry, Action<GroupEntry_FunctionCall, Dictionary<string, string>> functionCallHandler) : base(entry)
		{
			this.functionCallHandler = functionCallHandler;
			parameterOrder = new string[entry.Length - 1];
			parameterType = new Dictionary<string, string>(parameterOrder.Length);
		}

		public override void Finalize(SetupData data)
		{
			for (int i = 1; i < entry.Length; i++)
			{
				string[] attrPair = entry[i].Split('=');
				parameterOrder[i - 1] = attrPair[0];
				parameterType[attrPair[0]] = attrPair[1];
			}
		}
	}
}
