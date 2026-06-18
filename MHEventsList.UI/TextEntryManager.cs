using StardewValley;

namespace MHEventsList.UI;

internal class TextEntryManager
{
	private long LastTickOpen;

	public bool IsOpen => Game1.textEntry != null;

	public void Update()
	{
		if (IsOpen)
		{
			LastTickOpen = Game1.ticks;
		}
	}

	public bool JustClosed(int tolerance = 2)
	{
		if (!IsOpen)
		{
			return LastTickOpen >= Game1.ticks - tolerance;
		}
		return false;
	}
}
