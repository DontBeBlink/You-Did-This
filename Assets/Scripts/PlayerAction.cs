using UnityEngine;

[System.Serializable]
public class PlayerAction
{
    public enum ActionType
    {
        Move,
        Jump,
        CreateClone,
        Retract
    }
    
    public ActionType actionType;
    public Vector2 moveDirection;
    public float timestamp;
    public bool isPressed; // For jump/button actions
    
    public PlayerAction(ActionType type, Vector2 direction, float time, bool pressed = false)
    {
        actionType = type;
        moveDirection = direction;
        timestamp = time;
        isPressed = pressed;
    }
}