#pragma once

//pelaajat
enum{WHITE, BLACK};

//vakioarvot eri nappuloille. (NA = tyhj�ruutu).
enum{wR = 'R', wN = 'N', wB = 'B', wQ = 'Q', wK = 'K', wP = 'P', bR = 'r', bN = 'n', bB = 'b', bQ = 'q', bK = 'k', bP = 'p', NA = '.' };

//palauttaa nappulan v�rin
int PieceColor(int piece);
//palauttaa vastustajan
int Opponent(int player);
