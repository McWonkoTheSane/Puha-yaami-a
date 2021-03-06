﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject playerObj;
    public Animator[] ravenAnimators;
    private GameObject mainCamera;
    private Player player;
    public float deathDelay = 1.5f;
    [SerializeField] private Vector3 lastCheckpoint;
    [SerializeField]private Vector3 playerStartPos;
    [SerializeField] private int plantsSteppedOn = 0;
    //public Text scoreText;

    [SerializeField] private float bouncyModifier = 1.5f;
    [SerializeField] private float stickyModifier = 0.5f;

    public delegate void OnFailEvents();
    public static event OnFailEvents StandardLevelFail;
    //[SerializeField] private List<Vector3> checkpoints;

    void OnEnable()
    {
        Player.OnCollide += HandlePlatformCollide;
        Player.OnTrigger += HandleTriggerCollision;
    }

    void OnDisable()
    {
        Player.OnCollide -= HandlePlatformCollide;
        Player.OnTrigger -= HandleTriggerCollision;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject levelUI = GameObject.FindObjectOfType<PauseMenu>().gameObject;
        ravenAnimators = levelUI.GetComponentsInChildren<Animator>();
        if (playerObj == null)
        {
            playerObj = FindObjectOfType<Player>().gameObject;
        }
        player = playerObj.GetComponent<Player>();
        //UpdateScoreText();
        playerStartPos = player.transform.position;
        lastCheckpoint = playerStartPos;
        mainCamera = playerObj.GetComponentInChildren<Camera>().gameObject;
        //checkpoints.Add(playerStartPos);

        //UNCOMMENT ME TO TURN ON SAVE/LOAD
        //LoadData();
    }

    private void LoadData()
    {
        //Load data
        PlayerData pData = SaveSystem.LoadPlayer();

        //Fill in transform
        Vector3 playerLoadedPos;
        playerLoadedPos.x = pData.pos[0];
        playerLoadedPos.y = pData.pos[1];
        playerLoadedPos.z = pData.pos[2];

        //Move player to loaded transform
        player.transform.position = playerLoadedPos;

        //unlock the correct stuff
        bool gu = pData.glideUnlocked;
        bool du = pData.dashUnlocked;
        bool dju = pData.dblJumpUnlocked;
        if (dju)
            player.UnlockAbility(0);
        if (du)
            player.UnlockAbility(1);
        if (gu)
            player.UnlockAbility(2);

        //Load gm data
        GameManagerData gmData = SaveSystem.LoadGM();

        //Update score
        //score = gmData.score;
        //UpdateScoreText();

        //Load last checkpoint and update
        Vector3 loadedLastCheckpoint;
        loadedLastCheckpoint.x = gmData.lastCheckpoint[0];
        loadedLastCheckpoint.y = gmData.lastCheckpoint[1];
        loadedLastCheckpoint.z = gmData.lastCheckpoint[2];

        //Load all stored checkpoints and update
        /*        List<Vector3> loadedCheckpoints = new List<Vector3>();
                foreach(Vec3 pos in gmData.checkpointPositions)
                {
                    Vector3 loadedChkpt;
                    loadedChkpt.x = pos.x;
                    loadedChkpt.y = pos.y;
                    loadedChkpt.z = pos.z;

                    loadedCheckpoints.Add(loadedChkpt);
                }*/

        //checkpoints = loadedCheckpoints;
        lastCheckpoint = loadedLastCheckpoint;
    }

    void HandlePlatformCollide(int type)
    {
        switch(type)
        {
            //normal
            case 0: 
                break;
            //bouncy
            case 1:
                player.SetJumpForce(player.GetDefaultJumpForce() * bouncyModifier);
                break;
            //sticky
            case 2:
                player.SetJumpForce(player.GetDefaultJumpForce() * stickyModifier);
                break;
            //fail object
            case 3:
                OnFail();
                break;
            //score loss
            case 4:
                ScoreLoss();
                break;
        }
    }

    void HandleTriggerCollision(string type, Collider2D collider)
    {
        switch(type)
        {
            case "Checkpoint":
                /*                if(!checkpoints.Contains(checkpointCollider.transform.position))
                                {*/
                if (lastCheckpoint != collider.transform.position)
                {
                    lastCheckpoint = collider.transform.position;
                }
                    //checkpoints.Add(checkpointCollider.transform.position);
                //}
                break;
            case "Double_Jump_Unlock":
                player.UnlockAbility(0);
                break;
            case "Dash_Unlock":
                player.UnlockAbility(1);
                break;
            case "Glide_Unlock":
                player.UnlockAbility(2);
                break;
            case "Fail_Pit":
                StartCoroutine(PitCameraLock());
                break;
        }
    }

    IEnumerator PitCameraLock()
    {
        player.isDead = true;
        mainCamera.transform.parent = null;
        yield return new WaitForSeconds(deathDelay);
        OnFail();
    }

    void OnFail()
    {
        if (lastCheckpoint != null)
        {
            playerObj.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            playerObj.transform.position = lastCheckpoint;
            player.ResetCooldowns();
            mainCamera.transform.SetParent(playerObj.transform);
            mainCamera.transform.localPosition = new Vector3(0, 10, -50);
            player.isDead = false;
        }
        else
        {
            Debug.LogError("NO CHECKPOINTS FOUND");
        }
        //fire fail event
        StandardLevelFail?.Invoke();
    }

    void ScoreLoss()
    {
        plantsSteppedOn += 1;
        UpdateRavens();
    }

    void UpdateRavens()
    {
        if(plantsSteppedOn <= 3)
        {
            ravenAnimators[plantsSteppedOn - 1].SetTrigger("Takeoff");
        }
    }

    public int GetScore()
    {
        return plantsSteppedOn;
    }

/*    public List<Vector3> GetCheckpoints()
    {
        return checkpoints;
    }*/

    public Vector3 GetLastCheckpoint()
    {
        return lastCheckpoint;
    }
}
