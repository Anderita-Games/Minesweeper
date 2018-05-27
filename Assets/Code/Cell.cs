using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
	//Cell UI
	public UnityEngine.UI.Text Bomb_Quantity;
	public GameObject Flag;
	public GameObject Bomb;
	public GameObject Red_X;

	//Bomb Values
	int Bombs_Nearby;
	public bool Is_Bomb = false;
	int Pos_X = 0;
	int Pos_Y = 0;
	string[] Neighbors = new string[9];

	//Player Changable Values
	public bool Flagged = false;
	public bool Activated = false;

	//Motherland
	public GameMaster Creator;

	// Use this for initialization
	void Start () {
		Creator = GameObject.Find("Canvas").GetComponent<GameMaster>();

		string[] X_Text = new string[this.name.Length];
		string[] Y_Text = new string[this.name.Length];
		for (int Character_Count = 0; Character_Count < this.name.Length; Character_Count++) { //Get coordinates of cell
			char Character = this.name[Character_Count];
			if (char.IsNumber(Character)) {
				if (X_Text[0] == null) {
					for (int Char_Number = 0; char.IsNumber(this.name[Character_Count]); Char_Number++) {
						X_Text[Char_Number] = this.name[Character_Count].ToString();
						Character_Count++;
					}
				}else if (Y_Text[0] == null) {
					for (int Char_Number = 0; char.IsNumber(this.name[Character_Count]); Char_Number++) {
						Y_Text[Char_Number] = this.name[Character_Count].ToString();
						Character_Count++;
						if (Character_Count >= this.name.Length) {
							break;
						}
					}
				}
			}
		}
		Pos_X = int.Parse(string.Concat(X_Text));
		Pos_Y = int.Parse(string.Concat(Y_Text));

		//Finding neighbor cells
		Neighbors[0] = "Cell " + Pos_X + "-" + Pos_Y;
		Neighbors[1] = "Cell " + Pos_X + "-" + (Pos_Y + 1);
		Neighbors[2] = "Cell " + Pos_X + "-" + (Pos_Y - 1);
		Neighbors[3] = "Cell " + (Pos_X + 1) + "-" + Pos_Y;
		Neighbors[4] = "Cell " + (Pos_X - 1) + "-" + Pos_Y;
		Neighbors[5] = "Cell " + (Pos_X + 1) + "-" + (Pos_Y + 1);
		Neighbors[6] = "Cell " + (Pos_X + 1) + "-" + (Pos_Y - 1);
		Neighbors[7] = "Cell " + (Pos_X - 1) + "-" + (Pos_Y + 1);
		Neighbors[8] = "Cell " + (Pos_X - 1) + "-" + (Pos_Y - 1);
	}

	public void Pressed () {
		if (Creator.Flag_Mode == false && Flagged == false) {
			Activating();
		}else if (Creator.Flag_Mode == true) {
			Flagging();
		}
	}

	public void Self_Bomb_Check () {
		for (int Current_Bombs = PlayerPrefs.GetInt("Bomb Quantity"); Current_Bombs > 0; Current_Bombs--) {
			if (Creator.Bomb_Cells[Current_Bombs] == this.name) {
				Is_Bomb = true;
			}
		}
	}

	public void Activating () {
		if (Activated == false) {
			Activated = true;
			if (Creator.Bomb_Cells.Length == 0) {
				Creator.Bomb_Creation(Neighbors, PlayerPrefs.GetInt("Bomb Quantity"));
			}

			Self_Bomb_Check();

			Bombs_Nearby = Neighbor_Bomb_Check();

			if (Creator.Game_Active == true) {
				if (Is_Bomb == true) {
					this.GetComponent<UnityEngine.UI.RawImage>().color = Color.red;
					GameObject.Find("Canvas").GetComponent<GameMaster>().Game_Over("Death");
					Bomb.SetActive(true);
				}else {
					this.GetComponent<UnityEngine.UI.RawImage>().color = Color.white;
					if (Bombs_Nearby == 0) { //Expose 0 value Neighbors
						Bomb_Quantity.text = "";
						for (int i = 1; i <= 8; i++) {
							if (GameObject.Find(Neighbors[i]) == true && GameObject.Find(Neighbors[i]).GetComponent<Cell>().Activated == false) {
								GameObject.Find(Neighbors[i]).GetComponent<Cell>().Activating();
							}
						}
					}else {
						Bomb_Quantity.text = Bombs_Nearby.ToString();
					}
				}
			}else {
				if (Flagged && Is_Bomb == false) {
					this.GetComponent<UnityEngine.UI.RawImage>().color = Color.white;
					Flag.SetActive(false);
					Bomb.SetActive(true);
					Red_X.SetActive(true);
				}else if (Is_Bomb == true && Flagged == false) {
					this.GetComponent<UnityEngine.UI.RawImage>().color = Color.white;
					Bomb.SetActive(true);
				}
			}
			Creator.Cells_Activated++;
			Destroy(this.GetComponent<UnityEngine.UI.Button>());
		}
	}

	void Flagging () {
		if (int.Parse(Creator.Flag_Count_Remaining.text) > 0) {
			Self_Bomb_Check();
			if (Flagged == false) {
				Flagged = true;
				Flag.SetActive(true);
				if (Is_Bomb == true) {
					Creator.Bombs_Flagged++;
				}
				Creator.Flag_Text_Changer(-1);
			}
		}else {
			Flagged = false;
			Flag.SetActive(false);
			if (Is_Bomb == true) {
				Creator.Bombs_Flagged--;
			}
			Creator.Flag_Text_Changer(1);
		}
	}

	int Neighbor_Bomb_Check () { //Finding quantity of nearby bombs
		int Result = 0;
		foreach (string Neighbor in Neighbors) {
			foreach (string Bomb in Creator.Bomb_Cells) {
				if (Neighbor == Bomb) {
					Result++;
				}
			}
		}
		return Result;
	}
}
