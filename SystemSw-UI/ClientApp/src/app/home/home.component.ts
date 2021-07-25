import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subscription, timer } from 'rxjs';
import { environment } from 'src/environments/environment';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {
  ApiUrl: string;
  public SwitchMappings: ExtronMappedEntry[] = [];

  constructor(private http: HttpClient) {
    this.ApiUrl = environment.api;
  }

  ngOnInit() {
    this.getSwitcherMappings();
    
  }

  ngOnDestroy() {

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