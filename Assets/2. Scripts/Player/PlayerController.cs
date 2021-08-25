using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public CharacterController2D controller;
    public Player player;
    public Animator myAnimator;

    public float dashInvincibleTime;
    public float runSpeed = 40f;
    public float facingRight = 0f;
    [SerializeField] private Transform itemCheck;
    private float _horizontalMove = 0f;
    private bool _jump = false;
    private bool _downJump = false;
    private bool _isMaking = false;
    private bool _isTrigger = false; // jump, dash할 때 true
    private bool _haveShield = true; // 방패 소유의 여부
    private bool _isShieldOn = true; // 방패가 켜져있나의 여부
        
    private float _curDashCool = 0;
    public float dashCool = 5f;
    private float shieldResetTime = 0.3f; // 해당 시간이 지난 이후 shield가 -1(활성화가 안된 상태)로 변환
    private float shieldStandbyTime;
    
    public Transform[] shieldT = new Transform[4];
    public Shield curShield;
    private int _curShieldT = -1;
    [SerializeField] private AudioSource _playerAudioSource;

    public float shield_passive_speed = 1f;
    public float shield_passive_dashcooldown = 0f;

    private GameObject overlapObject;

    private CapsuleCollider2D m_CapsuleCollider2D;
    private BoxCollider2D m_BoxCollider2D;

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        m_CapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        m_BoxCollider2D = GetComponent<BoxCollider2D>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            SoundManager._snd.SndPlay(SoundManager.SoundType.SFX, 0); // 디버그 용 재시작 할 때 소리 재생하기 (임시)
            SceneManager.LoadScene(0);
        }
        
        if (_isMaking) return;
        _horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed * shield_passive_speed;
        
        UpdateMoveType();
        
        resetShieldNum();
        
        myAnimator.SetFloat("Direction", _horizontalMove);
        facingRight = Mathf.Sign(player.transform.localScale.x);

        if (Input.GetButtonDown("Jump"))
        {
            SoundManager._snd.SfxCall(_playerAudioSource, 3); // 점프할 때 소리 내기 (임시)
            _jump = true;
            
            myAnimator.SetTrigger("MoveTrigger");
            _isTrigger = true;
            myAnimator.SetFloat("MoveType", (float)CommonVariable.MoveType.JUMP);
        }

        _downJump = Input.GetKey(KeyCode.DownArrow);
        if (Input.GetButtonDown("Dash") && _curDashCool == 0)
        {
            player.GetInvincibility(dashInvincibleTime);
            controller.Dash();
            _curDashCool = dashCool - shield_passive_dashcooldown;
            
            myAnimator.SetTrigger("MoveTrigger");
            _isTrigger = true;
            myAnimator.SetFloat("MoveType", (float)CommonVariable.MoveType.DASH);
        }

        if (Input.GetButtonDown("Make") && curShield == null) //방패를 만드는 입력
        {
            if(overlapObject != null && overlapObject.gameObject.layer == 12)
            {
                myAnimator.SetTrigger("MakeShield");
                _haveShield = true;
                StartCoroutine(ProcessMakingShield(overlapObject.GetComponent<ShieldMaterial>()));
            }
            /*(var mats = (from col in Physics2D.OverlapCircleAll(itemCheck.position, 2f)
                where col.gameObject.layer == 12
                select col.GetComponent<ShieldMaterial>()).ToArray();
            if (mats.Length != 0)
            {
                myAnimator.SetTrigger("MakeShield");
                _haveShield = true;
                StartCoroutine(ProcessMakingShield(mats.First()));
            }
            else print("실패!");
            */
            
        }


        if (Input.GetButtonDown("Throw") && curShield != null)
        {
            // 플레이어 방향 확인 후 던져지는 방패 생성
            if (facingRight != -1) curShield.transform.SetPositionAndRotation(shieldT[0].position, shieldT[0].rotation);
            else curShield.transform.SetPositionAndRotation(shieldT[3].position, shieldT[3].rotation);

            // 던짐과 동시에 방패를 shield layer로 보냄
            // instantiate 직후에 보내는 것도 고려해 볼 만함
            curShield.gameObject.layer = 13;

            curShield.Use(true);
            curShield.Throw();
            //SoundManager._snd.RandomSfxCall(_playerAudioSource, 12, 18);
            GameManager.instance.shieldManager.throwShield(this, player, curShield);
            curShield = null;

            _haveShield = false;
        }

        if (Input.GetButtonDown("Root") && curShield == null)
        {
            if (overlapObject != null && overlapObject.gameObject.layer == 11)
            {
                ShieldItem itemScript = overlapObject.GetComponent<ShieldItem>();
                curShield = Instantiate(itemScript.GetItem(), shieldT[0].parent);
                GameManager.instance.shieldManager.makeShield(this, player, curShield, itemScript.shield_debuff_num, itemScript.shield_passive_num, itemScript.shield_level_num);

            }
            /*var items = (from col in Physics2D.OverlapCircleAll(itemCheck.position, 2f)
                where col.gameObject.layer == 11
                select col.GetComponent<ShieldItem>()).ToArray();
            if (items.Length != 0)
            {
                curShield = Instantiate(items.First().GetItem(), shieldT[0].parent);
                ShieldItem itemScript = items.First();
                GameManager.instance.shieldManager.makeShield(this, player, curShield, itemScript.shield_debuff_num, itemScript.shield_passive_num, itemScript.shield_level_num);
            }
            */
        }

        
        _isShieldOn = true;
        
        myAnimator.SetBool("haveShield", _haveShield);
        myAnimator.SetBool("isShieldOn", !_isShieldOn);

        for (var i = 0; i < 4; i++)
        {
            if (Input.GetButton("Shield_" + i.ToString()) && curShield != null)
            {
                _curShieldT = i;

                if (facingRight == 1) // 오른쪽
                {
                    myAnimator.SetFloat("ShieldNum", _curShieldT); // 0~3
                }
                else if (facingRight == -1) // 왼쪽
                {
                    float tmp = Math.Abs(3 - _curShieldT);
                    myAnimator.SetFloat("ShieldNum", tmp); // 0~3
                }
                
                curShield.Use(true);
                _isShieldOn = false;
                myAnimator.SetBool("isShieldOn", !_isShieldOn);
                
            }

            if (Input.GetButtonUp("Shield_" + i.ToString()))
            {
                myAnimator.SetBool("isShieldOn", !_isShieldOn);
                _isShieldOn = true;
            }
        }
        if (_isShieldOn) _curShieldT = -1;

        if(Input.GetKeyDown(KeyCode.F1))
        {
            GameManager.instance.gameExit();
        }
        if(Input.GetKeyDown(KeyCode.F2))
        {
            if(GameManager.instance.getGameState() == GameManager.State.play)
            {
                GameManager.instance.setGameState(GameManager.State.stop);
            }
            else if(GameManager.instance.getGameState() == GameManager.State.stop)
            {
                GameManager.instance.setGameState(GameManager.State.play);
            }
        }
    }
    private void resetShieldNum()
    {
        if (shieldStandbyTime <= 0)
        {
            _curShieldT = -1;
            myAnimator.SetFloat("ShieldNum", _curShieldT); // -1 : shield 활성화 x
            shieldStandbyTime = shieldResetTime;
        }
        else
        {
            shieldStandbyTime -= Time.deltaTime;
        }
    }

    private void UpdateMoveType()
    {
        if (_isTrigger)
        {
            StartCoroutine("SetMoveType");
            return;
        }
        if (_haveShield)
        {
            if (_horizontalMove != 0)
            {
                myAnimator.SetFloat("MoveType", (float)CommonVariable.MoveType.RUN);
                myAnimator.SetBool("isWalking", true);
            }
            else // 움직이지 않는 경우
            {
                myAnimator.SetFloat("MoveType", (float)CommonVariable.MoveType.IDLE);
                myAnimator.SetBool("isWalking", false);
            }
        }
        else
        {
            if (_horizontalMove != 0)
            {
                myAnimator.SetFloat("MoveType", (float)CommonVariable.MoveType.RUN);
                myAnimator.SetBool("isWalking", true);
            }
            else // 움직이지 않는 경우
            {
                myAnimator.SetFloat("MoveType", (float)CommonVariable.MoveType.EMPTY);
                myAnimator.SetBool("isWalking", false);
            }
        }
    }

    private IEnumerator SetMoveType()
    {
        
        yield return new WaitForSeconds(0.5f);
        _isTrigger = false;
    }

    private IEnumerator ProcessMakingShield(ShieldMaterial mat)
    {
        _isMaking = true;
        mat.MakeShield();
        player.GetInvincibility(3.5f);
        _horizontalMove = 0;
        myAnimator.SetBool("isWalking", false);
        // 애니메이션 변수
        yield return new WaitForSeconds(3.5f);
        // 애니메이션 변수
        _isMaking = false;
    }

    private void FixedUpdate()
    {
        controller.Move(_horizontalMove * Time.fixedDeltaTime, _jump, _downJump, _isShieldOn);
        _jump = false;

        if(_curDashCool != 0) 
        {
            _curDashCool -= Time.fixedDeltaTime;
            if(_curDashCool < 0) _curDashCool = 0;
        }
        if(_curShieldT != -1)
        {
            print(_curShieldT.ToString());
            curShield.transform.SetPositionAndRotation(shieldT[_curShieldT].position, shieldT[_curShieldT].rotation);
        }
        else if(curShield != null)
        {
            curShield.Use(false);
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Ground") && !controller.m_Grounded)
        {
            m_BoxCollider2D.isTrigger = true;
            StopCoroutine("triggerFalse");
        }
    }
    public void OnTriggerStay2D(Collider2D other)
    {

        SpriteRenderer spr;
        Color color;

        if (overlapObject != null)
        {
            spr = overlapObject.GetComponent<SpriteRenderer>();

            color = spr.material.color;
            color.a = 0.6f;
            spr.material.color = color;
            overlapObject = null;
        }

        GameObject[] items = (from col in Physics2D.OverlapCircleAll(itemCheck.position, 2f)
                              where col.gameObject.CompareTag("Interactable")
                              select col.gameObject).ToArray();

        if (items.Length != 0)
        {
            GameObject shortObject = items.First();
            float shortDistance = 100;

            for (int i = 0; i < items.Length; i++)
            {
                float dis = Vector2.Distance(itemCheck.transform.position, items[i].transform.position);
                if (shortDistance > dis)
                {
                    shortObject = items[i];
                    shortDistance = dis;
                }
            }
            overlapObject = shortObject;

            spr = overlapObject.GetComponent<SpriteRenderer>();

            color = spr.material.color;
            color.a = 1f;
            spr.material.color = color;

            if(overlapObject.gameObject.layer == 11)
            {
                ShieldItem tempItem =  overlapObject.GetComponent<ShieldItem>();
                Debug.Log("Hold debuff: " + tempItem.shield_debuff_num);
                Debug.Log("Hold passive: " + tempItem.shield_passive_num);
                Debug.Log("Hold level: " + tempItem.shield_level_num);
            }
        }
        
    }
    public void OnTriggerExit2D(Collider2D other)
    {
        SpriteRenderer spr;
        Color color;
        if (overlapObject != null)
        {
            spr = overlapObject.GetComponent<SpriteRenderer>();

            color = spr.material.color;
            color.a = 0.6f;
            spr.material.color = color;
        }
        overlapObject = null;

        if (other.gameObject.tag.Equals("Ground"))
        {
            m_BoxCollider2D.isTrigger = false;
        }
    }
    IEnumerator triggerFalse()
    {
        yield return new WaitForSeconds(0f);
        m_BoxCollider2D.isTrigger = false;

    }
}
