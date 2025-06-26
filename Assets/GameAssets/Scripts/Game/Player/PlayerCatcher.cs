using UnityEngine;

public class PlayerCatcher : Player
{
    public CharacterCatcher CharacterCatcher => character as CharacterCatcher;
    public override Character.CharacterType Role => Character.CharacterType.Catcher;

    public void UpdateVisibility(bool visible)
    {
        if (IsMine)
        {
            Color color = visible ? Color.white : new Color(1,1,1,0.6f);

            foreach (var sprite in sprites)
            {
                sprite.color = color;
            }
        }
        else
        {
            visual.SetActive(visible);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameController.IsCatcher) return;
        if (other.tag != MasterManager.CustomTagNames.PlayerCamper) return;

        PlayerCamper player = other.GetComponent<PlayerCamper>();
        InteractionCatcherManager.Instance.SetCamper(player);
    }

    protected void OnTriggerExit2D(Collider2D other)
    {
        if (!GameController.IsCatcher) return;
        if (other.tag != MasterManager.CustomTagNames.PlayerCamper) return;

        PlayerCamper player = other.GetComponent<PlayerCamper>();
        InteractionCatcherManager.Instance.RemoveCamper(player);
    }

    public override void LeftRoom()
    {
        GameController.LocalPlayer.Movement.AddBlockReason("CatcherLeft");
        GameController.Instance.ShowEndGameUI("Catcher Left. You won!");
    }
}