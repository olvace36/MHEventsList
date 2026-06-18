using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using StardewValley;

namespace MHEventsList.Core;

public static class I18nResolver
{
	private static readonly Dictionary<string, Dictionary<string, string>> I18nCache = new Dictionary<string, Dictionary<string, string>>();

	public static string ResolveI18nTokens(string script, string modPath)
	{
		if (string.IsNullOrEmpty(script) || string.IsNullOrEmpty(modPath))
		{
			return script;
		}
		if (!script.Contains("{{i18n:") && !script.Contains("{{I18n:"))
		{
			return script;
		}
		try
		{
			Dictionary<string, string> dictionary = LoadI18n(modPath);
			if (dictionary == null || dictionary.Count == 0)
			{
				return script;
			}
			string text = script;
			int num = 100;
			int num2 = 0;
			while ((text.Contains("{{i18n:") || text.Contains("{{I18n:")) && num2 < num)
			{
				num2++;
				int num3 = text.IndexOf("{{i18n:", StringComparison.OrdinalIgnoreCase);
				if (num3 < 0)
				{
					break;
				}
				int num4 = num3 + 7;
				int i;
				for (i = num4; i < text.Length; i++)
				{
					switch (text[i])
					{
					case '}':
						if (i + 1 >= text.Length || text[i + 1] != '}')
						{
							continue;
						}
						break;
					default:
						continue;
					case ' ':
					case '|':
						break;
					}
					break;
				}
				string text2 = text.Substring(num4, i - num4).Trim();
				int num5 = 1;
				int num6 = num3 + 2;
				while (num6 < text.Length - 1 && num5 > 0)
				{
					if (text[num6] == '{' && num6 + 1 < text.Length && text[num6 + 1] == '{')
					{
						num5++;
						num6 += 2;
					}
					else if (text[num6] == '}' && num6 + 1 < text.Length && text[num6 + 1] == '}')
					{
						num5--;
						if (num5 == 0)
						{
							num6 += 2;
							break;
						}
						num6 += 2;
					}
					else
					{
						num6++;
					}
				}
				string text3;
				if (dictionary.TryGetValue(text2, out var value))
				{
					if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length > 2)
					{
						value = value.Substring(1, value.Length - 2);
					}
					text3 = value;
				}
				else
				{
					text3 = "[" + text2 + "]";
				}
				text = text.Substring(0, num3) + text3 + text.Substring(num6);
			}
			text = text.Replace("\\\"", "\"");
			while (text.Contains("{{i18n:") || text.Contains("{{I18n:"))
			{
				int num7 = text.IndexOf("{{i18n:", StringComparison.OrdinalIgnoreCase);
				if (num7 < 0)
				{
					break;
				}
				int num8 = text.IndexOf("}}", num7);
				if (num8 <= num7)
				{
					break;
				}
				string text4 = text.Substring(num7 + 2, num8 - num7 - 2);
				text = text.Substring(0, num7) + "[" + text4 + "]" + text.Substring(num8 + 2);
			}
			return text;
		}
		catch
		{
			return script;
		}
	}

	public static string CleanupUnresolvedTokens(string script)
	{
		if (string.IsNullOrEmpty(script) || !script.Contains("{{"))
		{
			return script;
		}
		string text = script;
		int num = 100;
		int num2 = 0;
		while (text.Contains("{{") && text.Contains("}}") && num2 < num)
		{
			num2++;
			int num3 = text.IndexOf("{{");
			int num4 = text.IndexOf("}}", num3);
			if (num4 <= num3)
			{
				break;
			}
			string text2 = text.Substring(num3 + 2, num4 - num3 - 2);
			string text3 = text2;
			if (text3.Contains("|"))
			{
				text3 = text3.Substring(0, text3.IndexOf('|')).Trim();
			}
			text3 = text3.Replace("{{", "").Replace("}}", "");
			if (text3.Contains("_Name"))
			{
				string text4 = text3[..text3.LastIndexOf("_Name")];
				int num5 = text4.LastIndexOf('_');
				if (num5 >= 0)
				{
					text3 = text4.Substring(num5 + 1);
				}
			}
			else if (text3.Contains("_Upper") || text3.Contains("_Lower"))
			{
				int num6 = text3.IndexOf("_Upper");
				if (num6 < 0)
				{
					num6 = text3.IndexOf("_Lower");
				}
				if (num6 > 0)
				{
					string text5 = text3.Substring(0, num6);
					int num7 = text5.LastIndexOf('_');
					if (num7 >= 0)
					{
						text3 = text5.Substring(num7 + 1);
					}
				}
			}
			text = text.Substring(0, num3) + text3 + text.Substring(num4 + 2);
		}
		return text;
	}

	private static Dictionary<string, string> LoadI18n(string modPath)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (I18nCache.TryGetValue(modPath, out var value) && value.Count > 0)
		{
			return value;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		try
		{
			string text = Path.Combine(modPath, "i18n");
			if (!Directory.Exists(text))
			{
				text = FindI18nFolder(modPath);
				if (string.IsNullOrEmpty(text))
				{
					I18nCache[modPath] = dictionary;
					return dictionary;
				}
			}
			string text2 = ((object)LocalizedContentManager.CurrentLanguageCode/*cast due to constrained. prefix*/).ToString().ToLower();
			string[] array = new string[3]
			{
				text2 + ".json",
				"default.json",
				".json"
			};
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				string path = Path.Combine(text, text3);
				if (!File.Exists(path))
				{
					continue;
				}
				try
				{
					string json = File.ReadAllText(path);
					JsonSerializerOptions options = new JsonSerializerOptions
					{
						ReadCommentHandling = JsonCommentHandling.Skip,
						AllowTrailingCommas = true
					};
					Dictionary<string, string> dictionary2 = JsonSerializer.Deserialize<Dictionary<string, string>>(json, options);
					if (dictionary2 != null)
					{
						foreach (KeyValuePair<string, string> item in dictionary2)
						{
							if (!dictionary.ContainsKey(item.Key))
							{
								dictionary[item.Key] = item.Value;
							}
						}
					}
				}
				catch
				{
				}
				if (text3 == "default.json" || text3 == ".json")
				{
					break;
				}
			}
		}
		catch
		{
		}
		I18nCache[modPath] = dictionary;
		return dictionary;
	}

	private static string FindI18nFolder(string modPath)
	{
		if (string.IsNullOrEmpty(modPath) || !Directory.Exists(modPath))
		{
			return null;
		}
		try
		{
			string[] directories = Directory.GetDirectories(modPath);
			foreach (string path in directories)
			{
				string text = Path.Combine(path, "i18n");
				if (Directory.Exists(text))
				{
					return text;
				}
			}
			foreach (string item in Directory.EnumerateDirectories(modPath, "i18n", SearchOption.AllDirectories))
			{
				if (Directory.Exists(item))
				{
					return item;
				}
			}
		}
		catch
		{
		}
		return null;
	}

	public static string GetModPathFromSource(string modSource)
	{
		if (string.IsNullOrEmpty(modSource))
		{
			return null;
		}
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "Stardew Valley", "Mods");
		if (Directory.Exists(path))
		{
			string[] directories = Directory.GetDirectories(path);
			foreach (string text in directories)
			{
				string fileName = Path.GetFileName(text);
				if (fileName.Contains(modSource, StringComparison.OrdinalIgnoreCase))
				{
					return text;
				}
			}
		}
		return null;
	}

	public static void ClearCache()
	{
		I18nCache.Clear();
	}
}
