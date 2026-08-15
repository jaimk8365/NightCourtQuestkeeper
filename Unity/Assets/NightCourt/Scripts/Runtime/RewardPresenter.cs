using System.Collections;
using NightCourt.Core;
using UnityEngine;

namespace NightCourt.Runtime
{
    public sealed class RewardPresenter : MonoBehaviour
    {
        public static RewardPresenter Instance { get; private set; }
        [SerializeField] private ParticleSystem sparkles;
        [SerializeField] private AudioSource rewardSound;
        private void Awake() => Instance = this;

        public void Celebrate(Reward reward, bool levelUp)
        {
            if (sparkles != null) sparkles.Play();
            if (rewardSound != null) rewardSound.Play();
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
            Debug.Log(levelUp ? $"Level up! +{reward.Xp} XP" : $"Tiny win! +{reward.Xp} XP");
            StartCoroutine(StopAfter(levelUp ? 1.4f : 0.65f));
        }

        private IEnumerator StopAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if (sparkles != null) sparkles.Stop();
        }
    }
}
