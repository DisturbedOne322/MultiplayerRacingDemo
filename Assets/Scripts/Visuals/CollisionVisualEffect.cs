using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class ContinuousCollisionEffect : NetworkBehaviour
{
    [SerializeField]
    private VisualEffectAsset _continuousSparksVFXAsset;

    [SerializeField]
    private Light _collisionLight;

    private Light _leftCollisionLight;
    private Light _rightCollisionLight;

    private VisualEffect _leftSideSparks;
    private VisualEffect _rightSideSparks;

    [SerializeField]
    private CollisionEffectHandler _collisionHandler;

    private void Start()
    {
        if (!IsOwner)
            return;
        Initialize();
    }

    private void Initialize()
    {
        InitializeLeftContSparks();
        InitializeRightContSparks();

        _collisionHandler.OnRightCollisionStay += _collisionHandler_OnRightCollisionStay;
        _collisionHandler.OnLeftCollisionStay += _collisionHandler_OnLeftCollisionStay;

        _collisionHandler.OnRightCollisionExit += _collisionHandler_OnRightCollisionExit; 
        _collisionHandler.OnLeftCollisionExit += _collisionHandler_OnLeftCollisionExit;
    }

    private void OnDestroy()
    {
        _collisionHandler.OnRightCollisionStay -= _collisionHandler_OnRightCollisionStay;
        _collisionHandler.OnLeftCollisionStay -= _collisionHandler_OnLeftCollisionStay;

        _collisionHandler.OnRightCollisionExit -= _collisionHandler_OnRightCollisionExit;
        _collisionHandler.OnLeftCollisionExit -= _collisionHandler_OnLeftCollisionExit;
    }

    private void _collisionHandler_OnLeftCollisionExit()
    {
        _leftCollisionLight.gameObject.SetActive(false);
        _leftSideSparks.Stop();
    }

    private void _collisionHandler_OnRightCollisionExit()
    {
        _rightCollisionLight.gameObject.SetActive(false);
        _rightSideSparks.Stop();
    }

    private void _collisionHandler_OnLeftCollisionStay(Vector3 pos, float speed)
    {
        _leftSideSparks.SetVector3("position", pos);
        _leftSideSparks.SetFloat("spawnAmount", speed * 2);
        _leftSideSparks.Play();

        _leftCollisionLight.gameObject.SetActive(true);
        _leftCollisionLight.transform.position = pos;
    }

    private void _collisionHandler_OnRightCollisionStay(Vector3 pos, float speed)
    {
        _rightSideSparks.SetVector3("position", pos);
        _rightSideSparks.SetFloat("spawnAmount", speed * 2);
        _rightSideSparks.Play();

        _rightCollisionLight.gameObject.SetActive(true);
        _rightCollisionLight.transform.position = pos;
    }

    private void InitializeLeftContSparks()
    {
        GameObject leftContSparks = new GameObject("LeftContSparks");
        _leftCollisionLight = Instantiate(_collisionLight);
        _leftCollisionLight.transform.parent = leftContSparks.transform;
        leftContSparks.transform.parent = this.transform;

        leftContSparks.transform.localPosition = Vector3.zero;
        leftContSparks.transform.localRotation = Quaternion.Euler(Vector3.zero);

        leftContSparks.transform.localScale = new Vector3(-1,1,1);

        _leftSideSparks = leftContSparks.AddComponent<VisualEffect>();    
        _leftSideSparks.visualEffectAsset = _continuousSparksVFXAsset;

        _leftSideSparks.Stop();
    }

    private void InitializeRightContSparks()
    {
        GameObject rightContSparks = new GameObject("RightContSparks");
        _rightCollisionLight = Instantiate(_collisionLight);
        _rightCollisionLight.transform.parent = rightContSparks.transform;
        rightContSparks.transform.parent = this.transform;

        rightContSparks.transform.localPosition = Vector3.zero;
        rightContSparks.transform.localRotation = Quaternion.Euler(Vector3.zero);

        _rightSideSparks = rightContSparks.AddComponent<VisualEffect>();
        _rightSideSparks.visualEffectAsset = _continuousSparksVFXAsset;

        _rightSideSparks.Stop();
    }
}
