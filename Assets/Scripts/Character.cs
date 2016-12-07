using MMOServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Character
{
    private int charId;
    private int accountId;
    private string name;
    private ushort strength;
    private ushort agility;
    private ushort intellect;
    private ushort vitality;
    private ushort dexterity;

    public Character(SubPacket characterPacket)
    {
        CharacterQueryPacket cq = new CharacterQueryPacket();
        cq.ReadResponsePacket(characterPacket);
        charId = cq.GetCharId();
        accountId = cq.GetAccountId();
        name = cq.GetName();
        strength = cq.GetStrength();
        agility = cq.GetAgility();
        intellect = cq.GetIntellect();
        vitality = cq.GetVitalty();
        dexterity = cq.GetDexterity();
    }
}
