﻿using System.Net.WebSockets;
using System.Text;

namespace AiPainter.Helpers;

static class WebsocketHelper
{
    public static async Task ReceiveMessageAsync(this WebSocket ws, Action<string> onTextMessage, Action<byte[]> onBinaryMessage)
    {
        if (ws.State != WebSocketState.Open) return;

        using var ms = new MemoryStream();

        WebSocketReceiveResult result;
        do 
        {
            var messageBuffer = WebSocket.CreateClientBuffer(1024, 16);
            result = await ws.ReceiveAsync(messageBuffer, CancellationToken.None);
            ms.Write(messageBuffer.Array!, messageBuffer.Offset, result.Count);
        }
        while (!result.EndOfMessage);

        switch (result.MessageType)
        {
            case WebSocketMessageType.Text:
                onTextMessage(Encoding.UTF8.GetString(ms.ToArray()));
                break;

            case WebSocketMessageType.Binary:
                onBinaryMessage(ms.ToArray());
                break;
        }
    }
}