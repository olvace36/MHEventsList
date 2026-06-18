using System;
using System.Collections.Generic;
using System.Linq;
using MHEventsList.Config;
using MHEventsList.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;

namespace MHEventsList.UI;

public sealed class EventOverlay : IClickableMenu
{
	private const int BASE_WIDTH = 400;

	private const int BASE_HEADER = 40;

	private const int BASE_PADDING = 15;

	private static readonly List<string> PinnedEventIds = new List<string>();

	private readonly List<EventData> _pinnedEvents = new List<EventData>();

	private Rectangle _headerBounds;

	private Rectangle _minimizeBounds;

	private readonly List<Rectangle> _eventBounds = new List<Rectangle>();

	private readonly List<Rectangle> _unpinButtonBounds = new List<Rectangle>();

	private bool _isMinimized;

	private bool _isDragging;

	private Point _dragOffset;

	private int _clickCooldown;

	private static int MaxPinnedEvents => MHEventsListMod.Config?.MaxPinnedEvents ?? 3;

	private float Scale => (float)MHEventsListMod.Config.OverlayScale / 100f;

	private int ScaledWidth => (int)(400f * Scale);

	private int ScaledHeader => (int)(40f * Scale);

	private int ScaledPadding => (int)(15f * Scale);

	public static bool HasPinnedEvents => PinnedEventIds.Count > 0;

	public static int PinnedCount => PinnedEventIds.Count;

	public static bool IsEventPinned(string eventId)
	{
		return PinnedEventIds.Contains(eventId);
	}

	public EventOverlay()
		: base(0, 0, 400, 0, false)
	{
		InitializePosition();
		RefreshPinnedEvents();
	}

	public bool IsHeaderClick(int x, int y)
	{
		if (((Rectangle)(ref _headerBounds)).Contains(x, y))
		{
			return !((Rectangle)(ref _minimizeBounds)).Contains(x, y);
		}
		return false;
	}

	public void StartDrag(int x, int y)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_isDragging = true;
		_dragOffset = new Point(x - base.xPositionOnScreen, y - base.yPositionOnScreen);
	}

	public void UpdatePosition()
	{
		bool flag = MHEventsListMod.Config.OverlayPosition?.ToLower() != "left";
		int overlayOffsetX = MHEventsListMod.Config.OverlayOffsetX;
		int overlayOffsetY = MHEventsListMod.Config.OverlayOffsetY;
		if (flag)
		{
			base.xPositionOnScreen = ((Rectangle)(ref Game1.uiViewport)).Width - ScaledWidth - 20 + overlayOffsetX;
		}
		else
		{
			base.xPositionOnScreen = 20 + overlayOffsetX;
		}
		base.yPositionOnScreen = overlayOffsetY;
		base.xPositionOnScreen = Math.Clamp(base.xPositionOnScreen, 0, ((Rectangle)(ref Game1.uiViewport)).Width - ScaledWidth);
		base.yPositionOnScreen = Math.Clamp(base.yPositionOnScreen, 0, ((Rectangle)(ref Game1.uiViewport)).Height - 200);
		CalculateLayout();
	}

	private void InitializePosition()
	{
		UpdatePosition();
	}

	public static bool PinEvent(string eventId)
	{
		int num = MHEventsListMod.Config?.MaxPinnedEvents ?? 3;
		if (PinnedEventIds.Count >= num)
		{
			return false;
		}
		EventData eventById = EventRegistry.GetEventById(eventId);
		if (eventById != null)
		{
			if (string.IsNullOrEmpty(eventById.RawPreconditions) && eventById.HasWhenConditions)
			{
				return false;
			}
			List<string> conditionsToShow = GetConditionsToShow(eventById);
			if (conditionsToShow.Count == 0)
			{
				return false;
			}
		}
		if (!PinnedEventIds.Contains(eventId))
		{
			PinnedEventIds.Add(eventId);
			Game1.playSound("coin", (int?)null);
			EventUserDataManager.Instance?.Save();
			return true;
		}
		return false;
	}

	private static List<string> GetConditionsToShow(EventData evt)
	{
		List<string> list = new List<string>();
		if (evt == null || string.IsNullOrEmpty(evt.RawPreconditions))
		{
			return list;
		}
		try
		{
			string[] array = evt.RawPreconditions.Split('/');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!string.IsNullOrEmpty(text) && !text.StartsWith("x ") && (!text.Contains("{{") || !text.Contains("}}")))
				{
					list.Add(text);
				}
			}
		}
		catch
		{
		}
		return list;
	}

	public static bool UnpinEvent(string eventId)
	{
		if (PinnedEventIds.Remove(eventId))
		{
			Game1.playSound("trashcan", (int?)null);
			EventUserDataManager.Instance?.Save();
			return true;
		}
		return false;
	}

	public static bool TogglePin(string eventId)
	{
		if (IsEventPinned(eventId))
		{
			return UnpinEvent(eventId);
		}
		return PinEvent(eventId);
	}

	public static void ClearPins()
	{
		PinnedEventIds.Clear();
	}

	public static void LoadPinnedEvents(IEnumerable<string> eventIds)
	{
		PinnedEventIds.Clear();
		foreach (string eventId in eventIds)
		{
			if (PinnedEventIds.Count < MaxPinnedEvents)
			{
				PinnedEventIds.Add(eventId);
			}
		}
	}

	public static List<string> GetPinnedEventIds()
	{
		return new List<string>(PinnedEventIds);
	}

	public static void TrimPinnedEvents(int maxCount)
	{
		while (PinnedEventIds.Count > maxCount)
		{
			PinnedEventIds.RemoveAt(PinnedEventIds.Count - 1);
		}
		EventUserDataManager.Instance?.Save();
	}

	public void RefreshPinnedEvents()
	{
		_pinnedEvents.Clear();
		foreach (string item in PinnedEventIds.ToList())
		{
			EventData eventById = EventRegistry.GetEventById(item);
			if (eventById != null)
			{
				_pinnedEvents.Add(eventById);
			}
			else
			{
				PinnedEventIds.Remove(item);
			}
		}
		CalculateLayout();
	}

	private int GetEventHeight(EventData evt)
	{
		int num = (int)(32f * Scale);
		List<(string, bool)> conditionsWithStatus = GetConditionsWithStatus(evt);
		int count = conditionsWithStatus.Count;
		int num2 = (int)((float)(count * 19) * Scale);
		int num3 = (int)(12f * Scale);
		return num + num2 + num3;
	}

	private List<(string description, bool isMet)> GetConditionsWithStatus(EventData evt)
	{
		List<(string, bool)> list = new List<(string, bool)>();
		if (evt == null)
		{
			return list;
		}
		if (string.IsNullOrEmpty(evt.RawPreconditions))
		{
			return list;
		}
		try
		{
			string[] array = evt.RawPreconditions.Split('/');
			GameLocation val = Game1.getLocationFromName(evt.LocationName);
			if (val == null)
			{
				val = Game1.currentLocation;
			}
			if (val == null)
			{
				return list;
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!string.IsNullOrEmpty(text) && !text.StartsWith("x ") && (!text.Contains("{{") || !text.Contains("}}")))
				{
					string item = ConditionTranslator.Translate(text);
					bool flag = false;
					try
					{
						string text2 = val.checkEventPrecondition("-8888888/" + text, false);
						flag = text2 != "-1";
					}
					catch
					{
						flag = false;
					}
					list.Add((item, flag));
				}
			}
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Error checking conditions for event " + evt.Id + ": " + ex.Message, (LogLevel)0);
		}
		return list;
	}

	private void CalculateLayout()
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		int count = _pinnedEvents.Count;
		base.width = ScaledWidth;
		if (_isMinimized || count == 0)
		{
			base.height = ScaledHeader + (int)(10f * Scale);
		}
		else
		{
			int num = 0;
			foreach (EventData pinnedEvent in _pinnedEvents)
			{
				num += GetEventHeight(pinnedEvent);
			}
			base.height = ScaledHeader + num + ScaledPadding * 2 + (int)(5f * Scale);
		}
		base.xPositionOnScreen = Math.Clamp(base.xPositionOnScreen, 0, ((Rectangle)(ref Game1.uiViewport)).Width - base.width);
		base.yPositionOnScreen = Math.Clamp(base.yPositionOnScreen, 0, ((Rectangle)(ref Game1.uiViewport)).Height - base.height);
		_headerBounds = new Rectangle(base.xPositionOnScreen, base.yPositionOnScreen, base.width, ScaledHeader);
		int num2 = (int)(28f * Scale);
		int num3 = (int)(24f * Scale);
		_minimizeBounds = new Rectangle(base.xPositionOnScreen + base.width - (int)(36f * Scale), base.yPositionOnScreen + (int)(8f * Scale), num2, num3);
		_eventBounds.Clear();
		_unpinButtonBounds.Clear();
		if (!_isMinimized)
		{
			int num4 = base.yPositionOnScreen + ScaledHeader + ScaledPadding;
			for (int i = 0; i < count; i++)
			{
				int eventHeight = GetEventHeight(_pinnedEvents[i]);
				_eventBounds.Add(new Rectangle(base.xPositionOnScreen + ScaledPadding, num4, base.width - ScaledPadding * 2, eventHeight - 5));
				int num5 = (int)(22f * Scale);
				_unpinButtonBounds.Add(new Rectangle(base.xPositionOnScreen + base.width - ScaledPadding - num5, num4 + (int)(6f * Scale), num5, num5));
				num4 += eventHeight;
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		if (_pinnedEvents.Count == 0)
		{
			return;
		}
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		Theme current = Theme.Current;
		UIHelpers.DrawPanel(b, new Rectangle(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height), useDarkTheme);
		string text = Translation.op_Implicit(MHEventsListMod.I18n.Get("overlay.title"));
		Color val = (Color)(useDarkTheme ? Color.White : new Color(40, 30, 20));
		float scale = Scale;
		b.DrawString(Game1.smallFont, text, new Vector2((float)(base.xPositionOnScreen + (int)(15f * Scale)), (float)(base.yPositionOnScreen + (int)(10f * Scale))), val, 0f, Vector2.Zero, scale, (SpriteEffects)0, 1f);
		int mouseX = Game1.getMouseX();
		int mouseY = Game1.getMouseY();
		bool flag = ((Rectangle)(ref _minimizeBounds)).Contains(mouseX, mouseY);
		string text2 = (_isMinimized ? "[+]" : "[-]");
		Color val2 = (useDarkTheme ? new Color(180, 180, 180) : new Color(80, 60, 40));
		Color val3 = (useDarkTheme ? new Color(100, 181, 246) : new Color(41, 98, 255));
		Color val4 = (flag ? val3 : val2);
		b.DrawString(Game1.smallFont, text2, new Vector2((float)_minimizeBounds.X, (float)_minimizeBounds.Y), val4, 0f, Vector2.Zero, scale, (SpriteEffects)0, 1f);
		if (!_isMinimized)
		{
			for (int i = 0; i < _pinnedEvents.Count; i++)
			{
				DrawPinnedEvent(b, _pinnedEvents[i], _eventBounds[i], _unpinButtonBounds[i], useDarkTheme, current, i < _pinnedEvents.Count - 1);
			}
		}
	}

	private void DrawPinnedEvent(SpriteBatch b, EventData evt, Rectangle bounds, Rectangle unpinBounds, bool darkMode, Theme theme, bool showDivider)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		int mouseX = Game1.getMouseX();
		int mouseY = Game1.getMouseY();
		if (((Rectangle)(ref bounds)).Contains(mouseX, mouseY))
		{
			Color color = (darkMode ? new Color(55, 55, 70) : new Color(220, 200, 160));
			UIHelpers.DrawRect(b, bounds, color);
		}
		Color val = (darkMode ? new Color(240, 240, 240) : new Color(40, 30, 20));
		Color val2 = (darkMode ? new Color(200, 195, 185) : new Color(70, 55, 40));
		Color success = theme.Success;
		Color val3 = (darkMode ? new Color(255, 180, 180) : new Color(180, 50, 50));
		float scale = Scale;
		float num = 0.95f * scale;
		float num2 = 0.75f * scale;
		int num3 = bounds.X + (int)(10f * Scale);
		int num4 = bounds.Y + (int)(8f * Scale);
		int num5 = bounds.Width - (int)(50f * Scale);
		string text = evt.DisplayName ?? evt.LocationName ?? "Event";
		string text2 = UIHelpers.TruncateText(text, Game1.smallFont, num5, num);
		b.DrawString(Game1.smallFont, text2, new Vector2((float)num3, (float)num4), val, 0f, Vector2.Zero, num, (SpriteEffects)0, 1f);
		num4 += (int)(26f * Scale);
		List<(string, bool)> conditionsWithStatus = GetConditionsWithStatus(evt);
		foreach (var item2 in conditionsWithStatus)
		{
			string item = item2.Item1;
			Color val4 = (item2.Item2 ? success : val3);
			string text3 = UIHelpers.TruncateText("• " + item, Game1.smallFont, num5 - (int)(35f * Scale), num2);
			b.DrawString(Game1.smallFont, text3, new Vector2((float)num3, (float)num4), val4, 0f, Vector2.Zero, num2, (SpriteEffects)0, 1f);
			num4 += (int)(19f * Scale);
		}
		Color val5 = (((Rectangle)(ref unpinBounds)).Contains(mouseX, mouseY) ? theme.Error : theme.TextSecondary);
		float scale2 = Scale;
		b.DrawString(Game1.smallFont, "X", new Vector2((float)(unpinBounds.X + (int)(5f * Scale)), (float)(unpinBounds.Y + (int)(2f * Scale))), val5, 0f, Vector2.Zero, scale2, (SpriteEffects)0, 1f);
		if (showDivider)
		{
			UIHelpers.DrawRect(b, new Rectangle(bounds.X + 5, ((Rectangle)(ref bounds)).Bottom + 3, bounds.Width - 10, 1), darkMode ? new Color(70, 70, 85) : new Color(170, 155, 125));
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (_clickCooldown > 0)
		{
			return;
		}
		if (((Rectangle)(ref _minimizeBounds)).Contains(x, y))
		{
			_isMinimized = !_isMinimized;
			Game1.playSound("smallSelect", (int?)null);
			CalculateLayout();
			_clickCooldown = 10;
		}
		else
		{
			if (((Rectangle)(ref _headerBounds)).Contains(x, y) || _isMinimized)
			{
				return;
			}
			for (int i = 0; i < _unpinButtonBounds.Count; i++)
			{
				Rectangle val = _unpinButtonBounds[i];
				if (((Rectangle)(ref val)).Contains(x, y))
				{
					if (i < _pinnedEvents.Count)
					{
						Game1.playSound("trashcan", (int?)null);
						UnpinEvent(_pinnedEvents[i].Id);
						RefreshPinnedEvents();
						_clickCooldown = 15;
					}
					break;
				}
			}
		}
	}

	public override void leftClickHeld(int x, int y)
	{
		if (_isDragging)
		{
			base.xPositionOnScreen = x - _dragOffset.X;
			base.yPositionOnScreen = y - _dragOffset.Y;
			CalculateLayout();
		}
	}

	public override void releaseLeftClick(int x, int y)
	{
		if (_isDragging)
		{
			_isDragging = false;
			if (MHEventsListMod.Config.OverlayPosition?.ToLower() != "left")
			{
				int num = ((Rectangle)(ref Game1.uiViewport)).Width - ScaledWidth - 20;
				MHEventsListMod.Config.OverlayOffsetX = base.xPositionOnScreen - num;
			}
			else
			{
				MHEventsListMod.Config.OverlayOffsetX = base.xPositionOnScreen - 20;
			}
			MHEventsListMod.Config.OverlayOffsetY = base.yPositionOnScreen;
			MHEventsListMod.Helper.WriteConfig<ModConfig>(MHEventsListMod.Config);
		}
	}

	public override void update(GameTime time)
	{
		((IClickableMenu)this).update(time);
		if (_clickCooldown > 0)
		{
			_clickCooldown--;
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		InitializePosition();
		CalculateLayout();
	}
}
