using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    // 어떠한 스크립트에서도 사운드 매니저로 접근할 수 있는 스태틱 필드
    public static SoundManager _snd;

    private AudioSource     musSource;
    private AudioSource     sfxSource;
    private AudioSource     ambSource;
    private AudioSource     uixSource;
    private AudioSource[]   audSources;

    private AudioListener listener;

    // sfxCall로 효과음을 사용할 때, sfxMuted가 true면 sfxCall로 효과음을 재생하지 않는다.
    private bool sfxMuted = false;

    // 배경음악 재생 시 사용하는 현재 트랙넘버
    private int currentTrackNo = 0;

    // 볼륨 필드
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
        if (sfxMuted) return;
        if (number < sfxClips.Length && number >= 0)
        {
            caller.volume = caller.volume * SfxVol;
            caller.clip = sfxClips[number];
            caller.Play();
        }
        else
        {
            Debug.Log("SfxCall : 잘못된 효과음 번호입니다.");
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
        if (sfxMuted) return;
        if (number < sfxClips.Length && number >= 0)
        {
            caller.PlayOneShot(sfxClips[number], caller.volume * SfxVol);
        }
        else
        {
            Debug.Log("InstantSfxCall : 잘못된 효과음 번호입니다.");
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
        if (sfxMuted) return;
        if (number < sfxClips.Length && number >= 0)
        {
            caller.PlayOneShot(sfxClips[number], caller.volume * volume * SfxVol);
        }
        else
        {
            Debug.Log("InstantSfxCall : 잘못된 효과음 번호입니다.");
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
        if (sfxMuted) return;
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
    /// <summary>
    /// 글로벌 효과음을 재생합니다. 루프 설정 여부를 선택 가능합니다. (주의해서 사용해 주세요!)
    /// </summary>
    /// <param name="number"> 효과음 번호 </param>
    /// <param name="loop"> 루프 여부 </param>
    public void SfxPlay(int number, bool loop)
    {
        if (sfxMuted) return;
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
        if (sfxMuted) return;
        if (number < sfxClips.Length && number >= 0)
        {
            sfxSource.PlayOneShot(sfxClips[number]);
        }
        else
        {
            Debug.Log("잘못된 효과음 번호입니다.");
        }
    }

    /// <summary>
    /// 글로벌 인스턴트 효과음을 재생합니다. 루프는 불가능합니다.
    /// </summary>
    /// <param name="number"> 효과음 번호 </param>
    /// <param name="volume"> 효과음 음량(0.0f ~ 1.0f) </param>
    public void InstantSfxPlay(int number, float volume)
    {
        if (sfxMuted) return;
        if (number < sfxClips.Length && number >= 0)
        {
            sfxSource.PlayOneShot(sfxClips[number], volume);
        }
        else
        {
            Debug.Log("잘못된 효과음 번호입니다.");
        }
    }

    /// <summary>
    /// 지정한 범위 내에서 랜덤한 글로벌 효과음을 재생합니다. 인스턴트 여부를 선택 가능합니다.
    /// </summary>
    /// <param name="rangeStart"> 효과음 범위 시작 번호(포함) </param>
    /// <param name="rangeEnd"> 효과음 범위 종료 번호(포함) </param>
    /// <param name="instant"> True 일 경우 인스턴트로 재생 </param>
    public void RandomSfxPlay(int rangeStart, int rangeEnd, bool instant)
    {
        if (sfxMuted) return;

        var sfxOutput = Random.Range(rangeStart, rangeEnd + 1);

        if (instant)
        {
            InstantSfxPlay(sfxOutput);
        }
        else
        {
            SfxPlay(sfxOutput);
        }
    }

    /// <summary>
    /// 지정한 범위 내에서 랜덤한 글로벌 효과음을 재생합니다.
    /// </summary>
    /// <param name="rangeStart"> 효과음 범위 시작 번호(포함) </param>
    /// <param name="rangeEnd"> 효과음 범위 종료 번호(포함) </param>
    public void RandomSfxPlay(int rangeStart, int rangeEnd)
    {
        if (sfxMuted) return;
        var sfxOutput = Random.Range(rangeStart, rangeEnd + 1);
        SfxPlay(sfxOutput);
    }

    /// <summary>
    /// 지정한 범위 내에서 랜덤한 로컬 효과음을 지정한 AudioSource에서 재생합니다. 인스턴트 여부를 선택 가능합니다.
    /// </summary>
    /// <param name="rangeStart"> 효과음 범위 시작 번호(포함) </param>
    /// <param name="rangeEnd"> 효과음 범위 종료 번호(포함) </param>
    /// <param name="caller"> 효과음을 출력할 AudioSource </param>
    /// <param name="instant"> True 일 경우 인스턴트로 재생 </param>
    public void RandomSfxCall(AudioSource caller, int rangeStart, int rangeEnd, bool instant)
    {
        if (sfxMuted) return;

        var sfxOutput = Random.Range(rangeStart, rangeEnd + 1);

        if (instant)
        {
            InstantSfxCall(caller, sfxOutput);
        }
        else
        {
            SfxCall(caller, sfxOutput);
        }
    }

    /// <summary>
    /// 지정한 범위 내에서 랜덤한 로컬 효과음을 지정한 AudioSource에서 재생합니다.
    /// </summary>
    /// <param name="rangeStart"> 효과음 범위 시작 번호(포함) </param>
    /// <param name="rangeEnd"> 효과음 범위 종료 번호(포함) </param>
    /// <param name="caller"> 효과음을 출력할 AudioSource </param>
    public void RandomSfxCall(AudioSource caller, int rangeStart, int rangeEnd)
    {
        if (sfxMuted) return;
        var sfxOutput = Random.Range(rangeStart, rangeEnd + 1);
        SfxCall(caller, sfxOutput);
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

        var count = 0;

        while (count < audSources.Length)
        {
            switch (audSources[count].outputAudioMixerGroup.name)
            {
                case "Music":
                    musSource = audSources[count];
                    break;
                case "Direct":
                    sfxSource = audSources[count];
                    break;
                case "Ambient":
                    ambSource = audSources[count];
                    break;
                case "Interface":
                    uixSource = audSources[count];
                    break;
            }
            count++;
        }

        audSources = new AudioSource[] { musSource, sfxSource, ambSource, uixSource };

        count = 0;

        listener = GetComponent<AudioListener>();

        MusVol = 1.0f;
        SfxVol = 1.0f;
        AmbVol = 1.0f;

        //AdaptavieTrackScores();
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
            else if (Input.GetKeyDown(KeyCode.Alpha6)) { AdptMusPlay(0); } //임시 메서드, 적응형 사운드트랙용
            else if (Input.GetKeyDown(KeyCode.Alpha7)) { adptTrackTransitionIs = true; Debug.Log(adptTrackTransitionIs); }
            else                                       { return; }
        }
    }

    // 임시로 트랙 변경을 담당하는 메서드
    private void SongChange(int trackTo)
    {
        currentTrackNo = currentTrackNo + trackTo;
        currentTrackNo = Mathf.Clamp(currentTrackNo, 0, musClips.Length - 1);
        Debug.Log("현재 곡 번호는 " + currentTrackNo.ToString() + " 번입니다.");

        musSource.Pause(); // 이 메서드 잘 작동하는지 모르겠는데 나중에 검토해봐야 할 듯, 정확한 용도는 무엇인지?
        MusPlay(currentTrackNo, true);
    }

    // 파트별 음소거를 담당하는 메서드
    private void ToggleMute(KeyCode inputKey)
    {
        switch (inputKey)
        {
            case KeyCode.Alpha1:
                AudioListener.pause = AudioListener.pause == true ? false : true;
                sfxMuted = AudioListener.pause;
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

    // 적응형 사운드트랙 구현을 위한 스크립트 (더럽다, 보수 필요)

    //적응형 사운드트랙의 큰 번호(곡 별로 하나씩 할당)
    private bool adptTrackIsPlaying = false;
    private bool adptTrackTransitionIs = false;

    private int adptMeasureForceSet = -1; // -1일 경우 비활성화, 0부터는 마디 번호 지정

    private MeasureType[] currentAdptTracks; // 현재 적응형 트랙의 마디별 특성을 저장하는 배열

    private MeasureType[][] adptTracks; // 적응형 트랙별 마디 특성을 저장하는 배열?

    [SerializeField]
    private AudioClip[] adptTrackClips; // 현재 적응형 트랙의 마디 클립을 저장하는 배열

    // 마디 별 성질 
    // NEXT : 다음 마디로 이동한다.
    // LOOP : 스스로 루프한다.             TransitionOn일 경우 다음 마디로 이동한다.
    // MOVE : 특정 마디로 이동시킨다.      TransitionOn일 경우 다음 마디로 이동한다.
    // 이 모든 성질은 1순위 강제 뮤트 > 2순위 강제 다음 마디 설정 > 3순위로 적용된다.
    public enum MeasureType : int { NEXT, LOOP, MOVE, END };

    /*
    private void AdaptavieTrackScores()
    {
        adptTracks[0] = new MeasureType[10] { MeasureType.NEXT, MeasureType.LOOP, MeasureType.NEXT, MeasureType.NEXT, MeasureType.NEXT, MeasureType.NEXT, MeasureType.NEXT , MeasureType.NEXT , MeasureType.MOVE, MeasureType.END };

        //{ 0, (MeasureType)1, 0, 0, 0, 0, 0, 0, (MeasureType)2, (MeasureType)3 };
    }
    */

    public void AdptMusPlay(int trackNo)
    {
        currentAdptTracks = new MeasureType[10];
        //currentAdptTracks = adptTracks[trackNo];

        adptTrackIsPlaying = true;

        currentAdptTracks[0] = MeasureType.NEXT;
        currentAdptTracks[1] = MeasureType.LOOP;
        currentAdptTracks[2] = MeasureType.NEXT;
        currentAdptTracks[3] = MeasureType.NEXT;
        currentAdptTracks[4] = MeasureType.NEXT;
        currentAdptTracks[5] = MeasureType.NEXT;
        currentAdptTracks[6] = MeasureType.NEXT;
        currentAdptTracks[7] = MeasureType.NEXT;
        currentAdptTracks[8] = MeasureType.MOVE; // 3으로
        currentAdptTracks[9] = MeasureType.END;
            
        StartCoroutine(TMT(0, false));
    }

    IEnumerator TMT(int inputMeasure, bool scoreEnd)
    {
        if (scoreEnd == true)
        {
            adptTrackIsPlaying = false;
            yield return null;
        }

        musSource.loop = false;
        musSource.clip = adptTrackClips[inputMeasure];
        Debug.Log(musSource.clip);
        musSource.time = 0;
        musSource.Play();

        while (adptTrackIsPlaying)
        {
            yield return new WaitForSeconds(0.1f);

            if(Input.GetKey(KeyCode.BackQuote) && Input.GetKeyDown(KeyCode.Alpha8))
            {
                musSource.time = musSource.clip.length - 2.0f;
                Debug.Log("skipped!");
            }

            if(!musSource.isPlaying)
            {

                var addMeasure = 0;
                var nextMeasure = 0;

                switch (currentAdptTracks[inputMeasure])
                {
                    case MeasureType.NEXT:
                        addMeasure = addMeasure + 1;
                        break;
                    case MeasureType.LOOP:
                        addMeasure = adptTrackTransitionIs == true ? addMeasure + 1 : addMeasure;
                        adptTrackTransitionIs = false;
                        break;
                    case MeasureType.MOVE:
                        addMeasure = adptTrackTransitionIs == true ? addMeasure + 1 : addMeasure -5; // -5는 임시로 할당
                        adptTrackTransitionIs = false;
                        break;
                    case MeasureType.END:
                        scoreEnd = true;
                        Debug.Log("적응형 트랙 마지막 마디입니다.");
                        adptTrackTransitionIs = false;
                        break;
                    default:
                        Debug.Log("할당되지 않은 MeasureType입니다.");
                        break;
                }

                nextMeasure = inputMeasure + addMeasure;

                string trackStatus = "inputMeasure : " + inputMeasure.ToString() + " / nextMeasure : " + nextMeasure.ToString();
                Debug.Log(trackStatus);

                if (adptMeasureForceSet > 0)
                {
                    if (!scoreEnd)
                    {
                        nextMeasure = adptMeasureForceSet;
                        adptMeasureForceSet = -1;
                    }
                }

                StartCoroutine(TMT(nextMeasure, scoreEnd));
            }
        }

        
    }
    /*
    // 곡 내에서 한 마디의 재생과 종료 이후 다음 마디 재생을 담당하는 메서드
    private void TrackMeasureTransition(int measureTo)
    {
        if(adptTrackIsPlaying == true)
        {
            musSource.loop = false;
            
            musSource.clip = adptTrackClips[measureTo];
            Debug.Log(adptTrackClips[measureTo]);
            var clipLength = musSource.clip.length;
            musSource.Play();

            bool scoreEnd = false;
            if(adptTrackClips.Length == measureTo + 1)
            {
                scoreEnd = true;
                Debug.Log("적응형 트랙 마지막 마디입니다.");
            }

            StartCoroutine(AdaptiveNextTrack(measureTo, clipLength, scoreEnd));
        }
        return;
    }

    // 마디 재생 종료 후 다음 재생할 마디를 선택해주는 코루틴
    IEnumerator AdaptiveNextTrack(int nextMeasure, float delayedTime, bool scoreEnd)
    {
        if (scoreEnd == true) adptTrackIsPlaying = false; // 만약 scoreEnd가 참이면 적응형트랙 재생을 멈춘다
        yield return new WaitForSeconds(delayedTime);

        var addMeasure = 0; // 다음 마디로 넘어갈 준비가 안되었다면 이전 마디를 반복한다.
        if(adptTrackTransitionIs && !scoreEnd) // 다음 마디로 넘어갈 준비가 되었다면 넘긴다.
        {
            addMeasure = 1;
            adptTrackTransitionIs = false;
        } 

        TrackMeasureTransition(nextMeasure + addMeasure);
    }
   */
}
