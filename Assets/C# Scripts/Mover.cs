using Fire_Pixel.Utility;
using UnityEngine;


public class Mover : MonoBehaviour
{
    [SerializeField] private Vector3[] positions;
    [SerializeField] private float speed;
    [SerializeField] private float delay;

    private int posId;


    private void OnEnable() => UpdateScheduler.RegisterFixedUpdate(OnFixedUpdate);
    private void OnDisable() => UpdateScheduler.UnRegisterFixedUpdate(OnFixedUpdate);

    private void OnFixedUpdate()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, positions[posId], speed * Time.fixedDeltaTime);

        if (transform.localPosition == positions[posId])
        {
            posId += 1;
            if (posId == positions.Length)
            {
                posId = 0;
            }
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
