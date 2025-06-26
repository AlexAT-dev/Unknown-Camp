using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.U2D;

public class CharacterModel : MonoBehaviour
{
	[Header("Objects")]
	[SerializeField, ReadOnly] private List<GameObject> buildPrefabs;
	[SerializeField, ReadOnly] private List<GameObject> buildParts;

	private List<Transform> bones = new List<Transform>();
	private Transform buildTransform;


	public void Initialize(Character character, List<GameObject> build)
	{
		name = character.Name;
		buildPrefabs = build;
		bones = FindBones(transform);
		buildTransform = transform.Find("Build");
		Build();
	}

	private void Build()
	{
		foreach (GameObject prefab in buildPrefabs)
		{
			GameObject part = Instantiate(prefab, Vector3.zero, Quaternion.identity, buildTransform);
			buildParts.Add(part);
			foreach (Transform obj in part.transform)
			{
				SpriteSkin spriteSkin = obj.GetComponent<SpriteSkin>();

				if (spriteSkin == null) continue;

				var rootBoneProperty = typeof(SpriteSkin).GetProperty(nameof(SpriteSkin.rootBone));
				rootBoneProperty!.SetValue(spriteSkin, transform);

				SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

				if (spriteRenderer == null) continue;

				SpriteBone[] spriteBones = spriteRenderer.sprite.GetBones();

				var boneTransformsProperty = typeof(SpriteSkin).GetProperty(nameof(SpriteSkin.boneTransforms));
				boneTransformsProperty!.SetValue(spriteSkin, SpriteBonesToTransform(spriteBones));
			}
		}
	}

	private Transform[] SpriteBonesToTransform(SpriteBone[] spriteBones)
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

	private List<Transform> FindBones(Transform parent)
	{
		List<Transform> bones = new List<Transform>();

		foreach (Transform child in parent)
		{
			if (child.name.StartsWith("Bone"))
			{
				bones.Add(child);
			}

			bones.AddRange(FindBones(child));
		}

		return bones;
	}
}
