using System.Linq;
using System.Text;
using StardewValley;

namespace MHEventsList.Core;

public sealed class CharacterHeartRequirement
{
	public string NpcName { get; }

	public int FriendshipPoints { get; }

	public RequirementType Type { get; }

	public int Hearts => FriendshipPoints / 250;

	public CharacterHeartRequirement(string npcName, int friendshipPoints, RequirementType type = RequirementType.Friendship)
	{
		NpcName = npcName;
		FriendshipPoints = friendshipPoints;
		Type = type;
	}

	public string GetDisplayName()
	{
		NPC characterFromName = Game1.getCharacterFromName(NpcName, false, false);
		string name = ((characterFromName != null) ? ((Character)characterFromName).displayName : null) ?? ((characterFromName != null) ? ((Character)characterFromName).Name : null) ?? NpcName;
		if (!IsValidNpcName(name))
		{
			return null;
		}
		return SanitizeNpcName(name);
	}

	private static bool IsValidNpcName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}
		if (name.Contains("{{") || name.Contains("}}") || name.Contains("{") || name.Contains("["))
		{
			return false;
		}
		if (name.Contains('·') || name.Contains('•') || name.Contains('·') || name.Contains('・') || name.Contains('･'))
		{
			return false;
		}
		if (name.Length < 2)
		{
			return false;
		}
		if (!name.Any(char.IsLetter))
		{
			return false;
		}
		if (name.All(char.IsDigit))
		{
			return false;
		}
		if (name.StartsWith("??"))
		{
			return false;
		}
		if (name.Any((char c) => !char.IsLetterOrDigit(c) && c != ' ' && c != '-' && c != '\''))
		{
			return false;
		}
		return true;
	}

	private static string SanitizeNpcName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return name;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in name)
		{
			if (c != '·' && c != '•' && c != '·' && c != '・' && c != '･' && (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '\''))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString().Trim();
	}
}
