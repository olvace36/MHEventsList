using System;
using System.Linq;
using System.Text;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Network;

namespace MHEventsList.Core;

public static class ConditionTranslator
{
	public static string Translate(string condition)
	{
		if (string.IsNullOrWhiteSpace(condition))
		{
			return "";
		}
		condition = condition.Trim();
		if (condition.Length == 0)
		{
			return "";
		}
		try
		{
			bool flag = condition.StartsWith("!");
			if (flag)
			{
				condition = condition.Substring(1);
			}
			string[] array = condition.Split(' ');
			string text = array[0];
			if (text.Equals("sendmail", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					string mail = array[1];
					if (array.Length > 2)
					{
						string info = string.Join(" ", array.Skip(2));
						return GetTranslation("cond.sendmail", new { mail, info });
					}
					return GetTranslation("cond.sendmailSimple", new { mail });
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("Friendship", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 3)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 1; i + 1 < array.Length; i += 2)
					{
						string npcDisplayName = GetNpcDisplayName(array[i]);
						if (int.TryParse(array[i + 1], out var result))
						{
							double hearts = (double)result / 250.0;
							if (stringBuilder.Length > 0)
							{
								stringBuilder.Append(", ");
							}
							stringBuilder.Append(GetTranslation("cond.friendship", new
							{
								name = npcDisplayName,
								hearts = hearts
							}));
						}
					}
					return (stringBuilder.Length > 0) ? stringBuilder.ToString() : GetTranslation("cond.unknown");
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("SawEvent", StringComparison.OrdinalIgnoreCase) || text.Equals("SeenEvent", StringComparison.OrdinalIgnoreCase))
			{
				if (flag)
				{
					if (array.Length >= 2)
					{
						return GetTranslation("cond.notSeenEvent") + ": " + array[1];
					}
					return GetTranslation("cond.notSeenEvent");
				}
				if (array.Length >= 2)
				{
					return GetTranslation("cond.seenEvent") + ": " + array[1];
				}
				return GetTranslation("cond.seenEvent");
			}
			if (text.Equals("NotSawEvent", StringComparison.OrdinalIgnoreCase) || text.Equals("NotSeenEvent", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.notSeenEvent") + ": " + array[1];
				}
				return GetTranslation("cond.notSeenEvent");
			}
			if (text.Equals("Hearts", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 3)
				{
					string npcDisplayName2 = GetNpcDisplayName(array[1]);
					if (int.TryParse(array[2], out var result2))
					{
						return GetTranslation("cond.friendship", new
						{
							name = npcDisplayName2,
							hearts = (double)result2
						});
					}
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("LocalMail", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					if (flag)
					{
						return GetTranslation("cond.noMail");
					}
					return GetTranslation("cond.localMail", new
					{
						mail = array[1]
					});
				}
				return GetTranslation("cond.localMail", new
				{
					mail = "?"
				});
			}
			if (text.Equals("Season", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					string seasons = string.Join(", ", from s in array.Skip(1)
						select GetTranslation("season." + s.ToLower()));
					if (flag)
					{
						return GetTranslation("cond.notSeason", new { seasons });
					}
					return GetTranslation("cond.inSeason", new { seasons });
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("Time", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 3)
				{
					int.TryParse(array[1], out var result3);
					int.TryParse(array[2], out var result4);
					return GetTranslation("cond.time", new
					{
						start = FormatTime(result3),
						end = FormatTime(result4)
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("Weather", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					string translation = GetTranslation("weather." + array[1].ToLower());
					if (flag)
					{
						return GetTranslation("cond.notWeather", new
						{
							weather = translation
						});
					}
					return GetTranslation("cond.weather", new
					{
						weather = translation
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("Random", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2 && float.TryParse(array[1], out var result5))
				{
					return GetTranslation("cond.random", new
					{
						percent = result5 * 100f
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("Gender", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					string text2 = array[1].ToLower();
					return GetTranslation("cond.gender", new
					{
						gender = GetTranslation("gender." + text2)
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("DayOfWeek", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					for (int num = 1; num < array.Length; num++)
					{
						if (!string.IsNullOrEmpty(array[num]))
						{
							if (stringBuilder2.Length > 0)
							{
								stringBuilder2.Append(", ");
							}
							stringBuilder2.Append(GetTranslation("day." + array[num].ToLower()));
						}
					}
					string translation2 = GetTranslation("cond.daysOfWeek", new
					{
						days = stringBuilder2.ToString()
					});
					if (flag)
					{
						translation2 = GetTranslation("cond.notDayOfWeek", new
						{
							days = stringBuilder2.ToString()
						});
					}
					return translation2;
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("DayOfMonth", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					string text3 = string.Join(", ", array.Skip(1));
					return GetTranslation("cond.dayOfMonth") + ": " + text3;
				}
				return GetTranslation("cond.dayOfMonth");
			}
			if (text.Equals("MinutesPlayed", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.minutesPlayed", new
					{
						minutes = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("DaysPlayed", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.daysPlayed", new
					{
						days = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("SkillLevel", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 3)
				{
					return GetTranslation("cond.skillLevel", new
					{
						skill = array[1],
						level = array[2]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("HasRecipe", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					if (flag)
					{
						return GetTranslation("cond.noRecipe", new
						{
							recipe = array[1]
						});
					}
					return GetTranslation("cond.hasRecipe", new
					{
						recipe = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("HasProfession", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					if (flag)
					{
						return GetTranslation("cond.noProfession", new
						{
							profession = array[1]
						});
					}
					return GetTranslation("cond.hasProfession", new
					{
						profession = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("IsFestival", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					if (flag)
					{
						return GetTranslation("cond.notFestival", new
						{
							festival = array[1]
						});
					}
					return GetTranslation("cond.isFestival", new
					{
						festival = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("IsGreenRain", StringComparison.OrdinalIgnoreCase))
			{
				if (flag)
				{
					return GetTranslation("cond.notGreenRain");
				}
				return GetTranslation("cond.isGreenRain");
			}
			if (text.Equals("HasDialogueAnswer", StringComparison.OrdinalIgnoreCase) || text.Equals("ChoseDialogueAnswers", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 3)
				{
					return GetTranslation("cond.hasDialogueAnswer", new
					{
						question = array[1],
						answer = array[2]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("FestivalDay", StringComparison.OrdinalIgnoreCase))
			{
				if (flag)
				{
					return GetTranslation("cond.notFestivalDay");
				}
				return GetTranslation("cond.festivalDay");
			}
			if (text.Equals("NotFestivalDay", StringComparison.OrdinalIgnoreCase))
			{
				return GetTranslation("cond.notFestivalDay");
			}
			if (text.Equals("UpcomingFestival", StringComparison.OrdinalIgnoreCase))
			{
				if (flag)
				{
					return GetTranslation("cond.notUpcomingFestival");
				}
				if (array.Length >= 2)
				{
					return GetTranslation("cond.upcomingFestival", new
					{
						days = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("NotUpcomingFestival", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.notUpcomingFestival");
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("Skill", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 3)
				{
					return GetTranslation("cond.skillLevel", new
					{
						skill = array[1],
						level = array[2]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("Spouse", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					string npcDisplayName3 = GetNpcDisplayName(array[1]);
					if (flag)
					{
						return GetTranslation("cond.notMarried", new
						{
							name = npcDisplayName3
						});
					}
					return GetTranslation("cond.married", new
					{
						name = npcDisplayName3
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("NotSpouse", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					string npcDisplayName4 = GetNpcDisplayName(array[1]);
					return GetTranslation("cond.notMarried", new
					{
						name = npcDisplayName4
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("Roommate", StringComparison.OrdinalIgnoreCase))
			{
				if (flag)
				{
					return GetTranslation("cond.notRoommate");
				}
				return GetTranslation("cond.roommate");
			}
			if (text.Equals("Tile", StringComparison.OrdinalIgnoreCase))
			{
				return TranslateTile(array);
			}
			if (text.Equals("NotRoommate", StringComparison.OrdinalIgnoreCase))
			{
				return GetTranslation("cond.notRoommate");
			}
			if (text.Equals("SpouseBed", StringComparison.OrdinalIgnoreCase))
			{
				return GetTranslation("cond.spouseBed");
			}
			if (text.Equals("IsHost", StringComparison.OrdinalIgnoreCase))
			{
				return GetTranslation("cond.isHost");
			}
			if (text.Equals("HostMail", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.hostMail", new
					{
						mail = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("NotHostMail", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.notHostMail", new
					{
						mail = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("HostOrLocalMail", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.hostOrLocalMail", new
					{
						mail = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("NotHostOrLocalMail", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.notHostOrLocalMail", new
					{
						mail = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("NotLocalMail", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.noMail");
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("NotActiveDialogueEvent", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.notActiveDialogue", new
					{
						@event = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("ActiveDialogueEvent", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return TranslateActiveDialogueEvent(array);
				}
				return GetTranslation("cond.unknown");
			}
			if (text.Equals("InUpgradedHouse", StringComparison.OrdinalIgnoreCase))
			{
				int level = 2;
				if (array.Length >= 2 && int.TryParse(array[1], out var result6))
				{
					level = result6;
				}
				return GetTranslation("cond.inUpgradedHouse", new { level });
			}
			if (text.Equals("CommunityCenterOrWarehouseDone", StringComparison.OrdinalIgnoreCase))
			{
				return GetTranslation("cond.communityCenterDone");
			}
			if (text.Equals("NotCommunityCenterOrWarehouseDone", StringComparison.OrdinalIgnoreCase))
			{
				return GetTranslation("cond.notCommunityCenterDone");
			}
			if (text.Equals("JojaBundlesDone", StringComparison.OrdinalIgnoreCase))
			{
				return GetTranslation("cond.jojaComplete");
			}
			if (text.Equals("WorldState", StringComparison.OrdinalIgnoreCase))
			{
				if (array.Length >= 2)
				{
					return GetTranslation("cond.worldState", new
					{
						id = array[1]
					});
				}
				return GetTranslation("cond.unknown");
			}
			char c = condition[0];
			if (c == 'H' && condition.Length > 1 && char.IsLetter(condition[1]))
			{
				string translation3 = GetTranslation("cond.hostOnly");
				string text4 = Translate(condition.Substring(1));
				return translation3 + " + " + text4;
			}
			return c switch
			{
				'a' => TranslateTile(array), 
				'A' => TranslateNotActiveDialogueEvent(array), 
				'b' => TranslateReachedMineBottom(array), 
				'B' => GetTranslation("cond.spouseBed"), 
				'c' => TranslateFreeSlots(array), 
				'C' => GetTranslation("cond.communityCenterDone"), 
				'd' => TranslateNotDayOfWeek(array), 
				'D' => flag ? TranslateNotDating(array) : TranslateDating(array), 
				'e' => TranslateSeenEvent(array), 
				'f' => TranslateFriendship(array), 
				'F' => GetTranslation("cond.notFestivalDay"), 
				'g' => TranslateGender(array), 
				'G' => TranslateGameStateQuery(array), 
				'h' => TranslateMissingPet(array), 
				'H' => GetTranslation("cond.isHost"), 
				'i' => TranslateItem(array), 
				'j' => TranslateDaysPlayed(array), 
				'J' => GetTranslation("cond.jojaComplete"), 
				'k' => TranslateNotSeenEvent(array), 
				'l' => TranslateNotLocalMail(array), 
				'L' => TranslateInUpgradedHouse(array), 
				'm' => TranslateEarnedMoney(array), 
				'M' => TranslateMoney(array), 
				'n' => TranslateLocalMail(array), 
				'N' => TranslateGoldenWalnuts(array), 
				'o' => TranslateNotMarried(array), 
				'O' => TranslateMarried(array), 
				'p' => TranslateNpcPresent(array), 
				'q' => TranslateChoseDialogueAnswers(array), 
				'r' => TranslateRandom(array), 
				'R' => TranslateRoommate(array), 
				'S' => TranslateSecretNote(array), 
				's' => TranslateShippedItem(array), 
				't' => TranslateTime(array), 
				'u' => TranslateDayOfMonth(array), 
				'U' => TranslateNotUpcomingFestival(array), 
				'v' => TranslateNpcVisible(array), 
				'w' => flag ? TranslateNotWeather(array) : TranslateWeather(array), 
				'x' => GetTranslation("cond.markFlag"), 
				'X' => GetTranslation("cond.notCommunityCenterDone"), 
				'y' => TranslateYear(array), 
				'z' => TranslateNotSeason(array), 
				'*' => TranslateWorldStateOrMail(array, condition), 
				_ => GetTranslation("cond.unknown"), 
			};
		}
		catch
		{
			return GetTranslation("cond.error");
		}
	}

	private static string GetTranslation(string key, object tokens = null)
	{
		return Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get(key, tokens));
	}

	private static string TranslateFreeSlots(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		int.TryParse(args[1], out var result);
		return GetTranslation("cond.freeSlots", new
		{
			count = result
		});
	}

	private static string TranslateDating(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string npcDisplayName = GetNpcDisplayName(args[1]);
		return GetTranslation("cond.dating", new
		{
			name = npcDisplayName
		});
	}

	private static string TranslateNotDating(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string npcDisplayName = GetNpcDisplayName(args[1]);
		return GetTranslation("cond.notDating", new
		{
			name = npcDisplayName
		});
	}

	private static string TranslateFriendship(string[] args)
	{
		if (args.Length < 3)
		{
			return GetTranslation("cond.unknown");
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i + 1 < args.Length; i += 2)
		{
			string npcDisplayName = GetNpcDisplayName(args[i]);
			if (int.TryParse(args[i + 1], out var result))
			{
				double hearts = (double)result / 250.0;
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(GetTranslation("cond.friendship", new
				{
					name = npcDisplayName,
					hearts = hearts
				}));
			}
		}
		if (stringBuilder.Length <= 0)
		{
			return GetTranslation("cond.unknown");
		}
		return stringBuilder.ToString();
	}

	private static string TranslateItem(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string text = args[1];
		string itemDisplayName = GetItemDisplayName(text);
		return GetTranslation("cond.hasItem", new
		{
			item = itemDisplayName,
			id = text
		});
	}

	private static string TranslateMoney(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		int.TryParse(args[1], out var result);
		return GetTranslation("cond.money", new
		{
			amount = result
		});
	}

	private static string TranslateNoFlag(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.noFlag");
		}
		return GetTranslation("cond.noFlagSpecific", new
		{
			flag = args[1]
		});
	}

	private static string TranslateNotMarried(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string npcDisplayName = GetNpcDisplayName(args[1]);
		return GetTranslation("cond.notMarried", new
		{
			name = npcDisplayName
		});
	}

	private static string TranslateMarried(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string npcDisplayName = GetNpcDisplayName(args[1]);
		return GetTranslation("cond.married", new
		{
			name = npcDisplayName
		});
	}

	private static string TranslateNpcPresent(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string npcDisplayName = GetNpcDisplayName(args[1]);
		return GetTranslation("cond.npcPresent", new
		{
			name = npcDisplayName
		});
	}

	private static string TranslateRandom(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		if (float.TryParse(args[1], out var result))
		{
			return GetTranslation("cond.random", new
			{
				percent = result * 100f
			});
		}
		return GetTranslation("cond.unknown");
	}

	private static string TranslateSeenEvent(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.seenEvent");
		}
		if (args.Length == 2)
		{
			return GetTranslation("cond.seenEvent") + ": " + args[1];
		}
		string text = string.Join(", ", args.Skip(1));
		return GetTranslation("cond.seenEvent") + ": " + text;
	}

	private static string TranslateNotSeenEvent(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.notSeenEvent");
		}
		if (args.Length == 2)
		{
			return GetTranslation("cond.notSeenEvent") + ": " + args[1];
		}
		string text = string.Join(", ", args.Skip(1));
		return GetTranslation("cond.notSeenEvent") + ": " + text;
	}

	private static string TranslateChoseDialogueAnswers(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		if (args.Length == 2)
		{
			return GetTranslation("cond.dialogueAnswer") + ": " + args[1];
		}
		string text = string.Join(", ", args.Skip(1));
		return GetTranslation("cond.dialogueAnswer") + ": " + text;
	}

	private static string TranslateSecretNote(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		return GetTranslation("cond.secretNote", new
		{
			id = args[1]
		});
	}

	private static string TranslateTime(string[] args)
	{
		int result = 600;
		int result2 = 2600;
		if (args.Length > 1)
		{
			int.TryParse(args[1], out result);
		}
		if (args.Length > 2)
		{
			int.TryParse(args[2], out result2);
		}
		return GetTranslation("cond.time", new
		{
			start = FormatTime(result),
			end = FormatTime(result2)
		});
	}

	private static string TranslateGender(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string text = args[1].ToLower();
		return GetTranslation("cond.gender", new
		{
			gender = GetTranslation("gender." + text)
		});
	}

	private static string TranslateNotActiveDialogueEvent(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string text = args[1];
		if (Game1.player?.activeDialogueEvents != null && ((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)Game1.player.activeDialogueEvents).ContainsKey(text))
		{
			int num = ((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)Game1.player.activeDialogueEvents)[text];
			if (num > 0)
			{
				return GetTranslation("cond.conversationTopicActive", new
				{
					topic = text,
					days = num
				});
			}
			return GetTranslation("cond.conversationTopicExpiring", new
			{
				topic = text
			});
		}
		return GetTranslation("cond.conversationTopic", new
		{
			topic = text
		});
	}

	private static string TranslateActiveDialogueEvent(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string text = args[1];
		if (Game1.player?.activeDialogueEvents != null && ((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)Game1.player.activeDialogueEvents).ContainsKey(text))
		{
			int num = ((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)Game1.player.activeDialogueEvents)[text];
			if (num > 0)
			{
				return GetTranslation("cond.activeDialogueTopicActive", new
				{
					topic = text,
					days = num
				});
			}
			return GetTranslation("cond.activeDialogueTopic", new
			{
				topic = text
			});
		}
		return GetTranslation("cond.activeDialogueTopicNotActive", new
		{
			topic = text
		});
	}

	private static string TranslateTotalMoney(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		int.TryParse(args[1], out var result);
		return GetTranslation("cond.totalMoney", new
		{
			amount = result
		});
	}

	private static string TranslateGameStateQuery(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string query = string.Join(" ", args.Skip(1));
		return GetTranslation("cond.gameStateQuery", new { query });
	}

	private static string TranslateShippedItem(string[] args)
	{
		if (args.Length < 3)
		{
			return GetTranslation("cond.unknown");
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i + 1 < args.Length; i += 2)
		{
			string itemDisplayName = GetItemDisplayName(args[i]);
			if (int.TryParse(args[i + 1], out var result))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(GetTranslation("cond.shippedItem", new
				{
					item = itemDisplayName,
					count = result
				}));
			}
		}
		if (stringBuilder.Length <= 0)
		{
			return GetTranslation("cond.unknown");
		}
		return stringBuilder.ToString();
	}

	private static string TranslateNpcVisible(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string npcDisplayName = GetNpcDisplayName(args[1]);
		return GetTranslation("cond.npcVisible", new
		{
			name = npcDisplayName
		});
	}

	private static string TranslateNotLocalMail(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.noMail");
		}
		return GetTranslation("cond.notReceivedMail", new
		{
			mail = args[1]
		});
	}

	private static string TranslateNoFestivalDays(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		int.TryParse(args[1], out var result);
		return GetTranslation("cond.noFestivalDays", new
		{
			days = result
		});
	}

	private static string TranslateWeather(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string translation = GetTranslation("weather." + args[1].ToLower());
		return GetTranslation("cond.weather", new
		{
			weather = translation
		});
	}

	private static string TranslateNotWeather(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		string translation = GetTranslation("weather." + args[1].ToLower());
		return GetTranslation("cond.notWeather", new
		{
			weather = translation
		});
	}

	private static string TranslateYear(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		int.TryParse(args[1], out var result);
		if (result != 1)
		{
			return GetTranslation("cond.yearOrLater", new
			{
				year = result
			});
		}
		return GetTranslation("cond.yearOne");
	}

	private static string TranslateNotSeason(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i < args.Length; i++)
		{
			if (!string.IsNullOrEmpty(args[i]))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(GetTranslation("season." + args[i].ToLower()));
			}
		}
		return GetTranslation("cond.notSeason", new
		{
			seasons = stringBuilder.ToString()
		});
	}

	private static string TranslateTile(string[] args)
	{
		if (args.Length < 3)
		{
			return GetTranslation("cond.entry");
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i + 1 < args.Length; i += 2)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" " + GetTranslation("cond.or") + " ");
			}
			stringBuilder.Append(GetTranslation("cond.tile", new
			{
				x = args[i],
				y = args[i + 1]
			}));
		}
		if (stringBuilder.Length <= 0)
		{
			return GetTranslation("cond.entry");
		}
		return stringBuilder.ToString();
	}

	private static string TranslateReachedMineBottom(string[] args)
	{
		int result;
		int count = ((args.Length < 2 || !int.TryParse(args[1], out result)) ? 1 : result);
		return GetTranslation("cond.reachedMineBottom", new { count });
	}

	private static string TranslateNotDayOfWeek(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i < args.Length; i++)
		{
			if (!string.IsNullOrEmpty(args[i]))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(GetTranslation("day." + args[i].ToLower()));
			}
		}
		return GetTranslation("cond.notDayOfWeek", new
		{
			days = stringBuilder.ToString()
		});
	}

	private static string TranslateDaysPlayed(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		return GetTranslation("cond.daysPlayed", new
		{
			days = args[1]
		});
	}

	private static string TranslateEarnedMoney(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		int.TryParse(args[1], out var result);
		return GetTranslation("cond.totalMoney", new
		{
			amount = result
		});
	}

	private static string TranslateLocalMail(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		return GetTranslation("cond.localMail", new
		{
			mail = args[1]
		});
	}

	private static string TranslateGoldenWalnuts(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		return GetTranslation("cond.goldenWalnuts", new
		{
			count = args[1]
		});
	}

	private static string TranslateRoommate(string[] args)
	{
		return GetTranslation("cond.roommate");
	}

	private static string TranslateDialogueAnswer(string[] args)
	{
		if (args.Length < 3)
		{
			return GetTranslation("cond.questionAnswer");
		}
		return GetTranslation("cond.hasDialogueAnswer", new
		{
			question = args[1],
			answer = args[2]
		});
	}

	private static string TranslateDayOfMonth(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.dayOfMonth");
		}
		string text = string.Join(", ", args.Skip(1));
		return GetTranslation("cond.dayOfMonth") + ": " + text;
	}

	private static string TranslateNotUpcomingFestival(string[] args)
	{
		if (args.Length < 2)
		{
			return GetTranslation("cond.unknown");
		}
		return GetTranslation("cond.notUpcomingFestival");
	}

	private static string TranslateMissingPet(string[] args)
	{
		string type = ((args.Length >= 2) ? args[1] : "any");
		return GetTranslation("cond.missingPet", new { type });
	}

	private static string TranslateInUpgradedHouse(string[] args)
	{
		int level = 2;
		if (args.Length >= 2 && int.TryParse(args[1], out var result))
		{
			level = result;
		}
		return GetTranslation("cond.inUpgradedHouse", new { level });
	}

	private static string TranslateWorldStateOrMail(string[] args, string fullCondition)
	{
		if (fullCondition.Length > 1)
		{
			char c = fullCondition[1];
			if (c == 'n' && args.Length >= 2)
			{
				return GetTranslation("cond.hostOrLocalMail", new
				{
					mail = args[1]
				});
			}
			if (c == 'l' && args.Length >= 2)
			{
				return GetTranslation("cond.notHostOrLocalMail", new
				{
					mail = args[1]
				});
			}
		}
		if (args.Length >= 2)
		{
			return GetTranslation("cond.worldState", new
			{
				id = args[1]
			});
		}
		return GetTranslation("cond.unknown");
	}

	private static string GetNpcDisplayName(string npcName)
	{
		if (string.IsNullOrEmpty(npcName) || (npcName.Contains("{") && npcName.Contains("}")))
		{
			return Translation.op_Implicit(MHEventsListMod.I18n.Get("cond.unknownNpc"));
		}
		NPC characterFromName = Game1.getCharacterFromName(npcName, false, false);
		return ((characterFromName != null) ? ((Character)characterFromName).displayName : null) ?? ((characterFromName != null) ? ((Character)characterFromName).Name : null) ?? npcName;
	}

	private static string GetItemDisplayName(string itemId)
	{
		if (string.IsNullOrEmpty(itemId) || (itemId.Contains("{") && itemId.Contains("}")))
		{
			return Translation.op_Implicit(MHEventsListMod.I18n.Get("cond.unknownItem"));
		}
		try
		{
			ParsedItemData data = ItemRegistry.GetData(itemId);
			if (data != null)
			{
				return data.DisplayName;
			}
			if (int.TryParse(itemId, out var _))
			{
				data = ItemRegistry.GetData("(O)" + itemId);
				if (data != null)
				{
					return data.DisplayName;
				}
			}
			if (!itemId.StartsWith("("))
			{
				string[] array = new string[8] { "(O)", "(BC)", "(W)", "(H)", "(B)", "(F)", "(T)", "(P)" };
				string[] array2 = array;
				foreach (string text in array2)
				{
					Item val = ItemRegistry.Create(text + itemId, 1, 0, true);
					if (val != null)
					{
						return val.DisplayName;
					}
				}
			}
			else
			{
				Item val2 = ItemRegistry.Create(itemId, 1, 0, true);
				if (val2 != null)
				{
					return val2.DisplayName;
				}
			}
		}
		catch
		{
		}
		return itemId;
	}

	private static string FormatTime(int time)
	{
		int num = time % 2400;
		int num2 = num / 100;
		int value = num % 100;
		if (!MHEventsListMod.Config.Use24HourClock)
		{
			string value2 = ((num2 >= 12) ? "pm" : "am");
			if (num2 > 12)
			{
				num2 -= 12;
			}
			if (num2 == 0)
			{
				num2 = 12;
			}
			return $"{num2}:{value:D2}{value2}";
		}
		return $"{num2}:{value:D2}";
	}
}
