using UnityEngine;

public class InteractiveMelody : InteractiveItem
{
	[SerializeField] private string _puzzleNotSolvedText = "Listen the melody";
	[SerializeField] private string _puzzleSolvedText = "Puzzle already solved";
	[SerializeField] private MelodyPuzzleManager _melodyPuzzleManager = null;

	private bool _isPlaying = false;

	public override string GetText(CharacterManager cm)
	{
		return _melodyPuzzleManager.IsPuzzleSolved() ? _puzzleSolvedText : _puzzleNotSolvedText;
	}

	public override void Activate(CharacterManager characterManager)
	{
		if (!_isPlaying && !_melodyPuzzleManager.IsPuzzleSolved())
		{
			_isPlaying = true;

			characterManager.CallCoroutine(
				AudioManager.Instance.PlayClipBanks(
					_melodyPuzzleManager.AudioCollection.MixerGroupName,
					_melodyPuzzleManager.ShuffledAudioClips,
					transform.position,
					_melodyPuzzleManager.AudioCollection.Volume,
					_melodyPuzzleManager.AudioCollection.SpatialBlend,
					_melodyPuzzleManager.AudioCollection.Priority,
					() => { _isPlaying = false; }
				)
			);
		}
	}


}
