using System.Collections.Generic;
using System.Collections;
using Core;
using UnityEngine;
using MoreMountains.Feedbacks;
using Players;
using DG.Tweening;
using UI;
using Unity.Mathematics;
using UnityEditor;

namespace Troops
{
    public enum BuildingType
    {
        Troops,
        Weapon,
        Buff
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
        [SerializeField] private Troop troopPrefab;
        [SerializeField] private BuildingType buildingType;
        [SerializeField] private TroopType equipmentType;
        [SerializeField] private Link link;
        [SerializeField] private int numberOfTroops = 4;
        [SerializeField] private Sprite icon;
        
        private int _linksToTroops = 0;
        private int _linksToBuffs = 0;
        private int _level = 1;
        private bool _active;
        private readonly List<Link> _myLinks = new();
        private bool _linkedToTroops;
        private BuildingUI _buildingPanel;
        private DestroyUI _destroyPanel;

        #region Public Variables
        public PlayerTeam GetTroopTeam() => _team;
        public Rigidbody GetTroopRigidbody() => GetComponent<Rigidbody>();
        public Renderer[] GetRenderers() => renderers;
        public Link CreateLink() => Instantiate(link, transform);
        public BuildingType GetBuildingType() => buildingType;
        public BuildingUI GetBuildingPanel() => _buildingPanel;
        public MMFeedbacks GetLandFeedback() => landFeedback;
        public List<Link> GetMyLinks() => _myLinks;
        public TroopType GetEquipmentType() => equipmentType;
        public bool IsActive() => _active;
        public Sprite GetIcon() => icon;
        public int GetLevel() => _level;
        public int GetLinksCount() => _myLinks.Count;
        public int GetLinksToTroops() => _linksToTroops;
        public int GetLinksToBuffs() => _linksToBuffs;
        public void IncrementLinksToBuffs() => _linksToBuffs++;
        public void DecrementLinksToBuffs() => _linksToBuffs--;
        public List<Building> GetLinkedBuffBuilding()
        {
            var buildings = new List<Building>();
            foreach (var l in _myLinks)
            {
                if(l.GetLinkedBuilding(this).GetBuildingType() == BuildingType.Buff) 
                    buildings.Add(l.GetLinkedBuilding(this));
            }
            return buildings;
        }
        #endregion

        public void Start()
        {
            var screenPosition1 = GameManager.Camera.WorldToScreenPoint(transform.position);
            var screenPosition2 = GameManager.Camera.WorldToScreenPoint(transform.position + new Vector3(0, 3f, 0));

            var buildingUI = Instantiate(UIManager.Instance.BuildingPanel,
                UIManager.Instance.playPanel.buildingsPanel.transform);
            buildingUI.InitializePanel(icon, _level, buildingType,
                _team);
            _buildingPanel = buildingUI;
            _buildingPanel.transform.position = screenPosition1;

            var destroyPanel = Instantiate(UIManager.Instance.destroyPanelPrefab, _buildingPanel.transform);
            _destroyPanel = destroyPanel;
            _destroyPanel.transform.position = screenPosition2;
            StartCoroutine(UIHandler());
            if (buildingType == BuildingType.Troops) StartCoroutine(SpawnTroops());
        }

        private IEnumerator SpawnTroops()
        {
            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Battle);

            for (var i = 0; i < numberOfTroops; i++)
            {
                var targetX = i % 2 == 0 ? ((-i / 3f) - 0.2f) : ((i / 3f) + 0.2f);
                var targetPosition = transform.position + new Vector3(targetX, 0f, transform.forward.z * 1.75f);

                var spawnPosition = transform.position + new Vector3(0f, 0f, transform.forward.z);
                var spawnRotation = Quaternion.LookRotation(transform.forward);
                var troop = Instantiate(troopPrefab, spawnPosition, spawnRotation);

                troop.SetTroop(_team,_level);
                if(_myLinks.Count > 0) troop.GoToBuilding(_myLinks[0].GetLinkedBuilding(this));
                else troop.GoToBattle();
                
                yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Summon);
            StartCoroutine(SpawnTroops());
        }

        private IEnumerator UIHandler()
        {
            while (_buildingPanel != null)
            {
                _buildingPanel.transform.position = GameManager.Camera.WorldToScreenPoint(transform.position);
                yield return null;
            }
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
                if (l.LinkToActiveBuilding()) active = true;
            }

            if (!active) return;
            foreach (var l in _myLinks)
            {
                l.SetAllLinkedBuildingsActive();
            }
        }

        public void RemoveLink(Link myLink)
        {
            _myLinks.Remove(myLink);
        }


        public void SetDestroyRewardUI(bool state)
        {
            _destroyPanel.gameObject.SetActive(state);
        }

        public List<Link> GetAllLinks() => _myLinks;

        public void RemoveAllLinks()
        {
            foreach (var l in _myLinks)
            {
                Destroy(l.gameObject);
            }

            _myLinks.Clear();
        }

        public void IncrementLevel()
        {
            _level++;
            _buildingPanel.UpdateLevelNumber(_level);
        }

        public void IncrementLinksToTroops()
        {
            if (_linksToTroops != 0)
                _buildingPanel.AddLinkPoint(_team == PlayerTeam.Player1
                    ? GameManager.Instance.player1Color
                    : GameManager.Instance.player2Color);
            _linksToTroops++;
        }

        public void DecrementLinksToTroops()
        {
            if (_linksToTroops > 1) _buildingPanel.RemoveLinkPoint();
            _linksToTroops--;
        }

        public void SetLinksToTroops(int nbr)
        {
            _linksToTroops = nbr;
        }

        public void SetLinksToBuffs(int nbr)
        {
            _linksToBuffs = nbr;
        }

        private void OnDestroy()
        {
            if (_buildingPanel != null) Destroy(_buildingPanel.gameObject);
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

        public void TroopEntered(Troop troop)
        {
            
        }
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