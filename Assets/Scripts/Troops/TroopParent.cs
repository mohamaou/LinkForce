using DG.Tweening;
using MoreMountains.Feedbacks;
using Players;
using UnityEditor;
using UnityEngine;

namespace Troops
{
    public class TroopParent : MonoBehaviour
    { 
        [Header("General")] 
        [SerializeField] private new Collider collider;
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private Renderer[] renderers; 
        [SerializeField] private Transform gfx;
        [SerializeField] private MMFeedbacks landFeedback;
        [SerializeField] private Material player1, player2;
        private PlayerTeam _team;
        private Vector3 _localScale;
        
        
        
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
        #endregion
        
        public void Set(PlayerTeam team)
        {
            _team = team; 
            gfx.localScale = new Vector3(0,2,0);
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
        }
    }
    
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TroopParent), true)]
    public class TroopParentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Assign Components"))
            {
                foreach (var t in targets)
                {
                    var eachTower = (TroopParent)t;
                    eachTower.AssignComponents();
                    EditorUtility.SetDirty(eachTower);
                }
            }
        }
    }
#endif
}
