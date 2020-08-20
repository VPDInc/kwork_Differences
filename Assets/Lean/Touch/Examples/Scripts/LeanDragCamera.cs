using DG.Tweening;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Lean.Touch {
    /// <summary>This component allows you to move the current GameObject (e.g. Camera) based on finger drags and the specified ScreenDepth.</summary>
    [HelpURL(LeanTouch.HelpUrlPrefix + "LeanDragCamera")]
    [AddComponentMenu(LeanTouch.ComponentPathPrefix + "Drag Camera")]
    public class LeanDragCamera : MonoBehaviour {
        /// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
        public LeanFingerFilter Use = new LeanFingerFilter(true);

        /// <summary>The method used to find world coordinates from a finger. See LeanScreenDepth documentation for more information.</summary>
        public LeanScreenDepth ScreenDepth = new LeanScreenDepth(LeanScreenDepth.ConversionType.DepthIntercept);

        /// <summary>The movement speed will be multiplied by this.
        /// -1 = Inverted Controls.</summary>
        [Tooltip("The movement speed will be multiplied by this.\n\n-1 = Inverted Controls.")]
        public float Sensitivity = 1.0f;

        /// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
        /// -1 = Instantly change.
        /// 1 = Slowly change.
        /// 10 = Quickly change.</summary>
        [Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
        public float Dampening = -1.0f;

        /// <summary>This allows you to control how much momenum is retained when the dragging fingers are all released.
        /// NOTE: This requires <b>Dampening</b> to be above 0.</summary>
        [Tooltip("This allows you to control how much momenum is retained when the dragging fingers are all released.\n\nNOTE: This requires <b>Dampening</b> to be above 0.")]
        [Range(0.0f, 1.0f)]
        public float Inertia;

        [HideInInspector] [SerializeField] private Vector3 remainingDelta;

        [SerializeField] bool _useBounds = false;
        [SerializeField] Vector2 _minBounds = default;
        [SerializeField] Vector2 _maxBounds = default;
        [SerializeField] Vector2 _minSoftBounds = default;
        [SerializeField] Vector2 _maxSoftBounds = default;
        [SerializeField] Vector2 _softBoundsOffset = default;
        [SerializeField] bool _x = true;
        [SerializeField] bool _y = true;

        bool _isForcedMoving;

        /// <summary>This method moves the current GameObject to the center point of all selected objects.</summary>
        [ContextMenu("Move To Selection")]
        public virtual void MoveToSelection() {
            var center = default(Vector3);
            var count = 0;

            foreach (var selectable in LeanSelectable.Instances) {
                if (selectable.IsSelected == true) {
                    center += selectable.transform.position;
                    count += 1;
                }
            }

            if (count > 0) {
                var oldPosition = transform.localPosition;

                transform.position = center / count;

                remainingDelta += transform.localPosition - oldPosition;

                transform.localPosition = oldPosition;
            }
        }

        public void SetMaxBounds(Vector2 bounds) {
            _maxSoftBounds = bounds;
            _maxBounds = bounds + _softBoundsOffset;
        }

        public void SetMinBounds(Vector2 bounds) {
            // _minBounds = bounds;
            // _minSoftBounds = bounds + _softBoundsOffset;
            _minSoftBounds = bounds;
            _minBounds = bounds - _softBoundsOffset;
        }

        /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
        public void AddFinger(LeanFinger finger) {
            Use.AddFinger(finger);
        }

        /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
        public void RemoveFinger(LeanFinger finger) {
            Use.RemoveFinger(finger);
        }

        /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove all fingers.</summary>
        public void RemoveAllFingers() {
            Use.RemoveAllFingers();
        }

        public void MoveTo(Vector3 pos, bool isInstant) {
            transform.DOKill();
            _isForcedMoving = true;
            pos.x = Mathf.Clamp(pos.x, _minSoftBounds.x, _maxSoftBounds.x);
            pos.y = Mathf.Clamp(pos.y, _minSoftBounds.y, _maxSoftBounds.y);
            pos.z = transform.localPosition.z;
            transform.DOLocalMove(pos, isInstant ? 0 : 1f).OnComplete(()=>_isForcedMoving = false);
        }
        #if UNITY_EDITOR
        protected virtual void Reset() {
            Use.UpdateRequiredSelectable(gameObject);
        }
        #endif
        protected virtual void Awake() {
            Use.UpdateRequiredSelectable(gameObject);
        }

        protected virtual void LateUpdate() {
            if(_isForcedMoving || EventSystem.current.IsPointerOverGameObject()) return;
            
            // Get the fingers we want to use
            var fingers = Use.GetFingers();

            // Get the last and current screen point of all fingers
            var lastScreenPoint = LeanGesture.GetLastScreenCenter(fingers);
            var screenPoint = LeanGesture.GetScreenCenter(fingers);

            // Get the world delta of them after conversion
            var worldDelta = ScreenDepth.ConvertDelta(lastScreenPoint, screenPoint, gameObject);
            var isOuterSoftBounds = transform.localPosition.x < _minSoftBounds.x ||
                                    transform.localPosition.x > _maxSoftBounds.x;

            if (!_x)
                worldDelta.x = 0;

            if (!_y)
                worldDelta.y = 0;

            // Store the current position
            var oldPosition = transform.localPosition;

            // Pan the camera based on the world delta
            transform.position -= worldDelta * Sensitivity;
            
            if (isOuterSoftBounds)
                remainingDelta = Vector3.zero;

            // Add to remainingDelta
            remainingDelta += transform.localPosition - oldPosition;

            // Get t value
            var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);

            // Dampen remainingDelta
            var newRemainingDelta = Vector3.Lerp(remainingDelta, Vector3.zero, factor);

            var newPosition = oldPosition + remainingDelta - newRemainingDelta;
            if (_useBounds) {
                newPosition.x = Mathf.Clamp(newPosition.x, _minBounds.x, _maxBounds.x);
                newPosition.y = Mathf.Clamp(newPosition.y, _minBounds.y, _maxBounds.y);
            }

            // Shift this position by the change in delta
            transform.localPosition = newPosition;
            if (isOuterSoftBounds) {
                if (transform.localPosition.x < _minSoftBounds.x && fingers.Count == 0) {
                    var localPos = transform.localPosition;
                    localPos.x = Mathf.Lerp(localPos.x, _minSoftBounds.x, 5 * Time.deltaTime);
                    transform.localPosition = localPos;
                }

                if (transform.localPosition.x > _maxSoftBounds.x && fingers.Count == 0) {
                    var localPos = transform.localPosition;
                    localPos.x = Mathf.Lerp(localPos.x, _maxSoftBounds.x, 5 * Time.deltaTime);
                    transform.localPosition = localPos;
                }
            } else if (fingers.Count == 0 && Inertia > 0.0f && Dampening > 0.0f) {
                newRemainingDelta = Vector3.Lerp(newRemainingDelta, remainingDelta, Inertia);
            }

            // Update remainingDelta with the dampened value
            remainingDelta = newRemainingDelta;
        }
    }
}