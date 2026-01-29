using R3;
using Token.DATA;

public interface IViewProvider { ReadOnlyReactiveProperty<T> Stream<T>(StatType type) where T : struct; }

public interface IDisplayView : IViewProvider
{
    ReadOnlyReactiveProperty<short> curHealth => Stream<short>(StatType.CURRENT_HEALTH);
    ReadOnlyReactiveProperty<short> maxHealth => Stream<short>(StatType.MAX_HEALTH);
    ReadOnlyReactiveProperty<short> curTempHealth => Stream<short>(StatType.CURRENT_TEMP_HEALTH);
    ReadOnlyReactiveProperty<short> maxTempHealth => Stream<short>(StatType.MAX_TEMP_HEALTH);
    ReadOnlyReactiveProperty<short> dashCount => Stream<short>(StatType.DASH_COUNT);
    ReadOnlyReactiveProperty<float> dashCooltime => Stream<float>(StatType.DASH_COOLTIME);
    ReadOnlyReactiveProperty<short> jumpCount => Stream<short>(StatType.JUMP_COUNT);
    ReadOnlyReactiveProperty<WeaponCategory> weaponCategory => Stream<WeaponCategory>(StatType.WEAPON_CATEGORY);
}

public interface IActionView : IViewProvider
{
    ReadOnlyReactiveProperty<float> moveSpeed => Stream<float>(StatType.MOVE_SPEED);
    ReadOnlyReactiveProperty<short> dashCount => Stream<short>(StatType.DASH_COUNT);
    ReadOnlyReactiveProperty<float> dashCooltime => Stream<float>(StatType.DASH_COOLTIME);
    ReadOnlyReactiveProperty<float> dashDistance => Stream<float>(StatType.DASH_DISTANCE);
    ReadOnlyReactiveProperty<short> jumpCount => Stream<short>(StatType.JUMP_COUNT);
    ReadOnlyReactiveProperty<float> jumpForce => Stream<float>(StatType.JUMP_FORCE);
    ReadOnlyReactiveProperty<float> gvReduction => Stream<float>(StatType.GV_REDUCTION);
}

public interface IAgentView : IActionView { }

public interface ICharacterView : IDisplayView, IAgentView
{
    ReadOnlyReactiveProperty<float> invulDuration => Stream<float>(StatType.INVUL_DURATION);
}

public interface IPlayerView : ICharacterView { }

public interface IVitalView : IViewProvider
{
    ReadOnlyReactiveProperty<short> maxHealth => Stream<short>(StatType.MAX_HEALTH);
    ReadOnlyReactiveProperty<short> maxTempHealth => Stream<short>(StatType.MAX_TEMP_HEALTH);
    ReadOnlyReactiveProperty<float> moveSpeed => Stream<float>(StatType.MOVE_SPEED);
    ReadOnlyReactiveProperty<short> dashCount => Stream<short>(StatType.DASH_COUNT);
    ReadOnlyReactiveProperty<float> dashCooltime => Stream<float>(StatType.DASH_COOLTIME);
    ReadOnlyReactiveProperty<float> dashDistance => Stream<float>(StatType.DASH_DISTANCE);
    ReadOnlyReactiveProperty<short> jumpCount => Stream<short>(StatType.JUMP_COUNT);
    ReadOnlyReactiveProperty<float> jumpForce => Stream<float>(StatType.JUMP_FORCE);
    ReadOnlyReactiveProperty<float> gvReduction => Stream<float>(StatType.GV_REDUCTION);
}

public interface IAttackView : IViewProvider
{
    ReadOnlyReactiveProperty<short> damage => Stream<short>(StatType.DAMAGE);
    ReadOnlyReactiveProperty<float> atkSpeed => Stream<float>(StatType.ATK_SPEED);
    ReadOnlyReactiveProperty<float> atkRange => Stream<float>(StatType.ATK_RANGE);
    ReadOnlyReactiveProperty<float> atkInterval => Stream<float>(StatType.ATK_INTERVAL);
    ReadOnlyReactiveProperty<float> chargeMul => Stream<float>(StatType.CHARGE_MULTIPLE);
    ReadOnlyReactiveProperty<short> pierceCount => Stream<short>(StatType.PIERCE_COUNT);
    ReadOnlyReactiveProperty<WeaponCategory> weaponCategory => Stream<WeaponCategory>(StatType.WEAPON_CATEGORY);
}

public interface IWeaponView : IVitalView, IAttackView { }

public interface IAllView : IPlayerView { }
