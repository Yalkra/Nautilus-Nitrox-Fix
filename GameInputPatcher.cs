#if SUBNAUTICA
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine.InputSystem;
using Hotkey = (GameInput.Button button, GameInput.Device device);

namespace Nautilus.Patchers;

[HarmonyPatch(typeof(GameInputSystem))]
internal static class GameInputPatcher
{
    public record InputBinding(GameInput.Device Device, GameInput.BindingSet BindingSet, string Path);
    
    public static Dictionary<GameInput.Button, InputAction> CustomButtons = new();
    public static Dictionary<GameInput.Button, List<InputBinding>> Bindings = new();
    public static Dictionary<GameInput.Button, string> Categories = new();
    public static List<InputActionMap> CustomActionMaps = new();
    public static HashSet<(GameInput.Button button, GameInput.Device device)> BindableButtons = new();
    public static HashSet<(GameInput.Button button, GameInput.Device device)> ConflictEvaders = new();

    public static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(GameInputPatcher));
        InternalLogger.Info("Patched GameInputPatcher.");
    }
    
    public static void RegisterActions(InputActionAsset actionAsset)
    {
        CustomActionMaps.AddRange(actionAsset.actionMaps);
    }

    // Some mods, such as Nitrox 1.8.1.0, register custom GameInput.Buttons
    // directly into GameInput.ActionNames and GameInputSystem binding definitions.
    // Those buttons need InputActions before actionMapGameplay.Enable(), otherwise
    // other transpilers may fail when accessing GameInputSystem.actions[button].

    [HarmonyPatch(nameof(GameInputSystem.Initialize))]
    [HarmonyTranspiler]
    [HarmonyPriority(Priority.First)]
    private static IEnumerable<CodeInstruction> InitializeTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new[]
            {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(InputActionMap), nameof(InputActionMap.Enable)))
            })
            .Insert(new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GameInputPatcher), nameof(ImportExternallyRegisteredButtons)))
            })
            .InstructionEnumeration();
    }

    private static void ImportExternallyRegisteredButtons(GameInputSystem __instance)
    {
        foreach (var kvp in GameInput.ActionNames.valueToString.ToArray())
        {
            var button = kvp.Key;

            if (__instance.actions.ContainsKey(button))
            {
                continue;
            }

            if (CustomButtons.ContainsKey(button))
            {
                continue;
            }

            if (GameInput.AllActions.Contains(button))
            {
                continue;
            }

            string actionName = button.ToString();
            string displayName = kvp.Value;

            var action = new InputAction(actionName, InputActionType.Button);

            foreach (var bindingSet in GameInput.AllBindingSets)
            {
                if (TryGetBindingPath(button, GameInput.Device.Keyboard, bindingSet, out var keyboardPath))
                {
                    action.AddBinding(
                        keyboardPath,
                        groups: __instance.GetCompositeGroup(GameInput.Device.Keyboard, bindingSet)
                    );
                }

                if (TryGetBindingPath(button, GameInput.Device.Controller, bindingSet, out var controllerPath))
                {
                    action.AddBinding(
                        controllerPath,
                        groups: __instance.GetCompositeGroup(GameInput.Device.Controller, bindingSet)
                    );
                }
            }

            __instance.actions[button] = action;
            action.started += __instance.OnActionStarted;
            action.Enable();

            InternalLogger.Info($"Imported externally registered GameInput button before enable: {button} / {displayName}");
        }
    }

    [HarmonyPatch(nameof(GameInputSystem.Initialize))]
    [HarmonyPostfix]
    private static void InitializePostfix(GameInputSystem __instance)
    {
        foreach (var kvp in CustomButtons)
        {
            var button = kvp.Key;
            var action = kvp.Value;
            __instance.actions[button] = action;
            action.started += __instance.OnActionStarted;
            action.Enable();

            if (Bindings.TryGetValue(button, out var bindings))
            {
                foreach (var bindableButton in BindableButtons)
                {
                    foreach (var bindingSet in GameInput.AllBindingSets)
                    {
                        var index = bindings.FindIndex(x => x.Device == bindableButton.device && x.BindingSet == bindingSet);
                        if (index == -1)
                        {
                            bindings.Add(new(bindableButton.device, bindingSet, string.Empty));
                        }
                    }
                }

                foreach (var binding in bindings)
                {
                    action.AddBinding(binding.Path, groups: __instance.GetCompositeGroup(binding.Device, binding.BindingSet));
                }
            }
        }

        InternalLogger.Debug("Added all custom buttons.");
    }

    private static bool TryGetBindingPath(GameInput.Button button, GameInput.Device device, GameInput.BindingSet bindingSet, out string path)
    {
        path = string.Empty;

        try
        {
            path = device == GameInput.Device.Keyboard
                ? GameInputSystem.bindingsKeyboard[(button, bindingSet)]
                : GameInputSystem.bindingsController[(button, bindingSet)];

            return !string.IsNullOrWhiteSpace(path);
        }
        catch
        {
            return false;
        }
    }

    [HarmonyPatch(nameof(GameInputSystem.Deinitialize))]
    [HarmonyPostfix]
    private static void DeinitializePostfix(GameInputSystem __instance)
    {
        foreach (var customButton in CustomButtons)
        {
            customButton.Value.started -= __instance.OnActionStarted;
            customButton.Value.Disable();
        }
    }

    [HarmonyPatch(typeof(GameInput))]
    [HarmonyPatch(nameof(GameInput.IsBindable))]
    [HarmonyPostfix]
    private static void IsBindablePostfix(GameInput.Device device, GameInput.Button action, ref bool __result)
    {
        if (CustomButtons.ContainsKey(action) && BindableButtons.Contains((action, device)))
        {
            __result = true;
        }
    }
    
    [HarmonyPatch(nameof(GameInputSystem.SerializeSettings))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SerializeSettingsTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(GameInput), nameof(GameInput.AllActions))))
            .Advance(1)
            .Insert(Transpilers.EmitDelegate(IncludeCustomButtons));
        
        return matcher.InstructionEnumeration();
        
        GameInput.Button[] IncludeCustomButtons(GameInput.Button[] buttons)
        {
            var result = buttons.ToHashSet();
            result.AddRange(BindableButtons.Select(b => b.button));
            return result.ToArray();
        }
    }
    
    [HarmonyPatch(typeof(GameInput))]
    [HarmonyPatch(nameof(GameInput.PrimaryDevice), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool PrimaryDevicePrefix(ref GameInput.Device __result)
    {
        if (GameInput.input is not null)
        {
            return true;
        }
        __result = GameInput.Device.Keyboard;
        return false;

    }

    [HarmonyPatch(typeof(BindConflicts))]
    [HarmonyPatch(nameof(BindConflicts.GetConflicts))]
    [HarmonyPrefix]
    private static bool GetConflictsPrefix(GameInput.Device device, string input, GameInput.Button button,
        List<GameInput.Button> conflicts)
    {
        if (conflicts is null)
        {
            return true;
        }
        
        if (ConflictEvaders.Contains((button, device)))
        {
            conflicts.Clear();
            return false;
        }

        return true;
    }
}
#endif
