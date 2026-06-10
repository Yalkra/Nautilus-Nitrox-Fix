
## Nautilus / Nitrox Compatibility Fix

**Fixes Subnautica startup freeze at epilepsy warning screen when using Nitrox with Nautilus 1.0.0.50**

- Related issue: https://github.com/SubnauticaModding/Nautilus/issues/644

---

## Important: Nitrox and Mods

Nitrox was not designed to support arbitrary gameplay mods.

While this patch allows Nitrox and newer Nautilus versions to start correctly together, it does **not** guarantee that other mods will function correctly in multiplayer.

There is currently no universal server-side mod support system in Nitrox. As a result, many mods may cause unexpected behaviours.

- Discussion / comments: https://github.com/Yalkra/Nautilus-Nitrox-Fix/issues/1
- Mods Tested: https://github.com/Yalkra/Nautilus-Nitrox-Fix/issues/2

---

## Download

- Release page: https://github.com/Yalkra/Nautilus-Nitrox-Fix/releases/tag/v1.0.0
- Direct download: https://raw.githubusercontent.com/Yalkra/Nautilus-Nitrox-Fix/main/Nautilus1.0.0-pre.50_Nitrox1.8.1.0_FIX.zip

---

## About this patch

This repository contains a compatibility fix for Subnautica players who want to use Nitrox 1.8.1.0 multiplayer mod alongside newer Nautilus releases (1.0.0.45 and later).

**The Problem:** Subnautica hangs on the epilepsy warning screen when launching through Nitrox with Nautilus 1.0.0.45+. This is caused by changes to Nautilus' input system that conflict with Nitrox's custom input registration.

**The Solution:** This patch modifies Nautilus' `GameInputPatcher.cs` to restore compatibility by ensuring externally registered GameInput buttons are imported before Nautilus enables its input action map.

This is a community patch, not an official Nautilus or Nitrox update. Nitrox compatibility is not currently a development priority for the Nautilus team, so this repository is meant to help other players experiencing the same startup freeze issue.

---

## What was changed?

Only **one file** was modified from the original Nautilus source code:

```text
GameInputPatcher.cs
```

No other Nautilus source files were changed.

The modification imports externally registered GameInput buttons before Nautilus enables its input action map, restoring compatibility with Nitrox's custom input registration system.

---

## Tested Compatibility

Tested successfully with:

```text
Subnautica         1.22.83031
Nitrox             1.8.1.0
Nautilus           1.0.0-pre.50
```

The original compatibility issue was reproduced with Nautilus 1.0.0.45+ and was not present in older versions such as 1.0.0.43. If you want to update older versions, the source code is provided for reference.

---

## Installation

1. Back up your original Nautilus.dll
2. Replace:

```text
Subnautica\BepInEx\plugins\Nautilus\Nautilus.dll
```

with the patched version provided in this repository.

3. Launch Subnautica normally through Nitrox

---

## Disclaimer

I am not affiliated with the Nautilus team, Nitrox team, Unknown Worlds, or any other modding project.

This patch was created solely so my partner and I could continue playing Nitrox multiplayer while using newer Nautilus versions.

The source code modifications are included so that anyone can review exactly what was changed before using the compiled DLL.

If an official Nautilus update resolves this compatibility issue in the future, the official solution should be preferred.

---

## Credits

* The Nautilus developers for creating and maintaining Nautilus
* The Nitrox developers for creating Nitrox multiplayer
* Community members who tested and confirmed the compatibility fix
