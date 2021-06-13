using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Photon.Pun;


// This manager provides an API to allow every game object to reproduce sounds.

// reference a specific audio mixer
public class AudioMixerGroupInfo
{
	public AudioMixerGroup group = null;
	public IEnumerator trackFader = null;
	public string name = string.Empty;
}

// single clip properties
public class AudioPoolItem
{
	public GameObject gameObject = null;
	public Transform transform = null;
	public AudioSource audioSource = null;
	// used if pool is full to know which audiosource
	// use to switch the clip.
	public float unimportance = float.MaxValue;
	public bool isPlaying = false;
	public IEnumerator coroutine = null;
	// used to uniquely identify a single audio clip
	public ulong id = 0;
}

public class AudioManager : MonoBehaviour
{
	// Singleton
	private AudioManager() { }
	private static AudioManager _instance = null;

	public static AudioManager Instance { get { return _instance; } }

	private void Awake()
	{
		// START SINGLETON
		if (_instance == null)
		{
			_instance = FindObjectOfType<AudioManager>();

			if (_instance == null)
			{
				GameObject audioManager = new GameObject("Audio Manager");
				_instance = audioManager.AddComponent<AudioManager>();
			}
		}

		DontDestroyOnLoad(gameObject);
		// END SINGLETON

		if (!_mixer)
			return;

		// Fetch the _mixerGroups dictionary
		AudioMixerGroup[] groups = _mixer.FindMatchingGroups(string.Empty);
		foreach (AudioMixerGroup group in groups)
		{
			AudioMixerGroupInfo groupInfo = new AudioMixerGroupInfo();
			groupInfo.name = group.name;
			groupInfo.group = group;
			groupInfo.trackFader = null;
			_mixerGroups[group.name] = groupInfo;
		}

		// Create the pool
		for (int i = 0; i < _maxSounds; i++)
		{
			// Create gameobject, add audiosource and set the parent object
			GameObject go = new GameObject("AudioPool Item");
			AudioSource audioSource = go.AddComponent<AudioSource>();
			go.transform.parent = transform;

			// Create and configure pool item
			AudioPoolItem poolItem = new AudioPoolItem();
			poolItem.gameObject = go;
			poolItem.audioSource = audioSource;
			poolItem.transform = go.transform;
			poolItem.isPlaying = false;
			poolItem.gameObject.SetActive(false);
			_pool.Add(poolItem);
		}

		_photonView = GetComponent<PhotonView>();
	}

	// INTERNALS
	[SerializeField] private AudioMixer _mixer = null;
	// number of ojbects in the sound pool
	[SerializeField] private int _maxSounds = 10;

	// the string is the name of the mixer group
	private Dictionary<string, AudioMixerGroupInfo> _mixerGroups = new Dictionary<string, AudioMixerGroupInfo>();
	private List<AudioPoolItem> _pool = new List<AudioPoolItem>();
	// allows to access to active objects in the pool directly with the ID
	private Dictionary<ulong, AudioPoolItem> _activePool = new Dictionary<ulong, AudioPoolItem>();
	// the unique id given to sounds (increased by 1 at every call)
	private ulong _idGiver = 0;
	private Transform _listenerPos = null;

	private PhotonView _photonView = null;
	// all the game audio collections used
	public List<AudioCollection> gameAudioCollections = new List<AudioCollection>();
	// audio clips are uniquely identified by their name
	private Dictionary<string, AudioClip> _clipHash = new Dictionary<string, AudioClip>();

	public Transform ListenerPos { set { _listenerPos = value; } }

	private void Start()
	{
		// Initialize the clip dictionary
		for (int i = 0; i < gameAudioCollections.Count; i++)
		{
			AudioCollection collection = gameAudioCollections[i];

			for (int j = 0; j < collection.ClipBanks.Count; j++)
			{
				ClipBank bank = collection.ClipBanks[j];

				for (int k = 0; k < bank.clips.Count; k++)
				{
					string stringCode = bank.clips[k].name;
					_clipHash[stringCode] = bank.clips[k];
				}
			}
		}
	}

	public AudioMixerGroup GetMixerGroupFromName(string name)
	{
		AudioMixerGroupInfo groupInfo;
		if (_mixerGroups.TryGetValue(name, out groupInfo))
		{
			return groupInfo.group;
		}

		return null;
	}

	public float GetMixerGroupVolume(string name)
	{
		AudioMixerGroupInfo groupInfo;
		if (_mixerGroups.TryGetValue(name, out groupInfo))
		{
			float volume;
			_mixer.GetFloat(name + "Volume", out volume);
			return volume;
		}

		return float.MinValue;
	}

	public void SetMixerGroupVolume(string name, float volume, float fadeTime = 0.0f)
	{
		if (!_mixer)
			return;
		AudioMixerGroupInfo groupInfo;
		if (_mixerGroups.TryGetValue(name, out groupInfo))
		{
			// a coroutine is already running
			if (groupInfo.trackFader != null)
				StopCoroutine(groupInfo.trackFader);

			if (fadeTime == 0.0f)
				_mixer.SetFloat(name + "Volume", volume);
			else
			{
				groupInfo.trackFader = SetMixerGroupVolumeInternal(name, volume, fadeTime);
				StartCoroutine(groupInfo.trackFader);
			}
		}
	}

	// Coroutine to fade the volume
	protected IEnumerator SetMixerGroupVolumeInternal(string name, float volume, float fadeTime)
	{
		float timer = 0.0f;
		float startVolume = 0.0f;

		_mixer.GetFloat(name + "Volume", out startVolume);

		while (timer < fadeTime)
		{
			timer += Time.unscaledDeltaTime;
			_mixer.SetFloat(name + "Volume", Mathf.Lerp(startVolume, volume, timer / fadeTime));
			yield return null;
		}

		// this is to avoid floating point imprecisions
		_mixer.SetFloat(name + "Volume", volume);
	}

	// Used internally to configure a pool object
	protected ulong ConfigurePoolObject(int poolIndex, string mixerGroupName, AudioClip clip, Vector3 position, float volume,
										float spatialBlend, float unimportance)
	{
		// if poolIndex is out of range abort request
		if (poolIndex < 0 || poolIndex >= _pool.Count)
			return 0;

		// Get the pool item
		AudioPoolItem poolItem = _pool[poolIndex];

		// Generate new id (used to control if a sound should really be stopped)
		_idGiver++;

		// Configure the audio source
		AudioSource source = poolItem.audioSource;
		source.clip = clip;
		source.volume = volume;
		source.spatialBlend = spatialBlend;
		source.outputAudioMixerGroup = _mixerGroups[mixerGroupName].group;
		source.transform.position = position;

		// Enable Gameobject and record that it's playing
		poolItem.isPlaying = true;
		poolItem.unimportance = unimportance;
		poolItem.id = _idGiver;
		poolItem.gameObject.SetActive(true);
		source.Play();
		poolItem.coroutine = StopSoundDelayed(_idGiver, source.clip.length);
		StartCoroutine(poolItem.coroutine);

		// Add the sound to the active pool
		_activePool[_idGiver] = poolItem;

		return _idGiver;
	}

	// Stop and de-activate the poolItem
	protected IEnumerator StopSoundDelayed(ulong id, float duration)
	{
		yield return new WaitForSeconds(duration);

		AudioPoolItem activeItem;
		if (_activePool.TryGetValue(id, out activeItem))
		{
			activeItem.audioSource.Stop();
			activeItem.audioSource.clip = null;
			activeItem.gameObject.SetActive(false);
			activeItem.isPlaying = false;

			_activePool.Remove(id);

		}
	}

	// Scripts can call this to stop a specific sound directly
	public void StopOneShotSound(ulong id)
	{
		AudioPoolItem activeItem;
		if (_activePool.TryGetValue(id, out activeItem))
		{
			StopCoroutine(activeItem.coroutine);

			activeItem.audioSource.Stop();
			activeItem.audioSource.clip = null;
			activeItem.gameObject.SetActive(false);
			activeItem.isPlaying = false;

			_activePool.Remove(id);

		}
	}



	[PunRPC]
	private ulong PhotonPlayOneShotSound(string mixerGroupName, string clipName, Vector3 position, float volume,
										float spatialBlend, int priority = 128)
	{
		AudioClip clip = _clipHash[clipName];

		if (!_mixerGroups.ContainsKey(mixerGroupName) || clip == null || volume == 0.0f)
			return 0;

		// Calculate the unimportance of the sound
		float unimportance = (_listenerPos.position - position).sqrMagnitude / Mathf.Max(1, priority);

		int leastImportantIndex = -1;
		float leastImportantValue = float.MinValue;

		// Find an available source to use to play the clip
		for (int i = 0; i < _pool.Count; i++)
		{
			AudioPoolItem poolItem = _pool[i];

			if (!poolItem.isPlaying)
				return ConfigurePoolObject(i, mixerGroupName, clip, position, volume, spatialBlend, unimportance);
			// Record the lowest importance source
			else if (poolItem.unimportance > leastImportantValue)
			{
				leastImportantValue = poolItem.unimportance;
				leastImportantIndex = i;
			}
		}

		// No available items, switch with the one with lowest priority
		if (unimportance < leastImportantValue)
			return ConfigurePoolObject(leastImportantIndex, mixerGroupName, clip, position, volume, spatialBlend, unimportance);

		// if the importance of the request is negligible
		return 0;
	}

	// ...or this if the sound has to be delayed
	public IEnumerator PlayOneShotSoundDelayed(string mixerGroupName, AudioClip clip, Vector3 position, float volume,
										float spatialBlend, float duration, int priority = 128)
	{
		yield return new WaitForSeconds(duration);
		PlayOneShotSound(mixerGroupName, clip.name, position, volume, spatialBlend, priority);
	}

	// scripts can call this to play a clip for every client.
	public void PlayOneShotSound(string mixerGroupName, string clipName, Vector3 position, float volume,
										float spatialBlend, int priority = 128)
	{
		if (_photonView.isRuntimeInstantiated)
		{
			_photonView.RPC("PhotonPlayOneShotSound", RpcTarget.All, mixerGroupName, clipName, position, volume, spatialBlend, priority);
		}
		else
		{
			PlayWithoutPhotonOneShotSound(mixerGroupName, clipName, position, volume, spatialBlend, priority);
		}

	}

	public delegate void Callback();

	public IEnumerator PlayClipBanks(string mixerGroupName, List<AudioClip> audioClips, Vector3 position, float volume,
										float spatialBlend, int priority = 128, Callback callback = null)
	{
		foreach (AudioClip clip in audioClips)
		{
			PlayOneShotSound(
				mixerGroupName,
				clip.name,
				position,
				volume,
				spatialBlend,
				priority
			);
			yield return new WaitForSeconds(clip.length);
		}
		if (callback != null)
			callback();
	}


	private ulong PlayWithoutPhotonOneShotSound(string mixerGroupName, string clipName, Vector3 position, float volume,
										float spatialBlend, int priority = 128)
	{
		AudioClip clip = _clipHash[clipName];

		if (!_mixerGroups.ContainsKey(mixerGroupName) || clip == null || volume == 0.0f)
			return 0;

		// Calculate the unimportance of the sound
		float unimportance = 0;

		int leastImportantIndex = -1;
		float leastImportantValue = float.MinValue;

		// Find an available source to use to play the clip
		for (int i = 0; i < _pool.Count; i++)
		{
			AudioPoolItem poolItem = _pool[i];

			if (!poolItem.isPlaying)
				return ConfigurePoolObject(i, mixerGroupName, clip, position, volume, spatialBlend, unimportance);
			// Record the lowest importance source
			else if (poolItem.unimportance > leastImportantValue)
			{
				leastImportantValue = poolItem.unimportance;
				leastImportantIndex = i;
			}
		}

		// No available items, switch with the one with lowest priority
		if (unimportance < leastImportantValue)
			return ConfigurePoolObject(leastImportantIndex, mixerGroupName, clip, position, volume, spatialBlend, unimportance);

		// if the importance of the request is negligible
		return 0;
	}
}
