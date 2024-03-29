﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using LibOpenCraft.ServerPackets;

namespace LibOpenCraft.MojangProtocol
{
    [Export(typeof(CoreEventModule))]
    [ExportMetadata("Name", "Handshake")]
    public class Handshake : CoreEventModule
    {
        string name = "";
        public Handshake()
            : base(PacketType.Handshake)
        {
            
        }

        public override void Start()
        {
            base.Start();
            ModuleHandler.AddEventModule(PacketType.Handshake, new ModuleCallback(OnHandshake));
            base.RunModuleCache();
        }

        public void OnHandshake(ref PacketReader _pReader, PacketType pt, ref ClientManager _client)
        {
            string[] conString = _pReader.ReadString().Split(';');
            _client._player.name = conString[0];
            GridServer.player_list[_client.id].WaitToRead = false;
            #region Building Packet
            HandshakePacket p = new HandshakePacket(PacketType.Handshake);
            p.ConnectionHash = (string)Config.Configuration["Handshake"];
            p.BuildPacket();
            _client.SendPacket(p, _client.id, ref _client, false, false);
            try
            {
                int i = 0;
                for (; i < base.ModuleAddons.Count; i++)
                {
                    base.ModuleAddons.ElementAt(i).Value(pt, ModuleAddons.ElementAt(i).Key, ref _pReader, (PacketHandler)p, ref _client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message + " Source:" + e.Source + " Method:" + e.TargetSite + " Data:" + e.Data);
            }
            p = null;
            #endregion Building Packet
        }

        public override void Stop()
        {
            base.Stop();
            ModuleHandler.RemoveEventModule(PacketType.Handshake);
        }
    }
}
