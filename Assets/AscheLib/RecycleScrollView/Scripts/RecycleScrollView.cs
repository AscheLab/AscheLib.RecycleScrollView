using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AscheLib.UI {
	[RequireComponent(typeof(RectTransform))]
	public class RecycleScrollView : MonoBehaviour {
		#region ScrollViewSettings
		[SerializeField]
		ScrollType _scrollType;

		[SerializeField]
		ScrollRect.MovementType _movementType;

		[SerializeField]
		float _elasticity = 0.1f;

		[SerializeField]
		bool _inertia = true;

		[SerializeField]
		float _decelerationRate = 0.135f;

		[SerializeField]
		float _scrollSensitivity = 1.0f;

		[SerializeField]
		Scrollbar _scrollbar;

		[SerializeField]
		ScrollRect.ScrollbarVisibility _scrollbarVisibility;

		[SerializeField]
		ScrollRect.ScrollRectEvent _onValueChanged = new ScrollRect.ScrollRectEvent ();

		[SerializeField]
		bool _showMaskGraphic = false;

		[SerializeField]
		Sprite _maskImage = null;
		#endregion

		#region GridSetting
		[SerializeField]
		GameObject _cellPrefab;

		[SerializeField]
		Vector2 _cellSize = new Vector2(100, 30);

		[SerializeField]
		int _columnLimit;

		[SerializeField]
		ArrangementType _arrangementType;
		#endregion

		DisposableBundle _currentDisposable = null;

		public IDisposable SetDatas<T>(IEnumerable<T> datas) {
			// Dispose before scroll
			if(_currentDisposable != null) {
				_currentDisposable.Dispose();
				_currentDisposable = null;
			};
			_currentDisposable = new DisposableBundle();

			if(_cellPrefab == null) {
				Debug.LogError("Prefab not found.");
				return _currentDisposable;
			}
			if (_cellPrefab.GetComponent<IScrollableCell<T>>() == null) {
				Debug.LogError("No corresponding type of component is does not exist to this Prefab.");
				return _currentDisposable;
			}

			int mainPivotVectorValue;
			int subPivotVectorValue;
			float baseCellPosition;

			switch (_scrollType) {
			case ScrollType.Horizontal:
				mainPivotVectorValue = _arrangementType == ArrangementType.LeftDownToRightUp || _arrangementType == ArrangementType.LeftUpToRightDown ? 1 : -1;
				subPivotVectorValue = _arrangementType == ArrangementType.LeftDownToRightUp || _arrangementType == ArrangementType.RightDownToLeftUp ? 1 : -1;
				break;
			case ScrollType.Vertical:
				mainPivotVectorValue = _arrangementType == ArrangementType.LeftDownToRightUp || _arrangementType == ArrangementType.RightDownToLeftUp ? 1 : -1;
				subPivotVectorValue = _arrangementType == ArrangementType.LeftDownToRightUp || _arrangementType == ArrangementType.LeftUpToRightDown ? 1 : -1;
				break;
			default:
				throw new Exception(string.Format("ScrollType <{0}> does not exist.", _scrollType));
			}
			baseCellPosition = GetScrollTypeValue(_scrollType, _cellSize) * mainPivotVectorValue;

			List<T> dataList = datas.ToList();

			// Generate Scroll items
			IDisposable cellDisposable;
			var cellRoot = GenerateScrollContent(dataList, mainPivotVectorValue);
			var cellList = GenerateCellList(_cellPrefab, dataList, cellRoot, out cellDisposable);
			_currentDisposable.Add(cellDisposable);

			// Move to init position
			cellList.ForEach(cell => UpdateCellPosition(cell, cell.GetValue(), dataList, mainPivotVectorValue, subPivotVectorValue, baseCellPosition));

			// Start scroll coroutine
			var coroutine = StartCoroutine(UpdateCells(cellRoot, dataList, cellList, _scrollType, mainPivotVectorValue, subPivotVectorValue, baseCellPosition));
			_currentDisposable.Add(new CoroutineDisposable(coroutine, this));

			return _currentDisposable;
		}

		#region GenerateItems
		RectTransform GenerateScrollContent<T>(List<T> dataList, int mainPivotVectorValue) {
			if( _currentDisposable == null ) {
				_currentDisposable = new DisposableBundle();
			}
			var rootTransform = GetComponent<RectTransform>();
			var scrollRect = CreateScrollRect(rootTransform);
			var scrollRectTransform = scrollRect.GetComponent<RectTransform>();
			var viewport = CreateViewport(scrollRectTransform);
			var content = CreateContent(viewport, _scrollType, _arrangementType, dataList, mainPivotVectorValue, _columnLimit);
			scrollRect.content = content;
			scrollRect.viewport = viewport;
			SetScrollbar(scrollRect);

			_currentDisposable.Add(new GameObjectDisposable(content.gameObject));
			_currentDisposable.Add(new GameObjectDisposable(viewport.gameObject));
			_currentDisposable.Add(new GameObjectDisposable(scrollRect.gameObject));

			return content;
		}
		List<IScrollableCell<T>> GenerateCellList<T>(GameObject cellPrefab, List<T> dataList, Transform cellRoot, out IDisposable cellDisposable) {
			DisposableBundle disposable = new DisposableBundle();
			var rootTransform = GetComponent<RectTransform>();
			int cellNum = 0;
			switch(_scrollType) {
			case ScrollType.Horizontal:
				cellNum = (Mathf.CeilToInt(rootTransform.rect.width / _cellSize.x) + 1) * _columnLimit;
				break;
			case ScrollType.Vertical:
				cellNum = (Mathf.CeilToInt(rootTransform.rect.height / _cellSize.y) + 1) * _columnLimit;
				break;
			}
			
			List<IScrollableCell<T>> result = new List<IScrollableCell<T>>();
			for( int i = 0; i < cellNum; i++ ) {
				if( i >= dataList.Count ) {
					continue;
				}
				var cellObject = GameObject.Instantiate(cellPrefab);
				IScrollableCell<T> cell = cellObject.GetComponent<IScrollableCell<T>>();
				cell.SetValue(dataList[i]);
				Transform cellTransform = cell.transform;
				cellTransform.SetParent(cellRoot);
				cellTransform.localScale = Vector3.one;
				cellTransform.rotation = cellRoot.rotation;
				result.Add(cell);
				disposable.Add(new GameObjectDisposable(cellObject));
			}
			cellDisposable = disposable;
			return result;
		}

		RectTransform CreateViewport (RectTransform viewportRoot) {
			var viewportObject = new GameObject("Viewport");
			var result = viewportObject.AddComponent<RectTransform>();
			var mask = viewportObject.AddComponent<Mask>();
			var maskImage = viewportObject.AddComponent<Image>();
			result.SetParent(viewportRoot);
			FitRectTransform(viewportRoot, result);
			mask.showMaskGraphic = _showMaskGraphic;
			if(_maskImage != null) {
				maskImage.sprite = _maskImage;
			}
			return result;
		}

		ScrollRect CreateScrollRect (RectTransform scrollViewRoot) {
			var scrollViewObject = new GameObject("ScrollView");
			var scrollViewRectTransform = scrollViewObject.AddComponent<RectTransform>();
			var result = scrollViewObject.AddComponent<ScrollRect>();
			scrollViewRectTransform.SetParent(scrollViewRoot);
			FitRectTransform(scrollViewRoot, scrollViewRectTransform);
			result.vertical = _scrollType == ScrollType.Vertical;
			result.horizontal = _scrollType == ScrollType.Horizontal;
			result.movementType = _movementType;
			result.elasticity = _elasticity;
			result.inertia = _inertia;
			result.decelerationRate = _decelerationRate;
			result.movementType = _movementType;
			result.onValueChanged = _onValueChanged;

			return result;
		}

		void SetScrollbar (ScrollRect scrollRect) {
			switch (_scrollType) {
			case ScrollType.Horizontal:
				scrollRect.horizontalScrollbar = _scrollbar;
				scrollRect.horizontalScrollbarVisibility = _scrollbarVisibility;
				break;
			case ScrollType.Vertical:
				scrollRect.verticalScrollbar = _scrollbar;
				scrollRect.verticalScrollbarVisibility = _scrollbarVisibility;
				break;
			}
			scrollRect.scrollSensitivity = _scrollSensitivity;
		}

		RectTransform CreateContent<T>(RectTransform cellRoot, ScrollType scrollType, ArrangementType arrangementType, List<T> dataList, int mainPivotVectorValue, int columnLimit) {
			var widgetObject = new GameObject ("Content");
			var result = widgetObject.AddComponent<RectTransform>();
			result.SetParent(cellRoot);
			result.rotation = cellRoot.rotation;
			result.localScale = Vector3.one;
			result.localPosition = Vector3.zero;
			result.gameObject.layer = cellRoot.gameObject.layer;

			switch ( arrangementType ) {
			case ArrangementType.LeftUpToRightDown:
				result.pivot = new Vector2(0.0f, 1.0f);
				break;
			case ArrangementType.LeftDownToRightUp:
				result.pivot = new Vector2(0.0f, 0.0f);
				break;
			case ArrangementType.RightUpToLeftDown:
				result.pivot = new Vector2(1.0f, 1.0f);
				break;
			case ArrangementType.RightDownToLeftUp:
				result.pivot = new Vector2(1.0f, 0.0f);
				break;
			}

			Vector2 size = new Vector2();
			switch( scrollType ) {
			case ScrollType.Horizontal:
				size.x = Mathf.FloorToInt( _cellSize.x * Mathf.CeilToInt( ( float )dataList.Count / _columnLimit ) );
				size.y = Mathf.FloorToInt( _cellSize.y * _columnLimit );
				result.localPosition = new Vector2( cellRoot.rect.width * 0.5f * -mainPivotVectorValue, size.y * (result.pivot.y - 0.5f) );
				break;
			case ScrollType.Vertical:
				size.x = Mathf.FloorToInt( _cellSize.x * _columnLimit );
				size.y = Mathf.FloorToInt( _cellSize.y * Mathf.CeilToInt( ( float )dataList.Count / _columnLimit ) );
				result.localPosition = new Vector2( size.x * (result.pivot.x - 0.5f), cellRoot.rect.height * 0.5f * -mainPivotVectorValue );
				break;
			default:
				throw new Exception(string.Format("ScrollType <{0}> does not exist.", _scrollType));
			}

			result.sizeDelta = size;

			return result;
		}
		#endregion

		#region ScrollCore
		IEnumerator UpdateCells<T>(Transform cellRoot, List<T> dataList, List<IScrollableCell<T>> cellList, ScrollType scrollType, int mainPivotVectorValue, int subPivotVectorValue, float baseCellPosition) {
			// wait one frame
			yield return null;

			// keephold start position
			float startRootPositionArrangementValue = GetScrollTypeValue(scrollType, cellRoot.localPosition) * -mainPivotVectorValue;

			// current cellRoot position
			float cellRootPos = GetScrollTypeValue(scrollType, cellRoot.localPosition);

			// current display start dataList index
			int displayStartPos = 0;
			while( true ) {
				float tempCellRootPos = GetScrollTypeValue(scrollType, cellRoot.localPosition);
				if( cellRootPos != tempCellRootPos ) {
					cellRootPos = tempCellRootPos;
					// Update data index
					int tempDisplayStartPos = Mathf.FloorToInt( ( cellRootPos * -mainPivotVectorValue - startRootPositionArrangementValue ) / GetScrollTypeValue (scrollType, _cellSize)) * _columnLimit;
					if( displayStartPos != tempDisplayStartPos ) {
						displayStartPos = tempDisplayStartPos;
						UpdateStartIndex(displayStartPos, dataList, cellList, mainPivotVectorValue, subPivotVectorValue, baseCellPosition );
					}
				}
				yield return null;
			}
		}

		void UpdateStartIndex<T>(int newStartIndex, List<T> dataList, List<IScrollableCell<T>> cellList, int mainPivotVectorValue, int subPivotVectorValue, float baseCellPosition) {
			// pickup display data
			List<T> picupDataList = Enumerable.Range(newStartIndex, cellList.Count)
				.Where(dataIndex => dataIndex >= 0 && dataIndex < dataList.Count)
				.Select(dataIndex => dataList[dataIndex])
				.ToList();
			// current setting data list
			List<T> nowDataList = cellList.Select(cell => cell.GetValue()).ToList();
			// difference data list
			List<T> newDataList = picupDataList.Where( picupData => !nowDataList.Contains(picupData)).ToList();
			// setting difference data list
			int i = 0;
			foreach(var cell in cellList.Where(cell => !picupDataList.Contains(cell.GetValue()))) {
				if(newDataList.Count > i) {
					var info = newDataList[i];
					cell.SetValue(info);
					UpdateCellPosition(cell, info, dataList, mainPivotVectorValue, subPivotVectorValue, baseCellPosition);
				}
				i++;
			}
		}

		void UpdateCellPosition<T>(IScrollableCell<T> cell, T data, List<T> dataList, int mainPivotVectorValue, int subPivotVectorValue, float baseCellPosition) {
			int dataIndex = dataList.IndexOf(data);
			cell.transform.localPosition = 
				CreateScrollTypeVector (
					_scrollType,
					Mathf.CeilToInt(dataIndex / _columnLimit) * baseCellPosition + GetScrollTypeValue (_scrollType, _cellSize) * 0.5f * mainPivotVectorValue,
					dataIndex % _columnLimit * GetNonScrollTypeValue (_scrollType, _cellSize) * subPivotVectorValue + GetNonScrollTypeValue (_scrollType, _cellSize) * 0.5f * subPivotVectorValue);
		}
		#endregion

		#region InnerFunctions
		static void FitRectTransform (RectTransform root, RectTransform target) {
			target.rotation = root.rotation;
			target.localScale = Vector3.one;
			target.localPosition = Vector3.zero;
			target.gameObject.layer = root.gameObject.layer;
			target.pivot = root.pivot;
			target.anchorMax = root.anchorMax;
			target.anchorMin = root.anchorMin;
			target.anchoredPosition = root.anchoredPosition;
			target.offsetMax = root.offsetMax;
			target.offsetMin = root.offsetMin;
		}
		static Vector2 CreateScrollTypeVector(ScrollType scrollType, float arrangementValue, float nonArrangementValue) {
			switch(scrollType) {
			case ScrollType.Horizontal:
				return new Vector2(arrangementValue, nonArrangementValue);
			case ScrollType.Vertical:
				return new Vector2(nonArrangementValue, arrangementValue);
			}
			throw new Exception(string.Format("ScrollType <{0}> does not exist.", scrollType));
		}
		static float GetScrollTypeValue(ScrollType scrollType, Vector2 vector) {
			switch(scrollType) {
			case ScrollType.Horizontal:
				return vector.x;
			case ScrollType.Vertical:
				return vector.y;
			}
			throw new Exception(string.Format("ScrollType <{0}> does not exist.", scrollType));
		}
		static float GetNonScrollTypeValue(ScrollType scrollType, Vector2 vector) {
			switch(scrollType) {
			case ScrollType.Horizontal:
				return vector.y;
			case ScrollType.Vertical:
				return vector.x;
			}
			throw new Exception(string.Format("ScrollType <{0}> does not exist.", scrollType));
		}
		static List<IScrollableCell<T>> CreateCellList<T>(int cellNum, GameObject cellPrefab, List<T> dataList, Transform cellRoot) {
			List<IScrollableCell<T>> result = new List<IScrollableCell<T>>();
			for( int i = 0; i < cellNum; i++ ) {
				if( i >= dataList.Count ) {
					continue;
				}
				IScrollableCell<T> cell = GameObject.Instantiate( cellPrefab ).GetComponent<IScrollableCell<T>>();
				cell.SetValue( dataList[ i ] );
				Transform cellTransform = cell.transform;
				cellTransform.SetParent( cellRoot );
				cellTransform.localScale = Vector3.one;
				cellTransform.rotation = cellRoot.rotation;
				result.Add( cell );
			}
			return result;
		}
		#endregion
	}
}