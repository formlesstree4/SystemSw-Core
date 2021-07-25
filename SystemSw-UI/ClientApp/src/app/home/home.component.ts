import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  ApiUrl: string;
  public SwitchMappings: ExtronMappedEntry[] = [];

  constructor(
    private http: HttpClient) {
    this.ApiUrl = '';
  }

  ngOnInit() {
    this.computeApiUrl();
    this.getSwitcherMappings();
  }

  private getSwitcherMappings(): void {
    var endpoint = this.ApiUrl + 'switcher';
    this.http.get<ExtronMappedEntry[]>(endpoint).subscribe((c : ExtronMappedEntry[]) => {
      console.log(c);
      this.SwitchMappings = c;
    });
  }

  private computeApiUrl(): void {
    this.ApiUrl = 'http://' + document.location.hostname + ':' + environment.api.toString() + '/';
  }

  updateSwitcherMapping(mapping: ExtronMappedEntry): void {
    var endpoint = this.ApiUrl + 'switcher';
    this.http.post<ExtronMappedEntry[]>(endpoint, mapping).subscribe((c: ExtronMappedEntry[]) => {
      this.SwitchMappings = c;
    });
  }

}

export class ExtronMappedEntry {
  public Channel: number
  public ChannelName: string
  public IsActiveChannel: boolean
}