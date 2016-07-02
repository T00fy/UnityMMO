using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using MMOServer;
using System.Threading;
using System.Collections.Generic;

public class PacketProcessor {
    public Connection connect;

    /// <summary>
    /// Establishes the initial connection and sends the first login or registration packet
    /// </summary>
    /// <param name="packetToSend"></param>
    public void LoginOrRegister(BasePacket packetToSend)
    {
        if (!packetToSend.isAuthenticated())
        {
            Connection connect = new Connection();
            connect.EstablishConnection(); //connection now established
            connect.Send(packetToSend);

        }
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
        List<SubPacket> subPackets = receivedPacket.GetSubpackets();
        foreach (SubPacket subPacket in subPackets)
        {
            /*  var stdOut = System.Console.Out;
              var consoleOut = new System.IO.StringWriter();
              System.Console.SetOut(consoleOut);
              subPacket.debugPrintSubPacket();
              Debug.Log(consoleOut.ToString());
              System.Console.SetOut(stdOut);*/

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
                        CursorInput.menuHandler.LoggedInSuccessfully();
                        break;

                    default:
                        Debug.Log("Unknown or corrupted packet");
                        break;
                    }
            }

            CursorInput.menuHandler.SetDestroyStatusBox();

        }
    }




    






}
