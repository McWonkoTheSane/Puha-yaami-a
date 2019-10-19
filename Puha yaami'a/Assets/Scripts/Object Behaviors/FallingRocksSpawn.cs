﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingRocksSpawn : MonoBehaviour
{
    [Tooltip("The rock I spawn")]
    public GameObject rockObj;
    [Tooltip("How far apart rocks should fall")]
    public int rockSpacing = 2;
    private int spawnerWidth;
    [Tooltip("how long in seconds between rocks falling at each sub spawn location. Internally staggered between all locations")]
    public float timeBetweenSpawns = 3f;
    private List<RockSpawnLoc> spawnLocations;
    // Start is called before the first frame update
    void Start()
    {
        spawnLocations = new List<RockSpawnLoc>();
        spawnerWidth = Mathf.RoundToInt(this.GetComponent<SpriteRenderer>().size.x);
        for(int i = 0; i < spawnerWidth; i += rockSpacing)
        {
            float timerOffset = Random.Range(0, timeBetweenSpawns);
            RockSpawnLoc newSpawn = new RockSpawnLoc(new Vector2(i, 0), timeBetweenSpawns, timerOffset);
            spawnLocations.Add(newSpawn);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //loop through them all and decrease timers, then reassign once things have been changed
        for(int i = 0; i < spawnLocations.Count; i++)
        {
            RockSpawnLoc loc = spawnLocations[i];
            if (loc.timerCurr <= 0)
            {
                Vector3 vec3Loc = new Vector3(loc.loc.x - spawnerWidth / 2, 0, loc.loc.y);
                Instantiate(rockObj, this.transform.position + vec3Loc, Quaternion.identity);
                loc.timerCurr = loc.timerMax;
            }
            else
            {
                loc.timerCurr -= Time.deltaTime;
            }
            spawnLocations[i] = loc;
        }
    }

    [System.Serializable]
    private struct RockSpawnLoc
    {
        public Vector2 loc;
        public float timerMax;
        public float timerCurr;
        public RockSpawnLoc(Vector2 _loc, float _timer, float _timerStart)
        {
            loc = _loc;
            timerMax = _timer;
            timerCurr = _timerStart;
        }
    }
}