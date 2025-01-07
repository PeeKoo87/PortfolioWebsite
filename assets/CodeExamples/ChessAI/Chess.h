#pragma once

//pelaajat
enum{WHITE, BLACK};

//vakioarvot eri nappuloille. (NA = tyhjäruutu).
enum{wR = 'R', wN = 'N', wB = 'B', wQ = 'Q', wK = 'K', wP = 'P', bR = 'r', bN = 'n', bB = 'b', bQ = 'q', bK = 'k', bP = 'p', NA = '.' };

//palauttaa nappulan värin
int PieceColor(int piece);
//palauttaa vastustajan
int Opponent(int player);
