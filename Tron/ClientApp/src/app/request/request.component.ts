import { Component, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-request',
  templateUrl: './request.component.html'
})
export class RequestComponent {
  public requestTypes: string[] = ["GET", "POST", "PUT", "DELETE"];
  public result;

  private url: string;
  private _mode: string = "GET";
  private data: string;
  private _httpClient: HttpClient;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this._httpClient = http;
    this.url = "https://localhost:44329/api/Default";
    this.data = "{\"value\":\"5\"}";
  }

  changeUrl(url) {
    this.url = url;
  }

  changeMode(mode) {
    this._mode = mode
  }

  changeData(data) {
    this.data = data;
    //let body = JSON.stringify(data);
  }

  sendRequest() {
    switch (this._mode) {
      case 'GET':
        this._httpClient.get<string>(this.url).subscribe(
          result => { this.result = result },
          error => { this.result = error.message });
        break;

      case 'POST':
        let header = new HttpHeaders({ 'Content-Type': 'application/json' });
        this._httpClient.post<any>(this.url, this.data, { headers: header }).subscribe(
          result => { this.result = result },
          error => { this.result = error.message });
        break;
      default:
    }
  }

  sendTestRequest() {
    this.result = '';
    this._httpClient.get<string>('api/Test').subscribe(
      result => { this.result += result },
      error => { this.result += error.message });

    let header = new HttpHeaders({ 'Content-Type': 'application/json' });
    let data = JSON.stringify({ value: 5 });
    this._httpClient.post('api/Test', data, { headers: header }).subscribe(
      result => { this.result += result },
      error => { this.result += error.message });
  }
}
