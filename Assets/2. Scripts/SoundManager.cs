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
    private AudioSource[]   audSources;

    // sfxCall로 효과음을 사용할 때, sfxMuted가 true면 sfxCall로 효과음을 재생하지 않는다.
    private bool sfxMuted = false;

    private int currentTrackNo = 0;

    private AudioListener   listener;

    private float _masVol = 1.0f;
    private float _musVol = 1.0f;
    private float _sfxVol = 1.0f;
    private float _ambVol = 1.0f;

    // 다른 스크립트에서 효과음을 재생할 때 거쳐가는 필드
    //private AudioSource     receivedSource;

    public enum SoundType : int { MUSIC, SFX, AMBIENT, INSTANTSFX, ERROR };

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

    /// <summary>
    /// 옵션에서 음량을 바꿀 때마다 실행되는 볼륨 갱신 메서드 
    /// </summary>
    private void VolumeAdjustment()
    {
        AudioListener.volume = MasterVol;
        musSource.volume = MusVol;
        sfxSource.volume = SfxVol;
        ambSource.volume = AmbVol;
    }

    // 다른 스크립트에서 효과음을 불러올 때 거쳐가는 메서드 -> 검수 필요, 다른 오디오 소스에서 효과음을 재생 할 때 SoundManager의 sfxVol을 참고할 수 있도록 만드려고 함.
    /// <summary>
    /// 지정한 AudioSource에서 로컬 효과음이 나도록 합니다.
    /// <para> SoundManager에 있는 오디오 클립을 호출해서 사용합니다. </para>
    /// </summary>
    /// <param name="caller">효과음을 출력할 AudioSource</param>
    /// <param name="number">효과음 번호</param>
    public void SfxCall(AudioSource caller, int number)
    {
        if (!sfxMuted)
        {
            if (number < sfxClips.Length && number >= 0)
            {
                caller.volume = caller.volume * SfxVol;
                caller.clip = sfxClips[number];
                caller.Play();
            }
            else
            {
                Debug.Log("InstantSfxCall : 잘못된 효과음 번호입니다.");
            }
        }
    }

    /// <summary>
    /// 지정한 AudioSource에서 인스턴트 효과음이 나도록 합니다.
    /// <para> SoundManager에 있는 오디오 클립을 호출해서 사용합니다. </para>
    /// </summary>
    /// <param name="caller">효과음을 출력할 AudioSource</param>
    /// <param name="number">효과음 번호</param>
    public void InstantSfxCall(AudioSource caller, int number)
    {
        if (!sfxMuted)
        {
            if (number < sfxClips.Length && number >= 0)
            {
                caller.PlayOneShot(sfxClips[number], caller.volume * SfxVol);
            }
            else
            {
                Debug.Log("InstantSfxCall : 잘못된 효과음 번호입니다.");
            }
        }
    }

    /// <summary>
    /// 지정한 AudioSource에서 인스턴트 효과음이 나도록 합니다.
    /// <para> SoundManager에 있는 오디오 클립을 호출해서 사용합니다. </para>
    /// </summary>
    /// <param name="caller">효과음을 출력할 AudioSource</param>
    /// <param name="number">효과음 번호</param>
    /// <param name="volume">효과음 음량(0.0f ~ 1.0f)</param>
    public void InstantSfxCall(AudioSource caller, int number, float volume)
    {
        if (!sfxMuted)
        {
            if (number < sfxClips.Length && number >= 0)
            {
                caller.PlayOneShot(sfxClips[number], caller.volume * volume * SfxVol);
            }
            else
            {
                Debug.Log("InstantSfxCall : 잘못된 효과음 번호입니다.");
            }
        }
    }

    // 메인 카메라에 있는 AudioSource를 통해 사운드를 재생하는 메서드, loop 여부는 Mus,Amb = true, Sfx = false;
    /// <summary>
    /// SoundManager 게임오브젝트의 AudioSource를 통해 직접 사운드를 출력합니다. bool 값을 통해 루프 여부를 설정할 수 있습니다.
    /// <para> enum SoundType은 int값입니다. </para> 
    /// </summary>
    /// <param name="sndType">0 = 배경음 / 1 = 효과음 / 2 = 환경음 / 3 = 인스턴트 효과음</param>
    /// <param name="number">재생할 클립 번호</param>
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
            case SoundType.INSTANTSFX:
                InstantSfxPlay(number);
                break;
            case SoundType.ERROR:
            default:
                Debug.Log("SndPlay : 잘못된 오디오 타입입니다.");
                break;
        }
    }

    // 메인 카메라에 있는 AudioSource를 통해 사운드를 재생하는 메서드, loop 여부 선택 가능
    /// <summary>
    /// SoundManager 게임오브젝트의 AudioSource를 통해 직접 사운드를 출력합니다. bool 값을 통해 루프 여부를 설정할 수 있습니다.
    /// <para> enum SoundType은 int값입니다. </para> 
    /// </summary>
    /// <param name="sndType">0 = 배경음 / 1 = 효과음 / 2 = 환경음 / 3 = 인스턴트 효과음</param>
    /// <param name="number">재생할 클립 번호</param>
    /// <param name="loop">루프 여부</param>
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
            case SoundType.INSTANTSFX:
                if (loop)
                {
                    Debug.Log("SndPlay : INSTANTSFX는 Loop 할 수 없습니다.");
                }
                InstantSfxPlay(number);
                break;
            case SoundType.ERROR:
            default:
                Debug.Log("SndPlay : 잘못된 사운드 타입입니다.");
                break;
        }    
    }

    // 배경음악을 재생하는 메서드, loop = true;
    /// <summary>
    /// 배경음악을 재생합니다. 루프 설정은 ON 상태입니다.
    /// </summary>
    /// <param name="number"> 배경음악 번호 </param>
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
    /// <summary>
    /// 배경음악을 재생합니다. 루프 설정 여부를 선택 가능합니다.
    /// </summary>
    /// <param name="number"> 배경음악 번호 </param>
    /// <param name="loop"> 루프 여부 </param>
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
    /// <summary>
    /// 글로벌 효과음을 재생합니다. 루프 설정은 OFF 상태입니다.
    /// </summary>
    /// <param name="number"> 효과음 번호 </param>
    public void SfxPlay(int number)
    {
        if (!sfxMuted)
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
    }

    // 효과음을 메인 카메라에 있는 AudioSource로 재생하는 메서드, loop 여부 설정 가능
    /// <summary>
    /// 글로벌 효과음을 재생합니다. 루프 설정 여부를 선택 가능합니다. (주의해서 사용해 주세요!)
    /// </summary>
    /// <param name="number"> 효과음 번호 </param>
    /// <param name="loop"> 루프 여부 </param>
    public void SfxPlay(int number, bool loop)
    {
        if (!sfxMuted)
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
    }

    // 환경음을 재생하는 메서드, loop = true;
    /// <summary>
    /// 글로벌 환경음을 재생합니다. 루프 설정은 ON 상태입니다.
    /// </summary>
    /// <param name="number"> 환경음 번호 </param>
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
    /// <summary>
    /// 글로벌 환경음을 재생합니다. 루프 설정 여부를 선택 가능합니다.
    /// </summary>
    /// <param name="number"> 환경음 번호 </param>
    /// <param name="loop"> 루프 여부 </param>
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

    /// <summary>
    /// 글로벌 인스턴트 효과음을 재생합니다. 루프는 불가능합니다.
    /// </summary>
    /// <param name="number"> 효과음 번호 </param>
    public void InstantSfxPlay(int number)
    {
        if (!sfxMuted)
        {
            if (number < sfxClips.Length && number >= 0)
            {
                sfxSource.PlayOneShot(sfxClips[number]);
            }
            else
            {
                Debug.Log("잘못된 효과음 번호입니다.");
            }
        }
    }

    /// <summary>
    /// 글로벌 인스턴트 효과음을 재생합니다. 루프는 불가능합니다.
    /// </summary>
    /// <param name="number"> 효과음 번호 </param>
    /// <param name="volume"> 효과음 음량(0.0f ~ 1.0f) </param>
    public void InstantSfxPlay(int number, float volume)
    {
        if (!sfxMuted)
        {
            if (number < sfxClips.Length && number >= 0)
            {
                sfxSource.PlayOneShot(sfxClips[number], volume);
            }
            else
            {
                Debug.Log("잘못된 효과음 번호입니다.");
            }
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
        else if (_snd != this)
        {
            Destroy(this.gameObject);
        }

        audSources = GetComponents<AudioSource>();

        //Debug.Log(audSources.Length);

        musSource = audSources[0];
        sfxSource = audSources[1];
        ambSource = audSources[2];

        listener = GetComponent<AudioListener>();

        MasterVol = 1.0f;
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

    // 임시로 사운드 매니저 조작을 담당하는 메서드
    private void Update()
    {
        if (Input.GetKey(KeyCode.BackQuote))
        {
                 if (Input.GetKeyDown(KeyCode.Alpha1)) { ToggleMute(KeyCode.Alpha1); }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) { ToggleMute(KeyCode.Alpha2); }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) { ToggleMute(KeyCode.Alpha3); }
            else if (Input.GetKeyDown(KeyCode.Alpha4)) { ToggleMute(KeyCode.Alpha4); }
            else if (Input.GetKeyDown(KeyCode.Alpha5)) { ToggleMute(KeyCode.Alpha5); }
            else if (Input.GetKeyDown(KeyCode.Q))      { SongChange(-1); }
            else if (Input.GetKeyDown(KeyCode.E))      { SongChange(1); }
            else                                       { return; }
        }
    }

    // 임시로 트랙 변경을 담당하는 메서드
    private void SongChange(int trackTo)
    {
        currentTrackNo = currentTrackNo + trackTo;
        currentTrackNo = Mathf.Clamp(currentTrackNo, 0, musClips.Length - 1);
        string currentTrackNotice = "현재 곡 번호는 " + currentTrackNo.ToString() + " 번입니다.";
        Debug.Log(currentTrackNotice);

        musSource.Pause();
        MusPlay(currentTrackNo, true);
    }

    // 파트별 음소거를 담당하는 메서드
    private void ToggleMute(KeyCode inputKey)
    {
        switch (inputKey)
        {
            case KeyCode.Alpha1:
                AudioListener.pause = AudioListener.pause == true ? false : true;
                Debug.Log("AudioListener 음소거 상태 : " + AudioListener.pause.ToString());
                break;
            case KeyCode.Alpha2:
                musSource.mute = musSource.mute == true ? false : true;
                Debug.Log("배경음 음소거 상태 : " + musSource.mute.ToString());
                break;
            case KeyCode.Alpha3:
                sfxSource.mute = sfxSource.mute == true ? false : true;
                sfxMuted = sfxMuted == true ? false : true;
                Debug.Log("효과음 음소거 상태 : " + sfxSource.mute.ToString());
                if(sfxSource.mute != sfxMuted) { Debug.Log("ERROR : ToggleMute : sfxSource와 SfxMuted 가 일치하지 않습니다."); }
                break;
            case KeyCode.Alpha4:
                ambSource.mute = ambSource.mute == true ? false : true;
                Debug.Log("환경음 음소거 상태 : " + ambSource.mute.ToString());
                break;
            case KeyCode.Alpha5:
                string soundStatus =
                    "현재 음소거 상태 / AudioListener : " + AudioListener.pause.ToString() +
                    " / 배경음 : " + musSource.mute.ToString() +
                    " / 효과음 : " + sfxSource.mute.ToString() +
                    " / 환경음 : " + ambSource.mute.ToString();
                Debug.Log(soundStatus);
                break;
            default:
                Debug.Log("ToggleMute 메서드가 잘못 사용되었습니다.");
                break;
        }
    }
}
