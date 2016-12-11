using UnityEngine;
using System;
using System.Text;
using MMOServer;
using System.Collections.Generic;


//going to have to change this whole structure so its not a static class and uses monobehaviour ;_;
public class PacketProcessor : MonoBehaviour{
    public static Connection connect;
    public static bool isAuthenticated;
    public static bool loggedInSuccessfully;
    public CharacterLoader characterLoader;


    /// <summary>
    /// Establishes the initial connection and sends the first login or registration packet
    /// </summary>
    /// <param name="packetToSend"></param>
    public void LoginOrRegister(BasePacket packetToSend)
    {
        if (!packetToSend.isAuthenticated())
        {
            connect = GameObject.FindGameObjectWithTag("Connection").GetComponent<Connection>();
            connect.EstablishConnection();
            connect.Send(packetToSend);

        }
    }

    /// <summary>
    /// Sends a packet to the server
    /// </summary>
    /// <param name="characterCreationPacket"></param>
    public void SendPacket(BasePacket packet)
    {
            connect.Send(packet);
    }

    /// <summary>
    /// All incoming packets are handled through this and then directed to the appropriate function
    /// </summary>
    /// <param name="receivedPacket"></param>
    public void ProcessPacket(BasePacket receivedPacket)
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
                if (subPacket.header.type == (ushort)SubPacketTypes.ErrorPacket)
                {
                    if (subPacket.gameMessage.opcode == (ushort)GamePacketOpCode.AccountError)
                    {
                        ErrorPacket ep = new ErrorPacket();
                        ep.ReadPacket(subPacket.data);
                        string msg = ep.GetErrorMessage();
                        StatusBoxHandler.statusText = msg;
                        Debug.Log("fam error packet");
                        Utils.SetAccountName(null);
                        StatusBoxHandler.readyToClose = true;

                    }
                }

                if (subPacket.gameMessage.opcode == (ushort)GamePacketOpCode.RegisterSuccess)
                {
                    StatusBoxHandler.statusText = Encoding.Unicode.GetString(subPacket.data);
                    StatusBoxHandler.readyToClose = true;
                    break;
                }
            }
            else
            {
                if (subPacket.header.type == (ushort)SubPacketTypes.ErrorPacket)
                {
                    switch (subPacket.gameMessage.opcode)
                    {
                        case ((ushort)GamePacketOpCode.CreateCharacterError):
                            {
                                StatusBoxHandler.statusText = "Character name has already been taken";
                                StatusBoxHandler.readyToClose = true;
                                break;
                            }
                    }
                }
                switch (subPacket.gameMessage.opcode)
                {
                    case ((ushort)GamePacketOpCode.AccountSuccess):
                        StatusBoxHandler.statusText = Encoding.Unicode.GetString(subPacket.data);
                        isAuthenticated = true;
                        loggedInSuccessfully = true;
                        StatusBoxHandler.readyToClose = true;
                        break;

                    case ((ushort)GamePacketOpCode.CreateCharacter):
                        StatusBoxHandler.statusText = Encoding.Unicode.GetString(subPacket.data);
                        StatusBoxHandler.readyToClose = true;
                        break;

                    case ((ushort)GamePacketOpCode.CreateCharacterSuccess):
                        StatusBoxHandler.statusText = Encoding.Unicode.GetString(subPacket.data);
                        StatusBoxHandler.readyToClose = true;
                        break;

                    case ((ushort)GamePacketOpCode.CharacterListQuery):
                        if (BitConverter.ToInt32(subPacket.data, 0) == -1)
                        {
                            CharacterLoader.serverResponseFinished = true;
                            break;
                        }
                        else
                        {
                            characterLoader.SetCharacterListFromServer(subPacket);
                            //else send each received packet to CharacterLoader to process
                        }

                        break;

                    case ((ushort)GamePacketOpCode.CharacterDeleteSuccess):
                        StatusBoxHandler.statusText = Encoding.Unicode.GetString(subPacket.data);
                        StatusBoxHandler.readyToClose = true;
                        break;

                    default:
                        Debug.Log("Unknown or corrupted packet");
                        break;
                    }
            }

     //       StatusBoxHandler.readyToClose = true;

        }
    }

    private void DoAuthenticationChecks(BasePacket receivedPacket, SubPacket subPacket)
    {

        if (isAuthenticated && !receivedPacket.isAuthenticated() && subPacket.gameMessage.opcode != (ushort)GamePacketOpCode.RegisterSuccess)
        {
            isAuthenticated = false;
        }
    }
}
