using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISceneManager : MonoBehaviour {

    public GameObject readyBtn;
    public GameObject endGame;
    public Text endGameText;
    public Image endGameBGImg;

    private GameManager gameManager;

    #region Unity Methods
    public void Start()
    {
        gameManager = GameManager.instance;
        gameManager.FightStartCallback += OnFightStart;
        gameManager.GameEndCallback += OnGameEnd;

        endGame.SetActive(false);
        readyBtn.SetActive(true);
    }

	public void OnDestroy()
	{
		gameManager.GameEndCallback -= OnGameEnd;
	}
	#endregion


    #region Custom Methods
    private void CheckOnNull()
    {
        readyBtn.SetActive(true);
    }    

    public void ReturnToMainMenu()
    {
        if(gameManager.CanFight())
            gameManager.Loose();        
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
        PhotonNetwork.LeaveRoom();
    }

    public void Quit()
    {
        gameManager.Quit();
    }

    public void SetReady()
    {
        int id = PhotonNetwork.player.ID;
        PlayerManager player =  GameManager.instance.GetPlayerById(id);
        player.SetReady();
    }

   
    private void OnFightStart()
    {
        readyBtn.SetActive(false);
        gameManager.FightStartCallback -= OnFightStart;        
    }
    
    private void OnGameEnd(bool isWin)
    {
        string text = string.Empty;
        if (isWin)
        {
            endGameBGImg.color = Color.green;
            text = "YOU WIN!";
        }
        else
        {
            endGameBGImg.color = Color.cyan;
            text = "YOU LOOSE!";
        }

        //TODO: отловить баг, возможно с photon, endGame внезапно становится null O_O
        endGame.SetActive(true);
        endGameText.text = text; 
    }

    #endregion

}
