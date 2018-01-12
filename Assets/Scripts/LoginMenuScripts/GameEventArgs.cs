using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class GameEventArgs : EventArgs
{
    public bool ServerResponse { get; set; }
    public bool StatusBoxClosed { get; set; }
    public bool ClientSelectedEnterWorld { get; set; }
    public ActorWrapper Actor { get; set; }
}