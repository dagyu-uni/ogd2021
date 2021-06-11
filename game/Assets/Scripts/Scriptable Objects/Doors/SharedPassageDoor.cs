using UnityEngine;

[CreateAssetMenu(menuName = "Door/Shared Passage Door")]
public class SharedPassageDoor : Door
{
	public override void Interaction(InteractiveDoor interactiveDoor, CharacterManager cm)
	{
		string text = null;
		if (cm.IsKing())
		{
			text = "Find a lever to open the door";
		}
		else if (cm.IsWizard() && cm.HasCollectable(CollectableName.Passpartout))
		{
			text =  "You can open the door using a lever, find it.";
		}
		// wizards
		else if (cm.IsWizard() && cm.HasCollectable(CollectableName.LockPick))
		{
			interactiveDoor.StartPuzzle(cm);
		}
		else if (cm.IsWizard())
		{
			text = "You need a lockpick to open the door";
		}
		if (text != null)
			cm.CallCoroutine(cm.PlayerHUD.SetEventText(text, cm.PlayerHUD.eventColors[0]));
	}
}
