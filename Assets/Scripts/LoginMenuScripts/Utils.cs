using MMOServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Utils
{
    private static string accountName;
    private static Dictionary<ushort, Character> characterDictionary;

    public static void SetCharacterDictionary(Dictionary<ushort, Character> temp)
    {
        characterDictionary = temp;
    }


    public static string GetAccountName()
    {
        return accountName;
    }

    public static void SetAccountName(string account)
    {
        accountName = account;
    }

    public static void ThrowDebugErrorToServer(Connection connection)
    {
        SubPacket sp = new SubPacket(GamePacketOpCode.DebugThrow, 0, 0, new byte[0], SubPacketTypes.ErrorPacket);
        connection.Send(BasePacket.CreatePacket(sp, true, false));
    }

    public static T FindComponentInChildWithTag<T>(GameObject parent, string tag)where T:Component{
        Transform t = parent.transform;
        foreach(Transform tr in t)
        {
            if(tr.tag == tag)
            {
                return tr.GetComponent<T>();
            }
        }
        return null;
    }

    public static GameObject FindSiblingGameObjectByName(GameObject currentObject, string name)
    {
        Transform parent = currentObject.transform.parent;
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public static GameObject FindSiblingGameObjectByTag(GameObject currentObject, string tag)
    {
        Transform parent = currentObject.transform.parent;
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public static Character GetCharacter(ushort slot)
    {
        Character character;
        var blah = characterDictionary.TryGetValue(slot, out character);

        if (blah)
        {
            return character;
        }
        Debug.Log("null value");
        return null;
    }

}
