using System;
using System.Collections.Generic;
using RobbieWagnerGames.UI;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class RunMap : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas nodeCanvas;
        private RectTransform canvasTransform => nodeCanvas.transform as RectTransform;

        [SerializeField] private LevelButton levelButtonPrefab;
        private List<LevelButton> levelButtonInstances = new List<LevelButton>();
        [SerializeField] private Transform nodeParent;

        [SerializeField] private LineRenderer lineRendererPrefab;
        private List<LineRenderer> lineRendererInstances = new List<LineRenderer>(); 
        [SerializeField] private Transform lineParent;

        [SerializeField] private float tierDisplayDistance;
        [SerializeField] private float sideBuffer;
        [SerializeField] private Vector2 baseNodePlacement;

        protected void Awake()
        {
            RunManager.Instance.OnRunContinued += DisplayRunUI;
            RunManager.Instance.OnStartLevel += HideRunUI;
        }

        

        private void DisplayRunUI(Run run)
        {
            ClearUI();
            InstantiateNodes(run);
            InstantiateConnections(run);
            ConfigureIntractability(run);

            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
        }

        private void HideRunUI(Level level)
        {
            ClearUI();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
        }

        private void ClearUI()
        {
            foreach(LevelButton button in levelButtonInstances)
                Destroy(button.gameObject);
            levelButtonInstances.Clear();

            foreach(LineRenderer renderer in lineRendererInstances)
                Destroy(renderer.gameObject);
            lineRendererInstances.Clear();
        }

        private void InstantiateNodes(Run run)
        {
            for (int i = 0; i < run.levelTree.Count; i++)
            {
                List<LevelNode> nodes = run.levelTree[i];
                float nodeYPos = -canvasTransform.rect.height / 2 + baseNodePlacement.y + (tierDisplayDistance * i);
                
                for(int j = 0; j < nodes.Count; j++)
                {
                    float nodeXPos = 0;
                    if (nodes.Count > 1)
                    {
                        float spacing = (canvasTransform.rect.width - (sideBuffer * 2)) / (nodes.Count - 1);
                        nodeXPos = -canvasTransform.rect.width / 2 + sideBuffer + (spacing * j);
                    }
                    
                    LevelButton button = Instantiate(levelButtonPrefab, nodeParent);
                    button.transform.localPosition = new Vector3(nodeXPos, nodeYPos, 0);
                    button.level = nodes[j].level;
                    button.levelNode = nodes[j];
                    UpdateButtonAppearance(button);
                    
                    levelButtonInstances.Add(button);
                }
            }
        }

        private void UpdateButtonAppearance(LevelButton button)
        {
            button.button.image.sprite = GameConstants.Instance.levelIcons[button.level.levelType];
            button.button.image.color = GameConstants.Instance.levelColors[button.level.levelType];
        }

        private void InstantiateConnections(Run run)
        {
            Dictionary<LevelNode, LevelButton> nodeToButtonMap = new Dictionary<LevelNode, LevelButton>();
            
            foreach (LevelButton button in levelButtonInstances)
            {
                if (button.levelNode != null)
                    nodeToButtonMap[button.levelNode] = button;
            }
            
            for (int tier = 0; tier < run.levelTree.Count; tier++)
            {
                List<LevelNode> currentTier = run.levelTree[tier];
                
                foreach (LevelNode fromNode in currentTier)
                {
                    if (!nodeToButtonMap.ContainsKey(fromNode))
                        continue;
                        
                    LevelButton fromButton = nodeToButtonMap[fromNode];
                    
                    foreach (LevelNode toNode in fromNode.connections)
                    {
                        if (!nodeToButtonMap.ContainsKey(toNode))
                            continue;
                            
                        LevelButton toButton = nodeToButtonMap[toNode];

                        LineRenderer lineRenderer = Instantiate(lineRendererPrefab, lineParent);
                        
                        lineRenderer.transform.localPosition = Vector3.zero;
                        lineRenderer.transform.localRotation = Quaternion.identity;
                        
                        lineRenderer.positionCount = 2;
                        
                        Vector3 fromPos = fromButton.transform.localPosition;
                        Vector3 toPos = toButton.transform.localPosition;
                        
                        lineRenderer.SetPosition(0, fromPos);
                        lineRenderer.SetPosition(1, toPos);
                        
                        UpdateConnectionAppearance(lineRenderer, fromNode, toNode);
                        
                        lineRendererInstances.Add(lineRenderer);
                    }
                }
            }
        }

        private void UpdateConnectionAppearance(LineRenderer lineRenderer, LevelNode fromNode, LevelNode toNode)
        {
            lineRenderer.startWidth = 0.06f;
            lineRenderer.endWidth = 0.006f;
            
            lineRenderer.startColor = GameConstants.Instance.levelColors[fromNode.level.levelType];
            lineRenderer.endColor = GameConstants.Instance.levelColors[toNode.level.levelType];
        }

        
        private void ConfigureIntractability(Run run)
        {
            foreach(LevelButton button in levelButtonInstances)
            {
                button.DisableButton();

                if (button.level.tier < run.currentTier)
                    button.button.image.color = new Color(.1f, .1f, .1f, 1f);
                else if (button.level.tier == run.currentTier)
                    button.EnableButton();
                else
                    button.button.image.color = new Color(.4f, .4f, .4f, .4f);
            }
        }
    }
}