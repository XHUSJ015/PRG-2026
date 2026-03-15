using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static BattleManager;

public class EnemyController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image enemySprite;
    public TextMeshProUGUI nameplateText;
    public Slider healthSlider;

    private BattleManager battleManager;
    private PlayerController playerController;
    private Image enemyCardImage;
    public Image enemyCard;
    public TextMeshProUGUI enemyDistance;
    public TextMeshProUGUI hitChance;
    public TextMeshProUGUI enemyDamage;
    public TextMeshProUGUI enemyArmor;
    public TextMeshProUGUI enemyPerception;
    public TextMeshProUGUI enemyAgility;
    public TextMeshProUGUI enemySpeed;
    public TextMeshProUGUI combatText;

    public Color highlightColor = Color.red;
    public Color higlightActionColor = Color.blue;
    private Color originalColor;

    private string enemyName;
    private int currentHealth;
    private int health;
    private int perception;
    private int speed;
    private int agility;
    private int distance;
    private int minDamage;
    private int maxDamage;
    private int minArmor;
    private int maxArmor;
    private bool isMelee;

    public void Setup(Enemy enemyData)
    {
        this.enemyName = enemyData.name;
        this.currentHealth = enemyData.health;
        this.health = enemyData.health;
        this.perception = enemyData.perception;
        this.speed = enemyData.speed;
        this.agility = enemyData.agility;
        this.distance = Random.Range(5, 51);
        this.minDamage = enemyData.minDamage;
        this.maxDamage = enemyData.maxDamage;
        this.minArmor = enemyData.minArmor;
        this.maxArmor = enemyData.maxArmor;
        this.isMelee = enemyData.melee;
        this.enemySprite.sprite = enemyData.enemySprite;
        this.nameplateText.text = enemyData.name;
        Debug.Log(distance);
        this.healthSlider.maxValue = this.health;
        this.healthSlider.value = this.currentHealth;
        this.enemyDistance.text = $"Vzdálenost: {this.distance}m";
        enemyDamage.text = $"Poškození: {enemyData.minDamage} - {enemyData.maxDamage}";
        enemyArmor.text = $"Obrana: {enemyData.minArmor} - {enemyData.maxArmor}";
        enemyPerception.text = $"Pøesnost: {enemyData.perception.ToString()}";
        enemyAgility.text = $"Obratnost: {enemyData.agility.ToString()}";
        enemySpeed.text = $"Rychlost: {enemyData.speed.ToString()} m/tah";
        if (combatText != null) combatText.gameObject.SetActive(false);

        Debug.Log("Nepøítel vygenerován: " + enemyName);

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        battleManager = FindObjectOfType<BattleManager>();
        playerController = FindObjectOfType<PlayerController>();
        UpdateHitChanceUI();

        if (enemyCard != null)
        {
            originalColor = enemyCard.color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Zpracování tahu nepøítele
    public IEnumerator HandleEnemyAction(PlayerController player)
    {
        int ap = 2;
        Character playerCharacter = GlobalGameData.Instance.GetSelectedCharacter();

        if (playerCharacter == null || player == null) yield break;

        Debug.Log($"Tah nepøítele: {this.enemyName}");

        while (ap > 0 && playerCharacter.health > 0)
        {
            if (isMelee)
            {
                if (this.distance > 5)
                {
                    MoveTowardsPlayer();
                }
                else
                {
                    PerformAttack(player, playerCharacter);
                }
            }
            else
            {
                if (this.distance < 20)
                {
                    if (ap == 2) MoveAwayFromPlayer();
                    else PerformAttack(player, playerCharacter);
                }
                else
                {
                    PerformAttack(player, playerCharacter);
                }
            }
            ap--;

            yield return new WaitForSeconds(1.5f);
        }
    }

    // Zvýraznení kartièky nepøítele pøi najetí myší po výbìru útoku
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (battleManager != null && battleManager.currentState == BattleState.SelectingTarget && enemyCard != null)
        {
            enemyCard.color = highlightColor;
        }
    }

    // Reset zvýraznìní
    public void OnPointerExit(PointerEventData eventData)
    {
        if (enemyCard != null)
        {
            enemyCard.color = originalColor;
        }
    }

    // Výbìr nepøítele po klikutí vybraným útokem
    public void OnPointerClick(PointerEventData eventData)
    {
        if (battleManager != null && battleManager.currentState == BattleState.SelectingTarget)
        {
            enemyCard.color = originalColor;

            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.ExecuteActionOnTarget(battleManager.actionToPerform, this);
            }
        }
    }

    // Provedení útoku nepøítele
    private void PerformAttack(PlayerController player, Character playerCharacter)
    {
        int hitChance = CalculateEnemyHitChance(playerCharacter);

        bool hit = Random.Range(1, 101) <= hitChance;

        if (hit)
        {
            int damage = CalculateEnemyDamage(playerCharacter);
            battleManager.ShowPlayerCombatText($"ZÁSAH -{damage}", Color.red);
            player.TakeDamage(damage);
            Debug.Log($"{this.enemyName} zasáhl za {damage} (Šance: {hitChance}%)");
        }
        else
        {
            battleManager.ShowPlayerCombatText("MINUTÍ", Color.white);
            Debug.Log($"{this.enemyName} minul (Šance: {hitChance}%)");
        }
    }

    // Pøijmutí poškození od hráèe
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        healthSlider.value = currentHealth;
        
        Debug.Log($"{enemyName} obdržel {damageAmount} poškození (HP: {currentHealth})");

        if (currentHealth <= 0)
        {
            Debug.Log($"{enemyName} zemøel");
            gameObject.SetActive(false);
            battleManager.CheckWinCondition();
        }
    }

    // Kalkulace šance na zásah hráèe
    private int CalculateEnemyHitChance(Character playerCharacter)
    {
        int enemyPerception = this.perception;
        int playerAgility = playerCharacter.agility;
        int distance = this.distance;

        const float maxDistance = 51f;
        float distanceBonus = (maxDistance - distance) * 1.5f;

        int finalChance = Mathf.RoundToInt(enemyPerception + distanceBonus - playerAgility);

        finalChance = Mathf.Clamp(finalChance, 5, 95);

        return finalChance;
    }

    // Kalkulace udìleného poškození nepøítele
    private int CalculateEnemyDamage(Character playerCharacter)
    {
        int rawDamage = Random.Range(this.minDamage, this.maxDamage + 1);

        int playerArmorValue = Random.Range(playerCharacter.currentArmor.minArmorValue, playerCharacter.currentArmor.maxArmorValue + 1);

        int finalDamage = rawDamage - playerArmorValue;

        return Mathf.Max(1, finalDamage);
    }

    // Obnovení hodnoty vzdálenosti
    public void UpdateDistance(int change)
    {
        int newDistance = this.distance - change;

        this.distance = Mathf.Clamp(newDistance, 1, 50);

        if (enemyDistance != null)
        {
            enemyDistance.text = $"Vzdálenost: {this.distance}m";
        }

        UpdateHitChanceUI();

        Debug.Log($"{gameObject.name} je nyní vzdálen {this.distance}m.");
    }


    // Obnovení UI pro procentuální šanci na zásah
    public void UpdateHitChanceUI()
    {
        if (playerController == null || hitChance == null) { return; }

        int chance = playerController.CalculateHitChance(this);

        hitChance.text = $"{chance}%";
    }

    // Zvýraznení nepøitelovi kartièky pøi jeho tahu
    public void SetTurnHighlight(bool isActive)
    {
        if (enemyCard != null)
        {
            if (isActive)
            {
                enemyCard.color = higlightActionColor;
            }
            else
            {
                enemyCard.color = originalColor;
            }
        }
    }

    // Pohyb dopøedu
    private void MoveTowardsPlayer()
    {
        UpdateDistance(this.speed);
        UpdateHitChanceUI();
        ShowCombatText("PØIBLÍŽENÍ", Color.green);
        Debug.Log($"{this.enemyName} se pøiblížil");
    }

    // Pohyb dozadu
    private void MoveAwayFromPlayer()
    {
        UpdateDistance(-this.speed);
        UpdateHitChanceUI();
        ShowCombatText("ÚSTUP", Color.green);
        Debug.Log($"{this.enemyName} ustoupil");
    }


    // Zobrazení combat textu pro nepøítele
    public void ShowCombatText(string message, Color color)
    {
        if (combatText != null)
        {
            StartCoroutine(CombatTextRoutine(message, color));
        }
    }

    // Korutina pro combat text nepøítele
    private IEnumerator CombatTextRoutine(string message, Color color)
    {
        combatText.text = message;
        combatText.color = color;
        combatText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.2f);

        combatText.gameObject.SetActive(false);
    }
    public int GetAgility()
    {
        return this.agility;
    }

    public int GetDistance()
    {
        return this.distance;
    }

    public int GetArmor()
    {
        return Random.Range(this.minArmor, this.maxArmor + 1);
    }
}
