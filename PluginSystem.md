Very small example

```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using LibOpenCraft.ServerPackets;



namespace LibOpenCraft.Bukkit.Plugin
{
    [Export(typeof(CoreModule))] // this is the core module 
    //aka what allows this dll file to be imported and ran
    [ExportMetadata("Name", "Bukkit Plugin Loader")]//this is it's name
    public class BukkitLoader : CoreModule
    {
        string name = "";
        List<string> bukkitfiles;
        public BukkitLoader()
            : base()
        {
            bukkitfiles = new List<string>();
            string[] files = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
            foreach (string str in files)
            {
                if (str.Contains(".jar"))
                {
                    System.Diagnostics.Process.Start("ikvmc.exe -target:library " + str);
                    bukkitfiles.Add(str.Replace(".jar", ".dll"));
                }
            }

        }

        public override void Start()
        {
            base.Start();
           //Always do a base start so you can start a module cache
           //any packet that is implemented can be attached to by simply
           // entering PacketType.Packet what you return is a modded
           // packet and will be submitted to the user
           // you can access GridServer and the World to mod
           // player locations block types etc... be creative :)
 ModuleHandler.InvokeAddModuleAddon(PacketType.PlayerBlockPlacement, OnBlockChange);
            base.RunModuleCache();
        }

        public PacketHandler OnBlockChange(PacketType p_type, string CustomPacketType, ref PacketReader packet_reader, PacketHandler _p, ref ClientManager cm)
        {
            return _p;
        }

        public override void Stop()
        {
            base.Stop();
            ModuleHandler.RemoveEventModule(PacketType.ChatMessage);
        }
    }
}

```