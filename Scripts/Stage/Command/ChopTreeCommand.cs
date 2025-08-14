using UnityEngine;


public class ChopTreeCommand : Command
{
    private Player player;
    private InteractableTree targetTree;
    private PlayerAnimation playerAnimation;

    private int previousHitCount;

    private InteractableTree.TreeState previousTreeState;
    private bool gainedSeed;

    public ChopTreeCommand(Player p, InteractableTree tree)
    {
        player = p;
        targetTree = tree;
        playerAnimation = p.GetComponent<PlayerAnimation>();

        previousHitCount = targetTree.HitCount;
        previousTreeState = targetTree.CurrentState;
        gainedSeed = false;
    }

    public override void Execute()
    {
        if (targetTree == null || player == null || playerAnimation == null)
        {
            Debug.LogError("[ChopTreeCommand.Execute] Target Tree, Player, or Animation is null!");
            return;
        }

        if (targetTree.CurrentState == InteractableTree.TreeState.Fallen && previousTreeState == InteractableTree.TreeState.Fallen) 
        {
            return;
        }

        targetTree.Hit();

        if (previousTreeState != InteractableTree.TreeState.Fallen && targetTree.CurrentState == InteractableTree.TreeState.Fallen)
        {
            if (!player.HasSeed)
            {
                player.SetSeedPossession(true);
                gainedSeed = true;
            }
        }
    }

    public override void Undo()
    {
        if (targetTree == null || player == null)
        {
            Debug.LogError("[ChopTreeCommand.Undo] Target Tree or Player is null!");
            return;
        }

        if (gainedSeed)
        {
            player.SetSeedPossession(false);
            gainedSeed = false;
        }


        targetTree.RestoreState(previousTreeState, previousHitCount);
    }
}