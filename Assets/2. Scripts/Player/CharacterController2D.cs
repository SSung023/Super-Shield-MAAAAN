using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f;                          // Amount of force added when the player jumps.
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
    [SerializeField] private LayerMask m_WhatIsDownGround;
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;
    [SerializeField] private Transform m_PlayerCenter;         
    [SerializeField] private float m_dashDistance = 5f;
    [SerializeField] public int maxJumpCount = 2;

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    const float k_CeilingRadius = 0.2f; // Radius of the overlap circle to determine if the player can stand up
    private Rigidbody2D m_Rigidbody2D;
    private CircleCollider2D m_CircleCollider2D;
    private BoxCollider2D m_BoxCollider2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;
    private int jumpCount = 0;
    private bool jumpOverlapping = false;


    [Header("Test")]
    public Transform shieldT;
    private Collider2D[] groundColliders;
    private Collider2D[] ceilingColliders;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public int shield_passive_jumpbonus = 0;
    

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_CircleCollider2D = GetComponent<CircleCollider2D>();
        m_BoxCollider2D = GetComponent<BoxCollider2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        groundColliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        ceilingColliders = Physics2D.OverlapCircleAll(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround);

        //Debug.Log(ceilingColliders[0]);
        for (int i = 0; i < groundColliders.Length; i++)
        {
            if (groundColliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (jumpCount > 0)
                {
                    jumpCount = 0;
                    OnLandEvent.Invoke();
                }
            }
        }

        for (int i = 0; i < ceilingColliders.Length; i++)
        {
            if (ceilingColliders[i].gameObject != gameObject)
            {
                StartCoroutine(Ceiling());
            }
        }
    }

    public void Dash()
    {
        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2((m_FacingRight ? 1 : -1) * m_dashDistance * 10f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
        }
    }
    public void Move(float move, bool jump, bool downJump, bool isShieldOn)
    {

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
            if (move > 0 && !m_FacingRight)
            {
                Flip();
            }
            else if (move < 0 && m_FacingRight)
            {
                Flip();
            }
        }
        if (m_Grounded && jump && downJump)
        {
            StartCoroutine(controlDownGround());
        }
        // If the player should jump...
        else if (jump && jumpCount < (maxJumpCount + shield_passive_jumpbonus))
        {
            // Add a vertical force to the player.
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
            StartCoroutine(jumpCountUp());
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
    }
    IEnumerator jumpCountUp()
    {
        yield return new WaitForSeconds(0.05f);
        jumpCount++;
    }
    IEnumerator controlDownGround()
    {
        Collider2D[] cols = groundColliders;
        foreach (Collider2D col in cols)
        {
            var a = col.GetComponent<Ground>();
            if (a != null && a.canDown)
            {
                col.gameObject.layer = LayerMask.NameToLayer("DownGround");
            }
        }
        yield return new WaitForSeconds(0.5f);
        foreach (Collider2D col in cols)
        {
            var a = col.GetComponent<Ground>();
            if (a != null && a.canDown)
            {
                col.gameObject.layer = LayerMask.NameToLayer("Ground");
            }
        }
    }

    IEnumerator Ceiling()
    {
        bool grounded = false; 

        Collider2D[] cols = ceilingColliders;
        foreach (Collider2D col in cols)
        {
            var a = col.GetComponent<Ground>();
            if (a != null)
            {
                col.gameObject.layer = LayerMask.NameToLayer("DownGround");
            }
        }

        yield return new WaitForSeconds(0.2f);

        Collider2D[] overlap_coliders = Physics2D.OverlapCircleAll(m_PlayerCenter.position, 1f, m_WhatIsDownGround);
        int len_coliders = overlap_coliders.Length;
        Debug.Log(len_coliders);

        for (int i = 0; i < 10; i++)
        {

            foreach (Collider2D col in cols)
            {
                var a = col.GetComponent<Ground>();
                if (a != null && len_coliders == 0)
                {
                    col.gameObject.layer = LayerMask.NameToLayer("Ground");
                    grounded = true;
                }
            }

            if (grounded) break;
            else
            {
                yield return new WaitForSeconds(0.1f);
                overlap_coliders = Physics2D.OverlapCircleAll(m_PlayerCenter.position, 1f, m_WhatIsDownGround);

                len_coliders = overlap_coliders.Length;
                Debug.Log(len_coliders);
            }


        }

        /*
        foreach (Collider2D col in cols)
        {
            var a = col.GetComponent<Ground>();
            if (a != null)
            {
                col.gameObject.layer = LayerMask.NameToLayer("Ground");
            }
        }
        */




    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        Vector3 shieldScale = shieldT.localScale;
        shieldScale.x *= -1;
        shieldT.localScale = shieldScale;
    }

    private void Wall_inviciblity(bool val)
    {
        m_CircleCollider2D.isTrigger = val;
        m_BoxCollider2D.isTrigger = val;
    }
}