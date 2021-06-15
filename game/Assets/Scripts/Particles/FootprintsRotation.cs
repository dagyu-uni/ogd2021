using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FootprintsRotation : MonoBehaviour
{
	public float timeBetweenSteps = 0.5f;
	public LayerMask layerMask;

	private bool _switchFootprint = true;
	private float _timer = 0.0f;
	private PlayerController _controller = null;

	private PhotonView _photonView = null;

	private void Awake()
	{
		_controller = GetComponent<PlayerController>();
		_photonView = GetComponent<PhotonView>();
		_timer = 0.0f;
	}

	void Update()
	{
		if (_controller.AreFootprintActive)
		{
			_timer += Time.deltaTime;
			float modifier = _controller.status == Status.crouching ? 1.5f : _controller.status == Status.sprinting ? 0.5f :
						_controller.status == Status.walking ? 1.0f : 1000f;
			// if the particle system just played a footprint, switch it (activating the other system)
			if (_timer > timeBetweenSteps * modifier)
			{
				// Cast a ray to get the correct ground height for the footprint
				RaycastHit hit;
				Ray ray = new Ray(transform.position, Vector3.down);
				if (Physics.Raycast(ray, out hit, 2.0f, layerMask))
				{
					_photonView.RPC("AskForFootprint", RpcTarget.All, hit.point.y + 0.01f);
				}


				_switchFootprint = !_switchFootprint;
				_timer = 0.0f;
			}
		}
	}

	[PunRPC]
	private void AskForFootprint(float height)
	{
		GameManager.Instance.GenerateFootprint(gameObject.tag, height, _switchFootprint);
	}
}
