using UnityEngine;

[DisallowMultipleComponent]
public class BAAudioManager : MonoBehaviour
{
    private const float _buttonDuplicatePreventionTime = 0.08f;

    [SerializeField] private AudioSource _bgmAudioSource;
    [SerializeField] private AudioSource _sfxAudioSource;
    [SerializeField] private AudioClip _mainBgmClip;
    [SerializeField] private AudioClip _battleBgmClip;
    [SerializeField] private AudioClip _buttonClip;
    [SerializeField] private AudioClip _braveBustClip;
    [SerializeField] private AudioClip _rhinoCarrierClip;
    [SerializeField] private AudioClip _assembleClip;
    [SerializeField] private AudioClip _hitClip;
    [SerializeField] private AudioClip _deathClip;

    private BASkillManager _skillManager;
    private BASupportManager _supportManager;
    private BAAssembleManager _assembleManager;
    private BABattleManager _battleManager;
    private float _lastButtonPlayTime = float.NegativeInfinity;
    private int _lastHitPlayFrame = -1;

    public static BAAudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_bgmAudioSource != null)
        {
            _bgmAudioSource.loop = true;
        }

        if (_sfxAudioSource != null)
        {
            _sfxAudioSource.loop = false;
            _sfxAudioSource.ignoreListenerPause = true;
        }
    }

    private void Start()
    {
        _skillManager = BASkillManager.Instance;
        _supportManager = BASupportManager.Instance;
        _assembleManager = BAAssembleManager.Instance;
        _battleManager = BABattleManager.Instance;

        if (_skillManager != null)
        {
            _skillManager.SkillUsed += OnSkillUsed;
        }

        if (_supportManager != null)
        {
            _supportManager.AssembledSupportUsed += OnAssembledSupportUsed;
        }

        if (_assembleManager != null)
        {
            _assembleManager.AssembleStateChanged += OnAssembleStateChanged;
        }

        if (_battleManager != null)
        {
            _battleManager.DamageApplied += OnDamageApplied;
        }
    }

    public void PlayMainBgm()
    {
        PlayBgm(_mainBgmClip);
    }

    public void PlayBattleBgm()
    {
        PlayBgm(_battleBgmClip);
    }

    public void PlayButton()
    {
        if (Time.unscaledTime - _lastButtonPlayTime < _buttonDuplicatePreventionTime)
        {
            return;
        }

        _lastButtonPlayTime = Time.unscaledTime;
        PlaySfx(_buttonClip);
    }

    public void PlayBraveBust()
    {
        PlaySfx(_braveBustClip);
    }

    public void PlayRhinoCarrier()
    {
        PlaySfx(_rhinoCarrierClip);
    }

    public void PlayAssemble()
    {
        PlaySfx(_assembleClip);
    }

    public void PlayHit()
    {
        if (_lastHitPlayFrame == Time.frameCount)
        {
            return;
        }

        _lastHitPlayFrame = Time.frameCount;
        PlaySfx(_hitClip);
    }

    public void PlayDeath()
    {
        PlaySfx(_deathClip);
    }

    private void PlayBgm(AudioClip clip)
    {
        if (_bgmAudioSource == null || clip == null)
        {
            return;
        }

        if (_bgmAudioSource.isPlaying && _bgmAudioSource.clip == clip)
        {
            return;
        }

        _bgmAudioSource.clip = clip;
        _bgmAudioSource.Play();
    }

    private void PlaySfx(AudioClip clip)
    {
        if (_sfxAudioSource == null || clip == null)
        {
            return;
        }

        _sfxAudioSource.PlayOneShot(clip);
    }

    private void OnSkillUsed(int hitCount)
    {
        PlayBraveBust();
    }

    private void OnAssembledSupportUsed()
    {
        PlayRhinoCarrier();
    }

    private void OnAssembleStateChanged(bool isAssembled)
    {
        if (isAssembled)
        {
            PlayAssemble();
        }
    }

    private void OnDamageApplied(BAUnitView attacker, BAUnitView target, float damage)
    {
        PlayHit();
    }

    private void OnDestroy()
    {
        if (_skillManager != null)
        {
            _skillManager.SkillUsed -= OnSkillUsed;
        }

        if (_supportManager != null)
        {
            _supportManager.AssembledSupportUsed -= OnAssembledSupportUsed;
        }

        if (_assembleManager != null)
        {
            _assembleManager.AssembleStateChanged -= OnAssembleStateChanged;
        }

        if (_battleManager != null)
        {
            _battleManager.DamageApplied -= OnDamageApplied;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
