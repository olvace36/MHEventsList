using System;
using System.Collections.Generic;
using MHEventsList.UI;
using StardewModdingAPI;

namespace MHEventsList.Core;

public sealed class EventUserDataManager
{
	private readonly IModHelper _helper;

	private EventUserData _data;

	public static EventUserDataManager Instance { get; private set; }

	public EventUserDataManager(IModHelper helper)
	{
		_helper = helper;
		_data = new EventUserData();
		Instance = this;
	}

	public void Load()
	{
		try
		{
			_data = _helper.Data.ReadSaveData<EventUserData>("event-user-data") ?? new EventUserData();
			EventOverlay.LoadPinnedEvents(_data.PinnedEventIds ?? new List<string>());
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Error loading user data: " + ex.Message, (LogLevel)4);
			_data = new EventUserData();
		}
	}

	public void Save()
	{
		try
		{
			_data.PinnedEventIds = EventOverlay.GetPinnedEventIds();
			_helper.Data.WriteSaveData<EventUserData>("event-user-data", _data);
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Error saving user data: " + ex.Message, (LogLevel)4);
		}
	}

	public List<string> GetPinnedEventIds()
	{
		return _data.PinnedEventIds ?? new List<string>();
	}
}
