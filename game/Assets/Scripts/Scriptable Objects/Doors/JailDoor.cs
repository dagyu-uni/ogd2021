using UnityEngine;

[CreateAssetMenu(menuName = "Door/Jail Door")]

public class JailDoor : Door
{
	public override void Interaction(InteractiveDoor interactiveDoor, CharacterManager cm)
	{
		string text = null;
		if (cm.IsWizard() && cm.HasCollectable(CollectableName.BypassJail))
		{
			//open door
			interactiveDoor.ToggleDoor(cm);
			//loose artifact
			Collectable collectable = cm.SubtractCollectable(CollectableName.BypassJail);
			collectable.gameObject.SetActive(true);
		}
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
