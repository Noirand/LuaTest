/************************************************************
 * @title	DZLayoutParam.cs
 * @desc	レイアウトパラメータ定義クラス
 * @author	Noirand 2016
 ************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DZLayoutParam : ScriptableObject {
//===========================================================
// 変数宣言
//===========================================================
	//---------------------------------------------------
	// public
	//---------------------------------------------------
	[System.SerializableAttribute]
	public class Param
	{
		public string	name;
		public float	x;
		public float	y;
		public float	w;
		public float	h;
	}

	public List<Param>	list = new List<Param>();
	//---------------------------------------------------
//===========================================================
}
