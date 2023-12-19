using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Tire Smoke Effect")]
    public class CarVisualsTireSmoke : MonoBehaviour
    {
        public VisualEffectAssetType.Type _tireSmokeVisualEffectType;

        #region VFX
        [SerializeField]
        private VisualEffectAsset _tireSmokeVFX;
        private VisualEffect[] _tireSmokeVFXArray;
        #endregion

        #region ParticleSystem
        [SerializeField]
        private ParticleSystem _tireSmokeParticleSystem;
        private ParticleSystem[] _tireSmokePSArray;
        #endregion

        public void InstantiateSmoke(Transform[] wheelMeshes)
        {
            if(_tireSmokeVisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                TryInstantiateVFX(wheelMeshes);
            else
                TryInstantiatePS(wheelMeshes);
        }

        private void TryInstantiateVFX(Transform[] wheelMeshes)
        {
            if (_tireSmokeVFX == null)
            {
                Debug.LogWarning("You have Tire Smoke Effect, but Visual Effect Asset is not assigned");
                return;
            }
            _tireSmokeVFXArray = new VisualEffect[wheelMeshes.Length];

            int size = wheelMeshes.Length;
            for (int i = 0; i < size; i++)
            {
                _tireSmokeVFXArray[i] = wheelMeshes[i].gameObject.AddComponent<VisualEffect>();
                _tireSmokeVFXArray[i].visualEffectAsset = _tireSmokeVFX;
                _tireSmokeVFXArray[i].Stop();
            }
        }

        private void TryInstantiatePS(Transform[] wheelMeshes)
        {
            if (_tireSmokeParticleSystem == null)
            {
                Debug.LogWarning("You have Tire Smoke Effect, but Particle System is not assigned");
                return;
            }
            _tireSmokePSArray = new ParticleSystem[wheelMeshes.Length];

            int size = wheelMeshes.Length;
            for (int i = 0; i < size; i++)
            {
                _tireSmokePSArray[i] = Instantiate(_tireSmokeParticleSystem);
                _tireSmokePSArray[i].transform.parent = wheelMeshes[i].parent;
                _tireSmokePSArray[i].Stop();
            }
        }

        public void DisplaySmokeVFX(bool display, int id, Vector3 position, Vector3 rbVelocityNorm, float speed)
        {
            if(_tireSmokeVisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                DisplayVFX(display, id, position, rbVelocityNorm, speed);
            else
                DisplayPS(display, id, position, rbVelocityNorm, speed);         
        }
        private void DisplayVFX(bool display, int id, Vector3 position, Vector3 rbVelocityNorm, float speed)
        {
            if (_tireSmokeVFX == null)
                return;

            if (display)
            {
                _tireSmokeVFXArray[id].Play();
                _tireSmokeVFXArray[id].SetVector3("position", position);
                if (speed < 1)
                {
                    _tireSmokeVFXArray[id].SetVector3("velocity", -transform.forward);
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
        private void DisplayPS(bool display, int id, Vector3 position, Vector3 rbVelocityNorm, float speed)
        {
            if (_tireSmokeParticleSystem == null)
                return;
            if (display)
            {
                if(Mathf.Abs(speed) < 1)
                    rbVelocityNorm = transform.forward;

                _tireSmokePSArray[id].transform.position = position;
                _tireSmokePSArray[id].transform.forward = -rbVelocityNorm;
                _tireSmokePSArray[id].Play();
            }
            else
                _tireSmokePSArray[id].Stop();
        }
    }
}
