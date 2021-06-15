using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Eye")]
public class Eye : Skill
{
	// hit default objects
	public float castRange = 20.0f;
	public LayerMask layermask;
	public ParticleSystem projectile = null;

	private int _castingSpell;
	private float _projectileSpeed = 15.0f;

	public override void Initialize(CharacterManager charManager)
	{
		base.Initialize(charManager);
		_castingSpell = Animator.StringToHash("castingSpell");

		// this has to be set by hand
		_projectileSpeed = 15.0f;
	}

	public override void ActivateEffects()
	{
		// animation
		_charManager.Animator.SetTrigger(_castingSpell);

		RaycastHit hit;
		Ray ray = _charManager.Camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));

		if(Physics.Raycast(ray, out hit, castRange, layermask))
		{
			_charManager.CallCoroutine(WaitForCollision(hit));
		}
		else
		{
			_hasToDeactivate = false;
			_charManager.CallCoroutine(Wait());
			DeactivateEffects();
			PlayerHUD hud = _charManager.PlayerHUD;
			_charManager.CallCoroutine(hud.SetEventText("You are too far!", hud.eventColors[0]));
		}
	}

	public override void DeactivateEffects()
	{
		GameManager.Instance.DeactivateEyeRPC();
		_charManager.PlayerHUD.EyeUI.SetActive(false);
	}

	// this is to prevent a race-conflict bug
	private IEnumerator Wait()
	{
		float temp = baseDuration;
		baseDuration = 0.01f;
		yield return new WaitForSeconds(1.0f);
		baseDuration = temp;
	}

	private IEnumerator WaitForCollision(RaycastHit hit)
	{
		float dist = Vector3.Distance(_charManager.transform.position, hit.point);
		float timeToCollide = dist / _projectileSpeed;

		GameManager.Instance.GenerateProjectile(timeToCollide, hit.point);

		yield return new WaitForSeconds(timeToCollide);

		
		Vector3 normalToNormal = Vector3.Cross(hit.normal, _charManager.transform.position - hit.point);
		Vector3 offset = Vector3.Cross(normalToNormal, hit.normal);
		Vector3 finalPos = hit.point - hit.normal * 0.7f + offset.normalized * 0.5f;

		GameManager.Instance.GenerateEye(finalPos, Quaternion.LookRotation(hit.normal, Vector3.up));
		_charManager.PlayerHUD.EyeUI.SetActive(true);
	}
}
