using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AscheLib.UI;

class Example : MonoBehaviour {
	// Set Scroll View object to display data from Inspector
	[SerializeField]
	RecycleScrollView _scrollView = null;

	// Number of test data
	[SerializeField]
	int _dataRange = 50;

	private void Start () {
		// Create test data.
		List<ExampleData> dataList = Enumerable.Range(0, _dataRange)
			.Select(v => new ExampleData ("Data" + v))
			.ToList();

		// Sets data in scroll view.
		// Then the data will be displayed according to the content set in ScrollView
		_scrollView.SetDatas(dataList);
	}
}