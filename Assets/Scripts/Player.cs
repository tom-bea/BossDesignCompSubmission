using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
   #region Delegates and Events
   public delegate void IntDelegate(int id);
   public static event IntDelegate PlayerDiedEvent;
   #endregion

   #region Editor Variables
   [SerializeField]
   [Tooltip("How fast this character can run around.")]
   private float m_MoveSpeed = 10;

   [SerializeField]
   [Tooltip("How high this character can jump.")]
   private float m_JumpStrength = 5;

   [SerializeField]
   [Tooltip("How long the player can hold jump button before it doesn't do anything (in seconds).")]
   private float m_JumpTime = 0.5f;

   [SerializeField]
   [Tooltip("The gravity strength when this character is jumping. A smaller number makes it jump higher.")]
   private float m_JumpingGravity = 15;

   [SerializeField]
   [Tooltip("The gravity strength when this character is falling. A larger number makes it fall faster.")]
   private float m_FallingGravity = 50;

   [SerializeField]
   [Tooltip("How long to wait before the special can be used again.")]
   private float m_SpecialCooldownTime = 2;
   #endregion

   #region Private Variables
   private int p_PlayerID;

   // Button Variables
   private string p_HorizAxis;
   private string p_JumpButton;
   private string p_DownButton;
   private string p_InteractButton;
   private string p_SpecialButton;

   // Movement Variables
   private float p_MoveDir;
   private bool p_JumpNow;
   private bool p_IsJumping;
   private float p_CurJumpStrength;

   // Health Variables
   private int p_MaxHealth = 3;
   private int p_CurHealth;
   private float p_InvincibilityTimeLeft = 0;

   // Interact Variables
   private bool p_IsInteracting = false;
   private Transform p_Pin;

   // Special Variables
   private float p_CurSpecialCooldownTime = 0;
   #endregion

   #region Cached Components
   private Rigidbody2D cc_Rb;
   #endregion

   #region Initialization
   private void Awake()
   {
      p_CurHealth = p_MaxHealth;
      p_JumpNow = false;
      p_IsJumping = false;

      cc_Rb = GetComponent<Rigidbody2D>();
   }
   #endregion

   #region Main Updates
   private void Update()
   {
      if (!GameManager.GameRunning)
         return;

      // Movement
      p_MoveDir = Input.GetAxisRaw(p_HorizAxis);

      // Jumping
      if (Input.GetButtonDown(p_JumpButton) && !p_IsJumping)
         StartJumping();
      else if (Input.GetButtonUp(p_JumpButton))
         StopJumping();
      else if (p_CurJumpStrength > 0)
         p_CurJumpStrength -= (1 / m_JumpTime) * m_JumpStrength * Time.deltaTime;
      else
         StopJumping();

      // Falling
      if (Input.GetButtonDown(p_DownButton))
      {
         gameObject.layer = LayerMask.NameToLayer("PlayerThroughPlatform");
      }
      else if (Input.GetButtonUp(p_DownButton))
      {
         gameObject.layer = LayerMask.NameToLayer("Player");
      }

      // Interact
      if (Input.GetButtonDown(p_InteractButton))
      {
         if (p_IsInteracting)
            InteractInteracting();
         else
            InteractNotInteracting();
      }

      // Use special
      if (Input.GetButtonDown(p_SpecialButton) && p_CurSpecialCooldownTime == 0)
      {
         p_CurSpecialCooldownTime = m_SpecialCooldownTime;
         UseSpecial();
      }
      else if (p_CurSpecialCooldownTime > 0)
         p_CurSpecialCooldownTime -= Time.deltaTime;
      else
         p_CurSpecialCooldownTime = 0;

      // Health
      if (p_InvincibilityTimeLeft > 0)
         p_InvincibilityTimeLeft -= Time.deltaTime;
      else
         p_InvincibilityTimeLeft = 0;
   }

   private void FixedUpdate()
   {
      cc_Rb.velocity = Vector2.right * p_MoveDir * m_MoveSpeed;
      if (p_JumpNow)
      {
         p_IsJumping = true;
         cc_Rb.AddForce(Vector2.up * m_JumpStrength * m_JumpStrength);
      }
   }
   #endregion

   #region Accessors and Mutators
   public int PlayerID
   {
      get { return p_PlayerID; }
      set { p_PlayerID = value; SetButtons(); }
   }

   private void SetButtons()
   {
      p_HorizAxis = "Horizontal" + p_PlayerID;
      p_JumpButton = "Jump" + p_PlayerID;
      p_DownButton = "Down" + p_PlayerID;
      p_InteractButton = "Interact" + p_PlayerID;
      p_SpecialButton = "Special" + p_PlayerID;
   }
   #endregion

   #region Collision Methods
   private void OnCollisionEnter2D(Collision2D collision)
   {
      GameObject other = collision.collider.gameObject;
      if (other.CompareTag("Platform") &&
         collision.GetContact(0).point.y < transform.position.y)
      {
         p_IsJumping = false;
      }

      if (other.CompareTag("Boss"))
      {
         DecreaseHealth();
      }
   }
   #endregion

   #region Jump Methods
   private void StartJumping()
   {
      p_JumpNow = true;
      p_CurJumpStrength = m_JumpStrength;
      cc_Rb.gravityScale = m_JumpingGravity;
   }

   private void StopJumping()
   {
      p_JumpNow = false;
      cc_Rb.gravityScale = m_FallingGravity;
   }
   #endregion

   #region Interact Methods
   private void InteractNotInteracting()
   {
      p_IsInteracting = true;
      Collider2D hit = Physics2D.OverlapCircle(transform.position, 1, 1 << LayerMask.NameToLayer("Item"));
      if (hit != null)
      {
         hit.gameObject.transform.parent = transform;
         p_Pin = hit.gameObject.transform;
      }
   }

   private void InteractInteracting()
   {
      p_IsInteracting = false;
      p_Pin.parent = null;
      p_Pin.tag = "SetPin";
   }
   #endregion

   #region Special Methods
   private void UseSpecial()
   {

   }
   #endregion

   #region Health Methods
   private void IncreaseHealth()
   {

   }

   private void DecreaseHealth()
   {
      if (p_InvincibilityTimeLeft > 0)
         return;
      p_InvincibilityTimeLeft = 0.1f;
      p_CurHealth--;
      if (p_CurHealth == 0)
         Die();
   }

   private void Die()
   {
      PlayerDiedEvent?.Invoke(p_PlayerID);
      Destroy(gameObject);
   }
   #endregion
}
