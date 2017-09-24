using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;

namespace MoarJobs
{
	public static class WorkTypeUtils
	{
		/** All work types after defs are loaded, sorted from highest priority to
		 * lowest priority (i.e. the order displayed in the work priorities tab.) This array
		 * is null until initialized in DefsLoaded(). This array is updated every time
		 * SettingsChanged() is called on this class.
		 */
		public static string[] allWorkTypesByPriority { get; private set; }
		/** The first 'normal' job is the index in workTypeDefNames of the first job that
		 * is not a hidden job used for internal purposes.
		 */
		public static readonly int firstMoarJobsNormalJob = 1;
		public static readonly int firstWorkPriorityColumn = 2;
		private static WorkTypeState[] allWorkTypeStates;
		private static HugsLib.Utils.ModLogger logger;

		public static void Initialize(SetupData setupData, HugsLib.Utils.ModLogger logger)
		{
			WorkTypeUtils.logger = logger;
			UpdateWorkTypeArrays();
		}

		public static void Refresh()
		{
			UpdateWorkTypeArrays();
		}

		public static int GetIndexInWorkPawnTable(string workTypeDefName)
		{
			int index = MoarJobs.Utils.StringFirstIndexInArray(workTypeDefName, allWorkTypesByPriority);
			// Adjust position to account for invisible jobs
			for (int i = index - 1; i >= 0; i--)
			{
				if (!allWorkTypeStates[i].Visible)
				{
					index--;
				}
			}
			// We add some because the first few columns are not work priority
			return index + firstWorkPriorityColumn;
		}

		public static void DoFunctionCall(GroupEntry_FunctionCall call, Dictionary<string, string> values)
		{
			switch (call.name)
			{
				case "SetWorkTypeEnabled":
					SetWorkTypeEnabled(values["type"], bool.Parse(values["enabled"]));
					break;
				default:
					logger.Error("Invalid function call group entry on WorkTypeUtils:" + call.name);
					break;
			}
		}

		public static void SetWorkTypeEnabled(string defName, bool value)
		{
			if (Find.World != null && Find.WorldPawns != null)
			{
				allWorkTypeStates[MoarJobs.Utils.StringFirstIndexInArray(defName, allWorkTypesByPriority)].Enabled = value;
			}
			UpdateWorkTypeArrays();
		}

		/** Update the arrays storing data related to work type defs. */
		private static void UpdateWorkTypeArrays()
		{
			List<WorkTypeDef> allWorkTypeDefs = new List<WorkTypeDef>();
			foreach (WorkTypeDef wtd in DefDatabase<WorkTypeDef>.AllDefs)
			{
				allWorkTypeDefs.Add(wtd);
			}
			if (allWorkTypesByPriority != null && allWorkTypeDefs.Count != allWorkTypesByPriority.Length)
			{
				logger.Warning("Number of job definitions changed");
				return;
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
			if (allWorkTypeStates == null)
			{
				allWorkTypeStates = new WorkTypeState[allWorkTypesByPriority.Length];
			}
			for (int i = 0; i < allWorkTypeDefs.Count; i++)
			{
				allWorkTypesByPriority[i] = allWorkTypeDefs[i].defName;
				if (allWorkTypeStates[i] == null)
				{
					allWorkTypeStates[i] = new WorkTypeState(allWorkTypeDefs[i]);
				}
			}
		}

		private class WorkTypeState : DefState
		{
			public readonly WorkTypeDef workTypeDef;
			private bool enabledInternal;
			private PawnColumnDef columnDef;

			public bool Visible
			{
				get
				{
					return workTypeDef.visible;
				}
				set
				{
					// TODO make this actually change the visibility
					workTypeDef.visible = value;
				}
			}

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
						enabledInternal = value;
						PawnTable workPawnTable = MainTabWindow_PawnTable_CreateTable_Patch.workTabPawnTable;
						if (value)
						{
							if (!Visible)
							{
								if (columnDef == null)
								{
									logger.Message("Manually creating column");
									columnDef = new PawnColumnDef();
									columnDef.defName = "WorkPriority_" + workTypeDef.defName;
									columnDef.sortable = true;
									// Set this even though the base game doesn't currently set it
									columnDef.workType = workTypeDef;
									HugsLib.Utils.InjectedDefHasher.GiveShortHasToDef(columnDef, typeof(PawnColumnDef));
								}
								int insertAt = GetIndexInWorkPawnTable(workTypeDef.defName);
								logger.Message("insertAt:" + insertAt);
								// Adjust the header position of the def to be inserted
								if (insertAt == firstWorkPriorityColumn)
								{
									columnDef.moveWorkTypeLabelDown = false;
								}
								else
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
							int removeAt = WorkTypeUtils.GetIndexInWorkPawnTable(workTypeDef.defName);
							logger.Message("removeAt:" + removeAt);
							columnDef = workPawnTable.ColumnsListForReading[removeAt];
							workPawnTable.ColumnsListForReading.Remove(columnDef);
							PawnTableDefOf.Work.columns.Remove(columnDef);
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
						Visible = value;
					}
				}
			}

			public WorkTypeState(WorkTypeDef workTypeDef)
			{
				this.workTypeDef = workTypeDef;
				enabledInternal = true;
			}
		}
	}
}
