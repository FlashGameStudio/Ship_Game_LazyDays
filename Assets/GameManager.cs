using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;

public static class Extensions
{
    public static bool find<T>(this List<T> list, T target)
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        foreach (T i in list)
        {
            if (comparer.Equals(i, target))
            {
                return true;
            }
        }

        return false;
    }
}

public enum AwardType
{
    MonsterHunt,
    DailyQuests,
    TavernQuests,
    Achievements,
    LevelUp
}

public class GameManager : Singleton<GameManager>
{
    TraceType tt = TraceType.File;
    public float beforeConnect = 0;
    public float afterConnect = 0;

    //public PlayerCharacter player;
    // Helper properties and variables
    public string callingScene;

    public static bool UpdateUserInfo = false;
    public bool mapDataLoaded = false;
    public bool mapPlayersLoaded = false;
    public bool mapMonstersLoaded = false;
    public bool tavernShardGameCombinationReceived = false;
    public bool townhallRankingLoaded = false;
    //
    public string message = "";
    public int serverTick;
    [SerializeField]
    public bool huntInProgress = false;
    public bool unitReturning = false;

    public bool isQuartermasterOnBoard = true;

    // Information dialog
    [SerializeField]
    public GameObject infoPopup;
    [SerializeField]
    public GameObject infoPopupWithIcon;

    // LevelUp dialog
    [SerializeField]
    public GameObject levelUpDialog;
    public TMP_Text myLevelUpLevel;

    public const int GRADE2_COST = 2;
    public const int GRADE3_COST = 3;
    public const int GRADE4_COST = 5;
    public const int GRADE5_COST = 10;

    [SerializeField]
    public string currentScene = "HomeScene";

    public enum ActivityType
    {
        BountyHunting,
        CrewHiring,
        Sheltering
    }

  
    #region Scene Management

    public void RefreshScene(string sourceScene, string targetScene)
    {
        GameManager.Instance.callingScene = sourceScene;
        // unload scene
        if (SceneManager.GetSceneByName(sourceScene).IsValid())
            SceneManager.UnloadSceneAsync(sourceScene);
        // load new scene
        SceneManager.LoadScene(targetScene, LoadSceneMode.Additive);
    }

    public void LoadMap()
    {
        if (!SceneManager.GetSceneByName("MapScene").IsValid())
            SceneManager.LoadScene("MapScene", LoadSceneMode.Additive);
    }

    public void PrepareBeforeLoadingScene()
    {
        //Make sure that Map or Home scenes are unloaded before loading Heroes scene
        if (SceneManager.GetSceneByName("MapScene").IsValid())
            SceneManager.UnloadSceneAsync("MapScene");

        if (SceneManager.GetSceneByName("HomeScene").IsValid())
            SceneManager.UnloadSceneAsync("HomeScene");
    }

    #endregion

    #region Utilities

    /// <summary>
    /// This method is meant for loading new sprites however it doesnt load sprite with good quality hence is not used
    /// </summary>
    /// <param name="FilePath"></param>
    /// <param name="PixelsPerUnit"></param>
    /// <param name="spriteType"></param>
    /// <returns></returns>
    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

        return NewSprite;
    }

    // used above in LoadNewSprite
    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }


    #region File Handling

    /// <summary>
    /// Writes the given object instance to an XML file.
    /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
    /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
    /// <para>Object type must have a parameterless constructor.</para>
    /// </summary>
    /// <typeparam name="T">The type of object being written to the file.</typeparam>
    /// <param name="filePath">The file path to write the object instance to.</param>
    /// <param name="objectToWrite">The object instance to write to the file.</param>
    /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
    public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
    {
        TextWriter writer = null;
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            writer = new StreamWriter(filePath, append);
            serializer.Serialize(writer, objectToWrite);
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }
    }

    public static void WriteToJsonFile<T>(string fileName, T objectToWrite, bool append = false) where T : new()
    {
        string filePath = GetLogFilePath("");

        // create the file in the path if it doesn't exist
        // if the file path or name does not exist, return the default SO
        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        //string fileName = $"{itemId}.xml";
        filePath = System.IO.Path.Combine(filePath, "Log.json");
        filePath = filePath.Replace("\\", "/");

        try
        {
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(objectToWrite);
            System.IO.File.AppendAllText(filePath, jsonData);
        }
        finally
        {

        }

    }

    /// <summary>
    /// Create file path for where a file is stored on the specific platform given a folder name and file name
    /// </summary>
    /// <param name="FolderName"></param>
    /// <param name="FileName"></param>
    /// <returns></returns>
    public string GetFilePath(string FolderName)
    {
        string filePath;

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        // mac
        filePath = Path.Combine(Application.streamingAssetsPath, ("data/" + FolderName));

#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        // windows
        filePath = Path.Combine(Application.dataPath, ("" + FolderName));

#elif UNITY_ANDROID
        // android
        filePath = Path.Combine(Application.persistentDataPath, ("" + FolderName));

#elif UNITY_IOS
        // ios
        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

#endif

        // create the file in the path if it doesn't exist
        // if the file path or name does not exist, return the default SO
        filePath = filePath.Replace("\\", "/");
        //if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        //{
        //    Debug.Log("creating folder");
        //    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        //}
        //Debug.Log(filePath);
        return filePath;
    }

    public static string GetLogFilePath(string FolderName)
    {
        string filePath;

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        // mac
        filePath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, ("data/" + FolderName));

#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        // windows
        filePath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, ("data/" + FolderName));

#elif UNITY_ANDROID
        // android
        filePath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, ("data/" + FolderName));

#elif UNITY_IOS
        // ios
        filePath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, ("data/" + FolderName));

#endif

        // create the file in the path if it doesn't exist
        // if the file path or name does not exist, return the default SO
        filePath = filePath.Replace("\\", "/");
        //if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        //{
        //    Debug.Log("creating folder");
        //    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        //}
        //Debug.Log(filePath);
        return filePath;
    }

    /// <summary>
    /// Reads an object instance from an XML file.
    /// <para>Object type must have a parameterless constructor.</para>
    /// </summary>
    /// <typeparam name="T">The type of object to read from the file.</typeparam>
    /// <param name="filePath">The file path to read the object instance from.</param>
    /// <returns>Returns a new instance of the object read from the XML file.</returns>
    public static T ReadFromXmlFile<T>(string filePath) where T : new()
    {
        TextReader reader = null;
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            reader = new StreamReader(filePath);
            return (T)serializer.Deserialize(reader);
        }
        finally
        {
            if (reader != null)
                reader.Close();
        }
    }

    #endregion

    #region Tracer

    public enum TraceLevel
    {
        Error = 0,
        Warning = 1,
        Information = 2,
        Verbose = 3
    }

    public enum TraceType
    {
        Console = 0,
        File = 1,
        All = 2
    }

    public void Trace(TraceLevel tl, string message)
    {
        TraceMessage tm = new TraceMessage(tl, message);

        switch (tt)
        {
            case TraceType.File:
                TraceToFile(tm);
                break;
            case TraceType.Console:
                TraceToConsole(tm);
                break;
            case TraceType.All:
                TraceToConsole(tm);
                TraceToFile(tm);
                break;

            default:
                break;
        }
    }

    public void TraceToConsole(TraceMessage tm)
    {
        Debug.Log($"{tm.time} - {tm.traceLevel.ToString()} - {tm.message}");
    }

    public void TraceToFile(TraceMessage tm)
    {
        //WriteToXmlFile("D:\\temp\\pirate game\\Assets\\Images\\Officers\\Log.xml", tm, true);
        WriteToJsonFile("Log.json", tm, true);
    }


    public struct TraceMessage
    {
        public TraceLevel traceLevel;
        public string time;
        public string message;

        public TraceMessage(TraceLevel tl, string m)
        {
            traceLevel = tl;
            time = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            message = m;
        }
    }

    #endregion

    #endregion


    public class HexTile
    {
        public string TileName { get; set; }
        public Vector3S TileCoords { get; set; }
    }

    public List<HexTile> LoadTileMap()
    {
        string FolderName = "Map";
        UnityEngine.Object jsonMapFile = Resources.LoadAll(FolderName, typeof(TextAsset))[0];
        List<HexTile> loadedMap = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HexTile>>(jsonMapFile.ToString());

        return loadedMap;
    }

    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }

    //-----------------------------------------------
    // Start is called before the first frame update
    //-----------------------------------------------
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

    }

    public List<Event> mapListOfEvents = new List<Event>();

    public void LoadNPCs()
    {
    }

    public void LoadPlayerEvents()
    {
    }

    //---------------------------------
    // Update is called once per frame
    //---------------------------------
    // Next update in second
    private int nextUpdate = 1;
    private float nextIntervalUpdate = 0.1f;
    void Update()
    {
        if (Time.time >= nextUpdate)
        {
            //Debug.Log(Time.time + ">=" + nextUpdate);
            // Change the next update (current second+1)
            nextUpdate = Mathf.FloorToInt(Time.time) + 1;
            // Call your function
            UpdateEverySecond();
        }
    }
    public float gameTick = 0;
    public float gameTickRealSeconds = 0;
    public float gameLag = 0;
    //public int delta = 0;

    private void UpdateEverySecond()
    {
        gameTick++;
       
    }

    float period = 0.01f;

    private void UpdateEveryTimeInterval(float time, float interval)
    {
    }

    public void AddHuntEventToList(string playerAccountString, string NPCToken, List<Vector3> currentPath, bool goingTowards, bool returningFrom, int startingTimeUnix, int endingTimeUnix)
    {
    }

    public List<Vector3> CreateCurrentPathFromOriginal(Vector3 currentPosition, List<Vector3> searchResultPath)
    {
        List<Vector3> currentPath = new List<Vector3>()
            {
                currentPosition
            };

        foreach (Vector3 v in searchResultPath)
            currentPath.Add(v);


        return currentPath;
    }


    float intervalDuration = 0.1f;
    const int COUNTER_MAX_VALUE = 8;
    const float TIME_TO_LERP = 1f; //lerp for 1 second
    const float GROUND_Y = 1f;
    float timeLerped = 0.0f;
    int counter = 0;

    public bool IsPointerOverUIObject()
    {
        UnityEngine.EventSystems.PointerEventData eventDataCurrentPosition = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }


    float time = 0f;
    public void FixedUpdate()
    {
        
        time = time + Time.fixedDeltaTime;
        if (time >= nextIntervalUpdate)
        {
            nextIntervalUpdate = time + 0.1f;
            if (time >= 1)
            {
                time = 0f;
                nextIntervalUpdate = 0.1f;
            }
                
            // Call your function
            UpdateEveryTimeInterval(time, 0.1f);
        }

        }
    }
 
