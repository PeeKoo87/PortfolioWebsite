#pragma once
#include <string>
#include <iostream>


using namespace std;

//kuvaa aseman muutosta.
class Move
{
public:

	Move(){}

	Move(int start_row, int start_line, int dest_row, int dest_line,int pieceToPromote) :
		_start_row(start_row),_start_line(start_line),_dest_row(dest_row),_dest_line(dest_line),_pieceToPromote(pieceToPromote){}

	//l‰ksy : tehty
	//tee konstruktori, jonka avulla siirto-olio alustetaan annetusta merkkijonosta
	//esim "e2e4" tai "g8f8"
	Move(const string& m)
	{

		// Alustetaan _start_row ja _start_line merkkijonon ensimm‰isest‰ ja toisesta merkist‰
		_start_line = m[0] - 'a';
		_start_row = 7 -(m[1] - '1');

		// Alustetaan _dest_row ja _dest_line merkkijonon kolmannesta ja nelj‰nnest‰ merkist‰
		_dest_line = m[2] - 'a';
		_dest_row = 7 - (m[3] - '1');

		if(_pieceToPromote != NA)
			_pieceToPromote = m[5] - 'a';
	}
	
	// Metodi palauttaa siirron merkkijonona
	string ToString() const {
		string result = "";

		
		result += ('a' + _start_line);
		result += ('1' + (7 - _start_row));
		result += ('a' + _dest_line);
		result += ('1' + (7 - _dest_row));

		// Lis‰t‰‰n korotettava nappula merkkijonoon, jos se on asetettu
		if (_pieceToPromote != NA) {
			result += _pieceToPromote;
		}
		return result;
	}
	// Ylikuormitetaan vertailuoperaattorit, jotta Move-oliota voi vertailla toiseen
	bool operator==(const Move& other) const {
		return (_start_row == other._start_row &&
			_start_line == other._start_line &&
			_dest_row == other._dest_row &&
			_dest_line == other._dest_line &&
			_pieceToPromote == other._pieceToPromote);
	}

	bool operator!=(const Move& other) const {
		return !(*this == other);
	}

	

private:
	int _start_row;
	int _start_line;
	int _dest_row;
	int _dest_line;

	int _pieceToPromote = NA;

	friend class Position;

};