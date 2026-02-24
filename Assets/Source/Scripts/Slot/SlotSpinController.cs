using System.Collections.Generic;
using AxGrid;
using AxGrid.Base;
using AxGrid.Model;
using DG.Tweening;
using UnityEngine;

namespace SlotMachine
{
    public class SlotSpinController : MonoBehaviourExtBind
    {
        [Header("Reel Setup")]
        [SerializeField] private RectTransform maskRect;  
        [SerializeField] private RectTransform container; 
        [SerializeField] private GameObject itemPrefab;   
        [SerializeField] private float itemHeight = 200f;
        [SerializeField] private List<Sprite> symbols;

        [Header("Speed Settings")]
        [SerializeField] private float maxSpeed = 1000f;
        [SerializeField] private float accelerationTime = 1f;
        [SerializeField] private float decelerationTime = 2f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem spinParticles;
        [SerializeField] private ParticleSystem stopParticles;

        private enum Phase { Idle, Accelerating, FullSpeed, Decelerating, Snapping }
        private Phase _phase = Phase.Idle;

        private float _currentSpeed = 0f;
        private float _phaseTimer = 0f;
        private float _scrollOffset = 0f;

        private Tweener _snapTween;
        private List<SlotItem> _items = new List<SlotItem>();
        private int _totalCount;     // видимые + 2 буферных (сверху и снизу)

        [OnStart]
        private void StartThis()
        {
            if (itemPrefab == null) { Log.Debug("SlotSpinController: itemPrefab не назначен!"); return; }
            if (container == null) { Log.Debug("SlotSpinController: container не назначен!"); return; }
            if (symbols == null || symbols.Count == 0) { Log.Debug("SlotSpinController: symbols пустой!"); return; }
            SpawnItems();
        }

        private void SpawnItems()
        {
            foreach (var item in _items)
                if (item != null) Destroy(item.gameObject);
            _items.Clear();

            float maskHeight = maskRect != null ? maskRect.rect.height : Screen.height;
            _totalCount = Mathf.CeilToInt(maskHeight / itemHeight) + 2;

            for (int i = 0; i < _totalCount; i++)
            {
                var go = Instantiate(itemPrefab, container);
                var item = go.GetComponent<SlotItem>();
                _items.Add(item);
                // Позиции строго кратны itemHeight, центр = 0
                float yPos = ((_totalCount / 2) - i) * itemHeight;
                item.SetPosition(yPos);
                item.SetSymbol(symbols[Random.Range(0, symbols.Count)]);
            }

            // Начальный offset = 0 — гарантирует что scrollOffset всегда кратен itemHeight в точках выравнивания
            _scrollOffset = 0f;
        }

        // ── FSM события ────────────────────────────────────────────────────

        [Bind("OnSlotIdle")]
        private void OnIdle()
        {
            _phase = Phase.Idle;
            _currentSpeed = 0f;
            if (spinParticles != null) spinParticles.Stop();
        }

        [Bind("OnSlotAccelerating")]
        private void OnAccelerating()
        {
            _phase = Phase.Accelerating;
            _phaseTimer = 0f;
            if (spinParticles != null) spinParticles.Play();
        }

        [Bind("OnSlotFullSpeed")]
        private void OnFullSpeed()
        {
            _phase = Phase.FullSpeed;
            _currentSpeed = maxSpeed;
        }

        [Bind("OnSlotDecelerating")]
        private void OnDecelerating()
        {
            _phase = Phase.Decelerating;
            _phaseTimer = 0f;
        }

        // ── Update ─────────────────────────────────────────────────────────

        [OnUpdate]
        private void OnUpdate()
        {
            switch (_phase)
            {
                case Phase.Accelerating:
                    _phaseTimer += Time.deltaTime;
                    float t = Mathf.Clamp01(_phaseTimer / accelerationTime);
                    _currentSpeed = maxSpeed * (t * t * t);
                    ApplyScroll(_currentSpeed * Time.deltaTime);
                    break;

                case Phase.FullSpeed:
                    ApplyScroll(_currentSpeed * Time.deltaTime);
                    break;

                case Phase.Decelerating:
                    _phaseTimer += Time.deltaTime;
                    StartSnap();
                    break;
            }
        }

        // ── DOTween snap ────────────────────────────────────────────────────

        private void StartSnap()
        {
            _phase = Phase.Snapping;

            float extraDist = 2f * maxSpeed / decelerationTime;
            float rawTarget = _scrollOffset + extraDist;
            float remainder = rawTarget % itemHeight;
            float targetOffset = remainder < 0.01f ? rawTarget : rawTarget + (itemHeight - remainder);

            float snapDist = targetOffset - _scrollOffset;

            Log.Debug($"Snap: from={_scrollOffset:F1} to={targetOffset:F1} dist={snapDist:F1}");

            _snapTween?.Kill();
            _snapTween = DOTween.To(
                () => _scrollOffset,
                x =>
                {
                    float delta = x - _scrollOffset;
                    if (delta > 0f) ApplyScroll(delta);
                },
                targetOffset,
                decelerationTime
            )
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _phase = Phase.Idle;
                _currentSpeed = 0f;
                if (spinParticles != null) spinParticles.Stop();
                if (stopParticles != null) stopParticles.Play();

                // Находим элемент ближайший к центру (y == 0)
                SlotItem selected = null;
                float minDist = float.MaxValue;
                foreach (var item in _items)
                {
                    float dist = Mathf.Abs(item.GetY());
                    if (dist < minDist)
                    {
                        minDist = dist;
                        selected = item;
                    }
                }
                selected?.OnSelected();

                Log.Debug($"Aligned! offset={_scrollOffset:F1}");
                Settings.Fsm.Invoke("OnSpinAligned");
            });
        }

        [OnDestroy]
        private void DestroyThis()
        {
            _snapTween?.Kill();
        }

        // ── Скролл ─────────────────────────────────────────────────────────

        private void ApplyScroll(float delta)
        {
            if (delta <= 0f) return;
            _scrollOffset += delta;

            float totalHeight = _totalCount * itemHeight;

            for (int i = 0; i < _items.Count; i++)
            {
                // Целочисленный центр — позиции всегда кратны itemHeight
                float baseY = ((_totalCount / 2) - i) * itemHeight;
                float y = baseY - (_scrollOffset % totalHeight);

                if (y < -((_totalCount / 2 + 1) * itemHeight))
                    y += totalHeight;

                _items[i].SetPosition(y);

                float topBound = (_totalCount / 2 + 1) * itemHeight;
                if (y > topBound - itemHeight * 0.5f && y < topBound + itemHeight * 0.5f)
                    _items[i].SetSymbol(symbols[Random.Range(0, symbols.Count)]);
            }
        }
    }
}