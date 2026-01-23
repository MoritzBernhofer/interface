import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ClientServerDto } from '../models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ClientServerService {
  private http = inject(HttpClient);

  getAll(): Observable<ClientServerDto[]> {
    return this.http.get<ClientServerDto[]>(`${environment.apiUrl}/clientservers`);
  }

  getById(id: number): Observable<ClientServerDto> {
    return this.http.get<ClientServerDto>(`${environment.apiUrl}/clientservers/${id}`);
  }
}
