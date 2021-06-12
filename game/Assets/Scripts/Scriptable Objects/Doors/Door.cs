using UnityEngine;

public abstract class Door : ScriptableObject
{
	public abstract void Interaction(InteractiveDoor interactiveDoor, CharacterManager characterManager);
}
