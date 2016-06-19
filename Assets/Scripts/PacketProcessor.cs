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

    //general case when receiving any packet and deciding what to do with it
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
            var stdOut = System.Console.Out;
            var consoleOut = new System.IO.StringWriter();
            System.Console.SetOut(consoleOut);
            subPacket.debugPrintSubPacket();
            Debug.Log(consoleOut.ToString());
            System.Console.SetOut(stdOut);

            if (subPacket.header.type == (ushort)SubPacketTypes.GamePacket)
            {
                if (subPacket.gameMessage.opcode == (ushort)GamePacketOpCode.AccountSuccess)
                {
                    CursorInput.menuHandler.SetStatusText(Encoding.Unicode.GetString(subPacket.data));
                }
                if (subPacket.gameMessage.opcode == (ushort)GamePacketOpCode.AccountError)
                {
                    CursorInput.menuHandler.SetStatusText(Encoding.Unicode.GetString(subPacket.data));
                    
                }

            }
            CursorInput.menuHandler.SetDestroyStatusBox();

        }
    }

    public void LoginOrRegister(BasePacket packetToSend) {
        if (!packetToSend.isAuthenticated())
        {
            Connection connect = new Connection();
            connect.EstablishConnection(); //connection now established
            connect.Send(packetToSend);

        }




    }






}
