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
    private string userName;
    private string password;
    private GameObject cursor;
    private IPAddress[] ip;


    //general case when receiving any packet and deciding what to do with it
    public void ProcessPacket(Connection clientConnection, BasePacket receivedPacket)
    {
        if (receivedPacket == null) {
            Debug.Log("tis null");
        }
        List<SubPacket> subPackets = receivedPacket.GetSubpackets();
        foreach (SubPacket subPacket in subPackets)
        {
            subPacket.debugPrintSubPacket();

            if (subPacket.header.type == (ushort)SubPacketTypes.Account)
            {
                Debug.Log(Encoding.Unicode.GetString(subPacket.data));
            }

        }
    }

    public void LoginOrRegister(BasePacket packetToSend, MenuHandler statusBox) {
        if (!packetToSend.isAuthenticated())
        {
            Connection connect = new Connection(statusBox);
            connect.EstablishConnection(); //connection now established
            connect.Send(packetToSend);

        }




    }






}
