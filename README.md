# SackranyUI

A lightweight MVVM UI framework for Unity. Views are plain `MonoBehaviour`s,
view models are plain C# classes, and the two are wired together automatically
by attribute-driven reactive bindings — no manual `GetComponent`, no per-widget
glue code.

```csharp
public class CounterViewModel : ViewModel<Counter>
{
    [Bind("count_text")]  readonly ReactiveProperty<int> _count = new(0);
    [Bind("add_command")] readonly ReactiveCommand _add = new();

    protected override void OnInitialized()
    {
        Track(_add.Subscribe(_ => _count.Value++));
        Open();
    }
}
```

The view model never touches a component. It declares a **binding key**
(`"count_text"`); a `TextView` on the prefab declares the same key; the
framework matches them and builds the binder.

## Contents

- [Dependencies](#dependencies)
- [Installation](#installation)
- [Core ideas](#core-ideas)
- [Quick start](#quick-start)
- [Binding attributes](#binding-attributes)
- [Reactive building blocks](#reactive-building-blocks)
- [Collections](#collections)
- [Default views and built-in channels](#default-views-and-built-in-channels)
- [Initial values](#initial-values)
- [Lifecycle and transitions](#lifecycle-and-transitions)
- [Events](#events)
- [Extending the bindings](#extending-the-bindings)
- [Scene setup](#scene-setup)
- [Binding validation](#binding-validation)
- [License](#license)

## Dependencies

The core assembly (`SackranyUiCore`) references:

| Package | Used for |
| --- | --- |
| [R3](https://github.com/Cysharp/R3) | reactive properties, commands and streams |
| [UniTask](https://github.com/Cysharp/UniTask) | allocation-free async lifecycle |
| [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) | text components |
| [SerializeReferenceExtensions](https://github.com/mackysoft/Unity-SerializeReferenceExtensions) | the `[SubclassSelector]` dropdown for `[SerializeReference]` fields |
| [Unity Localization](https://docs.unity3d.com/Packages/com.unity.localization@latest) | `LocalizedString` support in `UIExtensions` |

All five must be present in the project or the core will not compile.

## Installation

Copy the `SackranyUI` folder into your project's `Assets` (or reference it as a
local package), then make sure the dependencies above are installed via the
Package Manager / OpenUPM.

## Core ideas

| Concept | Role |
| --- | --- |
| `ViewModel` | State and logic. Holds `ReactiveProperty`, `ReactiveCommand`, `ReactiveList`. |
| `View` | A `MonoBehaviour` on a prefab that exposes Unity components by string key. |
| `IViewModelTemplate` | Serializable data + prefab reference used to create a view model. |
| `IContext` / `UIContext` | Owns the live view models, their prefabs and the event bus. |
| `UIEventBus` | Typed publish/subscribe bus shared by every view model in a context. |

## Quick start

A view model and its template:

```csharp
using R3;
using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

public class CounterViewModel : ViewModel<Counter>
{
    // view model → view: pushes the count to a TextView with key "count_text"
    [Bind("count_text")] readonly ReactiveProperty<int> _count = new(0);

    // view → view model: a ButtonView with key "add_command" raises this command
    [Bind("add_command")] readonly ReactiveCommand _add = new();

    protected override void OnInitialized()
    {
        // Track() disposes the subscription automatically with the view model
        Track(_add.Subscribe(_ => _count.Value++));
        Open();
    }
}

[System.Serializable]
public class Counter : ViewModelTemplate<CounterViewModel> { }
```

The prefab needs a `TextView` with its `TextKey` set to `count_text` and a
`ButtonView` with its `ButtonKey` set to `add_command`. Numbers are formatted to
text automatically through the registered formatters.

## Binding attributes

A view model never references a component directly. Instead both sides declare a
binding key; the framework matches keys and builds the binder.

**View model side:**

| Attribute | Applies to | Direction |
| --- | --- | --- |
| `[Bind(id)]` | reactive field / property, or method | output for reactive members, input for methods |
| `[InitBind(id)]` | field / property | one-shot initial value pushed into the view |

**View side:**

| Attribute | Applies to | Direction |
| --- | --- | --- |
| `[OutputBind(id)]` | field / property / method | view model → view |
| `[InputBind(id)]` | field / property | view → view model |
| `[CollectionBind(id)]` | `CollectionAnchor` field | spawns a child view model per list item |

Both fields and properties can be bound. A member may carry several binding
attributes with different ids. Keys are matched after the view's per-instance
remap is applied (see [Default views](#default-views-and-built-in-channels)).

## Reactive building blocks

State lives in R3 reactive types on the view model:

```csharp
public class HealthBarViewModel : ViewModel<HealthBar>
{
    [Bind("hp_fill")]  readonly ReactiveProperty<float> _fill = new(1f);
    [Bind("hp_text")]  readonly ReactiveProperty<int>   _hp   = new(100);
    [Bind("hit")]      readonly ReactiveCommand<int>    _hit  = new();

    protected override void OnInitialized()
    {
        Track(_hit.Subscribe(dmg =>
        {
            _hp.Value   = Mathf.Max(0, _hp.Value - dmg);
            _fill.Value = _hp.Value / 100f;
        }));
        Open();
    }
}
```

- A `[Bind]` reactive member is **output**: its value is pushed into matching
  `[OutputBind]` members on the view, and re-pushed on every change.
- A `[Bind]` method is **input**: it is invoked by matching `[InputBind]`
  members on the view (a button click, a slider drag, etc.).

## Collections

A `[Bind]` `ReactiveList<TItemVM>` pairs with a `[CollectionBind]`
`CollectionAnchor` on the view to spawn one child view model per item.

```csharp
public class InventoryViewModel : ViewModel<Inventory>
{
    [Bind("slots")] readonly ReactiveList<SlotViewModel> _slots = new();

    protected override void OnInitialized()
    {
        _slots.Add(new SlotViewModel());
        _slots.Add(new SlotViewModel());
        Open();
    }
}
```

On the view, a field of type `CollectionAnchor` marked `[CollectionBind("slots")]`
points at a `Container` transform and an item `Prefab`. Each item view model is
instantiated, bound and initialized inside the same context and event bus as its
owner.

`ReactiveList<T>` supports `Add`, `Insert`, `Remove`, `RemoveAt`, `Move`,
`AddRange`, `Clear`, an indexer, and the `OnAdd` / `OnRemove` / `OnReplace` /
`OnMove` / `OnReset` streams. Item order is mirrored in the spawned hierarchy.

## Default views and built-in channels

Every shipped view derives from `ElementView`, so these keys are available on
**all** of them without writing any extra code:

| Key | Type | Effect |
| --- | --- | --- |
| `alpha` | `float` | sets alpha on every child `Graphic` |
| `color` | `Color` | sets color on every child `Graphic` |
| `active` | `bool` | toggles the element `GameObject` |

Interactive views (`ButtonView`, `SliderView`, `ToggleView`, `InputFieldView`,
`DropdownView`) also derive from `SelectableView` and add:

| Key | Type | Effect |
| --- | --- | --- |
| `interactable` | `bool` | toggles `Selectable.interactable` |

On top of that each view exposes its own keys (and the public `*Key` fields let
you rename any key per instance in the inspector):

| View | Extra keys |
| --- | --- |
| `TextView` | `text` |
| `ButtonView` | `title_text`, `title_color`, `button` |
| `ImageView` | `sprite`, `fill` |
| `RawImageView` | `texture` |
| `SliderView` | `label`, `slider` |
| `ToggleView` | `label`, `toggle` |
| `InputFieldView` | `label`, `input` |
| `DropdownView` | `label`, `dropdown` |
| `CanvasGroupView` | `alpha`, `canvas_interactable`, `canvas_blocks`, `canvas_active` |

Renaming a key per instance just works — the binder and the validator both
resolve the remapped key:

```csharp
// On the prefab: TextView.TextKey = "player_name"
[Bind("player_name")] readonly ReactiveProperty<string> _name = new("Hero");
```

## Initial values

`[InitBind]` pushes a one-shot value into the view before the reactive bindings
start. It is useful for seeding controls or static labels from template data:

```csharp
public class VolumeRowViewModel : ViewModel<VolumeRow>
{
    [InitBind("slider")] float  _initial = 0.8f;  // seeds the slider position
    [InitBind("label")]  string _caption = "SFX"; // seeds a label once
    [InitBind("color")]  Color  _tint    = Color.cyan; // seeds a built-in channel

    [Bind("slider")] readonly ReactiveCommand<float> _changed = new();
    // ...
}
```

`[InitBind]` reaches both the two-way controls (`Slider`, `Toggle`,
`TMP_InputField`, `TMP_Dropdown`) **and** output components/channels — text,
sprites, colors, the `active`/`alpha`/`color` element channels, and anything
with a registered output or text formatter. If a key also has a reactive
`[Bind]`, the reactive value takes over right after the initial value is applied.

## Lifecycle and transitions

`Open()` / `Close()` toggle the prefab synchronously. `OpenAsync()` /
`CloseAsync()` additionally await `OnOpenedAsync` / `OnClosingAsync` and play any
`IUITransition` components found on the prefab. `CanvasGroupTransition` ships as
a ready-made fade.

```csharp
protected override async UniTask OnOpenedAsync(CancellationToken ct)
{
    await LoadDataAsync(ct);
}
```

Override hooks:

| Hook | When |
| --- | --- |
| `OnInitialized()` | once, after wiring — subscribe and set initial state here |
| `OnOpened()` / `OnClosed()` | synchronous open/close |
| `OnOpenedAsync(ct)` / `OnClosingAsync(ct)` | async open/close, around transitions |
| `OnDispose()` | once, when the view model is torn down |

Subscriptions passed to `Track(...)` and bound reactive members are disposed
automatically; the prefab is destroyed by the context.

## Events

Declare an event by deriving from `AUIEvent<TSelf>` (extend the `UIEvents`
partial class or create your own type):

```csharp
public class MyEvent : AUIEvent<MyEvent> { }

// publish
Publish<MyEvent>();
Publish<MyEvent, int>(42);

// subscribe
Subscribe<MyEvent>(() => { });
Subscribe<MyEvent, int>(value => { });
```

Subscriptions returned from `Subscribe` are `IDisposable`; pass them to `Track`
to dispose them with the view model. The bus is shared by every view model in
the same context, so siblings communicate without referencing each other.

## Extending the bindings

Register custom binders, formatters and initializers at startup through
`BinderRegistry` — no core changes required:

```csharp
BinderRegistry.RegisterOutput<float, CanvasGroup>((group, v) => group.alpha = v);
BinderRegistry.RegisterTextFormatter<float>(v => v.ToString("0.0"));
BinderRegistry.RegisterInput<int, TMP_Dropdown>(
    (c, h) => c.onValueChanged.AddListener(h),
    (c, h) => c.onValueChanged.RemoveListener(h));
BinderRegistry.RegisterInit<float, Slider>((c, v) => c.value = v);
```

Values bound to a `TMP_Text` are formatted through the registered text
formatters, so any type with a formatter can be displayed as text.

## Scene setup

1. Add a `UserInterfaceInstaller` component to a Canvas.
2. Assign the default `IViewModelTemplate` list and a `UIContext` (the
   `[SubclassSelector]` dropdown lets you pick concrete types).
3. On `Start` the context instantiates each template, wires the bindings and
   opens the view models.

## Binding validation

In the editor (`UNITY_EDITOR`), every bound view model is checked at runtime:
each `[Bind]` without a matching view key, and each view key without a matching
`[Bind]`, logs a warning. Validation resolves per-instance key remaps, so it
matches exactly what the binder does.

## License

MIT — see [LICENSE](LICENSE).
