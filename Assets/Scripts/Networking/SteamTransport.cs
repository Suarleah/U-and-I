using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;
[DisallowMultipleComponent]
public class SteamTransport : NetworkTransport
{
    public static SteamTransport Instance { get; private set; }

    // Host side
    private SteamSocketManager socketManager;

    // Client side
    private SteamConnectionManager connectionManager;

    private readonly Dictionary<uint, ulong> handleToClientId = new();
    private readonly Dictionary<ulong, Connection> clientIdToConnection = new();
    private ulong nextClientId = 1; // 0 is reserved for ServerClientId

    private readonly Queue<(ulong clientId, NetworkEvent eventType, byte[] payload)> eventQueue = new();

    public override ulong ServerClientId => 0;


    private class SteamSocketManager : SocketManager
    {
        public SteamTransport transport;

        public override void OnConnecting(Connection connection, ConnectionInfo info)
        {
            connection.Accept();
        }

        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            ulong clientId = transport.nextClientId++;
            transport.handleToClientId[connection.Id] = clientId;
            transport.clientIdToConnection[clientId] = connection;
            transport.eventQueue.Enqueue((clientId, NetworkEvent.Connect, Array.Empty<byte>()));
            Debug.Log($"[SteamTransport] Client {clientId} connected ({info.Identity})");
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            if (!transport.handleToClientId.TryGetValue(connection.Id, out ulong clientId)) return;
            transport.handleToClientId.Remove(connection.Id);
            transport.clientIdToConnection.Remove(clientId);
            transport.eventQueue.Enqueue((clientId, NetworkEvent.Disconnect, Array.Empty<byte>()));
            Debug.Log($"[SteamTransport] Client {clientId} disconnected");
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size,
            long messageNum, long recvTime, int channel)
        {
            if (!transport.handleToClientId.TryGetValue(connection.Id, out ulong clientId)) return;
            byte[] buffer = new byte[size];
            Marshal.Copy(data, buffer, 0, size);
            transport.eventQueue.Enqueue((clientId, NetworkEvent.Data, buffer));
        }
    }

    private class SteamConnectionManager : ConnectionManager
    {
        public SteamTransport transport;

        public override void OnConnected(ConnectionInfo info)
        {
            transport.eventQueue.Enqueue((transport.ServerClientId, NetworkEvent.Connect, Array.Empty<byte>()));
            Debug.Log("[SteamTransport] Connected to host");
        }

        public override void OnDisconnected(ConnectionInfo info)
        {
            transport.eventQueue.Enqueue((transport.ServerClientId, NetworkEvent.Disconnect, Array.Empty<byte>()));
            Debug.Log("[SteamTransport] Disconnected from host");
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            byte[] buffer = new byte[size];
            Marshal.Copy(data, buffer, 0, size);
            transport.eventQueue.Enqueue((transport.ServerClientId, NetworkEvent.Data, buffer));
        }
    }

    public override void Initialize(NetworkManager networkManager = null)
    {
        Instance = this;
    }

    public override bool StartServer()
    {
        socketManager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);
        socketManager.transport = this;
        Debug.Log("[SteamTransport] Relay socket opened — hosting");
        return socketManager != null;
    }

    public override bool StartClient()
    {
        SteamId hostId = SteamLobbyManager.Instance.HostSteamId;
        connectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(hostId, 0);
        connectionManager.transport = this;
        Debug.Log($"[SteamTransport] Connecting to host {hostId}…");
        return connectionManager != null;
    }

    public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
    {
        SendType sendType = networkDelivery == NetworkDelivery.Unreliable
            ? SendType.Unreliable
            : SendType.Reliable;

        byte[] data = new byte[payload.Count];
        Array.Copy(payload.Array!, payload.Offset, data, 0, payload.Count);

        if (clientId == ServerClientId)
        {
            // Client → server
            connectionManager?.Connection.SendMessage(data, 0, data.Length, sendType);
        }
        else if (clientIdToConnection.TryGetValue(clientId, out Connection conn))
        {
            // Server → specific client
            conn.SendMessage(data, 0, data.Length, sendType);
        }
    }

    public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
    {
        // Pump Steam callbacks — this fires the OnMessage/OnConnected/etc. overrides above
        socketManager?.Receive();
        connectionManager?.Receive();

        receiveTime = Time.realtimeSinceStartup;

        if (eventQueue.TryDequeue(out var ev))
        {
            clientId = ev.clientId;
            payload  = ev.payload;
            return ev.eventType;
        }

        clientId = 0;
        payload  = default;
        return NetworkEvent.Nothing;
    }

    public override void DisconnectLocalClient()
    {
        connectionManager?.Connection.Close();
    }

    public override void DisconnectRemoteClient(ulong clientId)
    {
        if (clientIdToConnection.TryGetValue(clientId, out Connection conn))
            conn.Close();
    }

    public override ulong GetCurrentRtt(ulong clientId) => 0;

    public override void Shutdown()
    {
        socketManager?.Close();
        connectionManager?.Connection.Close();
        socketManager    = null;
        connectionManager = null;
        handleToClientId.Clear();
        clientIdToConnection.Clear();
        eventQueue.Clear();
        Debug.Log("[SteamTransport] Shut down");
    }
}
