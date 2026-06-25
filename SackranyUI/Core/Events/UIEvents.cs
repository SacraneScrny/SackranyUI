using UnityEngine.Scripting;

namespace SackranyUI.Core.Events
{
    /// <summary>Built-in UI events. Extend this partial class to declare your own shared event types.</summary>
    public static partial class UIEvents
    {
        [Preserve] public class CloseAllWindows : AUIEvent<CloseAllWindows> { }
        [Preserve] public class ContinueWindowCall : AUIEvent<ContinueWindowCall> { }
        [Preserve] public class NewGameWindowCall : AUIEvent<NewGameWindowCall> { }
        [Preserve] public class SettingsWindowCall : AUIEvent<SettingsWindowCall> { }
        [Preserve] public class InfoWindowCall : AUIEvent<InfoWindowCall> { }
        [Preserve] public class ExitWindowCall : AUIEvent<ExitWindowCall> { }
    }
}