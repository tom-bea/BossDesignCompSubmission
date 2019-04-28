using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Boss : MonoBehaviour
{
   #region Delegates and Events
   public delegate void EmptyDelegate();
   public static event EmptyDelegate BossKilledEvent;
   #endregion

   #region Editor Variables
   [SerializeField]
   [Tooltip("How much health the boss has during phase 1.")]
   private int m_MaxHealthPhase1;

   [SerializeField]
   [Tooltip("Minimum amount of time to wait between attacks for phase 1.")]
   private float m_MinWaitTimePhase1 = 2f;

   [SerializeField]
   [Tooltip("Maximum amount of time to wait between attacks for phase 1.")]
   private float m_MaxWaitTimePhase1 = 3.5f;
   
   [SerializeField]
   [Tooltip("The left hand of the boss.")]
   private Transform m_LeftHand;

   [SerializeField]
   [Tooltip("The right hand of the boss.")]
   private Transform m_RightHand;
   #endregion

   #region Private Variables
   private int p_CurHealth;

   private int p_CurPhase = 1;

   private float p_CurWaitTime = 0;

   private Transform m_PlatformLine;
   #endregion

   #region Cached References
   private Transform cr_ChosenPlayer;
   #endregion

   #region Initialization
   private void Awake()
   {
      m_PlatformLine = GameObject.FindGameObjectWithTag("PlatformLine").transform;
   }

   private void Setup()
   {
      p_CurHealth = m_MaxHealthPhase1;
      p_CurPhase = 1;
      p_CurWaitTime = m_MaxWaitTimePhase1;
   }
   #endregion

   #region OnEnable and OnDisable
   private void OnEnable()
   {
      GameManager.StartGameEvent += Setup;
   }

   private void OnDisable()
   {
      GameManager.StartGameEvent -= Setup;
   }
   #endregion

   #region Main Updates
   private void Update()
   {
      if (!GameManager.GameRunning)
         return;

      // Wait a little bit before attacking again
      if (p_CurWaitTime > 0)
      {
         p_CurWaitTime -= Time.deltaTime;
         return;
      }
      else
         p_CurWaitTime = 0;

      // Choose player to attack
      cr_ChosenPlayer = GameManager.Singleton.GetRandomPlayer();

      // Attack the player based on the phase
      switch (p_CurPhase)
      {
         case 1:
            Phase1Attack();
            break;
         default:
            break;
      }
   }
   #endregion

   #region Phase 1 Attack
   private void Phase1Attack()
   {
      // If chosen player is on a platform, swipe
      // If chosen player is on the floor, smash
      if (cr_ChosenPlayer.position.y > m_PlatformLine.position.y)
         StartCoroutine(Swipe());
      else
         StartCoroutine(Drop());

      p_CurWaitTime = Random.Range(m_MinWaitTimePhase1, m_MaxWaitTimePhase1);
   }

   private IEnumerator Swipe()
   {
      // Choose side to swipe from
      Transform handToMove = m_RightHand;
      Vector2 newPos = cr_ChosenPlayer.position;
      newPos.x = 6;
      float finalXPos = -6;
      if (Random.Range(0, 2) == 0)
      {
         handToMove = m_LeftHand;
         newPos.x = -6;
         finalXPos = 6;
      }
      Vector2 origPos = handToMove.position;

      // Move hand to same height as player
      float timer = 0;
      while (timer < 1)
      {
         handToMove.position = Vector2.Lerp(origPos, newPos, timer);
         timer += Time.deltaTime;
         yield return null;
      }

      // Swipe hand across screen
      timer = 0;
      while (timer < 0.35f)
      {
         Vector2 p = new Vector2(Mathf.Lerp(newPos.x, finalXPos, timer / 0.35f), handToMove.position.y);
         handToMove.position = p;
         timer += Time.deltaTime;
         yield return null;
      }

      // Return hand back
      newPos.x = finalXPos;
      timer = 0;
      while (timer < 0.5f)
      {
         handToMove.position = Vector2.Lerp(newPos, origPos, timer / 0.5f);
         timer += Time.deltaTime;
         yield return null;
      }
   }

   private IEnumerator Drop()
   {
      // Choose side to swipe from
      Transform handToMove = m_RightHand;
      Vector2 newPos = cr_ChosenPlayer.position;
      newPos.y = handToMove.position.y;
      RaycastHit2D hit = Physics2D.Raycast(newPos, Vector2.down, 100, 1 << LayerMask.NameToLayer("Floor"));
      float finalYPos = hit.point.y;
      if (Random.Range(0, 2) == 0)
         handToMove = m_LeftHand;
      Vector2 origPos = handToMove.position;

      // Move hand to same height as player
      float timer = 0;
      while (timer < 1)
      {
         handToMove.position = Vector2.Lerp(origPos, newPos, timer);
         timer += Time.deltaTime;
         yield return null;
      }

      // Swipe hand across screen
      timer = 0;
      while (timer < 0.35f)
      {
         Vector2 p = new Vector2(handToMove.position.x, Mathf.Lerp(newPos.y, finalYPos, timer / 0.35f));
         handToMove.position = p;
         timer += Time.deltaTime;
         yield return null;
      }

      // Return hand back
      newPos.y = finalYPos;
      timer = 0;
      while (timer < 0.5f)
      {
         handToMove.position = Vector2.Lerp(newPos, origPos, timer / 0.5f);
         timer += Time.deltaTime;
         yield return null;
      }
   }
   #endregion

   #region Collision Methods
   private void OnTriggerEnter2D(Collider2D other)
   {
      // If hit a tick, deal damage and destroy tick
      if (other.CompareTag("SetPin"))
      {
         Destroy(other.gameObject);
         ItemSpawner.curNumOfItems--;
         p_CurHealth--;
         if (p_CurHealth == 0)
         {
            BossKilledEvent?.Invoke();
            Destroy(gameObject);
         }
      }
   }
   #endregion
}
