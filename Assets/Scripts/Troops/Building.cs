using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Players;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Players;
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
        [SerializeField] private int possibleLinks = 0;
        [SerializeField] private int level = 1;
        [SerializeField] private Sprite icon;
        private List<Link> _myLinks = new List<Link>();
        private bool _active;
        private bool _linkedToTroops;

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

        #region Public Variables

        public PlayerTeam GetTroopTeam() => _team;
        public Rigidbody GetTroopRigidbody() => GetComponent<Rigidbody>();
        protected Renderer[] GetRenderers() => renderers;

        #endregion

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

        protected void SetBuilding()
        {
            SetActive(buildingType == BuildingType.Troops);
        }
        
        #region Public Variables
        public Link CreateLink() => Instantiate(link, transform);
        public BuildingType GetBuildingType() => buildingType;
        public List<Link> GetMyLinks() => _myLinks;
        public string GetId() => id;
        public bool IsActive() => _active;
        public Sprite GetIcon() => icon;
        public int GetLevel() => level;
        public int GetLinksCount() => _myLinks.Count;
        public int GetPossibleLinks() => possibleLinks;
        public void IncrementPossibleLinks(int nbr = 1) => possibleLinks += nbr;
        public void DecrementPossibleLinks(int nbr = 1) => possibleLinks -= nbr;
        #endregion
        
        #region Links

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
        #endregion
        
        #region Visuals

        public void IncrementLevel()
        {
            level++;
        }
        
        public void Highlite()
        {
            foreach (var r in GetRenderers())
            {
                float highlightValue = (Mathf.Sin(Time.time * Mathf.PI) + 1) / 2;
                r.material.SetFloat("_Highlite", highlightValue);
            }
        }
        public void RemoveHighlite()
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
