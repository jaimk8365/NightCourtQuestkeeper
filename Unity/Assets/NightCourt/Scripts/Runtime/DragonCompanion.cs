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
            if(GameBootstrap.Instance!=null&&GameBootstrap.Instance.Save.Companions.Count>0){int stage=GameBootstrap.Instance.Save.Companions[0].EvolutionStage;float size=stage==3?1.45f:stage==2?1.22f:1f;transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*size,Time.deltaTime*3f);}
        }
    }
}
