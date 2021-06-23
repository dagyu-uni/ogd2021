using UnityEngine;

[CreateAssetMenu(menuName = "Door/Shared Passage Door")]
public class SharedPassageDoor : Door
{
	public override void Interaction(InteractiveDoor interactiveDoor, CharacterManager cm)
	{
		string text = "";
		if (cm.IsKing())
		{
			text = "Find a lever to "+ (interactiveDoor.isLocked ? "open" : "close") + " the door";
		}
		else if (cm.IsWizard())
		{
			if (cm.HasCollectable(CollectableName.Passpartout))
			{
				text = "You can open or close the door using a lever, find it.";
			}
			else if (interactiveDoor.isLocked && cm.HasCollectable(CollectableName.LockPick))
			{
				interactiveDoor.StartPuzzle(cm);
			}
			else if(interactiveDoor.isLocked)
			{
				text = "You need a lockpick to open the door";
			}
			else if (!interactiveDoor.isLocked)
			{
				text = "You can't close the door";
			}

		}
		if (text != null)
			cm.CallCoroutine(cm.PlayerHUD.SetEventText(text, cm.PlayerHUD.eventColors[0]));
	}
}
