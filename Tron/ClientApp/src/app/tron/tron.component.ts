import { Component, HostListener, OnInit, AfterViewChecked, ViewChild, ElementRef } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HttpTransportType } from '@aspnet/signalr';

const _bike_width = 15;
const _bike_height = 5;
const _bike_width_center = _bike_width / 2;
const _bike_height_center = _bike_height / 2;

enum ETAPE_ENUM {
  NONE = 0,
  ROOM = 1,
  GAME = 2
}

enum MODAL_ENUM {
  NONE = 0,
  CLOSE = 1,
  RESTART = 2,
  LEFT = 3
}

const enum KEY_CODE {
  RIGHT_ARROW = 39,
  LEFT_ARROW = 37,
  UP_ARROW = 38,
  DOWN_ARROW = 40,
}

const enum DIRECTION {
  NONE = 0,
  RIGHT = 1,
  LEFT = 2,
  UP = 3,
  DOWN = 4,
}

@Component({
  selector: 'app-tron',
  templateUrl: './tron.component.html',
  styleUrls: ['./tron.component.css']
})
export class TronComponent implements OnInit, AfterViewChecked {
  // Allows you to use ETAPE_ENUM in template
  public ETAPE_ENUM: typeof ETAPE_ENUM = ETAPE_ENUM;
  public MODAL_ENUM: typeof MODAL_ENUM = MODAL_ENUM;
  public etape = ETAPE_ENUM.NONE;
  public modalType = MODAL_ENUM.NONE;
  public canvasWidth = 800;
  public canvasHeight = 600;
  public gameInPreparation = false;
  public isFirstBike = true;
  public isPrivateRoom = false;
  public opponentUserName: String;
  public roomName: String;
  public score: String = '0 - 0';
  public userName: String;
  public modalTitle: string = 'Title';
  public modalMessage: string = 'Message';

  private _data: Data;
  private _startingGameAnimation_StartDate: Date;
  private _startingGameAnimation_EndDate: Date;
  private _tron_handle_animation: number = 0;
  private _nextAction: DIRECTION = DIRECTION.NONE;
  private _hubConnection: HubConnection;
  private _connectionId: String;
  private _matchId: String;

  private _context: CanvasRenderingContext2D;
  private prevTronArena = null;
  public canvas: HTMLCanvasElement;

  //#region Life Cycle Methods

  @ViewChild('tronArena') tronArena: ElementRef;

  ngAfterViewChecked() {
    if (this.prevTronArena !== this.tronArena) {
      this.prevTronArena = this.tronArena;
      if (this.tronArena) {
        this.canvas = (this.tronArena.nativeElement as HTMLCanvasElement);
        this._context = this.canvas.getContext('2d');
        this.startGame();
      }
    }
  }

  ngOnInit() {
    this.initialisationHubConnection();
  }

  //#endregion Life Cycle Methods

  //#region HUBCONNECTION METHODS

  // Initialisation
  initialisationHubConnection() {
    this._hubConnection = new HubConnectionBuilder().withUrl("/hub/tron", {
      skipNegotiation: true,
      transport: HttpTransportType.WebSockets
    }).build();

    this._hubConnection
      .start()
      .then(() => { console.log('Connection started!'); this.askConnectionInformation(); })
      .catch(err => console.log('Error while establishing connection :(' + err.toString() + ')'));

    this._hubConnection.on('connected', (data) => { this.getConnectionInformation(data); });
    this._hubConnection.on('match_making_found', (data) => { this.getRoomInformation(data); });
    this._hubConnection.on('private_room_created', (data) => { this.privateRoomCreated(data); });
    this._hubConnection.on('private_room_completed', (data) => { this.privateRoomJoined(data); });
    this._hubConnection.on('match_started', (data) => { this.getMatchStarted(data); });
    this._hubConnection.on('map_information_received', (data) => { this.getMapInformation(data); });
    this._hubConnection.on('game_information', (data) => { this.gameInformationTreatment(data); });
    this._hubConnection.on('game_updated', (data) => { this.gameUpdated(data); });
  }

  // Connexion
  askConnectionInformation() {
    this._hubConnection
      .invoke('getConnectionInformation')
      .catch(err => console.error(err));
  }
  getConnectionInformation(connectionId) {
    this._connectionId = connectionId;
  }

  // Do match making
  askMatchMaking() {
    this._hubConnection
      .invoke('doMatchMaking', this.userName)
      .catch(err => console.error(err));
  }
  getRoomInformation(data) {
    this.fillRoomInformation(data);
    this.gameInPreparation = false;
  }

  // Create private room
  createPrivateRoom() {
    this._hubConnection
      .invoke('createPrivateRoom', this.userName)
      .catch(err => console.error(err));
  }
  privateRoomCreated(data) {
    this.fillRoomInformation(data);
    this.etape = ETAPE_ENUM.ROOM;
    this.isPrivateRoom = true;
  }

  // Join private room
  joinPrivateRoom(roomName) {
    this._hubConnection
      .invoke('joinPrivateRoom', this.userName, roomName)
      .catch(err => console.error(err));
  }
  privateRoomJoined(data) {
    this.fillRoomInformation(data);
    this.etape = ETAPE_ENUM.ROOM;
    this.isPrivateRoom = true;
  }

  // Start game
  sendPlayerReady() {
    this._hubConnection
      .invoke('playerReady', this._matchId, true)
      .catch(err => console.error(err));
  }
  getMatchStarted(data) {
    if (data) {
      this.etape = ETAPE_ENUM.GAME;
      this.sendMapInformation();
    }
  }

  // Send map info
  sendMapInformation() {
    this._hubConnection
      .invoke('mapInformation', this._matchId, this.canvasWidth, this.canvasHeight)
      .catch(err => console.error(err));
  }
  getMapInformation(data) {
    if (data && data.gameId == this._matchId && this.canvas) {
      if (data.width) {
        this.canvasWidth = data.width;
      }
      if (data.height) {
        this.canvasHeight = data.height;
      }
    }
  }

  // Send user action
  sendDirection() {
    this._hubConnection
      .invoke('bikeDirection', this._matchId, this._nextAction)
      .catch(err => console.error(err));
  }

  // Back to menu (leave game)
  leaveGame() {
    this._hubConnection
      .invoke('leaveGame', this._matchId)
      .catch(err => console.error(err));
  }
  gameUpdated(data) {
    if (data) {
      this.fillRoomInformation(data);
      if (data.isFinished) {
        this.openModal('Game over !', this.opponentUserName + ' has left the game.<br />The current score is : ' + data.scorePlayer1 + ' - ' + data.scorePlayer2, MODAL_ENUM.LEFT);
      }
      else {
        this.openModal('Player left', this.opponentUserName + ' has left the game.', MODAL_ENUM.CLOSE);
      }
    }
  }

  fillRoomInformation(data) {
    if (data) {
      if (data.id) {
        this._matchId = data.id;
      }
      if (data.name) {
        this.roomName = data.name;
      }
      if (data.player1 && data.player2) {
        if (this._connectionId == data.player1.connectionId) {
          this.isFirstBike = true;
          this.opponentUserName = data.player2.name;
        }
        else {
          this.isFirstBike = false;
          this.opponentUserName = data.player1.name;
        }
      }
    }
  }

  gameInformationTreatment(data) {
    if (data) {
      if (data.scorePlayer1 && data.scorePlayer2) {
        this.score = data.scorePlayer1 + ' - ' + data.scorePlayer2;

        if (data.hasScoreChanged) {
          let message = " | " + this.score + " | ";
          if (this.isFirstBike) {
            message = this.userName + message + this.opponentUserName;
          }
          else {
            message = this.opponentUserName + message + this.userName;
          }

          this.openModal("New score !", message, MODAL_ENUM.RESTART);
        }
      }

      if (data.width) {
        this.canvasWidth = data.width;
      }
      if (data.height) {
        this.canvasHeight = data.height;
      }

      if (!this._data) {
        this.initializeTronGame();
      }

      if (data.bikeInformationPlayer1) {
        this._data.player1.direction = data.bikeInformationPlayer1.direction;
        this._data.player1.x = data.bikeInformationPlayer1.x;
        this._data.player1.y = data.bikeInformationPlayer1.y;
        this._data.player1.traces = data.bikeInformationPlayer1.traces;
      }
      if (data.bikeInformationPlayer2) {
        this._data.player2.direction = data.bikeInformationPlayer2.direction;
        this._data.player2.x = data.bikeInformationPlayer2.x;
        this._data.player2.y = data.bikeInformationPlayer2.y;
        this._data.player2.traces = data.bikeInformationPlayer2.traces;
      }

      this.drawTron();
    }
  }

  //#endregion HUBCONNECTION METHODS

  //#region EVENT HANDLERS

  changeUserName(value: string) {
    this.userName = value;
  }
  doMatchMaking() {
    this.etape = ETAPE_ENUM.ROOM;
    this.isPrivateRoom = false;
    this.gameInPreparation = true;
    this.askMatchMaking();
  }
  switchBikes() {
  }
  readyForGame() {
    this.sendPlayerReady();
  }
  backToMainMenu() {
    this.etape = ETAPE_ENUM.NONE;
    this.stopGame();
    this.gameInPreparation = false;
  }

  @HostListener('window:keydown', ['$event'])
  keyEvent(event: KeyboardEvent) {
    let lastAction = this._nextAction;
    if (event.keyCode === KEY_CODE.RIGHT_ARROW) {
      this._nextAction = DIRECTION.RIGHT;
    }
    else if (event.keyCode === KEY_CODE.LEFT_ARROW) {
      this._nextAction = DIRECTION.LEFT;
    }
    else if (event.keyCode === KEY_CODE.UP_ARROW) {
      this._nextAction = DIRECTION.UP;
    }
    else if (event.keyCode === KEY_CODE.DOWN_ARROW) {
      this._nextAction = DIRECTION.DOWN;
    }

    if (lastAction != this._nextAction) {
      this.sendDirection();
    }
  }

  //#endregion EVENT HANDLERS

  //region MODAL HANDLERS

  openModal(title: string, message: string, mode: MODAL_ENUM) {
    this.modalTitle = (title) ? title : 'Title';
    this.modalMessage = (message) ? message : 'Message';
    this.modalType = mode;

    document.getElementById('openModalButton').click();
    document.getElementById('modalDialog').className = 'modal-open';
  }

  //endregion MODAL HANDLERS

  //#region TRON METHODS

  stopGame() {
    if (this._tron_handle_animation) {
      window.cancelAnimationFrame(this._tron_handle_animation);
    }

    this.leaveGame();

    this.reset();
  }
  startGame() {
    if (this._context) {
      // Initialize animation duration 
      this._startingGameAnimation_StartDate = this._startingGameAnimation_EndDate = new Date;
      this._startingGameAnimation_StartDate.setSeconds(this._startingGameAnimation_StartDate.getSeconds() + 1);
      this._startingGameAnimation_EndDate.setSeconds(this._startingGameAnimation_StartDate.getSeconds() + 1);

      this._tron_handle_animation = window.requestAnimationFrame(() => {
        this.startingGameAnimation();
      });
    }
  }
  startingGameAnimation() {
    this._context.fillStyle = "rgb(53,87,96)";
    this._context.fillRect(0, 0, this.canvas.width, this.canvas.height);

    let d = new Date();
    let interval = this._startingGameAnimation_EndDate.getSeconds() - d.getSeconds();

    if (interval >= 0) {
      let fontSize = 0;
      if (interval >= 3) {
        interval = 2;
        fontSize = Math.floor(this.canvas.height / 12);
      }
      else {
        fontSize = Math.floor((d.getMilliseconds() / 1000 * this.canvas.height / 6) + this.canvas.height / 12);
      }

      this._context.font = fontSize + "px Arial";
      this._context.fillStyle = "white";
      this._context.textAlign = 'center';
      this._context.fillText((interval + 1).toString(), this.canvas.width / 2, this.canvas.height / 2);

      // Rethrow to the next frame
      this._tron_handle_animation = window.requestAnimationFrame(() => {
        this.startingGameAnimation();
      });
    }
  }
  initializeTronGame() {
    this._data = new Data(this.isFirstBike);

    this._data.player1 = new Player();
    this._data.player1.x = this.canvas.width / 4;
    this._data.player1.y = this.canvas.height / 2;
    this._data.player1.direction = DIRECTION.RIGHT;
    this._data.player1.bikeImage = document.getElementById("icon_bike_one") as HTMLImageElement;

    this._data.player2 = new Player();
    this._data.player2.x = 3 * this.canvas.width / 4;
    this._data.player2.y = this.canvas.height / 2;
    this._data.player2.direction = DIRECTION.LEFT;
    this._data.player2.bikeImage = document.getElementById("icon_bike_two") as HTMLImageElement;
  }
  drawTron() {
    this._context.fillStyle = "rgb(53,87,96)";
    this._context.fillRect(0, 0, this.canvas.width, this.canvas.height);

    // Draw Player 1
    if (this._data.player1.traces) {
      this._context.fillStyle = "rgb(4,254,250)";
      this._data.player1.traces.forEach((t, i, a) => {
        this._context.fillRect(t.topLeftX, t.topLeftY, t.width, t.height);
      });
    }
    if (this._data.player1.bikeImage) {
      // Coordinates of player is the center of bike.
      this._context.save();
      this._context.translate(this._data.player1.x, this._data.player1.y);
      this._context.rotate(this.getAngleOfDirection(this._data.player1.direction));
      this._context.drawImage(this._data.player1.bikeImage, -_bike_width_center, -_bike_height_center, _bike_width, _bike_height);
      this._context.restore();
    }

    // Draw Player 2
    if (this._data.player2.traces) {
      this._context.fillStyle = "rgb(246,222,2)";
      this._data.player2.traces.forEach((t, i, a) => {
        this._context.fillRect(t.topLeftX, t.topLeftY, t.width, t.height);
      });
    }
    if (this._data.player2.bikeImage) {
      this._context.save();
      this._context.translate(this._data.player2.x, this._data.player2.y);
      this._context.rotate(this.getAngleOfDirection(this._data.player2.direction));
      this._context.drawImage(this._data.player2.bikeImage, -_bike_width_center, -_bike_height_center, _bike_width, _bike_height);
      this._context.restore();
    }
  }

  getAngleOfDirection(direction: DIRECTION): number {
    switch (direction) {
      case DIRECTION.DOWN:
        return Math.PI / 2;
      case DIRECTION.UP:
        return Math.PI + Math.PI / 2;
      case DIRECTION.LEFT:
        return Math.PI;
      case DIRECTION.RIGHT:
      default:
        return 0;
    }
  }

  reset() {
    // Reset some variables
    this._nextAction = DIRECTION.NONE;
    this._matchId = undefined;
    this.isPrivateRoom = false;
    this.isFirstBike = true;
    this.opponentUserName = undefined;
    this.score = '0 - 0';
  }

  //#endregion TRON METHODS
}

class Data {
  public player1: Player;
  public player2: Player;
  private _isPlayer1: boolean;

  constructor(isPlayer1: boolean) {
    this._isPlayer1 = isPlayer1;
  }

  getIsPlayer1() {
    return this._isPlayer1;
  }
}

class Player {
  public x = 0;
  public y = 0;
  public direction: DIRECTION;
  public bikeImage: HTMLImageElement;
  public traces;
}
