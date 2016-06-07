/************************************************************
 * @title	CZLuaTest.cs
 * @desc	Lua のテスト
 * @author	Noirand 2016
 ************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
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
		public void AnsWait()
		{
			if (m_Parent != null)
				m_Parent.SetAnsWait();
		}
		public void ActWait()
		{
			if (m_Parent != null)
				m_Parent.SetActWait();
		}
		public void AddAsk(string sAsk)
		{
			if (m_Parent != null)
				m_Parent.AddAsk(sAsk);
		}
		public int GetAnswer()
		{
			if (m_Parent != null)
			{
				int iRet = m_Parent.Answer;
				m_Parent.ResetAnswer();
				return iRet;
			}
			return CZLuaTest.DZ_ANSWER_DEF;
		}
	}

	public enum EZ_WAIT_KEY
	{
		 _NONE	// 待たない
		,TIME	// 時間待ち
		,KEY	// キー入力待ち
		,ANSWER	// 返答入力待ち
		,ACTION	// アクション待ち
	}

	public EZ_WAIT_KEY	WaitKey		{ get; private set; }
	public int			Answer		{ get; private set; }
	public void	ResetAnswer()	{ Answer = DZ_ANSWER_DEF; }
	public void	SetNoWait()		{ WaitKey = EZ_WAIT_KEY._NONE; }
	public void	SetTimeWait()	{ WaitKey = EZ_WAIT_KEY.TIME; }
	public void	SetKeyWait()	{ WaitKey = EZ_WAIT_KEY.KEY; }
	public void SetAnsWait()	{ WaitKey = EZ_WAIT_KEY.ANSWER;	}
	public void SetActWait()	{ WaitKey = EZ_WAIT_KEY.ACTION; }

	//---------------------------------------------------
	// private
	//---------------------------------------------------
	private const int	DZ_ANSWER_DEF	= -1;

	private GameObject		m_PrefAskPanel;
	private List<Button>	m_AskList;

	private Text	m_Text;
	private Text	m_Name;
	private string	m_CurMsg;
	private bool	m_bMsgSkip;

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
		m_PrefAskPanel = Resources.Load<GameObject>("Prefabs/Pref_AskPanel");
		m_AskList = new List<Button>();
		Answer = DZ_ANSWER_DEF;
		m_bMsgSkip = false;

		UserData.RegisterAssembly();

		m_Text	= GetComponent<Text>();
		m_Name	= GameObject.Find("Name").GetComponent<Text>();
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
		if (Input.anyKeyDown)
		{
			m_bMsgSkip = true;
		}
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
				//m_Text.text = x.CastToString();
				m_CurMsg	= "";

				m_bMsgSkip = false;

				var corPutText = StartCoroutine(PutText(m_Lua.Globals["g_TalkWords"].ToString()));
				m_Name.text = m_Lua.Globals["g_TalkChara"].ToString();

				Debug.Log(x);

				yield return corPutText;

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
					case EZ_WAIT_KEY.ANSWER:
						if (m_AskList.Count > 0)
						{
							yield return new WaitUntil(() => (Answer != DZ_ANSWER_DEF));
							for (int ii = 0; ii < m_AskList.Count; ++ii)
							{
								Destroy(m_AskList[ii].transform.parent.gameObject);
							}
							m_AskList.Clear();
						}
						else {
							goto case EZ_WAIT_KEY.KEY;
						}
						break;
				}
			}
			else {
				// スクリプト終了処理
				TryLuaCoroutine();
			}
		}
	}
	//--------------------------------------------------
	// メッセージを1文字ずつ表示するコルーチン
	//--------------------------------------------------
	IEnumerator PutText(string sText)
	{
		int iNum = 0;

		while (iNum < sText.Length)
		{
			if (m_bMsgSkip)
			{
				if (iNum > 0)
				{
					iNum = sText.Length - 1;
					m_bMsgSkip = false;
				}
			}

			m_CurMsg = sText.Substring(0, ++iNum);

			m_Text.text = m_CurMsg;

			yield return new WaitForSeconds(0.1f);
		}
	}
	//--------------------------------------------------
	// 質問追加
	//--------------------------------------------------
	public void AddAsk(string sAsk)
	{
		if (m_PrefAskPanel != null && m_AskList != null)
		{
			int i = m_AskList.Count;
			GameObject pObj = Instantiate(m_PrefAskPanel) as GameObject;
			pObj.transform.SetParent(transform, false);
			pObj.name = "Ask_" + i;

			//int iAbs = ((i % 2) == 0) ? -1 : 1;
			Vector2 vPos = new Vector2(128 * Mathf.Cos((Mathf.PI * 2) / (i+1)),
				128 * Mathf.Sin((Mathf.PI * 2) / (i+1)));
			pObj.transform.localPosition = vPos;

			Button btn = pObj.GetComponentInChildren<Button>();
			btn.GetComponentInChildren<Text>().text = sAsk;
			btn.onClick.AddListener( () => {
				Answer = i;
				Debug.Log("(>_<)< Answer is: " + sAsk);
			});
			m_AskList.Add(btn);
		}
	}
	//---------------------------------------------------
//===========================================================
}
