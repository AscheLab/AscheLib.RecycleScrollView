using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace AscheLib.UI {
	[CustomEditor(typeof(RecycleScrollView), true)]
	[CanEditMultipleObjects]
	public class RecycleScrollViewEditor : Editor {
		#region SerializedProperty
		SerializedProperty _scrollType;
		SerializedProperty _movementType;
		SerializedProperty _elasticity;
		SerializedProperty _inertia;
		SerializedProperty _decelerationRate;
		SerializedProperty _scrollSensitivity;
		SerializedProperty _scrollbar;
		SerializedProperty _scrollbarVisibility;
		SerializedProperty _onValueChanged;
		SerializedProperty _showMaskGraphic;
		SerializedProperty _maskImage;
		SerializedProperty _cellPrefab;
		SerializedProperty _cellSize;
		SerializedProperty _columnLimit;
		SerializedProperty _arrangementType;
		#endregion

		#region ShowFlag
		AnimBool _showElasticity;
		AnimBool _showDecelerationRate;
		#endregion

		GUIStyle _cacheBoldtext = null;
		GUIStyle Boldtext {
			get {
				if(_cacheBoldtext == null) {
					_cacheBoldtext = new GUIStyle(GUI.skin.label);
					_cacheBoldtext.fontStyle = FontStyle.Bold;
				}
				return _cacheBoldtext;
			}
		}

		protected virtual void OnEnable() {
			_scrollType = serializedObject.FindProperty("_scrollType");
			_movementType = serializedObject.FindProperty("_movementType");
			_elasticity = serializedObject.FindProperty("_elasticity");
			_inertia = serializedObject.FindProperty("_inertia");
			_decelerationRate = serializedObject.FindProperty("_decelerationRate");
			_scrollSensitivity = serializedObject.FindProperty("_scrollSensitivity");
			_scrollbar = serializedObject.FindProperty("_scrollbar");
			_scrollbarVisibility = serializedObject.FindProperty("_scrollbarVisibility");
			_onValueChanged = serializedObject.FindProperty("_onValueChanged");
			_showMaskGraphic = serializedObject.FindProperty("_showMaskGraphic");
			_maskImage = serializedObject.FindProperty("_maskImage");
			_cellPrefab = serializedObject.FindProperty("_cellPrefab");
			_cellSize = serializedObject.FindProperty("_cellSize");
			_columnLimit = serializedObject.FindProperty("_columnLimit");
			_arrangementType = serializedObject.FindProperty("_arrangementType");

			_showElasticity = new AnimBool(Repaint);
			_showDecelerationRate = new AnimBool(Repaint);

			SetAnimBools(true);
		}
		void SetAnimBools(bool instant) {
			SetAnimBool(_showElasticity, !_movementType.hasMultipleDifferentValues && _movementType.enumValueIndex == (int)ScrollRect.MovementType.Elastic, instant);
			SetAnimBool(_showDecelerationRate, !_inertia.hasMultipleDifferentValues && _inertia.boolValue == true, instant);
		}
		void SetAnimBool(AnimBool a, bool value, bool instant) {
			if (instant) a.value = value;
			else a.target = value;
		}
		public override void OnInspectorGUI () {
			SetAnimBools(false);
			serializedObject.Update();
			
			ShowGroup("ScrollView Settings", ShowScrollbarSettings);
			EditorGUILayout.Separator();
			
			ShowGroup("Mask Settings", ShowMaskSettings);
			EditorGUILayout.Separator();
			
			ShowGroup("Grid Settings", ShowGridSettings);

			serializedObject.ApplyModifiedProperties();
		}

		void ShowGroup(string title, Action showGroupFunction) {
			EditorGUILayout.LabelField(title, Boldtext);
			EditorGUI.indentLevel++;
			showGroupFunction();
			EditorGUI.indentLevel--;
		}
		
		void ShowScrollbarSettings () {
			EditorGUILayout.PropertyField(_scrollType);
			EditorGUILayout.PropertyField(_movementType);
			if (EditorGUILayout.BeginFadeGroup(_showElasticity.faded)) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_elasticity);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.PropertyField(_inertia);
			if (EditorGUILayout.BeginFadeGroup(_showDecelerationRate.faded)) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_decelerationRate);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.PropertyField(_scrollSensitivity);
			EditorGUILayout.PropertyField(_scrollbar);
			if (_scrollbar.objectReferenceValue && !_scrollbar.hasMultipleDifferentValues) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_scrollbarVisibility, new GUIContent ("Visibility"));
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.PropertyField(_onValueChanged);
		}
		void ShowMaskSettings() {
			EditorGUILayout.PropertyField(_showMaskGraphic);
			EditorGUILayout.PropertyField(_maskImage);
		}
		void ShowGridSettings() {
			EditorGUILayout.PropertyField(_cellPrefab);
			EditorGUILayout.PropertyField(_cellSize);
			EditorGUILayout.PropertyField(_columnLimit);
			EditorGUILayout.PropertyField(_arrangementType);
		}
	}
}