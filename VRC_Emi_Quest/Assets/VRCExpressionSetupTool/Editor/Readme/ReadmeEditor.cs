using System;
using UnityEditor;
using UnityEngine;

namespace VRCExpressionSetupTool.Editor.Readme
{
	[CustomEditor(typeof(Readme))]
	public class ReadmeEditor : UnityEditor.Editor
	{
		[SerializeField] private GUIStyle linkStyle;
		[SerializeField] private GUIStyle titleStyle;
		[SerializeField] private GUIStyle headingStyle;
		[SerializeField] private GUIStyle bodyStyle;
		private bool initialized;
		
		[MenuItem("Tutorial/Show ExpressionTool Tutorial")]
		private static Readme SelectReadme() 
		{
			var ids = AssetDatabase.FindAssets("Readme t:Readme");
			if (ids.Length == 1)
			{
				var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
			
				Selection.objects = new[]{readmeObject};
			
				return (Readme)readmeObject;
			}

			Debug.Log("Couldn't find a readme");
			return null;
		}
	
		protected override void OnHeaderGUI()
		{
			var readme = (Readme)this.target;
			this.Init();
		
			var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth/3f - 20f, 128f);
		
			GUILayout.BeginHorizontal("In BigTitle");
			{
				GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
				GUILayout.Label(readme.title, this.titleStyle);
			}
			GUILayout.EndHorizontal();
		}
	
		// ReSharper disable once CognitiveComplexity
		public override void OnInspectorGUI()
		{
			var readme = (Readme)this.target;
			this.Init();
		
			foreach (var section in readme.sections)
			{
				if (!string.IsNullOrEmpty(section.heading))
				{
					GUILayout.Label(section.heading, this.headingStyle);
				}
				if (!string.IsNullOrEmpty(section.text))
				{
					foreach (var s in section.text.Split(new[] {"<\\br>"}, StringSplitOptions.None))
					{
						GUILayout.Label(s, this.bodyStyle);
					}
				}
				if (!string.IsNullOrEmpty(section.linkText))
				{
					if (this.LinkLabel(new GUIContent(section.linkText)))
					{
						Application.OpenURL(section.url);
					}
				}
				GUILayout.Space(EditorGUIUtility.singleLineHeight);
			}
		}


		private void Init()
		{
			if (this.initialized) return;
			
			this.bodyStyle = new GUIStyle(EditorStyles.label) {wordWrap = true, fontSize = 14};

			this.titleStyle = new GUIStyle(this.bodyStyle) {fontSize = 26};

			this.headingStyle = new GUIStyle(this.bodyStyle) {fontSize = 18};

			this.linkStyle = new GUIStyle(this.bodyStyle)
			{
				wordWrap = false,
				normal = {textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f)},
				stretchWidth = false
			};

			this.initialized = true;
		}

		private bool LinkLabel (GUIContent label, params GUILayoutOption[] options)
		{
			var position = GUILayoutUtility.GetRect(label, this.linkStyle, options);

			Handles.BeginGUI ();
			Handles.color = this.linkStyle.normal.textColor;
			Handles.DrawLine (new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
			Handles.color = Color.white;
			Handles.EndGUI ();

			EditorGUIUtility.AddCursorRect (position, MouseCursor.Link);

			return GUI.Button (position, label, this.linkStyle);
		}
	}
}

