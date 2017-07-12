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
		public static readonly string modName = "[A17] Moar Jobs Mod";
		public static readonly string modSetupDataFileName = "MoarJobs.setupdata";
		/** All work types after defs are loaded, sorted from highest priority to
		 * lowest priority (i.e. the order displayed in the work priorities tab.) This array
		 * is null until initialized in DefsLoaded(). This array is updated every time
		 * SettingsChanged() is called on this class.
		 */
		public static string[] allWorkTypesByPriority { get; private set; }
		/** All work types, if they are visible or not. This array
		 * is null until initialized in DefsLoaded(). This array is updated every time
		 * SettingsChanged() is called on this class.
		 */
		public static bool[] allWorkTypesVisibility { get; private set; }
		/** Def names of the work types created by this mod. */
		public static readonly string[] moarJobsWorkTypeDefNames =
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
		/** The first 'normal' job is the index in workTypeDefNames of the first job that
		 * is not a hidden job used internally never enabled for the player.
		 */
		public static readonly int firstMoarJobsNormalJob = 1;
		public static readonly int firstWorkPriorityColumn = 2;

		public static string RootDir
		{
			get
			{
				foreach (ModMetaData mmd in ModLister.AllInstalledMods)
				{
					if (mmd.Name == modName)
					{
						return mmd.RootDir.FullName;
					}
				}
				return null;
			}
		}

		/** Shorthand for calling array.FirstIndexOf(predictate function) when looking for
		 * an exact string match.
		 */
		public static int StringFirstIndexInArray(string lookingFor, string[] array)
		{
			return array.FirstIndexOf(
							(string s) =>
							{
								return s == lookingFor;
							}
							);
		}

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
		private PawnColumnDef[] workTypePawnColumnDefs;

		public MoarJobs()
		{
			workTypeDefs = new Dictionary<string, WorkTypeDef>(moarJobsWorkTypeDefNames.Length);
			modSettingsJobToggles = new SettingHandle<bool>[moarJobsWorkTypeDefNames.Length - firstMoarJobsNormalJob];
		}

		public override void Initialize()
		{
			SetupData setupData = SetupDataInterpreter.Interpret(RootDir + @"\" + modSetupDataFileName);
		}

		public override void DefsLoaded()
		{
			if (!ModIsActive)
			{
				return;
			}
			UpdateWorkTypeArrays();
			// Set up mod settings
			for (int i = 0; i < moarJobsWorkTypeDefNames.Length; i++)
			{
				string defName = moarJobsWorkTypeDefNames[i];
				workTypeDefs.Add(
					defName,
					(WorkTypeDef) GenDefDatabase.GetDef(typeof(WorkTypeDef), defName, true)
					);
				if (i >= firstMoarJobsNormalJob)
				{
					modSettingsJobToggles[i - firstMoarJobsNormalJob] =
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
			UpdateWorkTypeArrays();
			foreach (SettingHandle<bool> handle in modSettingsJobToggles)
			{
				WorkTypeDef workTypeDef = workTypeDefs[handle.Name];
				if (Find.World != null && Find.WorldPawns != null)
				{
					PawnTable workPawnTable = MainTabWindow_PawnTable_CreateTable_Patch.workTabPawnTable;
					if (handle.Value)
					{
						if (!workTypeDef.visible)
						{
							int indexInNames = StringFirstIndexInArray(handle.Name, moarJobsWorkTypeDefNames);
							// TODO perform 'patches'
							PawnColumnDef columnDef = workTypePawnColumnDefs[StringFirstIndexInArray(handle.Name, allWorkTypesByPriority)];
							if (columnDef == null)
							{
								Logger.Message("Manually creating column");
								columnDef = new PawnColumnDef();
								columnDef.defName = "WorkPriority_" + handle.Name;
								columnDef.sortable = true;
								HugsLib.Utils.InjectedDefHasher.GiveShortHasToDef(columnDef, typeof(PawnColumnDef));
							}
							int insertAt = GetIndexInWorkPawnTable(handle.Name);
							Logger.Message("insertAt:" + insertAt);
							// Adjust the header position of the def to be inserted
							if (insertAt == firstWorkPriorityColumn)
							{
								columnDef.moveWorkTypeLabelDown = false;
							}
							else if (insertAt > firstWorkPriorityColumn)
							{
								columnDef.moveWorkTypeLabelDown = !workPawnTable.ColumnsListForReading[insertAt - 1].moveWorkTypeLabelDown;
							}
							// Adjust the header position of all following columns
							if (columnDef.moveWorkTypeLabelDown == workPawnTable.ColumnsListForReading[insertAt].moveWorkTypeLabelDown)
							{
								for (int i = insertAt; i < workPawnTable.ColumnsListForReading.Count; i++)
								{
									workPawnTable.ColumnsListForReading[i].moveWorkTypeLabelDown = !workPawnTable.ColumnsListForReading[i].moveWorkTypeLabelDown;
								}
							}
							MainTabWindow_PawnTable_CreateTable_Patch.workTabPawnTable
								.ColumnsListForReading.Insert(insertAt, columnDef);
							PawnTableDefOf.Work.columns.Insert(insertAt, columnDef);
							workTypePawnColumnDefs[StringFirstIndexInArray(handle.Name, allWorkTypesByPriority)] = null;
							workPawnTable.SetDirty();
						}
					}
					else
					{
						List<PawnColumnDef> workPawnTableColumns = workPawnTable.ColumnsListForReading;
						// Flip the header positions for following columns
						for (int i = 0; i < workPawnTableColumns.Count; i++)
						{
							if (workPawnTableColumns[i].workType == workTypeDef)
							{
								for (int i2 = i; i2 < workPawnTableColumns.Count; i2++)
								{
									workPawnTableColumns[i2].moveWorkTypeLabelDown =
										!workPawnTableColumns[i2].moveWorkTypeLabelDown;
								}
								break;
							}
						}
						// Remove the columns in the table def and the actual table
						int removeAt = GetIndexInWorkPawnTable(handle.Name);
						Logger.Message("removeAt:" + removeAt);
						PawnColumnDef colDef = workPawnTable.ColumnsListForReading[removeAt];
						workTypePawnColumnDefs[StringFirstIndexInArray(handle.Name, allWorkTypesByPriority)] = colDef;
						workPawnTable.ColumnsListForReading.Remove(colDef);
						PawnTableDefOf.Work.columns.Remove(colDef);
						workPawnTable.SetDirty();
						// Disable the work type for all existing pawns in all maps
						foreach (Map map in Find.Maps)
						{
							foreach (Pawn pawn in map.mapPawns.AllPawns)
							{
								if (pawn.workSettings != null && pawn.workSettings.EverWork)
								{
									pawn.workSettings.Disable(workTypeDef);
								}
							}
						}
					}
				}
				workTypeDef.visible = handle.Value;
				UpdateWorkTypeArrays();
			}
		}

		public int GetIndexInWorkPawnTable(string workTypeDefName)
		{
			int index = StringFirstIndexInArray(workTypeDefName, allWorkTypesByPriority);
			// Adjust position to account for invisible jobs
			for (int i = index - 1; i >= 0; i--)
			{
				if (!allWorkTypesVisibility[i])
				{
					index--;
				}
			}
			// We add some because the first few columns are not work priority
			return index + firstWorkPriorityColumn;
		}

		/** Update the arrays storing data related to work type defs. */
		private void UpdateWorkTypeArrays()
		{
			List<WorkTypeDef> allWorkTypeDefs = new List<WorkTypeDef>();
			foreach (WorkTypeDef wtd in DefDatabase<WorkTypeDef>.AllDefs)
			{
				allWorkTypeDefs.Add(wtd);
			}
			if (allWorkTypesByPriority != null && allWorkTypeDefs.Count != allWorkTypesByPriority.Length)
			{
				Logger.Warning("Number of job definitions changed");
			}
			allWorkTypeDefs.Sort((WorkTypeDef a, WorkTypeDef b) =>
			{
				return b.naturalPriority - a.naturalPriority;
			}
			);
			if (allWorkTypesByPriority == null)
			{
				allWorkTypesByPriority = new string[allWorkTypeDefs.Count];
			}
			if (allWorkTypesVisibility == null)
			{
				allWorkTypesVisibility = new bool[allWorkTypeDefs.Count];
			}
			if (workTypePawnColumnDefs == null)
			{
				workTypePawnColumnDefs = new PawnColumnDef[allWorkTypesByPriority.Length];
			}
			for (int i = 0; i < allWorkTypeDefs.Count; i++)
			{
				allWorkTypesByPriority[i] = allWorkTypeDefs[i].defName;
				allWorkTypesVisibility[i] = allWorkTypeDefs[i].visible;
			}
		}
	}
}
