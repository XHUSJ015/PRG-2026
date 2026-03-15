using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GlobalGameData;

public class LevelIntroManager : MonoBehaviour
{
    public TextMeshProUGUI zoneNameText;
    public Image backgroundImage;

    void Start()
    {
        ZoneInfo currentZone = GlobalGameData.Instance.GetCurrentZoneInfo();

        zoneNameText.text = $"ZÓNA {GlobalGameData.Instance.currentZoneLevel}: {currentZone.zoneName}";
        backgroundImage.sprite = currentZone.zoneBackground;
    }

    public void StartNextLevelBattle()
    {
        SceneManager.LoadScene("BattleScene");
    }
}