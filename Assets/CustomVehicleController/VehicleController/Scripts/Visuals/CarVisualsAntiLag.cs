using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Anti Lag Effect")]
    public class CarVisualsAntiLag : MonoBehaviour
    {
        [SerializeField]
        private VisualEffectAssetType.Type _antiLagVisualEffectType;

        [SerializeField]
        private Transform[] _exhaustsPositionArray;

        #region PS
        [SerializeField]
        private ParticleSystem _antiLagParticleSystem;
        private ParticleSystem[] _antiLagParticleSystemArray;
        #endregion

        #region VFX
        [SerializeField]
        private VisualEffectAsset _antiLagVFXAsset;
        private VisualEffect[] _antiLagVFXArray;
        #endregion

        [SerializeField]
        private float _backfireDelay = 0.25f;

        [SerializeField]
        private CurrentCarStats _currentCarStats;

        [SerializeField, Min(1)]
        private int _minBackfireCount = 2;
        [SerializeField]
        private int _maxBackfireCount = 5;

        private void Awake()
        {
            if(_antiLagVisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                InitializeVFX();
            }
            else
            {
                InstantiateAntiLagPS();
            }

            _currentCarStats.OnAntiLag += _currentCarStats_OnAntiLag;
            _currentCarStats.OnShiftedAntiLag += _currentCarStats_OnShiftedAntiLag;
        }


        private void _currentCarStats_OnShiftedAntiLag()
        {
            int size = _exhaustsPositionArray.Length;
            for (int i = 0; i < size; i++)
            {
                StartCoroutine(PlayAntilagNTimes(1, i, _backfireDelay));
            }
        }

        private void _currentCarStats_OnAntiLag()
        {
            int size = _exhaustsPositionArray.Length;
            for (int i = 0; i < size; i++)
            {
                StartCoroutine(PlayAntilagNTimes(Random.Range(_minBackfireCount, _maxBackfireCount), i, _backfireDelay));
            }
        }

        private void OnDestroy()
        {
            _currentCarStats.OnAntiLag -= _currentCarStats_OnAntiLag;
            _currentCarStats.OnShiftedAntiLag -= _currentCarStats_OnShiftedAntiLag;
        }

        private IEnumerator PlayAntilagNTimes(int times, int id, float delay)
        {

            if(_antiLagVisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                for (int i = 0; i < times; i++)
                {
                    _antiLagVFXArray[id].Play();
                    yield return new WaitForSeconds(delay);
                }
            }
            else
            {
                for (int i = 0; i < times; i++)
                {
                    _antiLagParticleSystemArray[id].Play();
                    yield return new WaitForSeconds(delay);
                }
                _antiLagParticleSystemArray[id].TriggerSubEmitter(0);
            }
        }

        private void InstantiateAntiLagPS()
        {
            if (_antiLagParticleSystem == null)
            {
                Debug.LogError("You have Anti-Lag Effect, but Particle System is not assigned"); ;
                return;
            }

            int size = _exhaustsPositionArray.Length;
            _antiLagParticleSystemArray = new ParticleSystem[size];
            for (int i = 0; i < size; i++)
            {
                _antiLagParticleSystemArray[i] = Instantiate(_antiLagParticleSystem);
                _antiLagParticleSystemArray[i].Stop();
                _antiLagParticleSystemArray[i].transform.parent = _exhaustsPositionArray[i].transform;
                _antiLagParticleSystemArray[i].transform.localPosition = new (0, 0, 0);
                _antiLagParticleSystemArray[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        private void InitializeVFX()
        {
            if (_antiLagVFXAsset == null)
            {
                Debug.LogWarning("You have Anti Lag Effect, but Visual Effect Asset is not assigned");
                return;
            }

            int size = _exhaustsPositionArray.Length;

            _antiLagVFXArray = new VisualEffect[size];

            for (int i = 0; i < size; i++)
            {
                _antiLagVFXArray[i] = _exhaustsPositionArray[i].gameObject.AddComponent<VisualEffect>();
                _antiLagVFXArray[i].visualEffectAsset = _antiLagVFXAsset;
                _antiLagVFXArray[i].Stop();
            }
        }

    }
}
