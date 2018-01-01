using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerManager : MonoBehaviour {

    public enum Mode
    {
        GAME_NOT_REAY,
        GAME_READY,
        FIGHT,
        MAINMENU
    }   

    public Image readyImage;
    public Text hpText;
    public GameObject hpRoot;
    public GameObject redyRoot;
    public Image backGround;
    public Color yourColor;
    public Color enemyColor;
    public Image message;
    public Text messageText;
    public string[] deadMessage;
    public string[] notReadyMessage;

    private GameObject canvas;
    private Coroutine activateCor;
    private float messgaeTime = 0.3f;

    private Mode mode
    {        
        set
        {
            switch (value)
            {
                case Mode.MAINMENU:
                    canvas.SetActive(false);
                    break;
                case Mode.GAME_NOT_REAY:
                    canvas.SetActive(true);
                    break;
                case Mode.GAME_READY:
                    OnReady();
                    break;
                case Mode.FIGHT:
                    OnFightStart();
                    break;
            }
        }
    }

    #region Unity Methods
    public void Awake()
    {
        canvas = hpRoot.transform.parent.gameObject;
        canvas.transform.LookAt(Camera.main.transform);
    }

	
	void Start ()
    {
        readyImage.enabled = false;
        message.enabled = false;
        hpRoot.SetActive(false);
        messageText.text = string.Empty;
	}

    #endregion


    #region Custom Methods
    public void OnFightStart()
    {
        redyRoot.SetActive(false);
        hpRoot.SetActive(true);
    }

    private void OnReady()
    {
        readyImage.enabled = true;
    }

    public void OnUpdateHP(int newHp)
    {
        hpText.text = string.Format("HP: {0}", newHp);
    }

    public void SetMode(Mode gameMode)
    {
        mode = gameMode;
    }

    public void SetColorBySide(bool mine)
    {
        if (mine)
            backGround.color = yourColor;
        else
            backGround.color = enemyColor;        
    }

    /// <summary>
    /// Получаем рандомное вращение и позицию сообщения. Возвращает (х, у - позиции, zRot -вращение)
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomPos()
    {
        Vector3 vec = Vector3.zero;

        float x = Random.Range(-0.3f, 0.3f);
        float y = Random.Range(-0.3f, 0.3f);

        float zRot = Random.Range(-30.0f, 30.0f);

        vec.x = x;
        vec.y = y;
        vec.z = zRot;

        return vec;
    }

    public void OnDeadClick()
    {
        if (activateCor == null)
            activateCor = StartCoroutine(ActiveMessageBoxForASeconds());
        else
            return;

        int inx = Random.Range(0, deadMessage.Length);
        string text = deadMessage[inx];
        messageText.text = text;

        Vector3 posRot = GetRandomPos();

        message.transform.localPosition = new Vector3(posRot.x, posRot.y, 0.8f);
        message.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 180.0f, posRot.z));
        
    }

    public void OnNotReadyClick()
    {
        if (activateCor == null)
            activateCor = StartCoroutine(ActiveMessageBoxForASeconds());
        else
            return;

        int inx = Random.Range(0, notReadyMessage.Length);
        string text = notReadyMessage[inx];
        messageText.text = text;

        Vector3 posRot = GetRandomPos();

        message.transform.localPosition = new Vector3(posRot.x, posRot.y, 0.8f);
        message.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 180.0f, posRot.z));
        
    }

    #endregion

    #region Coroutines

    IEnumerator ActiveMessageBoxForASeconds()
    {
        yield return new WaitForSeconds(messgaeTime);

        messageText.text = string.Empty;        
        activateCor = null;

    }

    #endregion

}
