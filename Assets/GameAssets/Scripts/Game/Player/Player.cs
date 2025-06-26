using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour, IPunInstantiateMagicCallback, IPunObservable
{
    [Header("Components")]
    [SerializeField] protected PhotonView photonView;
    [SerializeField] protected PlayerMovement movement;
    [SerializeField] protected Rigidbody2D rbody;
    [SerializeField] protected BoxCollider2D collision;
    [SerializeField] protected CapsuleCollider2D trigger;
    [SerializeField] protected GameObject visual;
    [SerializeField] protected GameObject myCollider;
    [SerializeField] protected GameObject cameraTarget;
    [SerializeField] protected bool flipVisual;
    [SerializeField] protected bool disabled;
    [SerializeField] protected List<SpriteRenderer> sprites;
    [SerializeField] protected Animator animator;

    protected GameObject characterObject;
    
    [Header("Player props")]
    [SerializeField] protected Character character;
    [SerializeField] protected SkinItem skin;

    public PhotonView PhotonView => photonView;
    public Photon.Realtime.Player Owner => photonView.Owner;
    public PlayerMovement Movement => movement;
    public Rigidbody2D Rigidbody => rbody;
    public BoxCollider2D Collision => collision;
    public CapsuleCollider2D Trigger => trigger;
    public GameObject Visual => visual;
    public GameObject CameraTarget => cameraTarget;

    public bool IsMine => photonView.IsMine;
    public Character Character => character;
    public SkinItem Skin => skin;
    public Animator Animator => animator; 
    public abstract Character.CharacterType Role { get; }


    protected void FixedUpdate()
    {
        if (!IsMine) return;

        movement.Move();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;
        if (data == null) return;

        character = MasterManager.CharactersList.FindCharacter(data[0]?.ToString(), Role);
        skin = character.FindSkin(data[1]?.ToString());
        GameObject prefab = skin ? skin.Prefab : character.Prefab;

        characterObject = Instantiate(prefab, visual.transform);
        sprites.AddRange(characterObject.GetComponentsInChildren<SpriteRenderer>());
        animator = characterObject.GetComponent<Animator>();
        PhotonAnimatorView animView = characterObject.GetComponent<PhotonAnimatorView>();
        if (animView != null) photonView.ObservedComponents.Add(animView);

        if (IsMine)
        {
            CameraMovement.Instance.SetTarget(cameraTarget.transform, true);
        }
        else
        {
            Destroy(rbody);
            Destroy(movement);
            Destroy(collision.gameObject);
            Destroy(myCollider);
        }

        if(this is PlayerCamper camper)
        {
            //camper.Traits = 
            GameController.Campers.Add(camper);
            camper.FindBoneHand();
            CamperIconList.Instance.CreateCamper(camper);
        }
        else if (this is PlayerCatcher catcher)
        {
            GameController.Catchers.Add(catcher);
            //CharacterCatcher.CatcherTraits traits = catcher.CharacterCatcher.Traits;
            //catcher.InitializeTrigger(traits.triggerOffset, traits.triggerSize);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(flipVisual);
        }
        else if (stream.IsReading) 
        {
            UpdateVisualDir((bool)stream.ReceiveNext());
        }
    }

    public void UpdateVisualDir(bool flipped)
    {
        flipVisual = flipped;
        Vector3 scale = visual.transform.localScale;
        float scaleX = Math.Abs(visual.transform.localScale.x);
        visual.transform.localScale = new Vector3(flipVisual ? -scaleX : scaleX, scale.y, scale.z);
    }

    public void Teleport(Vector3 pos)
    {
        if (!IsMine) return;
        
        transform.position = pos;
        Camera.main.transform.position = GameController.CameraTarget.transform.position;
    }

    public void TriggerRefresh()
    {
        trigger.enabled = false;
        trigger.enabled = true;
    }

    public void Disable()
    {
        disabled = true;
        trigger.enabled = false;
        if (collision) collision.enabled = false;
        if (IsMine) Movement.AddBlockReason("disabled");
    }

    public void Hide()
    {
        Visual.SetActive(false);
    }

    public virtual void LeftRoom()
    {
        Hide();
        Disable();
    }

}
