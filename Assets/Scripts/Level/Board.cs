using System;
using System.Collections.Generic;
using System.Linq;
using Players;
using Troops;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Level
{
    public class Board : MonoBehaviour
    {
        public static Board Instance{ get; private set; }
        [SerializeField] private Transform[] player1BoardPoints, player2BoardPoints;
        private List<Tile> _player1Tiles = new List<Tile>(), _player2Tiles = new List<Tile>();
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetTiles();
        }

        private void SetTiles()
        {
            for (int i = 0; i < player1BoardPoints.Length; i++)
            {
                if (i < 5)
                {
                    _player1Tiles.Add(new Tile(player1BoardPoints[i].position, BuildingType.Troops));
                }
                else if (i < 10)
                {
                    _player1Tiles.Add(new Tile(player1BoardPoints[i].position, BuildingType.Weapon));
                }
                else
                {
                    _player1Tiles.Add(new Tile(player1BoardPoints[i].position, BuildingType.Buff));
                }
            }

            for (int i = 0; i < player2BoardPoints.Length; i++)
            {
                if (i < 5)
                {
                    _player2Tiles.Add(new Tile(player2BoardPoints[i].position, BuildingType.Troops));
                }
                else if (i < 10)
                {
                    _player2Tiles.Add(new Tile(player2BoardPoints[i].position, BuildingType.Weapon));
                }
                else
                {
                    _player2Tiles.Add(new Tile(player2BoardPoints[i].position, BuildingType.Buff));
                }
            }
        }


        public Vector3 GetRandomBoardPoint(PlayerTeam team, BuildingType buildingType)
        {
            var tiles = team == PlayerTeam.Player1 ? _player1Tiles : _player2Tiles;
            var availableTiles = tiles.Where(tile => !tile.IsFull && tile.BuildingType == buildingType).ToList();

            if (availableTiles.Count == 0) return Vector3.zero;

            var tile = availableTiles[Random.Range(0, availableTiles.Count)];
            tile.IsFull = true;
            return tile.Position;
        }


        private void OnDrawGizmosSelected()
        {
            if (_player1Tiles != null)
            {
                foreach (var tile in _player1Tiles)
                {
                    DrawTileGizmo(tile);
                }
            }

            if (_player2Tiles != null)
            {
                foreach (var tile in _player2Tiles)
                {
                    DrawTileGizmo(tile);
                }
            }
        }

        private void DrawTileGizmo(Tile tile)
        {
            Color color;
            switch (tile.BuildingType)
            {
                case BuildingType.Troops:
                    color = Color.blue;
                    break;
                case BuildingType.Weapon:
                    color = Color.red;
                    break;
                case BuildingType.Buff:
                    color = Color.green;
                    break;
                default:
                    color = Color.white; // Fallback color
                    break;
            }

            Gizmos.color = color;
            Gizmos.DrawSphere(tile.Position, 0.5f); // Draw a sphere to represent the tile
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(tile.Position, 0.5f); // Draw a wireframe around it for clarity
        }



        private class Tile
        {
            public Vector3 Position;
            public bool IsFull;
            public BuildingType BuildingType;

            public Tile(Vector3 position, BuildingType buildingType)
            {
                Position = position;
                IsFull = false;
                BuildingType = buildingType;
            }
        }
    }
}
