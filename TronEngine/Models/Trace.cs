using System;
using System.Collections.Generic;
using TronEngine.Enums;

namespace TronEngine.Models
{
    public class Trace
    {
        #region CONSTANTES

        public const int BIKE_WIDTH_SIDE = 1;

        #endregion CONSTANTES

        #region PROPERTIES

        public Direction Direction { get; set; }

        public int BottomRightX { get; protected set; }

        public int BottomRightY { get; protected set; }

        public int TopLeftX { get; protected set; }

        public int TopLeftY { get; protected set; }

        public int Width
        {
            get
            {
                return BottomRightX - TopLeftX;
            }
        }

        public int Height
        {
            get
            {
                return BottomRightY - TopLeftY;
            }
        }

        #endregion PROPERTIES

        #region CONSTRUCTORS

        public Trace(List<int> Xs, List<int> Ys)
        {
            if (Xs.Count == 2 && Ys.Count == 2)
            {
                this.Initialize(Xs[0], Ys[0], Xs[1], Ys[1]);
            }
            else
            {
                throw new ArgumentException("Trace not constructible !");
            }
        }

        public Trace(int x1, int y1, int x2, int y2, Direction direction)
        {
            this.Initialize(x1, y1, x2, y2);
            this.Direction = direction;
        }

        #endregion CONSTRUCTORS

        #region PUBLIC METHODS

        public void ContinueTo(int extension)
        {
            switch (this.Direction)
            {
                case Direction.RIGHT:
                    this.BottomRightX += extension;
                    break;
                case Direction.LEFT:
                    this.TopLeftX -= extension;
                    break;
                case Direction.UP:
                    this.TopLeftY -= extension;
                    break;
                case Direction.DOWN:
                    this.BottomRightY += extension;
                    break;
                default:
                    break;
            }
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        private void Initialize(int x1, int y1, int x2, int y2)
        {
            if (x1 < x2)
            {
                TopLeftX = x1 - BIKE_WIDTH_SIDE;
                BottomRightX = x2 + BIKE_WIDTH_SIDE;
            }
            else
            {
                TopLeftX = x2 - BIKE_WIDTH_SIDE;
                BottomRightX = x1 + BIKE_WIDTH_SIDE;
            }

            if (y1 < y2)
            {
                TopLeftY = y1 - BIKE_WIDTH_SIDE;
                BottomRightY = y2 + BIKE_WIDTH_SIDE;
            }
            else
            {
                TopLeftY = y2 - BIKE_WIDTH_SIDE;
                BottomRightY = y1 + BIKE_WIDTH_SIDE;
            }
        }

        #endregion PRIVATE METHODS
    }
}
