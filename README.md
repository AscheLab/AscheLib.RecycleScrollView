# AscheLib.RecycleScrollView
Created by Syunta Washidu (AscheLab)

## What's RecycleScrollView?
RecycleScrollView is a library to easily implement a scroll view that can be recycled to reduce object generation cost when displaying a large amount of data

## Install
### Using UnityPackageManager
Find the manifest.json file in the Packages folder of your project and edit it to look like this.
```
"scopedRegistries": [
    {
      "name": "Unofficial Unity Package Manager Registry",
      "url": "https://upm-packages.dev",
      "scopes": [
        "com.aschelab"
      ]
    }
  ],
  "dependencies": {
    "com.aschelab.common.disposable": "1.0.4",
    "com.aschelab.recyclescrollview": "1.0.3",
  ...
  }
```
## Using for RecycleScrollView
```csharp
using UnityEngine;
using UnityEngine.UI;
using AscheLib.UI;
```

1. Prepare the data class you want to display
```csharp
// Test data class
class ExampleData {
	public string Text { private set; get; }
	public ExampleData (string text) {
		Text = text;
	}
}
```

2. Prepare cell components for displaying data
```csharp
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
```

3. Prepare cell Prefab to display data.<br>
please attach the cell component created in step 2

4. Place a UI object with the RecycleScrollView component attached on the scene

5. Set Prefab created in step 3 to CellPrefab of Grid Settings of RecycleScrollView component<br>
There are other items that can be set<br>
![b3dadb508c3ff1f6a0417df127becd9c](https://user-images.githubusercontent.com/47095602/61518763-52323c00-aa45-11e9-9382-110d664922b7.png)

6. Set the list of data you want to display to RecycleScrollView
```csharp
// Set Scroll View object to display data from Inspector
[SerializeField]
RecycleScrollView _scrollView;

// Number of test data
[SerializeField]
int _dataRange;

private void Start () {
    // Create test data.
    List<ExampleData> dataList = Enumerable.Range(0, _dataRange)
        .Select(v => new ExampleData ("Data" + v))
        .ToList();

    // Sets data in scroll view.
    // Then the data will be displayed according to the content set in ScrollView
    _scrollView.SetDatas(dataList);
}
```

## License
This library is under the MIT License.