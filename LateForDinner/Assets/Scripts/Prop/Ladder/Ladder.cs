using Token.PRIORITY;
using UnityEngine;

public class Ladder : Prop, ILadderProp
{
    public override PropPriority priority => PropPriority.LADDER;
    public float centerX => cCollider.bounds.center.x;
    public Bounds bounds => cCollider.bounds;
}
