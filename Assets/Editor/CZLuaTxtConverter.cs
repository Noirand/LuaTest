/************************************************************
 * @title	CZLuaTxtConverter.cs
 * @desc	.lua ファイルを .txt ファイルに変換
 * @author	Noirand 2016
 ************************************************************/
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

public class CZLuaTxtConverter : AssetPostprocessor {
//===========================================================
// 変数宣言
//===========================================================
	//---------------------------------------------------
	// private
	//---------------------------------------------------
	private const string LUA_FILE_PATH = "Assets/Resources/MoonSharp/Scripts";

//===========================================================
// 関数定義
//===========================================================
	//---------------------------------------------------
	// アセットがインポートされた時にコールされる
	//---------------------------------------------------
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
									   string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string sAsset in importedAssets)
		{
			string sTargetPath = Path.GetDirectoryName(sAsset);

			//Debug.Log(sTargetPath);

			if (sTargetPath != LUA_FILE_PATH)
				continue;

			string sExt = Path.GetExtension(sAsset);

			if (sExt.Equals(".lua"))
			{
				// .lua を .txt に
				FileUtil.CopyFileOrDirectory(sAsset, sAsset.Replace(".lua", ".txt"));
			}

			if (sExt.Equals(".txt"))
			{
				// コピー元とその .meta を削除
				string sFileName = Path.GetFileNameWithoutExtension(sAsset);
				FileUtil.DeleteFileOrDirectory(sAsset.Replace(".txt", ".lua"));
				FileUtil.DeleteFileOrDirectory(sAsset.Replace(".txt", ".lua.meta"));

				Debug.Log("(>_<)< Converted " + sFileName);

				AssetDatabase.Refresh();
			}
		}
	}
	//---------------------------------------------------
//===========================================================
}
