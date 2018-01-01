using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuLogic : MonoBehaviour {

    public Camera charcterSelectCam;
    public Transform charactersRoot;
    //----UI----
    public Text connectionText;
    public Image connBG;
    public Color onlineColor;
    public Color offlineColor;
    public Text statsText;
    public InputField characterName;
    ////--------


    private List<Transform> characterList;
    private GameManager gameManager;
    private Coroutine moveCor;
    private Coroutine onlineStateCor;
    private int selectedIndex = 0;

    private static MainMenuLogic _instance;
    public static MainMenuLogic instance
    {
        get { return _instance; }
        private set { _instance = value; }
    }

    #region Unity Methods
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);        
    }    

	void Start () 
    {
        gameManager = GameManager.instance;
        characterList = new List<Transform>();
        OnlineState();

        CreateCharacters();
        try
        {
            gameManager.LoadStatisticsForCharacters();            
        }
        catch {  }

        selectedIndex = gameManager.selectedIndex;
        
        StartCoroutine(MoveCamCor(selectedIndex));
	}

    #endregion


    #region Custom Methods
    private void CreateCharacters()
    {
        int inx = 0;
        bool isCreated = gameManager.characters.Count > 0;
        foreach (Transform child in charactersRoot)
        {
            if (!isCreated)            
            {
                Character character = new Character(child.gameObject.name, gameManager.SaveStatisticsForCharacters);
                character.id = inx;
                gameManager.characters.Add(character);
            }

            characterList.Add(child);            
        }
    }
    
    public void ChooseNext()
    {
        if (moveCor != null)
            StopCoroutine(moveCor);

        selectedIndex++;
        if (selectedIndex >= characterList.Count)
            selectedIndex = characterList.Count - 1;

        moveCor = StartCoroutine(MoveCamCor(selectedIndex));
    }

    public void ChoosePrev()
    {

        if (moveCor != null)
            StopCoroutine(moveCor);

        selectedIndex--;
        if (selectedIndex < 0)
            selectedIndex = 0;        

        moveCor = StartCoroutine(MoveCamCor(selectedIndex));
 
    }

    /// <summary>
    /// Выбираем уровень по интексу
    /// </summary>
    public void ChooseRoom(int sceneInx)
    {
        if (PhotonNetwork.connectedAndReady)
        {
            gameManager.selectedIndex = selectedIndex;
            SceneManager.LoadScene(sceneInx);
        }        
    }

    public void OnStatsUpdated()
    {
        if (gameManager.characters.Count == 0)
            return;

        CharacterStatistic stats = gameManager.characters[selectedIndex].statistic;        

        int win = stats.win;
        int loose = stats.loose;
        
        float rate = (float)((float)win / (loose == 0 ? 1 : (float)loose));

        string text = string.Format("Победы: {0}\nПоражения: {1}\nСоотношение: {2:0.0}", win, loose, rate);        
        statsText.text = text; 
    }

    public void OnlineState()
    {
        if (onlineStateCor == null)
            onlineStateCor = StartCoroutine(OnliceStateCorr());
        
    }
    public void Quit()
    {
        gameManager.Quit();
    }

    #endregion

    #region Coroutines
    private IEnumerator OnliceStateCorr()
    {
        if (PhotonNetwork.connectionState != ConnectionState.Connected)
        {
            connectionText.text = "Offline";
            connBG.color = offlineColor;
        }

        yield return new WaitWhile(() => PhotonNetwork.connectionState != ConnectionState.Connected);

        connectionText.text = "Online";
        connBG.color = onlineColor;

        onlineStateCor = null;

    }

    private IEnumerator MoveCamCor(int inx)
    {
        OnStatsUpdated();
        characterName.text = gameManager.characters[inx].name;

        float offset = 0.1f;
        float point = characterList[selectedIndex].transform.position.x;
        float speed = 10.0f;
        Vector3 pos = charcterSelectCam.transform.position;
        while (pos.x < point - offset || pos.x > point + offset)
        {            
            pos.x = Mathf.Lerp(pos.x, point, Time.deltaTime * speed);
            charcterSelectCam.transform.position = pos;
            yield return null;
        }

        moveCor = null;
    }
    #endregion
}
