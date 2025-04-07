using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ReusableScrollView
{
    public class ReusableScrollView<TData, TCell> : MonoBehaviour
        where TCell : MonoBehaviour, IReusableCell<TData>
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private TCell cellPrefab;
        private readonly int _buffer = 2;

        private float _cellHeight;
        private List<TData> _dataList = new();

        private readonly List<TCell> _pool = new();
        private int _visibleCount;

        private void Start()
        {
            _cellHeight = ((RectTransform)cellPrefab.transform).rect.height;
            _visibleCount = Mathf.CeilToInt(scrollRect.viewport.rect.height / _cellHeight) + _buffer;

            for (var i = 0; i < _visibleCount; i++)
            {
                var cell = Instantiate(cellPrefab, content);
                _pool.Add(cell);
            }

            scrollRect.onValueChanged.AddListener(_ => RefreshCells());
        }

        public void SetData(List<TData> data)
        {
            _dataList = data;
            content.sizeDelta = new Vector2(0, _cellHeight * _dataList.Count);
            RefreshCells();
        }

        private void RefreshCells()
        {
            var scrollY = content.anchoredPosition.y;
            var firstIndex = Mathf.FloorToInt(scrollY / _cellHeight);

            for (var i = 0; i < _pool.Count; i++)
            {
                var dataIndex = firstIndex + i;
                if (dataIndex < 0 || dataIndex >= _dataList.Count)
                {
                    _pool[i].gameObject.SetActive(false);
                }
                else
                {
                    _pool[i].Setup(_dataList[dataIndex], dataIndex);
                    _pool[i].transform.localPosition = new Vector3(0, -dataIndex * _cellHeight, 0);
                    _pool[i].gameObject.SetActive(true);
                }
            }
        }
    }
}