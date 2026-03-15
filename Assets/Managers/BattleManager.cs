using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static GlobalGameData;

public class BattleManager : MonoBehaviour
{
    //Character/Player
    public Character[] allCharacters;
    public Image characterImage;
    public Slider characterHealthSlider;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI characterDamage;
    public TextMeshProUGUI characterArmor;
    public TextMeshProUGUI characterPerception;
    public TextMeshProUGUI characterAgility;
    public TextMeshProUGUI characterSpeed;
    public GameObject playerTemplate;
    private PlayerController playerController;

    //Enemies
    public Enemy[] allEnemies;
    public GameObject enemyPrefab;
    public int currentZoneLevel; 
    public Transform[] spawnPoints;
    public Transform enemyContainer;

    //Battle
    public Button[] actionButtons;
    public Button continueButton;
    private bool isSpecialAttackAvailable = true;
    private bool isSpecialAbilityAvailable = true;
    public BattleState currentState;
    public int playerActionPoints = 2;
    public TextMeshProUGUI apText;
    private EnemyController[] allEnemyControllers;
    private int currentEnemyIndex = 0;
    public PlayerActionType actionToPerform;
    public Image backgroundImage;
    public TextMeshProUGUI zoneName;
    public TextMeshProUGUI characterCombatText;

    //Loot
    public GameObject lootPopupPanel;
    public TextMeshProUGUI lootItemNameText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject playerInstance = Instantiate(playerTemplate, Vector3.zero, Quaternion.identity);
        playerController = playerInstance.GetComponent<PlayerController>();
        ZoneInfo currentZone = GlobalGameData.Instance.GetCurrentZoneInfo();
        backgroundImage.sprite = currentZone.zoneBackground;
        zoneName.text = currentZone.zoneName;

        Character.Class selectedClass = GlobalGameData.Instance.selectedClass;
        Character character = GlobalGameData.Instance.GetSelectedCharacter();
        Debug.Log(character);
        Debug.Log(character.health);
        isSpecialAttackAvailable = true;
        isSpecialAbilityAvailable = true;
        if (characterCombatText != null) characterCombatText.gameObject.SetActive(false);

        InitializePlayerCard(character);
        InitializeActionBar(character);
        GenerateEnemiesForCurrentZone();
        InitializeBattle();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Úvodní nastavení hráèovy karty
    private void InitializePlayerCard(Character character)
    {
        characterImage.sprite = character.sprite;
        characterName.text = character.name;
        characterDamage.text = $"Poškození: {character.currentWeapon.minDamage} - {character.currentWeapon.maxDamage}";
        characterArmor.text = $"Obrana: {character.currentArmor.minArmorValue} - {character.currentArmor.maxArmorValue}";
        characterPerception.text = $"Celková pøesnost: {character.perception + character.currentWeapon.accuracy}"; 
        characterAgility.text = $"Obratnost: {character.agility.ToString()}";
        characterSpeed.text = $"Rychlost: {character.speed.ToString()} m/tah";

        if (characterHealthSlider != null)
        {
            characterHealthSlider.maxValue = character.maxHealth;
            characterHealthSlider.value = character.health;
        }
        UpdatePlayerHealthUI(character.health);
        RefreshPlayerCard();
    }

    // Obnovení hráèovy karty pro správné vykreslování
    public void RefreshPlayerCard()
    {
        Character player = GlobalGameData.Instance.GetSelectedCharacter();
        if (player == null) return;

        if (characterDamage != null && player.currentWeapon != null)
        {
            characterDamage.text = $"Útok: {player.currentWeapon.minDamage}-{player.currentWeapon.maxDamage}";
            characterPerception.text = $"Celková pøesnost: {player.perception + player.currentWeapon.accuracy}";
        }

        if (characterArmor != null)
        {
            if (player.currentArmor != null)
            {
                characterArmor.text = $"Obrana: {player.currentArmor.minArmorValue}-{player.currentArmor.maxArmorValue}";
            }
        }
    }

    // Generování nepøátel pro pøíslušnou zónu
    void GenerateEnemiesForCurrentZone()
    {
        List<Enemy> availableEnemies = GetAvailableEnemies(GlobalGameData.Instance.currentZoneLevel);

        int maxSpawn = Mathf.Min(spawnPoints.Length, 3);
        int numberOfEnemiesToSpawn = Random.Range(2, maxSpawn + 1);

        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            Enemy randomEnemyData = availableEnemies[Random.Range(0, availableEnemies.Count)];

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPoints[i].position, Quaternion.identity);
            newEnemy.transform.SetParent(enemyContainer, false);
            newEnemy.transform.localScale = Vector3.one;
            newEnemy.transform.localPosition = spawnPoints[i].localPosition;
            Debug.Log(randomEnemyData.name);

            newEnemy.GetComponent<EnemyController>().Setup(randomEnemyData);
        }
    }

    // Získání všech nepøátel pro danou zónu (level)
    List<Enemy> GetAvailableEnemies(int currentLevel)
    {
        List<Enemy> enemies = new List<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.level == currentLevel)
            {
                enemies.Add(enemy);
            }
        }
        return enemies;
    }

    // Úvodní vytvoøení akèních tlaèítek pro hráèe
    private void InitializeActionBar(Character character)
    {
        SetButtonText(actionButtons[0], "Útok");
        SetButtonText(actionButtons[3], "Pøiblížit se");
        SetButtonText(actionButtons[4], "Vzdálit se");
        SetButtonText(actionButtons[5], "Prohledat");
  
        SetButtonText(actionButtons[1], character.specialAttackName);
        SetButtonText(actionButtons[2], character.specialAbilityName);

        actionButtons[0].onClick.AddListener(() => OnActionButtonClicked(PlayerActionType.Attack));
        actionButtons[1].onClick.AddListener(() => OnActionButtonClicked(PlayerActionType.SpecialAttack));
        actionButtons[2].onClick.AddListener(() => OnActionButtonClicked(PlayerActionType.SpecialAbility));
        actionButtons[3].onClick.AddListener(() => OnActionButtonClicked(PlayerActionType.MoveForward));
        actionButtons[4].onClick.AddListener(() => OnActionButtonClicked(PlayerActionType.MoveBackwards));
        actionButtons[5].onClick.AddListener(() => OnActionButtonClicked(PlayerActionType.Search));
    }

    // Pøedání typu použítého tlaèítka PlayerControlleru
    private void OnActionButtonClicked(PlayerActionType action)
    {
        if (playerController != null)
        {
            playerController.HandlePlayerAction(action);     
        }
    }

    // Logika kliku na tlaèítko pokraèovat po konci hry
    public void OnContinueButtonClicked()
    {
        if (GlobalGameData.Instance.currentRound < 3)
        {
            Debug.Log("Následuje další kolo");

            GlobalGameData.Instance.currentRound++;

            SceneManager.LoadScene("BattleScene");
        }
        else
        {

            if (GlobalGameData.Instance.currentZoneLevel >= 2 && GlobalGameData.Instance.currentRound >= 3)
            {
                Debug.Log("Konec prototypu");
                SceneManager.LoadScene("PrototypeEndScene");
            }
            else
            {
                Debug.Log("Level dokonèen");

                GlobalGameData.Instance.currentZoneLevel++;
                GlobalGameData.Instance.currentRound = 1;

                SceneManager.LoadScene("LevelIntroScene");
            }
                
        }
    }

    // Nastavení textu tlaèítek
    private void SetButtonText(Button button, string text)
    {
        if (button != null)
        {
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }
    }

    // ***********************************************              ***********************************************
    // *********************************************** Battle Logic ***********************************************
    // ***********************************************              ***********************************************


    // Poèáteèní spuštìní boje
    void InitializeBattle()
    {
        allEnemyControllers = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        UpdateBattleState(BattleState.PlayerTurn);
    }

    // Logika aktualizace stavu hry
    public void UpdateBattleState(BattleState newState)
    {
        if (currentState == BattleState.BattleEnd && newState != BattleState.BattleEnd)
            return;

        currentState = newState;
        Debug.Log($"Nový state: {currentState}");

        switch (newState)
        {
            case BattleState.PlayerTurn:
                SetUIActive(true);
                Debug.Log("Hráèùv tah");
                break;

            case BattleState.EnemyTurn:
                SetUIActive(false);
                currentEnemyIndex = 0;
                StartEnemyTurn();
                break;

            case BattleState.ActionResolution:
                break;

            case BattleState.BattleEnd:
                SetUIActive(false);

                if (actionButtons.Length > 5)
                {
                    actionButtons[5].interactable = true;
                }
                Debug.Log("Bitva skonèila");
                break;
        }
    }

    // Spuštìní tahu nepøátel
    void StartEnemyTurn()
    {
        allEnemyControllers = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        List<EnemyController> activeEnemies = new List<EnemyController>();
        foreach (var enemy in allEnemyControllers)
        {
            if (enemy.gameObject.activeSelf)
            {
                activeEnemies.Add(enemy);
            }
        }

        if (activeEnemies.Count == 0)
        {
            Debug.Log("Všichni nepøátelé poraženi");
            UpdateBattleState(BattleState.BattleEnd);
            return;
        }

        StartCoroutine(ExecuteEnemyTurns(activeEnemies));
    }

    // Samotné provedení akcí nepøátel (korutina)
    IEnumerator ExecuteEnemyTurns(List<EnemyController> enemies)
    {
        PlayerController player = FindObjectOfType<PlayerController>();

        foreach (EnemyController enemy in enemies)
        {
            if (!enemy.gameObject.activeSelf) continue;

            enemy.SetTurnHighlight(true);

            Debug.Log($"Tah nepøítele: {enemy.gameObject.name}");
            yield return StartCoroutine(enemy.HandleEnemyAction(player));

            enemy.SetTurnHighlight(false);

            if (GlobalGameData.Instance.GetSelectedCharacter().health <= 0)
            {
                Debug.Log("Hráè zemøel");
                UpdateBattleState(BattleState.BattleEnd);
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        playerActionPoints = 2;
        UpdateAPUI();
        UpdateBattleState(BattleState.PlayerTurn);
    }

    // Kontrola výhry hráèe
    public void CheckWinCondition()
    {
        EnemyController[] activeEnemies = FindObjectsOfType<EnemyController>();

        if (activeEnemies.Length == 0)
        {
            EndBattle(true);
        }
    }

    // Logika pro konec bitvy po poražení nepøátel
    public void EndBattle(bool playerWon)
    {
        if (playerWon)
        {
            Debug.Log("Všichni nepøátelé poraženi");
            UpdateBattleState(BattleState.BattleEnd);

            if (continueButton != null)
                continueButton.gameObject.SetActive(true);

            SetUIActive(false);

            if (actionButtons.Length > 5)
            {
                actionButtons[5].interactable = true;
            }
        }
        else
        {
            Debug.Log("Hráè zemøel");
            UpdateBattleState(BattleState.BattleEnd);

            SceneManager.LoadScene("DeathScene");
        }
    }

    // Odeètení AP za akci
    public void PayActionPoints(int amount)
    {
        playerActionPoints -= amount;
        UpdateAPUI();

        if (playerActionPoints <= 0 && currentState == BattleState.PlayerTurn)
        {

            InventoryManager im = FindObjectOfType<InventoryManager>();
            if (im != null)
            {
                im.CloseInventory();
            }

            UpdateBattleState(BattleState.ActionResolution);
            StartCoroutine(TransitionToStateAfterDelay(1.5f, BattleState.EnemyTurn));
        }
    }

    // Zapnutí UI tlaèítek
    private void SetUIActive(bool active)
    {
        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (i == 5)
            {
                actionButtons[i].interactable = false;
                continue;
            }

            if (active)
            {
                if (i == 1 && !isSpecialAttackAvailable)
                {
                    actionButtons[i].interactable = false;
                }
                else if (i == 2 && !isSpecialAbilityAvailable)
                {
                    actionButtons[i].interactable = false;
                }
                else
                {
                    actionButtons[i].interactable = true;
                }
            }
            else
            {
                actionButtons[i].interactable = false;
            }
        }
    }
    // Obnova UI pro hráèovy životy
    public void UpdatePlayerHealthUI(int currentHealth)
    {
        if (characterHealthSlider != null)
        {
            characterHealthSlider.value = currentHealth;
        }
    }

    // Obnova UI pro AP
    public void UpdateAPUI()
    {
        if (apText != null)
        {
            apText.text = playerActionPoints.ToString();
        }
    }

    // Vynputí akèních tlaèítek
    public void DisableSpecialButton(bool isAttack)
    {
        int buttonIndex = isAttack ? 1 : 2;

        if (buttonIndex < actionButtons.Length)
        {
            actionButtons[buttonIndex].interactable = false;
        }

        if (isAttack)
        {
            isSpecialAttackAvailable = false;
        }
        else
        {
            isSpecialAbilityAvailable = false;
        }
    }

    // Návrat to stavu po prodlevì
    public IEnumerator TransitionToStateAfterDelay(float delay, BattleState nextState)
    {
        yield return new WaitForSeconds(delay);
        UpdateBattleState(nextState);
    }

    // Zobrazí combat textu (údaj o provedení akce)
    public void ShowPlayerCombatText(string message, Color color)
    {
        if (characterCombatText != null)
        {
            StartCoroutine(PlayerCombatTextRoutine(message, color));
        }
    }

    // Korutina pro hráèùv combat text
    private IEnumerator PlayerCombatTextRoutine(string message, Color color)
    {
        characterCombatText.text = message;
        characterCombatText.color = color;
        characterCombatText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.2f);

        characterCombatText.gameObject.SetActive(false);
    }

    // Akce
    public enum PlayerActionType
    {
        Attack,
        SpecialAttack,
        SpecialAbility,
        MoveForward,
        MoveBackwards,
        Search
    }

    // Stavy
    public enum BattleState
    {
        StartBattle,      
        PlayerTurn,
        SelectingTarget,
        EnemyTurn,        
        ActionResolution, 
        BattleEnd         
    }

    // ***********************************************      ***********************************************
    // *********************************************** Loot ***********************************************
    // ***********************************************      ***********************************************


    // Logika po kliku na tlaèítko prohledat
    public void OnSearchButtonClicked()
    {
        int currentLevel = GlobalGameData.Instance.currentZoneLevel;

        List<Weapon> possibleWeapons = GlobalGameData.Instance.allAvailableWeapons.FindAll(w => w.level == currentLevel);
        List<Armor> possibleArmors = GlobalGameData.Instance.allAvailableArmors.FindAll(a => a.level == currentLevel);

        string itemName = "";
        bool isWeapon = Random.Range(0, 2) == 0;

        if (isWeapon && possibleWeapons.Count > 0)
        {
            Weapon foundWeapon = possibleWeapons[Random.Range(0, possibleWeapons.Count)];
            GlobalGameData.Instance.playerWeapons.Add(foundWeapon);
            itemName = foundWeapon.name;
        }
        else if (possibleArmors.Count > 0)
        {
            Armor foundArmor = possibleArmors[Random.Range(0, possibleArmors.Count)];
            GlobalGameData.Instance.playerArmors.Add(foundArmor);
            itemName = foundArmor.name;
        }

        if (itemName != "")
        {
            lootItemNameText.text = itemName;
            lootPopupPanel.SetActive(true);

            actionButtons[5].interactable = false;
        }
    }


    // Zavøení vyskakovací okna pro loot
    public void CloseLootPopup()
    {
        lootPopupPanel.SetActive(false);
    }
}
