# PuerTS 3 integration

OneJS 2.3.3 uses the official PuerTS **3.0.3** Core, V8 and WebGL packages,
upgraded from PuerTS 2.1.0. Version 3.0.3 was the latest stable Unity release
when verified on October 30, 2026:
[official release](https://github.com/Tencent/puerts/releases/tag/Unity_v3.0.3).

The release archives were checked against GitHub's published SHA-256 digests:

| Package | SHA-256 |
| --- | --- |
| Core | `feafc11bf2f9dbdb7e5fd2546124a771ea69b0f7c1a194740d47cfffb721cea0` |
| V8 | `649b2de6dc74b24a17e1c624e5c6e98f381f05b1975b68b055ee8d5c47ea6e27` |
| WebGL | `9e52896a47bc7ba4246576fa780af5b2773ab530302e0e81a7d0637005488b59` |

The embedded package directories are `Puerts`, `Puerts.V8` and `Puerts.WebGL`.
Native binaries use Git LFS. The V8 assembly definition has its Core reference
restored, and two malformed OpenHarmony importer GUIDs have been repaired.
The matching upstream sources are tagged `Unity_v3.0.3`, commit `d887123`.

## OneJS API migration

- JavaScript values and functions use `ScriptObject` in place of the removed
  `JSObject` and `GenericDelegate` types. The event adapter keeps a distinct
  callback for each subscription and preserves unsubscription behavior.
- Existing `JsEnv` integration uses PuerTS's supplied compatibility class.
  Its CoreCLR bridge owns the callback lifetimes; no Mono function pointers
  or application-managed native callback patches are added.
- Static wrapper generation delegates to the current PuerTS generator. The
  unused copied PuerTS 2 wrapper generator and its helper are removed.
- Backend menus select installed PuerTS 3 backends rather than invoking the
  old downloader, which installed incompatible PuerTS 2 binaries.

## Local runtime compatibility patches

Static C# members named `length` collide with the native V8 constructor
template's built-in `Function.length` property and terminate the Editor.
This affects `Unity.Mathematics.math`, loaded by Studio's Assets panel. A
minimal class containing only a static `length` method reproduces the crash.
P/Invoke type registration now exports those static members under an internal
alias. The JavaScript bootstrap restores the original property descriptor
after constructor creation, preserving the public API, overloads and accessor
behavior. Instance members and the normal constructor length stay unchanged.

Automatic extension-method binding previously substituted a generic parameter's
base type even when it also required an interface. Unity 7's
`ProvideContext<T>` rejects the resulting `System.Object` argument and prevented
VisualElement binding. Extensions that fail generic constraint validation are
skipped during automatic registration; explicit concrete generic bindings
remain available through PuerTS.

Studio maintains native execution, GC, event subscription, JavaScript exception,
TypeScript generation, static length and Unity UI type regressions in
`Assets/Tests/OneJS`. A separate `StudioSceneStartupCheck.Run` opens and runs the
actual Studio scene to validate the JavaScript panel startup path. Compilation
and EditMode tests alone do not exercise that path.
