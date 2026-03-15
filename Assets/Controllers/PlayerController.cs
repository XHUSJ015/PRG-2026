using UnityEngine;
using static BattleManager;

public class PlayerController : MonoBehaviour
{
    private Character.Class playerClass;
    private BattleManager battleManager;

    private bool isSpecialAttackUsed = false;
    private bool isSpecialAbilityUsed = false;

    void Start()
    {
        playerClass = GlobalGameData.Instance.selectedCharacterData.class_;
        battleManager = FindObjectOfType<BattleManager>();
    }

    void Update()
    {
        
    }

    // Logika provedení vybraných akcí hráče
    public void HandlePlayerAction(PlayerActionType action)
    {
        BattleManager manager = FindObjectOfType<BattleManager>();

        bool canSearch = action == PlayerActionType.Search && manager.currentState == BattleState.BattleEnd;

        if (manager.currentState != BattleState.PlayerTurn && !canSearch)
        {
            Debug.Log("Není tvůj tah");
            return;
        }

        if (action == PlayerActionType.SpecialAttack && isSpecialAttackUsed) { Debug.Log("Speciální útok již použit"); return; }
        if (action == PlayerActionType.SpecialAbility && isSpecialAbilityUsed) { Debug.Log("Speciální schopnost již použita"); return; }

        bool needsTarget = false;

        switch (action)
        {
            case PlayerActionType.Attack:
            case PlayerActionType.SpecialAttack:
                needsTarget = true;
                break;

            case PlayerActionType.SpecialAbility:
                if (playerClass == Character.Class.Vojín) needsTarget = true;
                else needsTarget = false;
                break;
        }

        if (needsTarget)
        {
            manager.actionToPerform = action;
            manager.UpdateBattleState(BattleState.SelectingTarget);
            Debug.Log("Vyber cíl");
        }
        else
        {
            ExecuteActionNoTarget(action);
        }
    }

    // Provedení akce která nevyžaduje cíl
    private void ExecuteActionNoTarget(PlayerActionType action)
    {
        BattleManager manager = FindObjectOfType<BattleManager>();
        Character playerCharacter = GlobalGameData.Instance.GetSelectedCharacter();

        if (action == PlayerActionType.Search)
        {
            manager.OnSearchButtonClicked();
            return;
        }

        if (!PayActionPoints(1)) return;

        if (action == PlayerActionType.MoveForward)
        {
            manager.ShowPlayerCombatText("PŘIBLÍŽENÍ", Color.green);
            Move(5);
        }
        else if (action == PlayerActionType.MoveBackwards)
        {
            manager.ShowPlayerCombatText("ÚSTUP", Color.green);
            Move(-5);
        }

        else if (action == PlayerActionType.SpecialAbility)
        {
            MarkAbilityAsUsed();

            switch (playerClass)
            {
                case Character.Class.Kobra:
                    SpecialAbilityKobra();
                    break;
                case Character.Class.Řezník:
                    SpecialAbilityReznik();
                    break;
            }
        }
        CheckTurnEnd();
    }

    // Provedení akce který vyžaduje cíl
    public void ExecuteActionOnTarget(PlayerActionType action, EnemyController target)
    {
        Character playerCharacter = GlobalGameData.Instance.GetSelectedCharacter();
        if (playerCharacter == null) return;

        int hitChance = CalculateHitChance(target);

        if (playerClass == Character.Class.Kobra && action == PlayerActionType.SpecialAttack)
        {
            hitChance = 100;
        }

        bool hit = Random.Range(1, 101) <= hitChance;

        if (hit)
        {
            int damage = 0;

            if (action == PlayerActionType.Attack)
            {
                damage = CalculateDamage(playerCharacter, target);
            }
            else if (action == PlayerActionType.SpecialAttack)
            {
                MarkAttackAsUsed();
                switch (playerClass)
                {
                    case Character.Class.Vojín:
                        SpecialAttackVojin(playerCharacter);
                        return;

                    case Character.Class.Kobra:
                        damage = CalculateDamage(playerCharacter, target);
                        Debug.Log($"Naváděná střela použita");
                        break;

                    case Character.Class.Řezník:
                        damage = CalculateDamage(playerCharacter, target, ignoreArmor: true);
                        Debug.Log($"Průrazná ráana použita");
                        break;
                }
            }
            else if (action == PlayerActionType.SpecialAbility)
            {
                MarkAbilityAsUsed();
                if (playerClass == Character.Class.Vojín)
                {
                    damage = CalculateDamage(playerCharacter, target) * 2;
                    Debug.Log($"Bolestivý zásah použit");
                }
            }

            target.ShowCombatText($"ZÁSAH -{damage}", Color.red);
            target.TakeDamage(damage);
            Debug.Log($"Zásah za {damage}");
        }
        else
        {
            target.ShowCombatText("MINUTÍ", Color.white);
            Debug.Log("Minul jsi");
            if (action == PlayerActionType.SpecialAttack) MarkAttackAsUsed();
            if (action == PlayerActionType.SpecialAbility) MarkAbilityAsUsed();
        }

        if (battleManager.currentState != BattleManager.BattleState.BattleEnd)
        {
            PayActionPoints(1);
            CheckTurnEnd();
        }
    }

    // Úbytek AP za provedení akce
    private bool PayActionPoints(int cost)
    {
        if (battleManager.playerActionPoints >= cost)
        {
            battleManager.playerActionPoints -= cost;
            battleManager.UpdateAPUI();
            return true;
        }
        Debug.Log("Nedostatek AP");
        return false;
    }

    // Kontrola konce tahu
    private void CheckTurnEnd()
    {
        if (battleManager.currentState == BattleState.BattleEnd) return;

        if (battleManager.playerActionPoints <= 0)
        {
            battleManager.UpdateBattleState(BattleState.ActionResolution);
            battleManager.StartCoroutine(battleManager.TransitionToStateAfterDelay(1.5f, BattleState.EnemyTurn));
        }
        else
        {
            battleManager.UpdateBattleState(BattleState.PlayerTurn);
        }
    }

    // Vypnutí tlačítka použitého útoku
    private void MarkAttackAsUsed()
    {
        isSpecialAttackUsed = true;
        battleManager.DisableSpecialButton(true);
    }

    // Vypnutí tlačítka použité schopnosti
    private void MarkAbilityAsUsed()
    {
        isSpecialAbilityUsed = true;
        battleManager.DisableSpecialButton(false);
    }


    // Výpočet šance na zásah nepřítele
    public int CalculateHitChance(EnemyController target)
    {
        Character playerCharacter = GlobalGameData.Instance.GetSelectedCharacter();
        int playerPerception = playerCharacter.perception;
        int weaponAccuracy = playerCharacter.currentWeapon.accuracy;
        int enemyAgility = target.GetAgility();
        int distance = target.GetDistance();

        int baseChance = playerPerception + weaponAccuracy;

        const float maxDistance = 51f;
        float distanceBonus = (maxDistance - distance) * 1.5f;

        int finalChance = Mathf.RoundToInt(baseChance + distanceBonus - enemyAgility);
        finalChance = Mathf.Clamp(finalChance, 5, 95);

        return finalChance;
    }

    // Výpočet poškození postavy hráče
    private int CalculateDamage(Character playerCharacter, EnemyController target, bool ignoreArmor = false)
    {
        int rawDmg = Random.Range(playerCharacter.currentWeapon.minDamage, playerCharacter.currentWeapon.maxDamage + 1);
        int reduction = ignoreArmor ? 0 : target.GetArmor();
        return Mathf.Max(1, rawDmg - reduction);
    }


    // Přijmutí poškození od hráče
    public void TakeDamage(int damageAmount)
    {
        Character playerCharacter = GlobalGameData.Instance.GetSelectedCharacter();
        if (playerCharacter == null) return;

        playerCharacter.health -= damageAmount;
        battleManager.UpdatePlayerHealthUI(playerCharacter.health);


        Debug.Log($"Obdrženo {damageAmount} poškození (HP: {playerCharacter.health})");

        if (playerCharacter.health <= 0)
        {
            Debug.Log("Zemřel jsi");
            battleManager.EndBattle(false);
        }
    }


    // Speciální útok vojína
    private void SpecialAttackVojin(Character playerCharacter)
    {
        EnemyController[] allEnemies = FindObjectsOfType<EnemyController>();

        foreach (EnemyController enemy in allEnemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                int baseDmg = CalculateDamage(playerCharacter, enemy);
                int grenadeDmg = Mathf.RoundToInt(baseDmg * 0.8f);

                if (grenadeDmg < 1) grenadeDmg = 1;

                enemy.ShowCombatText($"ZÁSAH -{grenadeDmg}", Color.red);
                enemy.TakeDamage(grenadeDmg);
                Debug.Log($"Granát zasáhl za {grenadeDmg}.");
            }
        }

        if (battleManager.currentState != BattleManager.BattleState.BattleEnd)
        {
            PayActionPoints(1);
            CheckTurnEnd();
        }
    }

    // Speciální schopnost kobry
    private void SpecialAbilityKobra()
    {
        Move(10);
        Debug.Log("Taktický kotoul použit - pohyb o 10m");
    }

    // Speciální schopnost řezníka
    private void SpecialAbilityReznik()
    {
        Character player = GlobalGameData.Instance.GetSelectedCharacter();

        int healAmount = 15;

        player.health += healAmount;

        if (player.health > player.maxHealth)
        {
            player.health = player.maxHealth;
        }

        battleManager.UpdatePlayerHealthUI(player.health);
        Debug.Log($"Vyléčeno o {healAmount} (HP: {player.health})");
    }

    // Logika pohybu
    private void Move(int distance)
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();

        foreach (EnemyController enemy in enemies)
        {
            enemy.UpdateDistance(distance);
            enemy.UpdateHitChanceUI();
        }
        Debug.Log($"Hráč se posunul o {Mathf.Abs(distance)}m");
    }
}
