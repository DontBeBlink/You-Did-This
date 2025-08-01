using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides logic gates that act as a trigger and take two other triggers as inputs, or only one input if
/// the gate has the Not type
/// </summary>
public class LogicGateTrigger : TriggerObject {

    [Tooltip("The type of this logic gate")]
    public LogicGateType type;
    [Tooltip("The first input trigger")]
    public TriggerObject inputA;
    [Tooltip("The second input trigger")]
    public TriggerObject inputB;
    [Tooltip("The second input trigger")]
    public TriggerObject inputC;

    private bool latched = false;

    /// <summary>
    /// Returns the result of running the inputs through the logic gate
    /// </summary>
    /// <value>The result of the logic gate</value>
    public override bool Active
    {
        get
        {
            bool result = false;
            if (!inputA || (!inputB && type != LogicGateType.Not))
                result = false;
            else
            {
                switch (type)
                {
                    case LogicGateType.Is:
                        result = inputA.Active;
                        break;
                    case LogicGateType.Not:
                        result = !inputA.Active;
                        break;
                    case LogicGateType.And:
                        result = inputA.Active && inputB.Active;
                        break;
                    case LogicGateType.Or:
                        result = inputA.Active || inputB.Active;
                        break;
                    case LogicGateType.Xor:
                        result = inputA.Active != inputB.Active;
                        break;
                    case LogicGateType.Nand:
                        result = !(inputA.Active && inputB.Active);
                        break;
                    case LogicGateType.Nor:
                        result = !(inputA.Active || inputB.Active);
                        break;
                    case LogicGateType.And3:
                        result = inputA.Active && inputB.Active && inputC.Active;
                        break;
                    case LogicGateType.Or3:
                        result = inputA.Active || inputB.Active || inputC.Active;
                        break;
                    case LogicGateType.Xor3:
                        result = (inputA.Active && !inputB.Active && !inputC.Active) || (!inputA.Active && inputB.Active && !inputC.Active) || (!inputA.Active && !inputB.Active && inputC.Active);
                        break;
                    default:
                        result = false;
                        break;
                }
            }

            if (oneShot)
            {
                if (result)
                    latched = true;
                return latched;
            }
            else
            {
                return result;
            }
        }
    }

    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos() {
        Gizmos.color = Active? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    /// <summary>
    /// Types of logic gates
    /// </summary>
    public enum LogicGateType { 
        Is,
        Not,
        And,
        Or,
        Xor,
        Nand,
        Nor,
        And3,
        Or3,
        Xor3
    }
}