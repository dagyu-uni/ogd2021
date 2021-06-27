using UnityEngine;
using System.Collections;

public class BackgroundMusicTrigger : MonoBehaviour
{
	[SerializeField] private AudioCollection _audioCollection = null;

	private void OnTriggerExit(Collider other)
	{
		Debug.Log("Play Music");
	}


	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Stop Music");
	}
}
