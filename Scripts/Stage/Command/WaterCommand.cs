using UnityEngine;

public class WaterCommand : Command
{
    private Player player;
    private Farmland targetFarmland;
    private PlayerAnimation playerAnimation;

    private const Farmland.FarmlandState previousState = Farmland.FarmlandState.Plowed;

    public WaterCommand(Player p, Farmland farmland/*, Vector2 direction*/)
    {
        player = p;
        targetFarmland = farmland;
        playerAnimation = p.GetComponent<PlayerAnimation>();
    }

    public override void Execute()
    {
        if (targetFarmland == null || playerAnimation == null) return;

        if (targetFarmland.CurrentState != Farmland.FarmlandState.Plowed) return;

        playerAnimation.PlayAction(ActionType.Watering);

        targetFarmland.Water();
    }

    public override void Undo()
    {
        if (targetFarmland == null) return;
        targetFarmland.RestoreState(previousState);
    }
}