using UnityEngine;
using UnityEngine.Rendering;

namespace Towers
{
    public class CircleWithEdgeGenerator : MonoBehaviour
    {

        [SerializeField, Range(3, 360)] private int segments = 36;
        [SerializeField] private Material innerMaterial;
        [SerializeField] private Material edgeMaterial;

        private GameObject _innerCircleObject;
        private GameObject _edgeObject;

        private MeshRenderer _innerMeshRenderer, _edgeMeshRenderer;

        [SerializeField] private Color availableColor, unavailableColor;


        public void DestroyCircle()
        {
            if (this == null) return;
            Destroy(_edgeObject);
            Destroy(_innerCircleObject);
            enabled = false;
        }

        public void Available(bool availabe)
        {
            if (_innerMeshRenderer == null) return;
            _innerMeshRenderer.material.color = availabe ? availableColor : unavailableColor;
        }

        public void GenerateCircleWithEdge(float radius)
        {
            enabled = true;
            if (_innerCircleObject != null)
                DestroyImmediate(_innerCircleObject);

            if (_edgeObject != null)
                DestroyImmediate(_edgeObject);
        
            _innerCircleObject = new GameObject("InnerCircle");
            _innerCircleObject.transform.SetParent(this.transform, false);
            
            MeshFilter innerMeshFilter = _innerCircleObject.AddComponent<MeshFilter>();
            _innerMeshRenderer = _innerCircleObject.AddComponent<MeshRenderer>();
            _innerMeshRenderer.shadowCastingMode  = ShadowCastingMode.Off;
            _innerMeshRenderer.material = innerMaterial != null ? innerMaterial : new Material(Shader.Find("Standard"));
            
            var innerCircleMesh = GenerateCircleMesh(radius, segments);
            innerMeshFilter.mesh = innerCircleMesh;
            
            _edgeObject = new GameObject("EdgeFrame");
            _edgeObject.transform.SetParent(transform, false);
            _edgeObject.transform.position += Vector3.up * 0.1f;
            _edgeObject.transform.eulerAngles -= new Vector3(90, 0, 0);
            _innerCircleObject.transform.eulerAngles -= new Vector3(90, 0, 0);
            
            MeshFilter edgeMeshFilter = _edgeObject.AddComponent<MeshFilter>();
            _edgeMeshRenderer = _edgeObject.AddComponent<MeshRenderer>();
            _edgeMeshRenderer.shadowCastingMode  = ShadowCastingMode.Off;
            _edgeMeshRenderer.material = edgeMaterial != null ? edgeMaterial : new Material(Shader.Find("Standard"));
            
            Mesh edgeMesh = GenerateEdgeMesh(radius, radius + 0.1f, segments);
            edgeMeshFilter.mesh = edgeMesh;
        }
        

        private void Update()
        {
            _innerCircleObject.transform.localPosition = Vector3.up * 0.1f;
            _edgeObject.transform.localPosition = Vector3.up * 0.1f;
        }

        private Mesh GenerateCircleMesh(float radius, int segments)
        {
            Mesh mesh = new Mesh();
            mesh.name = "InnerCircleMesh";

            // Initialize vertices array (center + perimeter points)
            Vector3[] vertices = new Vector3[segments + 1];
            vertices[0] = Vector3.zero; // Center vertex

            float angleIncrement = 2 * Mathf.PI / segments;

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleIncrement * (i - 1);
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                vertices[i] = new Vector3(x, y, 0f);
            }

            // Initialize triangles array
            int[] triangles = new int[segments * 3];

            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0; // Center vertex
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
        private Mesh GenerateEdgeMesh(float innerRadius, float outerRadius, int segments)
        {
            Mesh mesh = new Mesh();
            mesh.name = "EdgeFrameMesh";

            // Initialize vertices array (inner and outer perimeter points)
            Vector3[] vertices = new Vector3[segments * 2];
            int[] triangles = new int[segments * 6];

            float angleIncrement = 2 * Mathf.PI / segments;

            // Create vertices for inner and outer rings
            for (int i = 0; i < segments; i++)
            {
                float angle = angleIncrement * i;
                float xInner = Mathf.Cos(angle) * innerRadius;
                float yInner = Mathf.Sin(angle) * innerRadius;
                float xOuter = Mathf.Cos(angle) * outerRadius;
                float yOuter = Mathf.Sin(angle) * outerRadius;

                vertices[i * 2] = new Vector3(xInner, yInner, 0f);      // Inner vertex
                vertices[i * 2 + 1] = new Vector3(xOuter, yOuter, 0f);  // Outer vertex
            }

            // Create triangles for the edge frame with corrected winding order
            for (int i = 0; i < segments; i++)
            {
                int innerIndex = i * 2;
                int outerIndex = innerIndex + 1;
                int nextInnerIndex = (innerIndex + 2) % (segments * 2);
                int nextOuterIndex = (outerIndex + 2) % (segments * 2);

                // Triangle 1 (Clockwise)
                triangles[i * 6] = innerIndex;
                triangles[i * 6 + 1] = outerIndex;
                triangles[i * 6 + 2] = nextInnerIndex;

                // Triangle 2 (Clockwise)
                triangles[i * 6 + 3] = outerIndex;
                triangles[i * 6 + 4] = nextOuterIndex;
                triangles[i * 6 + 5] = nextInnerIndex;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
        
    }
}
