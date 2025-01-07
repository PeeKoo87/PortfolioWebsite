#include "Position.h"
#include <iostream>
#include <vector>
#include <string>


using namespace std;



void Position::EmptyGrid()
{
	//aseta laudan jokaiseen ruutuun NA
	for (int row = 0; row < 8; ++row)
	{
		for (int line = 0; line < 8; ++line)
		{
			_board[row][line] = NA;
		}
	}
}

void Position::Print() const
{
	// tulostetaan merkit

	cout << "   +---+---+---+---+---+---+---+---+\n";

	// iteroidaan rivit
	for (int i = 0; i < 8; ++i) {
		// tulostetaan rivi numerot
		cout << 9 - (i + 1) << "  |";

		// iteroidaan linjat
		for (int j = 0; j < 8; ++j) {
			// tulostetaan nappulat asemilleen
			char piece = _board[i][j];
			cout << " " << piece << " |";
		}
		cout << "\n   +---+---+---+---+---+---+---+---+";
		// siirrytään seuraavalle riville
		cout << "\n";
	}

	// tulostetaan tyhjä rivi laudan alle
	//cout << "   +---+---+---+---+---+---+---+---+\n";
	cout << "     A   B   C   D   E   F   G   H\n";
	cout << "\n";
}

/**
 * \brief Reserves enough space for 300 moves in vector<Move>& moves.
 * \brief Goes through each square on the board
 * \brief If the piece is the same color as who's turn it is, RawMoves checks are called.
 * \brief \ref RookRawMoves \ref QueenRawMoves \ref KnightRawMoves \ref BishopRawMoves \ref KingRawMoves \ref PawnRawMoves
 */

void Position::AllRawMovesInDirection(int player, vector<Move>& moves) const
{
	moves.reserve(300);

	for (int row = 0; row < 8; ++row)
	{
		for (int line = 0; line < 8; ++line)
		{
			int cPiece = _board[row][line];

			//jos tyhjä ruutu
			if (cPiece == NA)
				continue;

			//jos vastustajan nappula
			if (PieceColor(cPiece) != player)
				continue;

			//haetaan nappulan raakasiirrot
			switch (cPiece)
			{
			case wR: case bR:
				RookRawMoves(row, line, player, moves);
				break;

			case wQ: case bQ:
				QueenRawMoves(row, line, player, moves);
				break;

			case wN: case bN:
				KnightRawMoves(row, line, player, moves);
				break;

			case wB: case bB:
				BishopRawMoves(row, line, player, moves);
				break;

			case wK: case bK:
				KingRawMoves(row, line, player, moves);
				break;

			case wP: case bP:
				PawnRawMoves(row, line, player, moves);
				break;
			}
		}
	}
	/*
	// Tulosta kerätyt siirrot - debug
	cout << "collected moves: ";
	for (const auto& move : moves)
	{
		cout << move.ToString() << " ";
	}
	cout << endl;
	*/
}

/**
 * \brief Checks if piece steps are still under max steps per turn.
 * \brief	Checks if move stays on board and if not, break out of the loop.
 * \brief	Checks if moving to empty square.
 * \brief		Checks if piece needs to capture to move to target square and ends loop if so.
 * \brief		Checks if piece promotes on the target square.
 * \brief			Uses \ref PawnPromotion to add correct moves.
 * \brief		Adds move to vector<Move>& moves.
 * \brief	Checks if piece is the same color as in the target square and breaks if so.
 * \brief	Checks if piece can capture in the target direction.
 * \brief		Adds move to vector<Move>& moves.
 * \brief	Checks if piece can promote on the target square.
 * \brief		Uses \ref PawnPromotion to add correct moves.
 */

void Position::RawMovesInDirection(int row, int line, int row_change, int line_change, int player, int max_Steps, bool can_Strike, bool must_Strike, bool can_promote, std::vector<Move>& moves, bool en_passant_move = false) const
{
	moves.reserve(300);
	int row_now = row;
	int line_now = line;
	int steps = 0;

	while (steps < max_Steps)
	{
		row_now += row_change;
		line_now += line_change;

		//tarkistetaanko onko siirto ulkona laudalta
		if (row_now < 0 || row_now > 7 || line_now < 0 || line_now > 7)
		{
			break;
		}

		//tarkistetaan onko ruutu tyhjä
		if (_board[row_now][line_now] == NA)
		{
			if (must_Strike)
			{
				steps++;
				break;
			}
			if (can_promote)
			{
				PawnPromotion(row, line, row_now, line_now, player, moves);
			}
			
			else {
				moves.push_back(Move(row, line, row_now, line_now, NA));
			}

			steps++;
			continue;
		}

		//tarkistetaan törmätäänkö omaan nappulaan
		if (PieceColor(_board[row_now][line_now]) == player)
		{
			break;
		}

		//lyödään vastustajan nappula
		if (can_Strike)
		{

			if (can_promote)
			{
				PawnPromotion(row, line,row_now,line_now,player,moves);
			}
			
			else {
				moves.push_back(Move(row, line, row_now, line_now, NA));
			}

			
		}

		break;

	}
}

#pragma region Pieces

/**
 * Moves are found by using \ref RawMovesInDirection.
 */
void Position::RookRawMoves(int row, int line, int player, vector<Move>& moves) const
{
	RawMovesInDirection(row, line, -1, 0, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, 1, 0, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, 0, -1, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, 0, 1, player, 7, true, false, false, moves);
}

/**
 * Moves are found by using \ref RawMovesInDirection.
 */

void Position::BishopRawMoves(int row, int line, int player, vector<Move>& moves) const
{
	RawMovesInDirection(row, line, -1, -1, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, -1, 1, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, 1, -1, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, 1, 1, player, 7, true, false, false, moves);
}

/**
 * Moves are found by \ref RawMovesInDirection.
 */
void Position::KnightRawMoves(int row, int line, int player, vector<Move>& moves) const
{
	//liike + vasemmalle lyönti
	RawMovesInDirection(row, line, -2, -1, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, -1, 2, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, 2, 1, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, -1, -2, player, 1, true, false, false, moves);

	//liike + oikealle lyönti
	RawMovesInDirection(row, line, -2, 1, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, 1, 2, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, 2, -1, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, 1, -2, player, 1, true, false, false, moves);
}

/**
 * Moves are found by \ref RawMovesInDirection.
 */
void Position::QueenRawMoves(int row, int line, int player, vector<Move>& moves) const
{
	//pysty ja vaaka suunnat
	RawMovesInDirection(row, line, -1, 0, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, 1, 0, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, 0, -1, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, 0, 1, player, 7, true, false, false, moves);
	//viistot
	RawMovesInDirection(row, line, -1, -1, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, -1, 1, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, 1, -1, player, 7, true, false, false, moves);
	RawMovesInDirection(row, line, 1, 1, player, 7, true, false, false, moves);
}

/**
 * Moves are found by \ref RawMovesInDirection.
 */
void Position::KingRawMoves(int row, int line, int player, vector<Move>& moves) const
{
	//pysty ja vaaka suunnat
	RawMovesInDirection(row, line, -1, 0, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, 1, 0, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, 0, -1, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, 0, 1, player, 1, true, false, false, moves);
	//viistot
	RawMovesInDirection(row, line, -1, -1, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, -1, 1, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, 1, -1, player, 1, true, false, false, moves);
	RawMovesInDirection(row, line, 1, 1, player, 1, true, false, false, moves);


}

/**
 * \brief Checks which players turn it is.
 * \brief Moves are found by using \ref RawMovesInDirection and \ref EnPassant.
 * \brief Checks if pawn is one step from promotion.
 * \brief Checks normal moves and captures
 * \brief Checks EnPassant
 */
void Position::PawnRawMoves(int row, int line, int player, vector<Move>& moves) const
{

	if (player == WHITE)
	{

		if (row == 1) // korotettavat siirrot
		{


			RawMovesInDirection(row, line, -1, 1, player, 1, true, true, true, moves);
			RawMovesInDirection(row, line, -1, -1, player, 1, true, true, true, moves);
			RawMovesInDirection(row, line, -1, 0, player, 1, false, false, true, moves);

		}
		else
		{
			if (row == 6)
				RawMovesInDirection(row, line, -1, 0, player, 2, false, false, false, moves);
			else
				RawMovesInDirection(row, line, -1, 0, player, 1, false, false, false, moves);

			RawMovesInDirection(row, line, -1, 1, player, 1, true, true, false, moves);
			RawMovesInDirection(row, line, -1, -1, player, 1, true, true, false, moves);
		}
		if (row == 3) {
			//en passant siirrot
			EnPassant(row, line, -1, -1, player, moves);
			//RawMovesInDirection(row, line, -1, 1, player, 1, false, false, false, moves, true);
		}
			
	}

	if (player == BLACK)
	{
		if (row == 6) // korotettavat siirrot
		{


			RawMovesInDirection(row, line, 1, 1, player, 1, true, true, true, moves);
			RawMovesInDirection(row, line, 1, -1, player, 1, true, true, true, moves);
			RawMovesInDirection(row, line, 1, 0, player, 1, false, false, true, moves);

		}
		else 
		{
			if (row == 1)
				RawMovesInDirection(row, line, 1, 0, player, 2, false, false, false, moves);
			else
				RawMovesInDirection(row, line, 1, 0, player, 1, false, false, false, moves);

			RawMovesInDirection(row, line, 1, 1, player, 1, true, true, false, moves);
			RawMovesInDirection(row, line, 1, -1, player, 1, true, true, false, moves);
		}
		if (row == 4) {
			//en passant siirrot
			EnPassant(row, line, 1, -1, player, moves);
			//RawMovesInDirection(row, line, 1, 1, player, 1, false, false, false, moves, true);
			
		}
			
		
	}

	
}


/**
* \brief Checks for the number of pieces on board, used for depth incrementation in AIMove within main.
*/
int Position::CountPieces() const {
	int count = 0;
	int BOARD_SIZE = 8;
	for (int row = 0; row < BOARD_SIZE; ++row) {
		for (int col = 0; col < BOARD_SIZE; ++col) {
			if (_board[row][col] != NA) {
				++count;
			}
		}
	}
	return count;
}


/**
 * \brief First checks which color piece is being promoted.
 * \brief Adds promotion move to vector<Move>& moves for each promotion option. (Queen, Rook, Knight and Bishop)
 */
void Position::PawnPromotion(int row, int line, int row_change, int line_change, int player, vector<Move>& moves) const
{
	int promoPieces[4]{};
	if (player == WHITE)
	{
		promoPieces[0] = wQ;
		promoPieces[1] = wR;
		promoPieces[2] = wN;
		promoPieces[3] = wB;
	}
	else
	{
		promoPieces[0] = bQ;
		promoPieces[1] = bR;
		promoPieces[2] = bN;
		promoPieces[3] = bB;
	}
	for (int i = 0; i < 4; ++i)
	{
		int promo_piece = promoPieces[i];
		moves.push_back(Move(row, line, row_change, line_change, promo_piece));
	}
}

/**
 * \brief Checks if a pawn has done a double move and checks if white or black piece would end on a right line.
 * \brief Checks if the move square is empty.
 * \brief Checks piece is still on board (redundant) and checks if the En Passant capture piece on left or right is of the opposite color.
 * \brief Adds move to vector<Move>& moves.
 */
void Position::EnPassant(int row, int line, int row_change, int line_change, int player, std::vector<Move>& moves) const
{
	//cout << row << " " << row_change << " " << row + row_change << "\n";

	// Check if en passant is possible
	//if (_doubleStepOnLine != -1 && ((_moveTurn == WHITE && _doubleStepOnLine == 3) || (_moveTurn == BLACK && _doubleStepOnLine == 4)))
	if (_doubleStepOnLine != -1 && ((_moveTurn == WHITE && row + row_change == 2) || (_moveTurn == BLACK && row + row_change == 5)))
	{
		//cout << "part1" << "\n";
		// Check if the target square for en passant capture is empty
		if (_board[row + row_change][_doubleStepOnLine] == NA)
		{
			//cout << "part2" << "\n";
			// Check for en passant to the right
			if (line + 1 < 8 && _board[row][line + 1] == (player == WHITE ? bP : wP))
			{
				//cout << "part3 right" << "\n";
				moves.push_back(Move(row, line, row + row_change, _doubleStepOnLine, NA));

			}
			// Check for en passant to the left
			if (line - 1 >= 0 && _board[row][line - 1] == (player == WHITE ? bP : wP))
			{
				//cout << "part3 left" << "\n";
				moves.push_back(Move(row, line, row + row_change, _doubleStepOnLine, NA));

			}
		}
	}
}
#pragma endregion

/**
 * \brief Chooses a piece based on Moves m start_row and start_line.
 * \brief Checks if the move has a promotion flag.
 * \brief Sets starting square as empty.
 * \brief Sets noPassant bool to False to prevent double capturing when doing a normal capture.
 * \brief Updates noPassant to allow capturing behind the piece when moving a pawn.
 * \brief Puts moving piece to the target square.
 */

//tekee annetun siirron laudalla. voidaan olettaa että, siirto on laillinen
void Position::MakeMove(const Move& m)
{
	//otetaan alkuruudussa oleva nappula talteen
	int cPiece = _board[m._start_row][m._start_line];

	if (m._pieceToPromote != NA)
	{
		cPiece = m._pieceToPromote;
	}
	//tyhjennetään alkuruutu
	_board[m._start_row][m._start_line] = NA;
	//en passant flag
	bool noPassant = false;
	if (_board[m._dest_row][m._dest_line] != NA)
	{
		noPassant = true;
	}

	// Tarkistetaan ohestalyönti
	if (cPiece == wP && m._start_line != m._dest_line && _board[m._dest_row][m._dest_line] == NA)
	{
		// Poista vastustajan sotilas ohestalyönnin kohteesta
		_board[m._start_row][m._dest_line] = NA;
	}
	else if (cPiece == bP && m._start_line != m._dest_line && _board[m._dest_row][m._dest_line] == NA)
	{
		// Poista vastustajan sotilas ohestalyönnin kohteesta
		_board[m._start_row][m._dest_line] = NA;
	}

	//sijoitetaan loppuruutuun alkuperäinen nappula.
	_board[m._dest_row][m._dest_line] = cPiece;
	

	/**
	 * Checks if the chosen piece is a king
	 * Checks if the king is making a move that is only possible by castling
	 * Moves rook to the corresponsing position
	 * Changes the global castle tags accordingly
	 */
#pragma region Castle
	//tutkitaan oliko siirto linnoitus. jos oli niin pitää siirtää myös tornia. 
	// Huom! linnoitus siirron alku-ja loppukordinaatit ovat kuninkaan alku ja loppu koordinaatit.(esim. "e1g1)
	if (cPiece == wK && m._start_row == 7 && m._start_line == 4 && m._dest_row == 7 )
	{
		if (m._dest_row == 7 && m._dest_line == 6)
		{
			//siirto oli valkean lyhyt linnoitus joten siirretään myös tornia
			_board[7][5] = wR;
			_board[7][7] = NA;
		}
		else if (m._dest_row == 7 && m._dest_line == 2)
		{
			//pitkä linnoitus
			_board[7][3] = wR;
			_board[7][0] = NA;
		}

	}
	else if (cPiece == bK && m._start_row == 0 && m._start_line == 4 && m._dest_row == 0)
	{
		//mustan linnoitus
		if (m._dest_row == 0 && m._dest_line == 6)
		{
			//lyhyt linnoitus
			_board[0][5] = bR;
			_board[0][7] = NA;
		}
		else if (m._dest_row == 0 && m._dest_line == 2)
		{
			//pitkä linnoitus
			_board[0][3] = bR;
			_board[0][0] = NA;
		}

	}
	//lippujen päivitys
	if (cPiece == wK)
	{
		_whiteShortCastleAllowed = false;
		_whiteLongCastleAllowed = false;
	}
	else if (cPiece == bK)
	{
		_blackShortCastleAllowed = false;
		_blackLongCastleAllowed = false;
	}
#pragma endregion


	/**
	 * \brief Checks if chosen piece is a pawn and moved two squares in one move.
	 * \brief Updates the _doubleStepOnLine to keep track of for next turn.
	 * \brief Checks if chosen piece is a pawn and has moved to a different line.
	 * \brief Captures a En Passant target.
	 */
#pragma region EnPassant
	//en passant
	//tarkistetaan sotilaan kaksoisaskel
	if (cPiece == wP && abs(m._start_row - m._dest_row) == 2)
	{
		_doubleStepOnLine = m._dest_line;
	}
	else if (cPiece == bP && abs(m._start_row - m._dest_row) == 2)
	{
		_doubleStepOnLine = m._dest_line;
	}
	else
	{
		_doubleStepOnLine = -1;
	}

	

#pragma endregion


	//vuoron päivitys
	_moveTurn = Opponent(_moveTurn);
};

#pragma region piece_square_tables
const int Position::pawnTable[8][8] = {
	{  0,  0,  0,  0,  0,  0,  0,  0},
	{ 50, 50, 50, 50, 50, 50, 50, 50},
	{ 10, 10, 20, 30, 30, 20, 10, 10},
	{  5,  5, 10, 25, 25, 10,  5,  5},
	{  0,  0,  0, 20, 20,  0,  0,  0},
	{  5, -5,-10,  0,  0,-10, -5,  5},
	{  5, 10, 10,-20,-20, 10, 10,  5},
	{  0,  0,  0,  0,  0,  0,  0,  0}
};

const int Position::knightTable[8][8] = {
	{-50,-40,-30,-30,-30,-30,-40,-50},
	{-40,-20,  0,  0,  0,  0,-20,-40},
	{-30,  0, 10, 15, 15, 10,  0,-30},
	{-30,  5, 15, 20, 20, 15,  5,-30},
	{-30,  0, 15, 20, 20, 15,  0,-30},
	{-30,  5, 10, 15, 15, 10,  5,-30},
	{-40,-20,  0,  5,  5,  0,-20,-40},
	{-50,-40,-30,-30,-30,-30,-40,-50}
};

const int Position::bishopTable[8][8] = {
	{-20,-10,-10,-10,-10,-10,-10,-20},
	{-10,  0,  0,  0,  0,  0,  0,-10},
	{-10,  0,  5, 10, 10,  5,  0,-10},
	{-10,  5,  5, 10, 10,  5,  5,-10},
	{-10,  0, 10, 10, 10, 10,  0,-10},
	{-10, 10, 10, 10, 10, 10, 10,-10},
	{-10,  5,  0,  0,  0,  0,  5,-10},
	{-20,-10,-10,-10,-10,-10,-10,-20}
};

const int Position::rookTable[8][8] = {
	{  0,  0,  0,  0,  0,  0,  0,  0},
	{  5, 10, 10, 10, 10, 10, 10,  5},
	{ -5,  0,  0,  0,  0,  0,  0, -5},
	{ -5,  0,  0,  0,  0,  0,  0, -5},
	{ -5,  0,  0,  0,  0,  0,  0, -5},
	{ -5,  0,  0,  0,  0,  0,  0, -5},
	{ -5,  0,  0,  0,  0,  0,  0, -5},
	{  0,  0,  0,  5,  5,  0,  0,  0}
};

const int Position::queenTable[8][8] = {
	{-20,-10,-10, -5, -5,-10,-10,-20},
	{-10,  0,  0,  0,  0,  0,  0,-10},
	{-10,  0,  5,  5,  5,  5,  0,-10},
	{ -5,  0,  5,  5,  5,  5,  0, -5},
	{ -5,  0,  5,  5,  5,  5,  0, -5},
	{-10,  5,  5,  5,  5,  5,  0,-10},
	{-10,  0,  5,  0,  0,  0,  0,-10},
	{-20,-10,-10, -5, -5,-10,-10,-20}
};

const int Position::kingTable[8][8] = {
	{-30,-40,-40,-50,-50,-40,-40,-30},
	{-30,-40,-40,-50,-50,-40,-40,-30},
	{-30,-40,-40,-50,-50,-40,-40,-30},
	{-30,-40,-40,-50,-50,-40,-40,-30},
	{-20,-30,-30,-40,-40,-30,-30,-20},
	{-10,-20,-20,-20,-20,-20,-20,-10},
	{ 20, 20,  0,  0,  0,  0, 20, 20},
	{ 20, 30, 10,  0,  0, 10, 30, 20}
};
#pragma endregion

const int Position::centerControlTable[8][8] =
{
	{2, 3, 4, 4, 4, 4, 3, 2},
	{3, 4, 6, 6, 6, 6, 4, 3},
	{4, 6, 8, 8, 8, 8, 6, 4},
	{4, 6, 8,10,10, 8, 6, 4},
	{4, 6, 8,10,10, 8, 6, 4},
	{4, 6, 8, 8, 8, 8, 6, 4},
	{3, 4, 6, 6, 6, 6, 6, 3},
	{2, 3, 4, 4, 4, 4, 3, 2}
};