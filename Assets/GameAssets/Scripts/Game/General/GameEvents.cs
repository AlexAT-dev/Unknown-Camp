using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }


    [SerializeField] private Light2D spotlight;
    public void BoatTaskSpotlightOn()
    {
        spotlight.gameObject.SetActive(true);
        //spotlight.intensity = Mathf.Lerp(0, 1.5f, Time.deltaTime);
    }

    [SerializeField] private GameObject[] mapPieces;
    public void ForestTaskMapShow(int mapPiece)
    {
        mapPieces[mapPiece].gameObject.SetActive(true);
    }

    [SerializeField] private InteractiveItem wood;
    public void CreateWood(GameObject point)
    {
        GameController.Instance.InstantiateItem(wood, point.transform.position, new Vector2(2, 0.3f));
    }

    public void ClearTask(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void AddMatchesTask(int value)
    {
        GameController.Instance.AddMatches("Tasks", value);
    }

    private int catchedCampers;
    public void CamperCatched(PlayerCamper camper)
    {
        if (!camper.IsMine && camper.Character.name != "Billy" && GameController.LocalCamper.Character.Name == "Billy")
        {
            catchedCampers++;
            if(catchedCampers % 2 == 0)
            {
                GameController.LocalCamperTraits.Buff(2);
                Debug.Log(GameController.LocalCamperTraits.mind);
            }
            if (catchedCampers % 5 == 0)
            {
                PunManager.SetPlayerProperties("Hp", (int)GameController.LocalCamper.Owner.CustomProperties["Hp"] + 1);
            }
        }
    }

    [Header("Phase")]
    [SerializeField] private Light2D mapLight;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color attackColor;
    [SerializeField] private float duration = 1f;
    [SerializeField] private AudioClip haunting;
    [SerializeField] private AudioClip forest;
    private Coroutine changeColor;
    public void StartAttack()
    {
        if(changeColor != null) StopCoroutine(changeColor);
        changeColor = StartCoroutine(ChangeColor(defaultColor, attackColor));
        SoundManager.Instance.Stop();
        SoundManager.Instance.Play(haunting);
    }

    public void StopAttack()
    {
        if (changeColor != null) StopCoroutine(changeColor);
        changeColor = StartCoroutine(ChangeColor(attackColor, defaultColor));
        SoundManager.Instance.Stop();
        SoundManager.Instance.Play(forest);
    }

    private IEnumerator ChangeColor(Color color1, Color color2)
    {
        float elapsedTime = 0f;
        Color startColor = color1;

        while (elapsedTime < duration)
        {
            mapLight.color = Color.Lerp(startColor, color2, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mapLight.color = color2;
    }
}
