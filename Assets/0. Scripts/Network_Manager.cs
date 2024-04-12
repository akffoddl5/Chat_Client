using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Text.Json;
using System.Collections.Concurrent;




public class AsyncObject
{
	public byte[] Buffer;
	public Socket WorkingSocket;
	public readonly int BufferSize;
	public AsyncObject(int bufferSize)
	{
		BufferSize = bufferSize;
		Buffer = new byte[(long)BufferSize];
	}

	public void ClearBuffer()
	{
		Array.Clear(Buffer, 0, BufferSize);
	}
}

public class Network_Manager : MonoBehaviour
{
    //네트워크 매니저 싱글톤 객체
    public static Network_Manager Instance { get; private set; }
	//메인쓰레드 큐잉
	private ConcurrentQueue<Action> mainThreadWorkQueue = new ConcurrentQueue<Action>();


	//클라이언트 소켓
	public Socket socket;
	bool isSocketClosed = false;

	//서버 정보.
	//string m_ip = "127.0.0.1";
	string m_ip = "210.179.17.24";
	int m_port = 60006;


	public void Connect()
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		IPAddress serverAddr = IPAddress.Parse(m_ip);//"10.0.0.10");
		IPEndPoint clientEP = new IPEndPoint(serverAddr, m_port);
		socket.BeginConnect(clientEP, new AsyncCallback(ConnectCallback), socket);
	}

	void ConnectCallback(IAsyncResult ar)
	{
		try
		{
			Socket client = (Socket)ar.AsyncState;
			client.EndConnect(ar);
			AsyncObject obj = new AsyncObject(4096);
			obj.WorkingSocket = socket;
			socket.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, DataReceived, obj);

			Debug.Log("연결");
		}
		catch (Exception e)
		{
			Debug.LogError("Error in accept callback: " + e.Message);
		}
	}

	void DataReceived(IAsyncResult ar)
	{
		//Debug.Log("something receive");
		if (isSocketClosed) return;
		
		AsyncObject obj = (AsyncObject)ar.AsyncState;

		try
		{
			int bytesRead = obj.WorkingSocket.EndReceive(ar);
			if (bytesRead > 0)
			{
				byte[] receivedData = new byte[bytesRead];
				Array.Copy(obj.Buffer, 0, receivedData, 0, bytesRead);
				Debug.Log(Encoding.Default.GetString(receivedData));
				string[] tmpCommand = Encoding.Default.GetString(receivedData).Split("/SPRING/");

				foreach (var Command in tmpCommand)
				{
					JsonDocument doc = JsonDocument.Parse(Command);
					JsonElement root = doc.RootElement;
					Debug.Log("받은 문자열   : " + root.GetRawText());


					string _action = root.GetProperty("action").GetString();
					if (_action == "JOIN_RESULT")   //회원가입 결과
					{
						bool _val = root.GetProperty("val").GetBoolean();
						if (_val)   //회원가입 성공
						{
							mainThreadWorkQueue.Enqueue(() => Login_Manager.Instance.JoinSuccess());
						}
						else        //회원가입 실패
						{
							mainThreadWorkQueue.Enqueue(() => Login_Manager.Instance.JoinFail());

						}
					}
					else if (_action == "LOGIN_RESULT")
					{ //로그인 결과
						bool _val = root.GetProperty("val").GetBoolean();
						if (_val)   //로그인 성공.
						{
							mainThreadWorkQueue.Enqueue(() => Login_Manager.Instance.LoginSuccess());
						}
						else        //로그인 실패
						{
							mainThreadWorkQueue.Enqueue(() => Login_Manager.Instance.ShowSignWindow(false));
						}
					}
					else if (_action == "DATA_ALL_USER")
					{
						JsonElement usersElement = root.GetProperty("val");

						mainThreadWorkQueue.Enqueue(() => Chat_Manager.Instance.Reset_User_List());


						foreach (JsonElement userElement in usersElement.EnumerateArray())
						{
							string userId = userElement.GetProperty("user_id").GetString();
							string isFriendWithA = userElement.GetProperty("is_friend_with_A").GetString();
							int numFriends = userElement.GetProperty("num_friends").GetInt32();
							int numFollowers = userElement.GetProperty("num_followers").GetInt32();
							string joinDate = userElement.GetProperty("join_date").GetString();
							string is_following_A = userElement.GetProperty("is_following_A").GetString();
							string friend_request_status = userElement.GetProperty("friend_request_status").GetString();

							// 사용자 정보를 콘솔(또는 디버그 로그)에 출력
							Debug.Log($"User ID: {userId}, Is Friend With A: {isFriendWithA}, Number of Friends: {numFriends}, Number of Followers: {numFollowers}, Join Date: {joinDate}, is_following_A : {is_following_A}");

							// 여기서 받은 사용자 정보를 바탕으로 필요한 처리 수행
							mainThreadWorkQueue.Enqueue(() => Chat_Manager.Instance.Add_User_list(userId, isFriendWithA, numFriends, numFollowers, joinDate, is_following_A, friend_request_status));



						}

					}
					else if (_action == "DATA_FRIEND_LIST")
					{
						JsonElement usersElement = root.GetProperty("val");

						mainThreadWorkQueue.Enqueue(() => Chat_Manager.Instance.Reset_Friend_List());

						foreach (JsonElement userElement in usersElement.EnumerateArray())
						{
							string friend_id = userElement.GetProperty("friend_id").GetString();
							string residate = userElement.GetProperty("residate").GetString();


							// 여기서 받은 사용자 정보를 바탕으로 필요한 처리 수행
							mainThreadWorkQueue.Enqueue(() => Chat_Manager.Instance.Add_Friend_list(friend_id, residate));

						}

					}
					else if (_action == "DATA_FRIEND_REQUEST_LIST")
					{
						JsonElement usersElement = root.GetProperty("val");

						mainThreadWorkQueue.Enqueue(() => Chat_Manager.Instance.Reset_Friend_Request_List());

						foreach (JsonElement userElement in usersElement.EnumerateArray())
						{
							//fr.user_id, fr.status, fr.request_date
							string user_id = userElement.GetProperty("user_id").GetString();
							string status = userElement.GetProperty("status").GetString();
							string request_date = userElement.GetProperty("request_date").GetString();


							//// 여기서 받은 사용자 정보를 바탕으로 필요한 처리 수행
							mainThreadWorkQueue.Enqueue(() => Chat_Manager.Instance.Add_Friend_Request_list(user_id, request_date, status));

						}

					}
					else if (_action == "LOGOUT_COMPLETE")
					{
						mainThreadWorkQueue.Enqueue(() => Chat_Manager.Instance.GoLogout());
					}

				}

				
				

			}

			// 다음 데이터 수신 대기
			obj.WorkingSocket.BeginReceive(obj.Buffer, 0, obj.Buffer.Length, 0, DataReceived, obj);
		}
		catch (Exception e)
		{
			Debug.LogError("Error in DataReceived: " + e.Message);
		}

		socket.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, DataReceived, obj);
	}


	private void Awake()
	{
        if (Instance == null)
        {
			Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
	}

	void Start()
    {
		Connect();
	}

    // Update is called once per frame
    void Update()
    {
		while (mainThreadWorkQueue.TryDequeue(out var work))
		{
			work?.Invoke();
		}
	}//

	private void OnDestroy()
	{
		SocketClose();
	}

	private void SocketClose()
	{
		isSocketClosed = true;
		
		if (socket != null)
		{
			socket.Close();
			socket.Dispose();
		}
	}

	public void SendMessages(string message)
	{
		if (socket == null || !socket.Connected)
		{
			Debug.LogError("Socket is not connected.");
			return;
		}

		try
		{
			// 메시지를 바이트 배열로 변환
			byte[] byteMessage = Encoding.UTF8.GetBytes(message);
			// 비동기적으로 메시지 전송
			socket.BeginSend(byteMessage, 0, byteMessage.Length, SocketFlags.None, SendCallback, socket);
		}
		catch (Exception e)
		{
			Debug.LogError("Sending message failed: " + e.Message);
		}
	}

	void SendCallback(IAsyncResult ar)
	{
		try
		{
			// 메시지 전송 완료
			Socket client = (Socket)ar.AsyncState;
			int bytesSent = client.EndSend(ar);
			Debug.Log($"Sent {bytesSent} bytes to server.");
		}
		catch (Exception e)
		{
			Debug.LogError("Error in send callback: " + e.Message);
		}
	}

	public void JoinTry(string id, string pw)
	{
		// 비밀번호 해시 생성
		string hashedPassword = ComputeSha256Hash(pw);
		string str_message = "{" +
			"\"action\" : \"" + "JOIN_TRY" + "\" " +
			",\"id\" : \"" + id + "\" " +
			",\"pw\" : \"" + hashedPassword + "\" " +
			"}";
		SendMessages(str_message);
	}

	public void LoginTry(string id, string pw)
	{
		// 비밀번호 해시 생성
		string hashedPassword = ComputeSha256Hash(pw);
		string str_message = "{" +
			"\"action\" : \"" + "LOGIN_TRY" + "\" " +
			",\"id\" : \"" + id + "\" " +
			",\"pw\" : \"" + hashedPassword + "\" " +
			"}";
		SendMessages(str_message);
	}

	public void GetAllData()
	{
		string str_message = "{" +
			"\"action\" : \"" + "GET_ALL_DATA" + "\" " +
			"}";
		SendMessages(str_message);
	}

	public void Request_Follow(string _id)
	{
		string str_message = "{" +
			"\"action\" : \"" + "REQUEST_FOLLOW" + "\" " +
			",\"id\" : \"" + _id + "\" " +
			"}";
		SendMessages(str_message);
	}

	public void Request_Friend(string _id)
	{
		string str_message = "{" +
			"\"action\" : \"" + "REQUEST_FRIEND" + "\" " +
			",\"id\" : \"" + _id + "\" " +
			"}";
		SendMessages(str_message);
	}

	public void Accept_Friend(string _id)
	{
		string str_message = "{" +
			"\"action\" : \"" + "ACCEPT_FRIEND" + "\" " +
			",\"id\" : \"" + _id + "\" " +
			"}";
		SendMessages(str_message);
	}

	public void Deny_Friend(string _id)
	{
		string str_message = "{" +
			"\"action\" : \"" + "DENY_FRIEND" + "\" " +
			",\"id\" : \"" + _id + "\" " +
			"}";
		SendMessages(str_message);
	}

	public void Delete_Friend(string _id)
	{
		string str_message = "{" +
			"\"action\" : \"" + "DELETE_FRIEND" + "\" " +
			",\"id\" : \"" + _id + "\" " +
			"}";
		SendMessages(str_message);
	}

	public void Logout()
	{
		string str_message = "{" +
			"\"action\" : \"" + "LOGOUT" + "\" " +
			"}";
		SendMessages(str_message);
	}

	public static string ComputeSha256Hash(string rawData)
	{
		// SHA256 인스턴스 생성
		using (SHA256 sha256Hash = SHA256.Create())
		{
			// 입력 문자열을 바이트 배열로 변환
			byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

			// 바이트 배열을 Hex 문자열로 변환
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
			{
				builder.Append(bytes[i].ToString("x2"));
			}
			return builder.ToString();
		}
	}
}
