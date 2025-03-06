using System;

[Serializable]
public struct GraphConnections
{
    
}


public struct Connection
{
    public string fromGUID;
    public string toGUID;
    public int fromPort;
    public int toPort;
}