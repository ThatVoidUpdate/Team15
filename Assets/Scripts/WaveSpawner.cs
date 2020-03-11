﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using System;

public enum EnemyType { Standard, Faster, MoreDamage, Strong, Ninja, Fast, Poison, Bloated, Beefy}

public class WaveSpawner : MonoBehaviour
{
    public string FileName;
    public string FallbackFileName;

    public GameObject StandardEnemy;
    public GameObject FasterEnemy;
    public GameObject MoreDamageEnemy;
    public GameObject StrongEnemy;
    public GameObject NinjaEnemy;
    public GameObject FastEnemy;
    public GameObject PoisonEnemy;
    public GameObject HexagonBloated;
    public GameObject BeefyHexagon;

    public GameObject Boss;

    public TextMeshProUGUI WaveCounter;

    public List<(GameObject, int)[]> Waves = new List<(GameObject, int)[]>();

    private List<GameObject> CurrentEnemies = new List<GameObject>();
    private GameObject[] spawners;
    public int CurrentWave = -1;

    private Dictionary<EnemyType, GameObject> EnemyTypes;

    // Start is called before the first frame update
    void Start()
    {
        //load file as array of strings
        //each line is a wve
        //syntax: enemytype amount, enemytype amount, enemytype amount
        //lines beginning with # are comments

        EnemyTypes = new Dictionary<EnemyType, GameObject>() { { EnemyType.Standard, StandardEnemy }, 
            { EnemyType.Faster, FasterEnemy }, 
            { EnemyType.MoreDamage, MoreDamageEnemy}, 
            { EnemyType.Strong, StrongEnemy}, 
            { EnemyType.Ninja, NinjaEnemy}, 
            { EnemyType.Fast, FastEnemy}, 
            { EnemyType.Poison, PoisonEnemy},
            { EnemyType.Bloated, HexagonBloated},
            { EnemyType.Beefy, BeefyHexagon}};

        string FilePath;
        if (File.Exists(FileName))
        {
            FilePath = FileName;
        }
        else
        {
            FilePath = FallbackFileName;
        }

        string[] lines = File.ReadAllLines(FilePath);
        lines = (from line in lines where !line.StartsWith("#") select line).ToArray();
        lines = (from line in lines where line != "" select line).ToArray();

        foreach (string line in lines)
        {
            string[] spawns = line.Split(',');
            List<(GameObject, int)> wave = new List<(GameObject, int)>();

            foreach (string spawn in spawns)
            {
                EnemyType type;
                if (!Enum.TryParse(spawn.Trim().Split(' ')[0], out type))
                {
                    Debug.LogWarning("Enemy type " + spawn.Trim().Split(' ')[0] + " does not exist (" + FilePath + ")");

                }
                wave.Add((EnemyTypes[type], Convert.ToInt32(spawn.Trim().Split(' ')[1])));
                
            }
            Waves.Add(wave.ToArray());            
        }

        spawners = GameObject.FindGameObjectsWithTag("Spawner");
    }


    void Update()
    {
        if (CurrentEnemies.Count == 0)
        {//All enemies have been killed. MOAR ENEMIES
            CurrentWave++;
            WaveCounter.text = "Wave: " + CurrentWave.ToString();

            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                player.GetComponent<Player>().StartWave();
            }

            if (CurrentWave > Waves.Count && Boss != null && Boss.activeSelf == false)
            {//We defeated all waves. time for the boss battle
                Boss.SetActive(true);
                foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
                {
                    FindObjectOfType<BarkEvents>().TriggerBarkLine(BarkEventTypes.Boss, player);
                }
            }

            foreach (GameObject spawner in spawners)
            {
                foreach ((GameObject, int) set in Waves[CurrentWave])
                {
                    CurrentEnemies.AddRange(spawner.GetComponent<Spawner>().Spawn(set.Item1, set.Item2));
                }
            }
        }
        else
        {
            CurrentEnemies.RemoveAll(item => item == null);
        }
    }
}
