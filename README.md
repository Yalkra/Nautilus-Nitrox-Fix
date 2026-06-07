# Nautilus / Nitrox Compatibility Fix

## Why this exists

This patch was created because my partner and I wanted to play Subnautica Nitrox multiplayer while continuing to use newer Nautilus releases.

When using Nitrox 1.8.1.0 with newer Nautilus versions, our game would become stuck on the epilepsy warning screen during startup. After investigating the issue, I traced the problem to changes introduced in Nautilus' input handling system, from 1.0.0-pre.45+, and created a compatibility fix.

The Nautilus team has stated that Nitrox compatibility is not currently a development priority. This is completely understandable, as Nitrox is a separate project with different goals and requirements. However, it means that players wishing to use both projects together currently have limited options.

This repository exists to provide a community fix for that specific compatibility issue.

---

## Symptoms this patch may fix

If you are using Nitrox and newer versions of Nautilus, you may experience:

* The game becoming stuck on the epilepsy warning screen.
* Failure to reach the main menu.
* Nitrox startup failures.
* Errors relating to `GameInputPatcher` or custom key bindings.

If those symptoms sound familiar, this patch may help.

---

## What was changed?

Only **one file** was modified from the original Nautilus source code:

```text
GameInputPatcher.cs
```

No other Nautilus source files were changed.

The modification imports externally registered GameInput buttons before Nautilus enables its input action map, restoring compatibility with Nitrox's custom input registration system.

---

## Important: Nitrox and Mods

Nitrox was not designed to support arbitrary gameplay mods.

While this patch allows Nitrox and newer Nautilus versions to start correctly together, it does **not** guarantee that other mods will function correctly in multiplayer.

There is currently no universal server-side mod support system in Nitrox. As a result, many mods may:

* Desynchronise players.
* Cause unexpected behaviour.
* Break progression.
* Affect physics.
* Work for one player but not another.

For example, during testing we found that the TODO List mod caused players to sink into the ground and stop consuming oxygen.

If you experience strange gameplay issues while using Nitrox, always test with fewer mods before reporting a Nitrox problem.

---

## Tested Compatibility

Tested successfully with:

```text
Subnautica         1.22.83031
Nitrox             1.8.1.0
Nautilus           1.0.0.pre50
```

The original compatibility issue was reproduced with newer Nautilus releases and was not present in older versions such as 1.0.0.43. If you want to update older versions I've made the GameInputPatcher.cs code available to use freely.

---

## Installation

1. Back up your original Nautilus.dll.
2. Replace:

```text
Subnautica\BepInEx\plugins\Nautilus\Nautilus.dll
```

with the version provided in this repository.
3. Launch the game normally through Nitrox.

---

## Disclaimer

I am not affiliated with the Nautilus team, Nitrox team, Unknown Worlds, or any other modding project.

This patch was created solely so my partner and I could continue playing Nitrox multiplayer while using newer Nautilus versions.

The source code modifications are included so that anyone can review exactly what was changed before using the compiled DLL.

If an official Nautilus update resolves this compatibility issue in the future, the official solution should be preferred.

---

## Credits

* The Nautilus developers for creating and maintaining Nautilus.
* The Nitrox developers for creating Nitrox multiplayer.
* Community members who tested and confirmed the compatibility fix.
