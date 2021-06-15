using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/TrueSight")]
public class TrueSight : Skill
{
	public float sightDistance = 30.0f;
	public Material maleXray = null;
	public Material femaleXray = null;
	public Material invisibleMat = null;

	private Transform _maleWiz = null;
	private Transform _femaleWiz = null;
	private List<SkinnedMeshRenderer> _maleRenderers = new List<SkinnedMeshRenderer>();
	private List<SkinnedMeshRenderer> _femaleRenderers = new List<SkinnedMeshRenderer>();
	private List<Material> _maleOriginalMats = new List<Material>();
	private List<Material> _femaleOriginalMats = new List<Material>();

	public override void Initialize(CharacterManager charManager)
	{
		base.Initialize(charManager);
		_charManager.CallCoroutine(DelayedInit());
	}

	public override void ActivateEffects()
	{
		// Check if wizards are in range
		bool isMaleInSight = IsInSight(_maleWiz.position);
		bool isFemaleInSight = IsInSight(_femaleWiz.position);

		bool isMaleInvisible = _maleRenderers[0].material.name == invisibleMat.name + " (Instance)";
		bool isFemaleInvisible = _femaleRenderers[0].material.name == invisibleMat.name + " (Instance)";

		if (isMaleInSight && !isMaleInvisible)
		{
			for (int i = 0; i < _maleRenderers.Count; i++)
			{
				if (_maleOriginalMats.Count <= i)
					_maleOriginalMats.Add(_maleRenderers[i].material);

				_maleRenderers[i].material = maleXray;
			}
		}

		if (isFemaleInSight && !isFemaleInvisible)
		{
			for (int i = 0; i < _femaleRenderers.Count; i++)
			{
				if (_femaleOriginalMats.Count <= i)
					_femaleOriginalMats.Add(_femaleRenderers[i].material);

				_femaleRenderers[i].material = femaleXray;
			}
		}

		GameManager.Instance.GenerateSightFeedback(isMaleInSight, isFemaleInSight);
	}

	public override void DeactivateEffects()
	{
		if (_maleOriginalMats.Count > 0)
		{
			for (int i = 0; i < _maleRenderers.Count; i++)
			{
				_maleRenderers[i].material = _maleOriginalMats[i];
			}
		}

		if (_femaleOriginalMats.Count > 0)
		{
			for (int i = 0; i < _femaleRenderers.Count; i++)
			{
				_femaleRenderers[i].material = _femaleOriginalMats[i];
			}
		}
	}

	// check for both range and screen visibility
	private bool IsInSight(Vector3 pos)
	{
		bool isInRange = (pos - _charManager.transform.position).sqrMagnitude <= (sightDistance * sightDistance);
		// check that the position is in your view frustum (even if not rendered)
		Vector3 screenPos = _charManager.Camera.WorldToScreenPoint(pos);    // z is for depth
		bool isInView = screenPos.x >= 0.0f && screenPos.x <= Screen.width && screenPos.y >= 0.0f && screenPos.y <= Screen.height;
		return isInRange && isInView;
	}

	private IEnumerator DelayedInit()
	{
		bool flag = true;
		while (flag)
		{
			GameObject male = GameObject.FindGameObjectWithTag("Wizard_1");
			GameObject female = GameObject.FindGameObjectWithTag("Wizard_2");
			if (male != null && female != null)
			{
				flag = false;
				_maleWiz = male.transform;
				_femaleWiz = female.transform;
			}
			yield return new WaitForSeconds(0.5f);
		}

		_maleRenderers = new List<SkinnedMeshRenderer>();
		_femaleRenderers = new List<SkinnedMeshRenderer>();
		_maleWiz.gameObject.GetComponentsInChildren(_maleRenderers);
		_femaleWiz.gameObject.GetComponentsInChildren(_femaleRenderers);

		_maleOriginalMats = new List<Material>();
		_femaleOriginalMats = new List<Material>();
	}
}
