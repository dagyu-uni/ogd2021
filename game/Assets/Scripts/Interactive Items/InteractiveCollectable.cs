using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
// Item that you can pick and store.
public class InteractiveCollectable : InteractiveItem
{
	// Inspector Assigned
	// NOTE the name text must be ONLY the item name (e.g. lockpick)
	[SerializeField] private bool _isPickable;
	[SerializeField] private bool _isTreasure;
	[SerializeField] private int _inventoryPriority;
	[SerializeField] private string _name = null;
	[SerializeField] private Sprite _icon = null;
	[TextArea(3, 10)]
	[Tooltip("What is showed by the inventory tooltip of this collectable")]
	[SerializeField] private string _tooltipContent = null;
	// if it's not pickable, give a feedback to the player
	[TextArea(3, 10)]
	[SerializeField] private string _activatedText = null;
	[SerializeField] private float _activatedTextDuration = 3.0f;
	// leave the collection to null if you don't want to play any sound
	[Tooltip("Use the first bank for fail sounds and the second one for success sounds.")]
	[SerializeField] private AudioCollection _audioCollection = null;
	[SerializeField] private PowerUp _powerUp = null;

	// Internals
	private IEnumerator _coroutine = null;
	private float _hideActivatedTextTime = 0.0f;
	private Collectable _thisCollectable = new Collectable();
	private CharacterManager _charManager = null;
	private Rigidbody _rb = null;

	// Properties
	public bool isPickable { set { _isPickable = value; } }
	public CharacterManager CharManager { get { return _charManager; } }

	private void Awake()
	{
		// Get Rigidbody
		_rb = GetComponent<Rigidbody>();
		if (_rb)
			_rb.drag = 0.9f;
	}

	protected override void Start()
	{
		base.Start();

		// Set Collectable
		_thisCollectable.uiPriority = _inventoryPriority;
		_thisCollectable.name = _name;
		_thisCollectable.isTreasure = _isTreasure;
		_thisCollectable.gameObject = this.gameObject;
		_thisCollectable.powerUp = _powerUp;
		_thisCollectable.icon = _icon;
		_thisCollectable.tooltipString = _tooltipContent;
		_thisCollectable.rb = _rb;
	}

	public override string GetText()
	{
		if (Time.time < _hideActivatedTextTime)
			return _activatedText;
		else
			return "Pick " + _name;
	}

	public override void Activate(CharacterManager characterManager)
	{
		int bank = -1;
		// Play audio based on collectable status
		if (_audioCollection != null && _coroutine == null)
		{
			bank = _isPickable ? 1 : 0;
		}

		// if it's not pickable, give a feedback
		if (!_isPickable)
		{
			_hideActivatedTextTime = Time.time + _activatedTextDuration;
			PlayCoroutine(bank);
		}
		// if it is, also store it
		else
		{
			if (!characterManager.StoreCollectable(_thisCollectable)) // inventory full
			{
				// you can play "inventory full" fail sounds here
				return;
			}


			PlayCoroutine(bank);

			// cache the manager
			_charManager = characterManager;

			// the collectable disappears from the scene
			gameObject.SetActive(false);
		}
	}

	private void PlayCoroutine(int bank)
	{
		if (bank > -1)
		{
			_coroutine = PlayCollectableSound(bank);
			StartCoroutine(_coroutine);
		}
	}

	// Use the first bank for fail sounds and the second one for success sounds
	private IEnumerator PlayCollectableSound(int bank)
	{
		if (AudioManager.Instance == null)
			yield break;

		//int bank = isPickable ? 1 : 0;
		AudioClip clip = _audioCollection[bank];
		if (clip == null)
			yield break;

		AudioManager.Instance.PhotonPlayOneShotSound(_audioCollection.MixerGroupName, clip.name, transform.position,
												_audioCollection.Volume, _audioCollection.SpatialBlend,
												_audioCollection.Priority);

		yield return new WaitForSeconds(clip.length);

		_coroutine = null;
	}
}
