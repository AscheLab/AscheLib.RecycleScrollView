using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AscheLib.UI {
	public interface IScrollableCell<T> {
		void SetValue(T value);
		T GetValue();
		Transform transform { get; }
	}
}