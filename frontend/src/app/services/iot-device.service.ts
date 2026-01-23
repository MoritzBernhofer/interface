import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IotDeviceDto, CreateIotDeviceDto } from '../models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class IotDeviceService {
  private http = inject(HttpClient);

  getAll(): Observable<IotDeviceDto[]> {
    return this.http.get<IotDeviceDto[]>(`${environment.apiUrl}/iotdevices`);
  }

  getById(id: number): Observable<IotDeviceDto> {
    return this.http.get<IotDeviceDto>(`${environment.apiUrl}/iotdevices/${id}`);
  }

  create(dto: CreateIotDeviceDto): Observable<IotDeviceDto> {
    return this.http.post<IotDeviceDto>(`${environment.apiUrl}/iotdevices`, dto);
  }

  delete(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${environment.apiUrl}/iotdevices/${id}`);
  }
}
