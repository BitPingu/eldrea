using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElfPosition : MonoBehaviour
{
    public Vector3 spawnPoint;
    private TilemapStructure groundMap;

    [SerializeField]
    private float maxDistance; // default is 3.5f
    public Vector2Int currentPos;
    public bool inDanger;

    public Dialogue dialogue;
    private bool triggerDia;
    public GameObject helpIcon;
    private GameObject helpIconChild;

    // public static ElfPosition elf;

    private void Awake()
    {
        // if (TempData.elfSaved)
        // {
        //     if (elf == null)
        //     {
        //         elf = this;
        //     } 
        //     else 
        //     {
        //         Destroy(gameObject);
        //         return;
        //     }

        //     DontDestroyOnLoad(gameObject);
        // }

        // Attach delegates
        // PosChange += CheckPosition;
        // PosChange += OTileSound;

        // Set event icon
        // if (!TempData.elfSaved)
        // {
        //     Vector3 iconPos = new Vector3(transform.position.x, transform.position.y + 1);
        //     newIcon = Instantiate(icon, iconPos, Quaternion.identity);
        //     newIcon.transform.parent = gameObject.transform;
        //     newIcon.GetComponent<EventIconData>().SetIcon("Event");
        // }
    }

    // Update is called once per frame
    private void Update()
    {
        if (inDanger)
        {
            // Stop moving
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            if (FindObjectOfType<PlayerPosition>().transform.position.x - transform.position.x > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }

            if (CheckPlayer())
            {
                helpIconChild.SetActive(true);
            }
            else
            {
                helpIconChild.SetActive(false);
            }

            if (CheckClosePlayer() && !FindObjectOfType<DialogueController>().isActive && !triggerDia)
            {
                triggerDia = true;
                FindObjectOfType<DialogueController>().StartDialogue("Helpless Elf");
                FindObjectOfType<DialogueController>().AddPrompt(new Dialogue("Please help me!"));
                FindObjectOfType<DialogueController>().DisplayNextSentence();
            }
            if (!CheckClosePlayer() && triggerDia)
            {
                triggerDia = false;
                FindObjectOfType<DialogueController>().EndDialogue();
            }
        }

        // Retrieve coordinates
        currentPos = Vector2Int.FloorToInt(transform.position);
        TempData.tempElfPos = new Vector3(transform.position.x, transform.position.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (inDanger && collision.gameObject.name.Contains("Player"))
        {
            SaveElf();
            FindObjectOfType<BattleManager>().Initiate(collision.gameObject, GameObject.FindGameObjectWithTag("SpecialEnemy"));
        }
    }

    public void InDanger()
    {
        Vector3 iconPos = new Vector3(transform.position.x, transform.position.y+1.5f);
        helpIconChild = Instantiate(helpIcon, iconPos, Quaternion.identity, transform);
        helpIcon.GetComponent<EventIconData>().SetIcon("Event");
        GetComponent<Animator>().SetBool("Jump", true);
        GetComponent<PartyMovement>().enabled = false;
        inDanger = true;
    }

    public void SaveElf()
    {
        // TempData.elfSaved = true;
        // newIcon.SetActive(false);
        // TempData.initElf = false;
        inDanger = false;
        Destroy(helpIconChild);
        GetComponent<Animator>().SetBool("Jump", false);
        GetComponent<PartyMovement>().minDistance = 0.8f;
        GetComponent<PartyMovement>().enabled = true;
        // DontDestroyOnLoad(gameObject);
    }

    public IEnumerator SaveElf2()
    {
        FindObjectOfType<DialogueController>().StartDialogue("Helpless Elf");
        FindObjectOfType<DialogueController>().AddPrompt(new Dialogue("Thanks!"));
        FindObjectOfType<DialogueController>().DisplayNextSentence();
        yield return new WaitForSeconds(1f);
        FindObjectOfType<DialogueController>().EndDialogue();
        GetComponent<PartyMovement>().minDistance = 1.55f;
        TempData.elfSaved = true;
    }

    private void RetrieveTilemap()
    {
        // Retrieve tilemap components
        groundMap = FindObjectOfType<TileGrid>().GetTilemap(TilemapType.Ground);
        // overworldMap = FindObjectOfType<TileGrid>().GetTilemap(TilemapType.Overworld);
    }

    // Generates a random spawn point
    // public void Spawn(bool initialSpawn, int scene)
    // {
    //     RetrieveTilemap();
    //     switch (scene)
    //     {
    //         case 1:
    //             if (!TempData.elfSaved)
    //             {
    //                 // Not saved yet
    //                 // Debug.Log("elf in danger");
    //                 if (initialSpawn)
    //                 {
    //                     // Debug.Log("new elf");
    //                     // Coming from main menu
    //                     if (TempData.newGame)
    //                     {
    //                         // Generate initial spawn point
    //                         float xCoord, yCoord, currentTile;
    //                         // Get player spawn
    //                         Vector3 worldSpawn = TempData.tempPlayerStartingSpawn;
    //                         do
    //                         {
    //                             // Choose random spawn point
    //                             xCoord = Random.Range(worldSpawn.x-5, worldSpawn.x+5);
    //                             yCoord = Random.Range(worldSpawn.y-5, worldSpawn.y+5);

    //                             // Check tile
    //                             currentTile = groundMap.GetTile((int)xCoord, (int)yCoord);
    //                         }
    //                         while (currentTile != (int)GroundTileType.Land);

    //                         // Generate spawn point
    //                         spawnPoint = new Vector3(xCoord, yCoord);
    //                         TempData.tempElfStartingSpawn = spawnPoint;
    //                     }
    //                     else
    //                     {
    //                         // Debug.Log("load unsaved elf");
    //                         // Load unsaved elf position
    //                         SaveData saveData = SaveSystem.Load();
    //                         spawnPoint.x = saveData.saveElfPos[0];
    //                         spawnPoint.y = saveData.saveElfPos[1];
    //                         spawnPoint.z = saveData.saveElfPos[2];
    //                     }
    //                 }
    //                 else
    //                 {
    //                     // Debug.Log("notsaved, going to spawn");
    //                     // Stay at spawn point
    //                     spawnPoint = TempData.tempElfStartingSpawn;
    //                 }
    //                 // Attacked by slime
    //                 GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    //                 GetComponent<Animator>().SetBool("Jump", true);
    //                 GetComponent<PartyMovement>().enabled = false;
    //             }
    //             else
    //             {
    //                 // saved
    //                 if (initialSpawn)
    //                 {
    //                     // Coming from main menu (only possible when load)
    //                     // Debug.Log("load elf");
    //                     // Load elf position
    //                     SaveData saveData = SaveSystem.Load();
    //                     spawnPoint.x = saveData.saveElfPos[0];
    //                     spawnPoint.y = saveData.saveElfPos[1];
    //                     spawnPoint.z = saveData.saveElfPos[2];
    //                 }
    //                 else
    //                 {
    //                     // exiting from building
    //                     // Debug.Log("saved2, going to player");
    //                     spawnPoint = TempData.tempPlayerPos;
    //                 }
    //             }
    //             break;
    //         case 2:
    //         case 3:
    //             // Dungeon or village spawn
    //             // Debug.Log("building, going to player");
    //             spawnPoint = TempData.tempPlayerPos;
    //             break;
    //         default:
    //             Debug.Log("elf cannot spawn!");
    //             break;
            
    //     }

    //     // Set spawn point
    //     transform.position = spawnPoint;
    //     prevPos = currentPos = Vector2Int.FloorToInt(transform.position);

    //     // Retrieve spawn point tile
    //     // prevGTile = currentGTile = groundMap.GetTile(currentPos.x, currentPos.y);
    //     // prevOTile = currentOTile = overworldMap.GetTile(currentPos.x, currentPos.y);
    // }

    private bool CheckPlayer()
    {
        // Calculate current distance from player
        float distance = Vector3.Distance(FindObjectOfType<PlayerPosition>().transform.position, transform.position);

        if (distance < maxDistance)
        {
            // Debug.Log("hi");
            return true;
        }

        return false;
    }

    private bool CheckClosePlayer()
    {
        // Calculate current distance from player
        float distance = Vector3.Distance(FindObjectOfType<PlayerPosition>().transform.position, transform.position);

        if (distance < maxDistance-1.5)
        {
            return true;
        }

        return false;
    }

    // Play sound based on current object tile
    // private void OTileSound()
    // {
    //     if (CheckPlayer())
    //     {
    //         // Get sound from current object tile
    //         if (currentOTile == (int)FoilageTileType.Tree)
    //         {
    //             FindObjectOfType<AudioManager>().PlayFx("Tree");
    //         }
    //     }
    // }
}
