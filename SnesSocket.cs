using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace SuperVision
{
    public class Snessocket
    {
        private ClientWebSocket ws;

        public Snessocket()
        {
            ws = new ClientWebSocket();
        }

        public enum Commands
        {
            DeviceList,
            Name,
            GetAddress,
            Reset,
            Attach,
            Info
        }

        public bool wsConnect()
        {
            var token = new CancellationTokenSource(3000);
            ws.ConnectAsync(new Uri("ws://localhost:23074"), token.Token).GetAwaiter().GetResult();
            return ws.State == WebSocketState.Open;
        }

        public void wsDisconnect()
        {
            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None).GetAwaiter().GetResult();
        }

        private Qresponse waitResponse(int to)
        {
            byte[] buffer = new byte[1024];
            var segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
            WebSocketReceiveResult wsRes;

            var task = ws.ReceiveAsync(segment, CancellationToken.None);
            int timeoutcounter = 0;
            while (!task.IsCompleted && timeoutcounter < to / 10)
            {
                Thread.Sleep(5);
                timeoutcounter++;
            }
            wsRes = task.Result;
            string wsResMsg = Encoding.UTF8.GetString(buffer.Take(wsRes.Count).ToArray());

            return JsonSerializer.Deserialize<Qresponse>(wsResMsg);
        }

        private void SendCommand(Commands com, string[] args)
        {
            Qrequest qreq = new Qrequest
            {
                Opcode = com.ToString(),
                Space = "SNES",
                Operands = args
            };
            string json = JsonSerializer.Serialize(qreq);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None).GetAwaiter().GetResult();
        }

        public string[] GetDevices()
        {
            SendCommand(Commands.DeviceList, [""]);
            Qresponse resp = waitResponse(1000);

            return resp.Results;
        }
        public string[] GetInfo()
        {
            SendCommand(Commands.Info, [""]);
            Qresponse resp = waitResponse(1000);

            return resp.Results;
        }

        public void Attach(string device)
        {
            SendCommand(Commands.Attach, [device]);
        }

        public void Name(string name)
        {
            SendCommand(Commands.Name, [name]);
        }

        public byte[] GetAddress(uint address, uint size)
        {
            SendCommand(Commands.GetAddress, [address.ToString("X"), size.ToString("X")]);
            byte[] result = new byte[size];
            byte[] ReadData = new byte[1024];

            //could maybe forloop this one
            int count = 0;
            while (count < size)
            {
                var task = ws.ReceiveAsync(new ArraySegment<byte>(ReadData), CancellationToken.None);
                int timeoutcounter = 0;
                while (!task.IsCompleted && timeoutcounter != 10)
                {
                    Thread.Sleep(5);
                    timeoutcounter++;
                }
                var wsResult = task.Result;
                if (wsResult.CloseStatus.HasValue)
                {
                    return new byte[0];
                }
                for (int i = 0; i < wsResult.Count; i++)
                {
                    result[i + count] = ReadData[i];
                }
                count += wsResult.Count;
            }
            return result;
        }

        public bool Connected()
        {
            return ws.State == WebSocketState.Open;
        }
    }
}