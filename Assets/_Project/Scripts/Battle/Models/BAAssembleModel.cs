using System;

public class BAAssembleModel
{
    private readonly string _id;
    private readonly string _displayName;
    private readonly string _supportPrefabKey;
    private readonly float _maxGauge;
    private readonly float _gaugeGainPerHit;
    private readonly float _duration;
    private readonly float _attackDamageMultiplier;
    private readonly float _moveSpeedMultiplier;
    private readonly float _attackSpeedMultiplier;

    private float _currentGauge;
    private float _remainingDuration;
    private bool _isAssembled;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string SupportPrefabKey => _supportPrefabKey;
    public float MaxGauge => _maxGauge;
    public float GaugeGainPerHit => _gaugeGainPerHit;
    public float Duration => _duration;
    public float AttackDamageMultiplier => _attackDamageMultiplier;
    public float MoveSpeedMultiplier => _moveSpeedMultiplier;
    public float AttackSpeedMultiplier => _attackSpeedMultiplier;
    public float CurrentGauge => _currentGauge;
    public float RemainingDuration => _remainingDuration;
    public bool IsAssembled => _isAssembled;
    public bool CanAssemble =>
        !_isAssembled &&
        _maxGauge > 0f &&
        _currentGauge >= _maxGauge;

    public event Action<float, float> GaugeChanged;
    public event Action<float, float> DurationChanged;
    public event Action<bool> AssembleStateChanged;

    public BAAssembleModel(BAAssembleData assembleData)
    {
        if (assembleData == null)
        {
            throw new ArgumentNullException(nameof(assembleData));
        }

        _id = assembleData.ID;
        _displayName = assembleData.DisplayName;
        _supportPrefabKey = assembleData.SupportPrefabKey;
        _maxGauge = Math.Max(0f, assembleData.MaxGauge);
        _gaugeGainPerHit = Math.Max(0f, assembleData.GaugeGainPerHit);
        _duration = Math.Max(0f, assembleData.Duration);
        _attackDamageMultiplier = Math.Max(0f, assembleData.AttackDamageMultiplier);
        _moveSpeedMultiplier = Math.Max(0f, assembleData.MoveSpeedMultiplier);
        _attackSpeedMultiplier = Math.Max(0f, assembleData.AttackSpeedMultiplier);
    }

    public void AddGaugeByHit()
    {
        if (_isAssembled)
        {
            return;
        }

        float nextGauge = Math.Min(_currentGauge + _gaugeGainPerHit, _maxGauge);

        if (_currentGauge == nextGauge)
        {
            return;
        }

        _currentGauge = nextGauge;
        GaugeChanged?.Invoke(_currentGauge, _maxGauge);
    }

    public bool TryStartAssemble()
    {
        if (!CanAssemble)
        {
            return false;
        }

        _currentGauge = 0f;
        _remainingDuration = _duration;
        _isAssembled = true;

        GaugeChanged?.Invoke(_currentGauge, _maxGauge);
        DurationChanged?.Invoke(_remainingDuration, _duration);
        AssembleStateChanged?.Invoke(_isAssembled);
        return true;
    }

    public void UpdateDuration(float deltaTime)
    {
        if (!_isAssembled || deltaTime <= 0f)
        {
            return;
        }

        _remainingDuration = Math.Max(0f, _remainingDuration - deltaTime);
        DurationChanged?.Invoke(_remainingDuration, _duration);

        if (_remainingDuration <= 0f)
        {
            EndAssemble();
        }
    }

    public void ResetState()
    {
        _currentGauge = 0f;
        _remainingDuration = 0f;
        _isAssembled = false;

        GaugeChanged?.Invoke(_currentGauge, _maxGauge);
        DurationChanged?.Invoke(_remainingDuration, _duration);
        AssembleStateChanged?.Invoke(_isAssembled);
    }

    private void EndAssemble()
    {
        if (!_isAssembled)
        {
            return;
        }

        _remainingDuration = 0f;
        _isAssembled = false;
        AssembleStateChanged?.Invoke(_isAssembled);
    }
}
