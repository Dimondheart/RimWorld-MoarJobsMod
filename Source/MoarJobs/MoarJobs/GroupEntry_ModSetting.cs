using System;
using System.Collections.Generic;

namespace MoarJobs
{
	public class GroupEntry_ModSetting : GroupEntry
	{
		public string DisplayName { get; private set; }
		public string Description { get; private set; }
		public Type SettingType { get; private set; }
		public string DefaultValue { get; private set; }
		public List<GroupEntry_FunctionCall> operationsSettingsChanged { get; private set; }

		public GroupEntry_ModSetting(string[] entry) : base(entry)
		{
		}

		public override void Finalize(SetupData data)
		{
			// TODO
			if (operationsSettingsChanged == null)
			{
				operationsSettingsChanged = new List<GroupEntry_FunctionCall>(0);
			}
		}
	}
}
