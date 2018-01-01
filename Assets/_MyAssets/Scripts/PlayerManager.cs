using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIPlayerManager))]
public class PlayerManager : MonoBehaviour {

    public bool isReady;
    public bool isDead;
    
    private PhotonView view;
    private int id;
    private GameManager gameManager;    
    private UIPlayerManager playerUI;
    private NetGameScnenManager netSceneManager;
    private Animator animator;

    //animator params
    private const string hit = "hit";
    private const string death = "death";
    private const string attack = "attack";

    #region Unity Methods
    private void Awake()
    {
        view = GetComponent<PhotonView>();
        id = view.OwnerActorNr;
        playerUI = GetComponent<UIPlayerManager>();
        animator = GetComponent<Animator>();        
    }
	
	void Start ()
    {
        isReady = false;
        isDead = false;
        gameManager = GameManager.instance;
        netSceneManager = NetGameScnenManager.instance;

        gameManager.FightStartCallback += OnFightStart;

        if (!SceneManager.GetActiveScene().name.Equals(SceneManager.GetSceneByBuildIndex(0).name))
        {
            gameManager.AddPlayerToDict(id, this);
            playerUI.SetMode(UIPlayerManager.Mode.GAME_NOT_REAY);
        }
        else
            playerUI.SetMode(UIPlayerManager.Mode.MAINMENU);

        playerUI.SetColorBySide(view.isMine);
    }
       

    private void OnDestroy()
    {
        gameManager.FightStartCallback -= OnFightStart;
        gameManager.RemovePlayerFromDict(id);

        if(gameManager.CanFight())
        {
            if(view.isMine)
                gameManager.Loose();
            else
                gameManager.Win();
        }        
    }

    private void OnMouseDown()
    {
        if (view.isMine)
            return;

        if (!isDead && gameManager.CanFight())
        {            
            netSceneManager.OnHit(view.ownerId);            
        }
        else 
        {
            if (isDead)
                playerUI.OnDeadClick();
            else if(!isReady)
                playerUI.OnNotReadyClick();               
        }

    }
    
    #endregion



    #region Custom Methods

    public void PlayAttack()
    {
        animator.SetTrigger(attack);
    }

    public void OnReceiveReady()
    {
        isReady = true;
        playerUI.SetMode(UIPlayerManager.Mode.GAME_READY);
    }

    public void SetReady()
    {
        isReady = true;
        playerUI.SetMode(UIPlayerManager.Mode.GAME_READY);
        netSceneManager.SendReady();        
    }

    public void OnDead()
    {
        isDead = true;
        playerUI.OnUpdateHP(0);
        animator.SetBool(death, true);
        
        if (view.isMine)
        {
            gameManager.Loose();
        }
        else
        {   
            gameManager.Win();
        }
        
    }

    public void UpdateHP(int health)
    {        
        animator.SetTrigger(hit);
        playerUI.OnUpdateHP(health);
    }
    

    internal void OnFightStart()
    {
        playerUI.OnFightStart();
    }    
    #endregion
    
}
