using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    // 어떠한 스크립트에서도 사운드 매니저로 접근할 수 있는 스태틱 필드
    public static SoundManager _snd;

    private AudioSource     musSource; // 배경음악 오디오소스
    private AudioSource     adpSource; // 적응형 사운드트랙 시 추가 사용하는 오디오소스
    private AudioSource     sfxSource; // 글로벌 효과음을 재생할 때 사용하는 오디오소스
    private AudioSource     ambSource; // 글로벌 환경음을 재생할 때 사용하는 오디오소스
    private AudioSource     uixSource; // 인터페이스 효과음을 재생할 때 사용하는 오디오소스

    private AudioListener listener;

    [SerializeField]
    private AudioMixer masterMix; // 마스터 믹서를 할당

    // 믹서 뮤트 관련 필드
    private bool masMute = false, musMute = false, sfxMute = false, ambMute = false, uixMute = false;
    private float masVol, musVol, sfxVol, ambVol, uixVol;

    // 일반 배경음악 재생 시 사용하는 현재 트랙넘버
    private int currentTrackNo = 0;

    public enum SoundType : int { MUSIC, SFX, AMBIENT, INSTANTSFX, ERROR };
    public enum MixerType : int { MASTER, MUSIC, DIRECT, AMBIENT, INTERFACE };

    //음악, 효과음, 환경음 클립이 저장되는 필드
    [SerializeField]
    private AudioClip[] musClips;

    [SerializeField]
    private AudioClip[] sfxClips;

    [SerializeField]
    private AudioClip[] ambClips;

    // ----------------------------------- 사운드 재생 관련 메서드 -------------------------------------

    // 다른 스크립트에서 효과음을 불러올 때 거쳐가는 메서드 
    /// <summary>
    /// 지정한 AudioSource에서 로컬 효과음이 나도록 합니다.
    /// <para> SoundManager에 있는 오디오 클립을 호출해서 사용합니다. </para>
    /// </summary>
    /// <param name="caller">효과음을 출력할 AudioSource</param>
    /// <param name="number">효과음 번호</param>
    public void SfxCall(AudioSource caller, int number)
    {
        if (number < sfxClips.Length && number >= 0)
        {
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
        if (number < sfxClips.Length && number >= 0)
        {
            caller.PlayOneShot(sfxClips[number], caller.volume);
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
        if (number < sfxClips.Length && number >= 0)
        {
            caller.PlayOneShot(sfxClips[number], caller.volume * volume);
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
    /// <param name="sndType">= 배경음 / 1 = 효과음 / 2 = 환경음 / 3 = 인스턴트 효과음</param>
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
        var sfxOutput = Random.Range(rangeStart, rangeEnd + 1);
        SfxCall(caller, sfxOutput);
    }

    // ---------------------------------------------------- Awake 메서드 -----------------------------------------------

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

        AudioSource[] audSources = GetComponents<AudioSource>();
        Debug.Log(audSources.Length);

        var count = 0;

        while (count < audSources.Length)
        {
            switch (audSources[count].outputAudioMixerGroup.name)
            {
                case "Music":
                    if (musSource == null) { musSource = audSources[count]; }
                    else                   { adpSource = audSources[count]; }
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

        // 적응형 사운드트랙때 사용할 두 개의 배경음악 오디오소스를 저장
        bgmSource[0] = musSource;
        bgmSource[1] = adpSource;

        // 시작 직후의 믹서 볼륨을 저장한다.
        SaveCurrentMixerStatus();

        listener = GetComponent<AudioListener>();
    }


    private void Start()
    {
        /*
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            MusPlay(1, true);
        }
        else
        {
            MusPlay(0, true);
        }
        */
    }

    // 임시로 사운드 매니저 조작을 담당하는 메서드
    private void Update()
    {
        if (Input.GetKey(KeyCode.BackQuote))
        {
                 if (Input.GetKeyDown(KeyCode.Alpha1)) { MuteMixer(MixerType.MASTER); }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) { MuteMixer(MixerType.MUSIC); }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) { MuteMixer(MixerType.DIRECT); }
            else if (Input.GetKeyDown(KeyCode.Alpha4)) { MuteMixer(MixerType.AMBIENT); }
            else if (Input.GetKeyDown(KeyCode.Alpha5)) { MuteMixer(MixerType.INTERFACE); }
            else if (Input.GetKeyDown(KeyCode.Q))      { SongChange(-1); }
            else if (Input.GetKeyDown(KeyCode.E))      { SongChange(1);  }
            else if (Input.GetKeyDown(KeyCode.Alpha6)) { AdptTrackStart(0); }
            else if (Input.GetKeyDown(KeyCode.Alpha7)) { transitionOn = true; Debug.Log(transitionOn); }
            else                                       { return; }
        }
    }

    // --------------------------------- 믹서 관련 메서드 --------------------------------

    /// <summary>
    /// 현재 믹서의 볼륨 상태를 저장하는 메서드
    /// </summary>
    private void SaveCurrentMixerStatus()
    {
        masterMix.GetFloat("MasterMixerVol", out masVol);
        musSource.outputAudioMixerGroup.audioMixer.GetFloat("MusicMixerVol", out musVol);
        sfxSource.outputAudioMixerGroup.audioMixer.GetFloat("DirectMixerVol", out sfxVol);
        ambSource.outputAudioMixerGroup.audioMixer.GetFloat("AmbientMixerVol", out ambVol);
        uixSource.outputAudioMixerGroup.audioMixer.GetFloat("InterfaceMixerVol", out uixVol);
    }

    // 지정된 믹서의 볼륨을 0으로 만들거나 이전 값으로 복구하는 메서드
    private void MuteMixer(MixerType type)
    {
        switch (type)
        {
            case MixerType.MASTER:
                if (!masMute)
                {
                    masterMix.GetFloat("MasterMixerVol", out masVol);
                    masterMix.SetFloat("MasterMixerVol", -80.0f);
                    masMute = true;
                }
                else
                {
                    masterMix.SetFloat("MasterMixerVol", masVol);
                    masMute = false;
                }
                break;
            case MixerType.MUSIC:
                if (!musMute)
                {
                    musSource.outputAudioMixerGroup.audioMixer.GetFloat("MusicMixerVol", out musVol);
                    musSource.outputAudioMixerGroup.audioMixer.SetFloat("MusicMixerVol", -80.0f);
                    musMute = true;
                }
                else
                {
                    musSource.outputAudioMixerGroup.audioMixer.SetFloat("MusicMixerVol", musVol);
                    musMute = false;
                }
                break;
            case MixerType.DIRECT:
                if (!sfxMute)
                {
                    sfxSource.outputAudioMixerGroup.audioMixer.GetFloat("DirectMixerVol", out sfxVol);
                    sfxSource.outputAudioMixerGroup.audioMixer.SetFloat("DirectMixerVol", -80.0f);
                    sfxMute = true;
                }
                else
                {
                    sfxSource.outputAudioMixerGroup.audioMixer.SetFloat("DirectMixerVol", sfxVol);
                    sfxMute = false;
                }
                break;
            case MixerType.AMBIENT:
                if (!ambMute)
                {
                    ambSource.outputAudioMixerGroup.audioMixer.GetFloat("AmbientMixerVol", out ambVol);
                    ambSource.outputAudioMixerGroup.audioMixer.SetFloat("AmbientMixerVol", -80.0f);
                    ambMute = true;
                }
                else
                {
                    ambSource.outputAudioMixerGroup.audioMixer.SetFloat("AmbientMixerVol", ambVol);
                    ambMute = false;
                }
                break;
            case MixerType.INTERFACE:
                if (!uixMute)
                {
                    uixSource.outputAudioMixerGroup.audioMixer.GetFloat("InterfaceMixerVol", out uixVol);
                    uixSource.outputAudioMixerGroup.audioMixer.SetFloat("InterfaceMixerVol", -80.0f);
                    uixMute = true;
                }
                else
                {
                    uixSource.outputAudioMixerGroup.audioMixer.SetFloat("InterfaceMixerVol", uixVol);
                    uixMute = false;
                }
                break;
        }
    }

    // --------------------------------- BGM 재생 관련 메서드 ---------------------------------

    // 임시로 트랙 변경을 담당하는 메서드
    private void SongChange(int trackTo)
    {
        currentTrackNo = currentTrackNo + trackTo;
        currentTrackNo = Mathf.Clamp(currentTrackNo, 0, musClips.Length - 1);
        Debug.Log("현재 곡 번호는 " + currentTrackNo.ToString() + " 번입니다.");

        musSource.Pause(); // 이 메서드 잘 작동하는지 모르겠는데 나중에 검토해봐야 할 듯, 정확한 용도는 무엇인지?
        MusPlay(currentTrackNo, true);
    }

    // --------------------------------- 적응형 사운드트랙 구현 메서드 ---------------------------------

    // 현재 적응형 사운드 트랙이 재생 중인지 확인하는 필드
    private bool adptTrackIsPlaying = false;

    // 현재 재생 중인 사운드 트랙에서 다음 파트로 전환할 것인지를 결정하는 필드
    private bool transitionOn = false;

    // 현재 재생 중인 사운드 트랙 내 현재 재생 중인 마디의 재생이 끝나면, 다음에 재생할 마디를 강제로 지정하는 필드
    private int adptMeasureForceSet = -1; // -1일 경우 비활성화, 0부터는 마디 번호 지정

    [SerializeField]
    private AudioClip[] adptTrackClips; // 현재 적응형 트랙의 마디 클립을 저장하는 배열

    private MeasureType[] currentAdptTracks; // 현재 재생 중인 적응형 트랙의 마디별 특성을 저장하는 배열

    int toggle;
    double measrueStartTime;
    double measureDuration;
    double measureCheckTime;
    public double adaptiveTrackStartOffset = 0.5d;
    AudioSource[] bgmSource = new AudioSource[2];

    // 마디 별 성질 
    // NEXT : 다음 마디로 이동한다.
    // LOOP : 스스로 루프한다.             transitionOn일 경우 다음 마디로 이동한다.
    // MOVE : 특정 마디로 이동시킨다.      transitionOn일 경우 다음 마디로 이동한다.
    // 이 모든 성질은 1순위 강제 중지 > 2순위 강제 다음 마디 설정 > 3순위로 적용된다.
    public enum MeasureType : int { NEXT, LOOP, MOVE, END };

    void AdptTrackStart(int trackNo)
    {
        adptTrackIsPlaying = true;
        toggle = 0;

        AudioClip[] thisClips = adptTrackClips;
        MeasureType[] measureTypes = GetMeasureTypes(trackNo);

        measrueStartTime = AudioSettings.dspTime + adaptiveTrackStartOffset;

        StartCoroutine(AdptTrackClipStarter(0));
    }

    IEnumerator AdptTrackClipStarter(int startClip)
    {
        if (adptTrackIsPlaying)
        {
            bgmSource[toggle].clip = adptTrackClips[startClip];

            bgmSource[toggle].PlayScheduled(measrueStartTime);

            measureDuration = (double)bgmSource[toggle].clip.samples / bgmSource[toggle].clip.frequency;
            measrueStartTime = measrueStartTime + measureDuration;
            measureCheckTime = measrueStartTime - adaptiveTrackStartOffset;

            //Debug.Log(measrueStartTime.ToString());
            //Debug.Log(measureDuration.ToString());
            //Debug.Log(measureCheckTime.ToString());

            yield return new WaitUntil(() => AudioSettings.dspTime > measureCheckTime);
            //Debug.Log("() => AudioSettings.dspTime > measureCheckTime");

            QueueMeasure(startClip);
        }
    }

    void QueueMeasure(int currentClip)
    {
        toggle = 1 - toggle;

        MeasureType type = GetMeasureTypes(0)[currentClip];

        int nextClip = 0;

        switch (type)
        {
            case MeasureType.NEXT:
                nextClip = currentClip + 1;
                break;
            case MeasureType.LOOP:
                nextClip = transitionOn == true ? currentClip + 1 : currentClip;
                transitionOn = false;
                break;
            case MeasureType.MOVE:
                nextClip = transitionOn == true ? currentClip + 1 : currentClip - 5; // -5는 임시로 할당
                transitionOn = false;
                break;
            case MeasureType.END:
                adptTrackIsPlaying = false;
                transitionOn = false;
                break;
        }

        StartCoroutine(AdptTrackClipStarter(nextClip));
    }

    MeasureType[] GetMeasureTypes(int trackNo)
    {
        currentAdptTracks = new MeasureType[10];

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

        return currentAdptTracks;
    }

    void StopAdaptiveSoundtrack(bool fadeout, bool now)
    {
        if (now)
        {
            musSource.Stop();
            adpSource.Stop();

            musSource.clip = null;
            adpSource.clip = null;
        }

        adptTrackIsPlaying = false;
        toggle = 0;
        transitionOn = false;
        
    }

    /*
    private void AdaptavieTrackScores()
    {
        adptTracks[0] = new MeasureType[10] { MeasureType.NEXT, MeasureType.LOOP, MeasureType.NEXT, MeasureType.NEXT, MeasureType.NEXT, MeasureType.NEXT, MeasureType.NEXT , MeasureType.NEXT , MeasureType.MOVE, MeasureType.END };

        //{ 0, (MeasureType)1, 0, 0, 0, 0, 0, 0, (MeasureType)2, (MeasureType)3 };
    }
    */


    /*
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
                        addMeasure = transitionOn == true ? addMeasure + 1 : addMeasure;
                        transitionOn = false;
                        break;
                    case MeasureType.MOVE:
                        addMeasure = transitionOn == true ? addMeasure + 1 : addMeasure -5; // -5는 임시로 할당
                        transitionOn = false;
                        break;
                    case MeasureType.END:
                        scoreEnd = true;
                        Debug.Log("적응형 트랙 마지막 마디입니다.");
                        transitionOn = false;
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
    }*/
}
