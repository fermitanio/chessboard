#pragma warning disable IDE0090
#pragma warning disable IDE1006
#pragma warning disable IDE0180
// #pragma warning disable IDE0056

using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace chessboard
{
    internal class BoardInterface
    {
        public const char C_BISHOP_NAME = 'A';
        public const char C_KING_NAME = 'R';
        public const char C_KNIGHT_NAME = 'C';
        public const char C_PAWN_NAME = 'P';
        public const char C_QUEEN_NAME = 'D';
        public const char C_ROOK_NAME = 'T';

        public const char C_BLACK_COLOR = 'b';
        public const char C_WHITE_COLOR = 'w';

        public const char C_FAST_PAWN = 'f';

        private const int C_BOARD_LENGTH = 8;
        private const int C_BOARD_LENGTH_MINUS_1 = 7;

        private bool isCheck_;
        private bool isKingCheckmated_;
        private bool isKingStalemated_;

        private string[][] aa_board;
        private List<Move> validMoves = new List<Move>();
        private bool isWhitePiecesTurn_ = true;
        private bool isWhitePiecesAtBottom_ = true;
        private Point fastPawn = new Point(-1, -1);

        private bool isWhiteShortCastlingValid_;
        private bool isBlackShortCastlingValid_;
        private bool isWhiteLongCastlingValid_;
        private bool isBlackLongCastlingValid_;

        // Data structures to implement the threefold repetition rule.
        List<Point[]> piecePositionsHistory = new List<Point[]>();
        int[][] aa_pieceIndexes = new int[][] {
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7},
            new int[] { 8, 9, 10, 11, 12, 13, 14, 15},
            new int[] { -1, -1, -1, -1, -1, -1, -1, -1 },
            new int[] { -1, -1, -1, -1, -1, -1, -1, -1 },
            new int[] { -1, -1, -1, -1, -1, -1, -1, -1 },
            new int[] { -1, -1, -1, -1, -1, -1, -1, -1 },
            new int[] { 16, 17, 18, 19, 20, 21, 22, 23 },
            new int[] { 24, 25, 26, 27, 28, 29, 30, 31 }};
        private bool tripleRepetitionOfMoves_ = false;

        // Variables to implement the fifty-move rule.
        private int movesCounter_ = 0;
        private bool fiftyMovesRule_;

        // Data structures to implement the insufficient material rule.
        private int[] numberOfBlackPieces = new int[6];
        private int[] numberOfWhitePieces = new int[6];
        private bool notEnoughPieces_;

        //
        private Point whiteKingCoordinates_ = new Point(-1, -1);
        private Point blackKingCoordinates_ = new Point(-1, -1);

        public BoardInterface(bool isWhitePiecesAtBottom_)
        {
            isKingChecked = false;
            isKingCheckmated = false;
            isKingStalemated = false;
            tripleRepetitionOfMoves = false;
            fiftyMovesRule = false;
            notEnoughPieces = false;

            isWhiteShortCastlingValid = true;
            isBlackShortCastlingValid = true;
            isWhiteLongCastlingValid = true;
            isBlackLongCastlingValid = true;

            whiteKingCoordinates_.X = 7;
            whiteKingCoordinates_.Y = 4;
            blackKingCoordinates_.X = 0;
            blackKingCoordinates_.Y = 4;

            isWhitePiecesAtBottom = isWhitePiecesAtBottom_;

            // Black black-squared bishops's counter.
            numberOfBlackPieces[0] = 1;
            // Black white-squared bishops's counter.
            numberOfBlackPieces[1] = 1;
            // Black knights's counter.
            numberOfBlackPieces[2] = 2;
            // Black pawns's counter.
            numberOfBlackPieces[3] = 8;
            // Black queens's counter.
            numberOfBlackPieces[4] = 1;
            // Black rooks's counter.
            numberOfBlackPieces[5] = 2;

            // White black-squared bishops's counter.
            numberOfWhitePieces[0] = 1;
            // White white-squared bishops's counter.
            numberOfWhitePieces[1] = 1;
            // White knights's counter.
            numberOfWhitePieces[2] = 2;
            // White pawns's counter.
            numberOfWhitePieces[3] = 8;
            // White queens's counter.
            numberOfWhitePieces[4] = 1;
            // White rooks's counter.
            numberOfWhitePieces[5] = 2;

            // Meanings of characters:
            // b means black, w means white, T means Torre (Rook),
            // C means Caballo (Knight), A means Alfil (Bishop),
            // D means Dama (Queen), R means Rey (king) and P means Peón (Pawn).
            // bTw means black (b) rook (T) on white square (w).
            aa_board = new string[][] {
                new string[] { "bTw", "bCb", "bAw", "bDb", "bRw", "bAb", "bCw", "bTb"},
                new string[] { "bPb", "bPw", "bPb", "bPw", "bPb", "bPw", "bPb", "bPw"},
                new string[] { "w", "b", "w", "b", "w", "b", "w", "b" },
                new string[] { "b", "w", "b", "w", "b", "w", "b", "w" },
                new string[] { "w", "b", "w", "b", "w", "b", "w", "b" },
                new string[] { "b", "w", "b", "w", "b", "w", "b", "w" },
                new string[] { "wPw", "wPb", "wPw", "wPb", "wPw", "wPb", "wPw", "wPb" },
                new string[] { "wTb", "wCw", "wAb", "wDw", "wRb", "wAw", "wCb", "wTw" }};
            /*
             aa_board = new string[][] {
            new string[] { "bRw", "b", "w", "b", "w", "b", "w", "b"},
                 new string[] { "b", "w", "b", "w", "b", "bTw", "b", "w"},
                 new string[] { "w", "b", "w", "b", "w", "b", "w", "b" },
                 new string[] { "b", "w", "b", "w", "b", "w", "b", "w" },
                 new string[] { "w", "b", "w", "b", "w", "b", "w", "b" },
                 new string[] { "bTb", "w", "b", "w", "b", "w", "b", "w" },
                 new string[] { "w", "b", "w", "b", "w", "b", "wRw", "b" },
                 new string[] { "b", "w", "b", "w", "b", "w", "b", "w" }};
         */

            if (!isWhitePiecesAtBottom_)
            {
                flipBoard();

                /*
                whiteKingCoordinates_.X = 2;
                whiteKingCoordinates_.Y = 0;

                blackKingCoordinates_.X = 0;
                blackKingCoordinates_.Y = 0;
                */

            }

            Point[] piecePositions = new Point[33];
            piecePositions[0] = new Point(0, 0);
            piecePositions[1] = new Point(0, 1);
            piecePositions[2] = new Point(0, 2);
            piecePositions[3] = new Point(0, 3);
            piecePositions[4] = new Point(0, 4);
            piecePositions[5] = new Point(0, 5);
            piecePositions[6] = new Point(0, 6);
            piecePositions[7] = new Point(0, 7);
            piecePositions[8] = new Point(1, 0);
            piecePositions[9] = new Point(1, 1);
            piecePositions[10] = new Point(1, 2);
            piecePositions[11] = new Point(1, 3);
            piecePositions[12] = new Point(1, 4);
            piecePositions[13] = new Point(1, 5);
            piecePositions[14] = new Point(1, 6);
            piecePositions[15] = new Point(1, 7);
            piecePositions[16] = new Point(6, 0);
            piecePositions[17] = new Point(6, 1);
            piecePositions[18] = new Point(6, 2);
            piecePositions[19] = new Point(6, 3);
            piecePositions[20] = new Point(6, 4);
            piecePositions[21] = new Point(6, 5);
            piecePositions[22] = new Point(6, 6);
            piecePositions[23] = new Point(6, 7);
            piecePositions[24] = new Point(7, 0);
            piecePositions[25] = new Point(7, 1);
            piecePositions[26] = new Point(7, 2);
            piecePositions[27] = new Point(7, 3);
            piecePositions[28] = new Point(7, 4);
            piecePositions[29] = new Point(7, 5);
            piecePositions[30] = new Point(7, 6);
            piecePositions[31] = new Point(7, 7);
            piecePositions[32] = new Point(-1, -1);

            piecePositionsHistory.Add(piecePositions);
        }

        // Getter and setter.
        public string[][] board    // the board property
        {
            get => aa_board;
            set => aa_board = value;
        }

        // Getter and setter.
        public bool isKingChecked
        {
            get => isCheck_;
            set => isCheck_ = value;
        }

        // Getter and setter.
        public bool isKingCheckmated
        {
            get => isKingCheckmated_;
            set => isKingCheckmated_ = value;
        }

        // Getter and setter.
        public bool isKingStalemated
        {
            get => isKingStalemated_;
            set => isKingStalemated_ = value;
        }

        // Getter and setter.
        private bool isWhiteShortCastlingValid
        {
            get => isWhiteShortCastlingValid_;
            set => isWhiteShortCastlingValid_ = value;
        }

        // Getter and setter.
        private bool isWhiteLongCastlingValid
        {
            get => isWhiteLongCastlingValid_;
            set => isWhiteLongCastlingValid_ = value;
        }

        // Getter and setter.
        private bool isBlackShortCastlingValid
        {
            get => isBlackShortCastlingValid_;
            set => isBlackShortCastlingValid_ = value;
        }

        // Getter and setter.
        private bool isBlackLongCastlingValid
        {
            get => isBlackLongCastlingValid_;
            set => isBlackLongCastlingValid_ = value;
        }

        // Getter and setter.
        public static int boardLength()
        {
            return C_BOARD_LENGTH;
        }

        // Getter and setter.
        public Point whiteKingCoordinates
        {
            get => whiteKingCoordinates_;
            set => whiteKingCoordinates_ = value;
        }

        // Getter and setter.
        public Point blackKingCoordinates
        {
            get => blackKingCoordinates_;
            set => blackKingCoordinates_ = value;
        }

        // Getter and setter.
        public bool isWhitePiecesTurn
        {
            get => isWhitePiecesTurn_;
            set => isWhitePiecesTurn_ = value;
        }

        // Getter and setter.
        public bool isWhitePiecesAtBottom
        {
            get => isWhitePiecesAtBottom_;
            set => isWhitePiecesAtBottom_ = value;
        }

        // Getter and setter.
        public bool tripleRepetitionOfMoves
        {
            get => tripleRepetitionOfMoves_;
            set => tripleRepetitionOfMoves_ = value;
        }

        // Getter and setter.
        public int movesCounter
        {
            get => movesCounter_;
            set => movesCounter_ = value;
        }

        // Getter and setter.
        public bool fiftyMovesRule
        {
            get => fiftyMovesRule_;
            set => fiftyMovesRule_ = value;
        }

        // Getter and setter.
        public bool notEnoughPieces
        {
            get => notEnoughPieces_;
            set => notEnoughPieces_ = value;
        }

        public bool isaOwnPiece(int row, int column, char pieceColor)
        {
            if ((isWhitePiece(row, column) && pieceColor == C_WHITE_COLOR) ||
                (isBlackPiece(row, column) && pieceColor == C_BLACK_COLOR))
            {
                return true;
            }
            return false;
        }

        public List<Move> getValidMoves()
        {
            return validMoves;
        }

        public bool isThereValidMoves()
        {
            if (validMoves != null && validMoves.Count > 0)
            {
                return true;
            }
            return false;
        }

        /* Examples of flipped positions:
            *  
            *  0,0 > C_BOARD_LENGTH_MINUS_1,7
            0,1 > C_BOARD_LENGTH_MINUS_1,6
            1,1 > 6,6
            3,4 > 4,3
        */
        public void flipBoard()
        {
            int boardLength = aa_board.Length;
            int halfOfboardLength = boardLength / 2;

            for (int i = 0; i < halfOfboardLength; i++)
            {
                for (int j = 0; j < boardLength; j++)
                {
                    string squareAux = aa_board[i][j];
                    aa_board[i][j] = aa_board[boardLength - 1 - i][boardLength - 1 - j];
                    aa_board[boardLength - 1 - i][boardLength - 1 - j] = squareAux;
                }
            }

            // Flip kings's coordenates.
            whiteKingCoordinates_.X = boardLength - 1 - whiteKingCoordinates_.X;
            whiteKingCoordinates_.Y = boardLength - 1 - whiteKingCoordinates_.Y;
            blackKingCoordinates_.X = boardLength - 1 - blackKingCoordinates_.X;
            blackKingCoordinates_.Y = boardLength - 1 - blackKingCoordinates_.Y;

            isWhitePiecesAtBottom = !isWhitePiecesAtBottom;
        }

        public bool isaValidMove(int row, int column)
        {
            Move m = GetMoveFromValidMoves(row, column);

            if (m != null)
            {
                return true;
            }

            return false;
        }

        // Evaluate if the square where user has clicked on,
        // contains a valid piece for move.
        public bool isaValidPieceForMove(int row, int column)
        {
            if (row < 0 || column < 0 ||
                row > C_BOARD_LENGTH_MINUS_1 ||
                column > C_BOARD_LENGTH_MINUS_1)
            {
                return false;
            }

            if (isSquareEmpty(row, column)) { return false; }

            if (isWhitePiecesTurn && isBlackPiece(row, column)) { return false; }

            if (!isWhitePiecesTurn && isWhitePiece(row, column)) { return false; }

            return true;
        }

        // Example of square values: "wP" for a white Pawn,
        // or "b" for an empty square.
        // See the board property's declaration above.
        public bool isSquareEmpty(int row, int column)
        {
            string square = getSquare(row, column);
            if (square != string.Empty && square.Length == 1) return true;
            return false;
        }

        // Example of square values: "wP" for a white pawn,
        // or " " for an empty square.
        // See the board property's declaration above.
        public bool isWhitePiece(int row, int column)
        {
            string square = getSquare(row, column);
            if (square != string.Empty && square.Length >= 3 && square[0] == C_WHITE_COLOR) { return true; }
            return false;
        }

        // Example of square values: "bP" for a black pawn,
        // or " " for an empty square.
        // See the board property's declaration above.
        public bool isBlackPiece(int row, int column)
        {
            string square = getSquare(row, column);
            if (square != string.Empty && square.Length >= 3 && square[0] == C_BLACK_COLOR) { return true; }
            return false;
        }

        // Example of square value for a fast pawn: "wPf".
        // See the board property's declaration above.
        public bool isFastPawn(int row, int column)
        {
            string square = getSquare(row, column);
            if (square != string.Empty && square.Length == 4 && square[2] == C_FAST_PAWN) { return true; }
            return false;
        }

        public Point getFastPawnCoordinates()
        {
            return fastPawn;
        }

        public void setFastPawnCoordinates(int row, int column)
        {
            if (row < 0 || column < 0 ||
                row > C_BOARD_LENGTH_MINUS_1 || column > C_BOARD_LENGTH_MINUS_1)
            {
                return;
            }

            fastPawn.X = row;
            fastPawn.Y = column;
        }

        public void unsetFastPawnCoordinates()
        {
            fastPawn.X = -1;
            fastPawn.Y = -1;
        }

        // Example of square values: "wP" for a white pawn,
        // or " " for an empty square.
        // See the board property's declaration above.
        public char getPieceColor(int row, int column)
        {
            string square = getSquare(row, column);
            if (square == string.Empty || square.Length == 1) { return '\0'; };
            return square[0];
        }

        // Example of square values: "wP" for a white pawn,
        // or " " for an empty square.
        // See the board property's declaration above.
        public char getPieceName(int row, int column)
        {
            string square = getSquare(row, column);
            if (square == string.Empty || square.Length == 1) { return '\0'; };
            return square[1];
        }

        public char getSquareColor(int row, int column)
        {
            string square = getSquare(row, column);

            if (square == string.Empty || square.Length == 0) { return '\0'; };

            // Example of square value: bCw (black knight on a white square).
            if (square.Length == 1) { return square[0]; };

            // Example of square value: bCw (black knight on a white square).
            if (square.Length == 3) { return square[2]; };

            // Example of square value: wPfw (white fast pawn on a white square).
            if (square.Length == 4) { return square[3]; };

            return '\0';
        }

        public string getSquare(int row, int column)
        {
            if (row < 0 ||
                column < 0 ||
                row >= aa_board.Length ||
                column >= aa_board[0].Length)
            {
                return string.Empty;
            }

            return aa_board[row][column];
        }

        private Move GetMoveFromValidMoves(int row, int column)
        {
            if (row < 0 || column < 0 ||
                row > C_BOARD_LENGTH_MINUS_1 || column > C_BOARD_LENGTH_MINUS_1)
            {
                return null!;
            }

            if (validMoves == null) { return null!; }

            for (int i = 0; i < validMoves.Count; i++)
            {
                Point pt = validMoves[i].coordinates;
                if (pt.X == row && pt.Y == column) { return validMoves[i]; }
            }
            return null!;
        }

        public void unsetValidMoves()
        {
            if (validMoves != null)
            {
                validMoves.Clear();
            }
            else
            {
                validMoves = new List<Move>();
            }
        }

        public void setValidMoves(int row, int column)
        {
            unsetValidMoves();

            if (row < 0 || column < 0 ||
                row > C_BOARD_LENGTH_MINUS_1 || column > C_BOARD_LENGTH_MINUS_1)
            {
                return;
            }

            if (isSquareEmpty(row, column)) { return; }

            char pieceName = getPieceName(row, column);
            char pieceColor = getPieceColor(row, column);

            switch (pieceName)
            {
                case C_ROOK_NAME:
                    if (pieceColor == C_WHITE_COLOR)
                    {
                        getValidWhiteRookMoves(row, column);
                    }
                    else if (pieceColor == C_BLACK_COLOR)
                    {
                        getValidBlackRookMoves(row, column);
                    }
                    break;
                case C_KNIGHT_NAME:
                    if (pieceColor == C_WHITE_COLOR)
                    {
                        getValidWhiteKnightMoves(row, column);
                    }
                    else if (pieceColor == C_BLACK_COLOR)
                    {
                        getValidBlackKnightMoves(row, column);
                    }
                    break;
                case C_BISHOP_NAME:
                    if (pieceColor == C_WHITE_COLOR)
                    {
                        getValidWhiteBishopMoves(row, column);
                    }
                    else if (pieceColor == C_BLACK_COLOR)
                    {
                        getValidBlackBishopMoves(row, column);
                    }
                    break;
                case C_QUEEN_NAME:
                    if (pieceColor == C_WHITE_COLOR)
                    {
                        getValidWhiteQueenMoves(row, column);
                    }
                    else if (pieceColor == C_BLACK_COLOR)
                    {
                        getValidBlackQueenMoves(row, column);
                    }
                    break;
                case C_KING_NAME:
                    if (pieceColor == C_WHITE_COLOR)
                    {
                        getValidWhiteKingMoves(row, column);
                    }
                    else if (pieceColor == C_BLACK_COLOR)
                    {
                        getValidBlackKingMoves(row, column);
                    }
                    break;
                case C_PAWN_NAME:
                    if (pieceColor == C_WHITE_COLOR)
                    {
                        getValidWhitePawnMoves(row, column);
                    }
                    else if (pieceColor == C_BLACK_COLOR)
                    {
                        getValidBlackPawnMoves(row, column);
                    }
                    break;
                default:
                    unsetValidMoves();
                    break;
            }
        }

        private bool isChecked(
            int initialRow,
            int initialColumn,
            int targetRow,
            int targetColumn,
            char kingColor)
        {
            if (initialRow < 0 || initialRow > C_BOARD_LENGTH_MINUS_1 ||
                initialColumn < 0 || initialColumn > C_BOARD_LENGTH_MINUS_1 ||
                targetRow < 0 || targetRow > C_BOARD_LENGTH_MINUS_1 ||
                targetColumn < 0 || targetColumn > C_BOARD_LENGTH_MINUS_1)
            {
                return false;
            }

            // Backup the square to be emptied.
            string initialSquareCopy = aa_board[initialRow][initialColumn];
            string targetSquareCopy = aa_board[targetRow][targetColumn];

            // Place the piece in its new square.
            char pieceName = getPieceName(initialRow, initialColumn);
            char pieceColor = getPieceColor(initialRow, initialColumn);
            char squareColor = getSquareColor(targetRow, targetColumn);
            char[] a_newPiece = { pieceColor, pieceName, squareColor };
            aa_board[targetRow][targetColumn] = new string(a_newPiece);

            // Remove the piece from its square.
            aa_board[initialRow][initialColumn] = getSquareColor(initialRow, initialColumn).ToString();

            // Check if the own king is checked.
            if ((kingColor == C_WHITE_COLOR &&
                isSquareCheckedByBlack(whiteKingCoordinates_.X, whiteKingCoordinates_.Y)) ||
                (kingColor == C_BLACK_COLOR &&
                isSquareCheckedByWhite(blackKingCoordinates_.X, blackKingCoordinates_.Y)))
            {
                // Restore the squares to its original contents.
                aa_board[initialRow][initialColumn] = initialSquareCopy;
                // Restore the square to its original content.
                aa_board[targetRow][targetColumn] = targetSquareCopy;

                // The king is checked.
                return true;
            }

            // The king is not pinned.

            // Restore the piece to its original position.
            aa_board[initialRow][initialColumn] = initialSquareCopy;
            // Restore the square to its original content.
            aa_board[targetRow][targetColumn] = targetSquareCopy;

            return false;
        }

        // The isChecked condition must go inside the loop,
        // not only to check if the bishop is pinned, but
        // also to make it legal for the bishop to eat a
        // piece that is checking the king.
        public void getValidWhiteBishopMoves(int row, int column)
        {
            int rowAux = row;
            int colAux = column;

            // Move 1: The bishop moves diagonally forward and to the left:
            // wA'
            //    wA
            // Try moves.
            while (--rowAux >= 0 && --colAux >= 0)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidWhiteBishopMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The bishop hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 2: The bishop moves diagonally forward and to the right:
            //   wA'
            // wA
            // Try moves.
            while (--rowAux >= 0 && ++colAux < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidWhiteBishopMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The bishop hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 3: The bishop moves diagonally back and to the right:
            // wA
            //   wA'
            // Try moves.
            while (++rowAux < C_BOARD_LENGTH && ++colAux < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidWhiteBishopMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The bishop hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 4: The bishop moves diagonally back and to the left:
            //    wA
            // wA'
            // Try moves.
            while (++rowAux < C_BOARD_LENGTH && --colAux >= 0)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidWhiteBishopMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The bishop hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        // The isChecked condition must go inside the loop,
        // not only to check if the bishop is pinned, but
        // also to make it legal for the bishop to eat a
        // piece that is checking the king.
        public void getValidBlackBishopMoves(int row, int column)
        {
            int rowAux = row;
            int colAux = column;

            // Move 1: The bishop moves diagonally forward and to the left:
            // wA'
            //    wA
            // Try moves.
            while (--rowAux >= 0 && --colAux >= 0)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidBlackBishopMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The bishop hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }

            }

            rowAux = row;
            colAux = column;

            // Move 2: The bishop moves diagonally forward and to the right:
            //   wA'
            // wA
            // Try moves.
            while (--rowAux >= 0 && ++colAux < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidBlackBishopMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The bishop hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 3: The bishop moves diagonally back and to the right:
            // wA
            //   wA'
            // Try moves.
            while (++rowAux < C_BOARD_LENGTH && ++colAux < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidBlackBishopMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The bishop hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 4: The bishop moves diagonally back and to the left:
            //    wA
            // wA'
            // Try moves.
            while (++rowAux < C_BOARD_LENGTH && --colAux >= 0)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidBlackBishopMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The bishop hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private bool isaValidWhiteBishopMove(int row, int column)
        {
            if (row >= 0 && row < C_BOARD_LENGTH && column >= 0 && column < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isSquareEmpty(row, column) || isBlackPiece(row, column))
                {
                    return true;
                }
            }

            return false;
        }

        private bool isaValidBlackBishopMove(int row, int column)
        {
            if (row >= 0 && row < C_BOARD_LENGTH && column >= 0 && column < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isSquareEmpty(row, column) || isWhitePiece(row, column))
                {
                    return true;
                }
            }

            return false;
        }

        public void getValidWhiteKingMoves(int row, int column)
        {
            // Move 1: The king moves diagonally forward and to the left:
            // wA'
            //    wA
            if (isaValidWhiteKingMove(row - 1, column - 1))
            {
                Move move = new Move(new Point(row - 1, column - 1));
                validMoves.Add(move);
            }

            // Move 2: The king moves forward:
            // wA'
            // wA 
            if (isaValidWhiteKingMove(row - 1, column))
            {
                Move move = new Move(new Point(row - 1, column));
                validMoves.Add(move);
            }

            // Move 3: The king moves diagonally forward and to the right:
            //   wA'
            // wA
            if (isaValidWhiteKingMove(row - 1, column + 1))
            {
                Move move = new Move(new Point(row - 1, column + 1));
                validMoves.Add(move);
            }

            // Move 4: The king moves to the right:
            // wA wA'
            if (isaValidWhiteKingMove(row, column + 1))
            {
                Move move = new Move(new Point(row, column + 1));
                validMoves.Add(move);
            }


            // Move 5: The king moves diagonally back and to the right:
            // wA
            //   wA'
            if (isaValidWhiteKingMove(row + 1, column + 1))
            {
                Move move = new Move(new Point(row + 1, column + 1));
                validMoves.Add(move);
            }

            // Move 6: The king moves back:
            // wA 
            // wA'
            if (isaValidWhiteKingMove(row + 1, column))
            {
                Move move = new Move(new Point(row + 1, column));
                validMoves.Add(move);
            }

            // Move 7: The king moves diagonally back and to the left:
            //    wA
            // wA'
            if (isaValidWhiteKingMove(row + 1, column - 1))
            {
                Move move = new Move(new Point(row + 1, column - 1));
                validMoves.Add(move);
            }

            // Move 8: The king moves to the left:
            // wA' wA 
            if (isaValidWhiteKingMove(row, column - 1))
            {
                Move move = new Move(new Point(row, column - 1));
                validMoves.Add(move);
            }

            // White king's short castling.
            if (isWhiteShortCastlingValid)
            {
                if (isWhitePiecesAtBottom)
                {
                    // Bishop and knight's squares are empty.
                    if (isSquareEmpty(7, 5) && isSquareEmpty(7, 6) &&
                        !isSquareCheckedByBlack(7, 5) &&
                        !isSquareCheckedByBlack(7, 6))
                    {
                        // Two moves to the right (short castling).
                        Move move = new Move(new Point(row, column + 2), Move.moveTypes.shortCastling);
                        validMoves.Add(move);
                    }
                }
                else
                {
                    // Bishop and knight's squares are empty.
                    if (isSquareEmpty(0, 1) && isSquareEmpty(0, 2) &&
                        !isSquareCheckedByBlack(0, 1) &&
                        !isSquareCheckedByBlack(0, 2))
                    {
                        // Two moves to the left (short castling).
                        Move move = new Move(new Point(row, column - 2), Move.moveTypes.shortCastling);
                        validMoves.Add(move);
                    }
                }
            }

            // White king's long castling.
            if (isWhiteLongCastlingValid)
            {
                if (isWhitePiecesAtBottom)
                {
                    // Bishop, knight and queen's squares are empty.
                    if (isSquareEmpty(7, 1) && isSquareEmpty(7, 2) && isSquareEmpty(7, 3) &&
                        !isSquareCheckedByBlack(7, 2) &&
                        !isSquareCheckedByBlack(7, 3))
                    {
                        // Two moves to the left (long castling).
                        Move move = new Move(new Point(row, column - 2), Move.moveTypes.longCastling);
                        validMoves.Add(move);
                    }
                }
                else
                {
                    // Bishop, knight and queen's squares are empty.
                    if (isSquareEmpty(0, 4) && isSquareEmpty(0, 5) && isSquareEmpty(0, 6) &&
                        !isSquareCheckedByBlack(0, 4) &&
                        !isSquareCheckedByBlack(0, 5))
                    {
                        // Two moves to the right (long castling).
                        Move move = new Move(new Point(row, column + 2), Move.moveTypes.longCastling);
                        validMoves.Add(move);
                    }
                }
            }
        }

        public void getValidBlackKingMoves(int row, int column)
        {
            // Move 1: The king moves diagonally forward and to the left:
            // wA'
            //    wA
            if (isaValidBlackKingMove(row - 1, column - 1))
            {
                Move move = new Move(new Point(row - 1, column - 1));
                validMoves.Add(move);
            }

            // Move 2: The king moves forward:
            // wA'
            // wA 
            if (isaValidBlackKingMove(row - 1, column))
            {
                Move move = new Move(new Point(row - 1, column));
                validMoves.Add(move);
            }

            // Move 3: The king moves diagonally forward and to the right:
            //   wA'
            // wA
            if (isaValidBlackKingMove(row - 1, column + 1))
            {
                Move move = new Move(new Point(row - 1, column + 1));
                validMoves.Add(move);
            }

            // Move 4: The king moves to the right:
            // wA wA'
            if (isaValidBlackKingMove(row, column + 1))
            {
                Move move = new Move(new Point(row, column + 1));
                validMoves.Add(move);
            }

            // Move 5: The king moves diagonally back and to the right:
            // wA
            //   wA'
            if (isaValidBlackKingMove(row + 1, column + 1))
            {
                Move move = new Move(new Point(row + 1, column + 1));
                validMoves.Add(move);
            }

            // Move 6: The king moves back:
            // wA 
            // wA'
            if (isaValidBlackKingMove(row + 1, column))
            {
                Move move = new Move(new Point(row + 1, column));
                validMoves.Add(move);
            }

            // Move 7: The king moves diagonally back and to the left:
            //    wA
            // wA'
            if (isaValidBlackKingMove(row + 1, column - 1))
            {
                Move move = new Move(new Point(row + 1, column - 1));
                validMoves.Add(move);
            }

            // Move 8: The king moves to the left:
            // wA' wA 
            if (isaValidBlackKingMove(row, column - 1))
            {
                Move move = new Move(new Point(row, column - 1));
                validMoves.Add(move);
            }

            // Black king's short castling.
            if (isBlackShortCastlingValid)
            {
                if (isWhitePiecesAtBottom)
                {
                    // Bishop and knight's squares are empty.
                    if (isSquareEmpty(0, 5) && isSquareEmpty(0, 6) &&
                        !isSquareCheckedByWhite(0, 5) &&
                        !isSquareCheckedByWhite(0, 6))
                    {
                        // Two moves to the right (short castling).
                        Move move = new Move(new Point(row, column + 2), Move.moveTypes.shortCastling);
                        validMoves.Add(move);
                    }
                }
                else
                {
                    // Bishop and knight's squares are empty.
                    if (isSquareEmpty(7, 1) && isSquareEmpty(7, 2) &&
                        !isSquareCheckedByWhite(7, 1) &&
                        !isSquareCheckedByWhite(7, 2))
                    {
                        // Two moves to the left (short castling).
                        Move move = new Move(new Point(row, column - 2), Move.moveTypes.shortCastling);
                        validMoves.Add(move);
                    }
                }
            }

            // Black king's long castling.
            if (isBlackLongCastlingValid)
            {
                if (isWhitePiecesAtBottom)
                {
                    // Bishop, knight and queen's squares are empty.
                    if (isSquareEmpty(0, 1) && isSquareEmpty(0, 2) && isSquareEmpty(0, 3) &&
                        !isSquareCheckedByWhite(0, 2) &&
                        !isSquareCheckedByWhite(0, 3))
                    {
                        // Two moves to the right (long castling).
                        Move move = new Move(new Point(row, column - 2), Move.moveTypes.longCastling);
                        validMoves.Add(move);
                    }
                }
                else
                {
                    // Bishop, knight and queen's squares are empty.
                    if (isSquareEmpty(7, 4) && isSquareEmpty(7, 5) && isSquareEmpty(7, 6) &&
                        !isSquareCheckedByWhite(7, 4) &&
                        !isSquareCheckedByWhite(7, 5))
                    {
                        // Two moves to the right (long castling).
                        Move move = new Move(new Point(row, column + 2), Move.moveTypes.longCastling);
                        validMoves.Add(move);
                    }
                }
            }
        }

        private bool isaValidWhiteKingMove(int row, int column)
        {
            if (row >= 0 && row < C_BOARD_LENGTH && column >= 0 && column < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color, and
                // is not in check.
                if (!isSquareCheckedByBlack(row, column) &&
                    (isSquareEmpty(row, column) || isBlackPiece(row, column)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool isaValidBlackKingMove(int row, int column)
        {
            if (row >= 0 && row < C_BOARD_LENGTH && column >= 0 && column < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color, and
                // is not in check.
                if (!isSquareCheckedByWhite(row, column) &&
                    (isSquareEmpty(row, column) || isWhitePiece(row, column)))
                {
                    return true;
                }
            }
            return false;
        }

        public void getValidWhiteKnightMoves(int row, int column)
        {
            // Move 1: Knight movement two square forward and one left:
            // wP'
            // 
            //    wP
            if (isaValidWhiteKnightMove(row, column, row - 2, column - 1))
            {
                Move move = new Move(new Point(row - 2, column - 1));
                validMoves.Add(move);
            }

            // Move 2: Knight movement two square forward and one right:
            //   wP'
            // 
            // wP
            if (isaValidWhiteKnightMove(row, column, row - 2, column + 1))
            {
                Move move = new Move(new Point(row - 2, column + 1));
                validMoves.Add(move);
            }

            // Move 3: Knight movement one square forward and two right:
            //       wP'
            // wP____
            // 
            if (isaValidWhiteKnightMove(row, column, row - 1, column + 2))
            {
                Move move = new Move(new Point(row - 1, column + 2));
                validMoves.Add(move);
            }

            // Move 4: Knight movement one square back and two right:
            // wP    
            //   ____wP'
            // 
            if (isaValidWhiteKnightMove(row, column, row + 1, column + 2))
            {
                Move move = new Move(new Point(row + 1, column + 2));
                validMoves.Add(move);
            }

            // Move 5: Knight movement two square back and one right:
            // wP    
            // 
            //   wP'
            if (isaValidWhiteKnightMove(row, column, row + 2, column + 1))
            {
                Move move = new Move(new Point(row + 2, column + 1));
                validMoves.Add(move);
            }

            // Move 6: Knight movement two square back and one left:
            //   wP    
            // 
            // wP'
            if (isaValidWhiteKnightMove(row, column, row + 2, column - 1))
            {
                Move move = new Move(new Point(row + 2, column - 1));
                validMoves.Add(move);
            }

            // Move 7: Knight movement one square back and two left:
            //        wP    
            // wP'----
            // 
            if (isaValidWhiteKnightMove(row, column, row + 1, column - 2))
            {
                Move move = new Move(new Point(row + 1, column - 2));
                validMoves.Add(move);
            }

            // Move 8: Knight movement one square back and two left:
            // wP'----
            //        wP  
            if (isaValidWhiteKnightMove(row, column, row - 1, column - 2))
            {
                Move move = new Move(new Point(row - 1, column - 2));
                validMoves.Add(move);
            }
        }

        public void getValidBlackKnightMoves(int row, int column)
        {
            // Move 1: Knight movement two square forward and one left:
            // wP'
            // 
            //    wP
            if (isaValidBlackKnightMove(row, column, row - 2, column - 1))
            {
                Move move = new Move(new Point(row - 2, column - 1));
                validMoves.Add(move);
            }

            // Move 2: Knight movement two square forward and one right:
            //   wP'
            // 
            // wP
            if (isaValidBlackKnightMove(row, column, row - 2, column + 1))
            {
                Move move = new Move(new Point(row - 2, column + 1));
                validMoves.Add(move);
            }

            // Move 3: Knight movement one square forward and two right:
            //       wP'
            // wP____
            // 
            if (isaValidBlackKnightMove(row, column, row - 1, column + 2))
            {
                Move move = new Move(new Point(row - 1, column + 2));
                validMoves.Add(move);
            }

            // Move 4: Knight movement one square back and two right:
            // wP    
            //   ____wP'
            // 
            if (isaValidBlackKnightMove(row, column, row + 1, column + 2))
            {
                Move move = new Move(new Point(row + 1, column + 2));
                validMoves.Add(move);
            }

            // Move 5: Knight movement two square back and one right:
            // wP    
            // 
            //   wP'
            if (isaValidBlackKnightMove(row, column, row + 2, column + 1))
            {
                Move move = new Move(new Point(row + 2, column + 1));
                validMoves.Add(move);
            }

            // Move 6: Knight movement two square back and one left:
            //   wP    
            // 
            // wP'
            if (isaValidBlackKnightMove(row, column, row + 2, column - 1))
            {
                Move move = new Move(new Point(row + 2, column - 1));
                validMoves.Add(move);
            }

            // Move 7: Knight movement one square back and two left:
            //        wP    
            // wP'----
            // 
            if (isaValidBlackKnightMove(row, column, row + 1, column - 2))
            {
                Move move = new Move(new Point(row + 1, column - 2));
                validMoves.Add(move);
            }

            // Move 8: Knight movement one square back and two left:
            // wP'----
            //        wP  
            if (isaValidBlackKnightMove(row, column, row - 1, column - 2))
            {
                Move move = new Move(new Point(row - 1, column - 2));
                validMoves.Add(move);
            }
        }

        private bool isaValidWhiteKnightMove(
            int startingRow, int startingColumn,
            int targetRow, int targetColumn)
        {
            if (targetRow >= 0 && targetRow < C_BOARD_LENGTH && targetColumn >= 0 && targetColumn < C_BOARD_LENGTH)
            {
                // The piece is pinned, so can't be moved.
                if (isChecked(startingRow, startingColumn, targetRow, targetColumn, C_WHITE_COLOR))
                {
                    return false;
                }

                // The new position is empty or
                // contains a piece of the opposite color.
                if (isSquareEmpty(targetRow, targetColumn) ||
                    isBlackPiece(targetRow, targetColumn))
                {
                    return true;
                }
            }
            return false;
        }

        private bool isaValidBlackKnightMove(
            int startingRow, int startingColumn,
            int targetRow, int targetColumn)
        {
            if (targetRow >= 0 && targetRow < C_BOARD_LENGTH && targetColumn >= 0 && targetColumn < C_BOARD_LENGTH)
            {
                // The piece is pinned, so can't be moved.
                if (isChecked(startingRow, startingColumn, targetRow, targetColumn, C_BLACK_COLOR))
                {
                    return false;
                }

                // The new position is empty or
                // contains a piece of the opposite color.
                if (isSquareEmpty(targetRow, targetColumn) ||
                    isWhitePiece(targetRow, targetColumn))
                {
                    return true;
                }
            }
            return false;
        }

        public void getValidWhitePawnMoves(int row, int column)
        {
            // White pieces are at the bottom of the board.
            if (isWhitePiecesAtBottom)
            {
                // White pawn movement one square forward:
                // wP'
                // wP
                int rowAux = row - 1;
                int colAux = column;

                // If piece is pinned then can't be moved in this direction.
                if (rowAux >= 0 &&
                isSquareEmpty(rowAux, colAux) &&
                !isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                {
                    // The pawn has not reached the last rank.
                    if (rowAux != 0)
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);
                    }
                    // The pawn reaches the last rank.
                    // So, the pawn promotions.
                    else
                    {
                        Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                        validMoves.Add(move);
                    }
                }

                // White pawn movement two squares forward:
                // wP'
                //  
                // wP
                rowAux = row - 2;
                colAux = column;

                // "Row == 6" means the pawn is on a starting square.
                if (row == 6 &&
                    isSquareEmpty(rowAux, colAux) &&
                    isSquareEmpty(rowAux + 1, colAux) &&
                    !isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                {
                    // Point coordinates = new Point(rowAux, colAux);
                    // Move move = new Move(new Point(rowAux, colAux));
                    Move move = new Move(
                            new Point(rowAux, colAux),
                            Move.moveTypes.FastPawn);
                    validMoves.Add(move);
                }

                // The white pawn moves one square diagonally to the left.
                // wP'
                //    wP
                rowAux = row - 1;
                colAux = column - 1;

                // 
                if (rowAux >= 0 &&
                    colAux >= 0 &&
                !isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                {
                    if (isBlackPiece(rowAux, colAux))
                    {
                        // The pawn has not reached the last rank.
                        if (rowAux != 0)
                        {
                            Move move = new Move(new Point(rowAux, colAux));
                            validMoves.Add(move);
                        }
                        // The pawn reaches the last rank.
                        // So, the pawn promotions.
                        else
                        {
                            Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                            validMoves.Add(move);
                        }
                    }
                    // The opponent's neighboring pawn has moved
                    // two squares in the previous turn.
                    // wP'
                    // bPf wP
                    else if (isSquareEmpty(rowAux, colAux) && isFastPawn(row, column - 1))
                    {
                        // Point coordinates = new Point(rowAux, colAux);
                        Move move = new Move(
                            new Point(rowAux, colAux),
                            Move.moveTypes.EnPassant,
                            new Point(row, column - 1));
                        validMoves.Add(move);
                    }
                }

                // The white pawn moves one square diagonally to the right.
                //   wP'
                // wP
                rowAux = row - 1;
                colAux = column + 1;

                // 
                if (rowAux >= 0 &&
                    colAux < C_BOARD_LENGTH &&
                    !isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                {
                    if (isBlackPiece(rowAux, colAux))
                    {
                        // The pawn has not reached the last rank.
                        if (rowAux != 0)
                        {
                            Move move = new Move(new Point(rowAux, colAux));
                            validMoves.Add(move);
                        }
                        // The pawn reaches the last rank.
                        // So, the pawn promotions.
                        else
                        {
                            Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                            validMoves.Add(move);
                        }
                    }
                    // The opponent's neighboring pawn has moved
                    // two squares in the previous turn.
                    //    wP'
                    // wP bPf
                    else if (isSquareEmpty(rowAux, colAux) && isFastPawn(row, column + 1))
                    {
                        // Point coordinates = new Point(rowAux, colAux);
                        Move move = new Move(
                           new Point(rowAux, colAux),
                           Move.moveTypes.EnPassant,
                           new Point(row, column + 1));
                        validMoves.Add(move);
                    }
                }
            }
            // White pieces are at the top of the board.
            else
            {
                // White pawn movement one square forward:
                // wP
                // wP'
                int rowAux = row + 1;
                int colAux = column;

                // If piece is pinned then can't be moved in this direction.
                if (rowAux < C_BOARD_LENGTH &&
                isSquareEmpty(rowAux, colAux) &&
                !isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                {
                    // The pawn has not reached the last rank.
                    if (rowAux != 7)
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);
                    }
                    // The pawn reaches the last rank.
                    // So, the pawn promotions.
                    else
                    {
                        Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                        validMoves.Add(move);
                    }
                }

                // White pawn movement two squares forward:
                // wP
                //  
                // wP'
                rowAux = row + 2;
                colAux = column;

                // "Row == 6" means the pawn is on a starting square.
                if (row == 1 &&
                    isSquareEmpty(rowAux, colAux) &&
                    isSquareEmpty(rowAux - 1, colAux) &&
                    !isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                {
                    // Point coordinates = new Point(rowAux, colAux);
                    // Move move = new Move(new Point(rowAux, colAux));
                    Move move = new Move(
                            new Point(rowAux, colAux),
                            Move.moveTypes.FastPawn);
                    validMoves.Add(move);
                }

                // The white pawn moves one square diagonally to the left.
                //    wP
                // wP'
                rowAux = row + 1;
                colAux = column - 1;

                // 
                if (rowAux < C_BOARD_LENGTH &&
                    colAux >= 0 &&
                !isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                {
                    if (isBlackPiece(rowAux, colAux))
                    {
                        // The pawn has not reached the last rank.
                        if (rowAux != 7)
                        {
                            Move move = new Move(new Point(rowAux, colAux));
                            validMoves.Add(move);
                        }
                        // The pawn reaches the last rank.
                        // So, the pawn promotions.
                        else
                        {
                            Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                            validMoves.Add(move);
                        }
                    }
                    // The opponent's neighboring pawn has moved
                    // two squares in the previous turn.
                    // bPf wP
                    // wP' 
                    else if (isSquareEmpty(rowAux, colAux) && isFastPawn(row, column - 1))
                    {
                        // Point coordinates = new Point(rowAux, colAux);
                        Move move = new Move(
                            new Point(rowAux, colAux),
                            Move.moveTypes.EnPassant,
                            new Point(row, column - 1));
                        validMoves.Add(move);
                    }
                }

                // The white pawn moves one square diagonally to the right.
                // wP
                //    wP'
                rowAux = row + 1;
                colAux = column + 1;

                // 
                if (rowAux < C_BOARD_LENGTH &&
                    colAux < C_BOARD_LENGTH &&
                    !isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                {
                    if (isBlackPiece(rowAux, colAux))
                    {
                        // The pawn has not reached the last rank.
                        if (rowAux != 7)
                        {
                            Move move = new Move(new Point(rowAux, colAux));
                            validMoves.Add(move);
                        }
                        // The pawn reaches the last rank.
                        // So, the pawn promotions.
                        else
                        {
                            Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                            validMoves.Add(move);
                        }
                    }
                    // The opponent's neighboring pawn has moved
                    // two squares in the previous turn.
                    // wP bPf
                    //    wP' 
                    else if (isSquareEmpty(rowAux, colAux) && isFastPawn(row, column + 1))
                    {
                        // Point coordinates = new Point(rowAux, colAux);
                        Move move = new Move(
                           new Point(rowAux, colAux),
                           Move.moveTypes.EnPassant,
                           new Point(row, column + 1));
                        validMoves.Add(move);
                    }
                }
            }
        }

        public void getValidBlackPawnMoves(int row, int column)
        {
            // White pieces are at the bottom of the board.
            if (isWhitePiecesAtBottom)
            {
                // Black pawn movement one square forward:
                // wP
                // wP'
                int rowAux = row + 1;
                int colAux = column;

                if (rowAux < C_BOARD_LENGTH &&
                    isSquareEmpty(rowAux, colAux) &&
                    !isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                {
                    // The pawn hast not reached the first rank.
                    if (rowAux < 7)
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);
                    }
                    // The pawn reaches the last rank.
                    // So, the pawn promotions.
                    else
                    {
                        Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                        validMoves.Add(move);
                    }
                }

                // Black pawn movement two squares forward:
                // wP
                //  
                // wP'
                rowAux = row + 2;
                colAux = column;

                // "Row == 2" means the pawn is on a starting square.
                if (row == 1 &&
                    isSquareEmpty(rowAux, colAux) &&
                    isSquareEmpty(rowAux - 1, colAux) &&
                    !isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                {
                    // Point coordinates = new Point(rowAux, colAux);
                    // Move move = new Move(new Point(rowAux, colAux));
                    Move move = new Move(
                            new Point(rowAux, colAux),
                            Move.moveTypes.FastPawn);
                    validMoves.Add(move);
                }

                // The black pawn moves one square diagonally to the right.
                // wP
                //    wP'
                rowAux = row + 1;
                colAux = column + 1;

                // 
                if (rowAux < C_BOARD_LENGTH &&
                    colAux < C_BOARD_LENGTH &&
                    !isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                {
                    if (isWhitePiece(rowAux, colAux))
                    {
                        // The pawn hast not reached the first rank.
                        if (rowAux < 7)
                        {
                            Move move = new Move(new Point(rowAux, colAux));
                            validMoves.Add(move);
                        }
                        // The pawn reaches the last rank.
                        // So, the pawn promotions.
                        else
                        {
                            Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                            validMoves.Add(move);
                        }
                    }
                    // The opponent's neighboring pawn has moved.
                    // two squares in the previous turn.
                    // wP bPf
                    //    wP'
                    else if (isSquareEmpty(rowAux, colAux) && isFastPawn(row, column + 1))
                    {
                        // Point coordinates = new Point(rowAux, colAux);
                        Move move = new Move(
                           new Point(rowAux, colAux),
                           Move.moveTypes.EnPassant,
                           new Point(row, column + 1));
                        validMoves.Add(move);
                    }
                }

                // The black pawn moves one square diagonally to the left.
                //    wP
                // wP'
                rowAux = row + 1;
                colAux = column - 1;

                // 
                if (rowAux < C_BOARD_LENGTH &&
                    colAux >= 0 &&
                    !isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                {
                    if (isWhitePiece(rowAux, colAux))
                    {
                        // The pawn hast not reached the first rank.
                        if (rowAux < 7)
                        {
                            Move move = new Move(new Point(rowAux, colAux));
                            validMoves.Add(move);
                        }
                        // The pawn reaches the last rank.
                        // So, the pawn promotions.
                        else
                        {
                            Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                            validMoves.Add(move);
                        }
                    }
                    // The opponent's neighboring pawn has moved
                    // two squares in the previous turn.
                    // bPf wP
                    // wP' 
                    else if (isSquareEmpty(rowAux, colAux) && isFastPawn(row, column - 1))
                    {
                        // Point coordinates = new Point(rowAux, colAux);
                        Move move = new Move(
                           new Point(rowAux, colAux),
                           Move.moveTypes.EnPassant,
                           new Point(row, column - 1));
                        validMoves.Add(move);
                    }
                }
            }
            // White pieces are at the top of the board.
            else
            {
                // Black pawn movement one square forward:
                // wP'
                // wP
                int rowAux = row - 1;
                int colAux = column;

                // If piece is pinned then can't be moved in this direction.
                if (rowAux >= 0 &&
                isSquareEmpty(rowAux, colAux) &&
                !isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                {
                    // The pawn has not reached the last rank.
                    if (rowAux != 0)
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);
                    }
                    // The pawn reaches the last rank.
                    // So, the pawn promotions.
                    else
                    {
                        Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                        validMoves.Add(move);
                    }
                }

                // Black pawn movement two squares forward:
                // wP'
                //  
                // wP
                rowAux = row - 2;
                colAux = column;

                // "Row == 6" means the pawn is on a starting square.
                if (row == 6 &&
                    isSquareEmpty(rowAux, colAux) &&
                    isSquareEmpty(rowAux + 1, colAux) &&
                    !isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                {
                    // Point coordinates = new Point(rowAux, colAux);
                    // Move move = new Move(new Point(rowAux, colAux));
                    Move move = new Move(
                            new Point(rowAux, colAux),
                            Move.moveTypes.FastPawn);
                    validMoves.Add(move);
                }

                // The black pawn moves one square diagonally to the left.
                // wP'
                //    wP
                rowAux = row - 1;
                colAux = column - 1;

                // 
                if (rowAux >= 0 &&
                    colAux >= 0 &&
                !isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                {
                    if (isWhitePiece(rowAux, colAux))
                    {
                        // The pawn has not reached the last rank.
                        if (rowAux != 0)
                        {
                            Move move = new Move(new Point(rowAux, colAux));
                            validMoves.Add(move);
                        }
                        // The pawn reaches the last rank.
                        // So, the pawn promotions.
                        else
                        {
                            Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                            validMoves.Add(move);
                        }
                    }
                    // The opponent's neighboring pawn has moved
                    // two squares in the previous turn.
                    // wP'
                    // bPf wP
                    else if (isSquareEmpty(rowAux, colAux) && isFastPawn(row, column - 1))
                    {
                        // Point coordinates = new Point(rowAux, colAux);
                        Move move = new Move(
                            new Point(rowAux, colAux),
                            Move.moveTypes.EnPassant,
                            new Point(row, column - 1));
                        validMoves.Add(move);
                    }
                }

                // The black pawn moves one square diagonally to the right.
                //   wP'
                // wP
                rowAux = row - 1;
                colAux = column + 1;

                // 
                if (rowAux >= 0 &&
                    colAux < C_BOARD_LENGTH &&
                    !isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                {
                    if (isWhitePiece(rowAux, colAux))
                    {
                        // The pawn has not reached the last rank.
                        if (rowAux != 0)
                        {
                            Move move = new Move(new Point(rowAux, colAux));
                            validMoves.Add(move);
                        }
                        // The pawn reaches the last rank.
                        // So, the pawn promotions.
                        else
                        {
                            Move move = new Move(new Point(rowAux, colAux), Move.moveTypes.PawnPromotion);
                            validMoves.Add(move);
                        }
                    }
                    // The opponent's neighboring pawn has moved
                    // two squares in the previous turn.
                    //    wP'
                    // wP bPf
                    else if (isSquareEmpty(rowAux, colAux) && isFastPawn(row, column + 1))
                    {
                        // Point coordinates = new Point(rowAux, colAux);
                        Move move = new Move(
                           new Point(rowAux, colAux),
                           Move.moveTypes.EnPassant,
                           new Point(row, column + 1));
                        validMoves.Add(move);
                    }
                }
            }
        }

        public void getValidWhiteQueenMoves(int row, int column)
        {
            getValidWhiteBishopMoves(row, column);
            getValidWhiteRookMoves(row, column);
        }

        public void getValidBlackQueenMoves(int row, int column)
        {
            getValidBlackBishopMoves(row, column);
            getValidBlackRookMoves(row, column);
        }

        // The isChecked condition must go inside the loop,
        // not only to check if the bishop is pinned, but
        // also to make it legal for the rook to eat a
        // piece that is checking the king.
        public void getValidWhiteRookMoves(int row, int column)
        {
            int rowAux = row;
            int colAux = column;

            // Move 1: The rook moves forward:
            // bT'
            // bT
            // Try moves.
            while (--rowAux >= 0)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidWhiteRookMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The rook hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 2: The rook moves to the right:
            // bT bT'
            // 
            // Try moves.
            while (++colAux < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidWhiteRookMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The rook hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 3: The rook moves back:
            // wT
            // wT'
            // Try moves.
            while (++rowAux < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidWhiteRookMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The rook hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 4: The rook moves to the left:
            // wT' wT
            // Try moves.
            while (--colAux >= 0)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidWhiteRookMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_WHITE_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The rook hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public void getValidBlackRookMoves(int row, int column)
        {
            int rowAux = row;
            int colAux = column;

            // Move 1: The rook moves forward:
            // bT'
            // bT
            // Try moves.
            while (--rowAux >= 0)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidBlackRookMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The rook hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 2: The rook moves to the right:
            // bT bT'
            // 
            // Try moves.
            while (++colAux < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidBlackRookMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The rook hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 3: The rook moves back:
            // wT
            // wT'
            // Try moves.
            while (++rowAux < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidBlackRookMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The rook hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            rowAux = row;
            colAux = column;

            // Move 4: The rook moves to the left:
            // wT' wT
            // Try moves.
            while (--colAux >= 0)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isaValidBlackRookMove(rowAux, colAux))
                {
                    // The new move does not put the king in check.
                    if (!isChecked(row, column, rowAux, colAux, C_BLACK_COLOR))
                    {
                        Move move = new Move(new Point(rowAux, colAux));
                        validMoves.Add(move);

                        // The rook hits a piece and cannot continue moving.
                        if (!isSquareEmpty(rowAux, colAux))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private bool isaValidWhiteRookMove(int row, int column)
        {
            if (row >= 0 && row < C_BOARD_LENGTH && column >= 0 && column < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isSquareEmpty(row, column) || isBlackPiece(row, column))
                {
                    return true;
                }
            }

            return false;
        }

        private bool isaValidBlackRookMove(int row, int column)
        {
            if (row >= 0 && row < C_BOARD_LENGTH && column >= 0 && column < C_BOARD_LENGTH)
            {
                // The new position is empty or
                // contains a piece of the opposite color.
                if (isSquareEmpty(row, column) || isWhitePiece(row, column))
                {
                    return true;
                }
            }

            return false;
        }

        private bool isaBlackBishop(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_BISHOP_NAME && pieceColor == C_BLACK_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaBlackKing(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_KING_NAME && pieceColor == C_BLACK_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaBlackKnight(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_KNIGHT_NAME && pieceColor == C_BLACK_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaBlackPawn(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_PAWN_NAME && pieceColor == C_BLACK_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaBlackQueen(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_QUEEN_NAME && pieceColor == C_BLACK_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaBlackRook(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_ROOK_NAME && pieceColor == C_BLACK_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaWhiteBishop(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_BISHOP_NAME && pieceColor == C_WHITE_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaWhiteKing(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_KING_NAME && pieceColor == C_WHITE_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaWhiteKnight(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_KNIGHT_NAME && pieceColor == C_WHITE_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaWhitePawn(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_PAWN_NAME && pieceColor == C_WHITE_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaWhiteQueen(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_QUEEN_NAME && pieceColor == C_WHITE_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaWhiteRook(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);
            char pieceName = getPieceName(row, column);

            if (pieceName == C_ROOK_NAME && pieceColor == C_WHITE_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaWhitePiece(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);

            if (pieceColor != '\0' && pieceColor == C_WHITE_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isaBlackPiece(int row, int column)
        {
            char pieceColor = getPieceColor(row, column);

            if (pieceColor != '\0' && pieceColor == C_BLACK_COLOR)
            {
                return true;
            }
            return false;
        }

        private bool isSquareCheckedByBlack(int row, int column)
        {
            int rowAux;
            int columnAux;

            //
            // Check if a BLACK BISHOP (bB) checks the square (S).
            //
            // Move 1: The bishop is up and to the left.
            // bB
            //   S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0 && --columnAux >= 0)
            {
                if (isaBlackBishop(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK BISHOP (bB) checks the square (S).
            // Move 2: The bishop is up and to the right.
            //  bB
            // S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0 && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackBishop(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK BISHOP (bB) checks the square (S).
            // Move 3: The bishop is down and to the right.
            // S
            //  bB
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackBishop(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK BISHOP (bB) checks the square (S).
            // Move 4: The bishop is down and to the left.
            //   S
            // bB
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH && --columnAux >= 0)
            {
                if (isaBlackBishop(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            //
            // Check if the BLACK KING (bK) checks the square (S).
            //
            // Move 1: The king is up and to the left.
            // bK
            //   S
            rowAux = row;
            columnAux = column;
            if (--rowAux >= 0 && --columnAux >= 0)
            {
                if (isaBlackKing(rowAux, columnAux)) { return true; }
            }

            // Check if the BLACK KING (bK) checks the square (S).
            // Move 2: The king is up.
            // bK
            //
            // S
            rowAux = row;
            columnAux = column;
            if (--rowAux >= 0)
            {
                if (isaBlackKing(rowAux, columnAux)) { return true; }
            }

            // Check if the BLACK KING (bK) checks the square (S).
            // Move 3: The king is up and to the right.
            //  bK
            // S
            rowAux = row;
            columnAux = column;
            if (--rowAux >= 0 && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackKing(rowAux, columnAux)) { return true; }
            }

            // Check if the BLACK KING (bK) checks the square (S).
            // Move 4: The king is on the right.
            // S    bK
            rowAux = row;
            columnAux = column;
            if (++columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackKing(rowAux, columnAux)) { return true; }
            }

            // Check if the BLACK KING (bK) checks the square (S).
            // Move 5: The king is down and to the right.
            // S
            //  bK
            rowAux = row;
            columnAux = column;
            if (++rowAux < C_BOARD_LENGTH && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackKing(rowAux, columnAux)) { return true; }
            }

            // Check if the BLACK KING (bK) checks the square (S).
            // Move 6: The king is down.
            // S
            //
            // bK
            rowAux = row;
            columnAux = column;
            if (++rowAux < C_BOARD_LENGTH)
            {
                if (isaBlackKing(rowAux, columnAux)) { return true; }
            }

            // Check if the BLACK KING (bK) checks the square (S).
            // Move 7: The king is down and to the left.
            //   S
            // bK
            rowAux = row;
            columnAux = column;
            if (++rowAux < C_BOARD_LENGTH && --columnAux >= 0)
            {
                if (isaBlackKing(rowAux, columnAux)) { return true; }
            }

            // Check if the BLACK KING (bK) checks the square (S).
            // Move 8: The king is on the left.
            // bK    S
            rowAux = row;
            columnAux = column;
            if (--columnAux >= 0)
            {
                if (isaBlackKing(rowAux, columnAux)) { return true; }
            }

            //
            // Check if a BLACK KNIGHT (bK) checks the square (S).
            //
            // Move 1:
            // bK
            //
            //   S
            rowAux = row - 2;
            columnAux = column - 1;
            if (rowAux >= 0 && columnAux >= 0)
            {
                if (isaBlackKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a BLACK KNIGHT (bK) checks the square (S).
            // Move 2:
            //  bK
            //
            // S
            rowAux = row - 2;
            columnAux = column + 1;
            if (rowAux >= 0 && columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a BLACK KNIGHT (bK) checks the square (S).
            // Move 3:
            //  ____bK
            // S
            // 
            rowAux = row - 1;
            columnAux = column + 2;
            if (rowAux >= 0 && columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a BLACK KNIGHT (bK) checks the square (S).
            // Move 4:
            // S 
            //  ____bK
            // 
            rowAux = row + 1;
            columnAux = column + 2;
            if (rowAux < C_BOARD_LENGTH && columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a BLACK KNIGHT (bK) checks the square (S).
            // Move 5:
            // S
            //
            //  bK
            rowAux = row + 2;
            columnAux = column + 1;
            if (rowAux < C_BOARD_LENGTH && columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a BLACK KNIGHT (bK) checks the square (S).
            // Move 6:
            //   S
            //
            // bK
            rowAux = row + 2;
            columnAux = column - 1;
            if (rowAux < C_BOARD_LENGTH && columnAux >= 0)
            {
                if (isaBlackKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a BLACK KNIGHT (bK) checks the square (S).
            // Move 7:
            //   ____S
            // bK 
            rowAux = row + 1;
            columnAux = column - 2;
            if (rowAux < C_BOARD_LENGTH && columnAux >= 0)
            {
                if (isaBlackKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a BLACK KNIGHT (bK) checks the square (S).
            // Move 8:
            // bK
            //   ____S
            rowAux = row - 1;
            columnAux = column - 2;
            if (rowAux >= 0 && columnAux >= 0)
            {
                if (isaBlackKnight(rowAux, columnAux)) { return true; }
            }

            //
            // Check if a BLACK PAWN (bP) checks the square (S).
            //
            if (isWhitePiecesAtBottom)
            {
                // The pawn is at the top left.
                // bP
                //   S
                rowAux = row - 1;
                columnAux = column - 1;
                if (rowAux >= 0 && columnAux >= 0)
                {
                    if (isaBlackPawn(rowAux, columnAux)) { return true; }
                }

                // The pawn is at the top right.
                //  bP
                // S
                rowAux = row - 1;
                columnAux = column + 1;
                if (rowAux >= 0 && columnAux < C_BOARD_LENGTH)
                {
                    if (isaBlackPawn(rowAux, columnAux)) { return true; }
                }
            }
            else
            {
                // The pawn is at the bottom left.
                //    S
                // bP  
                rowAux = row + 1;
                columnAux = column - 1;
                if (rowAux < C_BOARD_LENGTH && columnAux >= 0)
                {
                    if (isaBlackPawn(rowAux, columnAux)) { return true; }
                }

                // The pawn is at the bottom right.
                // S 
                //   bP
                rowAux = row + 1;
                columnAux = column + 1;
                if (rowAux < C_BOARD_LENGTH && columnAux < C_BOARD_LENGTH)
                {
                    if (isaBlackPawn(rowAux, columnAux)) { return true; }
                }
            }
            //
            // Check if a BLACK QUEEN (bQ) checks the square (S).
            //
            // Move 1: The queen is up and to the left.
            // bQ
            //   S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0 && --columnAux >= 0)
            {
                if (isaBlackQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK QUEEN (bQ) checks the square (S).
            // Move 2: The queen is up.
            // bQ
            //
            // S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0)
            {
                if (isaBlackQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK QUEEN (bQ) checks the square (S).
            // Move 3: The queen is up and to the right.
            //  bQ
            // S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0 && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK QUEEN (bQ) checks the square (S).
            // Move 4: The queen is on the right.
            // S    bQ
            rowAux = row;
            columnAux = column;
            while (++columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK QUEEN (bQ) checks the square (S).
            // Move 5: The queen is down and to the right.
            // S
            //  bQ
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK QUEEN (bQ) checks the square (S).
            // Move 6: The queen is down.
            // S
            //
            // bQ
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH)
            {
                if (isaBlackQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK QUEEN (bQ) checks the square (S).
            // Move 7: The queen is down and to the left.
            //   S
            // bQ
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH && --columnAux >= 0)
            {
                if (isaBlackQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK QUEEN (bQ) checks the square (S).
            // Move 8: The queen is on the left.
            // bQ    S
            rowAux = row;
            columnAux = column;
            while (--columnAux >= 0)
            {
                if (isaBlackQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            //
            // Check if a BLACK ROOK (bt) checks the square (S).
            //
            // Move 1: The rook is up.
            // bT
            //
            // S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0)
            {
                if (isaBlackRook(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK ROOK (bt) checks the square (S).
            // Move 2: The rook is on the right.
            // S    bT
            rowAux = row;
            columnAux = column;
            while (++columnAux < C_BOARD_LENGTH)
            {
                if (isaBlackRook(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK ROOK (bt) checks the square (S).
            // Move 3: The rook is down.
            // S
            //
            // bT
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH)
            {
                if (isaBlackRook(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            // Check if a BLACK ROOK (bt) checks the square (S).
            // Move 4: The rook is on the left.
            // bT    S
            rowAux = row;
            columnAux = column;
            while (--columnAux >= 0)
            {
                if (isaBlackRook(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaWhiteKing(rowAux, columnAux)) { break; }
            }

            return false;
        }

        private bool isSquareCheckedByWhite(int row, int column)
        {
            int rowAux;
            int columnAux;

            //
            // Check if a WHITE BISHOP (wB) checks the square (S).
            //
            // Move 1: The bishop is up and to the left.
            // wB
            //   S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0 && --columnAux >= 0)
            {
                if (isaWhiteBishop(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE BISHOP (wB) checks the square (S).
            // Move 2: The bishop is up and to the right.
            //  wB
            // S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0 && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteBishop(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE BISHOP (wB) checks the square (S).
            // Move 3: The bishop is down and to the right.
            // S
            //  wB
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteBishop(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE BISHOP (wB) checks the square (S).
            // Move 4: The bishop is down and to the left.
            //   S
            // wB
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH && --columnAux >= 0)
            {
                if (isaWhiteBishop(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            //
            // Check if the WHITE KING (wK) checks the square (S).
            //
            // Move 1: The king is up and to the left.
            // wK
            //   S
            rowAux = row;
            columnAux = column;
            if (--rowAux >= 0 && --columnAux >= 0)
            {
                if (isaWhiteKing(rowAux, columnAux)) { return true; }
            }

            // Check if the WHITE KING (wK) checks the square (S).
            // Move 2: The king is up.
            // wK
            //
            // S
            rowAux = row;
            columnAux = column;
            if (--rowAux >= 0)
            {
                if (isaWhiteKing(rowAux, columnAux)) { return true; }
            }

            // Check if the WHITE KING (wK) checks the square (S).
            // Move 3: The king is up and to the right.
            //  wK
            // S
            rowAux = row;
            columnAux = column;
            if (--rowAux >= 0 && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteKing(rowAux, columnAux)) { return true; }
            }

            // Check if the WHITE KING (wK) checks the square (S).
            // Move 4: The king is on the right.
            // S    wK
            rowAux = row;
            columnAux = column;
            if (++columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteKing(rowAux, columnAux)) { return true; }
            }

            // Check if the WHITE KING (wK) checks the square (S).
            // Move 5: The king is down and to the right.
            // S
            //  wK
            rowAux = row;
            columnAux = column;
            if (++rowAux < C_BOARD_LENGTH && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteKing(rowAux, columnAux)) { return true; }
            }

            // Check if the WHITE KING (wK) checks the square (S).
            // Move 6: The king is down.
            // S
            //
            // wK
            rowAux = row;
            columnAux = column;
            if (++rowAux < C_BOARD_LENGTH)
            {
                if (isaWhiteKing(rowAux, columnAux)) { return true; }
            }

            // Check if the WHITE KING (wK) checks the square (S).
            // Move 7: The king is down and to the left.
            //   S
            // wK
            rowAux = row;
            columnAux = column;
            if (++rowAux < C_BOARD_LENGTH && --columnAux >= 0)
            {
                if (isaWhiteKing(rowAux, columnAux)) { return true; }
            }

            // Check if the WHITE KING (wK) checks the square (S).
            // Move 8: The king is on the left.
            // wK    S
            rowAux = row;
            columnAux = column;
            if (--columnAux >= 0)
            {
                if (isaWhiteKing(rowAux, columnAux)) { return true; }
            }

            //
            // Check if a WHITE KNIGHT (wK) checks the square (S).
            //
            // Move 1:
            // wK
            //
            //   S
            rowAux = row - 2;
            columnAux = column - 1;
            if (rowAux >= 0 && columnAux >= 0)
            {
                if (isaWhiteKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a WHITE KNIGHT (wK) checks the square (S).
            // Move 2:
            //  wK
            //
            // S
            rowAux = row - 2;
            columnAux = column + 1;
            if (rowAux >= 0 && columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a WHITE KNIGHT (wK) checks the square (S).
            // Move 3:
            //  ____wK
            // S
            // 
            rowAux = row - 1;
            columnAux = column + 2;
            if (rowAux >= 0 && columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a WHITE KNIGHT (wK) checks the square (S).
            // Move 4:
            // S 
            //  ____wK
            // 
            rowAux = row + 1;
            columnAux = column + 2;
            if (rowAux < C_BOARD_LENGTH && columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a WHITE KNIGHT (wK) checks the square (S).
            // Move 5:
            // S
            //
            //  wK
            rowAux = row + 2;
            columnAux = column + 1;
            if (rowAux < C_BOARD_LENGTH && columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a WHITE KNIGHT (wK) checks the square (S).
            // Move 6:
            //   S
            //
            // wK
            rowAux = row + 2;
            columnAux = column - 1;
            if (rowAux < C_BOARD_LENGTH && columnAux >= 0)
            {
                if (isaWhiteKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a WHITE KNIGHT (wK) checks the square (S).
            // Move 7:
            //   ____S
            // wK 
            rowAux = row + 1;
            columnAux = column - 2;
            if (rowAux < C_BOARD_LENGTH && columnAux >= 0)
            {
                if (isaWhiteKnight(rowAux, columnAux)) { return true; }
            }

            // Check if a WHITE KNIGHT (wK) checks the square (S).
            // Move 8:
            // wK
            //   ____S
            rowAux = row - 1;
            columnAux = column - 2;
            if (rowAux >= 0 && columnAux >= 0)
            {
                if (isaWhiteKnight(rowAux, columnAux)) { return true; }
            }

            //
            // Check if a WHITE PAWN (wP) checks the square (S).
            //
            if (isWhitePiecesAtBottom)
            {
                // The pawn is down and to the left.
                //   S
                // wP
                rowAux = row + 1;
                columnAux = column - 1;
                if (rowAux >= 0 && columnAux >= 0)
                {
                    if (isaWhitePawn(rowAux, columnAux)) { return true; }
                }

                // The pawn is down and to the right.
                // S
                //   wP
                rowAux = row + 1;
                columnAux = column + 1;
                if (rowAux >= 0 && columnAux < C_BOARD_LENGTH)
                {
                    if (isaWhitePawn(rowAux, columnAux)) { return true; }
                }
            }
            else
            {
                // The pawn is at the bottom left.
                // wP
                //    S
                rowAux = row - 1;
                columnAux = column - 1;
                if (rowAux >= 0 && columnAux >= 0)
                {
                    if (isaWhitePawn(rowAux, columnAux)) { return true; }
                }

                // The pawn is at the bottom right.
                //   wP
                // S   
                rowAux = row - 1;
                columnAux = column + 1;
                if (rowAux >= 0 && columnAux < C_BOARD_LENGTH)
                {
                    if (isaWhitePawn(rowAux, columnAux)) { return true; }
                }
            }
            //
            // Check if a WHITE QUEEN (wQ) checks the square (S).
            //
            // Move 1: The queen is up and to the left.
            // wQ
            //   S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0 && --columnAux >= 0)
            {
                if (isaWhiteQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE QUEEN (wQ) checks the square (S).
            // Move 2: The queen is up.
            // wQ
            //
            // S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0)
            {
                if (isaWhiteQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE QUEEN (wQ) checks the square (S).
            // Move 3: The queen is up and to the right.
            //  wQ
            // S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0 && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE QUEEN (wQ) checks the square (S).
            // Move 4: The queen is on the right.
            // S    wQ
            rowAux = row;
            columnAux = column;
            while (++columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE QUEEN (wQ) checks the square (S).
            // Move 5: The queen is down and to the right.
            // S
            //  wQ
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH && ++columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE QUEEN (wQ) checks the square (S).
            // Move 6: The queen is down.
            // S
            //
            // wQ
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH)
            {
                if (isaWhiteQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE QUEEN (wQ) checks the square (S).
            // Move 7: The queen is down and to the left.
            //   S
            // wQ
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH && --columnAux >= 0)
            {
                if (isaWhiteQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE QUEEN (wQ) checks the square (S).
            // Move 8: The queen is on the left.
            // wQ    S
            rowAux = row;
            columnAux = column;
            while (--columnAux >= 0)
            {
                if (isaWhiteQueen(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            //
            // Check if a WHITE ROOK (wT) checks the square (S).
            //
            // Move 1: The rook is up.
            // wT
            //
            // S
            rowAux = row;
            columnAux = column;
            while (--rowAux >= 0)
            {
                if (isaWhiteRook(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE ROOK (sT) checks the square (S).
            // Move 2: The rook is on the right.
            // S    wT
            rowAux = row;
            columnAux = column;
            while (++columnAux < C_BOARD_LENGTH)
            {
                if (isaWhiteRook(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE ROOK (wT) checks the square (S).
            // Move 3: The rook is down.
            // S
            //
            // wT
            rowAux = row;
            columnAux = column;
            while (++rowAux < C_BOARD_LENGTH)
            {
                if (isaWhiteRook(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            // Check if a WHITE ROOK (wT) checks the square (S).
            // Move 4: The rook is on the left.
            // wT    S
            rowAux = row;
            columnAux = column;
            while (--columnAux >= 0)
            {
                if (isaWhiteRook(rowAux, columnAux)) { return true; }
                else if (!isSquareEmpty(rowAux, columnAux) &&
                    !isaBlackKing(rowAux, columnAux)) { break; }
            }

            return false;
        }

        private bool isaTrappedKing(char kingColor)
        {
            // 
            if (kingColor == C_WHITE_COLOR)
            {
                // K
                // Check the upper left adjacent square to the king.
                // X
                //  K
                int rowAux = whiteKingCoordinates_.X - 1;
                int columnAux = whiteKingCoordinates_.Y - 1;
                if (rowAux >= 0 && columnAux >= 0)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_WHITE_COLOR) ||
                        isSquareCheckedByBlack(rowAux, columnAux))) { return false; }
                }

                // X
                // K
                rowAux = whiteKingCoordinates_.X - 1;
                columnAux = whiteKingCoordinates_.Y;
                if (rowAux >= 0)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_WHITE_COLOR) ||
                        isSquareCheckedByBlack(rowAux, columnAux))) { return false; }
                }

                //  X
                // K
                rowAux = whiteKingCoordinates_.X - 1;
                columnAux = whiteKingCoordinates_.Y + 1;
                if (rowAux >= 0 && columnAux < C_BOARD_LENGTH)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_WHITE_COLOR) ||
                        isSquareCheckedByBlack(rowAux, columnAux))) { return false; }
                }

                // K X
                rowAux = whiteKingCoordinates_.X;
                columnAux = whiteKingCoordinates_.Y + 1;
                if (columnAux < C_BOARD_LENGTH)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_WHITE_COLOR) ||
                        isSquareCheckedByBlack(rowAux, columnAux))) { return false; }
                }

                // K
                //  X
                rowAux = whiteKingCoordinates_.X + 1;
                columnAux = whiteKingCoordinates_.Y + 1;
                if (rowAux < C_BOARD_LENGTH && columnAux < C_BOARD_LENGTH)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_WHITE_COLOR) ||
                        isSquareCheckedByBlack(rowAux, columnAux))) { return false; }
                }

                // K
                // X
                rowAux = whiteKingCoordinates_.X + 1;
                columnAux = whiteKingCoordinates_.Y;
                if (rowAux < C_BOARD_LENGTH)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_WHITE_COLOR) ||
                        isSquareCheckedByBlack(rowAux, columnAux))) { return false; }
                }

                //  K
                // X
                rowAux = whiteKingCoordinates_.X + 1;
                columnAux = whiteKingCoordinates_.Y - 1;
                if (rowAux < C_BOARD_LENGTH && columnAux >= 0)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_WHITE_COLOR) ||
                        isSquareCheckedByBlack(rowAux, columnAux))) { return false; }
                }

                // X K
                rowAux = whiteKingCoordinates_.X;
                columnAux = whiteKingCoordinates_.Y - 1;
                if (columnAux >= 0)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_WHITE_COLOR) ||
                        isSquareCheckedByBlack(rowAux, columnAux))) { return false; }
                }

                return true;
            }
            // 
            else if (kingColor == C_BLACK_COLOR)
            {
                // X
                //   K
                int rowAux = blackKingCoordinates_.X - 1;
                int columnAux = blackKingCoordinates_.Y - 1;
                if (rowAux >= 0 && columnAux >= 0)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_BLACK_COLOR) ||
                        isSquareCheckedByWhite(rowAux, columnAux))) { return false; }
                }

                // X
                // K
                rowAux = blackKingCoordinates_.X - 1;
                columnAux = blackKingCoordinates_.Y;
                if (rowAux >= 0)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_BLACK_COLOR) ||
                         isSquareCheckedByWhite(rowAux, columnAux))) { return false; }
                }

                //  X
                // K
                rowAux = blackKingCoordinates_.X - 1;
                columnAux = blackKingCoordinates_.Y + 1;
                if (rowAux >= 0 && columnAux < C_BOARD_LENGTH)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_BLACK_COLOR) ||
                        isSquareCheckedByWhite(rowAux, columnAux))) { return false; }
                }

                // K X
                rowAux = blackKingCoordinates_.X;
                columnAux = blackKingCoordinates_.Y + 1;
                if (columnAux < C_BOARD_LENGTH)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_BLACK_COLOR) ||
                        isSquareCheckedByWhite(rowAux, columnAux))) { return false; }
                }

                // K
                //  X
                rowAux = blackKingCoordinates_.X + 1;
                columnAux = blackKingCoordinates_.Y + 1;
                if (rowAux < C_BOARD_LENGTH && columnAux < C_BOARD_LENGTH)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_BLACK_COLOR) ||
                        isSquareCheckedByWhite(rowAux, columnAux))) { return false; }
                }

                // K
                // X
                rowAux = blackKingCoordinates_.X + 1;
                columnAux = blackKingCoordinates_.Y;
                if (rowAux < C_BOARD_LENGTH)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_BLACK_COLOR) ||
                        isSquareCheckedByWhite(rowAux, columnAux))) { return false; }
                }

                //  K
                // X
                rowAux = blackKingCoordinates_.X + 1;
                columnAux = blackKingCoordinates_.Y - 1;
                if (rowAux < C_BOARD_LENGTH && columnAux >= 0)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_BLACK_COLOR) ||
                        isSquareCheckedByWhite(rowAux, columnAux))) { return false; }
                }

                // X K
                rowAux = blackKingCoordinates_.X;
                columnAux = blackKingCoordinates_.Y - 1;
                if (columnAux >= 0)
                {
                    if (!(isaOwnPiece(rowAux, columnAux, C_BLACK_COLOR) ||
                        isSquareCheckedByWhite(rowAux, columnAux))) { return false; }
                }

                return true;
            }

            return false;
        }

        private bool isCheckmate(char kingColor)
        {
            if (kingColor == C_WHITE_COLOR)
            {
                if (isaTrappedKing(C_WHITE_COLOR))
                {
                    for (int i = 0; i < aa_board.Length; i++)
                    {
                        for (int j = 0; j < aa_board[0].Length; j++)
                        {
                            if (isaWhitePiece(i, j) && !isaWhiteKing(i, j))
                            {
                                setValidMoves(i, j);
                                if (isThereValidMoves())
                                {
                                    for (int k = 0; k < validMoves.Count; k++)
                                    {
                                        Move m = validMoves[k];
                                        if (!isChecked(i, j, m.coordinates.X, m.coordinates.Y, C_WHITE_COLOR))
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            // King color is black.
            else
            {
                if (isaTrappedKing(C_BLACK_COLOR))
                {
                    for (int i = 0; i < aa_board.Length; i++)
                    {
                        for (int j = 0; j < aa_board[0].Length; j++)
                        {
                            if (isaBlackPiece(i, j) && !isaBlackKing(i, j))
                            {
                                setValidMoves(i, j);
                                if (isThereValidMoves())
                                {
                                    for (int k = 0; k < validMoves.Count; k++)
                                    {
                                        Move m = validMoves[k];
                                        if (!isChecked(i, j, m.coordinates.X, m.coordinates.Y, C_BLACK_COLOR))
                                        {
                                            unsetValidMoves();
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        private bool isStalemate(char kingColor)
        {

            // Is white king stalemated?:
            if (kingColor == C_WHITE_COLOR)
            {
                if (isaTrappedKing(C_WHITE_COLOR))
                {
                    // Check if player can't move its pieces.
                    for (int i = 0; i < aa_board.Length; i++)
                    {
                        for (int j = 0; j < aa_board[0].Length; j++)
                        {
                            if (isaWhitePiece(i, j) && !isaWhiteKing(i, j))
                            {
                                setValidMoves(i, j);
                                if (isThereValidMoves())
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            // Is black king stalemated?:
            else if (kingColor == C_BLACK_COLOR)
            {
                if (isaTrappedKing(C_BLACK_COLOR))
                {
                    // Check if player can't move its pieces.
                    for (int i = 0; i < aa_board.Length; i++)
                    {
                        for (int j = 0; j < aa_board[0].Length; j++)
                        {
                            if (isaBlackPiece(i, j) && !isaBlackKing(i, j))
                            {
                                setValidMoves(i, j);
                                if (isThereValidMoves())
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    return true;
                }
            }

            return false;

        }

        private bool isTripleRepetitionOfMoves()
        {
            bool tripleRepetitionOfMoves = false;
            bool positionFound = false;
            Point[] piecePositions = new Point[33];

            // Build a new vector with the last board position.
            /*
            Array.Copy(piecePositionsHistory[piecePositionsHistory.Count - 1], piecePositions, 33);
            piecePositions[] = -1;
            piecePositions[] = -1;
            */

            for (int i = 0; i < aa_pieceIndexes[0].Length; i++)
            {
                for (int j = 0; j < aa_pieceIndexes[0].Length; j++)
                {
                    if (aa_pieceIndexes[i][j] != -1)
                    {
                        piecePositions[aa_pieceIndexes[i][j]] = new Point(i, j);
                    }
                }
            }

            piecePositions[32] = new Point(1, -1);

            // Check if the last board position is in the list of
            // board positions. If it is, then increment the number of times
            // the position appears in the game. If it is not, then insert the
            // last board position in the list of positions.
            for (int i = piecePositionsHistory.Count - 1; i > -1; i--)
            {
                if (tripleRepetitionOfMoves || positionFound)
                {
                    break;
                }

                Point[] piecePositionsAux = piecePositionsHistory[i];

                // Don't compare the last element.
                for (int j = 0; j < piecePositionsAux.Length - 1; j++)
                {
                    // The last board position...
                    if (piecePositionsAux[j] != piecePositions[j])
                    {
                        break;
                    }

                    // We've found the last board position in the list of board positions.
                    if (j == piecePositionsAux.Length - 2)
                    {
                        // The same position has been played three times.
                        // Therefore, the game ends in a draw.
                        if (piecePositionsHistory[i][32].X == 2)
                        {
                            tripleRepetitionOfMoves = true;
                        }
                        // Increment the number of times the position appears in the game.
                        else
                        {
                            piecePositionsHistory[i][32].X++;
                            positionFound = true;
                        }
                    }
                }
            }

            if (!positionFound)
            {
                piecePositionsHistory.Add(piecePositions);
            }

            return tripleRepetitionOfMoves;
        }

        public Move movePiece(int originX, int originY, int targetX, int targetY)
        {
            // Make sure the indexes are within the bounds of the board.
            if (originX < 0 || originY < 0 ||
                originX > C_BOARD_LENGTH_MINUS_1 || originY > C_BOARD_LENGTH_MINUS_1)
            {
                return null!;
            }

            if (targetX < 0 || targetY < 0 ||
                targetX > C_BOARD_LENGTH_MINUS_1 || targetY > C_BOARD_LENGTH_MINUS_1)
            {
                return null!;
            }

            char squareColor;
            char pieceColor;
            char pieceName;
            char[] ac_newTargetSquare;
            string newTargetSquare;
            int pieceIndex;

            // In the previous turn a pawn moved two squares forward.
            // We untag the fast pawn by removing the C_FAST_PAWN character.
            Point fastPawnCoordinates = getFastPawnCoordinates();
            if (fastPawnCoordinates.X != -1)
            {
                pieceColor = getPieceColor(fastPawnCoordinates.X, fastPawnCoordinates.Y);
                squareColor = getSquareColor(fastPawnCoordinates.X, fastPawnCoordinates.Y);
                pieceName = getPieceName(fastPawnCoordinates.X, fastPawnCoordinates.Y);

                // Remove the C_FAST_PAWN character.
                char[] a_newPiece = { pieceColor, pieceName, squareColor };
                aa_board[fastPawnCoordinates.X][fastPawnCoordinates.Y] = new string(a_newPiece);

                // Unset the fast pawn's coordinates.
                unsetFastPawnCoordinates();
            }

            Move move = GetMoveFromValidMoves(targetX, targetY);
            squareColor = getSquareColor(originX, originY);
            pieceColor = getPieceColor(originX, originY);
            pieceName = getPieceName(originX, originY);
            pieceIndex = aa_pieceIndexes[originX][originY];

            //
            // Invalidates castling if the king or the rook has been moved.
            //

            // If a king moves and the king hasn't castled, then
            // make the castling move not available anymore.
            if (pieceName == C_KING_NAME)
            {
                // Save the king's coordinates.
                // They are used to evaluate the king checks.
                if (pieceColor == C_WHITE_COLOR)
                {
                    whiteKingCoordinates_.X = targetX;
                    whiteKingCoordinates_.Y = targetY;

                    // The king has moved and hasn't castled. So, 
                    // the castling move is not valid anymore.
                    if (isWhiteShortCastlingValid &&
                        move.moveType != Move.moveTypes.shortCastling)
                    {
                        isWhiteShortCastlingValid = false;
                    }

                    // The king has moved and hasn't castled. So, 
                    // the castling move is not valid anymore.
                    if (isWhiteLongCastlingValid &&
                        move.moveType != Move.moveTypes.longCastling)
                    {
                        isWhiteLongCastlingValid = false;
                    }
                }
                else if (pieceColor == C_BLACK_COLOR)
                {
                    blackKingCoordinates_.X = targetX;
                    blackKingCoordinates_.Y = targetY;

                    // The king has moved and hasn't castled. So, 
                    // the castling move is not valid anymore.
                    if (isBlackShortCastlingValid &&
                    move.moveType != Move.moveTypes.shortCastling)
                    {
                        isBlackShortCastlingValid = false;
                    }

                    // The king has moved and hasn't castled. So, 
                    // the castling move is not valid anymore.
                    if (isBlackLongCastlingValid &&
                    move.moveType != Move.moveTypes.longCastling)
                    {
                        isBlackLongCastlingValid = false;
                    }
                }
            }
            // If a rook moves and the king hasn't castled, then
            // make the castling move not available anymore.
            else if (pieceName == C_ROOK_NAME)
            {
                if (pieceColor == C_WHITE_COLOR)
                {
                    if (isWhitePiecesAtBottom)
                    {
                        // A rook has moved and the king hasn't castled. 
                        if (isWhiteShortCastlingValid &&
                            move.moveType != Move.moveTypes.shortCastling &&
                            originX == 7 && originY == 7)
                        {
                            // Make the castling move not available.
                            isWhiteShortCastlingValid = false;
                        }

                        // A rook has moved and the king hasn't castled. 
                        if (isWhiteLongCastlingValid &&
                            move.moveType != Move.moveTypes.longCastling &&
                            originX == 7 && originY == 0)
                        {
                            // Make the castling move not available.
                            isWhiteLongCastlingValid = false;
                        }
                    }
                    // White pieces is at upper.
                    else
                    {
                        // A rook has moved and the king hasn't castled. 
                        if (isWhiteShortCastlingValid &&
                            move.moveType != Move.moveTypes.shortCastling &&
                            originX == 0 && originY == 0)
                        {
                            // Make the castling move not available.
                            isWhiteShortCastlingValid = false;
                        }

                        // A rook has moved and the king hasn't castled. 
                        if (isWhiteLongCastlingValid &&
                            move.moveType != Move.moveTypes.longCastling &&
                            originX == 0 && originY == 7)
                        {
                            // Make the castling move not available.
                            isWhiteLongCastlingValid = false;
                        }
                    }
                }
                // Piece's color is black.
                else
                {
                    if (isWhitePiecesAtBottom)
                    {
                        // A rook has moved and the king hasn't castled. 
                        if (isBlackShortCastlingValid &&
                            move.moveType != Move.moveTypes.shortCastling &&
                            originX == 0 && originY == 7)
                        {
                            // Make the castling move not available.
                            isBlackShortCastlingValid = false;
                        }

                        // A rook has moved and the king hasn't castled. 
                        if (isBlackLongCastlingValid &&
                            move.moveType != Move.moveTypes.longCastling &&
                            originX == 0 && originY == 0)
                        {
                            // Make the castling move not available.
                            isBlackLongCastlingValid = false;
                        }
                    }
                    // White pieces is at upper.
                    else
                    {
                        // A rook has moved and the king hasn't castled. 
                        if (isBlackShortCastlingValid &&
                            move.moveType != Move.moveTypes.shortCastling &&
                            originX == 7 && originY == 0)
                        {
                            // Make the castling move not available.
                            isBlackShortCastlingValid = false;
                        }

                        // A rook has moved and the king hasn't castled. 
                        if (isBlackLongCastlingValid &&
                            move.moveType != Move.moveTypes.longCastling &&
                            originX == 7 && originY == 7)
                        {
                            // Make the castling move not available.
                            isBlackLongCastlingValid = false;
                        }
                    }
                }
            }

            // A pawn has been moved or a capture has been made,
            // so reset the moves counter, according to the fifty-move rule.
            if (pieceName == C_PAWN_NAME || !isSquareEmpty(targetX, targetY))
            {
                movesCounter = 0;
            }
            else
            {
                movesCounter++;
            }

            // A capture has been made, so update the number of pieces.
            if (!isSquareEmpty(targetX, targetY))
            {
                char targetPieceName = getPieceName(targetX, targetY);
                char targetPieceColor = getPieceColor(targetX, targetY);
                if (targetPieceColor == C_BLACK_COLOR)
                {
                    switch (targetPieceName)
                    {
                        case C_BISHOP_NAME:
                            if (squareColor == C_BLACK_COLOR)
                            {
                                numberOfBlackPieces[0]--;
                            }
                            else if (squareColor == C_WHITE_COLOR)
                            {
                                numberOfBlackPieces[1]--;
                            }
                            break;
                        case C_KNIGHT_NAME:
                            numberOfBlackPieces[2]--;
                            break;
                        case C_PAWN_NAME:
                            numberOfBlackPieces[3]--;
                            break;
                        case C_QUEEN_NAME:
                            numberOfBlackPieces[4]--;
                            break;
                        case C_ROOK_NAME:
                            numberOfBlackPieces[5]--;
                            break;
                    }
                }
                else if (targetPieceColor == C_WHITE_COLOR)
                {
                    switch (targetPieceName)
                    {
                        case C_BISHOP_NAME:
                            if (squareColor == C_BLACK_COLOR)
                            {
                                numberOfWhitePieces[0]--;
                            }
                            else if (squareColor == C_WHITE_COLOR)
                            {
                                numberOfWhitePieces[1]--;
                            }
                            break;
                        case C_KNIGHT_NAME:
                            numberOfWhitePieces[2]--;
                            break;
                        case C_PAWN_NAME:
                            numberOfWhitePieces[3]--;
                            break;
                        case C_QUEEN_NAME:
                            numberOfWhitePieces[4]--;
                            break;
                        case C_ROOK_NAME:
                            numberOfWhitePieces[5]--;
                            break;
                    }
                }
            }

            // Put an empty square in the piece's previous position.
            if (squareColor == '\0') { return null!; }
            aa_board[originX][originY] = squareColor.ToString();

            // Unset an element in the index matrix:
            aa_pieceIndexes[originX][originY] = -1;

            // Retrieve the new position's square color.
            squareColor = getSquareColor(targetX, targetY);
            if (pieceColor == '\0' || pieceName == '\0' || squareColor == '\0')
            {
                return null!;
            }

            //
            // Put the piece/s in its new position.
            //
            switch (move.moveType)
            {
                // A piece has been moved.
                case Move.moveTypes.Normal:
                    // Put the piece in its new position.
                    // (the piece's previous position has been emptied above)
                    ac_newTargetSquare = new char[] { pieceColor, pieceName, squareColor };
                    newTargetSquare = new string(ac_newTargetSquare);
                    aa_board[targetX][targetY] = newTargetSquare;

                    // Put the index in its new position:
                    aa_pieceIndexes[targetX][targetY] = pieceIndex;
                    break;

                // A pawn has been moved two squares forward.
                case Move.moveTypes.FastPawn:
                    // Tag the piece as fast pawn, by adding the C_FAST_PAWN character.
                    ac_newTargetSquare = new char[] { pieceColor, pieceName, C_FAST_PAWN, squareColor };
                    newTargetSquare = new string(ac_newTargetSquare);

                    // Put the piece in its new position.
                    // (the piece's previous position has been emptied above)
                    aa_board[targetX][targetY] = newTargetSquare;
                    // Record the fast pawn's coordinates, to untag the pawn in the next turn.
                    setFastPawnCoordinates(targetX, targetY);

                    // Put the index in its new position:
                    aa_pieceIndexes[targetX][targetY] = pieceIndex;
                    break;

                // A pawn has reached the last row, and it will become a queen.
                case Move.moveTypes.PawnPromotion:
                    // Tag the piece as queen, by adding the 'D' character.
                    ac_newTargetSquare = new char[] { pieceColor, C_QUEEN_NAME, squareColor };
                    newTargetSquare = new string(ac_newTargetSquare);

                    // Put the piece in its new position.
                    // (the piece's previous position has been emptied above)
                    aa_board[targetX][targetY] = newTargetSquare;

                    // Put the index in its new position:
                    aa_pieceIndexes[targetX][targetY] = pieceIndex;

                    // Update the number of pieces in the board.
                    if (pieceColor == C_BLACK_COLOR)
                    {
                        numberOfBlackPieces[4]++;
                    }
                    else if (pieceColor == C_WHITE_COLOR)
                    {
                        numberOfWhitePieces[4]++;
                    }
                    break;

                // A king castles short.
                case Move.moveTypes.shortCastling:
                    // Put the king in its new position.
                    // (the piece's previous position has been emptied above)
                    ac_newTargetSquare = new char[] { pieceColor, pieceName, squareColor };
                    newTargetSquare = new string(ac_newTargetSquare);
                    aa_board[targetX][targetY] = newTargetSquare;

                    // Put the king's index in its new position:
                    aa_pieceIndexes[targetX][targetY] = pieceIndex;

                    // Put the rook in its new position.
                    if (isWhitePiecesTurn)
                    {
                        if (isWhitePiecesAtBottom)
                        {
                            // Put an empty square in the piece's previous
                            // position (7, 7), and put the rook in its new
                            // position (7, 6).
                            movePiece(7, 7, 7, 5);

                            // Put the rook's index in its new position:
                            aa_pieceIndexes[7][5] = aa_pieceIndexes[7][7];
                            aa_pieceIndexes[7][7] = -1;
                        }
                        else
                        {
                            // Put an empty square in the piece's previous
                            // position (0, 0), and put the rook in its new
                            // position (0, 2).
                            movePiece(0, 0, 0, 2);

                            // Put the rook's index in its new position:
                            aa_pieceIndexes[0][2] = aa_pieceIndexes[0][0];
                            aa_pieceIndexes[0][0] = -1;
                        }

                        isWhiteShortCastlingValid = false;
                    }
                    else
                    {
                        if (isWhitePiecesAtBottom)
                        {
                            // Put an empty square in the piece's previous
                            // position (0, 7), and put the rook in its new
                            // position (0, 5).
                            movePiece(0, 7, 0, 5);

                            // Put the rook's index in its new position:
                            aa_pieceIndexes[0][5] = aa_pieceIndexes[0][7];
                            aa_pieceIndexes[0][7] = -1;
                        }
                        else
                        {
                            // Put an empty square in the piece's previous
                            // position (7, 0), and put the rook in its new
                            // position (7, 2).
                            movePiece(7, 0, 7, 2);

                            // Put the rook's index in its new position:
                            aa_pieceIndexes[7][2] = aa_pieceIndexes[7][0];
                            aa_pieceIndexes[7][0] = -1;
                        }

                        isBlackShortCastlingValid = false;
                    }
                    break;

                // A king castles long.
                case Move.moveTypes.longCastling:
                    // Put the king in its new position.
                    // (the piece's previous position has been emptied above)
                    ac_newTargetSquare = new char[] { pieceColor, pieceName, squareColor };
                    newTargetSquare = new string(ac_newTargetSquare);
                    aa_board[targetX][targetY] = newTargetSquare;

                    // Put the king's index in its new position:
                    aa_pieceIndexes[targetX][targetY] = pieceIndex;

                    // Put the rook in its new position.
                    if (isWhitePiecesTurn)
                    {
                        if (isWhitePiecesAtBottom)
                        {
                            // Put an empty square in the piece's previous
                            // position (7, 0), and put the rook in its new
                            // position (7, 3).
                            movePiece(7, 0, 7, 3);

                            // Put the rook's index in its new position:
                            aa_pieceIndexes[7][3] = aa_pieceIndexes[7][0];
                            aa_pieceIndexes[7][0] = -1;
                        }
                        else
                        {
                            // Put an empty square in the piece's previous
                            // position (0, 7), and put the rook in its new
                            // position (0, 4).
                            movePiece(0, 7, 0, 4);

                            // Put the rook's index in its new position:
                            aa_pieceIndexes[0][4] = aa_pieceIndexes[0][7];
                            aa_pieceIndexes[0][7] = -1;
                        }

                        isWhiteLongCastlingValid = false;
                    }
                    else
                    {
                        if (isWhitePiecesAtBottom)
                        {
                            // Put an empty square in the piece's previous
                            // position (0, 0), and put the rook in its new
                            // position (0, 3).
                            movePiece(0, 0, 0, 3);

                            // Put the rook's index in its new position:
                            aa_pieceIndexes[0][3] = aa_pieceIndexes[0][0];
                            aa_pieceIndexes[0][0] = -1;
                        }
                        else
                        {
                            // Put an empty square in the piece's previous
                            // position (7, 7), and put the rook in its new
                            // position (7, 4).
                            movePiece(7, 7, 7, 4);

                            // Put the rook's index in its new position:
                            aa_pieceIndexes[7][4] = aa_pieceIndexes[7][7];
                            aa_pieceIndexes[7][7] = -1;
                        }

                        isBlackLongCastlingValid = false;
                    }
                    break;

                // A pawn captures another pawn en passant.
                case Move.moveTypes.EnPassant:
                    // Put the piece in its new position.
                    // (the piece's previous position has been emptied above)
                    ac_newTargetSquare = new char[] { pieceColor, pieceName, squareColor };
                    newTargetSquare = new string(ac_newTargetSquare);
                    aa_board[targetX][targetY] = newTargetSquare;

                    // Put the pawn's index in its new position:
                    aa_pieceIndexes[targetX][targetY] = pieceIndex;

                    // Paint an empty square in the opponent pawn's previous position.
                    Point pawnCoordinate = move.secondPiececoordinates;
                    squareColor = getSquareColor(pawnCoordinate.X, pawnCoordinate.Y);
                    if (squareColor == '\0') { return null!; }
                    aa_board[pawnCoordinate.X][pawnCoordinate.Y] = squareColor.ToString();

                    // Unset the captured pawn's index:
                    aa_pieceIndexes[pawnCoordinate.X][pawnCoordinate.Y] = -1;

                    // A capture has been made, so reset the moves counter,
                    // according to the fifty-move rule.
                    movesCounter = 0;

                    break;

                default:
                    break;
            }

            //
            // We have already moved a piece.
            // Now we will check if one player wins or there is a draw.
            //

            // The game ends in a draw if there is a triple repetition of moves.
            if (isTripleRepetitionOfMoves())
            {
                tripleRepetitionOfMoves = true;
            }

            // The game ends in a draw, if no capture has been made and
            // no pawn has been moved in the last fifty moves (for this
            // purpose a "move" consists of a player completing a turn
            // followed by the opponent completing a turn).
            if (movesCounter == 100)
            {
                fiftyMovesRule = true;
            }

            // Check if there are not enough pieces to end the game.
            if (numberOfWhitePieces[3] == 0 &&
                numberOfWhitePieces[4] == 0 &&
                numberOfWhitePieces[5] == 0 &&
                // One knight and zero bishops.
                (numberOfWhitePieces[2] == 1 &&
                numberOfWhitePieces[0] == 0 && 
                numberOfWhitePieces[1] == 0) ||
                // Or zero knights and one bishop.
                (numberOfWhitePieces[2] == 0 &&
                ((numberOfWhitePieces[0] == 1 &&
                numberOfWhitePieces[1] == 0) ||
                (numberOfWhitePieces[0] == 0 &&
                numberOfWhitePieces[1] == 1)))
                )
            {
                if (numberOfBlackPieces[3] == 0 &&
                    numberOfBlackPieces[4] == 0 &&
                    numberOfBlackPieces[5] == 0 &&
                    // One knight and zero bishops.
                    (numberOfBlackPieces[2] == 1 &&
                    numberOfBlackPieces[0] == 0 &&
                    numberOfBlackPieces[1] == 0) ||
                    // Or zero knights and one bishop.
                    (numberOfWhitePieces[2] == 0 &&
                    ((numberOfBlackPieces[0] == 1 &&
                    numberOfBlackPieces[1] == 0) ||
                    (numberOfBlackPieces[0] == 0 &&
                    numberOfBlackPieces[1] == 1))))
                {
                    notEnoughPieces = true;
                }
            }


            // Check if a player wins by checkmate, or
            // if the game ends in a draw by stalemate.
            if (isWhitePiecesTurn)
            {
                // The black king is checked.
                if (isSquareCheckedByWhite(blackKingCoordinates_.X, blackKingCoordinates_.Y))
                {
                    // White player wins the game by checkmate.
                    if (isCheckmate(C_BLACK_COLOR))
                    {
                        isKingChecked = true;
                        isKingCheckmated = true;
                    }
                    else
                    {
                        isKingChecked = true;
                    }
                }
                // Check for a smothered mate.
                else
                {
                    isKingChecked = false;

                    // The game ends in a draw by stalemate.
                    if (isStalemate(C_BLACK_COLOR))
                    {
                        isKingStalemated = true;
                    }
                }
            }
            else if (!isWhitePiecesTurn)
            {
                // The white king is checked.
                if (isSquareCheckedByBlack(whiteKingCoordinates_.X, whiteKingCoordinates_.Y))
                {
                    // Black player wins the game by checkmate.
                    if (isCheckmate(C_WHITE_COLOR))
                    {
                        isKingChecked = true;
                        isKingCheckmated = true;
                    }
                    else
                    {
                        isKingChecked = true;
                    }
                }
                // Check for a smothered mate.
                else
                {
                    isKingChecked = false;

                    // The game ends in a draw by stalemate.
                    if (isStalemate(C_WHITE_COLOR))
                    {
                        isKingStalemated = true;
                    }
                }
            }

            return move;
        }

        /*
           // Values for "column" variable: 'a', 'b', 'c', 'd', 'e', 'f', 'g' or 'h'.
           // Values for "row" variable: '1', '2', '3', '4', '5', '6', '7' or '8'.
           public string getSquare(char column, int row)
           {
               switch (row)
               {
                   case 1:
                       switch (column)
                       {
                           case 'a': return aa_board[8][1];
                           case 'b': return aa_board[8][2];
                           case 'c': return aa_board[8][3];
                           case 'd': return aa_board[8][4];
                           case 'e': return aa_board[8][5];
                           case 'f': return aa_board[8][6];
                           case 'g': return aa_board[8][7];
                           case 'h': return aa_board[8][8];
                           default: return null;
                       }
                       break;
                   case 2:
                       switch (column)
                       {
                           case 'a': return aa_board[7][1];
                           case 'b': return aa_board[7][2];
                           case 'c': return aa_board[7][3];
                           case 'd': return aa_board[7][4];
                           case 'e': return aa_board[7][5];
                           case 'f': return aa_board[7][6];
                           case 'g': return aa_board[7][7];
                           case 'h': return aa_board[7][8];
                           default: return null;
                       }
                       break;
                   case 3:
                       switch (column)
                       {
                           case 'a': return aa_board[6][1];
                           case 'b': return aa_board[6][2];
                           case 'c': return aa_board[6][3];
                           case 'd': return aa_board[6][4];
                           case 'e': return aa_board[6][5];
                           case 'f': return aa_board[6][6];
                           case 'g': return aa_board[6][7];
                           case 'h': return aa_board[6][8];
                           default: return null;
                       }
                       break;
                   case 4:
                       switch (column)
                       {
                           case 'a': return aa_board[5][1];
                           case 'b': return aa_board[5][2];
                           case 'c': return aa_board[5][3];
                           case 'd': return aa_board[5][4];
                           case 'e': return aa_board[5][5];
                           case 'f': return aa_board[5][6];
                           case 'g': return aa_board[5][7];
                           case 'h': return aa_board[5][8];
                           default: return null;

                       }
                       break;
                   case 5:
                       switch (column)
                       {
                           case 'a': return aa_board[4][1];
                           case 'b': return aa_board[4][2];
                           case 'c': return aa_board[4][3];
                           case 'd': return aa_board[4][4];
                           case 'e': return aa_board[4][5];
                           case 'f': return aa_board[4][6];
                           case 'g': return aa_board[4][7];
                           case 'h': return aa_board[4][8];
                           default: return null;
                       }
                       break;
                   case 6:
                       switch (column)
                       {
                           case 'a': return aa_board[3][1];
                           case 'b': return aa_board[3][2];
                           case 'c': return aa_board[3][3];
                           case 'd': return aa_board[3][4];
                           case 'e': return aa_board[3][5];
                           case 'f': return aa_board[3][6];
                           case 'g': return aa_board[3][7];
                           case 'h': return aa_board[3][8];
                           default: return null;
                       }
                       break;
                   case 7:
                       switch (column)
                       {
                           case 'a': return aa_board[2][1];
                           case 'b': return aa_board[2][2];
                           case 'c': return aa_board[2][3];
                           case 'd': return aa_board[2][4];
                           case 'e': return aa_board[2][5];
                           case 'f': return aa_board[2][6];
                           case 'g': return aa_board[2][7];
                           case 'h': return aa_board[2][8];
                           default: return null;
                       }
                       break;
                   case 8:
                       switch (column)
                       {
                           case 'a': return aa_board[1][1];
                           case 'b': return aa_board[1][2];
                           case 'c': return aa_board[1][3];
                           case 'd': return aa_board[1][4];
                           case 'e': return aa_board[1][5];
                           case 'f': return aa_board[1][6];
                           case 'g': return aa_board[1][7];
                           case 'h': return aa_board[1][8];
                           default: return null;
                       }
                       break;
                   default: return null;
               }
           }
           */
    }
}
