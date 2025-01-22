using Players;
using UnityEngine;

namespace Troops
{
    public class Link : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private new Renderer renderer;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private float uvScale = 1f;
        [SerializeField] private Color player1Color, player2Color;
        private Building _building1, _building2;

        
        public void SetLink(PlayerTeam team) =>
            renderer.material.SetColor("_Color", team == PlayerTeam.Player1 ? player1Color : player2Color);
        public void ShowLink(Vector3 startPos, Vector3 endPos)
        {
            startPos += Vector3.up * 0.1f;
            endPos += Vector3.up * 0.1f;
            var mesh = new Mesh();

            var localStartPos = transform.InverseTransformPoint(startPos);
            var localEndPos = transform.InverseTransformPoint(endPos);

            var direction = (localEndPos - localStartPos).normalized;
            var distance = Vector3.Distance(localStartPos, localEndPos);
            var halfWidth = lineWidth / 2;

            var right = Vector3.Cross(Vector3.up, direction).normalized * halfWidth;

            Vector3[] vertices = new Vector3[4];
            int[] triangles = new int[6];
            Vector2[] uvs = new Vector2[4];

            vertices[0] = localStartPos - right; // Bottom-left
            vertices[1] = localStartPos + right; // Bottom-right
            vertices[2] = localEndPos - right;   // Top-left
            vertices[3] = localEndPos + right;   // Top-right

            triangles[0] = 0;
            triangles[1] = 2;
            triangles[2] = 1;
            triangles[3] = 1;
            triangles[4] = 2;
            triangles[5] = 3;

            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(0, 1);
            uvs[2] = new Vector2(distance * uvScale, 0);
            uvs[3] = new Vector2(distance * uvScale, 1);

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
        }

        public void SetBuildings(Building building1, Building building2)
        {
            _building1 = building1;
            _building2 = building2;
        }
    }
}
