#pragma once
#include "Chess.h"
#include "Move.h"
#include <vector>
#include <map>
#include <limits>
#include <unordered_map>
using namespace std;

#include <chrono>
#include <future>
//minimax-funktion palauttama arvo.
class MinimaxValue
{
public:
	//MinimaxValue() {};
	MinimaxValue(float value, Move move) : _value(value), _move(move){}
	float _value;
	Move _move;
	string str() const { return _move.ToString(); } // Accessor function to get the move as a string


};



class Position
{
private:
	//piece-square table:n m‰‰ritys
	static const int pawnTable[8][8];
	static const int knightTable[8][8];
	static const int bishopTable[8][8];
	static const int rookTable[8][8];
	static const int queenTable[8][8];
	static const int kingTable[8][8];
	static const int centerControlTable[8][8];

	mutable unordered_map<int, vector<Move>> moveCache;

public:
	void EmptyGrid();

	void MakeMove(const Move& m);

	//l‰ksy tulosta lauta ascii-grafiikkana: tehty, funktio Position.cpp tiedostossa.
	void Print() const;

	void AllRawMovesInDirection(int player, vector<Move>& moves) const;

	void RookRawMoves(int row, int line, int player, vector<Move>& moves) const;
	void BishopRawMoves(int row, int line, int player, vector<Move>& moves) const;
	void KnightRawMoves(int row, int line, int player, vector<Move>& moves) const;
	void QueenRawMoves(int row, int line, int player, vector<Move>& moves) const;
	void KingRawMoves(int row, int line, int player, vector<Move>& moves) const;
	void PawnRawMoves(int row, int line, int player, vector<Move>& moves) const;

	int CountPieces() const;

	void PawnPromotion(int row, int line, int row_change, int line_change,int player, vector<Move>& moves) const;

	void EnPassant(int row, int linw, int row_change, int line_change, int player, vector<Move>& moves) const;
	
	void RawMovesInDirection(int row, int line, int row_change, int line_change, int player, int max_Steps, bool can_Strike, bool must_Strike,bool can_promote, vector<Move>& moves, bool en_passant_move) const;


	//laudan nappulat. Indeksointi [rivi][linja],esim.
	//
	//[0][0] : vasen yl‰nurkka ("a8")
	//[7][0] : vasen alanurkka ("a1")
	//[7][7] : oikea yl‰nurkka ("h1")

	int _board[8][8] = {
		{bR,bN,bB,bQ,bK,bB,bN,bR},
		{bP,bP,bP,bP,bP,bP,bP,bP},
		{NA,NA,NA,NA,NA,NA,NA,NA},
		{NA,NA,NA,NA,NA,NA,NA,NA},
		{NA,NA,NA,NA,NA,NA,NA,NA},
		{NA,NA,NA,NA,NA,NA,NA,NA},
		{wP,wP,wP,wP,wP,wP,wP,wP},
		{wR,wN,wB,wQ,wK,wB,wN,wR}
	};

	int _moveTurn = WHITE;


	/**
	* \brief Goes through the squares on the board until right piece is find.
	*/
	void FindPiece(int piece,int& row,int& line) const
	{
		for(int i = 0; i < 8; ++i)
			for (int j = 0; j < 8; ++j)
			{
				if (piece == _board[i][j])
				{
					row = i;
					line = j;
					return;
				}
			}
	}
	//kirjanpito,onko kuningas tai torni liikkunut
	//asetetaan tarpeen mukaan falseksi.
	bool _whiteShortCastleAllowed = true;
	bool _whiteLongCastleAllowed = true;
	bool _blackShortCastleAllowed = true;
	bool _blackLongCastleAllowed = true;

	//ohestalyˆnti
	int _doubleStepOnLine = -1;

	/**
	* \brief Reserves memory for 200 vector<Move> threatening_moves.
	* \brief Checks all opponents moves and adds then to vector<Move> threatening_moves using \ref AllRawMovesInDirection.
	* \brief Checks if target square is found on vector<Move> threatening_moves.
	* \brief Returns true if found.
	*/
	bool IsThreatened(int row, int line, int threatening_player) const
	{
		// Generoidaan uhkaavan pelaajan raakasiirrot
		vector<Move> threatening_moves;
		threatening_moves.reserve(200);

		AllRawMovesInDirection(threatening_player, threatening_moves);

		//tutkitaan, onko jokin raakasiirron loppukoordinaatit == rivi,linja
		//jos on,palauta true (muuten false)

		// Tutkitaan, onko jokin raakasiirron loppukoordinaatit annetussa ruudussa
		for (const Move& move : threatening_moves)
		{
			//cout << row << line << "threatening line";

			if (move._dest_row == row && move._dest_line == line)
			{
				return true;
			}
		}
		
		// Jos ei lˆytynyt uhkaavia siirtoja annetusta ruudusta
		return false;
		
	}

	/**
	* \brief Reserves memory for 300 moves in vector<Move> rawMoves.
	* \brief Checks current players allowed moves \ref AllRawMovesInDirection.
	* \brief Checks if castling is possible \ref giveCastlings.
	* \brief Loops through every move on vector<Move>& moves.
	* \brief Makes test positions for moves and checks if king(\ref find_piece) is in check after the move (\ref is_square_threatened).
	* \brief If king is not in check, move is added to vector<Move>& moves.
	*/

	//siirtogeneraattori
	void MoveGen(vector<Move>& moves) const
	{
		int king = _moveTurn == WHITE ? wK : bK;
		int player = _moveTurn;
		int opponent = Opponent(player);

		//luodaan pelaajan raakasiirrot. Osa siirroista saattaa
		//j‰tt‰‰ kuninkaan uhatuksi
		vector<Move> rawMoves;
		rawMoves.reserve(300);

		AllRawMovesInDirection(player, rawMoves);
		Castle(player, rawMoves);
		//testataan jokainen raakasiirto
		for (Move& rm : rawMoves)
		{
			//luodaan kopio nykyisest‰ asemasta
			Position testPos = *this;

			//pelataan raakasiirto testiasemassa
			testPos.MakeMove(rm);

			//etsit‰‰n kuningas 
			int row, line;
			testPos.FindPiece(king, row, line);

			//j‰ikˆ kuningas shakkiin,jos ei, lis‰t‰‰n siirto.
			if (!testPos.IsThreatened(row, line, opponent))
			{
				moves.push_back(rm);
			}
			
		}
	}

	/**
	* \brief Score evaluation:
	* \brief White gets checkmate 1000000
	* \brief Draw 0
	* \brief Black gets checkmate -1000000
	* \brief Function is called when there are no moves to make in the vector<Move>& moves.
	*/

	
	float FinalScore(float _value) const
	{
		float value = _value;

		if (_moveTurn == WHITE)
		{
			//etsit‰‰n kuningas
			int row, line;
			FindPiece(wK, row, line); //t‰lle parannus staattinen sijainnin seuranta?

			//onko valkea kuningas uhattu?
			if (IsThreatened(row, line, BLACK))
			{
				return -1000000 - value ; // musta on tehnyt matin
				
			}
			else
			{
				return 0; //patti (tasapeli)
			}


		}
		else 
		{
			//etsit‰‰n kuningas
			int row, line;
			FindPiece(bK, row, line); //t‰lle parannus staattinen sijainnin seuranta?

			//onko musta kuningas uhattu?
			if (IsThreatened(row, line, WHITE))
			{
				return 1000000 + value; // valkea tehnyt matin
			}
			else
			{
				return 0; //patti
			}
		}
	}

	/**
	* \brief Scores a chess position heuristically.
	*/
	float Evaluate() const
	{
		//return 1.0f * Material() + 0.3f * Mobility() + 0.02f * EvaluatePosition() + 0.2f * CenterControl();
		return 1.0f * Material() + 0.4f * Mobility() + 0.05f * EvaluatePosition() + 0.1f * CenterControl();


		
	}



	/**
	* \brief Uses multiple threads to call minimax.
	*/
	std::future<MinimaxValue> MinimaxAsync(int depth, float alpha, float beta, int max_depth) const {
		return std::async(std::launch::async, [=]() {
			return Minimax(depth, alpha, beta, max_depth);
			});
	}

	/**
	* \brief Uses Alpha-Beta to find best possible move.
	* \brief Calls itself until max depth is reached.
	*/
	MinimaxValue Minimax(int depth, float alpha, float beta, int max_depth) const {
		// Generate possible moves
		vector<Move> moves;
		MoveGen(moves);

		float best_value;
		Move best_move;

		// Check for the end of the game
		if (moves.empty()) {
			return MinimaxValue(FinalScore(static_cast<float>(depth)), Move()); // Return the final score
		}
		if (moves.empty() || depth == 0 || depth > max_depth) {
			// Base case: evaluate the position
			return MinimaxValue(Evaluate(), Move());
		}
		
		if (_moveTurn == WHITE) {
			best_value = std::numeric_limits<float>::lowest();
			for (Move& m : moves) {
				Position newPos = *this;
				newPos.MakeMove(m);
				auto future_value = newPos.MinimaxAsync(depth - 1, alpha, beta, max_depth); // Call asynchronously
				MinimaxValue value = future_value.get(); // Wait for the result
				if (value._value > best_value) {
					best_value = value._value;
					best_move = m;
				}
				alpha = std::max(alpha, best_value);
				if (beta <= alpha) {
					break;  // Beta cutoff
				}
			}
		}
		else {
			best_value = std::numeric_limits<float>::max();
			for (Move& m : moves) {
				Position newPos = *this;
				newPos.MakeMove(m);
				auto future_value = newPos.MinimaxAsync(depth - 1, alpha, beta, max_depth); // Call asynchronously
				MinimaxValue value = future_value.get(); // Wait for the result
				if (value._value < best_value) {
					best_value = value._value;
					best_move = m;
				}
				beta = std::min(beta, best_value);
				if (beta <= alpha) {
					break;  // Alpha cutoff
				}
			}
		}
		
		
		
		return MinimaxValue(best_value, best_move);
	}


	/**
	* \brief Calculates the material balance (value of white pieces - value of black pieces)
	* \brief Piece value:
	* \brief Pawn 1
	* \brief Knight 3
	* \brief Bishop 3
	* \brief Rook 5
	* \brief Queen 9
	*/
	float Material() const
	{
		//liitet‰‰n nappulatyyppeihin niiden arvot.
		static map<int, float> piece_values = {
			{wP, 1.0f},{wN, 3.0f},{wB, 3.0f},{wR, 5.0f},{wQ, 9.0f},
			{wK,0.0f},{bP,-1.0f},{bN,-3.0f},{bB,-3.0f},{bR,-5.0f},
			{bQ,-9.0f},{bK,-0.0f},{NA, 0.0f}
		};

		float value = 0;
		for (int row = 0; row < 8; row++)
		{
			for (int line = 0; line < 8; line++)
			{
				int cPiece = _board[row][line];
				value += piece_values[cPiece];
			}

		}
		return value;

	}

	/**
	* \brief Returns the difference between the numbers of white and black raw moves.
	*/
	float Mobility() const {
		// Tarkistetaan, onko siirrot jo laskettu ja tallennettu v‰limuistiin
		if (moveCache.find(WHITE) == moveCache.end() || moveCache.find(BLACK) == moveCache.end()) {
			std::vector<Move> white_moves;
			std::vector<Move> black_moves;

			// Generoidaan siirrot tarvittaessa ja tallennetaan ne v‰limuistiin
			AllRawMovesInDirection(WHITE, white_moves);
			AllRawMovesInDirection(BLACK, black_moves);
			moveCache[WHITE] = white_moves;
			moveCache[BLACK] = black_moves;
		}

		// Palautetaan valkean ja mustan raakasiirtojen lukum‰‰rien erotus v‰limuistista
		return static_cast<float>(moveCache[WHITE].size()) - static_cast<float>(moveCache[BLACK].size());
	}

	/**
	* \brief Checks player color.
	* \brief Checks if king would move over threatened squares and if pieces still have not moved.
	* \brief Adds move to the vector<Move>& moves.
	*/
	void Castle(int player, vector<Move>& moves) const
	{
		if (player == WHITE)
		{
			//kuningas ja torni eiv‰t liikkuneet, f1 tyhj‰,g1 tyhj‰,
			//kuningas ei shakissa (e1 ei uhattu), f1 ei uhattu.
			if (_whiteShortCastleAllowed && _board[7][5] == NA && _board[7][6] == NA &&
				!IsThreatened(7, 4, BLACK) && !IsThreatened(7, 5, BLACK))
			{
				//lis‰t‰‰n lyhyt linnoitus
				moves.push_back(Move(7,4,7,6, NA));

				//tai: moves.push_back(Move("e1g1"));
			}

			//tarkista if lauseke!!
			if (_whiteLongCastleAllowed && _board[7][3] == NA && _board[7][2] == NA && _board[7][1] == NA && !IsThreatened(7, 4, BLACK) &&
					!IsThreatened(7, 3, BLACK) && !IsThreatened(7, 2, BLACK))
			{
				//lis‰t‰‰n lyhyt linnoitus
				moves.push_back(Move(7, 4, 7, 2, NA));

				//tai: moves.push_back(Move("e1b1"));
			}
		}
		else  
		{
			//mustan linnoitukset
			if (_blackShortCastleAllowed && _board[0][5] == NA && _board[0][6] == NA &&
				!IsThreatened(0, 4, WHITE) && !IsThreatened(0, 5, WHITE))
			{
				//lis‰t‰‰n lyhyt linnoitus
				moves.push_back(Move(0, 4, 0, 6, NA));
			}

			//tarkista if lauseke!!
			if (_blackLongCastleAllowed && _board[0][3] == NA && _board[0][2] == NA && _board[0][1] == NA && !IsThreatened(0, 4, WHITE) &&
				!IsThreatened(0, 3, WHITE) && !IsThreatened(0, 2, WHITE))
			{
				//lis‰t‰‰n pitk‰ linnoitus
				moves.push_back(Move(0, 4, 0, 2, NA));
			}
		}
	}
	
	/**
	* \brief Checks each pieces positional values.
	* \brief returns which player is in a better position. Positive for White, Negative for Black.
	*/
	float EvaluatePosition() const
	{
		float eval = 0;
		

		for (int row = 0; row < 8; row++)
		{
			for (int line = 0; line < 8; line++)
			{
				int piece = _board[row][line];
				if (piece != NA)
				{
					int value = 0;
					switch (piece)
					{
					case wP:
						value = pawnTable[row][line];
						break;
					case wN:
						value = knightTable[row][line];
						break;
					case wR:
						value = rookTable[row][line];
						break;
					case wB:
						value = bishopTable[row][line];
						break;
					case wQ:
						value = queenTable[row][line];
						break;
					case wK:
						value = kingTable[row][line];
						break;

					case bP:
						value = -pawnTable[7 - row][line];
						break;
					case bN:
						value = -knightTable[7 - row][line];
						break;
					case bR:
						value = -rookTable[7 - row][line];
						break;
					case bB:
						value = -bishopTable[7 - row][line];
						break;
					case bQ:
						value = -queenTable[7 - row][line];
						break;
					case bK:
						value = -kingTable[7 - row][line];
						break;
					}
					eval += value;
				}
			}
		}
		return eval;
	}

	/**
	* \brief Checks each piece for their velue being closer to the middle.
	* \brief returns which player is in a better position. Positive for White, Negative for Black.
	*/
	float CenterControl() const
	{
		float eval = 0;

		for (int row = 0; row < 8; row++)
		{
			for (int line = 0; line < 8; line++)
			{
				int piece = _board[row][line];

				if (piece != NA)
				{
					int value = 0;
					switch (piece)
					{
					case wP:
						value = centerControlTable[row][line];
						break;
					case wQ:
						value = centerControlTable[row][line];
						break;
					case wK:
						value = centerControlTable[row][line];
						break;
					case wR:
						value = centerControlTable[row][line];
						break;
					case wN:
						value = centerControlTable[row][line];
						break;
					case wB:
						value = centerControlTable[row][line];
						break;

					case bP:
						value = -centerControlTable[7 - row][line];
						break;
					case bQ:
						value = -centerControlTable[7 - row][line];
						break;
					case bK:
						value = -centerControlTable[7 - row][line];
						break;
					case bR:
						value = -centerControlTable[7 - row][line];
						break;
					case bN:
						value = -centerControlTable[7 - row][line];
						break;
					case bB:
						value = -centerControlTable[7 - row][line];
						break;
					}
					eval += value;
				}
			}
		}

		return eval;
	}

	
};
