using System;
using Trove.UtilityAI;
using Unity.Entities;

[Serializable]
public struct HeroAI : IComponentData
{
    // Store references to our action instances
    public ActionReference IdleRef;
    public ActionReference ReturnToSquireRef;

    // Store references to our consideration instances
    public ConsiderationReference IdlingComfortRef;
    public ConsiderationReference DistanceFromSquireRef;
}