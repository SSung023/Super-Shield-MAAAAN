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
    public float dashInvincibleTime;
    public float runSpeed = 40f;
    [SerializeField] private Transform itemCheck;
    private float _horizontalMove = 0f;
    private bool _jump = false;
    private bool _downJump = false;
    private bool _isMaking = false;
    private float _curDashCool = 0;
    public float dashCool = 5f;
    public Transform[] shieldT = new Transform[4];
    public Shield curShield;
    private int _curShieldT = -1;
    [SerializeField] private AudioSource _playerAudioSource;

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
        _horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            SoundManager._snd.SfxCall(_playerAudioSource, 3); // 점프할 때 소리 내기 (임시)
            _jump = true;
        }

        _downJump = Input.GetKey(KeyCode.DownArrow);
        if (Input.GetButtonDown("Dash") && _curDashCool == 0)
        {
            player.GetInvincibility(dashInvincibleTime);
            controller.Dash();
            _curDashCool = dashCool;
        }

        if (Input.GetButtonDown("Make") && curShield == null)
        {
            var mats = (from col in Physics2D.OverlapCircleAll(itemCheck.position, 2f)
                where col.gameObject.layer == 12
                select col.GetComponent<ShieldMaterial>()).ToArray();
            if(mats.Length != 0) StartCoroutine(ProcessMakingShield(mats.First()));
            else print("실패!");
        }

        if (Input.GetButtonDown("Throw") && curShield != null)
        {
            // 던지는 애니메이션
            curShield.transform.SetPositionAndRotation(shieldT[0].position, shieldT[0].rotation);
            curShield.Use(true);
            curShield.Throw();
            curShield = null;
        }

        if (Input.GetButtonDown("Root") && curShield == null)
        {
            var items = (from col in Physics2D.OverlapCircleAll(itemCheck.position, 2f)
                where col.gameObject.layer == 11
                select col.GetComponent<ShieldItem>()).ToArray();
            if(items.Length != 0) curShield = Instantiate(items.First().GetItem(), shieldT[0].parent);
        }

        var isShieldUp = true;
        for (var i = 0; i < 4; i++)
        {
            if (Input.GetButton("Shield_" + i.ToString()) && curShield != null)
            {
                _curShieldT = i;
                curShield.Use(true);
                isShieldUp = false;
            }

            if (Input.GetButtonUp("Shield_" + i.ToString()))
            {
                isShieldUp = true;
            }
        }
        if (isShieldUp) _curShieldT = -1;
    }

    private IEnumerator ProcessMakingShield(ShieldMaterial mat)
    {
        _isMaking = true;
        mat.MakeShield();
        player.GetInvincibility(3.5f);
        _horizontalMove = 0;
        // 애니메이션 변수
        yield return new WaitForSeconds(3.5f);
        // 애니메이션 변수
        _isMaking = false;
    }

    private void FixedUpdate()
    {
        controller.Move(_horizontalMove * Time.fixedDeltaTime, _jump, _downJump);
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
}
