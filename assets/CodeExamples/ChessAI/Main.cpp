#include <iostream>
#include "Position.h"
#include "Move.h"
#include <vector>
#include <string>


#include <chrono>
using namespace std;
using namespace std::chrono;
int PieceColor(int piece)
{
    switch (piece)
    {

    case wR: case wN: case wB: case wQ: case wK: case wP:    
        return WHITE;
        break;

    default:
        return BLACK;
        break;
    }
}


int Opponent(int player)
{
    return player == WHITE ? BLACK : WHITE;
}

int ValidateInput(vector<Move>& moves,string& m)
{
    for (int i = 0; i < moves.size(); i++)
    {
        if (m == moves[i].ToString())
        {
            return i;
        }
    }
    return -1;
} 

void AIMove(Position& position)
{
    // Determine the current count of pieces on the board
    int pieceCount = position.CountPieces();

    int EARLYGAME_THRESHOLD = 15;  // Adjust as needed
    int MIDGAME_THRESHOLD = 10;    // Adjust as needed

    int EARLYGAME_DEPTH = 4;       // Adjust as needed
    int MIDGAME_DEPTH = 5;          // Adjust as needed
    int ENDGAME_DEPTH = 6;          // Adjust as needed

    int depth;

    if (pieceCount >= EARLYGAME_THRESHOLD) {
        depth = EARLYGAME_DEPTH;
    }
    else if (pieceCount >= MIDGAME_THRESHOLD) {
        depth = MIDGAME_DEPTH;
    }
    else {
        depth = ENDGAME_DEPTH;
    }
   
    cout << "current depth: " << depth << ", Pieces on board: " << pieceCount << endl;
    auto start_time = high_resolution_clock::now();
    //MinimaxValue Ai_choice = position.iterative_deepening_minimax(10.0,3);

    MinimaxValue Ai_choice = position.Minimax(depth, -numeric_limits<float>::infinity(), numeric_limits<float>::infinity(), 6);
    position.MakeMove(Ai_choice._move);

    cout << "last AI move: " << Ai_choice.str() << ", " << "last minmax value: " << Ai_choice._value << endl;
    //cout << "Player's turn: " << (position._moveTurn == WHITE ? "White" : "Black") << endl;

    // Stop measuring time for the current turn
    auto end_time = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end_time - start_time);
    cout << "Time taken to process AI move: " << duration.count() << " milliseconds" << endl;
}



int main()
{
	// Alkuasema.
	Position position;
    
    vector<Position> move_history;

	vector<Move> moves; //siirrot

	//position.AllRawMovesInDirection(position._moveTurn, moves);
    position.MoveGen(moves);
    
    string is_ai = "";
    string playerColor = "";
    cout << "Select color(W or B): " << endl;
    cin >> playerColor;

    cout << "AI opponent(Y/N)?: " <<endl;
    cin >> is_ai;

    

	while (moves.size() > 0)
	{
        
        if (is_ai == "Y" && playerColor == "B") {
            AIMove(position);
            
            move_history.push_back(position);
        }

        moves.clear();
        position.MoveGen(moves);
        position.Print();

        //tulostetaan siirrot muodossa, a2a4
        cout << "Possible moves:\n";
        for (const Move& move : moves) {
            cout <<move.ToString() << ", ";
        }

        // laillisten siirtojen lukumäärän tulostus esim: 20
        
        cout << "Number of legal moves: " << moves.size() << endl;

        //move_history.push_back(position);

        
        // pelaajan siirto
        string playerMove;

        cout << "Player's turn: " << (position._moveTurn == WHITE ? "White" : "Black") << endl;
        cout << "Player's turn: " << (Opponent(position._moveTurn)) << endl; // testi tulostus pelaajan vuoroon.
        
        int moveIndex = -1;
        while (moveIndex <= -1)
        {
            //undo kutsuttava kaksi kertaa - pitää selvittää miksi
            cout << "Input a move (I.E. e2e4) or type 'undo' to undo latest move: ";
            cin >> playerMove;
            if (playerMove == "undo")
            {
                if (!move_history.empty())
                {
                    move_history.pop_back();
                    move_history.pop_back();
                    position = move_history.back();
                    
                    break;
                }
                else
                {
                    cout << "no moves in history, cannot undo" << endl;
                }
            }
            else
            {
                moveIndex = ValidateInput(moves, playerMove);
                if (moveIndex == -1)
                {
                    cout << "Invalid move. Try again." << endl;
                }
            }
            
        }
        if (playerMove != "undo")
        {
            position.MakeMove(moves[moveIndex]);
            
            move_history.push_back(position);
        }
        
        
        if (is_ai == "Y" && playerColor == "W") {
            AIMove(position);
            
            move_history.push_back(position);
        }
           
        
        
        


        //auto start_time = high_resolution_clock::now();
        ////MinimaxValue Ai_choice = position.iterative_deepening_minimax(10.0,3);

        //MinimaxValue Ai_choice = position.Minimax(4, -numeric_limits<float>::infinity(), numeric_limits<float>::infinity(), 4);
        //position.MakeMove(Ai_choice._move);

        //cout << "last AI move: " << Ai_choice.str() << ", " << "last minmax value: " << Ai_choice._value << endl;
        ////cout << "Player's turn: " << (position._moveTurn == WHITE ? "White" : "Black") << endl;

        //// Stop measuring time for the current turn
        //auto end_time = high_resolution_clock::now();
        //auto duration = duration_cast<milliseconds>(end_time - start_time);
        //cout << "Time taken to process AI move: " << duration.count() << " milliseconds" << endl;

        /*minmax testi
        MinimaxValue value = position.minimax(2);
        float value = position.minimax(position,2);
        cout << value._value << " minmax arvo\n";*/


        
        /*moves.clear();
        position.MoveGen(moves);*/
        
	}
    
	return 0;
}
