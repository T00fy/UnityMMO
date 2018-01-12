using MMOServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Character : Actor
{
    public uint AccountId { get; set; }
    public string CharacterName { get; set; }
    public ushort Strength { get; set; }
    public ushort Agility { get; set; }
    public ushort Intellect { get; set; }
    public ushort Vitality { get; set; }
    public ushort Dexterity { get; set; }
    public ushort Slot { get; set; }

    void Start() { }
    
    public void SetCharacterInfoFromPacket(CharacterQueryPacket cq)
    {
        Id = cq.GetCharId();
        Slot = cq.GetCharacterSlot();
        AccountId = cq.GetAccountId();
        CharacterName = cq.GetName();
        Strength = cq.GetStrength();
        Agility = cq.GetAgility();
        Intellect = cq.GetIntellect();
        Vitality = cq.GetVitalty();
        Dexterity = cq.GetDexterity();
    }
}


