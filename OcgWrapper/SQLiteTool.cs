﻿/*
 * 由SharpDevelop创建。
 * 用户： Hasee
 * 日期: 2015/8/30
 * 时间: 8:54
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Text;
using System.IO;
using System.Data.SQLite;

namespace OcgWrapper
{
	/// <summary>
	/// Description of WinTool.
	/// </summary>
	public class SQLiteTool
	{

		public static void Init(string dbpath, string sql){
			if(!File.Exists(dbpath)){
				try{
					SQLiteConnection.CreateFile(dbpath);
					Command(dbpath, sql);
				}
				catch(Exception){
				}
			}
			// CREATE TABLE wins(id INTEGER PRIMARY KEY AUTOINCREMENT,[IDCardNo] VARCHAR (50),endtime TimeStamp NOT NULL DEFAULT CURRENT_TIMESTAMP);
		}
	
		#region 执行sql语句
		/// <summary>
		/// 执行sql语句
		/// </summary>
		/// <param name="DB">数据库</param>
		/// <param name="SQLs">sql语句</param>
		/// <returns>返回影响行数</returns>
		public static int Command(string DB, params string[] SQLs)
		{
			int result = 0;
			if ( File.Exists(DB) && SQLs != null )
			{
				using ( SQLiteConnection con = new SQLiteConnection(@"Data Source=" + DB) )
				{
					con.Open();
					using ( SQLiteTransaction trans = con.BeginTransaction() )
					{
						try
						{
							using ( SQLiteCommand cmd = new SQLiteCommand(con) )
							{
								foreach ( string SQLstr in SQLs )
								{
									cmd.CommandText = SQLstr;
									result += cmd.ExecuteNonQuery();
								}
							}
						}
						catch(Exception ex)
						{
							Console.WriteLine(ex.ToString());
							trans.Rollback();//出错，回滚
							result = -1;
						}
						finally
						{
							trans.Commit();
						}
					}
					con.Close();
				}
			}
			return result;
		}
		#endregion
	}
}
