import { Component, OnInit, HostListener } from '@angular/core';
import { SnakeData, SnakeStatus, ACTION } from './snake.data';
import { clearInterval } from 'timers';

const _spriteSize = 64;
const _timerInterval = 200;

const enum KEY_CODE {
  RIGHT_ARROW = 39,
  LEFT_ARROW = 37,
  UP_ARROW = 38,
  DOWN_ARROW = 40,
}

@Component({
  selector: 'app-snake',
  templateUrl: './snake.component.html',
  styleUrls: ['./snake.component.css']
})
export class SnakeComponent implements OnInit {
  public isInitialized = false;
  public isPaused = false;
  public isEndGame = false;
  public message = null;
  public highestScore = 0;
  public score = 0;
  public viewport = {
    width: 0,
    height: 0,
    border: 5
  };
  public snakeParts = [];
  public apple = {
    positionX: 0,
    positionY: 0
  };

  private _data: SnakeData;
  private _widthSpriteNumber;
  private _heightSpriteNumber;
  private _timerId;
  private _nextAction: ACTION;
  private _isStarted = false;

  ngOnInit() {
    this._nextAction = ACTION.NONE;
    this._widthSpriteNumber = Math.floor((window.innerWidth * 0.75) / _spriteSize) - 1;
    this._heightSpriteNumber = Math.floor((window.innerHeight * 0.95) / _spriteSize) - 1;

    this._data = new SnakeData(this._widthSpriteNumber, this._heightSpriteNumber);

    this.drawSnakeMap();
    this.drawSnake();

    this._timerId = setInterval(() => {
      this.snakeTick();
    }, _timerInterval);

    this.isInitialized = true;
  }

  @HostListener('window:keydown', ['$event'])
  keyEvent(event: KeyboardEvent) {
    if (event.keyCode === KEY_CODE.RIGHT_ARROW) {
      this._nextAction = ACTION.RIGHT;
    }
    else if (event.keyCode === KEY_CODE.LEFT_ARROW) {
      this._nextAction = ACTION.LEFT;
    }
    else if (event.keyCode === KEY_CODE.UP_ARROW) {
      this._nextAction = ACTION.UP;
    }
    else if (event.keyCode === KEY_CODE.DOWN_ARROW) {
      this._nextAction = ACTION.DOWN;
    }
  }

  drawSnakeMap() {
    this.viewport.width = _spriteSize * this._widthSpriteNumber;
    this.viewport.height = _spriteSize * this._heightSpriteNumber;
  }

  drawSnake() {
    // Get apple
    let apple = this._data.getApple();
    if (apple) {
      this.apple.positionX = apple.X * _spriteSize;
      this.apple.positionY = apple.Y * _spriteSize;
    }

    // Get snake
    let snakeParts = this._data.getSnake();
    let snakePartsToDisplay = [];
    if (snakeParts) {
      snakeParts.forEach(s => {
        
        let classParts = ['snake'];

        if (s.Status & SnakeStatus.Head) {
          classParts.push('head');
        }
        else if (s.Status & SnakeStatus.Tail) {
          classParts.push('tail');
        }

        if (s.Status & SnakeStatus.Down) {
          classParts.push('down');
        }
        if (s.Status & SnakeStatus.Up) {
          classParts.push('up');
        }
        if (s.Status & SnakeStatus.Left) {
          classParts.push('left');
        }
        if (s.Status & SnakeStatus.Right) {
          classParts.push('right');
        }

        let className = classParts.join('-');

        if (className) {
          className = 'snake-part ' + className;

          snakePartsToDisplay.push({
            class: className,
            positionX: s.X * _spriteSize,
            positionY: s.Y * _spriteSize,
          });
        }
      });

      this.snakeParts = snakePartsToDisplay;

      // Update scores
      this.score = this._data.getScore();
      this.highestScore = this._data.getHighestScore();
    }
  }

  restartGame() {
    this._data.initialize();
    this.drawSnake();

    // Reset some variables
    this.isEndGame = false;
    this._isStarted = false;
    this._nextAction = ACTION.NONE;
    this.message = '';
  }

  pauseGame() {
    this.isPaused = !this.isPaused;
  }

  snakeTick() {
    if (this.isPaused || this.isEndGame) {
      return;
    }

    let action = this._nextAction;
    if (action != ACTION.NONE && this._isStarted == false) {
      // Starting game
      this._isStarted = true;
    }

    if (this._isStarted) {
      
      if (this._data.moveSnake(action)) {
        this.drawSnake();
      }
      else {
        // Game over
        this._isStarted = false;
        this.isEndGame = true;
        this.message = 'GAME OVER !';
      }
    }
  }
}
