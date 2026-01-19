using Fire_Pixel.Utility;
using UnityEngine;


public class PlatformMover : MonoBehaviour
{
    [SerializeField] private Vector3[] positions;
    [SerializeField] private float speed;
    [SerializeField] private float delay;

    private int posId;


    private void OnEnable() => UpdateScheduler.RegisterFixedUpdate(OnFixedUpdate);
    private void OnDestroy() => UpdateScheduler.UnRegisterFixedUpdate(OnFixedUpdate);

    private void OnFixedUpdate()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, positions[posId], speed * Time.fixedDeltaTime);

        if (transform.localPosition == positions[posId])
        {
            posId.IncrementSmart(positions.Length);
            PauseDelay();
        }
    }

    private void PauseDelay()
    {
        UpdateScheduler.UnRegisterFixedUpdate(OnFixedUpdate);
        Invoke(nameof(Play), delay);
    }
    private void Play() => UpdateScheduler.RegisterFixedUpdate(OnFixedUpdate);
}
