/**************************************************************************************/
// Module written by Roberto Parente
//  
// Responsible for search all the time, for result files not sent to the server at game time
// (not connected to the Internet, for example)
/**************************************************************************************/
#if UNITY_STANDALONE || UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;
using System.IO;
using Npgsql;
using System.Threading;
//                                             TESTERESULTS


public class PgSqlThread : MonoBehaviour {
	
	private NpgsqlConnection conn = new NpgsqlConnection("Server=200.144.254.136;Port=54321;User Id=inres;Password=x1y2z3d4e5f6g7h8;Database=goleiro;SSL=true");

	private FileInfo FileUse;
	private String FileContent;
	private string sendDir = "SendedFiles";
	private string FilePath;
	private Thread ThreadPrincipal;
	private bool stopThread = false;
	private string dataPath;
	// Use this for initialization

	private string FileKey;   //170330 para inserir a chave do arquivo: YYMMDD_HHMMSS_RRR onde RRR vai de 000 a 999

	bool OpenConn()
	{
		//170109 para eliminar a msg "caca na bd": antes de abrir, verificar se a conexao existe e se o estado da base nao eh closed
		if ((conn != null) && (conn.State != System.Data.ConnectionState.Closed)) {    
			CloseConn();}                                
		
		try
		{               
			conn.Open();
			return true;
		}
		catch (Exception exp)
		{                
			Debug.Log("threadpSQL Error opening: "+ exp);
			return false;
		}
	}

	public void CloseConn()
	{
		try
		{
			conn.Close();
		}
		catch (Exception exp)
		{
			Debug.Log("threadpSQL Error closing: "+ exp);
		}
	}


	void Start () {
		// Creating our thread. The ThreadStart delegate is points to
		// the method being run in a new thread.
		dataPath = Application.dataPath;
		ThreadPrincipal = new Thread (new ThreadStart (this.execThread));

		// Starting our two threads. Thread.Sleep(10) gives the first Thread
		// 10 miliseconds more time.
		ThreadPrincipal.Start ();
	}


	// Update is called once per frame
	void execThread () {
		while (!stopThread) {
			// Enquanto o programa não for encerrado,tentamos entrar na thread
			if (checkFiles()) {
				// Primeiro passo: Armazenar o conteúdo do arquivo na string "FileContent"
				try {   // Open the text file using a stream reader.
					StreamReader sr = new StreamReader (FilePath);
					{
						// Read the stream to a string, and write the string to the console.
						FileContent = sr.ReadToEnd ();
						//print (FileContent);
						sr.Close();
					}
				} catch (Exception e) {
					Debug.Log("threadpSQL File could not be read:" + e.Message);    //171011
					//print (e.Message); foi para a linha de cima
				}

				// Segundo passo: Formatar a inserção no PostGreSQL
				//string query = "INSERT INTO goalGame VALUES ('" + FileContent + "')";
				//170330 criando serial e chave primaria na tabela
				string query = "INSERT INTO testeresults (gamekey, filecontent) VALUES ('" + FileKey + "', '" + FileContent + "')";

				// Terceiro passo: Conexão e envio ao banco de dados
				if (!this.OpenConn ()) {
					// testando se a conexão foi aberta.
					continue;
				}

				try {
					NpgsqlCommand cmd = new NpgsqlCommand (query, conn);
					cmd.ExecuteNonQuery ();
						
				} catch (NpgsqlException e) {
					Debug.Log("threadpSQL ExecuteNonQuery problem:" + conn.FullState);	  //171011
					if (e.BaseMessage.Contains ("pkey")) {
						/* O código gerou uma excecão de chave primária.
					 * Então precisamos elimintar o arquivo, pois ele
					 * já foi inserido em algum momento.
					 */
						conn.Close ();
						//finishFile ();   //170330 se o arquivo deu erro ao ser INSERT (nao cabia no tamanho varchar 5000) melhor nao mover
						continue;
					}
					Debug.Log("threadpSQL general failure at thread PostgreSQL\n" + e.BaseMessage);  //171011
				}
			
				this.CloseConn ();

				// 170714 se ficar aqui, move-se o arquivo para SendedFiles sem haver sido sended...
				//        mas, se sair daqui a thread ficará num loop eterno tentando mover este arquivo
				// Quarto passo: Movendo arquivo para diretório enviados & deletar
				finishFile ();

			} else {
				// Se não tem arquivo para enviar... para por 30segundos;
				// print("Nenhum arquivo novo para enviar, dormindo thread por 1s...");
			    // Thread.Sleep(1000);
			}
				
		}
	}

	bool finishFile(){
		string path = dataPath + "/" + sendDir;
		try 
		{
			// Determine whether the directory exists.
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
				Debug.Log("threadpSQL Directory created successfully at " + Directory.GetCreationTime(path));
			}
		} 
		catch (Exception e) 
		{
			Debug.Log("threadpSQL Directory creation failed: " + e.ToString());
		} 

		// Moving File to the directory
		//print(dataPath + "/"+sendDir+"/" + FileUse.Name);
		if(!File.Exists(dataPath + "/"+sendDir+"/" + FileUse.Name))
			FileUse.CopyTo(dataPath + "/"+sendDir+"/" + FileUse.Name,true);
		File.Delete (FilePath);
	//	FileUse.Delete ();
		return true;
	}


	bool checkFiles() {
		/*
		 * O método irá verificar se tem arquivos para enviar ao banco de dados. 
		 * Se tiver, o método retorna true e associa o caminho do arquivo na variável
		 * "FilePath".
		*/

		DirectoryInfo dir = new DirectoryInfo(dataPath);
		FileInfo[] fi = dir.GetFiles();


		foreach (FileInfo fiTemp in fi)
			if (fiTemp.Name.Contains ("Plays_") && fiTemp.Name.Contains (".csv") && !fiTemp.Name.Contains ("meta")) {
				Debug.Log("threadpSQL fiTemp.name=" + fiTemp.Name);
				FileUse = fiTemp;
				FilePath = dataPath + "/" + FileUse.Name;
				FileKey = fiTemp.Name.Substring (fiTemp.Name.Length - 21);   //170330 YYMMDD_HHMMSS_RRR.csv
				FileKey = FileKey.Substring (0, 17);                         //170330 YYMMDD_HHMMSS_RRR

				return true;
			}
		return false;
	}


	public void OnApplicationQuit(){
		stopThread = true;
	}

	public void OnDestroy(){
		stopThread = true;
	}


}
#endif