using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using System;
using System.Collections.Generic;
using Trove;
using Trove.UtilityAI;
using Action = Trove.UtilityAI.Action;

[Serializable]
public struct HeroConsiderationSet : IComponentData
{
	public BlobAssetReference<ConsiderationDefinition> EnemyInMeleeRange;
	public BlobAssetReference<ConsiderationDefinition> EnemyInRangedRange;
	public BlobAssetReference<ConsiderationDefinition> ThreatLevel;
	public BlobAssetReference<ConsiderationDefinition> ExplorationOpportunity;
	public BlobAssetReference<ConsiderationDefinition> DistanceFromSquire;
	public BlobAssetReference<ConsiderationDefinition> HungerLevel;
	public BlobAssetReference<ConsiderationDefinition> HealthLevel;
	public BlobAssetReference<ConsiderationDefinition> StaminaLevel;
	public BlobAssetReference<ConsiderationDefinition> IdlingComfort;
	public BlobAssetReference<ConsiderationDefinition> CombatReadiness;
}

[CreateAssetMenu(menuName = "Trove/UtilityAI/ConsiderationSets/HeroConsiderationSetData", fileName = "HeroConsiderationSetData")]
public class HeroConsiderationSetData : ScriptableObject
{
	[Header("Consideration Definitions")]
	public ConsiderationDefinitionAuthoring EnemyInMeleeRange  = ConsiderationDefinitionAuthoring.GetDefault(0f, 1f);
	public ConsiderationDefinitionAuthoring EnemyInRangedRange  = ConsiderationDefinitionAuthoring.GetDefault(0f, 1f);
	public ConsiderationDefinitionAuthoring ThreatLevel  = ConsiderationDefinitionAuthoring.GetDefault(0f, 1f);
	public ConsiderationDefinitionAuthoring ExplorationOpportunity  = ConsiderationDefinitionAuthoring.GetDefault(0f, 1f);
	public ConsiderationDefinitionAuthoring DistanceFromSquire  = ConsiderationDefinitionAuthoring.GetDefault(0f, 1f);
	public ConsiderationDefinitionAuthoring HungerLevel  = ConsiderationDefinitionAuthoring.GetDefault(0f, 1f);
	public ConsiderationDefinitionAuthoring HealthLevel  = ConsiderationDefinitionAuthoring.GetDefault(0f, 1f);
	public ConsiderationDefinitionAuthoring StaminaLevel  = ConsiderationDefinitionAuthoring.GetDefault(0f, 1f);
	public ConsiderationDefinitionAuthoring IdlingComfort  = ConsiderationDefinitionAuthoring.GetDefault(0f, 1f);
	public ConsiderationDefinitionAuthoring CombatReadiness  = ConsiderationDefinitionAuthoring.GetDefault(0f, 1f);
	
	public void Bake(IBaker baker, out HeroConsiderationSet considerationSetComponent)
	{
		considerationSetComponent = new HeroConsiderationSet();
		considerationSetComponent.EnemyInMeleeRange = EnemyInMeleeRange.ToConsiderationDefinition(baker);
		considerationSetComponent.EnemyInRangedRange = EnemyInRangedRange.ToConsiderationDefinition(baker);
		considerationSetComponent.ThreatLevel = ThreatLevel.ToConsiderationDefinition(baker);
		considerationSetComponent.ExplorationOpportunity = ExplorationOpportunity.ToConsiderationDefinition(baker);
		considerationSetComponent.DistanceFromSquire = DistanceFromSquire.ToConsiderationDefinition(baker);
		considerationSetComponent.HungerLevel = HungerLevel.ToConsiderationDefinition(baker);
		considerationSetComponent.HealthLevel = HealthLevel.ToConsiderationDefinition(baker);
		considerationSetComponent.StaminaLevel = StaminaLevel.ToConsiderationDefinition(baker);
		considerationSetComponent.IdlingComfort = IdlingComfort.ToConsiderationDefinition(baker);
		considerationSetComponent.CombatReadiness = CombatReadiness.ToConsiderationDefinition(baker);
		baker.AddComponent(baker.GetEntity(TransformUsageFlags.None), considerationSetComponent);
	}
	
}
