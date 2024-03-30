using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Assets.Scripts;

namespace Scripts.Game {

    [RequireComponent(typeof(Image))]
    public class Orb : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField]
        private Image img;

        private Vector2Int currentPosition;
        public Vector2Int Position {
            set { currentPosition = value; }
        }

        private OrbType type;
        private Func<Vector2Int, Vector2Int, IEnumerator> swapCallback;

        [SerializeField]
        private Vector2 minDragDistance = new Vector2(0.1f, 0.1f);

        private Vector2Int DragVecToDragDir(Vector2 dragVec) {
            float pX = Mathf.Abs(dragVec.x);
            float pY = Mathf.Abs(dragVec.y);
            Vector2Int draggedDir;
            
            if (pX > pY) {
                draggedDir = (dragVec.x > 0) ? Vector2Int.right : Vector2Int.left;
            } else {
                draggedDir = (dragVec.y > 0) ? Vector2Int.up : Vector2Int.down;
            }

            return draggedDir;
        }

        public void Init(Vector2Int currentPos, OrbType orbType, Sprite sprite, Func<Vector2Int, Vector2Int, IEnumerator> swapClbk) {
            currentPosition = currentPos;
            type = orbType;
            img.sprite = sprite;
            swapCallback = swapClbk;
        }

        public void UpdateInfo(Vector2Int currentPos, OrbType orbType, Sprite sprite) {
            currentPosition = currentPos;
            type = orbType;
            img.sprite = sprite;
        }

        public IEnumerator Matched() {
            yield return Util.TweenScale(gameObject.transform, new Vector2(1.0f, 1.0f), new Vector2(0.0f, 0.0f), 0.25f);
        }

        public void OnPointerUp(PointerEventData eventData) {
            Vector3 dragVec = (eventData.position - eventData.pressPosition).normalized;
            if (Math.Abs(dragVec.x) < minDragDistance.x && Math.Abs(dragVec.y) < minDragDistance.y) {
                return;
            } 

            Vector2Int dragDir = DragVecToDragDir(dragVec);
            StartCoroutine(swapCallback(currentPosition, dragDir));
        }

        public void OnPointerDown(PointerEventData eventData) { }

        public void OnPointerEnter(PointerEventData eventData) {
            Color c = img.color;
            c.a = 0.75f;
            img.color = c;
        }

        public void OnPointerExit(PointerEventData eventData) {
            Color c = img.color;
            c.a = 1.0f;
            img.color = c;
        }
    }
}
