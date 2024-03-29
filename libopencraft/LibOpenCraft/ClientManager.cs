﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;


namespace LibOpenCraft
{
    public delegate void RecievedPackets(Dictionary<string, Variable> data, TcpClient client);

    public class ClientManager
    {
        public PlayerClass _player = new PlayerClass();
        public Stopwatch ms_latency = new Stopwatch();
        #region Misc Variables
        public Dictionary<string, object> customAttributes = new Dictionary<string, object>();
        public int id = -1;
        public DateTime keep_alive = DateTime.Now;
        public RecievedPackets OnRecieved;
        public int PreChunkRan = 0;
        #endregion Misc Variables
        #region Networking and Threading
        public NetworkStream _stream = null;
        public TcpClient _client;
        private Thread recieved;
        private ThreadStart recieve_start;
        bool _shouldStop = false;
        #endregion Networking and Threading

        public ClientManager()
        {
            customAttributes = new Dictionary<string, object>();
        }
        public ClientManager(TcpClient client, int _id)
        {
            try
            {
                id = _id;
                _client = client;
                //_stream = new NetworkStream(client.Client);
                recieve_start = new ThreadStart(Recieve);
                
                recieved = new Thread(recieve_start);
                customAttributes.Add("PayLoad", id);
                _player.customerVariables.Add("BeforeFirstPosition", null);
                customAttributes.Add("MSLatency", null);
                if (recieved == null) recieved = new Thread(recieve_start);
                recieved.Start();
                //Recieve((object)id);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR:" + e.Message);
            }
        }
        public void Stop(bool ClearData)
        {
            if (ClearData == true)
            {
                _shouldStop = true;
                recieve_start = null;
                recieved = null;
                _stream = null;
                _client = null;
                GridServer.player_list[id] = null;
                //id = -1;
            }
            else
            {
                _shouldStop = true;
            }
        }
        public bool WaitToRead = false;
        public void Send(ClientManager cm, byte[] bytes)
        {
            if (cm != null && cm._client != null && cm._client.Connected == true)
                GridServer.player_list[cm.id]._client.Client.Send(bytes);
        }

        public void SendPacket(PacketHandler p, int id, ref ClientManager cm, bool PingType, bool Waitread)
        {
            try
            {
                byte[] t_byte = p.GetBytes();
                if (PingType == false)
                {
                    Console.WriteLine("Packet Sent: " + p._packetid.ToString() + " Length: " + t_byte.Length);
                    GridServer.player_list[cm.id]._client.Client.SendBufferSize = t_byte.Length;
                    GridServer.player_list[cm.id]._client.Client.NoDelay = true;
                    GridServer.player_list[cm.id].Send(cm, t_byte);
                    if (GridServer.player_list[cm.id].keep_alive != null) GridServer.player_list[cm.id].keep_alive = DateTime.Now;
                    else
                    {
                        GridServer.player_list[cm.id].keep_alive = new DateTime();
                        GridServer.player_list[cm.id].keep_alive = DateTime.Now;

                    }
                    GridServer.player_list[cm.id].WaitToRead = Waitread;
                }
                else if (PingType == true)
                {
                    //Console.WriteLine("Packet Sent: " + p._packetid.ToString() + " Length: " + t_byte.Length);
                    byte[] temp = new byte[256];
                    t_byte.CopyTo(temp, 0);

                    GridServer.player_list[cm.id]._client.Client.SendBufferSize = t_byte.Length;
                    GridServer.player_list[cm.id]._client.Client.NoDelay = true;
                    GridServer.player_list[cm.id].Send(cm, temp);
                    GridServer.player_list[cm.id]._client.Close();
                    GridServer.player_list[cm.id].Stop(true);
                }
                else
                {
                    Console.WriteLine("Packet Sent: " + p._packetid.ToString() + " Length: " + t_byte.Length);

                    GridServer.player_list[cm.id]._client.Client.SendBufferSize = t_byte.Length;
                    GridServer.player_list[cm.id]._client.Client.NoDelay = true;
                    GridServer.player_list[cm.id].Send(cm, t_byte);
                    GridServer.player_list[cm.id].keep_alive = DateTime.Now;
                    GridServer.player_list[cm.id].WaitToRead = Waitread;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("ERROR: " + e.Message + "\nSource:" + e.Source + "\nMethod:" + e.TargetSite + "\nData:" + e.Data + "\nStack trace:" + e.StackTrace);
                if (GridServer.player_list[id] != null)
                {
                    if (GridServer.player_list[id] != null)
                    {
                        GridServer.player_list[id].Stop(true);
                    }
                    else
                    {
                        this.Stop(true);
                    }
                }
                else
                {
                    this.Stop(true);
                }
            }
        }
        bool Suspendv = false;
        public void Suspend()
        {
            Suspendv = true;
        }
        public void Resume()
        {
            Suspendv = false;
        }
        int count = 0;
        
        public void Recieve()
        {
            Nanosecond nano = new Nanosecond();
            //System.Diagnostics.
            int _id = id;
            System.GC.KeepAlive(GridServer.player_list[_id]);
            _stream = new NetworkStream(_client.Client);
            NetworkStream stream = new NetworkStream(_client.Client);
            PacketReader p_reader = new PacketReader(stream);
            //bool connected = true;
            while (GridServer.player_list[_id] != null && !GridServer.player_list[_id]._shouldStop)
            {
                if (nano.RunMiliSecond()) System.Threading.Thread.Sleep(1);
                try
                {
                    if (GridServer.player_list[_id] == null)
                    {
                        break;
                    }
                    if (GridServer.player_list[_id].Suspendv == false)
                    {
                        if (GridServer.player_list[_id]._client.Connected == false)
                        {
                            GridServer.player_list[_id].Stop(true);
                            return;
                        }
                        if (GridServer.player_list[_id]._client.Client.Available > 0)
                        {
                            //System.Threading.Thread.Sleep(1);
                            GridServer.player_list[_id].WaitToRead = true;
                            byte read = p_reader.ReadByte();
                            PacketType p_type = (PacketType)read;
                            //Console.WriteLine("Packet Recieved: " + p_type.ToString() + " Packet Byte:" + (byte)p_type);
                            if (ModuleHandler.Eventmodules.Keys.Contains(p_type))
                            {
                                Thread.BeginCriticalRegion();
                                ModuleHandler.Eventmodules[p_type](ref p_reader, p_type, ref GridServer.player_list[_id]);
                                Thread.EndCriticalRegion();
                                //GridServer.player_list[_id].WaitToRead = false;
                            }
                            else
                            {
                                System.Console.WriteLine(p_type.ToString() + " is not implemented:" + (byte)p_type);
                                byte[] unsupported_garbage = new byte[(int)p_reader.reader.Length];
                                p_reader.reader.Read(unsupported_garbage, 0, (int)p_reader.reader.Length);
                                GridServer.player_list[_id].WaitToRead = false;

                            }
                            if (GridServer.player_list[_id] != null && GridServer.player_list[_id]._stream != null && DateTime.Now.Minute > GridServer.player_list[_id].keep_alive.Minute || GridServer.player_list[_id].keep_alive.Second + 1 < DateTime.Now.Second)
                            {
                                Random r = new Random(1789);
                                if (GridServer.player_list[_id].customAttributes.ContainsKey("PayLoad"))
                                    GridServer.player_list[_id].customAttributes["PayLoad"] = (object)r.Next(1024, 4096);
                                LibOpenCraft.ServerPackets.KeepAlivePacket p = new LibOpenCraft.ServerPackets.KeepAlivePacket(PacketType.KeepAlive);
                                p.ID = (int)GridServer.player_list[_id].customAttributes["PayLoad"];
                                p.BuildPacket();
                                #region ProcessLatency
                                ms_latency.Start();
                                #endregion ProcessLatency
                                SendPacket(p, _id, ref GridServer.player_list[_id], false, false);
                            }


                        }
                        else
                        {
                            if (_player.name == "")
                                Thread.Sleep(50);
                            else
                            {
                                if (GridServer.player_list[_id]._player.EntityUpdateCount < (int)Config.Configuration["EntityUpdate"])
                                {
                                    ClientManager[] player = GridServer.player_list;
                                    for (int i = 0; i < player.Length; i++)
                                    {
                                        if (player[i] == null || player[i].id == _id || player[i].PreChunkRan != 1)
                                        {

                                        }
                                        else
                                        {
                                            LibOpenCraft.ServerPackets.EntityPacket e = new LibOpenCraft.ServerPackets.EntityPacket(PacketType.Entity);
                                            e.EntityID = _id;
                                            e.BuildPacket();
                                            player[i].SendPacket(e, player[i].id, ref player[i], false, false);
                                            GridServer.player_list[_id]._player.EntityUpdateCount++;
                                        }
                                    }
                                }
                                else
                                    GridServer.player_list[_id]._player.EntityUpdateCount = 0;
                            }
                        }
                    }
                }

                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message + "\nSource:" + e.Source + "\nMethod:" + e.TargetSite + "\nData:" + e.Data + "\nStackTrace:" + e.StackTrace);
                    if (GridServer.player_list[_id] != null)
                    {
                        
                        GridServer.player_list[_id].Stop(true);
                    }
                    else
                    {
                        Stop(true);
                    }
                }
            }
        }
    }
}
#region  working non async
/* in Theory non async */
/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LibOpenCraft
{
    public delegate void RecievedPackets(Dictionary<string, Variable> data, TcpClient client);
    
    public class ClientManager
    {
        public PlayerClass _player = new PlayerClass();
        #region Misc Variables
        public Dictionary<string, object> customAttributes = new Dictionary<string, object>();
        public int id = -1;
        public DateTime keep_alive = DateTime.Now;
        public RecievedPackets OnRecieved;
        public int PreChunkRan = 0;
        #endregion Misc Variables
        #region Networking and Threading
        public NetworkStream _stream;
        protected TcpClient _client;
        private Thread recieved;
        private ParameterizedThreadStart recieve_start;
        private AsyncCallback recieve_async;
        #endregion Networking and Threading

        private ClientManager _recieveClient;

        public ClientManager(TcpClient client, int _id)
        {
            try
            {
                id = _id;
                _client = client;
                _stream = client.GetStream();
                recieve_async = new AsyncCallback(AsyncResult_recieve);
                recieve_start = new ParameterizedThreadStart(Recieve);
                recieved = new Thread(recieve_start);
                customAttributes.Add("PayLoad", null);
                recieved = new Thread(recieve_start);
                recieved.Start(id);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR:" + e.Message);
            }
        }
        public void Stop(bool ClearData)
        {
            if (ClearData == true)
            {
                if (recieved != null)
                    recieved.Abort();
                recieve_async = null;
                recieve_start = null;
                recieved = null;
                _stream = null;
                _client = null;
                id = -1;
            }
            else
            {
                recieved.Abort();
            }
        }
        public void Start(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
            recieve_async = new AsyncCallback(AsyncResult_recieve);
            recieve_start = new ParameterizedThreadStart(Recieve);
            recieved = new Thread(recieve_start);
            recieved.Start(id);
        }
        public bool WaitToRead = false;
        public void Start()
        {
            recieved.Start(id);
        }
        private void AsyncResult_recieve(IAsyncResult IAR)
        {

        }
        public void SendPacket(PacketHandler p, int id, bool PingType = false, ClientManager _cm = null, bool Waitread = false)
        {
            try
            {
                byte[] t_byte = p.GetBytes();
                if (GridServer.player_list.ContainsKey(id) && PingType == false)
                {
                    GridServer.player_list[id]._stream.Write(t_byte, 0, t_byte.Length);
                    GridServer.player_list[id]._stream.Flush();
                    GridServer.player_list[id].WaitToRead = Waitread;
                    Console.WriteLine("Packet Sent: " + p._packetid.ToString() + " Length: " + t_byte.Length);
                    t_byte = null;
                    p = null;
                }
                else if (PingType == true)
                {
                    byte[] temp = new byte[256];
                    t_byte.CopyTo(temp, 0);
                    GridServer.player_list[id]._stream.Write(temp, 0, temp.Length);
                    GridServer.player_list[id]._stream.Flush();
                    GridServer.player_list[id]._client.Close();
                    GridServer.player_list[id]._recieveClient.Stop(true);
                    GridServer.player_list[id].WaitToRead = Waitread;
                    Console.WriteLine("Packet Sent: " + p._packetid.ToString() + " Length: " + t_byte.Length);
                    t_byte = null;
                    p = null;
                }
                else
                {
                    t_byte = null;
                    p = null;
                }
            }
            catch(Exception e)
            {
                GridServer.player_list[id]._stream.Close();
                GridServer.player_list[id]._client.Close();
                GridServer.player_list[id].Stop(true);
                GridServer.player_list[id].WaitToRead = Waitread;
                Console.WriteLine("ERROR: " + e.Message + " Source:" + e.Source);
                Random r = new Random();
                GridServer.player_list.Remove(id);
            }
        }
        protected void Recieve(object obj)
        {
            int id = (int)obj;
            GridServer.player_list[id]._stream = new NetworkStream(GridServer.player_list[id]._client.Client);
            PacketReader p_reader = new PacketReader(new System.IO.BinaryReader(GridServer.player_list[id]._stream));
            bool connected = true;
            while (connected == true)
            {
                try
                {
                    Thread.Sleep(50);
                    if (GridServer.player_list[id]._client == null || id == -1)
                    {
                        GridServer.player_list[id].Stop(true);
                        break;
                    }
                    Thread.Sleep(50);
                    if (!GridServer.player_list[id]._client.Connected == true)
                    {
                        GridServer.player_list[id]._client.Close();
                        GridServer.player_list[id].Stop(true);
                        break;
                    }
                    Thread.Sleep(50);
                    if (GridServer.player_list[id].WaitToRead == false && GridServer.player_list[id]._stream != null && !GridServer.player_list[id].customAttributes.ContainsKey("InPrechunk") && GridServer.player_list[id]._client.Available >= 1)
                    {
                        GridServer.player_list[id].WaitToRead = true;
                        Thread.Sleep(50);
                        PacketType p_type = (PacketType)p_reader.ReadByte();
                        Thread.Sleep(50);
                        if (ModuleHandler.Eventmodules.ContainsKey(p_type))
                        {
                            ClientManager client = GridServer.player_list[id];
                            ModuleHandler.Eventmodules[p_type](ref p_reader, p_type, ref client);
                            GridServer.player_list[id] = client;
                        }
                        else
                        {
                            System.Console.WriteLine(p_type.ToString() + " is not implemented:" + (byte)p_type);
                            //byte[] temp = new byte[_recieveClient._stream.Length];
                            //_recieveClient._stream.Read(temp, 0, (int)_recieveClient._stream.Length);
                            //Console.Write(temp);
                        }

                    }
                    else
                    {
                        if (_recieveClient.WaitToRead == false && _recieveClient._stream != null && keep_alive.Second + 1 < DateTime.Now.Second && !_recieveClient.customAttributes.ContainsKey("InPrechunk"))
                        {
                            GridServer.player_list[id].WaitToRead = true;
                            Random r = new Random(1789);
                            GridServer.player_list[id].customAttributes["PayLoad"] = r.Next(1024, 4096);
                            LibOpenCraft.ServerPackets.KeepAlivePacket p = new LibOpenCraft.ServerPackets.KeepAlivePacket(PacketType.KeepAlive);
                            p.AddInt(id);
                            GridServer.player_list[id].SendPacket(p, id);
                            p = null;
                            keep_alive = DateTime.Now;
                        }
                        if (_player.name == "")
                            Thread.Sleep(10);
                        else
                            Thread.Sleep(3);
                    }
                }
                catch (Exception e)
                {
                    if (GridServer.player_list[id]._stream != null)
                        GridServer.player_list[id]._stream.Close();
                    if (GridServer.player_list[id]._client != null)
                        GridServer.player_list[id]._client.Close();
                    GridServer.player_list[id].Stop(true);
                    GridServer.player_list.Remove(id);
                    Console.WriteLine("ERROR: " + e.Message + " Source:" + e.Source);
                    break;
                }
            }
        }
    }
    
}
*/
#endregion working non async
#region working async needs work
/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LibOpenCraft
{
    public delegate void RecievedPackets(Dictionary<string, Variable> data, TcpClient client);
    
    public class ClientManager
    {
        public PlayerClass _player = new PlayerClass();
        #region Misc Variables
        public Dictionary<string, object> customAttributes = new Dictionary<string, object>();
        public int id = -1;
        public DateTime keep_alive = DateTime.Now;
        public RecievedPackets OnRecieved;
        public int PreChunkRan = 0;
        #endregion Misc Variables
        #region Networking and Threading
        public NetworkStream _stream = null;
        public TcpClient _client;
        private Thread recieved;
        private ThreadStart recieve_start;

        private AsyncCallback recieve_Async;
        #endregion Networking and Threading

        public ClientManager()
        {
            customAttributes = new Dictionary<string, object>();
        }
        public ClientManager(TcpClient client, int _id)
        {
            try
            {
                id = _id;
                _client = client;
                //_stream = new NetworkStream(client.Client);
                recieve_start = new ThreadStart(Recieve);
                
                recieved = new Thread(recieve_start);
                customAttributes.Add("PayLoad", id);
                _player.customerVariables.Add("BeforeFirstPosition", null);
                if (recieved == null) recieved = new Thread(recieve_start);
                recieved.Start();
                //Recieve((object)id);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR:" + e.Message);
            }
        }
        public void Stop(bool ClearData)
        {
            if (ClearData == true)
            {
                GridServer.player_list[id] = null;
                try
                {
                    if (recieved != null)
                        recieved.Abort();
                }
                catch (Exception)
                {

                }
                recieve_start = null;
                recieved = null;
                _stream = null;
                _client = null;
                id = -1;
            }
            else
            {
            }
        }
        public bool WaitToRead = false;
        public void SendPacket(PacketHandler p, int id, ref ClientManager cm, bool PingType, bool Waitread)
        {
            try
            {
                byte[] t_byte = p.GetBytes();
                if (PingType == null || PingType == false)
                {
                    GridServer.player_list[cm.id]._client.Client.Send(t_byte);
                    GridServer.player_list[cm.id].WaitToRead = false;
                    GridServer.player_list[id].keep_alive = DateTime.Now;
                    Console.WriteLine("Packet Sent: " + p._packetid.ToString() + " Length: " + t_byte.Length);
                    t_byte = null;
                }
                else if (PingType == true)
                {
                    byte[] temp = new byte[256];
                    t_byte.CopyTo(temp, 0);
                    GridServer.player_list[cm.id]._client.Client.Send(temp);
                    GridServer.player_list[cm.id]._client.Close();
                    GridServer.player_list[cm.id].Stop(true);
                    //GridServer.player_list[cm.id] = null;
                    //GridServer.player_list[cm.id]._stream.Flush();
                    Console.WriteLine("Packet Sent: " + p._packetid.ToString() + " Length: " + t_byte.Length);
                    
                }
                else
                {
                    t_byte = null;
                }
            }
            catch(Exception e)
            {
                _stream.Close();
                _client.Close();
                Console.WriteLine("ERROR: " + e.Message + " Source:" + e.Source + " Method:" + e.TargetSite + " Data:" + e.Data);
                Random r = new Random();
                GridServer.player_list[id] = null;
                Stop(true);
            }
        }
        public void recieve_async(IAsyncResult obj)
        {
            if (obj.IsCompleted == true)
            {
                object_data data = (object_data)obj.AsyncState;
                PacketReader pr = (PacketReader)data._pReader;
                ((ModuleCallback)data._callback).EndInvoke(ref pr, ref data._client, obj);
            }
        }
        public void Recieve()
        {
            int _id = id;
            recieve_Async = new AsyncCallback(recieve_async);
            System.GC.KeepAlive(GridServer.player_list[_id]);
            GridServer.player_list[id]._stream = new NetworkStream(GridServer.player_list[_id]._client.Client);
            PacketReader p_reader = new PacketReader(new System.IO.BinaryReader(GridServer.player_list[_id]._stream));
            bool connected = true;
            while (connected == true)
            {
                try
                {
                    if (GridServer.player_list[_id]._client == null || id == -1)
                    {
                        break;
                    }
                    if (GridServer.player_list[_id]._client.Connected == false)
                    {
                        GridServer.player_list[_id]._client.Close();
                        break;
                    }
                    if (GridServer.player_list[_id].WaitToRead == false && GridServer.player_list[_id]._stream != null && !GridServer.player_list[_id].customAttributes.ContainsKey("InPrechunk") && GridServer.player_list[_id]._client.Available > 0)
                    {
                        //System.Threading.Thread.Sleep(1);
                        GridServer.player_list[_id].WaitToRead = true;
                        PacketType p_type = (PacketType)p_reader.ReadByte();
                        if (ModuleHandler.Eventmodules.ContainsKey(p_type))
                        {
                            ModuleHandler.Eventmodules[p_type].BeginInvoke(ref p_reader, p_type, ref GridServer.player_list[_id], recieve_Async, new object_data(GridServer.player_list[_id], p_type, p_reader, ModuleHandler.Eventmodules[p_type]));
                            
                            //GridServer.player_list[_id].WaitToRead = false;
                        }
                        else
                        {
                            System.Console.WriteLine(p_type.ToString() + " is not implemented:" + (byte)p_type);
                            GridServer.player_list[_id].WaitToRead = false;

                        }
                        if (GridServer.player_list[_id]._stream != null && DateTime.Now.Minute > GridServer.player_list[_id].keep_alive.Minute || GridServer.player_list[_id].keep_alive.Second + 1 < DateTime.Now.Second)
                        {
                            Random r = new Random(1789);
                            GridServer.player_list[_id].customAttributes["PayLoad"] = (object)r.Next(1024, 4096);
                            LibOpenCraft.ServerPackets.KeepAlivePacket p = new LibOpenCraft.ServerPackets.KeepAlivePacket(PacketType.KeepAlive);
                            p.ID = (int)GridServer.player_list[_id].customAttributes["PayLoad"];
                            p.BuildPacket();
                            SendPacket(p, id, ref GridServer.player_list[_id], false, false);
                        }
                    }
                    else
                    {
                        if (_player.name == "")
                            Thread.Sleep(50);
                        else
                        {
                            Thread.Sleep(1);
                            if (GridServer.player_list[_id]._player.EntityUpdateCount < (int)Config.Configuration["EntityUpdate"])
                            {
                                ClientManager[] player = GridServer.player_list;
                                for (int i = 0; i < player.Count(); i++)
                                {
                                    if (player[i] == null && player[i].id == id || player[i].PreChunkRan != 1)
                                    {

                                    }
                                    else
                                    {
                                        LibOpenCraft.ServerPackets.EntityPacket e = new LibOpenCraft.ServerPackets.EntityPacket(PacketType.Entity);
                                        e.EntityID = id;
                                        e.BuildPacket();
                                        player[i].SendPacket(e, player[i].id, ref player[i], false, false);
                                        GridServer.player_list[_id]._player.EntityUpdateCount++;
                                    }
                                }
                            }
                            else
                                GridServer.player_list[_id]._player.EntityUpdateCount = 0;
                        }
                    }
                }

                catch (Exception e)
                {
                }
            }
            if (GridServer.player_list[_id]._stream != null)
                GridServer.player_list[_id]._stream.Close();
            if (GridServer.player_list[_id]._client != null)
                GridServer.player_list[_id]._client.Close();
            Console.WriteLine("Connection Closed.");
            GridServer.player_list[_id] = null;
            GC.Collect();
            GridServer.player_list[_id].Stop(true);
        }
    }
    public struct object_data
    {
        public ClientManager _client;
        public PacketType p_type;
        public object _pReader;
        public object _callback;
        public object_data(ClientManager client, PacketType _p_type, object pReader, object callback)
        {
            _client = client;
            p_type = _p_type;
            _pReader = pReader;
            _callback = callback;
        }
    }
}*/
#endregion