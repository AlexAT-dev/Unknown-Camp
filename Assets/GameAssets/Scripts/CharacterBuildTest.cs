using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.U2D.Animation;

public class CharacterBuildTest : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private Transform characterTransform;

	[Header("Prefabs")]
	[SerializeField] private GameObject headPrefab;
	[SerializeField] private GameObject torsoPrefab;
	[SerializeField] private GameObject legsPrefab;

	[Header("Objects")]
	[SerializeField, ReadOnly] private GameObject head;
	[SerializeField, ReadOnly] private GameObject torso;
	[SerializeField, ReadOnly] private GameObject legs;

	private List<Transform> bones = new List<Transform>();
	private Transform buildTransform;


	private void Start()
	{
		bones = FindBonesRecursively(characterTransform);
		buildTransform = characterTransform.Find("Build");
	}

	public void Build()
	{
		head = Instantiate(headPrefab, Vector3.zero, Quaternion.identity, buildTransform);
		torso = Instantiate(torsoPrefab, Vector3.zero, Quaternion.identity, buildTransform);
		legs = Instantiate(legsPrefab, Vector3.zero, Quaternion.identity, buildTransform);

		GameObject[] build = { head, torso, legs };

		foreach (GameObject part in build)
		{
			foreach (Transform obj in part.transform)
			{
				SpriteSkin spriteSkin = obj.GetComponent<SpriteSkin>();

				if (spriteSkin == null) continue;

				var rootBoneProperty = typeof(SpriteSkin).GetProperty(nameof(SpriteSkin.rootBone));
				rootBoneProperty!.SetValue(spriteSkin, characterTransform);

				SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

				if (spriteRenderer == null) continue;

				SpriteBone[] spriteBones = spriteRenderer.sprite.GetBones();

				var boneTransformsProperty = typeof(SpriteSkin).GetProperty(nameof(SpriteSkin.boneTransforms));
				boneTransformsProperty!.SetValue(spriteSkin, FindBones(spriteBones));
			}
		}
	}

	private Transform[] FindBones(SpriteBone[] spriteBones)
	{
		List<Transform> boneTransforms = new List<Transform>();
		
		foreach (SpriteBone bone in spriteBones)
		{
			Transform boneTransform = bones.Find(x => x.name == bone.name);

			if (boneTransform == null) continue;

			boneTransforms.Add(boneTransform);
		}

		return boneTransforms.ToArray();
	}

	public List<Transform> FindBonesRecursively(Transform parent)
	{
		List<Transform> bones = new List<Transform>();

		foreach (Transform child in parent)
		{
			if (child.name.StartsWith("Bone"))
			{
				bones.Add(child);
			}

			bones.AddRange(FindBonesRecursively(child));
		}

		return bones;
	}

	public void Clear()
	{
		Destroy(head);
		Destroy(torso);
		Destroy(legs);
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Space))
		{
			Clear();
			Build();
		}
	}
}
