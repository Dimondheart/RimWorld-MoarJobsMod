using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;

namespace TechnoWolf.MoarJobs
{
	public static class WorkGiverUtils
	{
		private static Dictionary<string, WorkGiverState> allWorkGiverStates;
		private static HugsLib.Utils.ModLogger logger;

		public static void Initialize(HugsLib.Utils.ModLogger logger)
		{
			WorkGiverUtils.logger = logger;
			allWorkGiverStates = new Dictionary<string, WorkGiverState>(DefDatabase<WorkGiverDef>.AllDefsListForReading.Count);
			foreach (WorkGiverDef def in DefDatabase<WorkGiverDef>.AllDefs)
			{
				allWorkGiverStates[def.defName] = new WorkGiverState(def);
			}
		}

		public static void SetWorkGiverEnabled(bool value, String defName)
		{
			if (Find.World != null && Find.WorldPawns != null)
			{
				allWorkGiverStates[defName].Enabled = value;
			}
		}

		public static void SetWorkGiversEnabled(bool value, params string[] defNames)
		{
			foreach (string defName in defNames)
			{
				SetWorkGiverEnabled(value, defName);
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
