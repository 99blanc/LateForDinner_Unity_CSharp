using Cysharp.Threading.Tasks;
using Token.ID;
using UnityEngine;

public class GameManager
{
    public async UniTask Init()
    {
        GameObject gameObject = await Managers.Resource.Instantiate(Define.Asset.PREFAB_PLAYER);
        Player player = gameObject.GetComponentAssert<Player>();

        if (Managers.Data.players.TryGetValue(PlayerID.DEFAULT, out var data))
            player.Init(data);
    }
}
