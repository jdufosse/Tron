"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var SnakeData = /** @class */ (function () {
    function SnakeData(widthSpriteNumber, heightSpriteNumber) {
        this._widthSpriteNumber = 12;
        this._heightSpriteNumber = 8;
        this._isInitialized = false;
        this._score = 0;
        this._highestScore = 0;
        this._widthSpriteNumber = widthSpriteNumber;
        this._heightSpriteNumber = heightSpriteNumber;
        this.initialize();
    }
    SnakeData.prototype.initialize = function () {
        if (this._widthSpriteNumber > 1 && this._heightSpriteNumber > 1) {
            var intialY = (this._heightSpriteNumber % 2 == 0) ? this._heightSpriteNumber / 2 : this._heightSpriteNumber / 2 + 1;
            this._snakeParts = [];
            // Initialize snake with head and tail parts
            this._snakeParts.push(new SnakePart(1, intialY, SnakeStatus.Head | SnakeStatus.Right));
            this._snakeParts.push(new SnakePart(0, intialY, SnakeStatus.Tail | SnakeStatus.Right));
            this.nextApple();
            this._isInitialized = true;
        }
        this._score = 0;
    };
    SnakeData.prototype.nextApple = function () {
        var emptyAreas;
        emptyAreas = [];
        for (var i = 0; i < this._widthSpriteNumber; i++) {
            for (var j = 0; j < this._heightSpriteNumber; j++) {
                var snakePart = this.getSnakePart(i, j);
                if (!snakePart) {
                    emptyAreas.push(new SnakePart(i, j, SnakeStatus.None));
                }
            }
        }
        if (emptyAreas.length > 0) {
            var index = Math.floor(Math.random() * emptyAreas.length);
            this._apple = emptyAreas[index];
            this._apple.Status = SnakeStatus.Apple;
            return true;
        }
        return false;
    };
    SnakeData.prototype.moveSnake = function (action) {
        if (this._snakeParts.length > 1) {
            // Get direction of snake
            var snakeDirection = (action != ACTION.NONE) ? SnakeData.getDirectionFromAction(action) : SnakeData.getDirectionFromStatus(this._snakeParts[0].Status);
            // Check if move is possible
            var checkResult = this.checkRedirection(snakeDirection);
            if (!checkResult || checkResult.isOutOfMap || checkResult.isCrossingOver) {
                return false;
            }
            // Create a new snakePart to had after the head
            if (checkResult.isEating || this._snakeParts.length >= 3) {
                var snakePart = SnakePart.assign(this._snakeParts[0]);
                snakePart.Status = SnakeStatus.Body | snakeDirection;
                // Add status in function of relative position of the next body part.
                snakePart.Status |= ((snakePart.X > this._snakeParts[1].X) ? SnakeStatus.Left : ((snakePart.X < this._snakeParts[1].X) ? SnakeStatus.Right : SnakeStatus.None));
                snakePart.Status |= ((snakePart.Y > this._snakeParts[1].Y) ? SnakeStatus.Up : ((snakePart.Y < this._snakeParts[1].Y) ? SnakeStatus.Down : SnakeStatus.None));
                this._snakeParts.splice(1, 0, snakePart);
            }
            // If the snake is eating, the new part is place after the head and the rest of snake parts don't move
            if (!checkResult.isEating) {
                // Manage Tail
                this._snakeParts[this._snakeParts.length - 1].X = this._snakeParts[this._snakeParts.length - 2].X;
                this._snakeParts[this._snakeParts.length - 1].Y = this._snakeParts[this._snakeParts.length - 2].Y;
                if (this._snakeParts.length >= 3) {
                    // remove the last part before the tail
                    this._snakeParts.splice(this._snakeParts.length - 2, 1);
                }
            }
            // Manage Head
            this._snakeParts[0].Status = SnakeStatus.Head;
            if (snakeDirection & SnakeStatus.Right) {
                this._snakeParts[0].X += 1;
                this._snakeParts[0].Status |= SnakeStatus.Right;
            }
            else if (snakeDirection & SnakeStatus.Left) {
                this._snakeParts[0].X -= 1;
                this._snakeParts[0].Status |= SnakeStatus.Left;
            }
            else if (snakeDirection & SnakeStatus.Up) {
                this._snakeParts[0].Y -= 1;
                this._snakeParts[0].Status |= SnakeStatus.Up;
            }
            else {
                this._snakeParts[0].Y += 1;
                this._snakeParts[0].Status |= SnakeStatus.Down;
            }
            // Manage Tail orientation
            if (this._snakeParts.length >= 2) {
                this._snakeParts[this._snakeParts.length - 1].Status = SnakeStatus.Tail;
                this._snakeParts[this._snakeParts.length - 1].Status |= ((this._snakeParts[this._snakeParts.length - 2].Y > this._snakeParts[this._snakeParts.length - 1].Y) ? SnakeStatus.Down : ((this._snakeParts[this._snakeParts.length - 2].Y < this._snakeParts[this._snakeParts.length - 1].Y) ? SnakeStatus.Up : SnakeStatus.None));
                this._snakeParts[this._snakeParts.length - 1].Status |= ((this._snakeParts[this._snakeParts.length - 2].X > this._snakeParts[this._snakeParts.length - 1].X) ? SnakeStatus.Right : ((this._snakeParts[this._snakeParts.length - 2].X < this._snakeParts[this._snakeParts.length - 1].X) ? SnakeStatus.Left : SnakeStatus.None));
            }
            if (checkResult.isEating) {
                this._score++;
                if (this._score > this._highestScore) {
                    this._highestScore = this._score;
                }
                return this.nextApple();
            }
            return true;
        }
    };
    SnakeData.prototype.checkRedirection = function (snakeDirection) {
        if (this._snakeParts.length > 0) {
            var result = {
                isOutOfMap: false,
                isEating: false,
                isCrossingOver: false
            };
            var snakePart = SnakePart.assign(this._snakeParts[0]);
            if (snakeDirection & SnakeStatus.Right) {
                snakePart.X += 1;
            }
            else if (snakeDirection & SnakeStatus.Left) {
                snakePart.X -= 1;
            }
            else if (snakeDirection & SnakeStatus.Up) {
                snakePart.Y -= 1;
            }
            else {
                snakePart.Y += 1;
            }
            if (snakePart.X < 0 || snakePart.X >= this._widthSpriteNumber
                || snakePart.Y < 0 || snakePart.Y >= this._heightSpriteNumber) {
                result.isOutOfMap = true;
            }
            else if (snakePart.X == this._apple.X && snakePart.Y == this._apple.Y) {
                result.isEating = true;
            }
            else {
                // Don't consider the tail because we can take place of the tail by moving
                for (var i = 0; i < this._snakeParts.length - 1 && result.isCrossingOver == false; i++) {
                    if (snakePart.X == this._snakeParts[i].X && snakePart.Y == this._snakeParts[i].Y) {
                        result.isCrossingOver = true;
                    }
                }
            }
            return result;
        }
        return false;
    };
    SnakeData.getDirectionFromAction = function (action) {
        if (action == ACTION.RIGHT) {
            return SnakeStatus.Right;
        }
        else if (action == ACTION.LEFT) {
            return SnakeStatus.Left;
        }
        else if (action == ACTION.UP) {
            return SnakeStatus.Up;
        }
        else {
            return SnakeStatus.Down;
        }
    };
    SnakeData.getDirectionFromStatus = function (status) {
        if (status & SnakeStatus.Right) {
            return SnakeStatus.Right;
        }
        else if (status & SnakeStatus.Left) {
            return SnakeStatus.Left;
        }
        else if (status & SnakeStatus.Up) {
            return SnakeStatus.Up;
        }
        else {
            return SnakeStatus.Down;
        }
    };
    SnakeData.prototype.getSnakePart = function (x, y) {
        for (var i = 0; i < this._snakeParts.length; i++) {
            if (this._snakeParts[i].X == x && this._snakeParts[i].Y == y) {
                return this._snakeParts[0];
            }
        }
    };
    SnakeData.prototype.isInitialized = function () {
        return this._isInitialized;
    };
    SnakeData.prototype.getApple = function () {
        return this._apple;
    };
    SnakeData.prototype.getHighestScore = function () {
        return this._highestScore;
    };
    SnakeData.prototype.getScore = function () {
        return this._score;
    };
    SnakeData.prototype.getSnake = function () {
        return this._snakeParts;
    };
    SnakeData.prototype.getWidthSpriteNumber = function () {
        this._widthSpriteNumber;
    };
    SnakeData.prototype.getHeightSpriteNumber = function () {
        this._heightSpriteNumber;
    };
    return SnakeData;
}());
exports.SnakeData = SnakeData;
var SnakePart = /** @class */ (function () {
    function SnakePart(x, y, status) {
        this.X = 0;
        this.Y = 0;
        this.Status = SnakeStatus.None;
        this.X = x;
        this.Y = y;
        this.Status = status;
    }
    SnakePart.assign = function (snakePart) {
        return new SnakePart(snakePart.X, snakePart.Y, snakePart.Status);
    };
    return SnakePart;
}());
exports.SnakePart = SnakePart;
var SnakeStatus;
(function (SnakeStatus) {
    SnakeStatus[SnakeStatus["None"] = 0] = "None";
    SnakeStatus[SnakeStatus["Up"] = 1] = "Up";
    SnakeStatus[SnakeStatus["Down"] = 2] = "Down";
    SnakeStatus[SnakeStatus["Left"] = 4] = "Left";
    SnakeStatus[SnakeStatus["Right"] = 8] = "Right";
    SnakeStatus[SnakeStatus["Body"] = 16] = "Body";
    SnakeStatus[SnakeStatus["Head"] = 32] = "Head";
    SnakeStatus[SnakeStatus["Tail"] = 64] = "Tail";
    SnakeStatus[SnakeStatus["Apple"] = 128] = "Apple";
})(SnakeStatus = exports.SnakeStatus || (exports.SnakeStatus = {}));
var ACTION;
(function (ACTION) {
    ACTION[ACTION["NONE"] = 0] = "NONE";
    ACTION[ACTION["RIGHT"] = 1] = "RIGHT";
    ACTION[ACTION["LEFT"] = 2] = "LEFT";
    ACTION[ACTION["UP"] = 3] = "UP";
    ACTION[ACTION["DOWN"] = 4] = "DOWN";
})(ACTION = exports.ACTION || (exports.ACTION = {}));
//# sourceMappingURL=snake.data.js.map