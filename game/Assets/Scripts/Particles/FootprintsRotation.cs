using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FootprintsRotation : MonoBehaviour
{
	public float timeBetweenSteps = 0.5f;
	public Transform feetHeight = null;

    private bool _switchFootprint = true;
    private float _timer = 0.0f;
	private PlayerController _controller = null;

	private PhotonView _photonView = null;

	private void Awake()
	{
		_controller = GetComponent<PlayerController>();
		_photonView = GetComponent<PhotonView>();
	}

	void Update()
    {
		_timer += Time.deltaTime;
		float modifier = 1.0f;
		modifier = _controller.status == Status.crouching ? 1.5f : _controller.status == Status.sprinting ? 0.5f :
					_controller.status == Status.walking ? 1.0f : 1000f;
		// if the particle system just played a footprint, switch it (activating the other system)
		if (_timer > timeBetweenSteps * modifier)
        {

			_photonView.RPC("AskForFootprint", RpcTarget.All);

			_switchFootprint = !_switchFootprint;
            _timer = 0.0f;
        }
    }

	[PunRPC]
	private void AskForFootprint()
	{
		GameManager.Instance.GenerateFootprint(gameObject.tag, feetHeight.position.y, _switchFootprint);
	}
}
