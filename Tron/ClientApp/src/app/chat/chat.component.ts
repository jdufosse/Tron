import { Component, OnInit } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HttpTransportType} from '@aspnet/signalr';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {
  private _hubConnection: HubConnection;
  isExpanded = false;
  nick = '';
  message = '';
  messages: string[] = [];
  constructor() { }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  ngOnInit() {
    this.nick = window.prompt('Your name:', 'Emily');

    this._hubConnection = new HubConnectionBuilder().withUrl("/hub/chat", {
      skipNegotiation: true,
      transport: HttpTransportType.WebSockets
    }).build();

    this._hubConnection
      .start()
      .then(() => console.log('Connection started!'))
      .catch(err => console.log('Error while establishing connection :('+ err.toString()+')'));

    this._hubConnection.on('ReceiveMessage', (nick: string, receivedMessage: string) => {
      const text = `${nick}: ${receivedMessage}`;
      this.messages.push(text);
    });
  }

  public sendMessage(): void {
    this._hubConnection
      .invoke('sendMessage', this.nick, this.message)
      .then(() => this.message = '')
      .catch(err => console.error(err));
  }
}
