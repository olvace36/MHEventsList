using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace MHEventsList.Integration;

public class ContentPatcherIntegration
{
	private readonly IContentPatcherAPI? Api;

	private readonly IManifest Manifest;

	private readonly Dictionary<string, IManagedConditions> ConditionsCache = new Dictionary<string, IManagedConditions>();

	private readonly Dictionary<string, IManagedTokenString> TokenStringCache = new Dictionary<string, IManagedTokenString>();

	public bool IsAvailable => Api != null;

	public bool IsReady => Api?.IsConditionsApiReady ?? false;

	public ContentPatcherIntegration(IModRegistry modRegistry, IManifest manifest)
	{
		Manifest = manifest;
		try
		{
			Api = modRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
			if (Api != null)
			{
				MHEventsListMod.Monitor.Log("Content Patcher API loaded successfully.", (LogLevel)2);
			}
			else
			{
				MHEventsListMod.Monitor.Log("Content Patcher not found. Using manual condition evaluation.", (LogLevel)2);
			}
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Error loading Content Patcher API: " + ex.Message, (LogLevel)3);
			Api = null;
		}
	}

	public (bool canEvaluate, bool isMatch, string? reason) EvaluateConditions(Dictionary<string, string> conditions)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		if (!IsAvailable || !IsReady || conditions == null || conditions.Count == 0)
		{
			return (canEvaluate: false, isMatch: false, reason: "Content Patcher not available");
		}
		try
		{
			string text = GenerateCacheKey(conditions);
			if (!ConditionsCache.TryGetValue(text, out IManagedConditions value))
			{
				if (MHEventsListMod.Config.VerboseLogging)
				{
					MHEventsListMod.Monitor.Log("[CP API] Parsing conditions: " + text, (LogLevel)0);
				}
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				foreach (KeyValuePair<string, string> condition in conditions)
				{
					dictionary[condition.Key] = condition.Value;
				}
				value = Api.ParseConditions(Manifest, dictionary, (ISemanticVersion)new SemanticVersion("2.8.0"));
				ConditionsCache[text] = value;
			}
			if (!value.IsValid)
			{
				if (MHEventsListMod.Config.VerboseLogging)
				{
					MHEventsListMod.Monitor.Log("[CP API] Invalid conditions: " + value.ValidationError, (LogLevel)0);
				}
				return (canEvaluate: false, isMatch: false, reason: value.ValidationError ?? "Invalid conditions");
			}
			value.UpdateContext();
			if (!value.IsReady)
			{
				if (MHEventsListMod.Config.VerboseLogging)
				{
					MHEventsListMod.Monitor.Log("[CP API] Conditions not ready", (LogLevel)0);
				}
				return (canEvaluate: false, isMatch: false, reason: "Conditions not ready (tokens unavailable)");
			}
			bool isMatch = value.IsMatch;
			string text2 = (isMatch ? null : value.GetReasonNotMatched());
			if (MHEventsListMod.Config.VerboseLogging)
			{
				MHEventsListMod.Monitor.Log($"[CP API] IsMatch: {isMatch}, Reason: {text2 ?? "matched"}", (LogLevel)0);
			}
			return (canEvaluate: true, isMatch: isMatch, reason: text2);
		}
		catch (Exception ex)
		{
			if (MHEventsListMod.Config.VerboseLogging)
			{
				MHEventsListMod.Monitor.Log("Error evaluating conditions via CP API: " + ex.Message, (LogLevel)0);
			}
			return (canEvaluate: false, isMatch: false, reason: ex.Message);
		}
	}

	public (bool canEvaluate, bool isMatch) EvaluateSingleCondition(string key, string value)
	{
		if (MHEventsListMod.Config.VerboseLogging)
		{
			MHEventsListMod.Monitor.Log("[CP API] Evaluating: " + key + " = " + value, (LogLevel)0);
			MHEventsListMod.Monitor.Log($"[CP API] IsAvailable: {IsAvailable}, IsReady: {IsReady}", (LogLevel)0);
		}
		Dictionary<string, string> conditions = new Dictionary<string, string> { [key] = value };
		(bool, bool, string) tuple = EvaluateConditions(conditions);
		if (MHEventsListMod.Config.VerboseLogging)
		{
			MHEventsListMod.Monitor.Log($"[CP API] Result: canEvaluate={tuple.Item1}, isMatch={tuple.Item2}, reason={tuple.Item3}", (LogLevel)0);
		}
		return (canEvaluate: tuple.Item1, isMatch: tuple.Item2);
	}

	public void ClearCache()
	{
		ConditionsCache.Clear();
		TokenStringCache.Clear();
	}

	public string ResolveTokens(string text, string? modId = null)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		if (!IsAvailable || !IsReady || string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (!text.Contains("{{") || !text.Contains("}}"))
		{
			return text;
		}
		try
		{
			string key = $"{modId ?? ""}:{text.GetHashCode()}";
			if (!TokenStringCache.TryGetValue(key, out IManagedTokenString value))
			{
				string[] assumeModIds = (string.IsNullOrEmpty(modId) ? null : new string[1] { modId });
				value = Api.ParseTokenString(Manifest, text, (ISemanticVersion)new SemanticVersion("2.8.0"), assumeModIds);
				TokenStringCache[key] = value;
			}
			if (!value.IsValid)
			{
				MHEventsListMod.Monitor.Log("[CP Token] Token validation failed: " + value.ValidationError, (LogLevel)1);
				return text;
			}
			value.UpdateContext();
			if (!value.IsReady)
			{
				MHEventsListMod.Monitor.Log("[CP Token] Tokens not ready in current context", (LogLevel)1);
				return text;
			}
			string value2 = value.Value;
			if (!string.IsNullOrEmpty(value2))
			{
				if (MHEventsListMod.Config.VerboseLogging && value2 != text)
				{
					MHEventsListMod.Monitor.Log($"[CP Token] Resolved: {text.Substring(0, Math.Min(50, text.Length))}... -> {value2.Substring(0, Math.Min(50, value2.Length))}...", (LogLevel)0);
				}
				return value2;
			}
			return text;
		}
		catch (Exception ex)
		{
			if (MHEventsListMod.Config.VerboseLogging)
			{
				MHEventsListMod.Monitor.Log("[CP Token] Error resolving tokens: " + ex.Message, (LogLevel)0);
			}
			return text;
		}
	}

	private static string GenerateCacheKey(Dictionary<string, string> conditions)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> condition in conditions)
		{
			list.Add(condition.Key + "=" + condition.Value);
		}
		list.Sort();
		return string.Join("|", list);
	}
}
