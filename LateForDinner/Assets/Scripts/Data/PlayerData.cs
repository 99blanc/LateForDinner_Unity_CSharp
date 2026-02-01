using CsvHelper.Configuration.Attributes;
using Token.ID;
using Token.DATA;

public class PlayerData : IData<PlayerID>, IMoveData, IJumpData, IPhysicsData, IDashData, ILadderData
{
    [Name("ID")] public PlayerID id { get; set; }
    [Name("최대 체력")] public short maxHealth { get; set; }
    [Name("최대 임시 체력")] public short maxTempHealth { get; set; }
    [Name("이동 속도")] public float moveSpeed { get; set; }
    [Name("가속")] public float acceleration { get; set; }
    [Name("감속")] public float deceleration { get; set; }
    [Name("역방향 가속")] public float turnVel { get; set; }
    [Name("사다리 감속")] public float decelLadder { get; set; }
    [Name("공격력")] public short damage { get; set; }
    [Name("공격 속도")] public float atkSpeed { get; set; }
    [Name("대시 횟수")] public short dashCount { get; set; }
    [Name("대시 쿨타임")] public float dashCooltime { get; set; }
    [Name("대시 거리")] public float dashDistance { get; set; }
    [Name("대시 속도")] public float dashSpeed { get; set; }
    [Name("점프 횟수")] public short jumpCount { get; set; }
    [Name("점프력")] public float jumpForce { get; set; }
    [Name("중력 보정")] public float gvMul { get; set; }
    [Name("중력 경감")] public float gvReduction { get; set; }
    [Name("지면 인식 거리")] public float gcDistance { get; set; }
    [Name("무적 시간")] public float invulDuration { get; set; }
    [Name("오차 허용 범위")] public float threshold { get; set; }
    [Name("무기 카테고리")] public WeaponCategory weaponCategory { get; set; }
}
