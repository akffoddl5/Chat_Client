using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chat_Manager : MonoBehaviour
{
	#region 패널
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

	#region DataBlock 관련
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


	////친구창 리스트
	//public void Onlick_Friend_List_Btn()
	//{
	//	Debug.Log("Onlick_Friend_List_Btn");
	//	All_Panel_Close();
	//	friend_list_Panel.SetActive(true);
	//}

	////친구수락 리스트
	//public void OnClick_Friend_Accept_Btn()
	//{
	//	Debug.Log("OnClick_Friend_Accept_Btn");
	//	All_Panel_Close();
	//	friend_accept_Panel.SetActive(true);
	//}

	////유저리스트
	//public void OnClick_User_List_Btn()
	//{
	//	Debug.Log("OnClick_User_List_Btn");
	//	All_Panel_Close();
	//	friend_user_list_Panel.SetActive(true);
	//}

	//특정 패널 키기
	/*
		0: 룸        패널
	    1: chat      패널
	    2: 유저리스트 패널
        3: 친구리스트 패널
	    4: 친구수락   패널
	*/
	public void Show_Panel(int idx)
	{
		if (All_Panel.Count-1 < idx) return;
		All_Panel_Close();
		All_Panel[idx].SetActive(true);
	}

	//모든 패널 끄기
	public void All_Panel_Close()
	{
		foreach (var Panel in All_Panel) {
			Panel.SetActive(false);
		}
	}

	//유저 리스트 초기화
	public void Reset_User_List()
	{
		foreach (Transform child in Scroll_Content_UserData.transform)
		{
			// 자식 오브젝트를 씬에서 제거
			Destroy(child.gameObject);
		}
	}

	
	public void Add_User_list(string userId, string isFriendWithA, int numFriends, int numFollower, string joinDate, string is_following_A)
	{

		var obj = Instantiate(Prefab_UserData_Block);
		obj.transform.SetParent(Scroll_Content_UserData.transform, false);
		
		// 유저 ID 텍스트 설정.
		var textUserID = obj.transform.Find("Text_ID").GetComponent<Text>();
		textUserID.text = "ID: " + userId;

		// 가입 날짜 텍스트 설정
		var textResidate = obj.transform.Find("Text_Residate").GetComponent<Text>();
		textResidate.text = "가입 날짜: " + joinDate.Substring(0, 10); ;

		// 친구 수 텍스트 설정
		var textFriendNum = obj.transform.Find("Text_FriendNum").GetComponent<Text>();
		textFriendNum.text = "친구 수: " + numFriends.ToString();

		// 팔로워 수 텍스트 설정
		var textFollowerNum = obj.transform.Find("Text_FollowerNum").GetComponent<Text>();
		textFollowerNum.text = "팔로워 수: " + numFollower.ToString();

		// 친구요청 버튼 활성화 여부
		var buttonRequestFriend = obj.transform.Find("Button_Request_Friend").GetComponent<Button>();
		if (isFriendWithA == "Yes")
		{
			buttonRequestFriend.gameObject.SetActive(false);
		}
		else
		{
			buttonRequestFriend.onClick.AddListener(() => RequestFriend(userId));
		}

		// 팔로워 버튼 활성화 여부
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

	//친구요청 보내기
	public void RequestFriend(string _id)
	{
		Debug.Log(_id + "에게 친구 요청");
	}

	//팔로우요청 보내기
	public void RequestFollow(string _id)
	{
		Debug.Log(_id + "에게 팔로우 요청");

	}



}
