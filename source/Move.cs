using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chessboard
{
    internal class Move
    {
        private Point coordinates_ = new Point(-1, -1);
        private moveTypes moveType_ = moveTypes.Normal;
        private Point secondPiececoordinates_ = new Point(-1, -1);
        
        public enum moveTypes
        {
            Normal,
            FastPawn,
            PawnPromotion,
            shortCastling,
            longCastling,
            EnPassant,
            Check,
            Chekmate
        }

        public Move(Point coordinates)
        {
            coordinates_ = coordinates;
            moveType_ = moveTypes.Normal;
        }

        public Move(Point coordinates, moveTypes moveType)
        {
            coordinates_ = coordinates;
            moveType_ = moveType;
        }

        public Move(Point coordinates, moveTypes moveType, Point secondPiececoordinates)
        {
            coordinates_ = coordinates;
            moveType_ = moveType;
            secondPiececoordinates_ = secondPiececoordinates;
        }

        public Point coordinates
        {
            get => coordinates_;
            set => coordinates_ = value;
        }

        public moveTypes moveType
        {
            get => moveType_;
            set => moveType_ = value;
        }

        public Point secondPiececoordinates
        {
            get => secondPiececoordinates_;
            set => secondPiececoordinates_ = value;
        }
    }
}
