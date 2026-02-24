using AxGrid;
using AxGrid.Base;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine
{
    public class SlotItem : MonoBehaviourExt
    {
        [SerializeField] private Image symbolImage;
        [SerializeField] private RectTransform rectTransform;

        public Sprite CurrentSymbol { get; private set; }

        private void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
            symbolImage = GetComponentInChildren<Image>();
        }

        public void SetPosition(float yPos)
        {
            rectTransform.anchoredPosition = new Vector2(0f, yPos);
        }

        public float GetY() => rectTransform.anchoredPosition.y;

        public void SetSymbol(Sprite sprite)
        {
            CurrentSymbol = sprite;
            symbolImage.sprite = sprite;
            symbolImage.color = Color.white;
        }

        public void OnSelected()
        {
            Log.Debug($"SlotItem selected: {CurrentSymbol?.name}");
            // TODO: сигнализировать результат наружу
        }
    }
}