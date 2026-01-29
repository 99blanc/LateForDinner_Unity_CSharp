using UnityEngine;
using Token.ID;
using Token.DATA;

public class Player : Character<Component, IPlayerView, PlayerData, PlayerID>
{
    public void InitBySelection(PlayerID id)
    {
        if (!Managers.Data.players.TryGetValue(id, out var data))
            return;

        Init(data);
    }

    public override void Init(PlayerData data) => base.Init(data);

    protected override void Components() => gameObject.GetOrAddComponentAssert<PlayerControl>();

    protected override void ApplyRegistry(PlayerData data)
    {
        registry.Set(StatType.CURRENT_HEALTH, data.maxHealth);
        registry.Set(StatType.MAX_HEALTH, data.maxHealth);
        registry.Set(StatType.CURRENT_TEMP_HEALTH, data.maxTempHealth);
        registry.Set(StatType.MAX_TEMP_HEALTH, data.maxTempHealth);
        registry.Set(StatType.MOVE_SPEED, data.moveSpeed);
        registry.Set(StatType.DAMAGE, data.damage);
        registry.Set(StatType.ATK_SPEED, data.atkSpeed);
        registry.Set(StatType.DASH_COUNT, data.dashCount);
        registry.Set(StatType.DASH_COOLTIME, data.dashCooltime);
        registry.Set(StatType.DASH_DISTANCE, data.dashDistance);
        registry.Set(StatType.JUMP_COUNT, data.jumpCount);
        registry.Set(StatType.JUMP_FORCE, data.jumpForce);
        registry.Set(StatType.GV_REDUCTION, data.gvReduction);
        registry.Set(StatType.INVUL_DURATION, data.invulDuration);
        registry.Set(StatType.WEAPON_CATEGORY, data.weaponCategory);
    }
}
