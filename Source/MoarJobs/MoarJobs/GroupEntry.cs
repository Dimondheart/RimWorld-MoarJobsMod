using System;

namespace MoarJobs
{
	public abstract class GroupEntry
	{
		public string name;
		protected string[] entry;

		public GroupEntry(string[] entry)
		{
			this.entry = entry;
			name = entry[0];
		}

		/** Parse the entry data and validate non-string values. */
		public abstract void Finalize(SetupData data);

		public string GetAttributeValueByName(string attributeName)
		{
			foreach (string attr in entry)
			{
				if (attr.StartsWith(attributeName))
				{
					if (attr.Contains("="))
					{
						return attr.Split('=')[1];
					}
					else
					{
						return "";
					}
				}
			}
			// Attribute not found
			return null;
		}

		public bool ValidateValue(string value, Type type, SetupData setupData)
		{
			if (type == typeof(string))
			{
				return true;
			}
			else if (type == typeof(bool))
			{
				return ValidateBoolean(value);
			}
			else if (type == typeof(int))
			{
				return ValidateInteger(value);
			}
			else if (type == typeof(float))
			{
				return ValidateSingle(value);
			}
			else if (type == typeof(Group))
			{
				return ValidateGroupName(value, setupData);
			}
			// Otherwise assume valid
			else
			{
				return true;
			}
		}

		public bool ValidateBoolean(string value)
		{
			bool temp = false;
			return bool.TryParse(value, out temp);
		}

		public bool ValidateInteger(string value)
		{
			int temp = 0;
			return int.TryParse(value, out temp);
		}

		public bool ValidateSingle(string value)
		{
			float temp = 0.0f;
			return float.TryParse(value, out temp);
		}

		public bool ValidateGroupName(string fullName, SetupData setupData)
		{
			return setupData.FindGroup(fullName) != null;
		}
	}
}
