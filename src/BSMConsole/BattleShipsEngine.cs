namespace BSMConsole;

public class BattleShipsEngine {
	// Define the size of the board
	const int BoardSize = 10;

	// Define the symbols for the board
	const char Empty = '.';
	const char ShipChar = 'S';
	const char Hit = 'X';
	const char Miss = 'O';

	// Define the directions for placing ships
	enum Direction { Horizontal, Vertical }

	// Define a class for ships
	class Ship {
		public int Length { get; set; } // The length of the ship
		public int Row { get; set; } // The row of the ship's head
		public int Col { get; set; } // The column of the ship's head
		public Direction Dir { get; set; } // The direction of the ship
		public bool IsSunk { get; set; } // Whether the ship is sunk or not

		// A constructor that takes the length and direction of the ship
		public Ship(int length, Direction dir) {
			Length = length;
			Dir = dir;
			IsSunk = false;
		}

		// A method that checks if a given coordinate is part of the ship
		public bool Contains(int row, int col) {
			if (Dir == Direction.Horizontal) {
				return row == Row && col >= Col && col < Col + Length;
			} else // Dir == Direction.Vertical
			  {
				return col == Col && row >= Row && row < Row + Length;
			}
		}

		// A method that checks if the ship is hit by a given coordinate
		public bool IsHit(int row, int col) {
			return Contains(row, col) && Board[row, col] == Hit;
		}

		// A method that updates the status of the ship based on the board
		public void UpdateStatus() {
			IsSunk = true; // Assume the ship is sunk
			for (int i = 0; i < Length; i++) {
				int row = Dir == Direction.Horizontal ? Row : Row + i;
				int col = Dir == Direction.Vertical ? Col : Col + i;
				if (Board[row, col] != Hit) // If any part of the ship is not hit, it is not sunk
				{
					IsSunk = false;
					break;
				}
			}
		}
	}

	// Define a list of ships for each player
	static readonly List<Ship> PlayerShips = new();
	static readonly List<Ship> ComputerShips = new();

	// Define a random number generator
	static readonly Random Random = new();

	// Define a 2D array for the board
	static readonly char[,] Board = new char[BoardSize, BoardSize];

	// Define a boolean flag to indicate if the game is over
	static bool GameOver = false;

	internal static void Execute() {
		Console.WriteLine("Welcome to Battleships!");
		Console.WriteLine($"The board size is {BoardSize}x{BoardSize}.");
		Console.WriteLine("You and the computer each have 5 ships of lengths 2, 3, 3, 4 and 5.");
		Console.WriteLine($"The symbols are: empty ({Empty}), ship ({ShipChar}), hit ({Hit}), miss ({Miss}).");
		Console.WriteLine("You will take turns to fire at each other's board.");
		Console.WriteLine("The first one to sink all the opponent's ships wins.");
		Console.WriteLine("Good luck!");

		// Initialize the board with empty symbols
		InitializeBoard();

		// Place the ships for both players randomly on the board
		PlaceShips(PlayerShips);
		PlaceShips(ComputerShips);

		// Start the game loop
		while (!GameOver) {
			// Display the board
			DisplayBoard();

			// Let the player fire at the computer's board
			PlayerFire();

			// Check if the game is over
			if (GameOver)
				break;

			// Let the computer fire at the player's board
			ComputerFire();

			// Check if the game is over
			if (GameOver)
				break;
		}

		// Display the final board and the winner
		DisplayBoard();
		Console.WriteLine(GameOver ? "Game over!" : "Something went wrong!");
	}

	// A method that initializes the board with empty symbols
	static void InitializeBoard() {
		for (int i = 0; i < BoardSize; i++) {
			for (int j = 0; j < BoardSize; j++) {
				Board[i, j] = Empty;
			}
		}
	}

	// A method that places a list of ships randomly on the board
	static void PlaceShips(List<Ship> ships) {
		// Define an array of ship lengths
		int[] shipLengths = { 2, 3, 3, 4, 5 };

		// Loop through the ship lengths and create a ship for each length
		foreach (int length in shipLengths) {
			// Choose a random direction for the ship
			Direction dir = (Direction)Random.Next(2);

			// Create a ship with the given length and direction
			Ship ship = new(length, dir);

			// Choose a random position for the ship's head
			int row, col;
			do {
				row = Random.Next(BoardSize);
				col = Random.Next(BoardSize);
			} while (!IsValidPosition(row, col, ship)); // Repeat until the position is valid

			// Set the ship's head position
			ship.Row = row;
			ship.Col = col;

			// Add the ship to the list
			ships.Add(ship);

			// Mark the ship's position on the board with the ship symbol
			for (int i = 0; i < length; i++) {
				int r = dir == Direction.Horizontal ? row : row + i;
				int c = dir == Direction.Vertical ? col : col + i;
				Board[r, c] = ShipChar;
			}
		}
	}

	// A method that checks if a given position is valid for placing a ship
	static bool IsValidPosition(int row, int col, Ship ship) {
		// Check if the position is within the board boundaries
		if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
			return false;

		// Check if the position overlaps with another ship
		if (Board[row, col] == ShipChar)
			return false;

		// Check if the position leaves enough space for the rest of the ship
		if (ship.Dir == Direction.Horizontal) {
			return col + ship.Length <= BoardSize;
		} else // ship.Dir == Direction.Vertical
		  {
			return row + ship.Length <= BoardSize;
		}
	}

	// A method that displays the board to the console
	static void DisplayBoard() {
		Console.WriteLine();
		Console.WriteLine("     A B C D E F G H I J "); // Display the column labels
		Console.WriteLine("    ---------------------");
		for (int i = 0; i < BoardSize; i++) {
			Console.Write("{0,2} |", i + 1); // Display the row label
			for (int j = 0; j < BoardSize; j++) {
				char symbol = Board[i, j];
				if (symbol == ShipChar && !IsPlayerShip(i, j)) // Hide the computer's ships
				{
					symbol = Empty;
				}
				Console.Write(" {0}", symbol); // Display the symbol
			}
			Console.WriteLine(" |");
		}
		Console.WriteLine("    ---------------------");
		Console.WriteLine();
	}

	// A method that checks if a given coordinate belongs to a player's ship
	static bool IsPlayerShip(int row, int col) {
		foreach (Ship ship in PlayerShips) {
			if (ship.Contains(row, col))
				return true;
		}
		return false;
	}

	// A method that lets the player fire at the computer's board
	static void PlayerFire() {
		Console.WriteLine("Your turn to fire!");
		int row, col;
		do {
			Console.Write("Enter a coordinate (e.g. A5): ");
			string input = Console.ReadLine()?.ToUpper() ?? "X1";
			if (input.Length is < 2 or > 3 ) // Invalid input length
			{
				if (input.Length > 0 && input[0] is 'q' or 'Q') {
					GameOver = true;
					row = 0; col = 0;
					Console.WriteLine("Quit!");
					break;
				}
				Console.WriteLine("Invalid input. Try again.");
				continue;
			}
			char letter = input[0];
			char number = input[1];
			if (letter < 'A' || letter > 'J') // Invalid letter
			{
				Console.WriteLine("Invalid letter. Try again.");
				continue;
			}
			if (number < '1' || number > '9') // Invalid number
			{
				Console.WriteLine("Invalid number. Try again.");
				continue;
			}
			// Convert the letter and number to row and column indices
			row = number - '1';
			col = letter - 'A';
			if (Board[row, col] == Hit || Board[row, col] == Miss) // Already fired at this coordinate
			{
				Console.WriteLine("You already fired at this coordinate. Try again.");
				continue;
			}
			break; // Valid input
		} while (true);

		if (GameOver) {
			return;
		}

		// Fire at the coordinate and update the board
		Fire(row, col);

		// Check if the player hit or missed a ship
		if (Board[row, col] == Hit) {
			Console.WriteLine("You hit a ship!");
		} else // Board[row, col] == Miss
		  {
			Console.WriteLine("You missed!");
		}

		// Check if the player sunk a ship
		foreach (Ship ship in ComputerShips) {
			if (ship.IsHit(row, col)) {
				ship.UpdateStatus();
				if (ship.IsSunk) {
					Console.WriteLine("You sunk a {0}-cell ship!", ship.Length);
				}
				break;
			}
		}

		// Check if the player won the game
		CheckWin(PlayerShips);
	}

	// A method that lets the computer fire at the player's board
	static void ComputerFire() {
		Console.WriteLine("Computer's turn to fire!");
		int row, col;
		do {
			// Choose a random coordinate
			row = Random.Next(BoardSize);
			col = Random.Next(BoardSize);
		} while (Board[row, col] == Hit || Board[row, col] == Miss); // Repeat until the coordinate is not already fired at

		// Fire at the coordinate and update the board
		Fire(row, col);

		// Check if the computer hit or missed a ship
		if (Board[row, col] == Hit) {
			Console.WriteLine("The computer hit your ship!");
		} else // Board[row, col] == Miss
		  {
			Console.WriteLine("The computer missed!");
		}

		// Check if the computer sunk a ship
		foreach (Ship ship in PlayerShips) {
			if (ship.IsHit(row, col)) {
				ship.UpdateStatus();
				if (ship.IsSunk) {
					Console.WriteLine("The computer sunk your {0}-cell ship!", ship.Length);
				}
				break;
			}
		}

		// Check if the computer won the game
		CheckWin(ComputerShips);
	}

	// A method that fires at a given coordinate and updates the board
	static void Fire(int row, int col) {
		// Check if the coordinate contains a ship or not
		if (Board[row, col] == ShipChar) {
			// Mark the coordinate as hit
			Board[row, col] = Hit;
		} else // Board[row, col] == Empty
		  {
			// Mark the coordinate as miss
			Board[row, col] = Miss;
		}
	}

	// A method that checks if a list of ships has won the game
	static void CheckWin(List<Ship> ships) {
		// Assume the game is over
		GameOver = true;
		foreach (Ship ship in ships) {
			// Update the status of each ship
			ship.UpdateStatus();
			if (!ship.IsSunk) // If any ship is not sunk, the game is not over
			{
				GameOver = false;
				break;
			}
		}
		if (GameOver) // If the game is over, announce the winner
		{
			if (ships == PlayerShips) {
				Console.WriteLine("You win!");
			} else // ships == ComputerShips
			  {
				Console.WriteLine("You lose!");
			}
		}
	}
}
