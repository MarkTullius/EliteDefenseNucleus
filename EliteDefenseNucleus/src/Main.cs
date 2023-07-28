using BepInEx;
using RoR2;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace EliteDefenseNucleus
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

  public class Main : BaseUnityPlugin
  {
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "MarkTullius";
    public const string PluginName = "EliteDefenseNucleus";
    public const string PluginVersion = "1.0.0";

    private List<EquipmentIndex> eliteQueue = new();
    private int constructCount;
    private int constructLimit;

    public void Awake()
    {
      On.RoR2.GlobalEventManager.OnCharacterDeath += OnCharacterDeath;
      On.RoR2.CharacterBody.Start += OnConstructSpawn;
    }

    private void OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
    {
      CharacterBody victimBody = damageReport.victimBody;            
      CharacterMaster attackerMaster = damageReport.attackerMaster;
      CharacterBody attackerBody = damageReport.attackerBody;
      if (attackerBody && attackerBody.isPlayerControlled)
      {
        CountUpdate(attackerMaster);
        Inventory inventory = (attackerMaster ? attackerMaster.inventory : null);
        if (victimBody.isElite && CanSpawnConstruct(inventory))
        {
          EquipmentIndex eliteEQ = victimBody.inventory.GetEquipmentIndex();
          eliteQueue.Add(eliteEQ);
        }
      }      
      orig(self, damageReport);
    }

    private void OnConstructSpawn(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
    {
      if (self.name == "MinorConstructOnKillBody(Clone)")
      {
        if (eliteQueue.Count > 0)
        {
          string eqName = EquipmentCatalog.GetEquipmentDef(eliteQueue.FirstOrDefault<EquipmentIndex>()).name;
          self.inventory.GiveEquipmentString(eqName);
          eliteQueue.RemoveAt(0);
        }
      }
      orig(self);
    }

    private bool CanSpawnConstruct(Inventory inventory)
    {
      if (constructCount < constructLimit)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    private void CountUpdate(CharacterMaster master)
    {
      constructCount = CountCharacterBodiesWithName("MinorConstructOnKillBody(Clone)");
      constructLimit = master.inventory.GetItemCount(ItemCatalog.FindItemIndex("MinorConstructOnKill")) * 4;
    }

    private int CountCharacterBodiesWithName(string nameToFind)
    {
      int count = 0;
      CharacterBody[] characterBodies = FindObjectsOfType<CharacterBody>();

      foreach (CharacterBody body in characterBodies)
      {
        if (body.name == nameToFind)
        {
          count++;
        }
      }

      return count;
    }
  }
}