**Tic-Tac-Toe Game With Unity**
===========================

## 📋 Content
- [Overview](#-overview)
- [Installation](#-installation)
- [Features](#-features)
- [Game Mechanics](#-game-mechanics)
- [Code Details](#-code-details)
- [Key Methods](#-key-methods)
- [License](#-license)


📖 **Overview**
---------------
This repository contains a Unity-based Tic-Tac-Toe game where the player competes against a computer AI. The AI utilizes advanced algorithms including Minimax with Alpha-Beta Pruning for optimized decision-making. The game features a timer, score tracking, and sound effects.

You can read more about the algorithms used in the following links: 
- [Is Minimax Algorithm Hard?](https://locall.host/is-minimax-algorithm-hard/)
- [Minimax Algorithm in Game Theory - GeeksforGeeks](https://www.geeksforgeeks.org/minimax-algorithm-in-game-theory-set-1-introduction/)
- [Alpha-Beta Pruning - Stanford](https://cs.stanford.edu/people/eroberts/courses/soco/projects/2003-04/intelligent-search/alphabeta.html)
- [Alpha-Beta Pruning - Wikipedia](https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning)
- [AI Alpha-Beta Pruning - JavaTpoint](https://www.javatpoint.com/ai-alpha-beta-pruning)

## 🎥 Demo Video
Watch the demo video on [YouTube](https://youtu.be/IISnz4yb6ng).
<iframe width="560" height="315" src="https://youtu.be/IISnz4yb6ng" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<!-- Watch the demo video [here](Demo%20Video/2024-09-12%2022-46-11.mp4). -->

🚀 **Installation**
-------------------
- Download the files from the release depending on you operating system. Extract the file with WinRar, 7Zip, WinZip or some .rar files.
- Open the folder and run the starting file

🛠️ **Features**
--------------------------
- **AI with Minimax Algorithm**: The AI uses the Minimax algorithm combined with Alpha-Beta Pruning to make optimal moves.
- **Adaptive Difficulty**: The AI adjusts its strategy based on player behavior, learning from previous moves.
- **Timer**: Players have a limited time to make their move, with penalties for running out of time.
- **Score Tracking**: The game tracks wins, losses, and draws for both the player and AI.
- **Sound Effects**: Includes sounds for winning, losing, making a move, and when time is running out.
- **Rematch Option**: Players can initiate a rematch after the game ends.

💡 **Game Mechanics**
------------
- **Player vs. AI**: The player takes turns with the AI to place Xs and Os on a 3x3 grid.
- **AI Strategy**: The AI evaluates potential moves using the Minimax algorithm with Alpha-Beta Pruning for efficiency. It also considers frequent moves and blocking strategies.
- **Timer**: Each turn is limited to 10 seconds. If the player runs out of time, the AI wins.
- **Sound Effects**: Different audio cues are used to enhance the gameplay experience, including winning, losing, and move sounds.

💻 **Code Details**
------------
*GameController Script*

The main script controlling the game logic is GameController.cs. This script manages:
- **Game Initialization**: Sets up the game board and UI elements.
- **Player and AI Moves**: Handles player input and AI decisions.
- **Win/Loss Conditions**: Checks for game outcomes and updates the result.
- **Timer**: Manages the countdown and triggers events when time is up.
- **Score Tracking**: Updates and displays scores based on game results.
- **AI Decision Making**: Implements Minimax with Alpha-Beta Pruning and other optimizations like board control and tracking player frequent used moves.

🔑 **Key Methods**
------------
- GameInitialize(): Sets up the game state and UI elements.
- TicTacToePlayableButtons(int x, int y): Handles the player's move and checks for win conditions.
- ComputerMove(): Executes the AI's move using various strategies.
- MiniMax(): The core algorithm for decision-making, including Alpha-Beta Pruning.
- AdjustedEvaluateBoard(): Evaluates board states to guide the AI's decisions.
- CheckWinner(): Determines if there's a winner or a draw.

📜 **License** 
--------------
This project is licensed under the MIT License - see the LICENSE file for details.
