using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class GameEventArgs : EventArgs
{
    public bool serverResponse { get; set; }
    public bool statusBoxClosed { get; set; }
    public bool clientSelectedEnterWorld { get; set; }
}