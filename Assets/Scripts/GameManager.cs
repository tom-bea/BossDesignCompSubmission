using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
   #region Global Variables
   private static GameManager singleton;
   public static GameManager Singleton { get { return singleton; } }

   private static bool p_GameRunning = false;
   public static bool GameRunning { get { return p_GameRunning; } }
   #endregion

   #region Delegates and Events
   public delegate void EmptyDelegate();
   public static event EmptyDelegate StartGameEvent;
   #endregion

   #region Editor Variables
   [SerializeField]
   [Tooltip("The number of players that will be playing the game.")]
   private int m_NumberOfPlayers = 2;

   [SerializeField]
   [Tooltip("The player game object.")]
   private GameObject m_PlayerPrefab;

   [SerializeField]
   [Tooltip("The boss game object.")]
   private GameObject m_BossPrefab;

   [SerializeField]
   [Tooltip("The positions where players can spawn.")]
   private Transform[] m_SpawnPositions;
   #endregion

   #region Cached References
   private List<GameObject> cr_Players;
   private GameObject cr_Boss;
   #endregion

   #region Initialization
   private void Awake()
   {
      if (singleton == null)
         singleton = this;
      else if (singleton != this)
         Destroy(gameObject);
   }
   #endregion

   #region OnEnable and OnDisable
   private void OnEnable()
   {
      Player.PlayerDiedEvent += PlayerHasDied;
      Boss.BossKilledEvent += EndGame;
   }

   private void OnDisable()
   {
      Player.PlayerDiedEvent -= PlayerHasDied;
      Boss.BossKilledEvent -= EndGame;
   }
   #endregion

   #region Main Updates
   private void Update()
   {
      if (p_GameRunning)
         RunningGameUpdate();
      else
         NonRunningGameUpdate();
   }
   #endregion

   #region Secondary Updates
   private void RunningGameUpdate()
   {

   }

   private void NonRunningGameUpdate()
   {
      for (int i = 1; i <= m_NumberOfPlayers; i++)
      {
         if (Input.GetButtonDown("Interact" + i))
            StartGame();
      }
   }
   #endregion

   #region Accessors and Mutators   
   public Transform GetRandomPlayer()
   {
      return cr_Players[Random.Range(0, cr_Players.Count)].transform;
   }
   #endregion

   #region Game Starting Methods
   private void StartGame()
   {
      SpawnPlayers();
      cr_Boss = Instantiate(m_BossPrefab);

      StartGameEvent?.Invoke();

      p_GameRunning = true;
   }

   private void SpawnPlayers()
   {
      cr_Players = new List<GameObject>();
      for (int i = 0; i < m_NumberOfPlayers; i++)
      {
         cr_Players.Add(SpawnAndSetupPlayer(i + 1));
         Vector3 newPos = m_SpawnPositions[i].position;
         newPos.z = 0;
         cr_Players[i].transform.position = newPos;
      }
   }

   private GameObject SpawnAndSetupPlayer(int id)
   {
      GameObject player = Instantiate(m_PlayerPrefab);
      player.GetComponent<Player>().PlayerID = id;
      return player;
   }
   #endregion

   #region Game Running Methods
   private void PlayerHasDied(int id)
   {
      if (cr_Players.Count == 1)
         EndGame();
      for (int i = 0; i < cr_Players.Count; i++)
      {
         if (cr_Players[i].GetComponent<Player>().PlayerID == id)
         {
            cr_Players.RemoveAt(i);
            break;
         }
      }
   }

   private void EndGame()
   {
      p_GameRunning = false;
      Destroy(cr_Boss.gameObject);
      for (int i = 0; i < cr_Players.Count; i++)
         Destroy(cr_Players[i]);
      cr_Players.Clear();
   }
   #endregion
}
