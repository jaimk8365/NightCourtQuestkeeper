using UnityEngine;
namespace NightCourt.Runtime
{
    public sealed class CozyCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 9f, -10f);
        public void SetTarget(Transform value) => target = value;
        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.Lerp(transform.position, target.position + offset, 1f - Mathf.Exp(-7f * Time.deltaTime));
            transform.LookAt(target.position + Vector3.up * .8f);
        }
    }
}
