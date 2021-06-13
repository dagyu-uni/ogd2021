using UnityEngine;

public class InteractiveInstrument : InteractiveItem
{
	[SerializeField] private string _puzzleNotSolvedText = "Play the note";
	[SerializeField] private string _puzzleSolvedText = "Puzzle already solved";
	[SerializeField] private MelodyPuzzleManager _melodyPuzzleManager = null;
	[SerializeField] private int indexAudioClip;

	public override string GetText(CharacterManager cm)
	{
		return _melodyPuzzleManager.IsPuzzleSolved() ? _puzzleSolvedText : _puzzleNotSolvedText;
	}

	public override void Activate(CharacterManager characterManager)
	{
		if (!_melodyPuzzleManager.IsPuzzleSolved())
		{
			_melodyPuzzleManager.Play(characterManager, transform.position, indexAudioClip);
		}
	}
}
