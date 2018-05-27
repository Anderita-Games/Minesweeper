using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {
	public RectTransform Game_Area;
	float Game_Arena_Max;
	public GameObject Cell;
	public GameObject Line;

	//Top User Interface
	public UnityEngine.UI.Image Face;
	public Sprite Face_Cool;
	public Sprite Face_Dead;
	public Sprite Face_Smile;
	public Sprite Face_Surprised;

	public UnityEngine.UI.Image Flag_Toggle;
	public Sprite Flag_Version;
	public Sprite Bomb_Version;

	public UnityEngine.UI.Text Flag_Count_Remaining;
	public UnityEngine.UI.Text Time_Count;

	//Bottom User Interface
	float Line_Thicc = 6;
	float Buffer = 40;
	float Cell_Size;
	float Arena_Width;
	float Arena_Height;

	//Settings User Interface
	public GameObject Settings;
	public UnityEngine.UI.Text Arena_Size_New;
	public UnityEngine.UI.Text Bomb_Amount_New;
	public UnityEngine.UI.Text Warning_Text;

	//Game Variables
	public string[] Bomb_Cells;
	int Arena_Size;
	int Arena_Blocks_X;
	int Arena_Blocks_Y;
	public bool Game_Active = true;
	public bool Flag_Mode = false;
	public int Bombs_Flagged = 0;
	public int Cells_Activated = 0;

	void Start () {
		if (PlayerPrefs.GetInt("Bomb Quantity") == null || PlayerPrefs.GetInt("Arena Size") == null || PlayerPrefs.GetInt("Arena Size") > 1000) {
			PlayerPrefs.SetInt("Arena Size", 100);
			PlayerPrefs.SetInt("Bomb Quantity", 10);
		}

		Flag_Text_Changer(PlayerPrefs.GetInt("Bomb Quantity"));

		Game_Arena_Max = (float)(Game_Area.rect.height < Game_Area.rect.width ? Game_Area.rect.height : Game_Area.rect.width) - 20;

		float Game_Arena_Average = (Game_Area.rect.width + Game_Area.rect.height) / 2.0f;
		Arena_Blocks_X = (int) Mathf.Round((Game_Area.rect.width / Game_Arena_Average) * Mathf.Sqrt(PlayerPrefs.GetInt("Arena Size")));
		Arena_Blocks_Y = (int) Mathf.Round((Game_Area.rect.height / Game_Arena_Average) * Mathf.Sqrt(PlayerPrefs.GetInt("Arena Size")));
		Arena_Size = Arena_Blocks_X * Arena_Blocks_Y;
		Line_Thicc = (8.0f / Mathf.Sqrt(Arena_Size)) * Line_Thicc;
		Arena_Width = (Game_Area.rect.width - (Buffer * 2));
		Arena_Height = ((Arena_Width - Line_Thicc) / Arena_Blocks_X) * Arena_Blocks_Y;
		
		Cell_Creation();
		Line_Creation();
	}


	void Update () {
		if (Game_Active == true) {
			if (Input.GetMouseButton(0)) {
				Face.sprite = Face_Surprised;
			}else {
				Face.sprite = Face_Smile;
			}

			if (int.Parse(Flag_Count_Remaining.text) == 0 && Bombs_Flagged == PlayerPrefs.GetInt("Bomb Quantity") && Cells_Activated == Arena_Blocks_X * Arena_Blocks_Y - PlayerPrefs.GetInt("Bomb Quantity")) {
				Game_Over("WIN!");
			}
		}
	}


	void Line_Creation () { //START OF CREATION
		for (int Line_Current = 1; Line_Current <= Arena_Blocks_X + 1; Line_Current++) {
			GameObject Clone_Horizontal = Instantiate(Line, new Vector3(0, 0, 0), Quaternion.identity);
			Clone_Horizontal.name = "Vertical Line #" + Line_Current;
			Clone_Horizontal.transform.SetParent(Game_Area.transform);
			Clone_Horizontal.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
			Clone_Horizontal.GetComponent<RectTransform>().sizeDelta = new Vector3(Line_Thicc, (Arena_Height + Line_Thicc), 1);
			Clone_Horizontal.transform.localPosition = new Vector3(((Arena_Width - Line_Thicc) / Arena_Blocks_X) * ((Line_Current) - 1) - ((Game_Area.rect.width - (Buffer * 2)) / 2) + (Line_Thicc / 2.0f), ((Arena_Height - Game_Area.rect.height) / 2.0f) + Buffer + (Line_Thicc / 2), 0);
		}
		for (int Line_Current = Arena_Blocks_Y + 1; Line_Current >= 1; Line_Current--) {
			GameObject Clone_Vertical = Instantiate(Line, new Vector3(0, 0, 0), Quaternion.identity);
			Clone_Vertical.name = "Horizontal Line #" + ((Arena_Blocks_Y + 2) - Line_Current);
			Clone_Vertical.transform.SetParent(Game_Area.transform);
			Clone_Vertical.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
			Clone_Vertical.GetComponent<RectTransform>().sizeDelta = new Vector3(Game_Area.rect.width - (Buffer * 2), Line_Thicc, 1);
			Clone_Vertical.transform.localPosition = new Vector3(0, (Arena_Height / Arena_Blocks_Y) * ((Line_Current) - 1) - (Game_Area.rect.height / 2.0f) + Buffer + (Line_Thicc / 2), 0); 
		}
	}

	void Cell_Creation () {
		Cell_Size = Mathf.Min(Arena_Width - Line_Thicc, Arena_Height - Line_Thicc) / Mathf.Min(Arena_Blocks_X, Arena_Blocks_Y) - (Line_Thicc);
		for (int Row_Current = 0; Row_Current < Arena_Blocks_X; Row_Current++) {
			for (int Column_Current = 0; Column_Current < Arena_Blocks_Y; Column_Current++) {
				GameObject Clone = Instantiate(Cell, new Vector3(0, 0, 0), Quaternion.identity);
				Clone.name = "Cell " + (Row_Current + 1) + "-" + (Column_Current + 1);
				Clone.transform.SetParent(Game_Area.transform);
				Clone.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
				Clone.GetComponent<RectTransform>().sizeDelta = new Vector3(Cell_Size, Cell_Size, 1);
				float Cell_Position_x = ((Arena_Width - Line_Thicc) / Arena_Blocks_X) * (Row_Current + .5f) - ((Game_Area.rect.width) / 2.0f) + Buffer + (Line_Thicc / 2.0f);
				float Cell_Position_y = ((Arena_Width - Line_Thicc) / Arena_Blocks_X) * (Column_Current + .5f) - (Game_Area.rect.height / 2.0f) + Buffer + (Line_Thicc / 2.0f);
				Clone.transform.localPosition = new Vector3(Cell_Position_x, Cell_Position_y, 0);
			}
		}
	}

	public void Bomb_Creation(string[] Clean_Cells, int Bomb_Quantity) {
		Bomb_Cells = new string[PlayerPrefs.GetInt("Bomb Quantity") + 1];
		for (int Current_Bombs = PlayerPrefs.GetInt("Bomb Quantity"); Current_Bombs > 0; Current_Bombs--) {
			int Current_X = Random.Range(1, Arena_Blocks_X + 1);
			int Current_Y = Random.Range(1, Arena_Blocks_Y + 1);
			Bomb_Cells[Current_Bombs] = Repetition(Current_X, Current_Y);
		}
		Game_Active = true;
		StartCoroutine(Time_Counter());
	}

	string Repetition (int Current_X, int Current_Y) { //LAST PART OF INTIAL CREATION
		string Result = "Cell " + Current_X + "-" + Current_Y;
		foreach (string Item in Bomb_Cells) {
			if (Item == Result) {
				Current_X = Random.Range(1, Arena_Blocks_X + 1);
				Current_Y = Random.Range(1, Arena_Blocks_Y + 1);
				Result = Repetition(Current_X, Current_Y);
			}
		}
		return Result;
	}

	IEnumerator Time_Counter () {
		while (Game_Active == true) {
			yield return new WaitForSecondsRealtime(1);
			if ((int.Parse(Time_Count.text) + 1) < 10) {
				Time_Count.text = "000" + (int.Parse(Time_Count.text) + 1).ToString();
			}else if ((int.Parse(Time_Count.text) + 1) < 100) {
				Time_Count.text = "00" + (int.Parse(Time_Count.text) + 1).ToString();
			}else if ((int.Parse(Time_Count.text) + 1) < 1000) {
				Time_Count.text = "0" + (int.Parse(Time_Count.text) + 1).ToString();
			}else if ((int.Parse(Time_Count.text) + 1) > 9999) {
				Time_Count.text = "9999";
			}else {
				Time_Count.text = (int.Parse(Time_Count.text) + 1).ToString();
			}
		}
	}

	public void Flag_Toggler () {
		if (Flag_Mode) {
			Flag_Mode = false;
			Flag_Toggle.sprite = Bomb_Version;
		}else {
			Flag_Mode = true;
			Flag_Toggle.sprite = Flag_Version;
		}
	}

	public void Flag_Text_Changer (int Change) {
		if (int.Parse(Flag_Count_Remaining.text) + Change < 10) {
			Flag_Count_Remaining.text = "000" + (int.Parse(Flag_Count_Remaining.text) + Change).ToString();
		}else if (int.Parse(Flag_Count_Remaining.text) + Change < 100) {
			Flag_Count_Remaining.text = "00" + (int.Parse(Flag_Count_Remaining.text) + Change).ToString();
		}else if (int.Parse(Flag_Count_Remaining.text) + Change < 1000) {
			Flag_Count_Remaining.text = "0" + (int.Parse(Flag_Count_Remaining.text) + Change).ToString();
		}else if (int.Parse(Flag_Count_Remaining.text) + Change > 9999) {
			Flag_Count_Remaining.text = "9999";
		}else {
			Flag_Count_Remaining.text = (int.Parse(Flag_Count_Remaining.text) + Change).ToString();
		}
	}

	public void Game_Over (string Cause) {
		Game_Active = false;
		if (Cause == "Death") {
			Face.sprite = Face_Dead;
			for (int Row_Current = 1; Row_Current <= Arena_Blocks_X; Row_Current++) {
				for (int Column_Current = 1; Column_Current <= Arena_Blocks_Y; Column_Current++) {
					if (GameObject.Find("Cell " + Row_Current + "-" + Column_Current).GetComponent<Cell>().Is_Bomb == true && GameObject.Find("Cell " + Row_Current + "-" + Column_Current).GetComponent<Cell>().Activated == false) {
						GameObject.Find("Cell " + Row_Current + "-" + Column_Current).GetComponent<Cell>().Activating();
					}
					Destroy(GameObject.Find("Cell " + Row_Current + "-" + Column_Current).GetComponent<UnityEngine.UI.Button>());
				}
			}
			for (int Row_Current = 1; Row_Current <= Arena_Blocks_X; Row_Current++) {
				for (int Column_Current = 1; Column_Current <= Arena_Blocks_Y; Column_Current++) {
					GameObject.Find("Cell " + Row_Current + "-" + Column_Current).GetComponent<Cell>().Activating();
				}
			}
		}else {
			Face.sprite = Face_Cool;
		}
	}

	public void Restart () {
		Application.LoadLevel("MainMenu");
	}

	public void Settings_Toggler () {
		if (Settings.active) {
			Settings.SetActive(false);
		}else {
			Settings.SetActive(true);
		}
	}

	public void Settings_Applyer () {
		if (int.Parse(Arena_Size_New.text) <= 10) {
			Warning_Text.text = "Minefield needs to be bigger!";
		}else if (int.Parse(Bomb_Amount_New.text) <= 0) {
			Warning_Text.text = "You need more bombs!";
		}else if (int.Parse(Bomb_Amount_New.text) >= int.Parse(Arena_Size_New.text)) {
			Warning_Text.text = "That is too many bombs!";
		}else {
			PlayerPrefs.SetInt("Arena Size", int.Parse(Arena_Size_New.text));
			PlayerPrefs.SetInt("Bomb Quantity", int.Parse(Bomb_Amount_New.text));
			Restart();
		}
	}
}