using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;
using System.Reflection;

namespace TechnoWolf.MoarJobs
{
	public static class ModSettingUtils
	{
		private static HugsLib.Utils.ModLogger logger;
		private static Dictionary<string, ModSetting> modSettings;

		public static void Initialize(HugsLib.Utils.ModLogger logger, ModSettingsPack settings)
		{
			ModSettingUtils.logger = logger;
			List<ModSetting> ms = new List<ModSetting>();
			ms.Add(
				new WorkTypeEnabledSetting(
					"Nursing",
					"Nurse Job",
					"Assists doctors by taking care of basic tasks",
					true,
					settings
					)
				);
			ms.Add(
				new WorkTypeEnabledSetting(
					"Feeding",
					"Feed Job",
					"Feeds prisoners and patients to allow wardens to focus on socializing",
					true,
					settings
					)
				);
			ms.Add(
				new WorkTypeEnabledSetting(
					"Refueling",
					"Refuel Job",
					"Adds fuel to generators, torches, etc.",
					true,
					settings
					)
				);
			ms.Add(
				new WorkTypeEnabledSetting(
					"TrapRearming",
					"Trap Rearm Job",
					"Rearms traps",
					true,
					settings
					)
				);
			ms.Add(
				new WorkTypeEnabledSetting(
					"Brewing",
					"Brew Job",
					"Brew beverages",
					true,
					settings
					)
				);
			ms.Add(
				new WorkTypeEnabledSetting(
					"Maintenance",
					"Maintenance Job",
					"Refuel, rearm, repair, build/destroy roofs, and other maintenance tasks",
					true,
					settings
					)
				);
			ms.Add(
				new WorkTypeEnabledSetting(
					"Demolition",
					"Demolition Job",
					"Demolish structures, uninstall furniture, smooth/remove flooring, etc.",
					true,
					settings
					)
				);
			ms.Add(
				new WorkTypeEnabledSetting(
					"Harvesting",
					"Harvest Job",
					"Harvest crops",
					true,
					settings
					)
				);
			ms.Add(
				new WorkTypeEnabledSetting(
					"Sowing",
					"Sow Job",
					"Plant crops",
					true,
					settings
					)
				);
			ms.Add(
				new WorkTypeEnabledSetting(
					"Loading",
					"Load Job",
					"Load/Unload caravans, transporters and other carriers",
					true,
					settings
					)
				);
			ms.Add(
				new WorkTypeEnabledSetting(
					"Mortician",
					"Mortician Job",
					"Bury bodies and carry out cremation tasks",
					true,
					settings
					)
				);
			ms.Add(
				new WorkGiverEnabledSetting(
					"HaulerHaulToBlueprint",
					"Haulers Can Haul to Blueprints",
					"Allow haulers to haul to construction blueprints",
					true,
					settings
					)
				);
			modSettings = new Dictionary<string, ModSetting>(ms.Count);
			foreach (ModSetting s in ms)
			{
				modSettings[s.codeName] = s;
			}
		}

		public static void SettingsChanged()
		{
		}

		private abstract class ModSetting
		{
			public readonly string codeName;
			public readonly string displayName;
			public readonly string description;
			public bool enabled;

			public bool Visible
			{
				get
				{
					return enabled;
				}
			}

			public ModSetting(string codeName, string displayName, string description)
			{
				this.codeName = codeName;
				this.displayName = displayName;
				this.description = description;
			}
		}

		private abstract class ModSetting<T> : ModSetting
		{
			public readonly T defaultValue;
			public readonly SettingHandle<T> settingHandle;

			public ModSetting(string codeName, string displayName, string description, T defaultValue, ModSettingsPack settingsPack)
				: base(codeName, displayName, description)
			{
				this.defaultValue = defaultValue;
				settingHandle = settingsPack.GetHandle<T>(
					codeName,
					displayName,
					description,
					defaultValue
					);
				settingHandle.OnValueChanged = ValueChanged;
				settingHandle.VisibilityPredicate = IsVisible;
			}

			protected abstract void ValueChanged(T newValue);

			private bool IsVisible()
			{
				return Visible;
			}
		}

		private class WorkTypeEnabledSetting : ModSetting<bool>
		{
			public WorkTypeEnabledSetting(string codeName, string displayName, string description, bool defaultValue, ModSettingsPack settingsPack)
				: base(codeName, displayName, description, defaultValue, settingsPack)
			{
			}

			protected override void ValueChanged(bool newValue)
			{
			}
		}

		private class WorkGiverEnabledSetting : ModSetting<bool>
		{
			public WorkGiverEnabledSetting(string codeName, string displayName, string description, bool defaultValue, ModSettingsPack settingsPack)
				: base(codeName, displayName, description, defaultValue, settingsPack)
			{
			}

			protected override void ValueChanged(bool newValue)
			{
			}
		}
	}
}
