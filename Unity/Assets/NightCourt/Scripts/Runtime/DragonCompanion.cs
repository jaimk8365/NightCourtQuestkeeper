using UnityEngine;
namespace NightCourt.Runtime
{
    public sealed class DragonCompanion : MonoBehaviour
    {
        [SerializeField] private Transform player;
        private Vector3 velocity;
        public void SetPlayer(Transform value) => player = value;
        private void LateUpdate()
        {
            if (player == null) return;
            Vector3 wanted = player.position - player.right * 1.05f - player.forward * .65f + Vector3.up * (1.15f + Mathf.Sin(Time.time * 3f) * .12f);
            transform.position = Vector3.SmoothDamp(transform.position, wanted, ref velocity, .28f);
            transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, Time.deltaTime * 5f);
        }
    }
}
