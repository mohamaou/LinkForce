using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Players;
using DG.Tweening;
using UI;
using UnityEditor;

namespace Troops
{
    public enum BuildingType
    {
        Troops, Weapon, Buff
    }

    public class Building : MonoBehaviour
    {
        [Header("General")] [SerializeField] private new Collider collider;
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Transform gfx;
        [SerializeField] private MMFeedbacks landFeedback;
        [SerializeField] private Material player1, player2;
        private PlayerTeam _team;
        private Vector3 _localScale;
        [SerializeField] private BuildingType buildingType;
        [SerializeField] private string id;
        [SerializeField] private Link link;
        [SerializeField] private int level = 1;
        [SerializeField] private Sprite icon;
        [SerializeField] private List<Link> _myLinks = new List<Link>();
        [SerializeField] private int _linksToTroops = 0;
        [SerializeField] private int _linksToBuffs = 0;
        private bool _active;
        private bool _linkedToTroops;
        private BuildingUI _buildingPanel;

        #region Public Variables
        public PlayerTeam GetTroopTeam() => _team;
        public Rigidbody GetTroopRigidbody() => GetComponent<Rigidbody>();
        public Renderer[] GetRenderers() => renderers;
        public Link CreateLink() => Instantiate(link, transform);
        public BuildingType GetBuildingType() => buildingType;
        public List<Link> GetMyLinks() => _myLinks;
        public string GetId() => id;
        public bool IsActive() => _active;
        public Sprite GetIcon() => icon;
        public int GetLevel() => level;
        public int GetLinksCount() => _myLinks.Count;
        public int GetLinksToTroops() => _linksToTroops;
        public int GetLinksToBuffs() => _linksToBuffs;
        public void IncrementLinksToBuffs() => _linksToBuffs++;
        public void DecrementLinksToBuffs() => _linksToBuffs--;

        #endregion
        
        public void Start()
        {
            var buildingUI = Instantiate(UIManager.Instance.BuildingPanel, this.transform);
            buildingUI.transform.localPosition = Vector3.zero;
            buildingUI.InitializePanel(this.GetIcon(), this.GetLevel(), buildingType);
            _buildingPanel = buildingUI;
        }

        public void AssignComponents()
        {
            var r = GetComponentsInChildren<Renderer>();
            renderers = r.Length > 0 ? r : null;
            gfx = transform.childCount > 0 ? transform.GetChild(0) : null;
            collider = GetComponent<Collider>();
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Reset()
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0, 0.85f, 0);
            box.size = new Vector3(2.4f, 1.7f, 2.4f);
            var r = gameObject.AddComponent<Rigidbody>();
            r.isKinematic = true;
        }

        public void Set(PlayerTeam team)
        {
            _team = team;
            gfx.localScale = new Vector3(0, 2, 0);
            gfx.localPosition = new Vector3(0, 1, 0);
            _localScale = Vector3.one;
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = team == PlayerTeam.Player1 ? player1 : player2;
            }

            GetComponent<Collider>().isTrigger = false;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            tag = team == PlayerTeam.Player1 ? "Player 1" : "Player 2";
            gfx.DOScale(_localScale, 0.2f).OnComplete(() => landFeedback?.PlayFeedbacks()).SetEase(Ease.OutBack);
            gfx.DOLocalMove(Vector3.zero, 0.2f);
            SetBuilding();
        }

        private void SetBuilding()
        {
            SetActive(buildingType == BuildingType.Troops);
        }
        
        public void SetBuildingLink(Link myLink)
        {
            _myLinks.Add(myLink);
            var active = false;
            foreach (var l in _myLinks)
            { 
                if(l.LinkToActiveBuilding()) active = true;
            }
            
            if(!active) return;
            foreach (var l in _myLinks)
            { 
                l.SetAllLinkedBuildingsActive();
            }
        }
        public void RemoveLink(Link myLink)
        {
            _myLinks.Remove(myLink);
        }

        public void IncrementLevel()
        {
            level++;
            _buildingPanel.UpdateLevelNumber(level);
        }

        public void IncrementLinksToTroops()
        {
            if (_linksToTroops != 0) _buildingPanel.AddLinkPoint();
            _linksToTroops++;
        }

        public void DecrementLinksToTroops()
        {
            if (_linksToTroops > 1) _buildingPanel.RemoveLinkPoint();
            _linksToTroops--;
        }
        
        #region Visuals

        public void Highlight()
        {
            foreach (var r in GetRenderers())
            {
                var highlightValue = (Mathf.Sin(Time.time * Mathf.PI) + 1) / 2;
                r.material.SetFloat("_Highlite", highlightValue);
            }
        }

        public void RemoveHighlight()
        {
            foreach (var r in GetRenderers())
            {
                r.material.SetFloat("_Highlite", 0);
            }
        }
        public void SetActive(bool active)
        {
            _active = active;
            foreach (var r in GetRenderers())
            {
                r.material.SetFloat("_Saturation", active ? 1 : 0);
            }
        }

        public void RunGFX()
        {
            gfx.localScale = new Vector3(0, 2, 0);
            gfx.localPosition = new Vector3(0, 1, 0);
            _localScale = Vector3.one;
            gfx.DOScale(_localScale, 0.2f).OnComplete(() => landFeedback?.PlayFeedbacks()).SetEase(Ease.OutBack);
            gfx.DOLocalMove(Vector3.zero, 0.2f);
            SetBuilding();
        }
        #endregion
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Building), true)]
    public class BuildingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Assign Components"))
            {
                foreach (var t in targets)
                {
                    var eachTower = (Building) t;
                    eachTower.AssignComponents();
                    EditorUtility.SetDirty(eachTower);
                }
            }
        }
    }
#endif
}
