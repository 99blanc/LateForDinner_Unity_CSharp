public interface IData { }

public interface IData<TKey> : IData
{
    TKey id { get; }
}

public interface IMoveData : IData
{
    float moveSpeed { get; }
    float acceleration { get; }
    float deceleration { get; }
    float turnVel { get; }
}

public interface IJumpData : IData
{
    float jumpForce { get; }
    short jumpCount { get; }
}

public interface IFallData : IData
{
    float decelObj { get; }
    float gvMul { get; }
    float gvReduction { get; }
    float gcDistance { get; }
}

public interface IDashData : IData
{
    short dashCount { get; }
    float dashCooltime { get; }
    float dashDistance { get; }
    float dashSpeed { get; }
}

public interface IClimbData : IData
{
    float moveSpeed { get; }
    float decelObj { get; }
    float gcDistance { get; }
}

public interface ISneakData : IData 
{
    float threshold { get; }
}