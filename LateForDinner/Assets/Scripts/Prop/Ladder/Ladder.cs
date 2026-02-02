using Token.PRIORITY;
using UnityEngine;

public class Ladder : TriggerProp, IClimbProp
{
    public float centerX => sensor.bounds.center.x;
    public Bounds bounds => sensor.bounds;
    public override PropPriority priority => PropPriority.LADDER;
}
