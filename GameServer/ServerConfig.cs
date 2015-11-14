using System;
using System.IO;
using AsyncServer;

namespace YGOCore
{
	public class ServerConfig
	{
		/// <summary>
		/// 服务端口
		/// </summary>
		public int ServerPort { get; private set; }
		/// <summary>
		/// 工作目录
		/// </summary>
		public string Path { get; private set; }
		/// <summary>
		/// 脚本目录
		/// </summary>
		public string ScriptFolder { get; private set; }
		/// <summary>
		/// 卡牌数据库
		/// </summary>
		public string CardCDB { get; private set; }
		/// <summary>
		/// 禁卡表文件
		/// </summary>
		public string BanlistFile { get; private set; }
		/// <summary>
		/// 超时自动结束回合
		/// </summary>
		public bool AutoEndTurn { get; private set; }
		/// <summary>
		/// 客户端版本
		/// </summary>
		public int ClientVersion { get; private set; }
		/// <summary>
		/// 异步模式
		/// </summary>
		public bool AsyncMode{get;private set;}
		/// <summary>
		/// 日志等级
		/// </summary>
		public int LogLevel{get;private set;}
		/// <summary>
		/// 需要密码
		/// </summary>
		public bool isNeedAuth{get;private set;}
		/// <summary>
		/// 最大客户端数量
		/// </summary>
		public int MaxRoomCount{get;private set;}
		/// <summary>
		/// 自动录像
		/// </summary>
		public bool AutoReplay{get;private set;}
		/// <summary>
		/// 录像保存文件夹
		/// </summary>
		public string replayFolder { get; private set; }
		
		public bool RecordWin{get;private set;}
		/// <summary>
		/// 记录
		/// </summary>
		public string WinDbName{get;private set;}
		public int MaxAICount{get;private set;}
		
		public bool AIisHide{get;private set;}
		public string AIPass{get;private set;}
		/// <summary>
		/// 帐号禁止模式
		/// 0 不禁止
		/// 1 禁止列表
		/// 2 只允许列表
		/// </summary>
		public int BanMode{get;private set;}
		/// <summary>
		/// 帐号列表
		/// </summary>
		public string File_BanAccont{get;private set;}
		public int Timeout{get;private set;}
		/// <summary>
		/// 控制台api 
		/// </summary>
		public bool ConsoleApi{get;private set;}

		public ServerConfig()
		{
			ClientVersion = 0x1336;
			ServerPort = 8911;
			//	ApiIp="127.0.0.1";
			Path = ".";
			ScriptFolder = "script";
			replayFolder="replay";
			CardCDB = "cards.cdb";
			BanlistFile = "lflist.conf";
			AutoEndTurn = true;
			isNeedAuth=false;
			MaxRoomCount=200;
			WinDbName="wins.db";
			RecordWin=false;
			//PrivateChat=false;
			//SaveRecordTime=1;//
			MaxAICount=10;
			AIPass="kenan123";
			AIisHide=false;
			AsyncMode=false;
			BanMode = 0;
			File_BanAccont = "namelist.txt";
			Timeout = 15;
			ConsoleApi = true;
			//	Timeout = 20;
		}

		public bool Load(string file = "config.txt")
		{
			bool loaded = false;
			if (File.Exists(file))
			{
				StreamReader reader = null;
				try
				{
					reader = new StreamReader(File.OpenRead(file));
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine();
						if (line == null) continue;
						line = line.Trim();
						if (line.Equals(string.Empty)) continue;
						if (!line.Contains("=")) continue;
						if (line.StartsWith("#")) continue;

						string[] data = line.Split(new[] { '=' }, 2);
						string variable = data[0].Trim().ToLower();
						string value = data[1].Trim();
						setValue(variable, value);
					}
					loaded = true;
				}
				catch (Exception ex)
				{
					Logger.Error(ex);
				}finally{
					reader.Close();
				}
			}
			return loaded;
		}
		
		public bool setValue(string variable,string value){
			if(string.IsNullOrEmpty(value)||string.IsNullOrEmpty(variable)){
				return false;
			}
			variable=variable.ToLower();
			switch (variable)
			{
				case "aipassword":
					AIPass=value;
					break;
				case "maxai":
					MaxAICount=Convert.ToInt32(value);
					break;
				case "serverport":
					ServerPort = Convert.ToInt32(value);
					break;
				case "path":
					Path = value;
					break;
				case "scriptfolder":
					ScriptFolder = value;
					break;
				case "cardcdb":
					CardCDB = value;
					break;
				case "banlist":
					BanlistFile = value;
					break;
				case "bannamemode":
					BanMode = Convert.ToInt32(value);
					break;
				case "bannamelist":
					File_BanAccont = value;
					break;
				case "loglevel":
					LogLevel = Convert.ToInt32(value);
					break;
				case "autoendturn":
					AutoEndTurn = Convert.ToBoolean(value);
					break;
				case "clientversion":
					ClientVersion = Convert.ToInt32(value, 16);
					break;
				case "needauth":
					isNeedAuth = (value.ToLower()=="true"||value=="1");
					break;
				case "maxroom":
					MaxRoomCount=Convert.ToInt32(value);
					if(MaxRoomCount<=10){
						MaxRoomCount = 10;
					}
					break;
				case "autoreplay":
					AutoReplay= (value.ToLower()=="true"||value=="1");
					break;
				case "replayfolder":
					replayFolder=value;
					break;
				case "windbname":
					WinDbName=value;
					break;
				case "recordwin":
					RecordWin=(value.ToLower()=="true"||value=="1");
					break;
				case "asyncmode":
					AsyncMode= (value.ToLower()=="true"||value=="1");
					break;
				case "timeout":
					Timeout = Convert.ToInt32(value);
					break;
				case "consoleapi":
					ConsoleApi = Convert.ToBoolean(value);
					break;
				default:
					return false;
			}
			return true;
		}

	}
}
