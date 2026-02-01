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
    float gcDistance { get; }
    float threshold { get; }
}

public interface IPhysicsData : IData
{
    float gvMul { get; }
    float gvReduction { get; }
    float decelLadder { get; }
}

public interface IDashData : IData
{
    short dashCount { get; }
    float dashCooltime { get; }
    float dashDistance { get; }
    float dashSpeed { get; }
}

public interface ILadderData : IData
{
    float moveSpeed { get; }
    float decelLadder { get; }
    float gcDistance { get; }
}