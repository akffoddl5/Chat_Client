using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{
	#region Components

	//�α��� �г�
	[SerializeField]
    GameObject login_Panel;

	[SerializeField]
	InputField input_login_id;

	[SerializeField]
	InputField input_login_pw;

	//ȸ������ �г�
	[SerializeField]
    GameObject join_Panel;

    [SerializeField]
	InputField input_join_id;

	[SerializeField]
	InputField input_join_pw1;

	[SerializeField]
	InputField input_join_pw2;

	//���� ����â
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

	//ȸ������ ��ưŬ��.
	public void Onclick_JoinTryBtn()
	{
		Debug.Log("Onclick_JoinBtn" + input_join_id.text + " " + input_join_pw1.text + " " +input_join_pw2.text + " ");


		//��й�ȣ Ȯ���� �ٸ��� return
		if (input_join_pw1.text != input_join_pw2.text)
		{
			ShowSignWindow(false);
			return;
		}
		//���̵�, ��й�ȣ ���ڼ� ����
		if (input_join_id.text.Length < 3 || input_join_pw1.text.Length < 3 || input_join_id.text.Length> 10 || input_join_pw1.text.Length> 10)
		{
			ShowSignWindow(false);
			return;
		}


		Network_Manager.Instance.JoinTry(input_join_id.text, input_join_pw1.text);
		
	}

	//�α��� ��ư Ŭ��.
	public void Onlick_LoginTryBtn()
	{
		Debug.Log("Onlick_LoginTryBtn "+ input_login_id.text + " " + input_login_pw.text);
		//���̵�, ��й�ȣ ���ڼ� ����
		if (input_login_id.text.Length < 3 || input_login_pw.text.Length < 3 || input_login_id.text.Length > 10 || input_login_pw.text.Length > 10)
		{
			ShowSignWindow(false);
			return;
		}

		Network_Manager.Instance.LoginTry(input_login_id.text, input_login_pw.text);
	}

	//ȸ������ ���
	public void Onclick_JoinCancelBtn()
	{
		Debug.Log("Onclick_JoinCancelBtn");
		Init_Login_Panel();
	}

	//ȸ�������Ϸ� ����
	public void Onclick_JoinBtn()
	{
		Debug.Log("Onclick_JoinCancelBtn");
		Init_Join_Panel();
	}

	//ȸ������ �г� �ʱ�ȭ
	public void Init_Join_Panel()
	{
		input_join_id.text = "";
		input_join_pw1.text = "";
		input_join_pw2.text = "";
		join_Panel.SetActive(true);
		login_Panel.SetActive(false);
	}

	//�α��� �г� �ʱ�ȭ
	public void Init_Login_Panel()
	{
		join_Panel.SetActive(false);
		login_Panel.SetActive(true);
	}

	//���� or ����â
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

	//���� or ����â ��������ϱ�
	IEnumerator Cor_ShowSign(GameObject SignPanel)
	{
		SignPanel.SetActive(true);
		yield return new WaitForSeconds(1);

		SignPanel.SetActive(false);
		yield break;
	}

	//ȸ������ ����ó��
	public void JoinSuccess()
	{
		ShowSignWindow(true);
		Init_Login_Panel();
	}

	//ȸ������ ����ó��
	public void JoinFail()
	{
		ShowSignWindow(false);
	}

	//�α��� ����ó��
	public void LoginSuccess()
	{
		//ShowSignWindow(true);
		Debug.Log("Login Success");
		StopAllCoroutines();
		SceneManager.LoadScene(1);
	}






}
