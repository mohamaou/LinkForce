using Core;
using DG.Tweening;
using Players;
using UnityEditor;
using UnityEngine;

namespace Level
{
    public class ArenaGfx : MonoBehaviour
    {
        private static readonly int Player1Lerp = Shader.PropertyToID("_Player_1_Lerp");
        private static readonly int Player2Lerp = Shader.PropertyToID("_Player_2_Lerp");
        private static readonly int Player1Reverse = Shader.PropertyToID("_Player_1_Reverse");
        private static readonly int Player2Reverse = Shader.PropertyToID("_Player_2_Reverse");
        
        [SerializeField] private Age age;
        [SerializeField] private GameObject gfx;
        [SerializeField] private Renderer[] renders;
        private bool _player1Active, _player2Active;
        


        public Age GetAge() => age;
        
        public void SetComponents()
        {
            gfx = transform.GetChild(0).gameObject;
            renders = gfx.GetComponentsInChildren<Renderer>();
        }

        public void Transition(bool active, PlayerTeam player, bool transition)
        {
            if (active) gfx.SetActive(true);
            if (transition)
            {
                if (player == PlayerTeam.Player1 && !_player1Active && !active) return;
                if (player == PlayerTeam.Player2 && !_player2Active && !active) return;
            }
            if(player == PlayerTeam.Player1) _player1Active = active;
            if(player == PlayerTeam.Player2) _player2Active = active;
            for (int i = 0; i < renders.Length; i++)
            {
                var mat = renders[i].material;
                mat.SetFloat(player == PlayerTeam.Player1 ? Player1Reverse : Player2Reverse, active ? 1f : 0f);
                mat.SetFloat(player == PlayerTeam.Player1 ? Player1Lerp : Player2Lerp, 0);
                var startValue = mat.GetFloat(player == PlayerTeam.Player1 ? Player1Lerp : Player2Lerp);
                var endValue = 30f;
                var duration = transition ? 1f : 0;
                DOTween.To(() => startValue, x =>
                {
                    mat.SetFloat(player == PlayerTeam.Player1 ? Player1Lerp : Player2Lerp, x);
                }, endValue, duration).SetEase(Ease.Linear).OnComplete(() =>
                {
                    if(!_player1Active && !_player2Active)gfx.SetActive(false);
                });
            }
        }
    }
    
    #if UNITY_EDITOR
[CanEditMultipleObjects] 
[CustomEditor(typeof(ArenaGfx), true)]
public class TowerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Assign Components"))
        {
            foreach (var t in targets)
            {
                var eachTower = (ArenaGfx)t;
                eachTower.SetComponents();
                EditorUtility.SetDirty(eachTower);
            }
        }
    }
}
#endif
}
