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
      On.RoR2.Run.Update += CountUpdate;
    }

    private void OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
    {
      Debug.Log("Death registered");
      CharacterBody victimBody = damageReport.victimBody;            
      CharacterMaster attackerMaster = damageReport.attackerMaster;
      CharacterBody attackerBody = damageReport.attackerBody;
      if (attackerBody && attackerBody.isPlayerControlled)
      {
        Inventory inventory = (attackerMaster ? attackerMaster.inventory : null);
        if (victimBody.isElite && CanSpawnConstruct(inventory))
        {
          Debug.Log("Enemy is elite");
          EquipmentIndex eliteEQ = victimBody.inventory.GetEquipmentIndex();
          Debug.Log($"Elite EQ name: {EquipmentCatalog.GetEquipmentDef(eliteEQ).name}");
          eliteQueue.Add(eliteEQ);
          Debug.Log("Elite Equipment added to queue");
        }
      }      
      orig(self, damageReport);
    }

    private void OnConstructSpawn(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
    {
      if (self.name == "MinorConstructOnKillBody(Clone)")
      {
        Debug.Log("Spawning Alpha Construct");
        if (eliteQueue.Count > 0)
        {
          Debug.Log("Valid EQ Index");
          string eqName = EquipmentCatalog.GetEquipmentDef(eliteQueue.FirstOrDefault<EquipmentIndex>()).name;
          self.inventory.GiveEquipmentString(eqName);
          Debug.Log($"Elite Equipment for {eqName} added");
          Debug.Log($"Is elite: {self.isElite}");
          Debug.Log($"Elite Buff count: {self.eliteBuffCount}");
          eliteQueue.RemoveAt(0);
        }
        else
        {
          Debug.Log("No equipments to give");
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

    private void CountUpdate(On.RoR2.Run.orig_Update orig, Run self)
    {
      orig(self);
      constructCount = CountCharacterBodiesWithName("MinorConstructOnKillBody(Clone)");
      constructLimit = PlayerCharacterMasterController._instancesReadOnly[0].body.inventory.GetItemCount(ItemCatalog.FindItemIndex("MinorConstructOnKill"));
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