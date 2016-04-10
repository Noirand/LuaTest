/************************************************************
 * @title	CZLuaTest.cs
 * @desc	Lua のテスト
 * @author	Noirand 2016
 ************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

public class CZLuaTest : MonoBehaviour {
//===========================================================
// 変数宣言
//===========================================================
	//---------------------------------------------------
	// public
	//---------------------------------------------------
	[MoonSharpUserData]
	class MyLua {
		CZLuaTest m_Parent;
		public MyLua(CZLuaTest parent)
		{
			m_Parent = parent;
		}

		public void TimeWait()
		{
			if (m_Parent != null)
				m_Parent.SetTimeWait();
		}
		public void KeyWait()
		{
			if (m_Parent != null)
				m_Parent.SetKeyWait();
		}
		public void NoWait()
		{
			if (m_Parent != null)
				m_Parent.SetNoWait();
		}
	}

	public enum EZ_WAIT_KEY
	{
		 _NONE	// 待たない
		,TIME	// 時間待ち
		,KEY	// キー入力待ち
	}

	public EZ_WAIT_KEY	WaitKey		{ get; private set; }
	public void	SetNoWait()		{ WaitKey = EZ_WAIT_KEY._NONE; }
	public void	SetTimeWait()	{ WaitKey = EZ_WAIT_KEY.TIME; }
	public void	SetKeyWait()	{ WaitKey = EZ_WAIT_KEY.KEY; }

	//---------------------------------------------------
	// private
	//---------------------------------------------------
	private Text	m_Text;

	// lua 用クラス
	private Script		m_Lua;
	private DynValue	m_Func;
	private DynValue	m_LuaCoroutine;

//===========================================================
// 関数定義
//===========================================================
	//---------------------------------------------------
	// コンストラクタ
	//---------------------------------------------------
	void Awake()
	{
		UserData.RegisterAssembly();

		m_Text	= GetComponent<Text>();
		m_Lua	= new Script();

		WaitKey = EZ_WAIT_KEY._NONE;
	}
	//---------------------------------------------------
	// 最初の更新
	//---------------------------------------------------
	void Start()
	{
		((ScriptLoaderBase)m_Lua.Options.ScriptLoader).ModulePaths = 
			ScriptLoaderBase.UnpackStringPaths(UnityAssetsScriptLoader.DEFAULT_PATH + "/?");

		m_Lua.Globals["obj"] = new MyLua(this);

		var ret = m_Lua.DoFile("main.lua");//Script.RunFile("test");
		Debug.Log( ret.Table );
		if (ret.Table != null)
		{
			foreach (DynValue pKey in ret.Table.Keys)
			{
				Debug.Log(pKey.CastToString() + "," + ret.Table.Get(pKey));
				m_Text.text += ret.Table.Get(pKey) + "\n";
			}

			//Debug.Log(ret.Table.Values);
			//Debug.Log( m_Lua.Globals.Get("req").String );
		}

		TryLuaCoroutine();
	}
	//---------------------------------------------------
	// 更新処理
	//---------------------------------------------------
	void Update()
	{
	
	}
	//---------------------------------------------------
	// Lua のコルーチンてすと
	//---------------------------------------------------
	void TryLuaCoroutine()
	{
		m_Func = m_Lua.DoFile("adv");
		m_LuaCoroutine = m_Lua.CreateCoroutine(m_Func);
/*
		foreach (DynValue x in m_LuaCoroutine.Coroutine.AsTypedEnumerable())
		{
			//DynValue x = coroutine.Coroutine.Resume();
			m_Text.text += x + "\n";
			Debug.Log(x);
		}
*/		
		StartCoroutine(ShowDialog());
	}
	//---------------------------------------------------
	// Lua のコルーチンを受け取る C# コルーチン
	//---------------------------------------------------
	IEnumerator ShowDialog()
	{
		foreach (DynValue x in m_LuaCoroutine.Coroutine.AsTypedEnumerable())
		{
			//DynValue x = coroutine.Coroutine.Resume();
			if (x.IsNotVoid())
			{
				//m_Text.text += x + "\n";
				m_Text.text = x.CastToString();
				Debug.Log(x);
			}
			switch (WaitKey)
			{
				case EZ_WAIT_KEY._NONE:
					yield return null;
					break;
				case EZ_WAIT_KEY.TIME:
					yield return new WaitForSeconds(3);
					break;
				case EZ_WAIT_KEY.KEY:
					yield return new WaitUntil(() => Input.anyKeyDown);
					break;
			}
		}
	}
	//---------------------------------------------------
//===========================================================
}

//===========================================================
//===========================================================
#if false
[MoonSharpUserData]
class MyLua {
	public void TimeWait()
	{
		CZLuaTest.SetTimeWait();
	}
	public void KeyWait()
	{
		CZLuaTest.SetKeyWait();
	}
	public void NoWait()
	{
		CZLuaTest.SetNoWait();
	}
}
#endif
//===========================================================
