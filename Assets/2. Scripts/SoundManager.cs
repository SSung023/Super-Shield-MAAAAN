using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    // 어떠한 스크립트에서도 사운드 매니저로 접근할 수 있는 스태틱 필드
    public static SoundManager _snd;

    private AudioSource     musSource; // 배경음악 오디오소스
    private AudioSource     adpSource; // 적응형 사운드트랙 재생 시 추가로 사용하는 오디오소스
    private AudioSource     sfxSource; // 글로벌 효과음을 재생할 때 사용하는 오디오소스
    private AudioSource     ambSource; // 글로벌 환경음을 재생할 때 사용하는 오디오소스
    private AudioSource     uixSource; // 인터페이스 효과음을 재생할 때 사용하는 오디오소스

    private AudioListener listener;

    [SerializeField]
    private AudioMixer masterMix; // 마스터 믹서를 할당

    // 플레이어가 옵션에서 조절할 수 있는 최상위 믹서
    AudioMixer topMusic, topDirect, topAmbient, topInterface;

    // 믹서 설정 관련 필드
    private bool masMute = false, musMute = false, sfxMute = false, ambMute = false, uixMute = false;
    private float masVol, musVol, sfxVol, ambVol, uixVol;

    // 일반 배경음악 재생 시 사용하는 현재 트랙넘버
    private int currentTrackNo = 0;

    /// <summary>
    /// 사운드의 타입, 음악/효과음/환경음/임시효과음/에러
    /// </summary>
    public enum SoundType : int { MUSIC, SFX, AMBIENT, INSTANTSFX, ERROR };

    /// <summary>
    /// 플레이어가 옵션으로 조절 가능한 믹서의 타입
    /// </summary>
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
        StopMusic();

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
        StopMusic();

        if (number < musClips.Length && number >= 0)
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
    /// 글로벌 효과음을 재생합니다. 루프 설정 여부를 선택 가능합니다. (루프 주의해서 사용)
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

    // ----------------------------------- 배경음악 정지 관련 메서드 -------------------------------------

    /// <summary>
    /// 모든 배경음악을 즉시 종료합니다.
    /// </summary>
    public void StopMusic()
    {
        StopCoroutine("AdptTrackClipStarter");

        musSource.Stop();
        adpSource.Stop();

        musSource.clip = null;
        adpSource.clip = null;

        adptTrackIsPlaying = false;
        toggle = 0;
        AdptTransition = false;
        AdptNextMeasureForceSet = -1;
    }

    /// <summary>
    /// 모든 배경음악을 천천히 종료합니다.
    /// </summary>
    /// <param name="now">현재 재생중인 클립이 종료되기를 기다리지 않고 즉시 트랙을 종료합니다.</param>
    /// <param name="fadeout">현재 재생중인 클립을 페이드아웃해서 종료시킬지 결정합니다. now == false 일 경우 적용되지 않습니다.</param>
    public void StopMusic(bool now, bool fadeout)
    {
        if (now)
        {
            if (fadeout)
            {

            }
            musSource.Stop();
            adpSource.Stop();

            musSource.clip = null;
            adpSource.clip = null;
        }

        adptTrackIsPlaying = false;
        toggle = 0;
        AdptTransition = false;
        AdptNextMeasureForceSet = -1;

        //StopCoroutine("AdptTrackClipStarter");
    }

    // ---------------------------------------------------- Awake 메서드 -----------------------------------------------

    // 씬 시작 직전에 스태틱 필드에 _snd를 넣고, AudioListener를 찾고 할당함
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

        InitializeAudiosources();

        InitializeMixers();

        SaveMixerStatusAll();

        listener = GetComponent<AudioListener>();
    }

    private void Start()
    {
        /*
        if (SceneManager.GetActiveScene().buildIndex == 1)
        { MusPlay(1, true); }
        else
        { MusPlay(0, true); }
        */
    }

    // 임시로 사운드 매니저 조작을 담당하는 메서드
    private void Update()
    {
        if (Input.GetKey(KeyCode.BackQuote))
        {
                 if (Input.GetKeyDown(KeyCode.Alpha1)) { ToggleMuteMixer(MixerType.MASTER); }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) { ToggleMuteMixer(MixerType.MUSIC); }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) { ToggleMuteMixer(MixerType.DIRECT); }
            else if (Input.GetKeyDown(KeyCode.Alpha4)) { ToggleMuteMixer(MixerType.AMBIENT); }
            else if (Input.GetKeyDown(KeyCode.Alpha5)) { ToggleMuteMixer(MixerType.INTERFACE); }
            else if (Input.GetKeyDown(KeyCode.Q))      { SongChange(-1); }
            else if (Input.GetKeyDown(KeyCode.E))      { SongChange(1);  }
            else if (Input.GetKeyDown(KeyCode.Alpha6)) { AdptTrackStart(0); }
            else if (Input.GetKeyDown(KeyCode.Alpha7)) { transitionOn = true; Debug.Log(transitionOn); }
            else if (Input.GetKeyDown(KeyCode.Alpha8)) { adptMeasureForceSet = 11; }
            else                                       { return; }
        }
    }

    // --------------------------------- 공용 초기화 메서드 ------------------------------
    
    // 오디오소스를 필드에 할당하는 초기화 메서드
    private void InitializeAudiosources()
    {
        AudioSource[] audSources = GetComponents<AudioSource>();

        var count = 0;

        while (count < audSources.Length)
        {
            switch (audSources[count].outputAudioMixerGroup.name)
            {
                case "Music":
                    if (musSource == null) { musSource = audSources[count]; }
                    else { adpSource = audSources[count]; }
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
    }

    // --------------------------------- 믹서 관련 메서드 --------------------------------

     // 최상위 믹서들을 필드에 할당하는 초기화 메서드
    private void InitializeMixers()
    {
        AudioMixerGroup[] mixers = masterMix.FindMatchingGroups("Top");

        var count = 0;

        while (count < mixers.Length)
        {
            switch (mixers[count].name)
            {
                case "TopMusic":
                    topMusic = mixers[count].audioMixer;
                    break;
                case "TopDirect":
                    topDirect = mixers[count].audioMixer;
                    break;
                case "TopAmbient":
                    topAmbient = mixers[count].audioMixer;
                    break;
                case "TopInterface":
                    topInterface = mixers[count].audioMixer;
                    break;
            }
            count++;
        }
    }

    /// <summary>
    /// 현재 믹서의 볼륨 상태를 저장하는 메서드
    /// </summary>
    private void SaveMixerStatusAll()
    {
        if (!masMute) masterMix.GetFloat("MasterMixerVol", out masVol);
        if (!musMute) topMusic.GetFloat("MusicMixerVol", out musVol);
        if (!sfxMute) topDirect.GetFloat("DirectMixerVol", out sfxVol);
        if (!ambMute) topAmbient.GetFloat("AmbientMixerVol", out ambVol);
        if (!uixMute) topInterface.GetFloat("InterfaceMixerVol", out uixVol);
    }

    /// <summary>
    /// 특정 믹서의 볼륨 상태만 저장하는 메서드
    /// </summary>
    /// <param name="type">믹서타입 Master, Music, Direct, Ambient, Interface</param>
    private void SaveMixerStatus(MixerType type)
    {
        switch (type)
        {
            case MixerType.MASTER:
                if (!masMute) masterMix.GetFloat("MasterMixerVol", out masVol);
                break;
            case MixerType.MUSIC:
                if (!musMute) topMusic.GetFloat("MusicMixerVol", out musVol);
                break;
            case MixerType.DIRECT:
                if (!sfxMute) topDirect.GetFloat("DirectMixerVol", out sfxVol);
                break;
            case MixerType.AMBIENT:
                if (!ambMute) topAmbient.GetFloat("AmbientMixerVol", out ambVol);
                break;
            case MixerType.INTERFACE:
                if (!uixMute) topInterface.GetFloat("InterfaceMixerVol", out uixVol);
                break;
        }
    }

    /// <summary>
    /// 지정된 믹서의 볼륨을 0으로 만들거나 이전 값으로 복구하는 메서드
    /// </summary>
    /// <param name="type">믹서타입 Master, Music, Direct, Ambient, Interface</param>
    private void ToggleMuteMixer(MixerType type)
    {
        switch (type)
        {
            case MixerType.MASTER:
                if (!masMute)
                {
                    SaveMixerStatus(MixerType.MASTER);
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
                    SaveMixerStatus(MixerType.MUSIC);
                    topMusic.SetFloat("MusicMixerVol", -80.0f);
                    musMute = true;
                }
                else
                {
                    topMusic.SetFloat("MusicMixerVol", musVol);
                    musMute = false;
                }
                break;
            case MixerType.DIRECT:
                if (!sfxMute)
                {
                    SaveMixerStatus(MixerType.DIRECT);
                    topDirect.SetFloat("DirectMixerVol", -80.0f);
                    sfxMute = true;
                }
                else
                {
                    topDirect.SetFloat("DirectMixerVol", sfxVol);
                    sfxMute = false;
                }
                break;
            case MixerType.AMBIENT:
                if (!ambMute)
                {
                    SaveMixerStatus(MixerType.AMBIENT);
                    topAmbient.SetFloat("AmbientMixerVol", -80.0f);
                    ambMute = true;
                }
                else
                {
                    topAmbient.SetFloat("AmbientMixerVol", ambVol);
                    ambMute = false;
                }
                break;
            case MixerType.INTERFACE:
                if (!uixMute)
                {
                    SaveMixerStatus(MixerType.INTERFACE);
                    topInterface.SetFloat("InterfaceMixerVol", -80.0f);
                    uixMute = true;
                }
                else
                {
                    topInterface.SetFloat("InterfaceMixerVol", uixVol);
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

            MusPlay(currentTrackNo, true);
    }

    // --------------------------------- 적응형 사운드트랙 구현 요소 선언 ---------------------------------

    // 현재 적응형 사운드 트랙이 재생 중인지 확인하는 필드
    private bool adptTrackIsPlaying = false;

    // 현재 재생 중인 사운드 트랙에서 다음 파트로 전환할 것인지를 결정하는 필드
    private bool transitionOn = false;

    // 현재 재생 중인 사운드 트랙 내 현재 재생 중인 마디의 재생이 끝나면, 다음에 재생할 마디를 강제로 지정하는 필드
    private int adptMeasureForceSet = -1; // -1일 경우 비활성화, 0부터는 마디 번호 지정

    // 현재 선택한 적응형 트랙의 클래스
    private AdaptiveTrack currentAdaptiveTrack;

    AudioSource[] bgmSource = new AudioSource[2];       // 적응형 사운드트랙 재생 시 번갈아가며 사용하는 오디오소스 배열

    private int toggle;                     // 적응형 사운드트랙을 재생할 때 어떤 오디오소스를 사용할지 결정하는 필드
    private double measureStartTime;        // 마디가 시작되는 시간
    private double measureDuration;         // 현재 마디가 지속되는 시간
    private double measureCheckTime;        // 다음 마디를 시작하는 시간 - offset의 을 말함, transition 상태를 점검하는 시간

    [SerializeField]
    private double adaptiveTrackStartOffset = 0.5d; // 오디오클립 재생 시 발생하는 로딩시간을 보완하는 값

    /// <summary>
    /// NEXT : 다음 마디로 이동한다.
    /// LOOP : 스스로 루프한다.             transitionOn일 경우 다음 마디로 이동한다.
    /// MOVE : 특정 마디로 이동시킨다.      transitionOn일 경우 다음 마디로 이동한다.
    /// 이 모든 성질은 1순위 강제 중지 > 2순위 강제 다음 마디 설정 > 3순위로 적용된다.
    /// </summary>
    public enum MeasureType : int { NEXT, LOOP, MOVE, END };

    // --------------------------------- 적응형 사운드트랙 관련 메서드 ---------------------------------

    /// <summary>
    /// 적응형 사운드트랙을 시작합니다.
    /// </summary>
    /// <param name="trackNo">재생할 트랙 넘버</param>
    public void AdptTrackStart(int trackNo)
    {
        StopMusic();

        adptTrackIsPlaying = true;
        toggle = 0;

        currentAdaptiveTrack = adaptiveTrack[trackNo];

        measureStartTime = AudioSettings.dspTime + adaptiveTrackStartOffset;

        StartCoroutine(AdptTrackClipStarter(0));
    }

    /// <summary>
    /// preDelay 초만큼 기다린 후 적응형 사운드트랙 재생을 시작합니다.
    /// </summary>
    /// <param name="trackNo">재생할 트랙 넘버</param>
    /// <param name="preDelay">(double) 기다리는 시간</param>
    public void AdptTrackStart(int trackNo, double preDelay)
    {
        adptTrackIsPlaying = true;
        toggle = 0;

        currentAdaptiveTrack = adaptiveTrack[trackNo];

        measureStartTime = AudioSettings.dspTime + adaptiveTrackStartOffset + preDelay;

        StartCoroutine(AdptTrackClipStarter(0));
    }

    // 클립 할당, 재생, 시간 계산을 담당하는 코루틴
    // 1. 클립을 할당하고, 
    // 2. measureStartTime에 클립을 시작한다. 
    // 3. measureDuration을 계산, 다음 measureStartTime을 산출
    // 4. checkTime에 QueueMeasure를 실행
    IEnumerator AdptTrackClipStarter(int startClip)
    {
        if (adptTrackIsPlaying)
        {
            bgmSource[toggle].clip = currentAdaptiveTrack.clips[startClip];     // 선택된 오디오소스에 클립을 넣는다

            bgmSource[toggle].PlayScheduled(measureStartTime);                  // 지정된 시작 시간에 클립을 재생한다.

            measureDuration = (double)bgmSource[toggle].clip.samples / bgmSource[toggle].clip.frequency; // 한 클립의 길이는 샘플/주파수로 계산 가능하다.
            measureStartTime = measureStartTime + measureDuration;                                       // 다음 클립의 시작 시간은 지정 시작 시간 + 한 클립을 연주하는데 걸리는 시간이다.
            measureCheckTime = measureStartTime - adaptiveTrackStartOffset;                              // 다음 클립의 시작 시간 - offset이 되면, 다음에 어떤 클립을 연주해야 할 지 선택해야한다.

            yield return new WaitUntil(() => AudioSettings.dspTime > measureCheckTime);                 // 다음 클립의 시작 시간 - offset이 될 때까지 기다린다.

            QueueMeasure(startClip);
        }
    }

    // 다음에 재생할 클립을 결정한 뒤 해당 클립을 코루틴으로 호출하는 메서드
    private void QueueMeasure(int currentClip)
    {
        toggle = 1 - toggle;

        MeasureType type = GetMeasureTypes()[currentClip];
        int allocation = GetMeasureAllocation()[currentClip];

        // 에러 체크용
        if(type == MeasureType.MOVE)
        {
            if (allocation < 0)
            {
                string message = "오류 : " + currentClip.ToString() + " 번 클립은 MOVE 이지만 다음 마디가 할당되어있지 않습니다.";
                print(message);
            }
        }

        int nextClip = 0;

        if (adptMeasureForceSet < 0)
        {
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
                    nextClip = transitionOn == true ? currentClip + 1 : nextClip = allocation;
                    transitionOn = false;
                    break;
                case MeasureType.END:
                    StopMusic(false, false); // 마지막 클립을 페이드아웃 없이 끝날 때까지 연주한다.
                    break;
            }
        }
        else
        {
            int adjust = Mathf.Clamp(adptMeasureForceSet, 0, currentAdaptiveTrack.clips.Length - 1); // 강제로 마디를 지정했을 때 범위를 벗어나지 않도록 제한을 둠
            nextClip = adjust;

            transitionOn = false;
            adptMeasureForceSet = -1;    
        }

        StartCoroutine(AdptTrackClipStarter(nextClip));
    }

    private MeasureType[] GetMeasureTypes()
    {
        MeasureType[] types = currentAdaptiveTrack.RecordInfo.types;
        return types;
    }

    private int[] GetMeasureAllocation()
    {
        int[] allocates = currentAdaptiveTrack.RecordInfo.allocatedNextMeasures;
        return allocates;
    }

    // ------------------------------ 적응형 사운드트랙 관련 외부에서 호출가능한 프로퍼티/메서드 ----------------------

    /// <summary>
    /// 현재 transitionOn 변수의 상태를 반환하거나, transitionOn의 상태를 스크립트 외부에서 변경하는 프로퍼티
    /// </summary>
    public bool AdptTransition
    {
        get { return transitionOn; }
        set { transitionOn = AdptTransition; }
    }

    /// <summary>
    /// 현재 적응형 사운드트랙 재생 상태를 반환하는 프로퍼티
    /// </summary>
    public bool AdaptiveTrackPlayStatus
    {
        get { return adptTrackIsPlaying; }
    }

    /// <summary>
    /// 현재 재생중인 적응형 사운드트랙의 다음 마디(클립)을 강제로 설정한다. transitionOn의 값과는 상관 없이 진행된다.
    /// </summary>
    public int AdptNextMeasureForceSet
    {
        get { return adptMeasureForceSet; }
        set { if (adptTrackIsPlaying) { adptMeasureForceSet = AdptNextMeasureForceSet; } }
    }

    // ------------------------------ 적응형 사운드트랙 관련 마디 데이터를 저장하는 클래스 ----------------------

    // 마디 하나하나의 성질과 MOVE일 경우 다음으로 이동해야 하는 마디를 기록하는 클래스
    
    public AdaptiveTrack[] adaptiveTrack = new AdaptiveTrack[0];

    [Serializable]
    public class AdaptiveTrack
    {
        [SerializeField]
        private int trackNo;
        public AudioClip[] clips;
        public RecordInfo RecordInfo;

        public int TrackNo
        {
            get { return trackNo; }
        }
    }

    [Serializable]
    public class RecordInfo
    {
        public RecordInfo(int length)
        {
            types = new MeasureType[length];
            allocatedNextMeasures = new int[length];
        }
        public MeasureType[] types; // 적응형 트랙의 마디별 특성을 저장하는 배열
        public int[] allocatedNextMeasures; // MeasureType.MOVE 일 경우 지정되는 다음 마디
    }

}
