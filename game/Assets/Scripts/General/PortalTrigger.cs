using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTrigger : MonoBehaviour
{

	[SerializeField] private List<string> _expectedTags = new List<string>();
	[Tooltip("Use the first bank for enter portal sounds")]
	[SerializeField] private AudioCollection _audioCollection = null;


	private void OnTriggerEnter(Collider other)
	{
		if (_audioCollection == null)
			return;

		if(_expectedTags.Exists(e => other.gameObject.CompareTag(e)))
			AudioManager.Instance.PlayOneShotSound(
				_audioCollection.MixerGroupName,
				_audioCollection[0].name,
				transform.position,
				_audioCollection.Volume,
				_audioCollection.SpatialBlend,
				_audioCollection.Priority
			);
	}
}
