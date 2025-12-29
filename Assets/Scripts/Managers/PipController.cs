using System;
using System.Collections.Generic;
using System.Linq;
using RobbieWagnerGames.Utilities;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class PipController : MonoBehaviourSingleton<PipController>
	{
		[SerializeField] private ResourcePip resourcePipPrefab;
		
		private List<ResourcePip> activePips = new List<ResourcePip>();
		
		public event Action<ResourcePip> OnPipCollected;
		public event Action OnAllPipsCleared;
		
		protected override void Awake()
		{
			base.Awake();
			
			LevelManager.Instance.OnLevelFailed += ClearAllPips;
			LevelManager.Instance.OnLevelCompleted += ClearAllPips;
		}
		
		protected override void OnDestroy()
		{
			base.OnDestroy();
			
			if (LevelManager.Instance != null)
			{
				LevelManager.Instance.OnLevelFailed -= ClearAllPips;
				LevelManager.Instance.OnLevelCompleted -= ClearAllPips;
			}
			
			ClearAllPips();
		}
		
		public void SpawnResourcePips(Transform parent, ResourceGatherData resourceData)
		{
			if (resourcePipPrefab == null || resourceData == null || resourceData.resources == null) 
				return;
			
			foreach (KeyValuePair<ResourceType, int> resource in resourceData.resources)
			{
				if (resource.Key == ResourceType.NONE || resource.Value <= 0) 
					continue;
				
				int remainingAmount = resource.Value;
				
				while (remainingAmount >= 10)
				{
					SpawnPip(parent, resource.Key, 10);
					remainingAmount -= 10;
				}
				
				while (remainingAmount >= 5)
				{
					SpawnPip(parent, resource.Key, 5);
					remainingAmount -= 5;
				}
				
				while (remainingAmount > 0)
				{
					SpawnPip(parent, resource.Key, 1);
					remainingAmount -= 1;
				}
			}
		}
		
		private void SpawnPip(Transform parent, ResourceType resourceType, int amount)
		{
			if (resourcePipPrefab == null) return;
			
			ResourcePip pip = Instantiate(resourcePipPrefab, parent.position, Quaternion.identity, transform);
			
			if (pip != null)
			{
				pip.Initialize(resourceType, amount);
				pip.AddRandomForce(UnityEngine.Random.Range(1f, 3f));
				
				pip.OnPipCollected += HandlePipCollected;
				pip.OnPipDestroyed += HandlePipDestroyed;
				
				activePips.Add(pip);
			}
		}
		
		private void HandlePipCollected(ResourcePip pip, ResourceType resourceType, int amount)
		{
			if (ResourceManager.Instance != null)
				ResourceManager.Instance.AddResource(resourceType, amount);
			
			OnPipCollected?.Invoke(pip);
			
			if (activePips.Remove(pip))
			{
				pip.OnPipCollected -= HandlePipCollected;
				pip.OnPipDestroyed -= HandlePipDestroyed;
				Destroy(pip.gameObject);
			}
		}
		
		private void HandlePipDestroyed(ResourcePip pip)
		{
			if (activePips.Remove(pip))
			{
				pip.OnPipCollected -= HandlePipCollected;
				pip.OnPipDestroyed -= HandlePipDestroyed;
			}
		}
		
		public void ClearAllPips(Level level = null)
		{
			foreach (var pip in activePips)
			{
				if (pip != null)
				{
					pip.OnPipCollected -= HandlePipCollected;
					pip.OnPipDestroyed -= HandlePipDestroyed;
					Destroy(pip.gameObject);
				}
			}
			activePips.Clear();
			
			OnAllPipsCleared?.Invoke();
		}
		
		public void DestroyPipsOfType(ResourceType resourceType)
		{
			List<ResourcePip> pipsToDestroy = activePips.Where(pip => pip.ResourceType == resourceType).ToList();
			
			foreach (var pip in pipsToDestroy)
			{
				if (pip != null)
				{
					activePips.Remove(pip);
					pip.OnPipCollected -= HandlePipCollected;
					pip.OnPipDestroyed -= HandlePipDestroyed;
					Destroy(pip.gameObject);
				}
			}
		}
		
		public int GetActivePipCount() => activePips.Count;
		public int GetActivePipCount(ResourceType resourceType) => 
			activePips.Count(pip => pip.ResourceType == resourceType);
		
		public bool HasActivePips() => activePips.Any();
	}
}