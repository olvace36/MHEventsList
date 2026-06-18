using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MHEventsList.Config;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Locations;

namespace MHEventsList.Core;

public static class EventRegistry
{
	private static readonly Dictionary<string, EventData> EventsById = new Dictionary<string, EventData>();

	private static readonly Dictionary<string, List<EventData>> EventsByNpc = new Dictionary<string, List<EventData>>();

	private static readonly List<EventData> AllEvents = new List<EventData>();

	private static readonly Dictionary<string, string> LogNamesByEventId = new Dictionary<string, string>();

	private static readonly Dictionary<string, List<Dictionary<string, string>>> WhenConditionsByEventId = new Dictionary<string, List<Dictionary<string, string>>>();

	private static readonly Dictionary<string, string> ModPathByEventId = new Dictionary<string, string>();

	private static bool isInitialized = false;

	private static readonly HashSet<string> HiddenEventIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "-999999", "-8888888" };

	internal static readonly Dictionary<string, string> IdPrefixToModName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	internal static readonly HashSet<string> VanillaEventIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
		"10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
		"20", "21", "25", "26", "27", "29", "33", "34", "35", "36",
		"38", "39", "40", "43", "44", "45", "46", "47", "50", "51",
		"52", "53", "54", "55", "56", "57", "58", "63", "65", "66",
		"67", "91", "92", "93", "94", "95", "96", "97", "100", "101",
		"102", "112", "60367", "100162", "150938", "181928", "191393", "195012", "195013", "195019",
		"195099", "233104", "288847", "318560", "371652", "384882", "384883", "404798", "418172", "423502",
		"463391", "471942", "502261", "502969", "503180", "520702", "528052", "529952", "558291", "558292",
		"571102", "584059", "611173", "611439", "611944", "639373", "690006", "691039", "711130", "719926",
		"733330", "739330", "831125", "897405", "900553", "901756", "911526", "917409", "963113", "963313",
		"980558", "980559", "992253", "992553", "992559", "1039573", "1053978", "1590166", "1848481", "2118991",
		"2119820", "2120303", "2123243", "2123343", "2128292", "2146991", "2481135", "2794460", "3091462", "3102768",
		"3131209", "3206194", "3900074", "3910674", "3910974", "3910975", "3910979", "3911124", "3912125", "3912132",
		"3917584", "3917585", "3917586", "3917587", "3917589", "3917590", "3917600", "3917601", "3917626", "3917666",
		"3918600", "3918601", "3918602", "3918603", "4081148", "4324303", "4325434", "5183338", "5837189", "6184643",
		"6184644", "6497421", "6497423", "6497428", "6963327", "7771191", "8357109", "8675611", "8959199", "9333219",
		"9333220", "9348571", "9581348", "10040609", "15389722", "16253595"
	};

	private static readonly HashSet<string> VanillaForkBranchIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"afraid", "angry", "arrogantJosh", "artShowSuggest", "backEntrance", "BadAnswer", "beatGame", "castBeam", "chargeAhead", "choseAnimals",
		"choseFarming", "choseInternet", "choseMinerals", "choseToBeKnown", "choseToBeKnown_pennySpouse", "choseToExplain", "creepySexualPass", "crying", "DadWeird", "decor",
		"decorate", "didntHear", "didntLeave", "elliottPianoJoin", "end", "enterRobin", "eventEnd", "extraHelp", "fieldTripEnd", "finalBossWarrior",
		"finalBossWizard", "giveAlexMoney", "haleyWontDoIt", "healedSam", "healer", "heavy", "honkytonk", "howLong", "internet", "internet2",
		"IslandDepart", "itsagift", "itsagift_pennySpouse", "leave", "lifestyleChoice", "linusWell", "missingBundleComplete", "mysteryBook", "Necromancer", "noFriends",
		"noPunch", "normal", "NoToElliott", "opening", "pennyHeartbroken", "PlayerKilled", "podRoom", "poppy", "positive", "prizeTicketIntro",
		"Punch", "ranAway", "rejectJosh", "rejectSam", "romanceBook", "sebastianRoom", "sewer", "stayPut", "swungWeapons", "techno",
		"toldTruth", "tooBold", "warrior", "wewereworried", "wizardDoor"
	};

	private static readonly HashSet<string> CommonForkBranchNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"choseInternet", "noPunch", "internet", "fork1", "fork2", "fork3", "option1", "option2", "option3", "yes",
		"no", "accept", "decline", "male", "female", "married", "single", "continue", "default", "else"
	};

	private static readonly HashSet<string> InvalidWhenConditionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"Action", "Target", "Entries", "Fields", "MoveEntries", "TextOperations", "Delimiter", "Operation", "Search", "Value",
		"ReplaceMode", "FromFile", "Priority", "LogName", "Enabled", "Update", "Local", "Include", "Format", "Changes",
		"ConfigSchema", "DynamicTokens"
	};

	private static readonly Dictionary<string, string> ModUniqueIdCache = new Dictionary<string, string>();

	public static void Initialize()
	{
		if (!isInitialized)
		{
			Clear();
			LoadLogNamesFromContentPatcher();
			ScanAllLocations();
			ScanContentPatcherEvents();
			MHEventsListMod.Monitor.Log($"CP scan complete: {WhenConditionsByEventId.Count} events with When conditions cached", (LogLevel)2);
			ApplyWhenConditionsToGameEvents();
			MarkDisabledCPEvents();
			isInitialized = true;
			MHEventsListMod.Monitor.Log($"Event registry initialized: {AllEvents.Count} events found", (LogLevel)2);
		}
	}

	public static void Refresh()
	{
		isInitialized = false;
		Initialize();
	}

	public static void Clear()
	{
		EventsById.Clear();
		EventsByNpc.Clear();
		AllEvents.Clear();
		LogNamesByEventId.Clear();
		WhenConditionsByEventId.Clear();
		ModPathByEventId.Clear();
		isInitialized = false;
	}

	private static void ApplyWhenConditionsToGameEvents()
	{
		int num = 0;
		foreach (EventData allEvent in AllEvents)
		{
			if (string.IsNullOrEmpty(allEvent.ModFolderPath) && ModPathByEventId.TryGetValue(allEvent.Id, out var value))
			{
				allEvent.ModFolderPath = value;
				allEvent.IsFromContentPatcher = true;
				RegisterIdPrefixMapping(allEvent.Id, value);
				if (string.IsNullOrEmpty(allEvent.ModUniqueId))
				{
					allEvent.ModUniqueId = GetModUniqueId(value);
				}
			}
			if (allEvent.HasWhenConditions || !WhenConditionsByEventId.TryGetValue(allEvent.Id, out var value2))
			{
				continue;
			}
			foreach (Dictionary<string, string> item in value2)
			{
				allEvent.AddWhenConditionsVariant(item);
			}
			allEvent.IsFromContentPatcher = true;
			num++;
		}
		if (MHEventsListMod.Config.VerboseLogging)
		{
			MHEventsListMod.Monitor.Log($"Applied cached When conditions to {num} game events", (LogLevel)1);
		}
	}

	private static void MarkDisabledCPEvents()
	{
		int num = 0;
		foreach (EventData allEvent in AllEvents)
		{
			if (allEvent.IsFromContentPatcher && !IsEventLoadedInGame(allEvent.Id, allEvent.LocationName))
			{
				allEvent.IsDisabledByCP = true;
				num++;
			}
		}
		if (num > 0)
		{
			MHEventsListMod.Monitor.Log($"Marked {num} CP events as disabled (not loaded in game due to When conditions)", (LogLevel)1);
		}
	}

	private static bool IsEventLoadedInGame(string eventId, string locationName)
	{
		try
		{
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data/Events/" + locationName);
			if (dictionary == null)
			{
				return false;
			}
			foreach (string key in dictionary.Keys)
			{
				string text = key;
				int num = key.IndexOf('/');
				if (num > 0)
				{
					text = key.Substring(0, num);
				}
				if (text.Equals(eventId, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
		catch
		{
			return true;
		}
	}

	public static EventData GetEventById(string eventId)
	{
		EventsById.TryGetValue(eventId, out var value);
		return value;
	}

	public static IReadOnlyList<EventData> GetAllEvents()
	{
		ModConfig config = MHEventsListMod.Config;
		if (config == null || config.ContentPatcherEventMode == ContentPatcherEventMode.LoadedOnly)
		{
			return AllEvents.Where((EventData e) => !e.IsDisabledByCP).ToList().AsReadOnly();
		}
		return AllEvents.AsReadOnly();
	}

	public static IReadOnlyList<EventData> GetEventsForNpc(string npcName)
	{
		npcName = npcName.ToLower();
		if (EventsByNpc.TryGetValue(npcName, out var value))
		{
			return value.AsReadOnly();
		}
		return Array.Empty<EventData>();
	}

	public static IEnumerable<string> GetNpcsWithEvents()
	{
		return EventsByNpc.Keys.OrderBy((string k) => k);
	}

	public static IEnumerable<EventData> FilterEvents(string searchText = null, string npcFilter = null, bool? showSeen = null, bool? heartEventsOnly = null)
	{
		IEnumerable<EventData> enumerable = AllEvents;
		if (!string.IsNullOrWhiteSpace(npcFilter))
		{
			npcFilter = npcFilter.ToLower();
			enumerable = enumerable.Where((EventData e) => e.HeartRequirements?.Any((CharacterHeartRequirement h) => h.NpcName.Contains(npcFilter)) ?? false);
		}
		if (!string.IsNullOrWhiteSpace(searchText))
		{
			searchText = searchText.ToLower();
			enumerable = enumerable.Where(delegate(EventData e)
			{
				if (!e.Id.ToLower().Contains(searchText) && !e.LocationName.ToLower().Contains(searchText) && !e.GetLocationDisplayName(hideUnseen: false).ToLower().Contains(searchText))
				{
					string modSource = e.ModSource;
					if (modSource == null || !modSource.ToLower().Contains(searchText))
					{
						return e.HeartRequirements?.Any((CharacterHeartRequirement h) => h.NpcName.Contains(searchText) || h.GetDisplayName().ToLower().Contains(searchText)) ?? false;
					}
				}
				return true;
			});
		}
		if (showSeen.HasValue && !showSeen.Value)
		{
			enumerable = enumerable.Where((EventData e) => !e.HasBeenSeen());
		}
		if (heartEventsOnly.HasValue && heartEventsOnly.Value)
		{
			enumerable = enumerable.Where((EventData e) => e.HeartRequirements != null && e.HeartRequirements.Count > 0);
		}
		return enumerable;
	}

	private static void ScanAllLocations()
	{
		Dictionary<string, LocationData> dictionary = DataLoader.Locations(Game1.content);
		foreach (KeyValuePair<string, LocationData> item in dictionary)
		{
			string key = item.Key;
			GameLocation locationFromName = Game1.getLocationFromName(key);
			if (locationFromName != null)
			{
				ScanLocationEvents(locationFromName, item.Value);
			}
		}
		foreach (GameLocation location in Game1.locations)
		{
			if (location != null && location.Name != null && !dictionary.ContainsKey(location.Name))
			{
				LocationData data = location.GetData();
				if (data != null)
				{
					ScanLocationEvents(location, data);
				}
			}
		}
	}

	private static void ScanLocationEvents(GameLocation location, LocationData locData)
	{
		Dictionary<string, string> dictionary = null;
		try
		{
			dictionary = Game1.content.Load<Dictionary<string, string>>("Data/Events/" + location.Name);
		}
		catch
		{
			return;
		}
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			try
			{
				if (!item.Key.Contains('/'))
				{
					continue;
				}
				string[] array = item.Key.Split('/');
				if (array.Length == 0)
				{
					continue;
				}
				string text = array[0];
				if (string.IsNullOrWhiteSpace(text) || HiddenEventIds.Contains(text) || IsForkBranch(text, item.Key))
				{
					continue;
				}
				string text2 = ((array.Length > 1) ? string.Join("/", array.Skip(1)) : "");
				if (text2.Split('/').Any((string c) => c.Trim().StartsWith("x ")) || IsEventVariant(text))
				{
					continue;
				}
				string value = item.Value;
				EventData eventData = new EventData(text, location.Name, text2, value);
				if (LogNamesByEventId.TryGetValue(text, out var value2))
				{
					eventData.DisplayName = value2;
				}
				if (string.IsNullOrWhiteSpace(value) || value.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
				{
					eventData.HasInvalidScript = true;
					if (MHEventsListMod.Config.VerboseLogging)
					{
						MHEventsListMod.Monitor.Log($"Event '{text}' in {location.Name} has invalid/null script", (LogLevel)3);
					}
				}
				eventData.ModSource = DetectModSource(location.Name, text);
				RegisterEvent(eventData);
			}
			catch (Exception ex)
			{
				if (MHEventsListMod.Config.VerboseLogging)
				{
					MHEventsListMod.Monitor.Log($"Error parsing event {item.Key} in {location.Name}: {ex.Message}", (LogLevel)3);
				}
			}
		}
	}

	private static void RegisterEvent(EventData eventData)
	{
		if (HiddenEventIds.Contains(eventData.Id))
		{
			return;
		}
		if (!EventsById.ContainsKey(eventData.Id))
		{
			EventsById[eventData.Id] = eventData;
			AllEvents.Add(eventData);
		}
		else
		{
			EventData eventData2 = EventsById[eventData.Id];
			if (!eventData.IsFromContentPatcher && eventData.GetEventScript() != null)
			{
				string eventScript = eventData2.GetEventScript();
				string eventScript2 = eventData.GetEventScript();
				if (((eventScript != null && eventScript.Contains("{{i18n:")) || (eventScript != null && eventScript.Contains("{{"))) && (eventScript2 == null || !eventScript2.Contains("{{")))
				{
					eventData2.UpdateScript(eventScript2);
				}
			}
			if (eventData.IsFromContentPatcher)
			{
				if (!eventData2.IsFromContentPatcher)
				{
					eventData2.IsFromContentPatcher = true;
				}
				if (string.IsNullOrEmpty(eventData2.ModFolderPath) && !string.IsNullOrEmpty(eventData.ModFolderPath))
				{
					eventData2.ModFolderPath = eventData.ModFolderPath;
					RegisterIdPrefixMapping(eventData2.Id, eventData.ModFolderPath);
				}
				if (string.IsNullOrEmpty(eventData2.ModUniqueId) && !string.IsNullOrEmpty(eventData.ModUniqueId))
				{
					eventData2.ModUniqueId = eventData.ModUniqueId;
				}
			}
			if (eventData.HasWhenConditions)
			{
				foreach (Dictionary<string, string> whenConditionsVariant in eventData.WhenConditionsVariants)
				{
					eventData2.AddWhenConditionsVariant(whenConditionsVariant);
				}
				if (!string.IsNullOrEmpty(eventData.LocationName) && eventData.LocationName != eventData2.LocationName)
				{
					eventData2.LocationName = eventData.LocationName;
				}
			}
		}
		if (eventData.HeartRequirements == null)
		{
			return;
		}
		foreach (CharacterHeartRequirement heartRequirement in eventData.HeartRequirements)
		{
			if (IsInvalidNpcName(heartRequirement.NpcName))
			{
				continue;
			}
			string text = SanitizeNpcName(heartRequirement.NpcName);
			if (!string.IsNullOrWhiteSpace(text))
			{
				if (!EventsByNpc.TryGetValue(text, out var value))
				{
					value = new List<EventData>();
					EventsByNpc[text] = value;
				}
				if (!value.Contains(eventData))
				{
					value.Add(eventData);
				}
			}
		}
	}

	private static bool IsInvalidNpcName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return true;
		}
		if (name.Contains("{{") || name.Contains("}}"))
		{
			return true;
		}
		if (name.StartsWith("$") || name.StartsWith("@"))
		{
			return true;
		}
		return false;
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
			if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '\'')
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString().Trim();
	}

	private static string DetectModSource(string locationName, string eventId)
	{
		HashSet<string> hashSet = new HashSet<string>
		{
			"Farm", "FarmHouse", "Town", "Beach", "Mountain", "Forest", "BusStop", "Mine", "Desert", "Woods",
			"Sewer", "Railroad", "SamHouse", "HaleyHouse", "JoshHouse", "ScienceHouse", "LeahHouse", "ElliottHouse", "ManorHouse", "SeedShop",
			"Saloon", "Hospital", "AnimalShop", "Blacksmith", "FishShop", "ArchaeologyHouse", "WizardHouse", "Trailer", "JojaMart", "Club",
			"CommunityCenter", "Backwoods", "Tunnel", "BathHouse_Entry", "BathHouse_Pool", "IslandSouth", "IslandNorth", "IslandWest", "IslandEast", "IslandFarmHouse",
			"IslandHut", "Caldera", "BoatTunnel"
		};
		if (locationName.StartsWith("Custom_") || !hashSet.Contains(locationName))
		{
			if (locationName.Contains("_"))
			{
				string[] array = locationName.Split('_');
				if (array.Length >= 2)
				{
					return array[1];
				}
			}
			return "Mod";
		}
		if (eventId.Contains(".") || eventId.Contains("_"))
		{
			string[] array2 = eventId.Split(new char[2] { '.', '_' }, 2);
			if (array2.Length >= 1 && array2[0].Length > 3)
			{
				return array2[0];
			}
		}
		return null;
	}

	private static bool IsEventVariant(string eventId)
	{
		if (string.IsNullOrEmpty(eventId))
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < eventId.Length && char.IsDigit(eventId[i]); i++)
		{
			num = i + 1;
		}
		if (num == eventId.Length || num == 0)
		{
			return false;
		}
		string text = eventId.Substring(num);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (char.IsUpper(text[0]) && text.Length >= 2)
		{
			NPC characterFromName = Game1.getCharacterFromName(text, false, false);
			if (characterFromName != null)
			{
				return true;
			}
			if (text.All((char c) => char.IsLetter(c)))
			{
				return true;
			}
		}
		return false;
	}

	private static void ScanContentPatcherEvents()
	{
		int num = 0;
		try
		{
			string path = Path.Combine(Path.GetDirectoryName(((ContentManager)Game1.content).RootDirectory), "Mods");
			if (!Directory.Exists(path))
			{
				return;
			}
			string[] directories = Directory.GetDirectories(path);
			string[] array = directories;
			foreach (string path2 in array)
			{
				string[] files = Directory.GetFiles(path2, "*.json", SearchOption.AllDirectories);
				string[] array2 = files;
				foreach (string text in array2)
				{
					try
					{
						string text2 = File.ReadAllText(text);
						if (text2.Contains("Data/Events/", StringComparison.OrdinalIgnoreCase) || text2.Contains("data/events/", StringComparison.OrdinalIgnoreCase))
						{
							num += ParseContentPatcherEventsFromJson(text2, text);
						}
					}
					catch (Exception ex)
					{
						if (MHEventsListMod.Config.VerboseLogging)
						{
							MHEventsListMod.Monitor.Log("Error scanning CP events in " + text + ": " + ex.Message, (LogLevel)0);
						}
					}
				}
			}
			if (num > 0)
			{
				MHEventsListMod.Monitor.Log($"Found {num} additional events from Content Patcher mods", (LogLevel)1);
			}
		}
		catch (Exception ex2)
		{
			MHEventsListMod.Monitor.Log("Error scanning Content Patcher events: " + ex2.Message, (LogLevel)3);
		}
	}

	private static int ParseContentPatcherEventsFromJson(string jsonContent, string filePath)
	{
		int num = 0;
		string content = ResolveModIdTokens(jsonContent, filePath);
		content = RemoveBlockComments(content);
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(content, new JsonDocumentOptions
			{
				AllowTrailingCommas = true,
				CommentHandling = JsonCommentHandling.Skip
			});
			if (jsonDocument.RootElement.TryGetProperty("Changes", out var value) && value.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement item in value.EnumerateArray())
				{
					num += ParseChangeElement(item, filePath);
				}
			}
			else
			{
				List<string> list = FindActionBlocks(content);
				foreach (string item2 in list)
				{
					num += ParseActionBlock(item2, filePath);
				}
			}
		}
		catch (JsonException)
		{
			List<string> list2 = FindActionBlocks(content);
			foreach (string item3 in list2)
			{
				num += ParseActionBlock(item3, filePath);
			}
		}
		return num;
	}

	private static int ParseChangeElement(JsonElement change, string filePath)
	{
		int num = 0;
		try
		{
			if (!change.TryGetProperty("Action", out var value))
			{
				return 0;
			}
			string text = value.GetString() ?? "";
			if (!text.Equals("EditData", StringComparison.OrdinalIgnoreCase))
			{
				return 0;
			}
			if (!change.TryGetProperty("Target", out var value2))
			{
				return 0;
			}
			string text2 = value2.GetString() ?? "";
			if (!text2.Contains("Data/Events/", StringComparison.OrdinalIgnoreCase))
			{
				return 0;
			}
			string text3 = "Unknown";
			int num2 = text2.IndexOf("Data/Events/", StringComparison.OrdinalIgnoreCase);
			if (num2 >= 0)
			{
				text3 = text2.Substring(num2 + 12);
				if (text3.Contains("/"))
				{
					text3 = text3.Substring(0, text3.IndexOf("/"));
				}
			}
			string value3 = null;
			if (change.TryGetProperty("LogName", out var value4) || change.TryGetProperty("Logname", out value4))
			{
				value3 = value4.GetString();
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (change.TryGetProperty("When", out var value5) && value5.ValueKind == JsonValueKind.Object)
			{
				foreach (JsonProperty item in value5.EnumerateObject())
				{
					string name = item.Name;
					if (IsValidWhenConditionKey(name))
					{
						dictionary[name] = item.Value.ValueKind switch
						{
							JsonValueKind.True => "true", 
							JsonValueKind.False => "false", 
							JsonValueKind.String => item.Value.GetString() ?? "", 
							_ => item.Value.GetRawText(), 
						};
					}
				}
			}
			if (change.TryGetProperty("Entries", out var value6) && value6.ValueKind == JsonValueKind.Object)
			{
				foreach (JsonProperty item2 in value6.EnumerateObject())
				{
					string name2 = item2.Name;
					string eventScript = ((item2.Value.ValueKind == JsonValueKind.String) ? (item2.Value.GetString() ?? "") : "");
					string text4 = name2;
					string preconditions = "";
					int num3 = name2.IndexOf('/');
					if (num3 > 0)
					{
						text4 = name2.Substring(0, num3);
						preconditions = name2.Substring(num3 + 1);
					}
					if (string.IsNullOrWhiteSpace(text4) || text4.Contains("{{") || HiddenEventIds.Contains(text4) || IsForkBranch(text4, name2))
					{
						continue;
					}
					EventData eventData = new EventData(text4, text3, preconditions, eventScript);
					eventData.IsFromContentPatcher = true;
					eventData.ModFolderPath = GetModFolderFromFilePath(filePath);
					eventData.ModUniqueId = GetModUniqueId(eventData.ModFolderPath);
					RegisterIdPrefixMapping(text4, eventData.ModFolderPath);
					if (!string.IsNullOrEmpty(value3) && !LogNamesByEventId.ContainsKey(text4))
					{
						LogNamesByEventId[text4] = value3;
					}
					if (dictionary.Count > 0)
					{
						eventData.AddWhenConditionsVariant(dictionary);
						if (!WhenConditionsByEventId.ContainsKey(text4))
						{
							WhenConditionsByEventId[text4] = new List<Dictionary<string, string>>();
						}
						WhenConditionsByEventId[text4].Add(new Dictionary<string, string>(dictionary));
					}
					if (!string.IsNullOrEmpty(eventData.ModFolderPath))
					{
						ModPathByEventId[text4] = eventData.ModFolderPath;
					}
					RegisterEvent(eventData);
					num++;
				}
			}
		}
		catch (Exception ex)
		{
			if (MHEventsListMod.Config.VerboseLogging)
			{
				MHEventsListMod.Monitor.Log("Error parsing Change element: " + ex.Message, (LogLevel)0);
			}
		}
		return num;
	}

	private static string ResolveModIdTokens(string jsonContent, string filePath)
	{
		if (!jsonContent.Contains("{{ModId}}"))
		{
			return jsonContent;
		}
		try
		{
			string directoryName = Path.GetDirectoryName(filePath);
			string text = null;
			while (!string.IsNullOrEmpty(directoryName))
			{
				string text2 = Path.Combine(directoryName, "manifest.json");
				if (File.Exists(text2))
				{
					text = text2;
					break;
				}
				if (Path.GetFileName(directoryName).Equals("Mods", StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
				directoryName = Path.GetDirectoryName(directoryName);
			}
			if (text == null)
			{
				return jsonContent;
			}
			string text3 = File.ReadAllText(text);
			string text4 = null;
			string[] array = text3.Split('\n');
			string[] array2 = array;
			foreach (string text5 in array2)
			{
				string text6 = text5.Trim();
				if (!text6.Contains("\"UniqueID\"") || !text6.Contains(":"))
				{
					continue;
				}
				int num = text6.IndexOf(':');
				if (num > 0)
				{
					string text7 = text6.Substring(num + 1).Trim().Trim('"', ',', ' ');
					if (!string.IsNullOrEmpty(text7))
					{
						text4 = text7;
						break;
					}
				}
			}
			if (!string.IsNullOrEmpty(text4))
			{
				return jsonContent.Replace("{{ModId}}", text4);
			}
		}
		catch (Exception ex)
		{
			if (MHEventsListMod.Config.VerboseLogging)
			{
				MHEventsListMod.Monitor.Log("Error resolving ModId tokens: " + ex.Message, (LogLevel)0);
			}
		}
		return jsonContent;
	}

	private static string RemoveBlockComments(string content)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		for (int i = 0; i < content.Length; i++)
		{
			if (!flag && i < content.Length - 1 && content[i] == '/' && content[i + 1] == '*')
			{
				flag = true;
				i++;
			}
			else if (flag && i < content.Length - 1 && content[i] == '*' && content[i + 1] == '/')
			{
				flag = false;
				i++;
			}
			else if (!flag)
			{
				stringBuilder.Append(content[i]);
			}
		}
		return stringBuilder.ToString();
	}

	private static List<string> FindActionBlocks(string jsonContent)
	{
		jsonContent = RemoveBlockComments(jsonContent);
		List<string> list = new List<string>();
		string[] array = jsonContent.Split('\n');
		int num = -1;
		int num2 = 0;
		List<string> list2 = new List<string>();
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			string text2 = text.Trim();
			if (text2.StartsWith("//"))
			{
				continue;
			}
			int num3 = text2.Count((char c) => c == '{');
			int num4 = text2.Count((char c) => c == '}');
			if (num < 0 && num3 > 0)
			{
				num = i;
				num2 = num3 - num4;
				list2.Clear();
				list2.Add(text);
				flag = false;
				flag2 = false;
				if (text2.Contains("\"Action\""))
				{
					flag = true;
				}
				if (text2.Contains("Data/Events/", StringComparison.OrdinalIgnoreCase))
				{
					flag2 = true;
				}
			}
			else
			{
				if (num < 0)
				{
					continue;
				}
				list2.Add(text);
				num2 += num3 - num4;
				if (text2.Contains("\"Action\""))
				{
					flag = true;
				}
				if (text2.Contains("Data/Events/", StringComparison.OrdinalIgnoreCase))
				{
					flag2 = true;
				}
				if (num2 <= 0)
				{
					if (flag && flag2)
					{
						string item = string.Join("\n", list2);
						list.Add(item);
					}
					num = -1;
					flag = false;
					flag2 = false;
					list2.Clear();
				}
			}
		}
		return list;
	}

	private static int ParseActionBlock(string blockContent, string filePath)
	{
		int num = 0;
		string[] array = blockContent.Split('\n');
		string text = null;
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (!text2.Contains("\"Target\"") || !text2.Contains("Data/Events/", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			int num2 = text2.IndexOf("Data/Events/", StringComparison.OrdinalIgnoreCase);
			if (num2 >= 0)
			{
				int num3 = text2.IndexOf('"', num2);
				if (num3 > num2)
				{
					string text3 = text2.Substring(num2, num3 - num2);
					text = text3.Substring(12);
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(text) || (text.Contains("{") && text.Contains("}")))
		{
			return 0;
		}
		string text4 = null;
		string[] array3 = array;
		foreach (string text5 in array3)
		{
			if (text5.Contains("\"Logname\"") || text5.Contains("\"LogName\""))
			{
				int num4 = text5.IndexOf(':');
				if (num4 > 0)
				{
					string text6 = text5.Substring(num4 + 1).Trim().TrimEnd(',');
					text4 = text6.Trim('"', ' ');
					break;
				}
			}
		}
		Dictionary<string, string> dictionary = ExtractWhenConditions(blockContent);
		bool flag = false;
		int num5 = 0;
		string[] array4 = array;
		foreach (string text7 in array4)
		{
			string text8 = text7.Trim();
			if (text8.Contains("\"Entries\"") && text8.Contains(":"))
			{
				flag = true;
				num5 = 0;
				if (text8.Contains("{"))
				{
					num5++;
				}
			}
			else
			{
				if (!flag)
				{
					continue;
				}
				string text9 = text8;
				for (int l = 0; l < text9.Length; l++)
				{
					switch (text9[l])
					{
					case '{':
						num5++;
						break;
					case '}':
						num5--;
						break;
					}
				}
				if (num5 <= 0)
				{
					flag = false;
				}
				else
				{
					if (!text8.StartsWith("\"") || !text8.Contains(":"))
					{
						continue;
					}
					int num6 = text8.IndexOf(':');
					if (num6 <= 0)
					{
						continue;
					}
					string text10 = text8.Substring(0, num6).Trim().Trim('"');
					if (!text10.Contains("/"))
					{
						continue;
					}
					string[] array5 = text10.Split('/');
					string text11 = array5[0];
					if (string.IsNullOrWhiteSpace(text11))
					{
						continue;
					}
					if (EventsById.ContainsKey(text11))
					{
						EventData eventData = EventsById[text11];
						if (dictionary.Count > 0)
						{
							eventData.AddWhenConditionsVariant(dictionary);
							eventData.IsFromContentPatcher = true;
							if (!WhenConditionsByEventId.ContainsKey(text11))
							{
								WhenConditionsByEventId[text11] = new List<Dictionary<string, string>>();
							}
							WhenConditionsByEventId[text11].Add(new Dictionary<string, string>(dictionary));
						}
						if (!string.IsNullOrEmpty(text4) && string.IsNullOrEmpty(eventData.DisplayName))
						{
							eventData.DisplayName = text4;
						}
						if (!eventData.HasInvalidScript)
						{
							continue;
						}
						int num7 = text8.IndexOf(':');
						if (num7 <= 0)
						{
							continue;
						}
						string text12 = text8.Substring(num7 + 1).Trim();
						if (!text12.StartsWith("\""))
						{
							continue;
						}
						int num8 = text12.LastIndexOf('"');
						if (num8 > 0)
						{
							string text13 = text12.Substring(1, num8 - 1);
							if (!string.IsNullOrEmpty(text13))
							{
								eventData.UpdateScript(text13);
							}
						}
					}
					else
					{
						if ((text11.Contains("{") && text11.Contains("}")) || IsEventVariant(text11) || HiddenEventIds.Contains(text11))
						{
							continue;
						}
						string text14 = ((array5.Length > 1) ? string.Join("/", array5.Skip(1)) : "");
						string eventKey = text8.Split(':')[0].Trim().Trim('"');
						if (IsForkBranch(text11, eventKey) || text14.Split('/').Any((string c) => c.Trim().StartsWith("x ")))
						{
							continue;
						}
						string text15 = text8.Substring(num6 + 1).Trim();
						string text16 = "";
						if (text15.StartsWith("\""))
						{
							int num9 = 0;
							int num10 = text15.LastIndexOf('"');
							if (num10 > num9)
							{
								text16 = text15.Substring(1, num10 - 1);
							}
						}
						EventData eventData2 = new EventData(text11, text, text14, text16);
						eventData2.ModSource = DetectModSourceFromPath(filePath);
						eventData2.IsFromContentPatcher = true;
						eventData2.ModFolderPath = GetModFolderFromFilePath(filePath);
						eventData2.ModUniqueId = GetModUniqueId(eventData2.ModFolderPath);
						RegisterIdPrefixMapping(eventData2.Id, eventData2.ModFolderPath);
						if (dictionary.Count > 0)
						{
							eventData2.AddWhenConditionsVariant(dictionary);
							if (!WhenConditionsByEventId.ContainsKey(text11))
							{
								WhenConditionsByEventId[text11] = new List<Dictionary<string, string>>();
							}
							WhenConditionsByEventId[text11].Add(new Dictionary<string, string>(dictionary));
							ModPathByEventId[text11] = eventData2.ModFolderPath;
						}
						string value;
						if (!string.IsNullOrEmpty(text4))
						{
							eventData2.DisplayName = text4;
						}
						else if (LogNamesByEventId.TryGetValue(text11, out value))
						{
							eventData2.DisplayName = value;
						}
						if (string.IsNullOrWhiteSpace(text16) || text16.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
						{
							eventData2.HasInvalidScript = true;
						}
						RegisterEvent(eventData2);
						num++;
						if (!MHEventsListMod.Config.VerboseLogging)
						{
							continue;
						}
						string value2 = "";
						if (eventData2.HasWhenConditions)
						{
							List<string> list = new List<string>();
							foreach (Dictionary<string, string> whenConditionsVariant in eventData2.WhenConditionsVariants)
							{
								foreach (KeyValuePair<string, string> item in whenConditionsVariant)
								{
									list.Add(item.Key + "=" + item.Value);
								}
							}
							value2 = " (When: " + string.Join(", ", list) + ")";
						}
						MHEventsListMod.Monitor.Log($"[CP Event] ID={text11} | Location={text} | File={Path.GetFileName(filePath)}{value2}", (LogLevel)1);
					}
				}
			}
		}
		return num;
	}

	private static string DetectModSourceFromPath(string filePath)
	{
		try
		{
			string[] array = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			for (int i = 0; i < array.Length - 1; i++)
			{
				if (array[i].Equals("Mods", StringComparison.OrdinalIgnoreCase))
				{
					return array[i + 1];
				}
			}
		}
		catch
		{
		}
		return "Mod";
	}

	private static bool IsValidWhenConditionKey(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		if (InvalidWhenConditionKeys.Contains(key))
		{
			return false;
		}
		if (key.StartsWith("[") || key.StartsWith("$"))
		{
			return false;
		}
		return true;
	}

	private static bool IsForkBranch(string eventId, string eventKey)
	{
		if (string.IsNullOrEmpty(eventId))
		{
			return true;
		}
		if (VanillaForkBranchIds.Contains(eventId))
		{
			return true;
		}
		if (CommonForkBranchNames.Contains(eventId))
		{
			return true;
		}
		if (eventKey == eventId || !eventKey.Contains("/"))
		{
			if (VanillaEventIds.Contains(eventId))
			{
				return false;
			}
			if (long.TryParse(eventId, out var _))
			{
				return false;
			}
			if (eventId.Contains("."))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private static Dictionary<string, string> ExtractWhenConditions(string blockContent)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		try
		{
			string text = blockContent.Trim();
			if (!text.StartsWith("{"))
			{
				text = "{" + text;
			}
			if (!text.EndsWith("}"))
			{
				text = text.TrimEnd(',', ' ', '\n', '\r') + "}";
			}
			using JsonDocument jsonDocument = JsonDocument.Parse(text, new JsonDocumentOptions
			{
				AllowTrailingCommas = true,
				CommentHandling = JsonCommentHandling.Skip
			});
			if (jsonDocument.RootElement.TryGetProperty("When", out var value) && value.ValueKind == JsonValueKind.Object)
			{
				foreach (JsonProperty item in value.EnumerateObject())
				{
					string name = item.Name;
					if (IsValidWhenConditionKey(name))
					{
						dictionary[name] = item.Value.ValueKind switch
						{
							JsonValueKind.True => "true", 
							JsonValueKind.False => "false", 
							JsonValueKind.String => item.Value.GetString() ?? "", 
							_ => item.Value.GetRawText(), 
						};
					}
				}
			}
		}
		catch (JsonException)
		{
			dictionary = ExtractWhenConditionsFallback(blockContent);
		}
		catch (Exception ex2)
		{
			if (MHEventsListMod.Config.VerboseLogging)
			{
				MHEventsListMod.Monitor.Log("Error extracting When conditions: " + ex2.Message, (LogLevel)0);
			}
			dictionary = ExtractWhenConditionsFallback(blockContent);
		}
		List<string> list = dictionary.Keys.Where((string k) => !IsValidWhenConditionKey(k)).ToList();
		foreach (string item2 in list)
		{
			dictionary.Remove(item2);
		}
		return dictionary;
	}

	private static Dictionary<string, string> ExtractWhenConditionsFallback(string blockContent)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] array = blockContent.Split('\n');
		bool flag = false;
		int num = 0;
		string[] array2 = array;
		foreach (string text in array2)
		{
			string text2 = text.Trim();
			if (text2.Contains("\"When\"") && !text2.Contains("\"Entries\""))
			{
				flag = true;
				num = 0;
				if (text2.Contains("{"))
				{
					num++;
				}
			}
			else
			{
				if (!flag)
				{
					continue;
				}
				string text3 = text2;
				for (int j = 0; j < text3.Length; j++)
				{
					switch (text3[j])
					{
					case '{':
						num++;
						break;
					case '}':
						num--;
						break;
					}
				}
				if (text2.StartsWith("\""))
				{
					int num2 = 1;
					int num3 = -1;
					bool flag2 = false;
					for (int k = 1; k < text2.Length; k++)
					{
						if (flag2)
						{
							flag2 = false;
						}
						else if (text2[k] == '\\')
						{
							flag2 = true;
						}
						else if (text2[k] == '"')
						{
							num3 = k;
							break;
						}
					}
					if (num3 > num2)
					{
						string text4 = text2.Substring(num2, num3 - num2);
						int num4 = text2.IndexOf(':', num3);
						if (num4 > num3)
						{
							string text5 = text2.Substring(num4 + 1).Trim().TrimEnd(',');
							string value = text5.Trim('"', ' ');
							if (!string.IsNullOrEmpty(text4))
							{
								dictionary[text4] = value;
							}
						}
					}
				}
				if (num <= 0)
				{
					flag = false;
				}
			}
		}
		return dictionary;
	}

	private static string GetModFolderFromFilePath(string filePath)
	{
		try
		{
			string text = Path.GetDirectoryName(filePath);
			string text2 = null;
			while (!string.IsNullOrEmpty(text))
			{
				string path = Path.Combine(text, "manifest.json");
				if (File.Exists(path))
				{
					text2 = text;
				}
				if (Path.GetFileName(text).Equals("Mods", StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
				string directoryName = Path.GetDirectoryName(text);
				if (string.IsNullOrEmpty(directoryName) || directoryName == text)
				{
					break;
				}
				text = directoryName;
			}
			if (!string.IsNullOrEmpty(text2))
			{
				return text2;
			}
			string[] array = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			for (int i = 0; i < array.Length - 1; i++)
			{
				if (array[i].Equals("Mods", StringComparison.OrdinalIgnoreCase))
				{
					return string.Join(Path.DirectorySeparatorChar.ToString(), array, 0, i + 2);
				}
			}
		}
		catch
		{
		}
		return null;
	}

	internal static void RegisterIdPrefixMapping(string eventId, string modFolderPath)
	{
		if (string.IsNullOrEmpty(eventId) || string.IsNullOrEmpty(modFolderPath))
		{
			return;
		}
		string fileName = Path.GetFileName(modFolderPath.TrimEnd('\\', '/'));
		if (string.IsNullOrEmpty(fileName))
		{
			return;
		}
		List<string> list = new List<string>();
		if (eventId.Contains("."))
		{
			int num = eventId.IndexOf('.');
			if (num > 0)
			{
				list.Add(eventId.Substring(0, num));
			}
		}
		if (eventId.Length >= 2 && char.IsUpper(eventId[0]))
		{
			int i;
			for (i = 1; i < eventId.Length && char.IsUpper(eventId[i]); i++)
			{
			}
			if (i >= 2 && i <= 4 && i < eventId.Length && char.IsLower(eventId[i]))
			{
				list.Add(eventId.Substring(0, i));
			}
		}
		if (eventId.Contains("_"))
		{
			int num2 = eventId.IndexOf('_');
			if (num2 > 1)
			{
				string text = eventId.Substring(0, num2);
				if (char.IsLetter(text[0]))
				{
					list.Add(text);
				}
			}
		}
		foreach (string item in list)
		{
			if (!string.IsNullOrEmpty(item) && !IdPrefixToModName.ContainsKey(item))
			{
				IdPrefixToModName[item] = fileName;
			}
		}
	}

	private static string GetModUniqueId(string modFolderPath)
	{
		if (string.IsNullOrEmpty(modFolderPath))
		{
			return null;
		}
		if (ModUniqueIdCache.TryGetValue(modFolderPath, out var value))
		{
			return value;
		}
		try
		{
			string path = Path.Combine(modFolderPath, "manifest.json");
			if (File.Exists(path))
			{
				string input = File.ReadAllText(path);
				Match match = Regex.Match(input, "\"UniqueID\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
				if (match.Success)
				{
					string value2 = match.Groups[1].Value;
					ModUniqueIdCache[modFolderPath] = value2;
					return value2;
				}
			}
		}
		catch
		{
		}
		ModUniqueIdCache[modFolderPath] = null;
		return null;
	}

	private static void LoadLogNamesFromContentPatcher()
	{
		try
		{
			string path = Path.Combine(Path.GetDirectoryName(((ContentManager)Game1.content).RootDirectory), "Mods");
			if (!Directory.Exists(path))
			{
				return;
			}
			string[] directories = Directory.GetDirectories(path);
			string[] array = directories;
			foreach (string path2 in array)
			{
				string[] files = Directory.GetFiles(path2, "*.json", SearchOption.AllDirectories);
				string[] array2 = files;
				foreach (string text in array2)
				{
					try
					{
						string text2 = File.ReadAllText(text);
						string[] array3 = text2.Split('\n');
						string text3 = null;
						for (int k = 0; k < array3.Length; k++)
						{
							string text4 = array3[k].Trim();
							if (text4.Contains("\"LogName\"") || text4.Contains("'LogName'"))
							{
								int num = text4.IndexOf(':');
								if (num >= 0)
								{
									string text5 = text4.Substring(num + 1).Trim();
									text5 = text5.Trim('"', '\'', ',', ' ');
									text3 = text5;
								}
							}
							else
							{
								if (text3 == null || !text4.Contains("\"Entries\""))
								{
									continue;
								}
								for (int l = k + 1; l < array3.Length && l < k + 100; l++)
								{
									string text6 = array3[l].Trim();
									if (text6.StartsWith("}") && text6.Contains(","))
									{
										text3 = null;
										break;
									}
									if (!text6.StartsWith("\"") || !text6.Contains("/"))
									{
										continue;
									}
									int num2 = text6.IndexOf('"');
									int num3 = text6.IndexOf('"', num2 + 1);
									if (num2 < 0 || num3 <= num2)
									{
										continue;
									}
									string text7 = text6.Substring(num2 + 1, num3 - num2 - 1);
									int num4 = text7.IndexOf('/');
									if (num4 <= 0)
									{
										continue;
									}
									string text8 = text7.Substring(0, num4);
									if (!LogNamesByEventId.ContainsKey(text8))
									{
										LogNamesByEventId[text8] = text3;
										if (MHEventsListMod.Config.VerboseLogging)
										{
											MHEventsListMod.Monitor.Log("Found LogName '" + text3 + "' for event " + text8, (LogLevel)0);
										}
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						if (MHEventsListMod.Config.VerboseLogging)
						{
							MHEventsListMod.Monitor.Log("Error parsing " + text + ": " + ex.Message, (LogLevel)0);
						}
					}
				}
			}
			if (MHEventsListMod.Config.VerboseLogging)
			{
				MHEventsListMod.Monitor.Log($"Loaded {LogNamesByEventId.Count} LogName mappings from Content Patcher mods", (LogLevel)1);
			}
		}
		catch (Exception ex2)
		{
			MHEventsListMod.Monitor.Log("Error loading LogNames: " + ex2.Message, (LogLevel)3);
		}
	}
}
