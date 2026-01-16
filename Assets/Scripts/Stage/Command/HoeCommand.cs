using UnityEngine;

public class HoeCommand : Command
{
    private Player player;
    private Farmland targetFarmland;
    private PlayerAnimation playerAnimation;

    private const Farmland.FarmlandState previousState = Farmland.FarmlandState.Normal;

    private bool playerHadSeedBeforeExecution;

    public HoeCommand(Player p, Farmland farmland)
    {
        player = p;
        targetFarmland = farmland;
        playerAnimation = p.GetComponent<PlayerAnimation>();

        playerHadSeedBeforeExecution = player.HasSeed;
    }

    public override void Execute()
    {
        if (targetFarmland == null || playerAnimation == null || player == null) return;

        if (targetFarmland.CurrentState == Farmland.FarmlandState.Normal && player.HasSeed)
        {
            playerAnimation.PlayAction(ActionType.Hoe);
            targetFarmland.Hoe();
            player.SetSeedPossession(false);
        }
        else
        {
            if (targetFarmland.CurrentState != Farmland.FarmlandState.Normal)
                Debug.Log($"[HoeCommand] È£¹ÌÁú¸øÇØ¿ä");
            else if (!player.HasSeed)
                Debug.Log("[HoeCommand] ¾¾¾Ñ¾øÀ½");
            return;
        }
    }

    public override void Undo()
    {
        if (targetFarmland == null || player == null) return;

        if (targetFarmland.CurrentState == Farmland.FarmlandState.Plowed)
        {
            targetFarmland.RestoreState(previousState);

            if (playerHadSeedBeforeExecution)
            {
                player.SetSeedPossession(true);
            }
        }
    }
}