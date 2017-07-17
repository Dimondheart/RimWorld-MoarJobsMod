using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;
using System.Reflection;

namespace MoarJobs
{
	public static class ModSettingUtils
	{
		private static HugsLib.Utils.ModLogger logger;
		private static SetupData setupData;
		private static Dictionary<string, SettingHandle> modSettingHandles;
		private static Dictionary<string, List<GroupEntry_FunctionCall>> operationsSettingsChanged;

		public static void Initialize(SetupData setupData, HugsLib.Utils.ModLogger logger, ModSettingsPack settings)
		{
			ModSettingUtils.logger = logger;
			ModSettingUtils.setupData = setupData;
			modSettingHandles = new Dictionary<string, SettingHandle>(setupData.GetEntries("ModSettings").Count);
			operationsSettingsChanged = new Dictionary<string, List<GroupEntry_FunctionCall>>(setupData.GetEntries("ModSettings").Count);
			foreach(GroupEntry_ModSetting modSetting in setupData.GetEntries("ModSettings"))
			{
				if (modSetting.SettingType == typeof(bool))
				{
					modSettingHandles[modSetting.name] = settings.GetHandle<bool>(
					modSetting.name,
					modSetting.DisplayName,
					modSetting.Description,
					bool.Parse(modSetting.DefaultValue)
					);
				}
				else if (modSetting.SettingType == typeof(string))
				{
					modSettingHandles[modSetting.name] = settings.GetHandle<string>(
					modSetting.name,
					modSetting.DisplayName,
					modSetting.Description,
					modSetting.DefaultValue
					);
				}
				else
				{
					logger.Warning("Setting type not allowed:" + modSetting.SettingType.ToString());
				}
				operationsSettingsChanged[modSetting.name] = modSetting.operationsSettingsChanged;
			}
		}

		public static void SettingsChanged()
		{
			foreach (GroupEntry_ModSetting setting in setupData.GetEntries("ModSettings"))
			{
				foreach (GroupEntry_FunctionCall call in setting.operationsSettingsChanged)
				{
					Dictionary<string, string> passValues = new Dictionary<string, string>(call.parameters.Count);
					// Set values to pass
					foreach (System.Collections.Generic.KeyValuePair<string, string> pair in call.parameters)
					{
						string parameter = pair.Value;
						// Perform reference substitutions
						if (parameter.Contains("^"))
						{
							if (parameter.Contains("!^Value"))
							{
								parameter = parameter.Replace("!^Value", (!((SettingHandle<bool>)modSettingHandles[setting.name]).Value).ToString());
							}
							if (parameter.Contains("^Value"))
							{
								parameter = parameter.Replace("^Value", ((SettingHandle<bool>)modSettingHandles[setting.name]).Value.ToString());
							}
							if (parameter.Contains("^"))
							{
								logger.Warning("Possible unprocessed reference(s) in parameter:" + parameter);
							}
						}
						passValues[pair.Key] = parameter;
					}
					call.functionDeclaration.functionCallHandler(call, passValues);
				}
			}
		}
	}
}
