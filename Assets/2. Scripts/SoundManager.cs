using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    // 어떠한 스크립트에서도 사운드 매니저로 접근할 수 있는 스태틱 필드
    public static SoundManager _snd;

    private AudioSource     musSource;
    private AudioSource     sfxSource;
    private AudioSource     ambSource;

    private AudioListener   listener;

    private float _masVol = 1.0f;
    private float _musVol = 1.0f;
    private float _sfxVol = 1.0f;
    private float _ambVol = 1.0f;

    // 다른 스크립트에서 효과음을 재생할 때 거쳐가는 필드
    private AudioSource     receivedSource;

    public enum SoundType : int { MUSIC, SFX, AMBIENT, ERROR };

    //음악, 효과음, 환경음 클립이 저장되는 필드
    [SerializeField]
    private AudioClip[] musClips;

    [SerializeField]
    private AudioClip[] sfxClips;

    [SerializeField]
    private AudioClip[] ambClips;

    // 볼륨 값은 0.0f ~ 1.0f로 적용
    public float MasterVol
    {
        get { return _masVol; }
        set
        {
            _masVol = Mathf.Clamp(value, 0.0f, 1.0f);
        }
    }

    public float MusVol
    {
        get { return _musVol; }
        set
        {
            _musVol = Mathf.Clamp(value, 0.0f, 1.0f);
        }
    }

    public float SfxVol
    {
        get { return _sfxVol; }
        set
        {
            _sfxVol = Mathf.Clamp(value, 0.0f, 1.0f);
        }
    }

    public float AmbVol
    {
        get { return _ambVol; }
        set
        {
            _ambVol = Mathf.Clamp(value, 0.0f, 1.0f);
        }
    }

    // 옵션에서 음량을 바꿀 때마다 실행되는 볼륨 갱신 메서드
    private void VolumeAdjustment()
    {
        musSource.volume = MusVol * MasterVol;
        sfxSource.volume = SfxVol * MasterVol;
        ambSource.volume = AmbVol * MasterVol;
    }

    // 다른 스크립트에서 효과음을 불러올 때 거쳐가는 메서드 -> 검수 필요, 다른 오디오 소스에서 효과음을 재생 할 때 SoundManager의 sfxVol을 참고할 수 있도록 만드려고 함.
    public void SfxCall(AudioSource caller, int number)
    {
        caller.volume = caller.volume * SfxVol * MasterVol;
        caller.clip = sfxClips[number];
        caller.Play();
    }


    // 메인 카메라에 있는 AudioSource를 통해 사운드를 재생하는 메서드, loop 여부는 Mus,Amb = true, Sfx = false;
    public void SndPlay(SoundType sndType, int number)
    {
        switch (sndType)
        {
            case SoundType.MUSIC:
                MusPlay(number);
                break;
            case SoundType.SFX:
                SfxPlay(number);
                break;
            case SoundType.AMBIENT:
                AmbPlay(number);
                break;
            case SoundType.ERROR:
            default:
                Debug.Log("SndPlay : 잘못된 오디오 타입입니다.");
                break;
        }
    }

    // 메인 카메라에 있는 AudioSource를 통해 사운드를 재생하는 메서드, loop 여부 선택 가능
    public void SndPlay(SoundType sndType, int number, bool loop)
    {
        switch (sndType)
        {
            case SoundType.MUSIC:
                MusPlay(number, loop);
                break;
            case SoundType.SFX:
                SfxPlay(number, loop);
                break;
            case SoundType.AMBIENT:
                AmbPlay(number, loop);
                break;
            case SoundType.ERROR:
            default:
                Debug.Log("SndPlay : 잘못된 사운드 타입입니다.");
                break;
        }    
    }

    // 배경음악을 재생하는 메서드, loop = true;
    public void MusPlay(int number)
    {
        if (number < musClips.Length && number >= 0)
        {
            musSource.clip = musClips[number];
            musSource.loop = true;
            musSource.Play();
        }
        else
        {
            Debug.Log("잘못된 음악 번호입니다.");
        }
    }

    // 배경음악을 재생하는 메서드, loop 여부 설정 가능
    public void MusPlay(int number, bool loop)
    {
        if(number < musClips.Length && number >= 0)
        {
            musSource.clip = musClips[number];
            musSource.loop = loop;
            musSource.Play();
        }
        else
        {
            Debug.Log("잘못된 음악 번호입니다.");
        }
    }

    // 효과음을 메인 카메라에 있는 AudioSource로 재생하는 메서드, loop = false;
    public void SfxPlay(int number)
    {
        if (number < sfxClips.Length && number >= 0)
        {
            sfxSource.clip = sfxClips[number];
            sfxSource.loop = false;
            sfxSource.Play();
        }
        else
        {
            Debug.Log("잘못된 효과음 번호입니다.");
        }
    }

    // 효과음을 메인 카메라에 있는 AudioSource로 재생하는 메서드, loop 여부 설정 가능
    public void SfxPlay(int number, bool loop)
    {
        if (number < sfxClips.Length && number >= 0)
        {
            sfxSource.clip = sfxClips[number];
            sfxSource.loop = loop;
            sfxSource.Play();
        }
        else
        {
            Debug.Log("잘못된 효과음 번호입니다.");
        }
    }

    // 환경음을 재생하는 메서드, loop = true;
    public void AmbPlay(int number)
    {
        if (number < ambClips.Length && number >= 0)
        {
            ambSource.clip = ambClips[number];
            ambSource.loop = true;
            ambSource.Play();
        }
        else
        {
            Debug.Log("잘못된 환경음 번호입니다.");
        }
    }

    // 환경음을 재생하는 메서드, loop 여부 설정 가능
    public void AmbPlay(int number, bool loop)
    {
        if (number < ambClips.Length && number >= 0)
        {
            ambSource.clip = ambClips[number];
            ambSource.loop = loop;
            ambSource.Play();
        }
        else
        {
            Debug.Log("잘못된 환경음 번호입니다.");
        }
    }

    // 씬 시작 직전에 스태틱 필드에 _snd를 넣고, AudioSource, AudioListener를 찾고 할당함
    private void Awake()
    {
        if(_snd == null)
        {
            Debug.Log("SoundManager : _snd 스태틱 필드 할당됨");
            _snd = this;
            DontDestroyOnLoad(_snd);
        }
        
        AudioSource[] audSources = GetComponents<AudioSource>();

        //Debug.Log(audSources.Length);

        musSource = audSources[0];
        sfxSource = audSources[1];
        ambSource = audSources[2];

        listener = GetComponent<AudioListener>();

        MasterVol = 2.0f;
        MusVol = 1.0f;
        SfxVol = 1.0f;
        AmbVol = 1.0f;

    }


    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            MusPlay(1, true);
        }
        else
        {
            MusPlay(0, true);
        }
    }
}
