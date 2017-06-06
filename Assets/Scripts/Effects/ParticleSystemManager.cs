using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemManager : MonoBehaviour 
{
	public ParticleSet[] particleSet;
	
	public ParticleSystem perfectionEffect;
	public ParticleSystem comboEffect;

	void Start()
	{
		Conductor.beatOnHitEvent += BeatOnHit;
	}

	void OnDestroy()
	{
		Conductor.beatOnHitEvent -= BeatOnHit;
	}

	//will be informed by the Conductor after a beat is hit
	void BeatOnHit(int track, Conductor.Rank rank)
	{
		if (rank == Conductor.Rank.PERFECT)
		{
			particleSet[track].perfect.Play();
			perfectionEffect.Play();
			comboEffect.Play();
		}
		else if (rank == Conductor.Rank.GOOD)
		{
			particleSet[track].good.Play();
			comboEffect.Play();
		}
		else if (rank == Conductor.Rank.BAD)
		{
			particleSet[track].bad.Play();
		}

		//do nothing if missed
	}

	[System.Serializable]
	public class ParticleSet
	{
		public ParticleSystem perfect;
		public ParticleSystem good;
		public ParticleSystem bad;
	}
}
