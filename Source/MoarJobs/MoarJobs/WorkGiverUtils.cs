using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;

namespace MoarJobs
{
	public static class WorkGiverUtils
	{
		private static Dictionary<string, WorkGiverState> allWorkGiverStates;
		private static SetupData setupData;
		private static HugsLib.Utils.ModLogger logger;

		public static void Initialize(SetupData setupData, HugsLib.Utils.ModLogger logger)
		{
			WorkGiverUtils.setupData = setupData;
			WorkGiverUtils.logger = logger;
			allWorkGiverStates = new Dictionary<string, WorkGiverState>(DefDatabase<WorkGiverDef>.AllDefsListForReading.Count);
			foreach (WorkGiverDef def in DefDatabase<WorkGiverDef>.AllDefs)
			{
				allWorkGiverStates[def.defName] = new WorkGiverState(def);
			}
		}

		public static void DoFunctionCall(GroupEntry_FunctionCall call, Dictionary<string, string> values)
		{
			switch (call.name)
			{
				case "SetWorkGiverEnabled":
					SetWorkGiverEnabled(values["giver"], bool.Parse(values["enabled"]));
					break;
				case "SetWorkGiversEnabled":
					SetWorkGiversEnabled(setupData.FindGroup(values["group"]), bool.Parse(values["enabled"]));
					break;
				default:
					logger.Error("Invalid function call group entry on WorkGiverUtils:" + call.name);
					break;
			}
		}

		public static void SetWorkGiverEnabled(String defName, bool value)
		{
			if (Find.World != null && Find.WorldPawns != null)
			{
				allWorkGiverStates[defName].Enabled = value;
			}
		}

		public static void SetWorkGiversEnabled(Group group, bool value)
		{
			foreach (GroupEntry entry in group.Entries)
			{
				SetWorkGiverEnabled(entry.name, value);
			}
		}

		private class WorkGiverState : DefState
		{
			public readonly WorkGiverDef workGiverDef;
			private WorkTypeDef originalWorkType;
			private bool enabledInternal;

			public override bool Enabled
			{
				get
				{
					return enabledInternal;
				}
				set
				{
					if (value != enabledInternal)
					{
						if (value)
						{
							WorkGiverUtils.logger.Message("Enable work giver:" + workGiverDef.defName);
							// TODO
						}
						else
						{
							WorkGiverUtils.logger.Message("Disable work giver:" + workGiverDef.defName);
							// TODO
						}
						enabledInternal = value;
					}
				}
			}

			public WorkGiverState(WorkGiverDef workGiverDef)
			{
				this.workGiverDef = workGiverDef;
				originalWorkType = workGiverDef.workType;
				enabledInternal = true;
			}
		}
	}
}
