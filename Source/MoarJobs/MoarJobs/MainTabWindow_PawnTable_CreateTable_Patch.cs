using System;
using RimWorld;
using Verse;
using Harmony;

[HarmonyPatch(typeof(MainTabWindow_PawnTable), "CreateTable")]
public static class MainTabWindow_PawnTable_CreateTable_Patch
{
	public static PawnTable workTabPawnTable { get; private set; }

	[HarmonyPostfix]
	public static void StorePawnTableAccess(MainTabWindow_PawnTable __instance, ref PawnTable __result)
	{
		if (workTabPawnTable != null)
		{
			Log.Warning("[MoarJobs] workTabPawnTable has been set more than once");
		}
		workTabPawnTable = __result;
	}
}
