import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { environment } from 'src/environments/environment';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {

  ApiUrl: string;
  public SwitchMappings: ExtronMappedEntry[] = [];

  constructor(private http: HttpClient) {
    this.ApiUrl = environment.api;
  }

  ngOnInit() {
    this.getSwitcherMappings();
  }

  getSwitcherMappings() {
    var endpoint = this.ApiUrl + 'switcher';
    this.http.get<ExtronMappedEntry[]>(endpoint).subscribe((c : ExtronMappedEntry[]) => {
      console.log(c);
      this.SwitchMappings = c;
    });
  }

  updateSwitcherMapping(mapping: ExtronMappedEntry) {
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