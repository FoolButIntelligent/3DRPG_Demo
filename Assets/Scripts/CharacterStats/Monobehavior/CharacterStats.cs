using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack;
    public CharacterData_SO templateData;
    public CharacterData_SO CharacterData;
    public AttackData_SO AttackData;

    [HideInInspector]
    public bool isCritical;

    private void Awake()
    {
        if (templateData != null)
            CharacterData = Instantiate(templateData);
    }

    #region Read from Data_SO
    //直接从ScriptableData读取数值
    public int MaxHealth
    {
        get{if (CharacterData != null)return CharacterData.maxHealth;
            else return 0;}
        set{CharacterData.maxHealth = value; } 
    }

    public int CurrrentHealth
    {
        get
        {
            if (CharacterData != null) return CharacterData.currentHealth;
            else return 0;
        }
        set { CharacterData.currentHealth = value; }
    }

    public int BaseDefence
    {
        get
        {
            if (CharacterData != null) return CharacterData.baseDefence;
            else return 0;
        }
        set { CharacterData.baseDefence = value; }
    }

    public int CurrentDefence
    {
        get
        {
            if (CharacterData != null) return CharacterData.currentDefence;
            else return 0;
        }
        set { CharacterData.currentDefence = value; }
    }
    #endregion

    #region Character Combat

    public void TakeDamage(CharacterStats attacker,CharacterStats defender)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - defender.CurrentDefence,0);
        CurrrentHealth = Mathf.Max(CurrrentHealth-damage, 0);

        if(attacker.isCritical)
        {
            defender.GetComponent<Animator>().SetTrigger("hit");
        }

        //TODO: Update UI
        UpdateHealthBarOnAttack?.Invoke(CurrrentHealth, MaxHealth);
        //TODO: Update experience
        if (CurrrentHealth <= 0)
            attacker.CharacterData.UpdateExp(CharacterData.killPoint);
    }

    public void TakeDamage(int damage,CharacterStats defender)
    {
        int currentDmg = Mathf.Max(damage - defender.CurrentDefence, 0);
        CurrrentHealth = Mathf.Max(CurrrentHealth - currentDmg, 0);
        UpdateHealthBarOnAttack?.Invoke(CurrrentHealth, MaxHealth);
        if (CurrrentHealth <= 0)
            GameManager.Instance.playStats.CharacterData.UpdateExp(CharacterData.killPoint);
    }
    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(AttackData.minDamage, AttackData.maxDamage);
        if (isCritical)
        {
            coreDamage *= AttackData.criticalMultiplier;
            Debug.Log("暴击" + coreDamage);
        }
        return (int)coreDamage;
    }

    #endregion
}
