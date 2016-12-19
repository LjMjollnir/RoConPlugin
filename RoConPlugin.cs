using Rocket.API;
using Rocket.Core.Commands;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LjMjollnir
{
    public class RoConPlugin : RocketPlugin
    {
        private readonly string Version = "0.3.1.0";
        CultureInfo invC = CultureInfo.InvariantCulture;
        NumberStyles ns = NumberStyles.Float;

        protected override void Load()
        {
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
        }

        protected override void Unload()
        {
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            base.Unload();
        }

        [RocketCommand("RoConVersion", "Displays RoConPlugin build number", "RoConBuild", AllowedCaller.Console)]
        public void ExecuteBuildEX(IRocketPlayer caller, string[] para)
        {
            Logger.Log(String.Format(invC, "RoConVersion {0}", Version));
        }

        private void UnturnedPlayerEvents_OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, Steamworks.CSteamID murderer)
        {
            Logger.Log(String.Format(invC, "DeathEX,{0},{1},{2},{3},{4},{5}", player.CSteamID, murderer, cause, limb, player.Position.x, player.Position.z));
        }

        [RocketCommand("gettime", "Nothing to see here", "<fred>", AllowedCaller.Console)] //TODO: Make /Watch command convert cycle/time -> 24 hour
        public void ExecuteTimeEX(IRocketPlayer caller, string[] para)
        {
            uint time = LightingManager.time;
            Logger.Log(time.ToString());
            uint cycle = LightingManager.cycle;
            Logger.Log(cycle.ToString());
        }

        [RocketCommand("broadcastex", "Nothing to see here", "<fred>", AllowedCaller.Console)]
        public void ExecuteBroadcastEX(IRocketPlayer caller, string[] para)
        {
            if (para == null) return;
            if (para.Length != 2) return;
            if (para.Length == 2)
            {
                string[] SSV = para[1].Split(' ');
                float r, g, b, a;
                float.TryParse(SSV[0], ns, invC, out r);
                float.TryParse(SSV[1], ns, invC, out g);
                float.TryParse(SSV[2], ns, invC, out b);
                float.TryParse(SSV[3], ns, invC, out a);
                r = r / 255;
                g = g / 255;
                b = b / 255;
                a = a / 255;
                UnityEngine.Color Col = new UnityEngine.Color(r, g, b, a);
                UnturnedChat.Say(para[0], Col);
            }
        }
        [RocketCommand("mapex", "Nothing to see here", "<fred>", AllowedCaller.Console)]
        public void ExecuteMapEX(IRocketPlayer caller, string[] para)
        {
            Logger.Log(String.Format(invC, "MapEX,{0},{1},{2}", Provider.map, Level.size, Level.border));
            foreach (Node n in LevelNodes.nodes)
            {
                if (n.type != ENodeType.LOCATION) continue;
                Logger.Log(String.Format(invC, "MapNode,{0},{1},{2}", ((LocationNode)n).name, n.point.x, n.point.z));
            }
        }
        [RocketCommand("serverex", "Nothing to see here", "<fred>", AllowedCaller.Both)]
        public void ExecuteServerEX(IRocketPlayer caller, string[] para)
        {
            Logger.Log(String.Format("NameEX {0}", Provider.serverName));
            Logger.Log(String.Format("WelcomeEX {0}", ChatManager.welcomeText));
        }

        [RocketCommand("pingvehicles", "Nothing to see here", "<fred>", AllowedCaller.Both)]
        public void ExecuteVehicleListEX(IRocketPlayer caller, string[] para)
        {
            List<InteractableVehicle> Vehicles = SDG.Unturned.VehicleManager.vehicles;
            Logger.Log("VPing.Clear");

            foreach (InteractableVehicle v in Vehicles)
            {
                if (v.health <= 0) continue;
                float x = 0, z = 0;
                x = v.gameObject.transform.position.x;
                z = v.gameObject.transform.position.z;
                Logger.Log(String.Format(invC, "VehiclePing,{0},{1},{2}", v.asset.vehicleName, x, z));
            }
        }
#if (PRO || DEBUG)
        [RocketCommand("pingstructures", "Nothing to see here", "<fred>", AllowedCaller.Both)]
        [RocketCommandAlias("ps")]
        public void ExecuteStructureListEX(IRocketPlayer caller, string[] para)
        {
            // Pro FEATURES are not provided for Free -- CODE REMOVED LJMjollnir
        }
        [RocketCommand("pingbaricade", "Nothing to see here", "<fred>", AllowedCaller.Both)]
        [RocketCommandAlias("pb")]
        public void ExecuteCadeListEX(IRocketPlayer caller, string[] para)
        {
            // Pro FEATURES are not provided for Free -- CODE REMOVED LJMjollnir
        }
#endif

        [RocketCommand("teleportex", "Nothing to see here", "<fred>", AllowedCaller.Console)]
        public void ExecuteTeleportEX(IRocketPlayer caller, string[] para)
        {
            if (para.Length != 3) return;
            float x = 0, y = 0;
            float.TryParse(para[1], ns, invC, out x);
            float.TryParse(para[2], ns, invC, out y);
            UnturnedPlayer player = UnturnedPlayer.FromName(para[0]);
            UnityEngine.RaycastHit raycastHit = new UnityEngine.RaycastHit();
            UnityEngine.Physics.Raycast(new UnityEngine.Vector3(x, 519, y), UnityEngine.Vector3.down, out raycastHit);
            //            Logger.Log(String.Format("Raycast Y:{0}", raycastHit.point.y));
            if (player.IsInVehicle)
            {
                Logger.Log("Unable to teleport players in Vehicles");
                return;
            }
            player.Teleport(new UnityEngine.Vector3(x, raycastHit.point.y, y), 0);
        }

        [RocketCommand("airdropex", "Nothing to see here", "<fred>", AllowedCaller.Console)]
        public void ExecuteAirDropEX(IRocketPlayer caller, string[] para)
        {
            if (para.Length != 3) return;
            float x = 0, y = 0;
            ushort AirID = 0;
            float.TryParse(para[0], ns, invC, out x);
            float.TryParse(para[1], ns, invC, out y);
            ushort.TryParse(para[2], ns, invC, out AirID);
            LevelManager.airdrop(new UnityEngine.Vector3(x, 519, y), AirID);
        }

        [RocketCommand("playersex", "Nothing to see here", "<fred>", AllowedCaller.Console)]
        public void ExecutePlayersEX(IRocketPlayer caller, string[] para)
        {
            if (Provider.clients.Count == 0) { Logger.Log("PlayersEX.NoPlayers"); return; }
            Logger.Log("PlayersEX.Clear");
            foreach (SteamPlayer plr in Provider.clients)
            {
                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromSteamPlayer(plr);
                int Ping = (int)Math.Round(unturnedPlayer.Ping * 1000);
                //                Logger.Log("PlayersEX.Player" + "," + unturnedPlayer.CharacterName + "," + Ping.ToString() + "," + unturnedPlayer.Health.ToString() + "," + unturnedPlayer.Hunger.ToString() + "," + unturnedPlayer.Thirst.ToString() + "," + unturnedPlayer.Infection.ToString() + "," + unturnedPlayer.Bleeding.ToString() + "," + unturnedPlayer.Broken.ToString() + "," + unturnedPlayer.Position.x.ToString() + "," + unturnedPlayer.Position.z.ToString() + "," + unturnedPlayer.CSteamID.ToString());
                Logger.Log(String.Format(invC, "PlayersEX.Player,{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", unturnedPlayer.CharacterName, Ping, unturnedPlayer.Health, unturnedPlayer.Hunger, unturnedPlayer.Thirst, unturnedPlayer.Infection, unturnedPlayer.Bleeding, unturnedPlayer.Broken, unturnedPlayer.Position.x, unturnedPlayer.Position.z, unturnedPlayer.CSteamID));
            }
            Logger.Log("PlayersEX.End");
        }

        [RocketCommand("spawntable", "Nothing to see here", "<fred>", AllowedCaller.Both)]
        public void ExecuteSpawnTableEX(IRocketPlayer caller, string[] para)
        {
            Logger.Log("SpawnTable.Clear");
            foreach (var t in LevelItems.tables)
            {
                Logger.Log(String.Format(invC, "SpawnTable,{0},{1}", t.name, t.tableID));
            }
        }

        [RocketCommand("modplayer", "Nothing to see here", "<fred>", AllowedCaller.Console)]
        public void ExecuteModPlayer(IRocketPlayer caller, string[] para)
        {
            if (para.Length != 2) return;
            List<string> CSV = para[1].Split(',').ToList();
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromName(para[0]);
            if (unturnedPlayer == null) return;
            int health = unturnedPlayer.Health;
            int newHealth = 100, newHunger = 0, newWater = 0, newRad = 0;
            bool newBleed = false, newBones = false;
            int.TryParse(CSV[1], ns, invC, out newHealth);
            int.TryParse(CSV[2], ns, invC, out newHunger);
            int.TryParse(CSV[3], ns, invC, out newWater);
            int.TryParse(CSV[4], ns, invC, out newRad);
            bool.TryParse(CSV[5], out newBleed);
            bool.TryParse(CSV[6], out newBones);

            if (newHealth > health && (newHealth - health <= Byte.MaxValue))
                unturnedPlayer.Heal(Convert.ToByte(newHealth - health));
            if (newHealth < health && (health - newHealth < Byte.MaxValue))
                unturnedPlayer.Damage(Convert.ToByte(health - newHealth), new UnityEngine.Vector3(0, 0, 0), EDeathCause.BREATH, ELimb.SKULL, new Steamworks.CSteamID());

            unturnedPlayer.Hunger = ByteLimit(100 - newHunger);
            unturnedPlayer.Thirst = ByteLimit(100 - newWater);
            unturnedPlayer.Infection = ByteLimit(100 - newRad);
            unturnedPlayer.Bleeding = newBleed;
            unturnedPlayer.Broken = newBones;
        }

        private Byte ByteLimit(int value)
        {
            if (value > Byte.MaxValue) return Byte.MaxValue;
            if (value < Byte.MinValue) return Byte.MinValue;
            return Convert.ToByte(value);
        }
    }
}
