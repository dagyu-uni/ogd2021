using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MelodyPuzzleManager : PuzzleManager, Randomizer
{
	[SerializeField] private AudioCollection audioCollection = null;
	[SerializeField] private int audioClipIndex = 0;
	[SerializeField] private int secondsForPuzzleReset = 60;

	private List<AudioClip> clips = null;
	private bool _shuffled = false;
	private bool _playing = false;


	private int currentSolvedIndex = 0;
	private int currentIndex = 0;
	private DateTime lastTime;

	public AudioCollection AudioCollection { get { return audioCollection; } }
	public List<AudioClip> ShuffledAudioClips { get { return clips; } }
	public List<AudioClip> NotShuffledAudioClips { get { return audioCollection.ClipBanks[audioClipIndex].clips; } }

	public void Play(CharacterManager characterManager, Vector3 position, int indexAudioClip)
	{
		if (!_playing)
		{
			DateTime now = DateTime.Now;
			if (lastTime != null && (now - lastTime) > new TimeSpan(0, 0, secondsForPuzzleReset))
			{
				ResetPuzzle();
			}
			lastTime = now;
			characterManager.CallCoroutine(PlaySoundAndCheckPuzzle(NotShuffledAudioClips[indexAudioClip], position));
		}
	}

	private void ResetPuzzle()
	{
		currentIndex = 0;
		currentSolvedIndex = 0;
	}

	private bool IsPlayedInstrumentRight(AudioClip audioClip)
	{
		return clips[currentIndex].Equals(audioClip);
	}

	public override bool IsPuzzleSolved()
	{
		return currentSolvedIndex == clips.Count;
	}

	public override void ActivateReward()
	{
		base.ActivateReward();
		// TODO do something
		Debug.Log("Melody Puzzle Solved!");
	}

	private IEnumerator PlaySoundAndCheckPuzzle(AudioClip audioClip, Vector3 position)
	{
		_playing = true;
		AudioManager.Instance.PlayOneShotSound(
			audioCollection.MixerGroupName,
			audioClip.name,
			position,
			audioCollection.Volume,
			audioCollection.SpatialBlend,
			audioCollection.Priority
		);
		yield return new WaitForSeconds(audioClip.length);
		_playing = false;
		if (currentIndex >= clips.Count)
		{
			ResetPuzzle();
		}

		if (currentIndex == currentSolvedIndex && IsPlayedInstrumentRight(audioClip))
		{
			currentSolvedIndex += 1;
		}
		currentIndex += 1;
	}

	public void InitRandom()
	{
		if (!_shuffled)
		{
			clips = new List<AudioClip>(NotShuffledAudioClips);
			GameManager.Instance.Shuffle(clips);
		}
	}
}
