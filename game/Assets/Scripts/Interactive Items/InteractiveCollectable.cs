using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
// Item that you can pick and store.
public class InteractiveCollectable : InteractiveItem
{
	// Inspector Assigned
	// NOTE the name text must be ONLY the item name (e.g. lockpick)
	[SerializeField] private bool _isTreasure;
	[SerializeField] private bool _isPickable;
	[SerializeField] private List<Role> _pickableBy = new List<Role>();
	[SerializeField] private int _inventoryPriority;
	[SerializeField] private CollectableName _name;
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
	// interface that activates when you pick up the collectable.
	[SerializeField] private GameObject _interface = null;

	// Internals
	private IEnumerator _coroutine = null;
	private float _hideActivatedTextTime = 0.0f;
	private Collectable _thisCollectable = new Collectable();
	private CharacterManager _charManager = null;
	private Rigidbody _rb = null;
	// stores if this object has been collected by someone or not
	private bool _isPicked = false;

	// Properties
	public bool isPickable { set { _isPickable = value; } }
	public bool isPicked
	{
		get { return _isPicked; }
		set { _isPicked = value; }
	}
	public CollectableName Collectable { get { return _name; } }
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

	public override string GetText(CharacterManager cm)
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
			PlayCollectableSound(bank);
		}
		// if it is, also store it
		else if (_pickableBy.Contains(characterManager.Role))
		{
			if (!characterManager.StoreCollectable(_thisCollectable)) // inventory full
			{
				// you can play "inventory full" fail sounds here
				return;
			}

			PlayCollectableSound(bank);

			// cache the manager
			_charManager = characterManager;

			// the collectable disappears from the scene
			gameObject.SetActive(false);

			_isPicked = true;
		}
		else // pickable but not by you
		{
			PlayerHUD hud = characterManager.PlayerHUD;
			StartCoroutine(hud.SetEventText("You cannot handle this item.", hud.eventColors[0]));
		}

		if (_interface != null)
		{
			if (_interface.activeInHierarchy)
			{
				_interface.SetActive(false);
			}
			else
			{
				_interface.SetActive(true);
			}
		}
	}

	private void PlayCollectableSound(int bank)
	{
		if (bank > -1)
		{
			_coroutine = PlayCollectableSoundCoroutine(bank);
			StartCoroutine(_coroutine);
		}
	}

	// Use the first bank for fail sounds and the second one for success sounds
	private IEnumerator PlayCollectableSoundCoroutine(int bank)
	{
		if (AudioManager.Instance == null)
			yield break;

		//int bank = isPickable ? 1 : 0;
		AudioClip clip = _audioCollection[bank];
		if (clip == null)
			yield break;

		AudioManager.Instance.PlayOneShotSound(_audioCollection.MixerGroupName, clip.name, transform.position,
												_audioCollection.Volume, _audioCollection.SpatialBlend,
												_audioCollection.Priority);

		yield return new WaitForSeconds(clip.length);

		_coroutine = null;
	}
}
