using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{    
    public OnFightStartCallback FightStartCallback;
    public event OnGameEndCallback GameEndCallback;    
    public List<Character> characters;
    [Header("Без расширения")]
    public string statisticsFileName;
    public int selectedIndex;
    [HideInInspector]
    public string connectionIP;
    [HideInInspector]
    public int connectionPort;

    /// <summary>
    /// ActorNr, PlayerManager
    /// </summary>
    private Dictionary<int, PlayerManager> players;
    private string statisticPath;
    private static GameManager _instance;
    private bool canFight = false;
    private string settingsPath;
    private int loadTrys = 0;


    public delegate void OnFightStartCallback();
    public delegate void OnGameEndCallback(bool isWin);    


    public static GameManager instance
    {
        get { return _instance;}
        private set { _instance = value; }
    }

    public string sceneName
    {
        get
        {
            return SceneManager.GetActiveScene().name;
        }
    }

    #region Unity Methods
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(this);
        
        selectedIndex = 0;
        characters = new List<Character>();
        players = new Dictionary<int, PlayerManager>();

        statisticPath = string.Format("{0}/{1}.sts", Application.dataPath, statisticsFileName);
        settingsPath = string.Format("{0}/ConnectionSettings.xml", Application.dataPath);

        LoadSettings();
    }

    #endregion


    #region Custom Methods

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
       Application.Quit();
#endif
    }
    
    public void ResertCharactersStats()
    {
        foreach (Character chr in characters)
        {
            chr.ResetStats();
        }

        SaveStatisticsForCharacters();
        LoadStatisticsForCharacters();
        
    }

    private bool LoadSettings()
    {
        loadTrys++;
        connectionIP = "127.0.0.1";
        connectionPort = 5055;

        try
        {
            using (FileStream fstream = new FileStream(settingsPath, FileMode.Open))
            {
                XmlDocument doc = new XmlDocument();
                string body = string.Empty;

                using (StreamReader sreader = new StreamReader(fstream))
                {
                    body = sreader.ReadToEnd();
                }

                doc.LoadXml(body);
                XmlElement root = doc.DocumentElement;
                connectionIP =  root.Attributes[0].InnerText;
                string port =  root.Attributes[1].InnerText;
                connectionPort = XmlConvert.ToInt32(port);
            }

            Debug.Log(string.Format("Settings are loaded! Using ip: {0} port: {1}", connectionIP, connectionPort));
            return true;

        }
        catch(System.Exception e)
        {            
            Debug.LogError(string.Format("Can't load {0}! {1}", settingsPath, e.Message));   
         
            if (loadTrys > 1)
                return false;

            Debug.Log(string.Format("Creating new file with default settings!"));
            //Создаем
            using (FileStream fstream = new FileStream(settingsPath, FileMode.Create))
            {                
                XmlDocument doc = new XmlDocument();

                XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

                doc.AppendChild(decl);                

                XmlElement ipNode = doc.CreateElement("connectionSettings");
                ipNode.SetAttribute("ip", connectionIP);
                ipNode.SetAttribute("port", connectionPort.ToString());

                doc.AppendChild(ipNode);
                doc.Save(fstream);

            }

            Debug.Log(string.Format("File {0} created! Using default settings! ip: {1} port: {2}", settingsPath, connectionIP, connectionPort));

            if (LoadSettings())
            {
                return true;
            }
            else
            {
                Debug.LogError(string.Format("Using default settings! ip: {1} port: {2}", connectionIP, connectionPort));
                return false;
            }
        }
        
    }

    public void SaveStatisticsForCharacters()
    {        
        Dictionary<string, CharacterStatistic> dict = new Dictionary<string, CharacterStatistic>();

        foreach (Character chr in characters)
        {
            dict.Add(chr.name, chr.statistic);            
        }

        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            using (FileStream fstream = new FileStream(statisticPath, FileMode.Create))
            {
                formatter.Serialize(fstream, dict);
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Message);
        }

        Debug.Log("Stats are saved!");        
    }

    public bool LoadStatisticsForCharacters()
    {
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fstream = new FileStream(statisticPath, FileMode.Open))
            {
                Dictionary<string, CharacterStatistic> dict = formatter.Deserialize(fstream) as Dictionary<string, CharacterStatistic>;
                foreach (KeyValuePair<string, CharacterStatistic> pair in dict)
                {
                    Character[] chr = (from character in characters where character.name == pair.Key select character).ToArray();
                    chr[0].statistic = pair.Value;                    
                    if (chr.Length > 1)
                    {
                        Debug.LogError("Имеются персонажи с одинкаковымим именами!!!");
                    }

                    Debug.Log(string.Format("Loaded stats for name {0} win {1} loose {2}", chr[0].name, chr[0].statistic.win, chr[0].statistic.loose));
                }

                if (MainMenuLogic.instance)
                    MainMenuLogic.instance.OnStatsUpdated();

                return true;
            }
        }
        catch (IOException e)
        {
            Debug.LogError(string.Format("Can't load stats {0}!\n{1}", statisticPath,e.Message));
            Debug.Log(string.Format("Creating new file for statistic!"));
            SaveStatisticsForCharacters();

            return false;
        }
    }

    public void AddPlayerToDict(int id, PlayerManager playerScript)
    {
        if (!players.ContainsValue(playerScript))
            players.Add(id, playerScript);
    }

    public void RemovePlayerFromDict(int id)
    {
        players.Remove(id);
    }

    public PlayerManager GetPlayerById(int id)
    {
        PlayerManager player;

        if (players.TryGetValue(id, out player))
            return player;
        else
            return null;
    }

    public void FightStart()
    {
        canFight = true;
        FightStartCallback();
        NetGameScnenManager.instance.CloseRoom();
    }    

    public bool CanFight()
    {        
        return canFight;        
    }

    public void Loose()
    {
        canFight = false;
        characters[selectedIndex].Loose();
        GameEndCallback(false);
    }

    public void Win()
    {
        canFight = false;
        characters[selectedIndex].Win();
        GameEndCallback(true);
    }


    public PlayerManager GetOtherPlayer(int inx)
    {
        PlayerManager[] sorted = (from plrs in players where plrs.Key != inx select plrs.Value).ToArray();
        return sorted[0];

    }

    #endregion

}
