using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using static GameObjects.EnumManager;
using System.Linq;
//using GameObjects;
using System;
using Unity.VisualScripting;

public class MapSceneInitializer : MonoBehaviour
{
    public GameObject levelUpDialog;
    public TMP_Text myLevelUpLevel;
    [SerializeField]
    public GameObject tileDialog;
    [SerializeField]
    public GameObject npcDialog;
    [SerializeField]
    public GameObject attackDialog;
    [SerializeField]
    private GameObject squadDialog;

    public static GameObject TileDialog;
    public static GameObject AttackDialog;
    public static GameObject SquadDialog;
    public static Hex SelectedTile;
    public static TMP_Text NPCMonsterType;
    public static TMP_Text NPCLevel;
    public static NPC SelectedNPC;
    public static TMP_InputField AttackingCrew;
    public static Toggle IncludeLeader;
    public static TMP_Text NPCHPPercentage;

    public bool finishedSpawning;

    private HeroUnit selectedHeroUnit;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.currentScene = "MapScene";
        finishedSpawning = false;

        TileDialog = tileDialog;
        AttackDialog = attackDialog;
        SquadDialog = squadDialog;

        tileDialog.SetActive(false);
        attackDialog.SetActive(false);
        squadDialog.SetActive(false);


        StartCoroutine(LoadMapData());

        MovingHeroUnitSelectionEvent.Instance += OnMovingHeroClicked;
    }

    void OnDestroy()
    {
        MovingHeroUnitSelectionEvent.Instance -= OnMovingHeroClicked;
    }
    /// <summary>
    /// Coroutine/Method for loading map data depending
    /// depends on loading data from the server
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadMapData()
    {

        while (!GameManager.Instance.mapPlayersLoaded && !GameManager.Instance.mapMonstersLoaded)
        {
            // add image representing loading screen here
            yield return null;
        }

        ShowSpawnedUnits();
    }

    public void ShowSpawnedUnits()
    {
        finishedSpawning = true;
    }

    const float interval = 0.1f;
    float nextTime = 0;
    public GameObject infoPopup;
    // Update is called once per frame
    void Update()
    {

        if (Time.time >= nextTime)
        {
            //do something here every interval seconds
            CheckUnitEngagementList();
            nextTime += interval;
        }
    }

    public void CheckUnitEngagementList()
    {
        if (!finishedSpawning)
            return;
    }

    public void HideLevelUp()
    {
        levelUpDialog.SetActive(false);
        //myLevelUpLevel.text = MyLevel.ToString();
    }

    public static void ShowTileDialog(string coords)
    {
        //NPCDialog.SetActive(false);
        AttackDialog.SetActive(false);
        SquadDialog.SetActive(false);
        TileDialog.SetActive(true);

        GameObject go = GameObject.Find("Text_Rewards");
        TMP_Text gotext = go.GetComponent<TMP_Text>();
        gotext.text = coords;
    }

    public static void HideTileDialog()
    {
        SelectedTile = null;
        TileDialog.SetActive(false);
    }

    public static void ShowNPCDialog(string coords)
    {
        TileDialog.SetActive(false);
        //NPCDialog.SetActive(true);

        GameObject go = GameObject.Find("Text_Rewards");
        TMP_Text gotext = go.GetComponent<TMP_Text>();
        gotext.text = coords;
    }

    public static void HideNPCDialog()
    {
        //NPCDialog.SetActive(false);
    }

    public void SailToTile()
    {
        if (UnitManager.instance.PlayersTurn == false)
            return;

        if (GameManager.Instance.unitReturning == true)
        {
            TileDialog.SetActive(false);
            GameManager.Instance.infoPopup.GetComponent<ScriptPopupInformation>().popupTitle.text = "Message!";
            GameManager.Instance.infoPopup.GetComponent<ScriptPopupInformation>().popupMessage.text = "Captain, you cannot jump to another location while your crew is returning to ship!";
            GameManager.Instance.infoPopup.SetActive(true);
            return;
        }

        TileDialog.SetActive(false);
    }

    public void ScoutPrivateer()
    {
        string NPCToken = SelectedNPC.gameObject.name;
        Debug.Log($"{System.DateTime.Now} [Cleint based] {NPCToken} scouted");
    }

    private static bool IsPointerOverUIObject()
    {
        UnityEngine.EventSystems.PointerEventData eventDataCurrentPosition = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public static void PopupTileInfo(GameObject hexGO)
    {
        Hex tile = hexGO.GetComponent<Hex>();
        SelectedTile = tile;
        if (tile.CanMoveToTile())
            ShowTileDialog(ConvertCoords(hexGO.transform.position));//.HexCoords.ToString()); ;
        else
        {
            //if (IsPointerOverUIObject() != true)
            //{
            TileDialog.SetActive(false);
            //NPCDialog.SetActive(false);
            //}
        }
    }

    public static void PopupNPCInfo(GameObject npcGO)
    {
        NPC npc = npcGO.GetComponent<NPC>();
        //NPC tile = npcGO.GetComponent<Hex>();
        SelectedNPC = npc;

        //if (tile.CanMoveToTile())
        //    ShowTileDialog(tile.HexCoords.ToString());
        //else
        TileDialog.SetActive(false);
        //NPCDialog.SetActive(false);
    }
    public static void AttackOnNPCInfo(GameObject npc)
    {
        TileDialog.SetActive(false);
        SquadDialog.SetActive(false);
        AttackDialog.SetActive(true);
    }
    public static void ShowSquadPopup(bool value)
    {
        TileDialog.SetActive(false);
        AttackDialog.SetActive(false);
        SquadDialog.SetActive(value);
    }
    public static string ConvertCoords(Vector3 position)
    {
        Vector3Int pos = HexGrid.GetClosestHexCoords(position);

        return $"( {pos.x} , {pos.z} )";
    }
    private void OnMovingHeroClicked(HeroUnit heroUnit)
    {
        selectedHeroUnit = heroUnit;
        ShowSquadPopup(true);
    }
    public void OnFollowButtonPressed()
    {
        ShowSquadPopup(false);
        SetCameraTargetEvent.Instance?.Invoke(selectedHeroUnit.transform, true);
    }
    public void OnRecallButtonPressed()
    {
        ShowSquadPopup(false);
        RecallHeroUnitEvent.Instance?.Invoke(selectedHeroUnit);
    }
}
