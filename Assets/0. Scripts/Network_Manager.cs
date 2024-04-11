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

	//서버 정보
	string m_ip = "127.0.0.1";
	int m_port = 60000;


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
		Debug.Log("something receive");
		if (isSocketClosed) return;
		
		AsyncObject obj = (AsyncObject)ar.AsyncState;

		try
		{
			int bytesRead = obj.WorkingSocket.EndReceive(ar);
			if (bytesRead > 0)
			{
				byte[] receivedData = new byte[bytesRead];
				Array.Copy(obj.Buffer, 0, receivedData, 0, bytesRead);

				JsonDocument doc = JsonDocument.Parse(Encoding.Default.GetString(receivedData));
				JsonElement root = doc.RootElement;
				Debug.Log("받은 문자열  : " + root.GetRawText());


				string _action = root.GetProperty("action").GetString();
				if (_action == "JOIN_RESULT")   //회원가입 결과
				{
					bool _val = root.GetProperty("val").GetBoolean();
					if (_val)   //회원가입 성공
					{
						mainThreadWorkQueue.Enqueue(() => UI_Manager.Instance.JoinSuccess());
					}
					else        //회원가입 실패
					{
						mainThreadWorkQueue.Enqueue(() => UI_Manager.Instance.JoinFail());

					}
				} else if (_action == "LOGIN_RESULT") { //로그인 결과
					bool _val = root.GetProperty("val").GetBoolean();
					if (_val)   //로그인 성공.
					{
						mainThreadWorkQueue.Enqueue(() => UI_Manager.Instance.LoginSuccess());
					}
					else        //로그인 실패
					{
						mainThreadWorkQueue.Enqueue(() => UI_Manager.Instance.ShowSignWindow(false));
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
