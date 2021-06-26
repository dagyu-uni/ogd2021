using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Works as interface to collect and handle all the relevant data related to a specific puzzle.
// NOTE: this is just a parent class from which inherit specific puzzle implementations.
public abstract class PuzzleManager : InteractiveItem
{
	// Inspector Assigned

	// A puzzle may not be achievable/active at the start
	[SerializeField] protected bool _isPuzzleActive = true;
	[SerializeField] protected bool _deactivateAfterComplete = true;
	// potential collectable related to the puzzle
	[SerializeField] protected InteractiveCollectable _puzzleCollectable = null;
	// potential success audio to play when solved
	[Tooltip("NOTE: if used the audio source will be placed on this transform")]
	[SerializeField] protected AudioCollection _solvedSounds = null;
	// Solving this puzzle may activate another puzzle
	[SerializeField] protected PuzzleManager _nextPuzzle = null;
	// It also may have an interactive interface for the player to use
	// implemented as a canvas
	[SerializeField] protected GameObject _puzzleInterface = null;
	[TextArea(3, 10)]
	[SerializeField] private string _infoText = null;
	[SerializeField] private string _interfaceActiveText = null;

	// Properties
	public bool isPuzzleActive
	{
		get { return _isPuzzleActive; }
		set { _isPuzzleActive = value; }
	}

	public bool isInterfaceActive { get { return _puzzleInterface.gameObject.activeInHierarchy; } }

	// Check if the puzzle is solved and activate/deactivate it based on that.
	private void Update()
	{
		if (!isPuzzleActive)
			return;

		if (_nextPuzzle != null)
			_nextPuzzle.isPuzzleActive = false;

		if (IsPuzzleSolved())
			ActivateReward();
		else
			DeactivateReward();
	}

	private void ActivateNextPuzzle()
	{
		if (_nextPuzzle != null)
			_nextPuzzle.isPuzzleActive = true;
	}

	// Virtuals
	// Check if conditions are met
	public abstract bool IsPuzzleSolved();

	// Apply the benefits/malus achieved by solving the puzzle.
	// Also activate the next puzzle if any (not null)
	public virtual void ActivateReward()
	{
		// Activation Sound
		if (_solvedSounds != null)
			AudioManager.Instance.PlayOneShotSound(
									_solvedSounds.MixerGroupName, _solvedSounds.AudioClip.name,
									transform.position, _solvedSounds.Volume, _solvedSounds.SpatialBlend,
									_solvedSounds.Priority);
		// Next puzzle
		ActivateNextPuzzle();

		// (de)activate the puzzle
		if (_puzzleCollectable == null && _deactivateAfterComplete)
		{
			_isPuzzleActive = false;
		}
		// if there is a collectable and has been collected by a player
		else if (_puzzleCollectable != null && _puzzleCollectable.CharManager != null)
		{
			_isPuzzleActive = !_deactivateAfterComplete;
		}

		//ACTIVATE COLLECTABLE
		if(_puzzleCollectable != null)
		{
			_puzzleCollectable.isPickable = true;
		}

	}

	// optional method to handle situations where you want the puzzle
	// to be solved at a specific time, for instance when trying to collect something 
	// (so it doesn't work to solve it randomly doing many attempts for example)
	public virtual void DeactivateReward() { }


	// Overridden Interactive Methods
	public override string GetText(CharacterManager cm)
	{
		if (!isPuzzleActive)
			return "";

		if (isInterfaceActive)
			return _interfaceActiveText;
		else
			return _infoText;
	}

	// if the puzzle has an interface, activate it and show a different text
	public override void Activate(CharacterManager characterManager)
	{
		if (!isPuzzleActive)
			return;

		OpenCloseInterface(characterManager);
	}

	public void OpenCloseInterface(CharacterManager characterManager)
	{
		if (_puzzleInterface == null)
			return;

		if (_puzzleInterface.gameObject.activeInHierarchy)
		{
			_puzzleInterface.SetActive(false);
			characterManager.EnableControllerMovements();
			characterManager.EnableCameraMovements();
			characterManager.DisableCursor();
		}
		else
		{
			_puzzleInterface.SetActive(true);
			characterManager.DisableControllerMovements();
			characterManager.DisableCameraMovements();
			characterManager.EnableCursor();
		}
	}
}
