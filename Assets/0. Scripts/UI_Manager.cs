using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{
	#region Components

	//로그인 패널
	[SerializeField]
    GameObject login_Panel;

	[SerializeField]
	InputField input_login_id;

	[SerializeField]
	InputField input_login_pw;

	//회원가입 패널
	[SerializeField]
    GameObject join_Panel;

    [SerializeField]
	InputField input_join_id;

	[SerializeField]
	InputField input_join_pw1;

	[SerializeField]
	InputField input_join_pw2;

	//성공 실패창
	[SerializeField]
	GameObject success_Panel;

	[SerializeField]
	GameObject fail_Panel;

	#endregion

	public static UI_Manager Instance { get; private set; }

	private void Awake()
	{
		if (Instance)
		{
			Destroy(Instance);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	//회원가입 버튼클릭.
	public void Onclick_JoinTryBtn()
	{
		Debug.Log("Onclick_JoinBtn" + input_join_id.text + " " + input_join_pw1.text + " " +input_join_pw2.text + " ");


		//비밀번호 확인이 다르면 return
		if (input_join_pw1.text != input_join_pw2.text)
		{
			ShowSignWindow(false);
			return;
		}
		//아이디, 비밀번호 글자수 제한
		if (input_join_id.text.Length < 3 || input_join_pw1.text.Length < 3 || input_join_id.text.Length> 10 || input_join_pw1.text.Length> 10)
		{
			ShowSignWindow(false);
			return;
		}


		Network_Manager.Instance.JoinTry(input_join_id.text, input_join_pw1.text);
		
	}

	//로그인 버튼 클릭.
	public void Onlick_LoginTryBtn()
	{
		Debug.Log("Onlick_LoginTryBtn "+ input_login_id.text + " " + input_login_pw.text);
		//아이디, 비밀번호 글자수 제한
		if (input_login_id.text.Length < 3 || input_login_pw.text.Length < 3 || input_login_id.text.Length > 10 || input_login_pw.text.Length > 10)
		{
			ShowSignWindow(false);
			return;
		}

		Network_Manager.Instance.LoginTry(input_login_id.text, input_login_pw.text);
	}

	//회원가입 취소
	public void Onclick_JoinCancelBtn()
	{
		Debug.Log("Onclick_JoinCancelBtn");
		Init_Login_Panel();
	}

	//회원가입하러 가기
	public void Onclick_JoinBtn()
	{
		Debug.Log("Onclick_JoinCancelBtn");
		Init_Join_Panel();
	}

	//회원가입 패널 초기화
	public void Init_Join_Panel()
	{
		input_join_id.text = "";
		input_join_pw1.text = "";
		input_join_pw2.text = "";
		join_Panel.SetActive(true);
		login_Panel.SetActive(false);
	}

	//로그인 패널 초기화
	public void Init_Login_Panel()
	{
		join_Panel.SetActive(false);
		login_Panel.SetActive(true);
	}

	//성공 or 실패창
	public void ShowSignWindow(bool isSuccess)
	{
		if (isSuccess)
		{
			StartCoroutine(Cor_ShowSign(success_Panel));
		}
		else
		{
			StartCoroutine(Cor_ShowSign(fail_Panel));
		}
	}

	//성공 or 실패창 사라지게하기
	IEnumerator Cor_ShowSign(GameObject SignPanel)
	{
		SignPanel.SetActive(true);
		yield return new WaitForSeconds(1);

		SignPanel.SetActive(false);
		yield break;
	}

	//회원가입 성공처리
	public void JoinSuccess()
	{
		ShowSignWindow(true);
		Init_Login_Panel();
	}

	//회원가입 실패처리
	public void JoinFail()
	{
		ShowSignWindow(false);
	}

	//로그인 성공처리
	public void LoginSuccess()
	{
		//ShowSignWindow(true);
		Debug.Log("Login Success");
		StopAllCoroutines();
		SceneManager.LoadScene(1);
	}






}
