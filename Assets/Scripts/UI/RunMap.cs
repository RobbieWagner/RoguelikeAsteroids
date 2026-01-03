using System;
using System.Collections.Generic;
using System.Linq;
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
            SetCanvasTransform(run);

            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
        }

        private void SetCanvasTransform(Run run)
        {
            Vector2 canvasPos = Vector2.down * tierDisplayDistance * (run.currentNode != null ? run.currentNode.tier : 0) * 2;
            Debug.Log(canvasPos);
            canvasGroup.transform.position = canvasPos; 
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
            LevelNode currentNode = run.currentNode;

            foreach (LevelButton button in levelButtonInstances)
                button.DisableButton();

            List<LevelButton> presentButtons = new List<LevelButton>();
            List<LevelButton> futureButtons = new List<LevelButton>();
            List<LevelButton> pastButtons = new List<LevelButton>();

            if (currentNode == null) // if no current node, enable the root nodes only
            {
                presentButtons.AddRange(levelButtonInstances.Where(b => b.levelNode.tier == 0));
                futureButtons.AddRange(levelButtonInstances.Where(b => b.levelNode.tier != 0));
            }
            else
            {
                presentButtons.AddRange(levelButtonInstances.Where(b => 
                    currentNode.connections.Any(conn => conn.Equals(b.levelNode))));
                HashSet<LevelNode> reachableNodes = FindReachableNodes(currentNode);
                
                futureButtons.AddRange(levelButtonInstances.Where(b => 
                    reachableNodes.Contains(b.levelNode) && 
                    !presentButtons.Contains(b)));
                
                pastButtons.AddRange(levelButtonInstances.Where(b => 
                    !presentButtons.Contains(b) && 
                    !futureButtons.Contains(b)));
            }

            ApplyButtonStates(presentButtons, futureButtons, pastButtons);
        }

        private HashSet<LevelNode> FindReachableNodes(LevelNode startNode)
        {
            HashSet<LevelNode> reachable = new HashSet<LevelNode>();
            Queue<LevelNode> toProcess = new Queue<LevelNode>();
            
            foreach (LevelNode connection in startNode.connections)
            {
                reachable.Add(connection);
                toProcess.Enqueue(connection);
            }
            
            while (toProcess.Count > 0)
            {
                LevelNode current = toProcess.Dequeue();
                
                foreach (LevelNode next in current.connections)
                {
                    if (!reachable.Contains(next))
                    {
                        reachable.Add(next);
                        toProcess.Enqueue(next);
                    }
                }
            }
            
            return reachable;
        }

        private void ApplyButtonStates(List<LevelButton> presentButtons, List<LevelButton> futureButtons, List<LevelButton> pastButtons)
        {
            foreach (LevelButton button in presentButtons)
            {
                button.EnableButton();
                button.button.image.color = GameConstants.Instance.levelColors[button.level.levelType];
            }
            
            foreach (LevelButton button in futureButtons)
            {
                button.DisableButton();
                Color futureColor = GameConstants.Instance.levelColors[button.level.levelType];
                futureColor *= 0.5f;
                button.button.image.color = futureColor;
            }
            
            foreach (LevelButton button in pastButtons)
            {
                button.DisableButton();
                Color pastColor = GameConstants.Instance.levelColors[button.level.levelType];
                pastColor *= 0.2f;
                button.button.image.color = pastColor;
            }
        }
    }
}