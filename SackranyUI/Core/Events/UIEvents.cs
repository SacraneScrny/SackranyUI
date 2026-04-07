using UnityEngine.Scripting;

namespace SackranyUI.Core.Events
{
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