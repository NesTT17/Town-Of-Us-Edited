using UnityEngine;

namespace TownOfUs.Modules.Debugger;

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class Keyboard_Joystick
{
    public static int ControllingFigure;
    public static void Postfix()
    {
        if (!TownOfUsPlugin.DebuggerLoaded) return;

        if (Input.GetKeyDown(KeyCode.F5))
        {
            CreatePlayer();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            Switch(true);
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            Switch(false);
        }
        else if (Input.GetKeyDown(KeyCode.F6))
            TownOfUsPlugin.Persistence = !TownOfUsPlugin.Persistence;

        if (Input.GetKeyDown(KeyCode.F11))
            InstanceControlPatches.RemoveAllPlayers();
    }

    internal static void CreatePlayer()
    {
        ControllingFigure = PlayerControl.LocalPlayer.PlayerId;

        if (PlayerControl.AllPlayerControls.Count == 15 && !Input.GetKeyDown(KeyCode.LeftShift)) return; 
        //press f6 and f5 to bypass limit

        InstanceControlPatches.CleanUpLoad();
        InstanceControlPatches.CreatePlayerInstance();
    }

    internal static void Switch(bool increment)
    {
        if (LobbyBehaviour.Instance) return;

        Cycle(increment);
        InstanceControlPatches.SwitchTo((byte)ControllingFigure);
    }

    private static void Cycle(bool increment)
    {
        if (increment)
            ControllingFigure++;
        else
            ControllingFigure--;

        if (ControllingFigure < 0)
            ControllingFigure = InstanceControlPatches.Clients.Count - 1;
        else if (ControllingFigure > InstanceControlPatches.Clients.Count)
            ControllingFigure = 0;
    }
}
