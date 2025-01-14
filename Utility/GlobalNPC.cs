﻿using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LevelPlus {
  class levelplusGlobalNPC : GlobalNPC {

    public override bool InstancePerEntity => true;

    public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment) {
      base.ApplyDifficultyAndPlayerScaling(npc, numPlayers, balance, bossAdjustment);
      if (LevelPlusConfig.Instance.ScalingEnabled) {
        float averageLevel = 0;



        foreach (Player i in Main.player)
          if (i.active) {
            numPlayers++;
            averageLevel += i.GetModPlayer<LevelPlusModPlayer>().level;
          }

        averageLevel /= numPlayers;

        //only apply scaling to non-boss enemies
        if (!npc.boss)
          {
            npc.damage = (int)Math.Clamp(npc.damage * (long)Math.Round(1 + averageLevel * LevelPlusConfig.Instance.ScalingDamage), 0, 2147483000);
            npc.lifeMax = (int)Math.Clamp(npc.lifeMax * (long)Math.Round(1 + averageLevel * LevelPlusConfig.Instance.ScalingHealth), 0, 2147483000);
          }

      }
    }


    public override void OnKill(NPC npc) {
      base.OnKill(npc);

      if (npc.type != NPCID.TargetDummy && !npc.SpawnedFromStatue && !npc.friendly && !npc.townNPC) {
        ulong amount;
        if (npc.boss) {
          amount = (ulong)(npc.lifeMax * LevelPlusConfig.Instance.BossXP);
        }
        else {
          amount = (ulong)(npc.lifeMax * LevelPlusConfig.Instance.MobXP);
        }

        if (Main.netMode == NetmodeID.SinglePlayer) {
          Main.LocalPlayer.GetModPlayer<LevelPlusModPlayer>().AddXp(amount);
        }
        else if (Main.netMode == NetmodeID.Server) {
          for (int i = 0; i < npc.playerInteraction.Length; ++i) {
            if (npc.playerInteraction[i]) {
              ModPacket packet = LevelPlus.Instance.GetPacket();
              packet.Write((byte)PacketType.XP);
              packet.Write(amount);
              packet.Send(i);
            }
          }
        }
      }
    }
  }
}

