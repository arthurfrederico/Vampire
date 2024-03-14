using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Killfeed;

[HarmonyPatch(typeof(VampireDownedServerEventSystem), nameof(VampireDownedServerEventSystem.OnUpdate))]
public static class VampireDownedHook
{
    public static void Prefix(VampireDownedServerEventSystem __instance)
    {
        var downedEvents = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in downedEvents)
        {
            ProcessVampireDowned(entity);
        }
    }

    private static void ProcessVampireDowned(Entity entity)
    {
        
        if(!VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, VWorld.Server.EntityManager, out var victimEntity))
        {
            Plugin.Logger.LogMessage("Não foi possível declarar a vitima");
            return;
        }
		
		var downBuff = entity.Read<VampireDownedBuff>();

		
		if (!VampireDownedServerEventSystem.TryFindRootOwner(downBuff.Source, 1, VWorld.Server.EntityManager, out var killerEntity))
		{
			Plugin.Logger.LogMessage("Não foi possivel declarar a causa da morte.");
			return;
		}
		
		var victim = victimEntity.Read<PlayerCharacter>();

		Plugin.Logger.LogMessage($"{victim.Name} é a vitima");
		var unitKiller = killerEntity.Has<UnitLevel>();
		
		if (unitKiller)
        {
			Plugin.Logger.LogInfo($"{victim.Name} foi morto por uma criatura.");
            return;
		}

		var playerKiller = killerEntity.Has<PlayerCharacter>();

		if (!playerKiller)
        {
            Plugin.Logger.LogWarning($"Deixa MathPonte saber que tem mais uma morte e não foi pvp ou mob.");
            return;
        }
		
		var killer = killerEntity.Read<PlayerCharacter>();

		if (killer.UserEntity == victim.UserEntity)
        {
            Plugin.Logger.LogInfo($"{victim.Name} se matou.");
            return;
        }
		
        var location = victimEntity.Read<LocalToWorld>();
		
        DataStore.RegisterKillEvent(victim, killer, location.Position);
    }
}
