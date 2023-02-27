import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { DateTime } from 'luxon';

@Component({
    selector: 'app-fetch-data',
    templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent implements OnInit {
    public forecasts: WeatherForecast[];

    constructor(
        private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string
    ) { }

    ngOnInit() {
        this.http.get<WeatherForecast[]>(`${this.baseUrl}weatherforecast`).subscribe(forecasts => {
            for (const forecast of forecasts) {
                forecast.date = DateTime.fromISO(forecast.date).toLocaleString(DateTime.DATE_MED);
            }
            this.forecasts = forecasts;
        }, error => console.error(error));
    }
}

interface WeatherForecast {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}
