using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chat_Manager : MonoBehaviour
{
	#region �г�
	[SerializeField]
	GameObject friend_list_Panel;

	[SerializeField]
	GameObject friend_accept_Panel;

	[SerializeField]
	GameObject friend_user_list_Panel;

	[SerializeField]
	GameObject rooms_Panel;

	[SerializeField]
	GameObject chat_Panel;
	#endregion

	#region DataBlock ����
	[SerializeField]
	GameObject Prefab_UserData_Block;

	[SerializeField]
	GameObject Scroll_Content_UserData;


	#endregion

	public static Chat_Manager Instance { get; private set; }
	public List<GameObject> All_Panel = new List<GameObject>();

	

	private void Awake()
	{
		if (Instance)
		{
			Destroy(Instance);
		}
		else
		{
			Instance = this;
		}
	}

	private void Start()
	{
		All_Panel.Add(rooms_Panel);
		All_Panel.Add(chat_Panel);
		All_Panel.Add(friend_user_list_Panel);
		All_Panel.Add(friend_list_Panel);
		All_Panel.Add(friend_accept_Panel);
		Debug.Log("flag 1");
		Network_Manager.Instance.GetAllData();
		Debug.Log("flag 2");
	}


	////ģ��â ����Ʈ
	//public void Onlick_Friend_List_Btn()
	//{
	//	Debug.Log("Onlick_Friend_List_Btn");
	//	All_Panel_Close();
	//	friend_list_Panel.SetActive(true);
	//}

	////ģ������ ����Ʈ
	//public void OnClick_Friend_Accept_Btn()
	//{
	//	Debug.Log("OnClick_Friend_Accept_Btn");
	//	All_Panel_Close();
	//	friend_accept_Panel.SetActive(true);
	//}

	////��������Ʈ
	//public void OnClick_User_List_Btn()
	//{
	//	Debug.Log("OnClick_User_List_Btn");
	//	All_Panel_Close();
	//	friend_user_list_Panel.SetActive(true);
	//}

	//Ư�� �г� Ű��
	/*
		0: ��        �г�
	    1: chat      �г�
	    2: ��������Ʈ �г�
        3: ģ������Ʈ �г�
	    4: ģ������   �г�
	*/
	public void Show_Panel(int idx)
	{
		if (All_Panel.Count-1 < idx) return;
		All_Panel_Close();
		All_Panel[idx].SetActive(true);
	}

	//��� �г� ����
	public void All_Panel_Close()
	{
		foreach (var Panel in All_Panel) {
			Panel.SetActive(false);
		}
	}

	//���� ����Ʈ �ʱ�ȭ
	public void Reset_User_List()
	{
		foreach (Transform child in Scroll_Content_UserData.transform)
		{
			// �ڽ� ������Ʈ�� ������ ����
			Destroy(child.gameObject);
		}
	}

	
	public void Add_User_list(string userId, string isFriendWithA, int numFriends, int numFollower, string joinDate, string is_following_A)
	{

		var obj = Instantiate(Prefab_UserData_Block);
		obj.transform.SetParent(Scroll_Content_UserData.transform, false);
		
		// ���� ID �ؽ�Ʈ ����.
		var textUserID = obj.transform.Find("Text_ID").GetComponent<Text>();
		textUserID.text = "ID: " + userId;

		// ���� ��¥ �ؽ�Ʈ ����
		var textResidate = obj.transform.Find("Text_Residate").GetComponent<Text>();
		textResidate.text = "���� ��¥: " + joinDate.Substring(0, 10); ;

		// ģ�� �� �ؽ�Ʈ ����
		var textFriendNum = obj.transform.Find("Text_FriendNum").GetComponent<Text>();
		textFriendNum.text = "ģ�� ��: " + numFriends.ToString();

		// �ȷο� �� �ؽ�Ʈ ����
		var textFollowerNum = obj.transform.Find("Text_FollowerNum").GetComponent<Text>();
		textFollowerNum.text = "�ȷο� ��: " + numFollower.ToString();

		// ģ����û ��ư Ȱ��ȭ ����
		var buttonRequestFriend = obj.transform.Find("Button_Request_Friend").GetComponent<Button>();
		if (isFriendWithA == "Yes")
		{
			buttonRequestFriend.gameObject.SetActive(false);
		}
		else
		{
			buttonRequestFriend.onClick.AddListener(() => RequestFriend(userId));
		}

		// �ȷο� ��ư Ȱ��ȭ ����
		var buttonRequestFollow = obj.transform.Find("Button_Request_Follow").GetComponent<Button>();
		if (is_following_A == "Yes")
		{
			buttonRequestFollow.gameObject.SetActive(false);
		}
		else
		{
			buttonRequestFollow.onClick.AddListener(() => RequestFollow(userId));
		}



	}

	//ģ����û ������
	public void RequestFriend(string _id)
	{
		Debug.Log(_id + "���� ģ�� ��û");
	}

	//�ȷο��û ������
	public void RequestFollow(string _id)
	{
		Debug.Log(_id + "���� �ȷο� ��û");

	}



}
