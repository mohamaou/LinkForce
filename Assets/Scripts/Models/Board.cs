using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Players;
using Troops;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Models
{
    public class Board : MonoBehaviour
    {
        public static Board Instance { get; private set; }
        [SerializeField] private bool makeRocks;
        [SerializeField] private Transform[] player1BoardPoints, player2BoardPoints;
        [SerializeField] private GameObject[] rocks;
        private readonly List<Tile> _player1Tiles = new (), _player2Tiles = new ();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetTiles();
            if (makeRocks)
            {
                foreach (var tile in GetRandomTiles(PlayerTeam.Player1))
                {
                    var rock = Instantiate(rocks[Random.Range(0, rocks.Length)], tile.Position, Quaternion.identity);
                    tile.SetRock(rock);
                }
                foreach (var tile in GetRandomTiles(PlayerTeam.Player2))
                {
                    var rock = Instantiate(rocks[Random.Range(0, rocks.Length)], tile.Position, Quaternion.identity);
                    tile.SetRock(rock);
                }
            }
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
            var availableTiles = tiles.Where(tile => !tile.IsFull() && tile.BuildingType == buildingType).ToList();

            if (availableTiles.Count == 0) return Vector3.zero;

            var tile = availableTiles[Random.Range(0, availableTiles.Count)];
            tile.SetFull(true);
            return tile.Position;
        }
        

        public void BuildingMerged(Vector3 mergePosition, PlayerTeam team)
        { 
            var mergedTile = PositionToTile(position: mergePosition);
            foreach (var tile in GetNearByTiles(mergedTile,team))
            {
                if(tile.HasRock()) tile.RemoveRock();
            }
        }
        
        
        #region Tiles
        private List<Tile> GetRandomTiles(PlayerTeam team)
        {
            var allTiles = team == PlayerTeam.Player1 ? _player1Tiles : _player2Tiles;
            if (allTiles.Count == 0) return new List<Tile>();

            int minCount = Mathf.CeilToInt(allTiles.Count * 0.4f);
            int maxCount = Mathf.CeilToInt(allTiles.Count * 0.6f);
            if (maxCount < minCount) maxCount = minCount;

            int targetCount = Random.Range(minCount, maxCount + 1);
            if (targetCount > allTiles.Count) targetCount = allTiles.Count;

            var shuffledTiles = allTiles.OrderBy(x => Random.value).ToList();
            var zBinCount = new Dictionary<int, int>();
            var selectedTiles = new List<Tile>();

            foreach (var tile in shuffledTiles)
            {
                if (selectedTiles.Count >= targetCount)
                    break;

                int zBin = Mathf.RoundToInt(tile.Position.z);

                if (!zBinCount.ContainsKey(zBin))
                    zBinCount[zBin] = 0;

                if (zBinCount[zBin] < 3)
                {
                    zBinCount[zBin]++;
                    selectedTiles.Add(tile);
                }
            }

            if (selectedTiles.Count < minCount)
            {
                foreach (var tile in shuffledTiles)
                {
                    if (selectedTiles.Count >= minCount)
                        break;
                    if (!selectedTiles.Contains(tile))
                        selectedTiles.Add(tile);
                }
            }

            return selectedTiles;
        }
        public void ClearTile(Vector3 tile)
        {
            PositionToTile(tile).SetFull(false);
        }
        private Tile PositionToTile(Vector3 position)
        {
            var closetDistance = float.MaxValue;
            Tile tile = null;
            var availableTiles = _player1Tiles.Concat(_player2Tiles).ToList();
            foreach (var t in availableTiles)
            {
                var dist = Vector3.Distance(t.Position, position);
                if (dist < closetDistance)
                {
                    closetDistance = dist;
                    tile = t;
                }
            }
            return tile;
        }
        private Tile[] GetNearByTiles(Tile tile, PlayerTeam team)
        {
            var tiles = team == PlayerTeam.Player1 ? _player1Tiles : _player2Tiles;
            if (tiles == null || tiles.Count == 0) return System.Array.Empty<Tile>();

            var up = tiles
                .Where(t => Mathf.Abs(t.Position.x - tile.Position.x) < 0.01f && t.Position.z > tile.Position.z)
                .OrderBy(t => t.Position.z)
                .FirstOrDefault();

            var down = tiles
                .Where(t => Mathf.Abs(t.Position.x - tile.Position.x) < 0.01f && t.Position.z < tile.Position.z)
                .OrderByDescending(t => t.Position.z)
                .FirstOrDefault();

            var left = tiles
                .Where(t => Mathf.Abs(t.Position.z - tile.Position.z) < 0.01f && t.Position.x < tile.Position.x)
                .OrderByDescending(t => t.Position.x)
                .FirstOrDefault();

            var right = tiles
                .Where(t => Mathf.Abs(t.Position.z - tile.Position.z) < 0.01f && t.Position.x > tile.Position.x)
                .OrderBy(t => t.Position.x)
                .FirstOrDefault();

            return new[] { up, down, left, right }.Where(t => t != null).ToArray();
        }


        #endregion
        

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
                    color = Color.white; 
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
            private bool isFull;
            public BuildingType BuildingType;
            private GameObject rock;

            public void SetRock(GameObject r) => rock = r;
            public bool HasRock()=> rock != null;
            public void SetFull(bool f) => isFull = f;

            public void RemoveRock()
            {
                if (rock == null) return;
                rock.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBounce).OnComplete(() =>
                {
                    Destroy(rock);
                });
            }

            public bool IsFull()
            {
                if(isFull) return true;
                if (HasRock()) return true;
                return false;
            }
            
            public Tile(Vector3 position, BuildingType buildingType)
            {
                Position = position;
                isFull = false;
                BuildingType = buildingType;
            }
        }
    }
}