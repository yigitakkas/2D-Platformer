namespace GlobalTypes
{
    public enum GroundType
    {
        None,
        LevelGeom,
        OneWayPlatform,
        MovingPlatform,
        CollapsablePlatform,
        JumpPad,
        Spike
    }

    public enum WallType
    {
        None,
        Normal,
        Sticky
    }

    public enum AirEffectorType
    {
        None,
        Ladder,
        Updraft,
        TractorBeam
    }

    public enum ControllerMoveType
    {
        physicsBased,
        nonPhysicsBased,
        none
    }
}