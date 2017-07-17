using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;

namespace MoarJobs
{
	public class MoarJobsMod : ModBase
	{
		public static readonly string modName = "[A17] Moar Jobs Mod";
		public static readonly string modSetupDataFileName = "MoarJobs.setupdata";

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

		public override string ModIdentifier
		{
			get
			{
				return "A17MoarJobs";
			}
		}

		public override void DefsLoaded()
		{
			if (!ModIsActive)
			{
				return;
			}
			SetupData setupData = SetupDataInterpreter.Interpret(RootDir + @"\" + modSetupDataFileName, Logger);
			WorkTypeUtils.Initialize(setupData, Logger);
			WorkGiverUtils.Initialize(setupData, Logger);
			ModSettingUtils.Initialize(setupData, Logger, Settings);
		}

		public override void SettingsChanged()
		{
			ModSettingUtils.SettingsChanged();
		}
	}
}
