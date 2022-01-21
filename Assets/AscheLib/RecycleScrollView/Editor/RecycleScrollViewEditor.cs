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
		#region GUIContent
		GUIContent _spriteTypeContent;
        GUIContent _clockwiseContent;
		#endregion

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
		SerializedProperty _maskSprite;
		SerializedProperty _type;
		SerializedProperty _useSpriteMesh;
		SerializedProperty _fillCenter;
		SerializedProperty _fillMethod;
		SerializedProperty _fillOrigin;
		SerializedProperty _fillAmount;
		SerializedProperty _fillClockwise;
		SerializedProperty _cellPrefab;
		SerializedProperty _cellSize;
		SerializedProperty _columnLimit;
		SerializedProperty _arrangementType;
		#endregion

		#region ShowFlag
		AnimBool _showElasticity;
		AnimBool _showDecelerationRate;
		AnimBool _showSlicedOrTiled;
		AnimBool _showSliced;
		AnimBool _showTiled;
		AnimBool _showFilled;
		AnimBool _showType;
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
			_spriteTypeContent = EditorGUIUtility.TrTextContent("Image Type");
			_clockwiseContent = EditorGUIUtility.TrTextContent("Clockwise");

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
			_maskSprite = serializedObject.FindProperty("_maskSprite");
			_type = serializedObject.FindProperty("_type");
			_useSpriteMesh = serializedObject.FindProperty("_useSpriteMesh");
			_fillCenter = serializedObject.FindProperty("_fillCenter");
			_fillMethod = serializedObject.FindProperty("_fillMethod");
			_fillOrigin = serializedObject.FindProperty("_fillOrigin");
			_fillAmount = serializedObject.FindProperty("_fillAmount");
			_fillClockwise = serializedObject.FindProperty("_fillClockwise");
			_cellPrefab = serializedObject.FindProperty("_cellPrefab");
			_cellSize = serializedObject.FindProperty("_cellSize");
			_columnLimit = serializedObject.FindProperty("_columnLimit");
			_arrangementType = serializedObject.FindProperty("_arrangementType");

			_showElasticity = new AnimBool(Repaint);
			_showDecelerationRate = new AnimBool(Repaint);

			var typeEnum = (Image.Type)_type.enumValueIndex;
			_showSlicedOrTiled = new AnimBool(!_type.hasMultipleDifferentValues && typeEnum == Image.Type.Sliced, Repaint);
			_showSliced = new AnimBool(!_type.hasMultipleDifferentValues && typeEnum == Image.Type.Sliced, Repaint);
			_showTiled = new AnimBool(!_type.hasMultipleDifferentValues && typeEnum == Image.Type.Tiled, Repaint);
			_showFilled = new AnimBool(!_type.hasMultipleDifferentValues && typeEnum == Image.Type.Filled, Repaint);

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
			EditorGUILayout.PropertyField(_maskSprite);
			EditorGUILayout.PropertyField(_type, _spriteTypeContent);
			Image.Type typeEnum = (Image.Type)_type.enumValueIndex;

			bool showSlicedOrTiled = (!_type.hasMultipleDifferentValues && (typeEnum == Image.Type.Sliced || typeEnum == Image.Type.Tiled));
			if (showSlicedOrTiled && targets.Length > 1)
				showSlicedOrTiled = targets.Select(obj => obj as Image).All(img => img.hasBorder);

			_showSlicedOrTiled.target = showSlicedOrTiled;
			_showSliced.target = (showSlicedOrTiled && !_type.hasMultipleDifferentValues && typeEnum == Image.Type.Sliced);
			_showTiled.target = (showSlicedOrTiled && !_type.hasMultipleDifferentValues && typeEnum == Image.Type.Tiled);
			_showFilled.target = (!_type.hasMultipleDifferentValues && typeEnum == Image.Type.Filled);

			RecycleScrollView recycleScrollView = target as RecycleScrollView;
			if (EditorGUILayout.BeginFadeGroup(_showSlicedOrTiled.faded))
			{
				if (recycleScrollView.hasMaskBorder)
					EditorGUILayout.PropertyField(_fillCenter);
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(_showSliced.faded))
			{
				if (recycleScrollView.maskSprite != null && !recycleScrollView.hasMaskBorder)
					EditorGUILayout.HelpBox("This Image doesn't have a border.", MessageType.Warning);
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(_showTiled.faded))
			{
				if (recycleScrollView.maskSprite != null && !recycleScrollView.hasMaskBorder && (recycleScrollView.maskSprite.texture.wrapMode != TextureWrapMode.Repeat || recycleScrollView.maskSprite.packed))
					EditorGUILayout.HelpBox("It looks like you want to tile a sprite with no border. It would be more efficient to modify the Sprite properties, clear the Packing tag and set the Wrap mode to Repeat.", MessageType.Warning);
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(_showFilled.faded))
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(_fillMethod);
				if (EditorGUI.EndChangeCheck())
				{
					_fillOrigin.intValue = 0;
				}
				switch ((Image.FillMethod)_fillMethod.enumValueIndex)
				{
					case Image.FillMethod.Horizontal:
						_fillOrigin.intValue = (int)(Image.OriginHorizontal)EditorGUILayout.EnumPopup("Fill Origin", (Image.OriginHorizontal)_fillOrigin.intValue);
						break;
					case Image.FillMethod.Vertical:
						_fillOrigin.intValue = (int)(Image.OriginVertical)EditorGUILayout.EnumPopup("Fill Origin", (Image.OriginVertical)_fillOrigin.intValue);
						break;
					case Image.FillMethod.Radial90:
						_fillOrigin.intValue = (int)(Image.Origin90)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin90)_fillOrigin.intValue);
						break;
					case Image.FillMethod.Radial180:
						_fillOrigin.intValue = (int)(Image.Origin180)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin180)_fillOrigin.intValue);
						break;
					case Image.FillMethod.Radial360:
						_fillOrigin.intValue = (int)(Image.Origin360)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin360)_fillOrigin.intValue);
						break;
				}
				EditorGUILayout.PropertyField(_fillAmount);
				if ((Image.FillMethod)_fillMethod.enumValueIndex > Image.FillMethod.Vertical)
				{
					EditorGUILayout.PropertyField(_fillClockwise, _clockwiseContent);
				}
			}
			EditorGUILayout.EndFadeGroup();
		}
		void ShowGridSettings() {
			EditorGUILayout.PropertyField(_cellPrefab);
			EditorGUILayout.PropertyField(_cellSize);
			EditorGUILayout.PropertyField(_columnLimit);
			EditorGUILayout.PropertyField(_arrangementType);
		}
	}
}