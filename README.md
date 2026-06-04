# SackranyUI

A lightweight MVVM UI framework for Unity. Views are plain `MonoBehaviour`s,
view models are plain C# classes, and the two are wired together automatically
by attribute-driven reactive bindings.

Built on top of:

- [R3](https://github.com/Cysharp/R3) — reactive properties, commands and streams
- [UniTask](https://github.com/Cysharp/UniTask) — allocation-free async lifecycle
- [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) — text components

## Core ideas

| Concept | Role |
| --- | --- |
| `ViewModel` | State and logic. Holds `ReactiveProperty`, `ReactiveCommand`, `ReactiveList`. |
| `View` | A `MonoBehaviour` on a prefab that exposes Unity components by string key. |
| `IViewModelTemplate` | Serializable data + prefab reference used to create a view model. |
| `IContext` / `UIContext` | Owns the live view models, their prefabs and the event bus. |
| `UIEventBus` | Typed publish/subscribe bus shared by every view model in a context. |

A view model never references a component directly. Instead both sides declare a
**binding key**; the framework matches keys and builds the binder.

## Binding attributes

View model side:

| Attribute | Applies to | Direction |
| --- | --- | --- |
| `[Bind(id)]` | reactive field / property, or method | output for reactive members, input for methods |
| `[InitBind(id)]` | field / property | one-shot initial value pushed into the view |

A `[Bind]` `ReactiveList<TItemVM>` pairs with a `[CollectionBind]`
`CollectionAnchor` on the view to spawn one child view model per item.

View side:

| Attribute | Applies to | Direction |
| --- | --- | --- |
| `[OutputBind(id)]` | field / property / method | view model → view |
| `[InputBind(id)]` | field / property | view → view model |
| `[CollectionBind(id)]` | `CollectionAnchor` field | spawns a child view model per list item |

Both fields and properties can be bound. Members may carry several binding
attributes with different ids.

## Reactive building blocks

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

[Serializable]
public class Counter : ViewModelTemplate<CounterViewModel> { }
```

`ReactiveList<T>` drives collection bindings and supports `Add`, `Insert`,
`Remove`, `RemoveAt`, `Move`, `AddRange`, an indexer and `OnAdd` / `OnRemove` /
`OnReplace` / `OnMove` / `OnReset` streams. Item order is mirrored in the
spawned hierarchy.

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

## Events

```csharp
public class MyEvent : AUIEvent<MyEvent> { }

Publish<MyEvent>();
Publish<MyEvent, int>(42);

Subscribe<MyEvent>(() => { });
Subscribe<MyEvent, int>(value => { });
```

Subscriptions returned from `Subscribe` are `IDisposable`; pass them to `Track`
to dispose them with the view model.

## Extending the bindings

Register custom binders, formatters and initializers at startup through
`BinderRegistry` — no core changes required:

```csharp
BinderRegistry.RegisterOutput<float, CanvasGroup>((group, v) => group.alpha = v);
BinderRegistry.RegisterTextFormatter<float>(v => v.ToString("0.0"));
BinderRegistry.RegisterInput<int, TMP_Dropdown>(
    (c, h) => c.onValueChanged.AddListener(h),
    (c, h) => c.onValueChanged.RemoveListener(h));
```

Values bound to a `TMP_Text` are formatted through the registered text
formatters, so any type with a formatter can be displayed as text.

## Scene setup

1. Add a `UserInterfaceInstaller` component to a Canvas.
2. Assign the default `IViewModelTemplate` list and a `UIContext`.
3. On `Start` the context instantiates each template, wires the bindings and
   opens the view models.

## License

MIT — see [LICENSE](LICENSE).
