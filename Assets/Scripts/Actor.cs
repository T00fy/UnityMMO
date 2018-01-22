using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour
{
    [SerializeField]
    private uint id;

    public uint Id
    {
        get
        {
            return id;
        }

        set
        {
            id = value;
        }
    }

    // Use this for initialization
    void Start()
    {

    }
}
