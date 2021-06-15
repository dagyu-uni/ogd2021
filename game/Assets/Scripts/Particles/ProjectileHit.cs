using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHit : MonoBehaviour
{
	private void OnParticleCollision(GameObject other)
	{
		Debug.Log("COLLISION");
	}
}
