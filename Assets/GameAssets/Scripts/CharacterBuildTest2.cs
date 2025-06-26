using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBuildTest2 : MonoBehaviour
{
	[SerializeField] private CharacterModel characterModelPrefab;
	[SerializeField] private Character character;
	[SerializeField] private List<GameObject> prefabs;
	[SerializeField] private Vector3 pos;


	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Space))
		{
			CharacterModel characterModel = Instantiate(characterModelPrefab, pos, Quaternion.identity);
			characterModel.Initialize(character, prefabs);
		}
	}
}
