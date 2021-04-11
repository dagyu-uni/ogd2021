using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Define the Interactive Item interface
public class InteractiveItem : MonoBehaviour
{
	// Inspector Assigned
	[SerializeField] protected int _rayPriority = 0;

	// Private/Protected
	protected GameManager _gameManager = null;
	protected Collider _collider = null;

	// Properties
	public int RayPriority { get { return _rayPriority; } }

	// Virtual Methods
	public virtual string GetText() { return null; }
	public virtual void Activate(CharacterManager characterManager) { }
	protected virtual void Start()
	{
		_gameManager = GameManager.Instance;
		_collider = GetComponent<Collider>();

		if (_gameManager && _collider)
		{
			_gameManager.RegisterInteractiveItem(_collider.GetInstanceID(), this);
		}
	}
}
