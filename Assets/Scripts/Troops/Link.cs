using System;
using Core;
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
        public Building _building1, _building2;
        private int random;

        public void SetLink(PlayerTeam team)
        {
            renderer.material.SetColor("_Line_Color",
                team == PlayerTeam.Player1 ? GameManager.Instance.player1Color : GameManager.Instance.player2Color);
        }
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
            vertices[2] = localEndPos - right; // Top-left
            vertices[3] = localEndPos + right; // Top-right

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
            if (ReverseLink(building1.GetBuildingType(), building2.GetBuildingType()))
            {
                _building1 = building2;
                _building2 = building1;
                ShowLink(building2.transform.position, building1.transform.position);
            }
            else
            {
                _building1 = building1;
                _building2 = building2;
            }
        }
        private bool ReverseLink(BuildingType building1, BuildingType building2)
        {
            if (building1 == BuildingType.Weapon && building2 == BuildingType.Troops) return true;
            if (building1 == BuildingType.Buff && building2 == BuildingType.Weapon) return true;
            return false;
        }
        public bool IsBeenCut(Vector3 cutPoint)
        {
            var point1 = _building1.transform.position;
            var point2 = _building2.transform.position;

            point1.y = 0;
            point2.y = 0;
            cutPoint.y = 0;

            var lineDirection = (point2 - point1).normalized;
            var toCutPoint = cutPoint - point1;

            var projectionLength = Vector3.Dot(toCutPoint, lineDirection);
            var closestPoint = point1 + projectionLength * lineDirection;

            var distanceToLine = Vector3.Distance(cutPoint, closestPoint);
            var withinSegment = projectionLength >= 0 && projectionLength <= Vector3.Distance(point1, point2);

            return distanceToLine <= .5f && withinSegment;
        }
        public void Cut()
        {
            _building1.RemoveLink(this);
            _building2.RemoveLink(this);
        }
        
        public bool LinkToActiveBuilding()
        {
            if (_building1.IsActive()) return true;
            if (_building2.IsActive()) return true;
            return false;
        }

        public bool IsLinkedToTroops()
        {
            return _building1.GetBuildingType() == BuildingType.Troops ||
                   _building2.GetBuildingType() == BuildingType.Troops;
        }
        
        public Building GetLinkedBuilding(Building building)
        {
            return _building1 == building ? _building2 : _building1;
        }
        
        public void SetAllLinkedBuildingsActive()
        {
           _building1.SetActive(true);
           _building2.SetActive(true);
        }
    }
}