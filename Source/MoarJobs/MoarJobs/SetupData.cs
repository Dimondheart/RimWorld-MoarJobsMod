using System;
using System.Collections.Generic;

namespace MoarJobs
{
	public class SetupData
	{
		public readonly HugsLib.Utils.ModLogger logger;
		public List<Group> groups = new List<Group>(10);

		public SetupData(HugsLib.Utils.ModLogger logger)
		{
			this.logger = logger;
		}

		public IList<GroupEntry> GetEntries(string fullGroupName)
		{
			Group g = FindGroup(fullGroupName);
			return g == null ? null : g.Entries;
		}

		public Group FindGroup(string fullName)
		{
			return groups.Find(
				(Group group) =>
				{
					return group.FullName == fullName;
				}
				);
		}

		/** Create and add  a new group with the specified parent and local name.
		 * Returns the new group class, unless the group already exists or the parent
		 * was not found in this setup data then it will return null.
		 */
		public Group AddGroup(string name, Group parent)
		{
			if (parent != null && !groups.Contains(parent))
			{
				return null;
			}
			if (groups.Find((Group g) => { return g.parent == parent && g.localName == name; }) != null)
			{
				return null;
			}
			Group newGroup = new Group(name, parent);
			groups.Add(newGroup);
			return newGroup;
		}

		public void AddGroupEntry(GroupEntry entry, params Group[] groups)
		{
			foreach (Group group in groups)
			{
				group.AddEntry(entry);
			}
		}

		public GroupEntry FindEntry(string entryName, string searchInGroup)
		{
			Group group = FindGroup(searchInGroup);
			if (group != null)
			{
				foreach (GroupEntry entry in group.Entries)
				{
					if (entry.name == entryName)
					{
						return entry;
					}
				}
			}
			return null;
		}
	}
}
