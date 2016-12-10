using MMOServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Character : MonoBehaviour
{
    private int charId;
    private int accountId;
    private string characterName;
    private ushort strength;
    private ushort agility;
    private ushort intellect;
    private ushort vitality;
    private ushort dexterity;
    private ushort slot;

    void Start() { }
    
    public void SetCharacterInfoFromPacket(CharacterQueryPacket cq)
    {
        charId = cq.GetCharId();
        slot = cq.GetCharacterSlot();
        accountId = cq.GetAccountId();
        characterName = cq.GetName();
        strength = cq.GetStrength();
        agility = cq.GetAgility();
        intellect = cq.GetIntellect();
        vitality = cq.GetVitalty();
        dexterity = cq.GetDexterity();
    }

    public int CharId
    {
        get
        {
            return charId;
        }

        set
        {
            charId = value;
        }
    }

    public int AccountId
    {
        get
        {
            return accountId;
        }

        set
        {
            accountId = value;
        }
    }

    public ushort Strength
    {
        get
        {
            return strength;
        }

        set
        {
            strength = value;
        }
    }

    public ushort Agility
    {
        get
        {
            return agility;
        }

        set
        {
            agility = value;
        }
    }

    public ushort Intellect
    {
        get
        {
            return intellect;
        }

        set
        {
            intellect = value;
        }
    }

    public ushort Vitality
    {
        get
        {
            return vitality;
        }

        set
        {
            vitality = value;
        }
    }

    public ushort Dexterity
    {
        get
        {
            return dexterity;
        }

        set
        {
            dexterity = value;
        }
    }

    public ushort Slot
    {
        get
        {
            return slot;
        }

        set
        {
            slot = value;
        }
    }

    public string CharacterName
    {
        get
        {
            return characterName;
        }

        set
        {
            characterName = value;
        }
    }
}


