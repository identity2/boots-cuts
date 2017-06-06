using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//An object pool of music nodes
public class MusicNodePool : MonoBehaviour 
{
	public static MusicNodePool instance;

	public GameObject nodePrefab;

	public int initialAmount;

	private List<MusicNode> objList;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		objList = new List<MusicNode>();

		for (int i = 0; i < initialAmount; i++)
		{
			GameObject obj= (GameObject) Instantiate(nodePrefab);
			obj.SetActive(false);
			objList.Add(obj.GetComponent<MusicNode>());
		}
	}

	public MusicNode GetNode(float posX, float startY, float endY, float removeLineY, float posZ, float beat, int times, Color color)
	{
		//check if there is an inactive instance
		foreach (MusicNode node in objList)
		{
			if (!node.gameObject.activeInHierarchy)
			{
				node.Initialize(posX, startY, endY, removeLineY, posZ, beat, times, color);
				node.gameObject.SetActive(true);
				return node;
			}
		}

		//no inactive instances, instantiate a new GetComponent
		MusicNode musicNode = ((GameObject) Instantiate(nodePrefab)).GetComponent<MusicNode>();
		musicNode.Initialize(posX, startY, endY, removeLineY, posZ, beat, times, color);
		objList.Add(musicNode);
		return musicNode;
		
	}

}
