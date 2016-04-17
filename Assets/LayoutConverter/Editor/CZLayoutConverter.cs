/************************************************************
 * @title	CZLayoutConverter.cs
 * @desc	
 * @author	Noirand 2016
 ************************************************************/
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;//.Serialization;

public class CZLayoutConverter : AssetPostprocessor {
//===========================================================
// 変数宣言
//===========================================================
	//---------------------------------------------------
	// public
	//---------------------------------------------------

	//---------------------------------------------------
	// private
	//---------------------------------------------------
	private const string LAYOUT_FILE_PATH	= "Assets/LayoutConverter/XML";
	private const string LAYOUT_OBJ_PATH	= "Assets/LayoutConverter";

//===========================================================
// 関数定義
//===========================================================
	//---------------------------------------------------
	// アセットインポート時のコールバック
	//---------------------------------------------------
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets)
		{
			string sTargetPath = Path.GetDirectoryName(asset);
			string sFileName = Path.GetFileNameWithoutExtension(asset);
			string sOutputPath = LAYOUT_OBJ_PATH + "/" + sFileName + ".asset";

			if (sTargetPath != LAYOUT_FILE_PATH)
				continue;

			DZLayoutParam data = (DZLayoutParam)AssetDatabase.LoadAssetAtPath(sOutputPath, typeof(DZLayoutParam));

			if (data == null)
			{
				data = ScriptableObject.CreateInstance<DZLayoutParam>();
				AssetDatabase.CreateAsset((ScriptableObject)data, sOutputPath);
				data.hideFlags = HideFlags.NotEditable;
			}

			data.list.Clear();

			//using (FileStream stream = File.Open(asset, FileMode.Open, FileAccess.Read))
			//{
			//	XmlDocument xml;
			//}
			XmlDocument xml = new XmlDocument();
			xml.Load(asset);
			XmlNode root = xml.ChildNodes[1];
			foreach (XmlNode child in root.ChildNodes)
			{
				DZLayoutParam.Param p = new DZLayoutParam.Param();
				p.name = child.Attributes[0].Value;
				p.x = float.Parse(child.Attributes[2].Value);
				p.y = float.Parse(child.Attributes[3].Value);
				p.w = float.Parse(child.Attributes[4].Value);
				p.h = float.Parse(child.Attributes[5].Value);

				data.list.Add(p);
			}

			ScriptableObject obj = AssetDatabase.LoadAssetAtPath(sOutputPath, typeof(ScriptableObject)) as ScriptableObject;
			EditorUtility.SetDirty(obj);
		}
	}
	//---------------------------------------------------
//===========================================================
}
