using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using MMOServer;
using System.Threading;
using System.Collections.Generic;

public static class PacketProcessor {
    private static Connection connect;
    public static bool isAuthenticated;

    /// <summary>
    /// Establishes the initial connection and sends the first login or registration packet
    /// </summary>
    /// <param name="packetToSend"></param>
    public static void LoginOrRegister(BasePacket packetToSend)
    {
        if (!packetToSend.isAuthenticated())
        {
            connect = new Connection();
            connect.EstablishConnection();
            connect.Send(packetToSend);

        }
    }

    /// <summary>
    /// Sends a character creation packet to the server
    /// </summary>
    /// <param name="characterCreationPacket"></param>
    public static void SendCharacterCreationPacket(BasePacket characterCreationPacket)
    {
            connect.Send(characterCreationPacket);
    }

    /// <summary>
    /// All incoming packets are handled through this and then directed to the appropriate function
    /// </summary>
    /// <param name="receivedPacket"></param>
    public static void ProcessPacket(BasePacket receivedPacket)
    {
        if (connect == null)
        {

        }

        if (receivedPacket == null) {
            Debug.Log("tis null");
        }

        if (!isAuthenticated && receivedPacket.isAuthenticated())
        {
            isAuthenticated = true;
        }


        List<SubPacket> subPackets = receivedPacket.GetSubpackets();
        foreach (SubPacket subPacket in subPackets)
        {
            /*  var stdOut = System.Console.Out;
              var consoleOut = new System.IO.StringWriter();
              System.Console.SetOut(consoleOut);
              subPacket.debugPrintSubPacket();
              Debug.Log(consoleOut.ToString());
              System.Console.SetOut(stdOut);*/
            DoAuthenticationChecks(receivedPacket, subPacket);

            if (!receivedPacket.isAuthenticated())
            {
                if (subPacket.header.type == (ushort)SubPacketTypes.GamePacket)
                {
                    if (subPacket.gameMessage.opcode == (ushort)GamePacketOpCode.AccountError)
                    {
                        ErrorPacket ep = new ErrorPacket();
                        ep.ReadPacket(subPacket.data);
                        string msg = ep.GetErrorMessage();
                        CursorInput.menuHandler.SetStatusText(msg);

                    }

                }
            }
            else
            {
                switch (subPacket.gameMessage.opcode)
                {
                    case ((ushort)GamePacketOpCode.AccountSuccess):
                        CursorInput.menuHandler.SetStatusText(Encoding.Unicode.GetString(subPacket.data));
                        isAuthenticated = true;
                        CursorInput.menuHandler.LoggedInSuccessfully();
                        break;

                    case ((ushort)GamePacketOpCode.RegisterSuccess):
                        CursorInput.menuHandler.SetStatusText(Encoding.Unicode.GetString(subPacket.data));
                        break;

                    case ((ushort)GamePacketOpCode.CreateCharacter):
                        CursorInput.menuHandler.SetStatusText(Encoding.Unicode.GetString(subPacket.data));
                        break;

                    default:
                        Debug.Log("Unknown or corrupted packet");
                        break;
                    }
            }

            CursorInput.menuHandler.SetDestroyStatusBox();

        }
    }

    private static void DoAuthenticationChecks(BasePacket receivedPacket, SubPacket subPacket)
    {

        if (isAuthenticated && !receivedPacket.isAuthenticated() && subPacket.gameMessage.opcode != (ushort)GamePacketOpCode.RegisterSuccess)
        {
            isAuthenticated = false;
        }
    }
}
