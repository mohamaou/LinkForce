using System.Collections.Generic;
using System.Linq;
using Players;
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
            foreach (var tile in player1BoardPoints)
            {
                _player1Tiles.Add(new Tile(tile.position));
            }

            foreach (var tile in player2BoardPoints)
            {
                _player2Tiles.Add(new Tile(tile.position));
            }
        }

        public Vector3 GetRandomBoardPoint(PlayerTeam team)
        {
            var tiles = team == PlayerTeam.Player1 ? _player1Tiles : _player2Tiles;
            var availableTiles = tiles.Where(tile => !tile.IsFull).ToList();

            if (availableTiles.Count == 0) return Vector3.zero;
            
            var tile = availableTiles[Random.Range(0, availableTiles.Count)];
            tile.IsFull = true;
            return tile.Position;
        }
        
        private class Tile
        {
            public Vector3 Position;
            public bool IsFull;

            public Tile(Vector3 position)
            {
                Position = position;
                IsFull = false;
            }
        }
    }
}
