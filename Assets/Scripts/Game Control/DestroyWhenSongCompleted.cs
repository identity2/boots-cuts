using UnityEngine;

public class DestroyWhenSongCompleted : MonoBehaviour 
{

	void Awake()
	{
		Conductor.songCompletedEvent += DestroyWhenCompleted;
	}

	void OnDestroy()
	{
		Conductor.songCompletedEvent -= DestroyWhenCompleted;
	}

	void DestroyWhenCompleted()
	{
		Destroy(gameObject);
	}
}
