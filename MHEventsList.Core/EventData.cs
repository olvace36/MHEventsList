using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MHEventsList.Config;
using MHEventsList.Integration;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace MHEventsList.Core;

public sealed class EventData
{
	private string eventScript;

	public string Id { get; }

	public string LocationName { get; set; }

	public string DisplayName { get; set; }

	public string ModSource { get; set; }

	public string RawPreconditions { get; }

	public int StartTime { get; private set; } = -1;

	public int EndTime { get; private set; } = -1;

	public int RequiredFreeSlots { get; private set; }

	public List<string> RequiredItems { get; private set; }

	public List<string> RequiredNpcsPresent { get; private set; }

	public List<CharacterHeartRequirement> HeartRequirements { get; private set; }

	public bool EndsDay { get; private set; }

	public bool HasInvalidScript { get; set; }

	public bool HasUnresolvedTokens { get; set; }

	public bool IsMarkerEvent { get; private set; }

	public List<Dictionary<string, string>> WhenConditionsVariants { get; set; }

	public Dictionary<string, string> WhenConditions
	{
		get
		{
			List<Dictionary<string, string>> whenConditionsVariants = WhenConditionsVariants;
			if (whenConditionsVariants == null || whenConditionsVariants.Count <= 0)
			{
				return null;
			}
			return WhenConditionsVariants[0];
		}
		set
		{
			if (value != null && value.Count > 0)
			{
				if (WhenConditionsVariants == null)
				{
					WhenConditionsVariants = new List<Dictionary<string, string>>();
				}
				if (WhenConditionsVariants.Count == 0)
				{
					WhenConditionsVariants.Add(value);
				}
			}
		}
	}

	public bool HasWhenConditions
	{
		get
		{
			if (WhenConditionsVariants != null && WhenConditionsVariants.Count > 0)
			{
				return WhenConditionsVariants.Any((Dictionary<string, string> v) => v.Count > 0);
			}
			return false;
		}
	}

	public bool IsFromContentPatcher { get; set; }

	public string ModFolderPath { get; set; }

	public string ModUniqueId { get; set; }

	public bool IsDisabledByCP { get; set; }

	public string ModName
	{
		get
		{
			if (!string.IsNullOrEmpty(ModFolderPath))
			{
				string text = ModFolderPath.Replace('/', '\\');
				ModConfig config = MHEventsListMod.Config;
				if (config != null && config.UseManifestFolderForModName)
				{
					return Path.GetFileName(text.TrimEnd('\\', '/'));
				}
				int num = text.IndexOf("Mods\\", StringComparison.OrdinalIgnoreCase);
				if (num >= 0)
				{
					string text2 = text.Substring(num + 5);
					int num2 = text2.IndexOf('\\');
					if (num2 > 0)
					{
						return text2.Substring(0, num2);
					}
					return text2;
				}
				return Path.GetFileName(text.TrimEnd('\\', '/'));
			}
			if (!string.IsNullOrEmpty(Id))
			{
				if (Id.Contains("."))
				{
					int num3 = Id.IndexOf('.');
					if (num3 > 0)
					{
						string key = Id.Substring(0, num3);
						if (EventRegistry.IdPrefixToModName.TryGetValue(key, out var value))
						{
							return value;
						}
					}
				}
				if (Id.Length >= 2 && char.IsUpper(Id[0]))
				{
					int i;
					for (i = 1; i < Id.Length && char.IsUpper(Id[i]); i++)
					{
					}
					if (i >= 2 && i <= 4 && i < Id.Length && char.IsLower(Id[i]))
					{
						string key2 = Id.Substring(0, i);
						if (EventRegistry.IdPrefixToModName.TryGetValue(key2, out var value2))
						{
							return value2;
						}
					}
				}
				if (Id.Contains("_"))
				{
					int num4 = Id.IndexOf('_');
					if (num4 > 1)
					{
						string key3 = Id.Substring(0, num4);
						if (EventRegistry.IdPrefixToModName.TryGetValue(key3, out var value3))
						{
							return value3;
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(Id))
			{
				if (Id.Contains("."))
				{
					string[] array = Id.Split('.');
					if (array.Length >= 2 && array[0].Length > 0 && array[1].Length > 0 && char.IsLetter(array[0][0]) && char.IsLetter(array[1][0]))
					{
						int num5 = Id.IndexOf('_');
						if (num5 > 0)
						{
							string text3 = Id.Substring(0, num5);
							if (text3.Contains("."))
							{
								return text3;
							}
						}
						return array[0] + "." + array[1];
					}
				}
				if (EventRegistry.VanillaEventIds.Contains(Id))
				{
					return null;
				}
				if (!long.TryParse(Id, out var _))
				{
					return "Mod";
				}
			}
			return null;
		}
	}

	public int WhenVariantsCount => WhenConditionsVariants?.Count ?? 0;

	public List<string> RequiredNpcs
	{
		get
		{
			List<string> list = new List<string>();
			if (HeartRequirements != null)
			{
				foreach (CharacterHeartRequirement heartRequirement in HeartRequirements)
				{
					if (!list.Contains(heartRequirement.NpcName))
					{
						list.Add(heartRequirement.NpcName);
					}
				}
			}
			if (RequiredNpcsPresent != null)
			{
				foreach (string item in RequiredNpcsPresent)
				{
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			return list;
		}
	}

	public string GetModDisplayName()
	{
		ModConfig config = MHEventsListMod.Config;
		if (config != null && config.UseModDisplayName && !string.IsNullOrEmpty(ModUniqueId))
		{
			try
			{
				IModHelper helper = MHEventsListMod.Helper;
				IModInfo val = ((helper != null) ? helper.ModRegistry.Get(ModUniqueId) : null);
				if (val != null)
				{
					return val.Manifest.Name;
				}
			}
			catch
			{
			}
		}
		return ModSource ?? ModName;
	}

	public string GetSourceDisplay()
	{
		if (string.IsNullOrEmpty(ModSource))
		{
			return "Vanilla";
		}
		return GetModDisplayName();
	}

	public void AddWhenConditionsVariant(Dictionary<string, string> conditions)
	{
		if (conditions != null && conditions.Count != 0)
		{
			if (WhenConditionsVariants == null)
			{
				WhenConditionsVariants = new List<Dictionary<string, string>>();
			}
			if (!WhenConditionsVariants.Any((Dictionary<string, string> existing) => existing.Count == conditions.Count && existing.All((KeyValuePair<string, string> kvp) => conditions.TryGetValue(kvp.Key, out var value) && value == kvp.Value)))
			{
				WhenConditionsVariants.Add(new Dictionary<string, string>(conditions));
			}
		}
	}

	public string GetEventScript()
	{
		return eventScript;
	}

	public void UpdateScript(string newScript)
	{
		if (!string.IsNullOrEmpty(newScript))
		{
			HasInvalidScript = false;
			ParseEventScript(newScript);
		}
	}

	public EventData(string id, string locationName, string preconditions, string eventScript = null)
	{
		Id = id;
		LocationName = locationName;
		RawPreconditions = preconditions;
		ParsePreconditions(preconditions);
		ParseEventScript(eventScript);
	}

	public bool HasBeenSeen()
	{
		return ((NetHashSet<string>)(object)Game1.player?.eventsSeen)?.Contains(Id) == true;
	}

	public bool AreConditionsMet()
	{
		if (IsMarkerEvent)
		{
			return false;
		}
		string rawPreconditions = RawPreconditions;
		if (rawPreconditions == null || !rawPreconditions.Contains("/x "))
		{
			string rawPreconditions2 = RawPreconditions;
			if (rawPreconditions2 == null || !rawPreconditions2.StartsWith("x "))
			{
				string rawPreconditions3 = RawPreconditions;
				if (rawPreconditions3 == null || !rawPreconditions3.Contains("/r -1"))
				{
					string rawPreconditions4 = RawPreconditions;
					if (rawPreconditions4 == null || !rawPreconditions4.StartsWith("r -1"))
					{
						if (IsFromContentPatcher && HasUnresolvedTokens)
						{
							return false;
						}
						if (HasWhenConditions && !AreWhenConditionsMet())
						{
							return false;
						}
						if (string.IsNullOrEmpty(RawPreconditions) && !HasWhenConditions)
						{
							return false;
						}
						string modifiedPreconditions = GetModifiedPreconditions();
						if (string.IsNullOrEmpty(modifiedPreconditions) && !HasWhenConditions)
						{
							return false;
						}
						if (modifiedPreconditions.Contains("{{") && modifiedPreconditions.Contains("}}"))
						{
							return false;
						}
						try
						{
							GameLocation val = Game1.getLocationFromName(LocationName);
							if (val == null)
							{
								ModConfig config = MHEventsListMod.Config;
								if (config != null && config.EvaluateUnloadedLocations)
								{
									val = Game1.currentLocation;
								}
							}
							if (val != null)
							{
								string text = val.checkEventPrecondition("-8888888/" + modifiedPreconditions, false);
								return text != "-1";
							}
						}
						catch
						{
						}
						return MHEventsListMod.Config?.EvaluateUnloadedLocations ?? false;
					}
				}
				return false;
			}
		}
		return false;
	}

	public bool AreWhenConditionsMet()
	{
		if (!HasWhenConditions)
		{
			return true;
		}
		ContentPatcherIntegration contentPatcher = MHEventsListMod.ContentPatcher;
		if (contentPatcher != null && contentPatcher.IsAvailable && contentPatcher.IsReady)
		{
			foreach (Dictionary<string, string> whenConditionsVariant in WhenConditionsVariants)
			{
				if (whenConditionsVariant != null && whenConditionsVariant.Count != 0)
				{
					var (flag, flag2, _) = contentPatcher.EvaluateConditions(whenConditionsVariant);
					if (flag && flag2)
					{
						return true;
					}
				}
			}
			return false;
		}
		foreach (Dictionary<string, string> whenConditionsVariant2 in WhenConditionsVariants)
		{
			if (whenConditionsVariant2 == null || whenConditionsVariant2.Count == 0)
			{
				continue;
			}
			bool flag3 = true;
			bool flag4 = false;
			foreach (KeyValuePair<string, string> item in whenConditionsVariant2)
			{
				string key = item.Key;
				string value = item.Value;
				var (flag5, flag6) = EvaluateWhenCondition(key, value);
				if (flag5 && !flag6)
				{
					flag3 = false;
					break;
				}
				if (!flag5)
				{
					flag4 = true;
				}
			}
			if (flag3 && !flag4)
			{
				return true;
			}
		}
		return false;
	}

	private (bool canEvaluate, bool isMet) EvaluateWhenCondition(string key, string expectedValue)
	{
		try
		{
			if (key.Contains("{{") && key.Contains("}}"))
			{
				return (canEvaluate: false, isMet: false);
			}
			string text = key;
			string text2 = "";
			bool flag = false;
			if (key.Contains("|contains="))
			{
				int num = key.IndexOf("|contains=");
				text = key.Substring(0, num).Trim();
				text2 = key.Substring(num + 10).Trim();
				flag = true;
			}
			else if (key.Contains("|contains"))
			{
				text = key[..key.IndexOf("|contains")].Trim();
				flag = true;
				text2 = expectedValue;
			}
			else if (key.Contains(":"))
			{
				int num2 = key.IndexOf(':');
				string text3 = key.Substring(0, num2).Trim();
				if (text3 == "Query")
				{
					string expression = key.Substring(num2 + 1).Trim();
					return EvaluateQueryExpression(expression, expectedValue);
				}
				if (text3 == "Hearts")
				{
					string npcName = key.Substring(num2 + 1).Trim();
					return EvaluateHeartsRange(npcName, expectedValue);
				}
			}
			bool flag2 = expectedValue.Equals("true", StringComparison.OrdinalIgnoreCase) || expectedValue == "true";
			bool flag3 = false;
			switch (text)
			{
			case "HasSeenEvent":
				flag3 = ((!flag || string.IsNullOrEmpty(text2)) ? ((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(expectedValue) : ((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(text2));
				break;
			case "HasFlag":
			case "HasReadLetter":
				flag3 = ((!flag || string.IsNullOrEmpty(text2)) ? ((NetHashSet<string>)(object)Game1.player.mailReceived).Contains(expectedValue) : ((NetHashSet<string>)(object)Game1.player.mailReceived).Contains(text2));
				break;
			case "HasActiveQuest":
			{
				string text7 = (flag ? text2 : expectedValue);
				flag3 = Game1.player.hasQuest(text7);
				break;
			}
			case "PlayerGender":
			{
				string value = (Game1.player.IsMale ? "Male" : "Female");
				flag3 = expectedValue.Equals(value, StringComparison.OrdinalIgnoreCase);
				break;
			}
			case "Season":
				flag3 = Game1.currentSeason.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
				break;
			case "Weather":
			{
				bool flag4 = !Game1.isRaining && !Game1.isSnowing && !Game1.isLightning;
				bool flag5 = Game1.isRaining && !Game1.isLightning;
				bool isLightning = Game1.isLightning;
				bool isSnowing = Game1.isSnowing;
				bool isDebrisWeather = Game1.isDebrisWeather;
				string text4 = expectedValue.ToLower();
				flag3 = ((text4 == "sun" || text4 == "sunny") && flag4) || ((text4 == "rain" || text4 == "rainy") && flag5) || ((text4 == "storm" || text4 == "stormy") && isLightning) || ((text4 == "snow" || text4 == "snowy") && isSnowing) || ((text4 == "wind" || text4 == "windy") && isDebrisWeather);
				break;
			}
			case "DayOfWeek":
			{
				string dayName = Game1.Date.DayOfWeek.ToString();
				if (flag)
				{
					string text6 = ((!string.IsNullOrEmpty(text2)) ? text2 : expectedValue);
					flag3 = text6.Contains(dayName, StringComparison.OrdinalIgnoreCase);
					break;
				}
				IEnumerable<string> source = from d in expectedValue.Split(',')
					select d.Trim();
				flag3 = source.Any((string d) => d.Equals(dayName, StringComparison.OrdinalIgnoreCase));
				break;
			}
			case "Spouse":
			{
				string text5 = Game1.player.spouse ?? "";
				if (flag)
				{
					string value2 = ((!string.IsNullOrEmpty(text2)) ? text2 : expectedValue);
					flag3 = text5.Contains(value2, StringComparison.OrdinalIgnoreCase);
				}
				else
				{
					flag3 = text5.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
				}
				break;
			}
			case "Relationship":
				return (canEvaluate: false, isMet: false);
			case "Year":
			{
				if (int.TryParse(expectedValue, out var result2))
				{
					flag3 = Game1.Date.Year == result2;
				}
				break;
			}
			case "Day":
			{
				if (int.TryParse(expectedValue, out var result))
				{
					flag3 = Game1.dayOfMonth == result;
				}
				break;
			}
			case "IsCommunityCenterComplete":
				flag3 = expectedValue.Equals(Game1.MasterPlayer.hasCompletedCommunityCenter().ToString(), StringComparison.OrdinalIgnoreCase);
				break;
			case "Query":
				return (canEvaluate: false, isMet: false);
			default:
				if (flag2)
				{
					flag3 = ((NetHashSet<string>)(object)Game1.player.mailReceived).Contains(text);
					break;
				}
				return (canEvaluate: false, isMet: false);
			}
			bool item = (flag2 ? flag3 : (!flag3));
			return (canEvaluate: true, isMet: item);
		}
		catch
		{
			return (canEvaluate: false, isMet: false);
		}
	}

	private (bool canEvaluate, bool isMet) EvaluateHeartsRange(string npcName, string rangeValue)
	{
		try
		{
			NPC characterFromName = Game1.getCharacterFromName(npcName, true, false);
			if (characterFromName == null)
			{
				return (canEvaluate: false, isMet: false);
			}
			int friendshipHeartLevelForNPC = Game1.player.getFriendshipHeartLevelForNPC(((Character)characterFromName).Name);
			List<int> list = new List<int>();
			string[] array = rangeValue.Split(',');
			foreach (string text in array)
			{
				string s = text.Trim();
				if (int.TryParse(s, out var result))
				{
					list.Add(result);
				}
			}
			if (list.Count > 0)
			{
				return (canEvaluate: true, isMet: list.Contains(friendshipHeartLevelForNPC));
			}
			return (canEvaluate: false, isMet: false);
		}
		catch
		{
			return (canEvaluate: false, isMet: false);
		}
	}

	private (bool canEvaluate, bool isMet) EvaluateQueryExpression(string expression, string expectedValue)
	{
		try
		{
			bool flag = expectedValue.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (expression.Contains(" AND ", StringComparison.OrdinalIgnoreCase))
			{
				string[] array = expression.Split(new string[1] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
				string[] array2 = array;
				foreach (string text in array2)
				{
					var (flag2, flag3) = EvaluateSingleQueryExpression(text.Trim());
					if (!flag2)
					{
						return (canEvaluate: false, isMet: false);
					}
					if (!flag3)
					{
						return (canEvaluate: true, isMet: !flag);
					}
				}
				return (canEvaluate: true, isMet: flag);
			}
			if (expression.Contains(" OR ", StringComparison.OrdinalIgnoreCase))
			{
				string[] array3 = expression.Split(new string[1] { " OR " }, StringSplitOptions.RemoveEmptyEntries);
				string[] array4 = array3;
				foreach (string text2 in array4)
				{
					var (flag4, flag5) = EvaluateSingleQueryExpression(text2.Trim());
					if (flag4 && flag5)
					{
						return (canEvaluate: true, isMet: flag);
					}
				}
				return (canEvaluate: true, isMet: !flag);
			}
			(bool, bool) tuple3 = EvaluateSingleQueryExpression(expression);
			if (!tuple3.Item1)
			{
				return (canEvaluate: false, isMet: false);
			}
			return (canEvaluate: true, isMet: flag ? tuple3.Item2 : (!tuple3.Item2));
		}
		catch
		{
			return (canEvaluate: false, isMet: false);
		}
	}

	private (bool canEvaluate, bool isMet) EvaluateSingleQueryExpression(string expression)
	{
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			expression = expression.Trim();
			if (expression.Contains("{{Hearts:"))
			{
				int num = expression.IndexOf("{{Hearts:") + 9;
				int num2 = expression.IndexOf("}}", num);
				if (num2 > num)
				{
					string text = expression.Substring(num, num2 - num);
					string comparisonExpr = expression.Substring(num2 + 2).Trim();
					int friendshipHeartLevelForNPC = Game1.player.getFriendshipHeartLevelForNPC(text);
					return EvaluateComparison(friendshipHeartLevelForNPC, comparisonExpr);
				}
			}
			if (expression.Contains("{{Time}}"))
			{
				int timeOfDay = Game1.timeOfDay;
				string comparisonExpr2 = expression.Replace("{{Time}}", "").Trim();
				return EvaluateComparison(timeOfDay, comparisonExpr2);
			}
			if (expression.Contains("{{DayOfWeek}}") && expression.Contains(" IN "))
			{
				string currentDay = Game1.Date.DayOfWeek.ToString();
				int num3 = expression.IndexOf(" IN ", StringComparison.OrdinalIgnoreCase);
				if (num3 > 0)
				{
					string text2 = expression.Substring(num3 + 4).Trim();
					text2 = text2.Trim('(', ')', ' ');
					List<string> source = (from d in text2.Split(',')
						select d.Trim().Trim('\'', '"', ' ')).ToList();
					bool item = source.Any((string d) => d.Equals(currentDay, StringComparison.OrdinalIgnoreCase));
					return (canEvaluate: true, isMet: item);
				}
			}
			if (expression.Contains("{{Season}}"))
			{
				string currentSeason = Game1.currentSeason;
				if (expression.Contains("=="))
				{
					string value = expression.Split(new string[1] { "==" }, StringSplitOptions.None)[1].Trim().Trim('"', '\'', ' ');
					return (canEvaluate: true, isMet: currentSeason.Equals(value, StringComparison.OrdinalIgnoreCase));
				}
			}
			if (expression.Contains("{{DaysPlayed}}"))
			{
				int daysPlayed = (int)Game1.stats.DaysPlayed;
				string comparisonExpr3 = expression.Replace("{{DaysPlayed}}", "").Trim();
				return EvaluateComparison(daysPlayed, comparisonExpr3);
			}
			if (expression.Contains("{{Year}}"))
			{
				int year = Game1.Date.Year;
				string comparisonExpr4 = expression.Replace("{{Year}}", "").Trim();
				return EvaluateComparison(year, comparisonExpr4);
			}
			if (expression.Contains("{{SpouseGender}}"))
			{
				string text3 = "undefined";
				if (Game1.player.isMarriedOrRoommates())
				{
					NPC spouse = Game1.player.getSpouse();
					if (spouse != null)
					{
						text3 = (((int)((Character)spouse).Gender == 0) ? "male" : "female");
					}
				}
				if (expression.Contains("="))
				{
					string[] array = expression.Split('=');
					if (array.Length >= 2)
					{
						string value2 = array[^1].Trim().Trim('\'', '"', ' ');
						return (canEvaluate: true, isMet: text3.Equals(value2, StringComparison.OrdinalIgnoreCase));
					}
				}
			}
			expression.Contains("{{Random");
			return (canEvaluate: false, isMet: false);
		}
		catch
		{
			return (canEvaluate: false, isMet: false);
		}
	}

	private (bool canEvaluate, bool isMet) EvaluateComparison(int actualValue, string comparisonExpr)
	{
		try
		{
			comparisonExpr = comparisonExpr.Trim();
			string text = "";
			string text2 = "";
			if (comparisonExpr.StartsWith(">="))
			{
				text = ">=";
				text2 = comparisonExpr.Substring(2).Trim();
			}
			else if (comparisonExpr.StartsWith("<="))
			{
				text = "<=";
				text2 = comparisonExpr.Substring(2).Trim();
			}
			else if (comparisonExpr.StartsWith(">"))
			{
				text = ">";
				text2 = comparisonExpr.Substring(1).Trim();
			}
			else if (comparisonExpr.StartsWith("<"))
			{
				text = "<";
				text2 = comparisonExpr.Substring(1).Trim();
			}
			else if (comparisonExpr.StartsWith("=="))
			{
				text = "==";
				text2 = comparisonExpr.Substring(2).Trim();
			}
			else
			{
				if (!comparisonExpr.StartsWith("="))
				{
					return (canEvaluate: false, isMet: false);
				}
				text = "=";
				text2 = comparisonExpr.Substring(1).Trim();
			}
			if (!int.TryParse(text2, out var result))
			{
				return (canEvaluate: false, isMet: false);
			}
			return (canEvaluate: true, isMet: text switch
			{
				">=" => actualValue >= result, 
				"<=" => actualValue <= result, 
				">" => actualValue > result, 
				"<" => actualValue < result, 
				"==" => actualValue == result, 
				"=" => actualValue == result, 
				_ => false, 
			});
		}
		catch
		{
			return (canEvaluate: false, isMet: false);
		}
	}

	public string GetModifiedPreconditions()
	{
		if (string.IsNullOrEmpty(RawPreconditions))
		{
			return "";
		}
		string[] array = RawPreconditions.Split('/');
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			string text2 = text.Trim();
			if (text2.Length != 0)
			{
				string text3 = ConvertExtendedCondition(text2);
				if (text3 != null && !ShouldSkipCondition(text3))
				{
					list.Add(text3);
				}
			}
		}
		return string.Join("/", list);
	}

	private string ConvertExtendedCondition(string condition)
	{
		string[] array = condition.Split(' ');
		string text = array[0];
		bool flag = text.StartsWith("!");
		if (flag)
		{
			text = text.Substring(1);
		}
		if (text.Equals("Friendship", StringComparison.OrdinalIgnoreCase) && array.Length >= 3)
		{
			return "f " + string.Join(" ", array.Skip(1));
		}
		if ((text.Equals("SawEvent", StringComparison.OrdinalIgnoreCase) || text.Equals("SeenEvent", StringComparison.OrdinalIgnoreCase)) && array.Length >= 2)
		{
			string text2 = string.Join(" ", array.Skip(1));
			if (!flag)
			{
				return "e " + text2;
			}
			return "k " + text2;
		}
		if ((text.Equals("NotSawEvent", StringComparison.OrdinalIgnoreCase) || text.Equals("NotSeenEvent", StringComparison.OrdinalIgnoreCase)) && array.Length >= 2)
		{
			return "k " + array[1];
		}
		if (text.Equals("Hearts", StringComparison.OrdinalIgnoreCase) && array.Length >= 3 && int.TryParse(array[2], out var result))
		{
			return $"f {array[1]} {result * 250}";
		}
		if (text.Equals("Time", StringComparison.OrdinalIgnoreCase) && array.Length >= 3)
		{
			return "t " + array[1] + " " + array[2];
		}
		if (text.Equals("Weather", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "w " + array[1];
		}
		if (text.Equals("Random", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "r " + array[1];
		}
		if (text.Equals("DayOfWeek", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("Day", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "d " + string.Join(" ", array.Skip(1));
		}
		if (text.Equals("Season", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("Year", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "y " + array[1];
		}
		if (text.Equals("HasSeenEvent", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			string text3 = array[1].Split('|')[0];
			if (!flag)
			{
				return "e " + text3;
			}
			return "k " + text3;
		}
		if (text.Equals("HasFlag", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			string text4 = array[1].Split('|')[0];
			if (!flag)
			{
				return "h " + text4;
			}
			return "l " + text4;
		}
		if (text.Equals("Gender", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "g " + array[1];
		}
		if (text.Equals("LocalMail", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("ActiveDialogueEvent", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("DayOfMonth", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "u " + string.Join(" ", array.Skip(1));
		}
		if (text.Equals("MinutesPlayed", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("DaysPlayed", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("IsGreenRain", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("IsFestival", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("HasRecipe", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("SkillLevel", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("HasProfession", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if ((text.Equals("HasDialogueAnswer", StringComparison.OrdinalIgnoreCase) || text.Equals("ChoseDialogueAnswers", StringComparison.OrdinalIgnoreCase)) && array.Length >= 2)
		{
			return "q " + string.Join(" ", array.Skip(1));
		}
		if (text.Equals("CommunityCenterOrWarehouseDone", StringComparison.OrdinalIgnoreCase))
		{
			return "C";
		}
		if (text.Equals("NotCommunityCenterOrWarehouseDone", StringComparison.OrdinalIgnoreCase))
		{
			return "X";
		}
		if (text.Equals("JojaBundlesDone", StringComparison.OrdinalIgnoreCase))
		{
			return "J";
		}
		if (text.Equals("GoldenWalnuts", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "N " + array[1];
		}
		if (text.Equals("InUpgradedHouse", StringComparison.OrdinalIgnoreCase))
		{
			string text5 = ((array.Length >= 2) ? array[1] : "2");
			return "L " + text5;
		}
		if (text.Equals("NPCVisible", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "v " + array[1];
		}
		if (text.Equals("NpcVisibleHere", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "p " + array[1];
		}
		if (text.Equals("FestivalDay", StringComparison.OrdinalIgnoreCase))
		{
			return condition;
		}
		if (text.Equals("NotFestivalDay", StringComparison.OrdinalIgnoreCase))
		{
			return "F";
		}
		if (text.Equals("UpcomingFestival", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return condition;
		}
		if (text.Equals("NotUpcomingFestival", StringComparison.OrdinalIgnoreCase))
		{
			return "U " + ((array.Length >= 2) ? array[1] : "0");
		}
		if (text.Equals("Spouse", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "O " + array[1];
		}
		if (text.Equals("NotSpouse", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "o " + array[1];
		}
		if (text.Equals("Dating", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "D " + array[1];
		}
		if (text.Equals("Roommate", StringComparison.OrdinalIgnoreCase))
		{
			return "R";
		}
		if (text.Equals("NotRoommate", StringComparison.OrdinalIgnoreCase))
		{
			return "Rf";
		}
		if (text.Equals("SpouseBed", StringComparison.OrdinalIgnoreCase))
		{
			return "B";
		}
		if (text.Equals("IsHost", StringComparison.OrdinalIgnoreCase))
		{
			return "H";
		}
		if (text.Equals("HostMail", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "Hn " + array[1];
		}
		if (text.Equals("NotHostMail", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "Hl " + array[1];
		}
		if (text.Equals("HostOrLocalMail", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "*n " + array[1];
		}
		if (text.Equals("NotHostOrLocalMail", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "*l " + array[1];
		}
		if (text.Equals("NotLocalMail", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "l " + array[1];
		}
		if (text.Equals("NotDayOfWeek", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "d " + string.Join(" ", array.Skip(1));
		}
		if (text.Equals("NotActiveDialogueEvent", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "A " + array[1];
		}
		if (text.Equals("Skill", StringComparison.OrdinalIgnoreCase) && array.Length >= 3)
		{
			return condition;
		}
		if (text.Equals("ReachedMineBottom", StringComparison.OrdinalIgnoreCase))
		{
			string text6 = ((array.Length >= 2) ? array[1] : "1");
			return "b " + text6;
		}
		if (text.Equals("EarnedMoney", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "m " + array[1];
		}
		if (text.Equals("HasMoney", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "M " + array[1];
		}
		if (text.Equals("FreeInventorySlots", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "c " + array[1];
		}
		if (text.Equals("HasItem", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "i " + array[1];
		}
		if (text.Equals("Shipped", StringComparison.OrdinalIgnoreCase) && array.Length >= 3)
		{
			return "s " + string.Join(" ", array.Skip(1));
		}
		if (text.Equals("SawSecretNote", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "S " + array[1];
		}
		if (text.Equals("Tile", StringComparison.OrdinalIgnoreCase) && array.Length >= 3)
		{
			return "a " + string.Join(" ", array.Skip(1));
		}
		if (text.Equals("MissingPet", StringComparison.OrdinalIgnoreCase))
		{
			if (array.Length < 2)
			{
				return "h";
			}
			return "h " + array[1];
		}
		if (text.Equals("WorldState", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "* " + array[1];
		}
		if (text.Equals("SendMail", StringComparison.OrdinalIgnoreCase) && array.Length >= 2)
		{
			return "x " + string.Join(" ", array.Skip(1));
		}
		return condition;
	}

	private bool ShouldSkipCondition(string condition)
	{
		if (condition.Contains("{{") || condition.Contains("}}") || (condition.Contains("{") && condition.Contains("}")))
		{
			return true;
		}
		string[] array = condition.Split(' ');
		string text = array[0].TrimStart('!');
		switch (text.ToLower())
		{
		case "allow":
		case "query":
		case "hearts":
		case "season":
		case "ltt":
		case "year":
		case "dayofweek":
		case "relationship":
			return true;
		default:
		{
			if (char.IsDigit(text[0]))
			{
				return true;
			}
			if (array.Length == 2 && char.IsUpper(text[0]) && int.TryParse(array[1], out var _) && text.Length > 1)
			{
				return true;
			}
			if (condition.Length >= 1)
			{
				switch (condition[0])
				{
				case 'c':
				case 'i':
				case 'p':
				case 'r':
				case 't':
					return true;
				}
			}
			return false;
		}
		}
	}

	public string GetLocationDisplayName(bool hideUnseen = true)
	{
		if (hideUnseen && MHEventsListMod.Config.HideUnseenLocations && !((NetHashSet<string>)(object)Game1.player.locationsVisited).Contains(LocationName))
		{
			return "???";
		}
		GameLocation locationFromName = Game1.getLocationFromName(LocationName);
		if (locationFromName != null)
		{
			return locationFromName.DisplayName;
		}
		Translation val = MHEventsListMod.Helper.Translation.Get("loc." + LocationName);
		if (val.HasValue())
		{
			return ((object)val).ToString();
		}
		return LocationName;
	}

	public string GetTimeRange()
	{
		if (StartTime < 0 || EndTime < 0)
		{
			return null;
		}
		return FormatTime(StartTime) + "-" + FormatTime(EndTime);
	}

	public string GetConditionsSummary()
	{
		List<string> list = new List<string>();
		string timeRange = GetTimeRange();
		if (!string.IsNullOrEmpty(timeRange))
		{
			list.Add(timeRange);
		}
		if (RequiredFreeSlots > 0)
		{
			list.Add($"{RequiredFreeSlots} slots");
		}
		if (RequiredItems != null && RequiredItems.Count > 0)
		{
			List<string> list2 = new List<string>();
			foreach (string requiredItem in RequiredItems)
			{
				ParsedItemData data = ItemRegistry.GetData(requiredItem);
				if (data == null && int.TryParse(requiredItem, out var _))
				{
					data = ItemRegistry.GetData("(O)" + requiredItem);
				}
				list2.Add(data?.DisplayName ?? requiredItem);
			}
			list.Add(string.Join(", ", list2));
		}
		if (HeartRequirements != null && HeartRequirements.Count > 0)
		{
			List<string> list3 = new List<string>();
			foreach (CharacterHeartRequirement heartRequirement in HeartRequirements)
			{
				string displayName = heartRequirement.GetDisplayName();
				if (!string.IsNullOrWhiteSpace(displayName))
				{
					if (heartRequirement.Type == RequirementType.Dating)
					{
						list3.Add(displayName + " (D)");
					}
					else if (heartRequirement.Type == RequirementType.Married)
					{
						list3.Add(displayName + " (M)");
					}
					else if (heartRequirement.Hearts > 0)
					{
						list3.Add($"{displayName} {heartRequirement.Hearts}h");
					}
					else
					{
						list3.Add(displayName);
					}
				}
			}
			if (list3.Count > 0)
			{
				list.Add(string.Join(", ", list3));
			}
		}
		if (list.Count == 0)
		{
			if (HasWhenConditions)
			{
				return Translation.op_Implicit(MHEventsListMod.I18n.Get("list.requiresConditions"));
			}
			return Translation.op_Implicit(MHEventsListMod.I18n.Get("list.goToLocation"));
		}
		return string.Join(" | ", list);
	}

	public string GetPrimaryNpc()
	{
		if (HeartRequirements != null && HeartRequirements.Count > 0)
		{
			return HeartRequirements[0].NpcName;
		}
		return null;
	}

	public string GetTranslatedLocation()
	{
		if (MHEventsListMod.Config.HideUnseenLocations && !((NetHashSet<string>)(object)Game1.player.locationsVisited).Contains(LocationName))
		{
			return "???";
		}
		GameLocation locationFromName = Game1.getLocationFromName(LocationName);
		if (locationFromName != null && !string.IsNullOrEmpty(locationFromName.DisplayName))
		{
			return locationFromName.DisplayName;
		}
		Translation val = MHEventsListMod.Helper.Translation.Get("loc." + LocationName);
		if (val.HasValue())
		{
			return ((object)val).ToString();
		}
		return LocationName;
	}

	public string GetNpcSummary()
	{
		if (HeartRequirements == null || HeartRequirements.Count == 0)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (CharacterHeartRequirement heartRequirement in HeartRequirements)
		{
			string displayName = heartRequirement.GetDisplayName();
			if (string.IsNullOrWhiteSpace(displayName))
			{
				continue;
			}
			displayName = char.ToUpper(displayName[0]) + displayName.Substring(1);
			switch (heartRequirement.Type)
			{
			case RequirementType.Dating:
				list.Add(displayName + " (dating)");
				continue;
			case RequirementType.Married:
				list.Add(displayName + " (married)");
				continue;
			}
			int hearts = heartRequirement.Hearts;
			if (hearts > 0)
			{
				list.Add($"{displayName} ({hearts}h)");
			}
			else
			{
				list.Add(displayName);
			}
		}
		return string.Join(", ", list);
	}

	private void ParsePreconditions(string preconditions)
	{
		if (string.IsNullOrEmpty(preconditions))
		{
			return;
		}
		string[] array = preconditions.Split('/');
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			string text2 = text.Trim();
			if (text2.Length == 0)
			{
				continue;
			}
			string[] array3 = text2.Split(' ');
			string text3 = array3[0];
			if (text3.Equals("Friendship", StringComparison.OrdinalIgnoreCase) || text3.Equals("SawEvent", StringComparison.OrdinalIgnoreCase) || text3.Equals("SeenEvent", StringComparison.OrdinalIgnoreCase) || text3.Equals("NotSawEvent", StringComparison.OrdinalIgnoreCase) || text3.Equals("NotSeenEvent", StringComparison.OrdinalIgnoreCase) || text3.Equals("Hearts", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			if (text3.Equals("Time", StringComparison.OrdinalIgnoreCase))
			{
				if (array3.Length >= 3)
				{
					int.TryParse(array3[1], out var result);
					int.TryParse(array3[2], out var result2);
					StartTime = result;
					EndTime = result2;
				}
			}
			else
			{
				if (text3.Equals("Season", StringComparison.OrdinalIgnoreCase) || text3.Equals("Weather", StringComparison.OrdinalIgnoreCase) || text3.Equals("Day", StringComparison.OrdinalIgnoreCase) || text3.Equals("DayOfWeek", StringComparison.OrdinalIgnoreCase) || text3.Equals("Year", StringComparison.OrdinalIgnoreCase) || text3.Equals("Random", StringComparison.OrdinalIgnoreCase) || text3.Equals("Gender", StringComparison.OrdinalIgnoreCase) || text3.Equals("HasSeenEvent", StringComparison.OrdinalIgnoreCase) || text3.Equals("HasFlag", StringComparison.OrdinalIgnoreCase) || text3.Equals("LocalMail", StringComparison.OrdinalIgnoreCase) || text3.Equals("ActiveDialogueEvent", StringComparison.OrdinalIgnoreCase) || text2.Length == 0)
				{
					continue;
				}
				switch (text2[0])
				{
				case 't':
					if (array3.Length >= 3)
					{
						int.TryParse(array3[1], out var result4);
						int.TryParse(array3[2], out var result5);
						StartTime = result4;
						EndTime = result5;
					}
					break;
				case 'c':
				{
					if (array3.Length >= 2 && int.TryParse(array3[1], out var result6))
					{
						RequiredFreeSlots = ((result6 > 0) ? result6 : 0);
					}
					break;
				}
				case 'i':
					if (array3.Length >= 2)
					{
						if (RequiredItems == null)
						{
							List<string> list3 = (RequiredItems = new List<string>());
						}
						RequiredItems.Add(array3[1]);
					}
					break;
				case 'p':
					if (array3.Length >= 2)
					{
						if (RequiredNpcsPresent == null)
						{
							List<string> list3 = (RequiredNpcsPresent = new List<string>());
						}
						RequiredNpcsPresent.Add(array3[1].ToLower());
					}
					break;
				case 'f':
				{
					if (array3.Length < 3)
					{
						break;
					}
					if (HeartRequirements == null)
					{
						List<CharacterHeartRequirement> list = (HeartRequirements = new List<CharacterHeartRequirement>());
					}
					for (int j = 1; j + 1 < array3.Length; j += 2)
					{
						if (int.TryParse(array3[j + 1], out var result3))
						{
							HeartRequirements.Add(new CharacterHeartRequirement(array3[j].ToLower(), result3));
						}
					}
					break;
				}
				case 'D':
					if (array3.Length >= 2)
					{
						if (HeartRequirements == null)
						{
							List<CharacterHeartRequirement> list = (HeartRequirements = new List<CharacterHeartRequirement>());
						}
						HeartRequirements.Add(new CharacterHeartRequirement(array3[1].ToLower(), 2000, RequirementType.Dating));
					}
					break;
				case 'O':
					if (array3.Length >= 2)
					{
						if (HeartRequirements == null)
						{
							List<CharacterHeartRequirement> list = (HeartRequirements = new List<CharacterHeartRequirement>());
						}
						HeartRequirements.Add(new CharacterHeartRequirement(array3[1].ToLower(), 2500, RequirementType.Married));
					}
					break;
				}
			}
		}
	}

	private void ParseEventScript(string script)
	{
		eventScript = script?.Replace("\\\"", "\"");
		if (string.IsNullOrEmpty(script))
		{
			return;
		}
		if (script.Contains("{{") && script.Contains("}}"))
		{
			HasUnresolvedTokens = true;
		}
		bool flag = script.Contains("/speak ") || script.Contains("/message ") || script.Contains("/question ") || script.Contains("/dialogue ") || script.Contains("/showFrame ") || script.Contains("/animate ");
		bool flag2 = script.Length < 150;
		if (!flag && flag2)
		{
			IsMarkerEvent = true;
		}
		else if (!flag && (script.Contains("addMailReceived") || script.Contains("addMailToMailbox")) && !script.Contains("viewport"))
		{
			IsMarkerEvent = true;
		}
		if (script.Contains("end newDay"))
		{
			EndsDay = true;
		}
		HashSet<string> hashSet = new HashSet<string>();
		string[] array = script.Split(new string[1] { "/speak " }, StringSplitOptions.None);
		for (int i = 1; i < array.Length; i++)
		{
			int num = array[i].IndexOf(' ');
			if (num > 0)
			{
				string text = array[i].Substring(0, num);
				hashSet.Add(text.ToLower());
			}
		}
		if (hashSet.Count <= 0)
		{
			return;
		}
		if (HeartRequirements == null)
		{
			List<CharacterHeartRequirement> list = (HeartRequirements = new List<CharacterHeartRequirement>());
		}
		foreach (string item in hashSet)
		{
			HeartRequirements.Add(new CharacterHeartRequirement(item, 0));
		}
	}

	public List<string> GetEventActionsSummary()
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(eventScript))
		{
			return list;
		}
		try
		{
			string[] array = eventScript.Split('/');
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Trim().Split(' ');
				if (array3.Length == 0)
				{
					continue;
				}
				switch (array3[0])
				{
				case "addConversationTopic":
					if (array3.Length >= 2)
					{
						string topic = array3[1];
						int result3;
						int num2 = ((array3.Length >= 3 && int.TryParse(array3[2], out result3)) ? result3 : 0);
						if (num2 > 0)
						{
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.addTimegate", (object)new
							{
								topic = topic,
								days = num2
							})));
						}
						else
						{
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.addTopic", (object)new { topic })));
						}
					}
					break;
				case "friendship":
					if (array3.Length >= 3)
					{
						string npc = array3[1];
						if (int.TryParse(array3[2], out var result))
						{
							int num = Math.Abs(result / 250);
							string sign = ((result > 0) ? "+" : "");
							string text2 = ((num == 1) ? "action.friendship" : "action.friendshipPlural");
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get(text2, (object)new
							{
								npc = npc,
								sign = sign,
								hearts = num
							})));
						}
					}
					break;
				case "addQuest":
					if (array3.Length >= 2)
					{
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.addQuest", (object)new
						{
							quest = array3[1]
						})));
					}
					break;
				case "removeQuest":
					if (array3.Length >= 2)
					{
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.removeQuest", (object)new
						{
							quest = array3[1]
						})));
					}
					break;
				case "addMail":
					if (array3.Length >= 2)
					{
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.addMail", (object)new
						{
							mail = array3[1]
						})));
					}
					break;
				case "mailToday":
					if (array3.Length >= 2)
					{
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.mailToday", (object)new
						{
							mail = array3[1]
						})));
					}
					break;
				case "mailReceived":
				case "addMailReceived":
					if (array3.Length >= 2)
					{
						if (array3.Length < 3 || array3[2] != "false")
						{
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.mailFlag", (object)new
							{
								flag = array3[1]
							})));
						}
						else
						{
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.removeMailFlag", (object)new
							{
								flag = array3[1]
							})));
						}
					}
					break;
				case "money":
				{
					if (array3.Length >= 2 && int.TryParse(array3[1], out var result4))
					{
						string sign2 = ((result4 > 0) ? "+" : "");
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.money", (object)new
						{
							sign = sign2,
							amount = Math.Abs(result4)
						})));
					}
					break;
				}
				case "addItem":
				case "addObject":
					if (array3.Length >= 2)
					{
						string itemId3 = array3[1];
						int result5;
						int count2 = ((array3.Length < 3 || !int.TryParse(array3[2], out result5)) ? 1 : result5);
						string itemName3 = GetItemName(itemId3);
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.addItem", (object)new
						{
							item = itemName3,
							count = count2
						})));
					}
					break;
				case "removeItem":
					if (array3.Length >= 2)
					{
						string itemId2 = array3[1];
						int result2;
						int count = ((array3.Length < 3 || !int.TryParse(array3[2], out result2)) ? 1 : result2);
						string itemName2 = GetItemName(itemId2);
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.removeItem", (object)new
						{
							item = itemName2,
							count = count
						})));
					}
					break;
				case "itemAboveHead":
					if (array3.Length >= 2)
					{
						string itemId = array3[1];
						string itemName = GetItemName(itemId);
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.itemAboveHead", (object)new
						{
							item = itemName
						})));
					}
					break;
				case "addCookingRecipe":
					if (array3.Length >= 2)
					{
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.addCookingRecipe", (object)new
						{
							recipe = array3[1]
						})));
					}
					break;
				case "addCraftingRecipe":
					if (array3.Length >= 2)
					{
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.addCraftingRecipe", (object)new
						{
							recipe = array3[1]
						})));
					}
					break;
				case "addSpecialOrder":
					if (array3.Length >= 2)
					{
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.addSpecialOrder", (object)new
						{
							order = array3[1]
						})));
					}
					break;
				case "removeSpecialOrder":
					if (array3.Length >= 2)
					{
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.removeSpecialOrder", (object)new
						{
							order = array3[1]
						})));
					}
					break;
				case "awardFestivalPrize":
					if (array3.Length >= 2)
					{
						string prize = array3[1];
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.festivalPrize", (object)new { prize })));
					}
					break;
				case "eventSeen":
					if (array3.Length >= 2)
					{
						if (array3.Length < 3 || array3[2] != "false")
						{
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.eventSeen", (object)new
							{
								eventId = array3[1]
							})));
						}
						else
						{
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.eventUnseen", (object)new
							{
								eventId = array3[1]
							})));
						}
					}
					break;
				case "questionAnswered":
					if (array3.Length >= 2 && (array3.Length < 3 || array3[2] != "false"))
					{
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.questionAnswered", (object)new
						{
							answer = array3[1]
						})));
					}
					break;
				case "rustyKey":
					list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.rustyKey")));
					break;
				case "cave":
					list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.caveChoice")));
					break;
				case "animalNaming":
					list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.animalNaming")));
					break;
				case "catQuestion":
					list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.petAdoption")));
					break;
				case "changeName":
					if (array3.Length >= 3)
					{
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.changeName", (object)new
						{
							actor = array3[1],
							name = array3[2]
						})));
					}
					break;
				case "changeSprite":
					if (array3.Length >= 2)
					{
						string sprite = ((array3.Length >= 3) ? array3[2] : "default");
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.changeSprite", (object)new
						{
							actor = array3[1],
							sprite = sprite
						})));
					}
					break;
				case "changePortrait":
					if (array3.Length >= 2)
					{
						string portrait = ((array3.Length >= 3) ? array3[2] : "default");
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.changePortrait", (object)new
						{
							actor = array3[1],
							portrait = portrait
						})));
					}
					break;
				case "end":
					if (array3.Length >= 2)
					{
						if (array3[1] == "newDay")
						{
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.endDay")));
						}
						else if (array3[1] == "bed")
						{
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.endBed")));
						}
						else if (array3[1] == "position" && array3.Length >= 4)
						{
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.endPosition", (object)new
							{
								x = array3[2],
								y = array3[3]
							})));
						}
						else if (array3[1] == "warpOut")
						{
							list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.endWarpOut")));
						}
					}
					break;
				case "warp":
					if (array3.Length >= 4)
					{
						string location = ((array3.Length >= 5) ? string.Join(" ", array3.Skip(4)) : "???");
						list.Add(Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("action.warp", (object)new { location })));
					}
					break;
				}
			}
		}
		catch
		{
		}
		return list;
	}

	private static string GetItemName(string itemId)
	{
		try
		{
			ParsedItemData data = ItemRegistry.GetData(itemId);
			if (data == null && int.TryParse(itemId, out var _))
			{
				data = ItemRegistry.GetData("(O)" + itemId);
			}
			return data?.DisplayName ?? itemId;
		}
		catch
		{
			return itemId;
		}
	}

	private static string FormatTime(int time)
	{
		if (MHEventsListMod.Config.Use24HourClock)
		{
			int num = time % 2400;
			int value = num / 100;
			int value2 = num % 100;
			return $"{value}:{value2:D2}";
		}
		int num2 = time % 2400;
		int num3 = num2 / 100;
		int value3 = num2 % 100;
		string value4 = ((num3 >= 12) ? "pm" : "am");
		if (num3 > 12)
		{
			num3 -= 12;
		}
		if (num3 == 0)
		{
			num3 = 12;
		}
		return $"{num3}:{value3:D2}{value4}";
	}

	public void WriteAllConditions()
	{
		ITranslationHelper i18n = MHEventsListMod.I18n;
		string[] array = RawPreconditions.Split('/');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string text2 = text.Trim();
			if (string.IsNullOrEmpty(text2))
			{
				continue;
			}
			if (text2.StartsWith("x ") || (text2.Contains("{{") && text2.Contains("}}")))
			{
				MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.conditionSkipped", (object)new
				{
					condition = text2,
					translation = ConditionTranslator.Translate(text2)
				})), (LogLevel)2);
				continue;
			}
			bool flag = false;
			try
			{
				GameLocation locationFromName = Game1.getLocationFromName(LocationName);
				if (locationFromName != null)
				{
					string text3 = locationFromName.checkEventPrecondition("-8888888/" + text2, false);
					flag = text3 != "-1";
				}
			}
			catch (Exception)
			{
				flag = false;
			}
			string translation = ConditionTranslator.Translate(text2);
			if (flag)
			{
				MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.conditionSatisfied", (object)new
				{
					condition = text2,
					translation = translation
				})), (LogLevel)2);
			}
			else
			{
				MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.conditionNotSatisfied", (object)new
				{
					condition = text2,
					translation = translation
				})), (LogLevel)2);
			}
		}
	}
}
