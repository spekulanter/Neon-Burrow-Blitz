using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum PlayerSlot
{
    Player1,
    Player2
}

[System.Serializable]
public class PlayerInputConfig
{
    public PlayerSlot playerSlot;

    public float Move
    {
        get
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return 0f;

            if (playerSlot == PlayerSlot.Player1)
                return ReadAxis(keyboard.aKey, keyboard.dKey);
            return ReadAxis(keyboard.jKey, keyboard.lKey);
        }
    }

    public bool CrouchHeld
    {
        get
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return false;

            if (playerSlot == PlayerSlot.Player1)
                return keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed;
            return keyboard.uKey.isPressed;
        }
    }

    public bool JumpPressed
    {
        get
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return false;

            return playerSlot == PlayerSlot.Player1
                ? keyboard.spaceKey.wasPressedThisFrame
                : keyboard.iKey.wasPressedThisFrame;
        }
    }

    public bool JumpHeld
    {
        get
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return false;

            return playerSlot == PlayerSlot.Player1
                ? keyboard.spaceKey.isPressed
                : keyboard.iKey.isPressed;
        }
    }

    public bool ShootPressed
    {
        get
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null)
                return false;

            if (playerSlot == PlayerSlot.Player1)
                return keyboard.leftCtrlKey.wasPressedThisFrame || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            return keyboard.kKey.wasPressedThisFrame;
        }
    }

    public bool SwitchWeaponPressed
    {
        get
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return false;

            return playerSlot == PlayerSlot.Player1
                ? keyboard.eKey.wasPressedThisFrame
                : keyboard.oKey.wasPressedThisFrame;
        }
    }

    static float ReadAxis(KeyControl left, KeyControl right)
    {
        float value = 0f;
        if (left.isPressed)
            value -= 1f;
        if (right.isPressed)
            value += 1f;
        return value;
    }
}
