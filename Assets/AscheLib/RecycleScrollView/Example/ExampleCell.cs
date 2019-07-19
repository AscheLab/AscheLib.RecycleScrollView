using UnityEngine;
using UnityEngine.UI;
using AscheLib.UI;

// Cell component for displaying test data
class ExampleCell : MonoBehaviour, IScrollableCell<ExampleData> {
	[SerializeField]
	Text _textLabel;

	// Hold the set data
	ExampleData _cacheData = null;

	// Method to return the held data
	ExampleData IScrollableCell<ExampleData>.GetValue () {
		return _cacheData;
	}

	// Executed when data is set
	void IScrollableCell<ExampleData>.SetValue (ExampleData value) {
		_cacheData = value;
		_textLabel.text = value.Text;
	}
}