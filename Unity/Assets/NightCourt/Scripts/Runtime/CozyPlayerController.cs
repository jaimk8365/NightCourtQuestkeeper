using UnityEngine;
using UnityEngine.EventSystems;

namespace NightCourt.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CozyPlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 4.2f;
        [SerializeField] private float turnSpeed = 12f;
        private CharacterController controller;
        private Camera mainCamera;
        private Vector3 tapTarget;
        private bool hasTapTarget;

        private void Awake() { controller = GetComponent<CharacterController>(); mainCamera = Camera.main; tapTarget = transform.position; }
        private void Update()
        {
            Vector2 keys = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector3 movement = new Vector3(keys.x, 0f, keys.y);
            if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
                { tapTarget = new Vector3(hit.point.x, transform.position.y, hit.point.z); hasTapTarget = true; }
            }
            if (movement.sqrMagnitude > .01f) hasTapTarget = false;
            else if (hasTapTarget)
            {
                Vector3 delta = tapTarget - transform.position; delta.y = 0f;
                if (delta.magnitude < .18f) hasTapTarget = false; else movement = delta.normalized;
            }
            if (movement.sqrMagnitude > 1f) movement.Normalize();
            if (movement.sqrMagnitude > .01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), turnSpeed * Time.deltaTime);
                controller.SimpleMove(movement * speed);
                Transform body = transform.Find("Body");
                if (body != null) body.localPosition = new Vector3(0f, .72f + Mathf.Sin(Time.time * 11f) * .025f, 0f);
            }
        }
    }
}
