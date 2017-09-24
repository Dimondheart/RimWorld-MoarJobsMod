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

		public override void Finalize(SetupData setupData)
		{
			// Display name
			DisplayName = GetAttributeValueByName("DisplayName");
			if (DisplayName == null)
			{
				setupData.logger.Warning("Failed to get DisplayName for:" + name);
			}
			else if (DisplayName.Contains("<"))
			{
				if (DisplayName.Contains("<Name"))
				{
					DisplayName = DisplayName.Replace("<Name", name);
				}
			}
			// Description
			Description = GetAttributeValueByName("Description");
			if (Description == null)
			{
				setupData.logger.Warning("Failed to get Description for:" + name);
			}
			else if (Description.Contains("<"))
			{
				if (Description.Contains("<Name"))
				{
					Description = Description.Replace("<Name", name);
				}
				if (Description.Contains("<DisplayName") && DisplayName != null)
				{
					Description = Description.Replace("<DisplayName", DisplayName);
				}
			}
			// Setting type
			SettingType = Type.GetType("System." + GetAttributeValueByName("SettingType"));
			if (SettingType == null)
			{
				setupData.logger.Warning("Invalid type specified for mod setting:" + GetAttributeValueByName("SettingType") + "|" + name);
			}
			// Default value
			DefaultValue = GetAttributeValueByName("DefaultValue");
			if (DefaultValue == null)
			{
				setupData.logger.Warning("Failed to get DefaultValue for:" + name);
			}
			else
			{
				if (DefaultValue.Contains("<"))
				{
					if (DefaultValue.Contains("<Name"))
					{
						DefaultValue = DefaultValue.Replace("<Name", name);
					}
					if (DefaultValue.Contains("<DisplayName") && DisplayName != null)
					{
						DefaultValue = DefaultValue.Replace("<DisplayName", DisplayName);
					}
				}
				if(!ValidateValue(DefaultValue, SettingType, setupData))
				{
					setupData.logger.Warning("Invalid DefaultValue for:" + name + "|" + DefaultValue);
				}
			}
			// OperationsSettingsChanged
			string opCallsSC = GetAttributeValueByName("OperationsSettingsChanged");
			if (opCallsSC == null)
			{
				setupData.logger.Warning("Failed to get OperationsSettingsChanged for:" + name);
			}
			else
			{
				if (opCallsSC.Contains("<"))
				{
					if (opCallsSC.Contains("<Name"))
					{
						opCallsSC = opCallsSC.Replace("<Name", name);
					}
					if (opCallsSC.Contains("<DisplayName") && DisplayName != null)
					{
						opCallsSC = opCallsSC.Replace("<DisplayName", DisplayName);
					}
				}
				if (opCallsSC.Contains("("))
				{
					string[] opsSplit = opCallsSC.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					operationsSettingsChanged = new List<GroupEntry_FunctionCall>(opsSplit.Length);
					foreach (string op in opsSplit)
					{
						string opName = op.Split('(')[0];
						GroupEntry_FunctionCall call = new GroupEntry_FunctionCall(new string[] { op }, (GroupEntry_FunctionDeclaration)setupData.FindEntry(opName, "FunctionDeclarations"));
						call.Finalize(setupData);
						operationsSettingsChanged.Add(call);
					}
				}
			}
			// Make sure we initialize the list at least to something empty
			if (operationsSettingsChanged == null)
			{
				operationsSettingsChanged = new List<GroupEntry_FunctionCall>(0);
			}
		}
	}
}
