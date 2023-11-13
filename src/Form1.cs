#pragma warning disable IDE0090
#pragma warning disable IDE1006

using System;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace chessboard
{
    public partial class Form1 : Form
    {

        private BoardInterface board;
        bool checkedWhiteKingImagePainted;
        bool checkedBlackKingImagePainted;
        private Point selectedPiece_ = new Point(-1, -1);
        private Point startMoveTrace_ = new Point(-1, -1);
        private Point endMoveTrace_ = new Point(-1, -1);
        private bool flippedBoard_ = false;

        public Form1()
        {
            InitializeComponent();
            board = new BoardInterface(true);
            checkedWhiteKingImagePainted = false;
            checkedBlackKingImagePainted = false;
            flippedBoard = false;
        }

        private bool isPieceSelected()
        {
            if (selectedPiece_.X >= 0 &&
                selectedPiece_.Y >= 0) { return true; }
            return false;
        }

        private void selectPiece(int row, int column)
        {
            if (row < 0 || column < 0 ||
                row > 7 || column > 7)
            {
                unselectPiece();
                return;
            }

            selectedPiece_.X = row;
            selectedPiece_.Y = column;

            board.setValidMoves(row, column);
        }

        private void unselectPiece()
        {
            selectedPiece_.X = -1;
            selectedPiece_.Y = -1;

            board.unsetValidMoves();
        }

        public Point getSelectedPiece()
        {
            return selectedPiece_;
        }

        // Getter and setter.
        public Point startMoveTrace
        {
            get => startMoveTrace_;
            set => startMoveTrace_ = value;
        }

        // Getter and setter.
        public Point endMoveTrace
        {
            get => endMoveTrace_;
            set => endMoveTrace_ = value;
        }

        // Getter and setter.
        public bool flippedBoard
        {
            get => flippedBoard_;
            set => flippedBoard_ = value;
        }

        private void setPictureBoxPieceImage(int row, int column)
        {
            // Avoid program exceptions in the call to
            // GetControlFromPosition(), by checking the input data.
            if (row < 0 || column < 0 ||
                row >= tableLayoutPanel1.RowCount ||
                column >= tableLayoutPanel1.ColumnCount)
            {
                return;
            }

            // The ! operator is the null forgiving operator. We use it to
            // suppress all nullable warnings for the preceding expression.
            // The null forgiving operator has no effect at run time. It
            // only affects the compiler's static flow analysis.
            PictureBox pb =
                (PictureBox)tableLayoutPanel1.GetControlFromPosition(column, row)!;

            char pieceColor = board.getPieceColor(row, column);
            char squareColor = board.getSquareColor(row, column);
            char pieceName = board.getPieceName(row, column);

            // Square color is white.
            if (squareColor == 'w')
            {
                switch (pieceName)
                {
                    case '\0':
                        pb.BackgroundImage = Properties.Resources.white;
                        break;
                    case 'T':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.T_white_on_white;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.T_black_on_white;
                        }
                        break;
                    case 'C':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.C_white_on_white;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.C_black_on_white;
                        }
                        break;
                    case 'A':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.A_white_on_white;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.A_black_on_white;
                        }
                        break;
                    case 'D':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.D_white_on_white;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.D_black_on_white;
                        }
                        break;
                    case 'R':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.R_white_on_white;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.R_black_on_white;
                        }
                        break;
                    case 'P':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.P_white_on_white;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.P_black_on_white;
                        }
                        break;
                }
            }
            // Square color is black.
            else
            {
                switch (pieceName)
                {
                    case '\0':
                        pb.BackgroundImage = Properties.Resources.black;
                        break;
                    case 'T':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.T_white_on_black;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.T_black_on_black;
                        }
                        break;
                    case 'C':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.C_white_on_black;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.C_black_on_black;
                        }
                        break;
                    case 'A':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.A_white_on_black;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.A_black_on_black;
                        }
                        break;
                    case 'D':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.D_white_on_black;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.D_black_on_black;
                        }
                        break;
                    case 'R':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.R_white_on_black;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.R_black_on_black;
                        }
                        break;
                    case 'P':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.P_white_on_black;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.P_black_on_black;
                        }
                        break;
                }
            }
        }

        private void setPictureBoxEmptySquareImage(int row, int column)
        {
            // Avoid program exceptions in the call to
            // GetControlFromPosition(), by checking the input data.
            if (row < 0 || column < 0 ||
                row >= tableLayoutPanel1.RowCount ||
                column >= tableLayoutPanel1.ColumnCount)
            {
                return;
            }

            // The ! operator is the null forgiving operator. We use it to
            // suppress all nullable warnings for the preceding expression.
            // The null forgiving operator has no effect at run time. It
            // only affects the compiler's static flow analysis.
            PictureBox pb =
                (PictureBox)tableLayoutPanel1.GetControlFromPosition(column, row)!;

            char squareColor = board.getSquareColor(row, column);

            // Square color is white.
            if (squareColor == 'w')
            {
                pb.BackgroundImage = Properties.Resources.white;
            }
            // Square color is black.
            else if (squareColor == 'b')
            {
                pb.BackgroundImage = Properties.Resources.black;
            }
            else { return; }
        }

        private void setPictureBoxSelectedPieceImage(int row, int column)
        {
            // Avoid program exceptions in the call to
            // GetControlFromPosition(), by checking the input data.
            if (row < 0 || column < 0 ||
                row >= tableLayoutPanel1.RowCount ||
                column >= tableLayoutPanel1.ColumnCount)
            {
                return;
            }

            // The ! operator is the null forgiving operator. We use it to
            // suppress all nullable warnings for the preceding expression.
            // The null forgiving operator has no effect at run time. It
            // only affects the compiler's static flow analysis.
            PictureBox pb =
                (PictureBox)tableLayoutPanel1.GetControlFromPosition(column, row)!;

            char pieceColor = board.getPieceColor(row, column);
            char squareColor = board.getSquareColor(row, column);
            char pieceName = board.getPieceName(row, column);

            // Square color is white.
            if (squareColor == 'w')
            {
                switch (pieceName)
                {
                    case 'T':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.T_white_on_white_clicked;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.T_black_on_white_clicked;
                        }
                        break;
                    case 'C':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.C_white_on_white_clicked;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.C_black_on_white_clicked;
                        }
                        break;
                    case 'A':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.A_white_on_white_clicked;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.A_black_on_white_clicked;
                        }
                        break;
                    case 'D':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.D_white_on_white_clicked;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.D_black_on_white_clicked;
                        }
                        break;
                    case 'R':
                        if (pieceColor == 'w')
                        {
                            if (!checkedWhiteKingImagePainted)
                            {
                                pb.BackgroundImage = Properties.Resources.R_white_on_white_clicked;
                            }
                            else
                            {
                                pb.BackgroundImage = Properties.Resources.R_white_on_white_checked_clicked;
                            }
                        }
                        else
                        {
                            if (!checkedBlackKingImagePainted)
                            {
                                pb.BackgroundImage = Properties.Resources.R_black_on_white_clicked;
                            }
                            else
                            {
                                pb.BackgroundImage = Properties.Resources.R_black_on_white_checked_clicked;
                            }
                        }
                        break;
                    case 'P':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.P_white_on_white_clicked;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.P_black_on_white_clicked;
                        }
                        break;
                }
            }
            // Square color is black.
            else
            {
                switch (pieceName)
                {
                    case 'T':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.T_white_on_black_clicked;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.T_black_on_black_clicked;
                        }
                        break;
                    case 'C':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.C_white_on_black_clicked;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.C_black_on_black_clicked;
                        }
                        break;
                    case 'A':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.A_white_on_black_clicked;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.A_black_on_black_clicked;
                        }
                        break;
                    case 'D':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.D_white_on_black_clicked;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.D_black_on_black_clicked;
                        }
                        break;
                    case 'R':
                        if (pieceColor == 'w')
                        {
                            if (!checkedWhiteKingImagePainted)
                            {
                                pb.BackgroundImage = Properties.Resources.R_white_on_black_clicked;
                            }
                            else
                            {
                                pb.BackgroundImage = Properties.Resources.R_white_on_black_checked_clicked;
                            }
                        }
                        else
                        {
                            if (!checkedBlackKingImagePainted)
                            {
                                pb.BackgroundImage = Properties.Resources.R_black_on_black_clicked;
                            }
                            else
                            {
                                pb.BackgroundImage = Properties.Resources.R_black_on_black_checked_clicked;
                            }
                        }
                        break;
                    case 'P':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.P_white_on_black_clicked;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.P_black_on_black_clicked;
                        }
                        break;
                }
            }
        }

        private void setPictureBoxEmptySquareTrackImage(int row, int column)
        {
            // Avoid program exceptions in the call to
            // GetControlFromPosition(), by checking the input data.
            if (row < 0 || column < 0 ||
                row >= tableLayoutPanel1.RowCount ||
                column >= tableLayoutPanel1.ColumnCount)
            {
                return;
            }

            // The ! operator is the null forgiving operator. We use it to
            // suppress all nullable warnings for the preceding expression.
            // The null forgiving operator has no effect at run time. It
            // only affects the compiler's static flow analysis.
            PictureBox pb =
                (PictureBox)tableLayoutPanel1.GetControlFromPosition(column, row)!;

            char squareColor = board.getSquareColor(row, column);

            // Square color is white.
            if (squareColor == 'w')
            {
                pb.BackgroundImage = Properties.Resources.white_moved;
            }
            // Square color is black.
            else if (squareColor == 'b')
            {
                pb.BackgroundImage = Properties.Resources.black_moved;
            }
            else { return; }
        }

        private void setPictureBoxPieceTrackImage(int row, int column)
        {
            // Avoid program exceptions in the call to
            // GetControlFromPosition(), by checking the input data.
            if (row < 0 || column < 0 ||
                row >= tableLayoutPanel1.RowCount ||
                column >= tableLayoutPanel1.ColumnCount)
            {
                return;
            }

            // The ! operator is the null forgiving operator. We use it to
            // suppress all nullable warnings for the preceding expression.
            // The null forgiving operator has no effect at run time. It
            // only affects the compiler's static flow analysis.
            PictureBox pb =
                (PictureBox)tableLayoutPanel1.GetControlFromPosition(column, row)!;

            char pieceColor = board.getPieceColor(row, column);
            char squareColor = board.getSquareColor(row, column);
            char pieceName = board.getPieceName(row, column);

            // Square color is white.
            if (squareColor == 'w')
            {
                switch (pieceName)
                {
                    case 'T':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.T_white_on_white_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.T_black_on_white_moved;
                        }
                        break;
                    case 'C':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.C_white_on_white_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.C_black_on_white_moved;
                        }
                        break;
                    case 'A':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.A_white_on_white_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.A_black_on_white_moved;
                        }
                        break;
                    case 'D':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.D_white_on_white_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.D_black_on_white_moved;
                        }
                        break;
                    case 'R':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.R_white_on_white_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.R_black_on_white_moved;
                        }
                        break;
                    case 'P':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.P_white_on_white_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.P_black_on_white_moved;
                        }
                        break;
                }
            }
            // Square color is black.
            else
            {
                switch (pieceName)
                {
                    case 'T':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.T_white_on_black_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.T_black_on_black_moved;
                        }
                        break;
                    case 'C':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.C_white_on_black_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.C_black_on_black_moved;
                        }
                        break;
                    case 'A':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.A_white_on_black_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.A_black_on_black_moved;
                        }
                        break;
                    case 'D':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.D_white_on_black_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.D_black_on_black_moved;
                        }
                        break;
                    case 'R':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.R_white_on_black_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.R_black_on_black_moved;
                        }
                        break;
                    case 'P':
                        if (pieceColor == 'w')
                        {
                            pb.BackgroundImage = Properties.Resources.P_white_on_black_moved;
                        }
                        else
                        {
                            pb.BackgroundImage = Properties.Resources.P_black_on_black_moved;
                        }
                        break;
                }
            }
        }

        private void setPictureBoxUntrackImage(int row, int column)
        {
            string square = board.getSquare(row, column);
            switch (square.Length)
            {
                // Value examples: "b" (black square).
                case 1:
                    setPictureBoxEmptySquareImage(row, column);
                    break;
                // Value example: "wPb" (white pawn on a black square).
                case 3:
                // Value example: "wPfb" (white fast pawn on a black square).
                case 4:
                    setPictureBoxPieceImage(row, column);
                    break;
                default:
                    return;
            }
        }

        private void setPictureBoxCheckedKingImage(int row, int column)
        {
            // Avoid program exceptions in the call to
            // GetControlFromPosition(), by checking the input data.
            if (row < 0 || column < 0 ||
                row >= tableLayoutPanel1.RowCount ||
                column >= tableLayoutPanel1.ColumnCount)
            {
                return;
            }

            char pieceName = board.getPieceName(row, column);
            char pieceColor = board.getPieceColor(row, column);
            char squareColor = board.getSquareColor(row, column);

            //
            if (pieceName != BoardInterface.C_KING_NAME)
            {
                return;
            }

            // The ! operator is the null forgiving operator. We use it to
            // suppress all nullable warnings for the preceding expression.
            // The null forgiving operator has no effect at run time. It
            // only affects the compiler's static flow analysis.
            PictureBox pb =
                (PictureBox)tableLayoutPanel1.GetControlFromPosition(column, row)!;

            switch (pieceColor)
            {
                // Value examples: "b" (black square).
                case BoardInterface.C_WHITE_COLOR:
                    if (squareColor == BoardInterface.C_WHITE_COLOR)
                    {
                        pb.BackgroundImage = Properties.Resources.R_white_on_white_checked;
                    }
                    else
                    {
                        pb.BackgroundImage = Properties.Resources.R_white_on_black_checked;
                    }
                    break;
                // Value examples: "wPb" (white pawn on a black square).
                case BoardInterface.C_BLACK_COLOR:
                    if (squareColor == BoardInterface.C_WHITE_COLOR)
                    {
                        pb.BackgroundImage = Properties.Resources.R_black_on_white_checked;
                    }
                    else
                    {
                        pb.BackgroundImage = Properties.Resources.R_black_on_black_checked;
                    }
                    break;
                default:
                    return;
            }
        }

        public void setStartMoveTraceCoordinates(int row, int column)
        {
            if (row < 0 || column < 0 ||
                row > 7 || column > 7)
            {
                startMoveTrace_.X = -1;
                startMoveTrace_.Y = -1;
            }

            startMoveTrace_.X = row;
            startMoveTrace_.Y = column;
        }

        public void setEndMoveTraceCoordinates(int row, int column)
        {
            if (row < 0 || column < 0 ||
               row > 7 || column > 7)
            {
                startMoveTrace_.X = -1;
                startMoveTrace_.Y = -1;
            }

            endMoveTrace_.X = row;
            endMoveTrace_.Y = column;
        }

        private void paintCheckedKing(TableLayoutPanelCellPosition coordinates)
        {
            // It is possible that in the previous turn, one king was in check,
            // and in the current turn, another king is in check. This code
            // ensures that two kings are not painted at the same time as
            // being in check, by painting an unchecked king.
            if (checkedWhiteKingImagePainted)
            {
                if (coordinates.Row != board.whiteKingCoordinates.X ||
                    coordinates.Column != board.whiteKingCoordinates.Y)
                {
                    setPictureBoxPieceImage(board.whiteKingCoordinates.X, board.whiteKingCoordinates.Y);
                }

                checkedWhiteKingImagePainted = false;
            }
            // Paint the unchecked black king with its usual color.
            else if (checkedBlackKingImagePainted)
            {
                if (coordinates.Row != board.blackKingCoordinates.X ||
                    coordinates.Column != board.blackKingCoordinates.Y)
                {
                    setPictureBoxPieceImage(board.blackKingCoordinates.X, board.blackKingCoordinates.Y);
                }

                checkedBlackKingImagePainted = false;
            }

            // Paint the checked white king.
            if (board.isWhitePiecesTurn)
            {
                // The selected piece is the checked king.
                if (coordinates.Row == board.whiteKingCoordinates.X &&
                    coordinates.Column == board.whiteKingCoordinates.Y)
                {
                    // Paint a pressed checked king.
                    setPictureBoxSelectedPieceImage(coordinates.Row, coordinates.Column);
                }
                // The selected piece is not the checked king.
                else
                {
                    // Paint a not pressed checked king.
                    setPictureBoxCheckedKingImage(
                        board.whiteKingCoordinates.X,
                        board.whiteKingCoordinates.Y);
                }
                checkedWhiteKingImagePainted = true;
            }
            // Paint the checked black king.
            else
            {
                // The selected piece is the checked king.
                if (coordinates.Row == board.blackKingCoordinates.X &&
                    coordinates.Column == board.blackKingCoordinates.Y)
                {
                    // Paint a pressed checked king.
                    setPictureBoxSelectedPieceImage(coordinates.Row, coordinates.Column);
                }
                // The selected piece is not the checked king.
                else
                {
                    // Paint a not pressed checked king.
                    setPictureBoxCheckedKingImage(
                        board.blackKingCoordinates.X,
                        board.blackKingCoordinates.Y);
                }
                checkedBlackKingImagePainted = true;
            }
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            TableLayoutPanelCellPosition coordinates = tableLayoutPanel1.GetCellPosition(pb);

            // Game is over.
            if (board.isKingCheckmated || 
                board.isKingStalemated || 
                board.tripleRepetitionOfMoves ||
                board.fiftyMovesRule ||
                board.notEnoughPieces)
            {
                return;
            }

            // None of the user's pieces were previously selected.
            if (!isPieceSelected())
            {
                // User has clicked on one of their pieces.
                if (board.isaValidPieceForMove(coordinates.Row, coordinates.Column))
                {
                    selectPiece(coordinates.Row, coordinates.Column);

                    setPictureBoxSelectedPieceImage(coordinates.Row, coordinates.Column);
                }
            }
            // One of the user's pieces were previously selected.
            else
            {
                // User has pressed on a piece of their color.
                if ((board.isWhitePiecesTurn &&
                    board.isaOwnPiece(coordinates.Row, coordinates.Column, BoardInterface.C_WHITE_COLOR)) ||
                    (!board.isWhitePiecesTurn &&
                    board.isaOwnPiece(coordinates.Row, coordinates.Column, BoardInterface.C_BLACK_COLOR))
                    )
                {
                    setPictureBoxPieceImage(getSelectedPiece().X, getSelectedPiece().Y);
                    setPictureBoxSelectedPieceImage(coordinates.Row, coordinates.Column);

                    // If king is checked, also paint it checked.
                    if (board.isKingChecked)
                    {
                        paintCheckedKing(coordinates);
                    }

                    // Update the selected piece's coordinates.
                    selectPiece(coordinates.Row, coordinates.Column);
                }
                // User has clicked on a empty square or on an opponent's piece.
                else if (board.isaValidMove(coordinates.Row, coordinates.Column))
                {
                    Move move = board.movePiece(
                        getSelectedPiece().X,
                        getSelectedPiece().Y,
                        coordinates.Row, coordinates.Column);

                    if (move == null)
                    {
                        return;
                    }

                    // Repaint by unmarking the squares marked in the previous turn.
                    setPictureBoxUntrackImage(startMoveTrace.X, startMoveTrace.Y);
                    setPictureBoxUntrackImage(endMoveTrace.X, endMoveTrace.Y);

                    // Record the coordinates of the squares implied in the current move.
                    setStartMoveTraceCoordinates(getSelectedPiece().X, getSelectedPiece().Y);
                    setEndMoveTraceCoordinates(coordinates.Row, coordinates.Column);

                    // A pawn has captured en passant.
                    if (move.moveType == chessboard.Move.moveTypes.EnPassant)
                    {
                        // Remove the previous position of the pawn.
                        setPictureBoxEmptySquareTrackImage(getSelectedPiece().X, getSelectedPiece().Y);
                        // Paint the new position of the pawn.
                        setPictureBoxPieceTrackImage(coordinates.Row, coordinates.Column);
                        // Remove the pawn captured en passant.
                        setPictureBoxEmptySquareImage(move.secondPiececoordinates.X, move.secondPiececoordinates.Y);
                    }
                    // A king has castled.
                    else if (move.moveType == chessboard.Move.moveTypes.shortCastling)
                    {
                        // Remove the king of its previous position.
                        setPictureBoxEmptySquareTrackImage(getSelectedPiece().X, getSelectedPiece().Y);
                        // Paint the king in its new position.
                        setPictureBoxPieceTrackImage(coordinates.Row, coordinates.Column);

                        if (board.isWhitePiecesTurn)
                        {
                            if (board.isWhitePiecesAtBottom)
                            {
                                // Remove the tower of its previous position.
                                setPictureBoxEmptySquareImage(7, 7);
                                // Paint the tower in its new position.
                                setPictureBoxPieceImage(7, 5);
                            }
                            else
                            {
                                // Remove the tower of its previous position.
                                setPictureBoxEmptySquareImage(0, 0);
                                // Paint the tower in its new position.
                                setPictureBoxPieceImage(0, 2);
                            }
                        }
                        else
                        {
                            if (board.isWhitePiecesAtBottom)
                            {
                                // Remove the tower of its previous position.
                                setPictureBoxEmptySquareImage(0, 7);
                                // Paint the tower in its new position.
                                setPictureBoxPieceImage(0, 5);
                            }
                            else
                            {
                                // Remove the tower of its previous position.
                                setPictureBoxEmptySquareImage(7, 0);
                                // Paint the tower in its new position.
                                setPictureBoxPieceImage(7, 2);
                            }

                        }
                        // Paint the new position of the tower.
                        // setPictureBoxPieceImage(Move, Move);
                    }
                    else if (move.moveType == chessboard.Move.moveTypes.longCastling)
                    {
                        // Remove the king of its previous position.
                        setPictureBoxEmptySquareTrackImage(getSelectedPiece().X, getSelectedPiece().Y);
                        // Paint the king in its new position.
                        setPictureBoxPieceTrackImage(coordinates.Row, coordinates.Column);

                        if (board.isWhitePiecesTurn)
                        {
                            if (board.isWhitePiecesAtBottom)
                            {
                                // Remove the tower of its previous position.
                                setPictureBoxEmptySquareImage(7, 0);
                                // Paint the tower in its new position.
                                setPictureBoxPieceImage(7, 3);
                            }
                            else
                            {
                                // Remove the tower of its previous position.
                                setPictureBoxEmptySquareImage(0, 7);
                                // Paint the tower in its new position.
                                setPictureBoxPieceImage(0, 4);
                            }
                        }
                        else
                        {
                            if (board.isWhitePiecesAtBottom)
                            {
                                // Remove the tower of its previous position.
                                setPictureBoxEmptySquareImage(0, 0);
                                // Paint the tower in its new position.
                                setPictureBoxPieceImage(0, 3);
                            }
                            else
                            {
                                // Remove the tower of its previous position.
                                setPictureBoxEmptySquareImage(7, 7);
                                // Paint the tower in its new position.
                                setPictureBoxPieceImage(7, 4);
                            }

                        }
                    }
                    else
                    {
                        setPictureBoxEmptySquareTrackImage(getSelectedPiece().X, getSelectedPiece().Y);
                        setPictureBoxPieceTrackImage(coordinates.Row, coordinates.Column);
                    }

                    unselectPiece();

                    //
                    board.isWhitePiecesTurn = !board.isWhitePiecesTurn;

                    // The game is declared a draw by threefold repetition.
                    if (board.tripleRepetitionOfMoves)
                    {
                        MessageBox.Show(this, "Draw by threefold repetition", "Draw", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // The game is declared a draw by threefold repetition.
                    if (board.fiftyMovesRule)
                    {
                        MessageBox.Show(this, "Draw by the Fifty-move rule", "Draw", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // The game is declared a draw by not enough pieces.
                    if (board.notEnoughPieces)
                    {
                        MessageBox.Show(this, "Draw by not enough pieces", "Draw", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // A king has been checked.
                    if (board.isKingChecked)
                    {
                        paintCheckedKing(coordinates);
                        if (board.isKingCheckmated)
                        {
                            if (board.isWhitePiecesTurn)
                            {
                                MessageBox.Show(this, "Black wins", "Checkmate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show(this, "White wins", "Checkmate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }

                    }
                    // Paint the unchecked white king with its usual color.
                    else
                    {
                        if (checkedWhiteKingImagePainted)
                        {
                            if (coordinates.Row != board.whiteKingCoordinates.X ||
                                coordinates.Column != board.whiteKingCoordinates.Y)
                            {
                                setPictureBoxPieceImage(board.whiteKingCoordinates.X, board.whiteKingCoordinates.Y);
                            }

                            checkedWhiteKingImagePainted = false;
                        }
                        // Paint the unchecked black king with its usual color.
                        else if (checkedBlackKingImagePainted)
                        {
                            if (coordinates.Row != board.blackKingCoordinates.X ||
                                coordinates.Column != board.blackKingCoordinates.Y)
                            {
                                setPictureBoxPieceImage(board.blackKingCoordinates.X, board.blackKingCoordinates.Y);
                            }

                            checkedBlackKingImagePainted = false;
                        }

                        if (board.isKingStalemated)
                        {
                            if (board.isWhitePiecesTurn)
                            {
                                MessageBox.Show(this, "White is stalemated", "Checkmate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show(this, "Black is stalemated", "Checkmate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }

                }
                // The user has selected a square that the
                // previously selected piece cannot move to.
                // Therefore, deselect the previously selected square.
                else
                {
                    // If king is checked, also paint it checked.
                    if (board.isKingChecked)
                    {
                        paintCheckedKing(coordinates);
                    }
                    else
                    {
                        setPictureBoxPieceImage(getSelectedPiece().X, getSelectedPiece().Y);
                    }

                    unselectPiece();
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox13_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox15_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox16_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox17_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox18_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox19_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox20_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox21_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox22_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox23_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox24_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox25_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox26_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox27_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox28_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox29_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox30_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox31_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox32_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox33_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox34_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox35_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox36_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox37_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox38_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox39_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox40_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox41_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox42_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox43_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox44_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox47_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox48_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox49_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox50_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox45_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox46_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox51_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox52_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox53_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox54_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox55_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox56_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox57_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox58_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox59_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox60_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox61_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox62_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox63_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        private void pictureBox64_Click(object sender, EventArgs e)
        {
            pictureBox_Click(sender, e);
        }

        // Flip board.
        private void invertirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // DialogResult ds = MessageBox.Show(this, "OK?", "Title", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            board.flipBoard();

            for (int i = 0; i < BoardInterface.boardLength(); i++)
            {
                for (int j = 0; j < BoardInterface.boardLength(); j++)
                {
                    //
                    setPictureBoxPieceImage(i, j);
                }
            }

            // Flip the coordinates of the squares implied in the last move.
            setStartMoveTraceCoordinates(
                tableLayoutPanel1.RowCount - 1 - startMoveTrace.X,
                tableLayoutPanel1.RowCount - 1 - startMoveTrace.Y);
            setEndMoveTraceCoordinates(
                tableLayoutPanel1.RowCount - 1 - endMoveTrace.X,
                tableLayoutPanel1.RowCount - 1 - endMoveTrace.Y);

            // Repaint the squares implied in the last move.
            setPictureBoxEmptySquareTrackImage(startMoveTrace.X, startMoveTrace.Y);
            setPictureBoxPieceTrackImage(endMoveTrace.X, endMoveTrace.Y);

            if (board.isKingChecked)
            {
                // Paint the checked king.
                if (board.isWhitePiecesTurn)
                {
                    setPictureBoxCheckedKingImage(
                        board.whiteKingCoordinates.X,
                        board.whiteKingCoordinates.Y);

                    // checkedWhiteKingImagePainted = true;
                }
                else if (!board.isWhitePiecesTurn)
                {
                    setPictureBoxCheckedKingImage(
                        board.blackKingCoordinates.X,
                        board.blackKingCoordinates.Y);

                    // checkedBlackKingImagePainted = true;
                }
            }

            flippedBoard = !flippedBoard;
        }

        // New game.
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!flippedBoard)
            {
                board = null!;
                board = new BoardInterface(true);
            }
            else
            {
                board = null!;
                board = new BoardInterface(false);
            }

            unselectPiece();
            setStartMoveTraceCoordinates(-1, -1);
            setEndMoveTraceCoordinates(-1, -1);
            checkedWhiteKingImagePainted = false;
            checkedBlackKingImagePainted = false;

            for (int i = 0; i < BoardInterface.boardLength(); i++)
            {
                for (int j = 0; j < BoardInterface.boardLength(); j++)
                {
                    //
                    setPictureBoxPieceImage(i, j);
                }
            }
        }
    }
}