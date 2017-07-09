using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;

namespace MoarJobs
{
	public class MoarJobs : ModBase
	{
		public static readonly string[] workTypeDefNames =
		{
			"HiddenJob",
			"Maintenance",
			"Demolition",
			"Brewing",
			"Harvesting",
			"Sowing",
			"Loading",
			"Mortician",
			"Feeding",
			"Nursing",
			"Refueling",
			"TrapRearming"
		};
		/** The first 'Normal' job is the index in workTypeDefNames of the first job that
		 * is not a hidden job used internally never enabled for the player.
		 */
		public static readonly int firstNormalJob = 1;

		public override string ModIdentifier
		{
			get
			{
				return "MoarJobs";
			}
		}

		/** A dictionary of name:Def for work types created by this mod. */
		public Dictionary<string, WorkTypeDef> workTypeDefs { get; private set; }
		private SettingHandle<bool>[] modSettingsJobToggles;

		public MoarJobs()
		{
			workTypeDefs = new Dictionary<string, WorkTypeDef>(workTypeDefNames.Length);
			modSettingsJobToggles = new SettingHandle<bool>[workTypeDefNames.Length - firstNormalJob];
		}

		public override void Initialize()
		{
		}

		public override void DefsLoaded()
		{
			if (!ModIsActive)
			{
				return;
			}
			for (int i = 0; i < workTypeDefNames.Length; i++)
			{
				string defName = workTypeDefNames[i];
				workTypeDefs.Add(
					defName,
					(WorkTypeDef) GenDefDatabase.GetDef(typeof(WorkTypeDef), defName, true)
					);
				if (i >= firstNormalJob)
				{
					modSettingsJobToggles[i - firstNormalJob] =
						Settings.GetHandle<bool>(
							defName,
							workTypeDefs[defName].labelShort,
							workTypeDefs[defName].description,
							true
							);
				}
			}
		}

		public override void SettingsChanged()
		{
			Logger.Message("SettingsChanged");
			foreach (SettingHandle<bool> handle in modSettingsJobToggles)
			{
				WorkTypeDef workTypeDef = workTypeDefs[handle.Name];
				Logger.Message("workTypeDef:" + workTypeDef + "|Val:" + handle.Value);
				workTypeDef.visible = handle.Value;
				//WorkTypeDef currentDef = (WorkTypeDef) GenDefDatabase.GetDef(typeof(WorkTypeDef), handle.Name, true);
				//Logger.Message("localVal:" + workTypeDef.visible + "|actual:" + currentDef.visible);
				if (handle.Value)
				{
					// TODO perform 'patches'
				}
				else
				{
					// TODO revert 'patches'
					//Logger.Message("SettingsChangedTryDisableWork");
					//Logger.Message(Find.World + "|" + Find.WorldPawns + "|" + Find.WorldPawns.AllPawnsAlive);
					if (Find.World != null && Find.WorldPawns != null && Find.WorldPawns.AllPawnsAlive != null)
					{
						//Logger.Message("SettingsChangedDoDisableWork|" + Find.WorldPawns.AllPawnsAliveOrDead.ToString());
						foreach (Map map in Find.Maps)
						{
							foreach (Pawn pawn in map.mapPawns.AllPawns)
							{
								//Logger.Message("TryDisablingWork:" + pawn.ToString() + "|" + pawn.IsColonist);
								if (pawn.workSettings != null && pawn.workSettings.EverWork)
								{
									//Logger.Message("DisablingWork:" + pawn.ToString());
									pawn.workSettings.Disable(workTypeDef);
								}
							}
						}
					}
				}
			}
			// TODO force 'relayout' of work priorities menu 
		}
	}
}
