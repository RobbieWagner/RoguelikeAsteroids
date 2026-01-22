using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class RunHUD : MonoBehaviour
	{
		[SerializeField] private Canvas canvas;
		[SerializeField] private ResourceUI resourceUIPrefab;
		[SerializeField] private LayoutGroup resourcesList;
		[SerializeField] private TextMeshProUGUI vpText;
		private List<ResourceUI> displayedResourceUIs = new List<ResourceUI>();
		
		private Dictionary<ResourceType, ResourceUI> activeResourceUIs = new Dictionary<ResourceType, ResourceUI>();

		private void Awake()
		{
			RunManager.Instance.OnRunContinued += DisplayHUD;
            RunManager.Instance.OnStartLevel += HideHUD;
			ResourceManager.Instance.OnResourcesUpdated += OnResourcesUpdated;
			ResourceManager.Instance.OnResourcesReset += OnResourcesReset;
		}

        private void DisplayHUD(Run run)
        {
			canvas.enabled = true;
			InitializeResourceUI();
			vpText.text = $"x {run.victoryPoints}";
        }
		
        private void HideHUD(Level level)
        {
            canvas.enabled = false;
        }
		
		private void InitializeResourceUI()
		{
			ClearResourceUIs();
			
			foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
			{
				if (type != ResourceType.NONE)
				{
					int amount = GetResourceAmount(type);
					CreateResourceUI(type, amount);
				}
			}
		}
		
		private void ClearResourceUIs()
		{
			foreach (ResourceUI ui in activeResourceUIs.Values)
			{
				if (ui != null && ui.gameObject != null)
					Destroy(ui.gameObject);
			}
			activeResourceUIs.Clear();
			displayedResourceUIs.Clear();
		}
		
		private void CreateResourceUI(ResourceType resourceType, int amount)
		{
			if (resourceUIPrefab == null || resourcesList == null)
			{
				Debug.LogError("Resource UI prefab or resources list not assigned!");
				return;
			}
			
			ResourceUI newUI = Instantiate(resourceUIPrefab, resourcesList.transform);
			newUI.Initialize(resourceType, amount);
			activeResourceUIs[resourceType] = newUI;
			displayedResourceUIs.Add(newUI);
		}
		
		private void UpdateResourceUI(ResourceType resourceType, int amount)
		{
			if (!activeResourceUIs.ContainsKey(resourceType) || activeResourceUIs[resourceType] == null)
				CreateResourceUI(resourceType, amount);
			else
				activeResourceUIs[resourceType].UpdateAmount(amount);
		}
		
		private void OnResourcesUpdated(Dictionary<ResourceType, int> resources)
		{
			if (resources == null) return;
			
			foreach (KeyValuePair<ResourceType, int> resource in resources)
			{
				if (resource.Key != ResourceType.NONE)
					UpdateResourceUI(resource.Key, resource.Value);
			}
		}
		
		private void OnResourcesReset()
		{
			InitializeResourceUI();
		}
		
		private int GetResourceAmount(ResourceType resourceType)
		{
			if (ResourceManager.Instance != null && ResourceManager.Instance.gatheredResources.ContainsKey(resourceType))
				return ResourceManager.Instance.gatheredResources[resourceType];
			return 0;
		}
		
		private void OnDestroy()
		{
			RunManager.Instance.OnRunContinued -= DisplayHUD;
			RunManager.Instance.OnStartLevel -= HideHUD;
			
			ResourceManager.Instance.OnResourcesUpdated -= OnResourcesUpdated;
			ResourceManager.Instance.OnResourcesReset -= OnResourcesReset;
		}
	}
}