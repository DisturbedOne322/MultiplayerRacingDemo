using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    public class CarVisualsTireSmoke
    {
        private VisualEffect[] _tireSmokeVFXArray;
        private ParticleSystem[] _tireSmokePSArray;

        private TireSmokeParameters _effectParameters;

        private Transform[] _wheelMeshes;

        private Transform _transform;

        private float[] _radiusArray;

        public CarVisualsTireSmoke(Transform[] wheelMeshes, WheelController[] wheelControllers, Transform transform, TireSmokeParameters effectParameters)
        {
            _wheelMeshes = wheelMeshes;
            _transform = transform;
            _effectParameters  = effectParameters;

            _radiusArray = new float[wheelControllers.Length];
            for (int i = 0; i < wheelControllers.Length; i++)
                _radiusArray[i] = wheelControllers[i].Radius;

            if (_effectParameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                TryInstantiateVFX(wheelMeshes);
            else
                TryInstantiatePS(wheelMeshes);
        }

        private void TryInstantiateVFX(Transform[] wheelMeshes)
        {
            if (_effectParameters.VisualEffect.VFXAsset == null)
            {
                Debug.LogWarning("You have Tire Smoke Effect, but Visual Effect Asset is not assigned");
                return;
            }
            _tireSmokeVFXArray = new VisualEffect[wheelMeshes.Length];

            int size = wheelMeshes.Length;
            for (int i = 0; i < size; i++)
            {
                _tireSmokeVFXArray[i] = wheelMeshes[i].gameObject.AddComponent<VisualEffect>();
                _tireSmokeVFXArray[i].visualEffectAsset = _effectParameters.VisualEffect.VFXAsset;
                _tireSmokeVFXArray[i].Stop();
            }
        }

        private void TryInstantiatePS(Transform[] wheelMeshes)
        {
            if (_effectParameters.VisualEffect.ParticleSystem == null)
            {
                Debug.LogWarning("You have Tire Smoke Effect, but Particle System is not assigned");
                return;
            }
            _tireSmokePSArray = new ParticleSystem[wheelMeshes.Length];

            int size = wheelMeshes.Length;
            for (int i = 0; i < size; i++)
            {
                _tireSmokePSArray[i] = GameObject.Instantiate(_effectParameters.VisualEffect.ParticleSystem);
                _tireSmokePSArray[i].transform.parent = wheelMeshes[i];
                _tireSmokePSArray[i].Stop();
            }
        }

        public void HandleSmokeEffects(bool display, int id, Vector3 rbVelocityNorm, float speed)
        {
            if(_effectParameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                DisplayVFX(display, id, rbVelocityNorm, speed);
            else
                DisplayPS(display, id, rbVelocityNorm, speed);         
        }
        private void DisplayVFX(bool display, int id, Vector3 rbVelocityNorm, float speed)
        {
            if (_effectParameters.VisualEffect.VFXAsset == null)
                return;

            if (display)
            {
                _tireSmokeVFXArray[id].Play();
                _tireSmokeVFXArray[id].SetVector3("position", _wheelMeshes[id].position - new Vector3(0 , _radiusArray[id] - _effectParameters.VerticalOffset, 0));
                if (speed < 1)
                {
                    _tireSmokeVFXArray[id].SetVector3("velocity", -_transform.forward);
                }
                else
                {
                    _tireSmokeVFXArray[id].SetVector3("velocity", -rbVelocityNorm);
                }
            }
            else
            {
                _tireSmokeVFXArray[id].Stop();
            }
        }
        private void DisplayPS(bool display, int id, Vector3 rbVelocityNorm, float speed)
        {
            if (_effectParameters.VisualEffect.ParticleSystem == null)
                return;
            if (display)
            {
                if(Mathf.Abs(speed) < 1)
                    rbVelocityNorm = _transform.forward;

                _tireSmokePSArray[id].transform.position = _wheelMeshes[id].position - new Vector3(0, _radiusArray[id] - _effectParameters.VerticalOffset, 0);
                _tireSmokePSArray[id].transform.forward = -rbVelocityNorm;
                _tireSmokePSArray[id].Play();
            }
            else
                _tireSmokePSArray[id].Stop();
        }
    }
}
