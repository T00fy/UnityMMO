using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Data
{
    public static uint SESSION_ID;
    public static uint CHARACTER_ID;
    public static string LOGIN_ADDRESS = "127.0.0.1";
    public static int LOGIN_PORT = 3425;
    public static string LOGIN_IP = LOGIN_ADDRESS + ":" + LOGIN_PORT;
    public static Character CHARACTER_ON_LOGIN;
    public static Dictionary<uint, Npc> drawnNpcs = new Dictionary<uint, Npc>();
    public static Dictionary<uint, Character> drawnCharacters = new Dictionary<uint, Character>();
}
