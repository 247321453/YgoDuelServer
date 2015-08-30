﻿using OcgWrapper.Enums;

namespace YGOCore.Game
{
	public class Player
	{
		public Game Game { get; private set; }
		public string Name {
			get{
				if(string.IsNullOrEmpty(namepassword)){
					return namepassword;
				}
				return namepassword.Split('$')[0];
			}
			private set{
				if(value!=null){
					namepassword=value.Trim();
				}
			}
		}
		private string namepassword;
		public bool IsAuthentified { get; private set; }
		public int Type { get; set; }
		public int TurnSkip { get; set; }
		public Deck Deck { get; private set; }
		public PlayerState State { get; set; }
		private GameClient m_client;

		public Player(GameClient client)
		{
			Game = client.Game;
			Type = (int)PlayerType.Undefined;
			State = PlayerState.None;
			m_client = client;
			TurnSkip = 0;
		}

		public void Send(GameServerPacket packet)
		{
			m_client.Send(packet.GetContent());
		}

		public void Disconnect()
		{
			m_client.Close();
		}

		public void OnDisconnected()
		{
			if (IsAuthentified){
				if(Game!=null){
					Game.RemovePlayer(this);
				}
			}
		}

		public void SendTypeChange()
		{
			if(Game==null)return;
			GameServerPacket packet = new GameServerPacket(StocMessage.TypeChange);
			packet.Write((byte)(Type + (Game.HostPlayer.Equals(this) ? (int)PlayerType.Host : 0)));
			Send(packet);
		}

		public bool Equals(Player player)
		{
			return ReferenceEquals(this, player);
		}

		public void Parse(GameClientPacket packet)
		{
			CtosMessage msg = packet.ReadCtos();
			switch (msg)
			{
				case CtosMessage.PlayerInfo:
					OnPlayerInfo(packet);
					break;
				case CtosMessage.JoinGame:
					OnJoinGame(packet);
					break;
				case CtosMessage.CreateGame:
					OnCreateGame(packet);
					break;
			}
			if (!IsAuthentified){
				return;
			}
			switch (msg)
			{
				case CtosMessage.Chat:
					OnChat(packet);
					break;
				case CtosMessage.HsToDuelist:
					if(Game!=null)
						Game.MoveToDuelist(this);
					break;
				case CtosMessage.HsToObserver:
					if(Game!=null)
						Game.MoveToObserver(this);
					break;
				case CtosMessage.LeaveGame:
					if(Game!=null)
						Game.RemovePlayer(this);
					break;
				case CtosMessage.HsReady:
					if(Game!=null)
						Game.SetReady(this, true);
					break;
				case CtosMessage.HsNotReady:
					if(Game!=null)
						Game.SetReady(this, false);
					break;
				case CtosMessage.HsKick:
					OnKick(packet);
					break;
				case CtosMessage.HsStart:
					if(Game!=null)
						Game.StartDuel(this);
					break;
				case CtosMessage.HandResult:
					OnHandResult(packet);
					break;
				case CtosMessage.TpResult:
					OnTpResult(packet);
					break;
				case CtosMessage.UpdateDeck:
					OnUpdateDeck(packet);
					break;
				case CtosMessage.Response:
					OnResponse(packet);
					break;
				case CtosMessage.Surrender:
					if(Game!=null)
						Game.Surrender(this, 0);
					break;
			}
		}

		private void OnPlayerInfo(GameClientPacket packet)
		{
			if (Name != null)
				return;
			Name = packet.ReadUnicode(20);

			if (string.IsNullOrEmpty(Name)){
				LobbyError("Username Required");
			}
			IsAuthentified = CheckAuth();
		}
		
		private bool CheckAuth(){
			if(Program.Config.isNeedAuth){
				string[] _names=namepassword.Split('$');
				if(_names.Length==1){
					LobbyError("You need a password.");
					return false;
				}else{
					if(!Server.onLogin(_names[0],_names[1])){
						//LobbyError("Auth Fail");
						return false;
					}
				}
			}
			return true;
		}

		private void OnCreateGame(GameClientPacket packet)
		{
			if (string.IsNullOrEmpty(Name) || Type != (int)PlayerType.Undefined)
				return;
			GameRoom room = null;
			
			room = GameManager.CreateOrGetGame(new GameConfig(packet));

			if (room == null)
			{
				LobbyError("Server Full");
				return;
			}

			Game = room.Game;
			Game.AddPlayer(this);
			IsAuthentified = CheckAuth();
			if(!IsAuthentified){
				LobbyError("Auth fail");
			}
		}

		private void OnJoinGame(GameClientPacket packet)
		{
			if (string.IsNullOrEmpty(Name) || Type != (int)PlayerType.Undefined)
				return;
			int version = packet.ReadInt16();
			if (version < Program.Config.ClientVersion)
			{
				LobbyError("Version too low");
				return;
			}
			else if (version > Program.Config.ClientVersion)
				ServerMessage("Warning: client version is higher than servers.");
			

			packet.ReadInt32();//gameid
			packet.ReadInt16();

			string joinCommand = packet.ReadUnicode(60);

			GameRoom room = null;
			
			IsAuthentified = CheckAuth();
			if(!IsAuthentified){
				LobbyError("Auth fail");
				return;
			}
			if(!GameManager.CheckPassword(joinCommand)){
				LobbyError("Password Error");
				return;
			}
			if(string.IsNullOrEmpty(joinCommand)){
				room = GameManager.GetRandomGame();
				if (room == null){
					room =  GameManager.CreateOrGetGame(new GameConfig(joinCommand));
				}
			}
			else if (GameManager.GameExists(joinCommand)){
				room = GameManager.GetGame(joinCommand);
			}
//			else if (joinCommand.ToLower() == "random")
//			{
//				room = GameManager.GetRandomGame();
//
//				if (room == null)
//				{
//					LobbyError("No Games");
//					return;
//				}
//			}
//			else if (joinCommand.ToLower() == "spectate")
//			{
//				room = GameManager.SpectateRandomGame();
//
//				if (room == null)
//				{
//					LobbyError("No Games");
//					return;
//				}
//			}
//			else if (string.IsNullOrEmpty(joinCommand) || joinCommand.ToLower() == "tcg" || joinCommand.ToLower() == "ocg"
//			         || joinCommand.ToLower() == "ocg/tcg" || joinCommand.ToLower() == "tcg/ocg")
//			{
//				int filter = string.IsNullOrEmpty(joinCommand) ? -1 :
//					(joinCommand.ToLower() == "ocg/tcg" || joinCommand.ToLower() == "tcg/ocg") ? 2 :
//					joinCommand.ToLower() == "tcg" ? 1 : 0;
//
//				room = GameManager.GetRandomGame(filter);
//				if (room == null) //if no games just make a new one!!
//					room = GameManager.CreateOrGetGame(new GameConfig(joinCommand));
//			}
			else
				room = GameManager.CreateOrGetGame(new GameConfig(joinCommand));

			if (room == null)
			{
				LobbyError("Server Busy");
				return;
			}
			if (!room.IsOpen)
			{
				LobbyError("Game Finished");
				return;
			}
			if(room.Game!=null && room.Game.Config!=null){
				//TODO: tips
				if(room.Game.Config.NoCheckDeck){
					ServerMessage("Warning: this room is no check deck.");
				}
				
				if(room.Game.Config.NoShuffleDeck){
					ServerMessage("Warning: this room is no shuffle deck.");
				}
				
				if(room.Game.Config.EnablePriority){
					ServerMessage("Warning: this room is enable priority.");
				}
				if(room.Game.Config.Rule!=2){
					ServerMessage("Warning: this room is "+(room.Game.Config.Rule==1?"TCG":"OCG")+" rule .");
				}
			}
			Game = room.Game;
			Game.AddPlayer(this);

		}

		private void OnChat(GameClientPacket packet)
		{
			string msg = packet.ReadUnicode(256);
			ChatCommand.onCommand(this, msg);
			if(Game!=null)
				Game.Chat(this, msg);
		}

		private void OnKick(GameClientPacket packet)
		{
			int pos = packet.ReadByte();
			if(Game!=null)
				Game.KickPlayer(this, pos);
		}

		private void OnHandResult(GameClientPacket packet)
		{
			int res = packet.ReadByte();
			if(Game!=null)
				Game.HandResult(this, res);
		}

		private void OnTpResult(GameClientPacket packet)
		{
			bool tp = packet.ReadByte() != 0;
			if(Game!=null)
				Game.TpResult(this, tp);
		}

		private void OnUpdateDeck(GameClientPacket packet)
		{
			if (Game==null||Type == (int)PlayerType.Observer)
				return;
			Deck deck = new Deck();
			int main = packet.ReadInt32();
			int side = packet.ReadInt32();
			for (int i = 0; i < main; i++)
				deck.AddMain(packet.ReadInt32());
			for (int i = 0; i < side; i++)
				deck.AddSide(packet.ReadInt32());
			if (Game.State == GameState.Lobby)
			{
				Deck = deck;
				Game.IsReady[Type] = false;
			}
			else if (Game.State == GameState.Side)
			{
				if (Game.IsReady[Type])
					return;
				if (!Deck.Check(deck))
				{
					GameServerPacket error = new GameServerPacket(StocMessage.ErrorMsg);
					error.Write((byte)3);
					error.Write(0);
					Send(error);
					return;
				}
				Deck = deck;
				Game.IsReady[Type] = true;
				Game.ServerMessage(Name + " is ready.");
				Send(new GameServerPacket(StocMessage.DuelStart));
				Game.MatchSide();
			}
		}

		private void OnResponse(GameClientPacket packet)
		{
			if (Game==null||Game.State != GameState.Duel)
				return;
			if (State != PlayerState.Response)
				return;
			byte[] resp = packet.ReadToEnd();
			if (resp.Length > 64)
				return;
			State = PlayerState.None;
			Game.SetResponse(resp);
		}

		public void LobbyError(string message)
		{
			GameServerPacket join = new GameServerPacket(StocMessage.JoinGame);
			join.Write(0U);
			join.Write((byte)0);
			join.Write((byte)0);
			join.Write(0);
			join.Write(0);
			join.Write(0);
			// C++ padding: 5 bytes + 3 bytes = 8 bytes
			for (int i = 0; i < 3; i++)
				join.Write((byte)0);
			join.Write(0);
			join.Write((byte)0);
			join.Write((byte)0);
			join.Write((short)0);
			Send(join);

			GameServerPacket enter = new GameServerPacket(StocMessage.HsPlayerEnter);
			enter.Write("[" + message + "]", 20);
			enter.Write((byte)0);
			Send(enter);
		}

		public void ServerMessage(string msg, string head="[Server] ")
		{
			string finalmsg = head + msg;
			GameServerPacket packet = new GameServerPacket(StocMessage.Chat);
			packet.Write((short)PlayerType.Yellow);
			packet.Write(finalmsg, finalmsg.Length + 1);
			Send(packet);
		}
	}
}