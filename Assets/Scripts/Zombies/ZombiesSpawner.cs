using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using Level;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Zombies
{
    [Serializable]
    public class SpawnPoint
    {
        public Transform spawnPoint;
        public bool opened;
        public Rigidbody rb;

        public void Open()
        {
            opened = true;
            rb.isKinematic = false;
            rb.transform.DOScale(Vector3.zero,0.5f).SetDelay(2f).OnComplete(()=>rb.isKinematic = true);
        }
    }
    public class ZombiesSpawner : MonoBehaviour
    {
        public static ZombiesSpawner Instance { get; private set; }
        [SerializeField] private bool spawnZombies;
        [SerializeField] private Transform zombiesParent;
        [SerializeField] private SpawnPoint[] player1SpawnPoints, player2SpawnPoints;
        [SerializeField] private Zombie[] zombies;
        [SerializeField] private float arenaLenght = 6f;
        private List<Zombie> _player1Zombies = new List<Zombie>(), _player2Zombies = new List<Zombie>();
        

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetDoorsState();
        }
        
        private void SetDoorsState()
        {
            for (int i = 0; i < player1SpawnPoints.Length; i++)
            {
                player1SpawnPoints[i].rb.isKinematic = true;
                player1SpawnPoints[i].opened = false;
            }
            for (int i = 0; i < player2SpawnPoints.Length; i++)
            {
                player2SpawnPoints[i].rb.isKinematic = true;
                player2SpawnPoints[i].opened = false;
            }
        }

        public void OpenRandomDoor()
        {
            var player1CloseDoors = new List<SpawnPoint>();
            for (int i = 0; i < player1SpawnPoints.Length; i++)
            {
                if (!player1SpawnPoints[i].opened) player1CloseDoors.Add(player1SpawnPoints[i]);
            }
            if (player1CloseDoors.Count > 0) player1CloseDoors[Random.Range(0, player1CloseDoors.Count)].Open();
            
            var player2CloseDoors = new List<SpawnPoint>();
            for (int i = 0; i < player2SpawnPoints.Length; i++)
            {
                if (!player2SpawnPoints[i].opened) player2CloseDoors.Add(player2SpawnPoints[i]);
            }
            if (player2CloseDoors.Count > 0) player2CloseDoors[Random.Range(0, player2CloseDoors.Count)].Open();
        }
        public void OpenDoor(int doorIndex)
        {
            var player1CloseDoors = new List<SpawnPoint>();
            for (int i = 0; i < player1SpawnPoints.Length; i++)
            {
                if (!player1SpawnPoints[i].opened) player1CloseDoors.Add(player1SpawnPoints[i]);
            }
            if (player1CloseDoors.Count > 0) player1CloseDoors[doorIndex].Open();
            
            var player2CloseDoors = new List<SpawnPoint>();
            for (int i = 0; i < player2SpawnPoints.Length; i++)
            {
                if (!player2SpawnPoints[i].opened) player2CloseDoors.Add(player2SpawnPoints[i]);
            }
            if (player2CloseDoors.Count > 0) player2CloseDoors[doorIndex].Open();
        }
        
        public IEnumerator SpawnZombies(Round round, Action roundEnds)
        {
            var listsCount = round.zombiesInfo.Length;
            for (int i = 0; i < round.zombiesInfo.Length; i++)
            {
                StartCoroutine(SpawnZombiesList(round.zombiesInfo[i], ()=> listsCount--));
            }
            yield return new WaitUntil(() => listsCount == 0);
            roundEnds?.Invoke();
        }
        private IEnumerator SpawnZombiesList(ZombiesInfo zombiesInfo, Action roundEnds)
        {
            yield return new WaitForSeconds(zombiesInfo.timeToStart);
            for (int i = 0; i < zombiesInfo.zombiesCount; i++)
            {
                SpawnZombieInBothSides(zombiesInfo.zombieType);
                yield return new WaitForSeconds(zombiesInfo.spawnTime);
            }
            roundEnds?.Invoke();
        }

        
        
        private void SpawnZombieInBothSides(ZombieType zombieType)
        {
            if (!spawnZombies) return;
            SpawnZombie(PlayerTeam.Player1, zombieType, true);
            SpawnZombie(PlayerTeam.Player2, zombieType,true);
        }
        public void SpawnZombie(PlayerTeam playerTeam, ZombieType zombieType, bool natural)
        {
            //if (playerTeam == PlayerTeam.Player2 && !GameManager.Instance.makeZombiesForTheEnemy) return;
            var selectedZombies = new List<Zombie>();
            for (int i = 0; i < zombies.Length; i++)
            {
                if(zombies[i].GetZombieType()== zombieType) selectedZombies.Add(zombies[i]);
            }
            if(selectedZombies.Count == 0) return;
            var z = Instantiate(selectedZombies[Random.Range(0,selectedZombies.Count)], GetRandomSpawnPoint(playerTeam),Quaternion.identity);
            z.SetTarget(playerTeam,natural);
            z.transform.SetParent(zombiesParent);
            if (playerTeam == PlayerTeam.Player1) _player1Zombies.Add(z);
            else _player2Zombies.Add(z);
        }
        public void SpawnZombie(PlayerTeam playerTeam, ZombieType zombieType, Vector3 spawnPoint, bool natural)
        {
            var selectedZombies = new List<Zombie>();
            for (int i = 0; i < zombies.Length; i++)
            {
                if(zombies[i].GetZombieType()== zombieType) selectedZombies.Add(zombies[i]);
            }
            if(selectedZombies.Count == 0) return;
            var z = Instantiate(selectedZombies[Random.Range(0,selectedZombies.Count)], spawnPoint,Quaternion.identity);
            z.SetTarget(playerTeam ,natural);
            z.transform.SetParent(zombiesParent);
            if (playerTeam == PlayerTeam.Player1) _player1Zombies.Add(z);
            else _player2Zombies.Add(z);
        }
        private Vector3 GetRandomSpawnPoint(PlayerTeam playerTeam)
        {
            var points = playerTeam == PlayerTeam.Player1 ? player1SpawnPoints : player2SpawnPoints;
            var availablePoint = new List<Vector3>();
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].opened) availablePoint.Add(points[i].spawnPoint.position);
            }
            if(availablePoint.Count == 0) return Vector3.zero;
            return availablePoint[Random.Range(0, availablePoint.Count)];
        }


        public bool AllZombiesDeath()
        { 
            return  _player1Zombies.Count == 0 &&  _player2Zombies.Count == 0;
        }
        public void ZombieDies(Zombie zombie)
        {
            for (int i = 0; i < _player1Zombies.Count; i++)
            {
                if(zombie == _player1Zombies[i]) _player1Zombies.RemoveAt(i);
            }
            for (int i = 0; i < _player2Zombies.Count; i++)
            {
                if(zombie == _player2Zombies[i]) _player2Zombies.RemoveAt(i);
            }
        }

        public Vector3 GetRandomArenaPoint(PlayerTeam playerTeam, Vector3 playerPos, float radius)
        {
            var points = new List<Vector3>();
            foreach (var point in playerTeam == PlayerTeam.Player1 ? player1SpawnPoints : player2SpawnPoints)
            { 
                if (point.opened)
                {
                    var spawnPos = point.spawnPoint.position + point.spawnPoint.forward * Random.Range(arenaLenght / 3f, arenaLenght);
                    if (Vector3.Distance(spawnPos, playerPos) <= radius)
                    { 
                        spawnPos.x += Random.Range(-0.05f, 0.05f);
                        points.Add(spawnPos);
                    }
                }
            }
            return points.Count == 0 ? Vector3.zero : points[Random.Range(0, points.Count)];
        }

        public Vector3[] GetOpenDoorsPoint(PlayerTeam playerTeam)
        {
            var points = new List<Vector3>();
            foreach (var point in playerTeam == PlayerTeam.Player1 ? player1SpawnPoints : player2SpawnPoints)
            { 
                if (point.opened) points.Add(point.spawnPoint.position);
            }
            return points.ToArray();
        }

        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            var startPosPlayer1 = player1SpawnPoints[2].spawnPoint.position + Vector3.up * 1.5f;
            var endPosPlayer1 = player1SpawnPoints[2].spawnPoint.position + Vector3.back * arenaLenght + Vector3.up * 1.5f;
            Gizmos.DrawSphere(endPosPlayer1, 0.5f);
            Gizmos.DrawLine(startPosPlayer1,endPosPlayer1);
            
            Gizmos.color = Color.red;
            var startPosPlayer2 = player2SpawnPoints[2].spawnPoint.position + Vector3.up * 1.5f;
            var endPosPlayer2 = player2SpawnPoints[2].spawnPoint.position + Vector3.forward * arenaLenght + Vector3.up * 1.5f;
            Gizmos.DrawSphere(endPosPlayer2, 0.5f);
            Gizmos.DrawLine(startPosPlayer2, endPosPlayer2);
        }
    }
}
