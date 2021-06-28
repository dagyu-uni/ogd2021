using UnityEngine;

[CreateAssetMenu(menuName = "Door/Final Door")]
public class FinalDoor : Door
{

	public bool openable = false;

	public override void Interaction(InteractiveDoor interactiveDoor, CharacterManager cm)
	{
		string text = "";
		if (cm.IsKing())
		{
			text = "You can't pass!";
		}
		else if (!openable)
		{
			text = "Some sort of dark magic is protecting this door.";
		}
		else if (cm.IsWizard())
		{
			if (interactiveDoor.isLocked && cm.HasCollectable(CollectableName.LockPick))
			{
				interactiveDoor.StartPuzzle(cm);
			}
			else if (interactiveDoor.isLocked)
			{
				text = "You need a lockpick to open the door";
			}

		}
		if (text != null)
			cm.CallCoroutine(cm.PlayerHUD.SetEventText(text, cm.PlayerHUD.eventColors[0]));
	}
}
