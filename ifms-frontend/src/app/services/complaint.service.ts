import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ComplaintService {
  private readonly apiUrl = 'http://localhost:5010/gateway/complaints';

  constructor(private http: HttpClient, private auth: AuthService) {}

  private headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.auth.getToken()}` });
  }

  raise(data: {
    category: string; subject: string; description: string; referenceId?: string;
  }): Observable<any> {
    return this.http.post(this.apiUrl, data, { headers: this.headers() });
  }

  getMine(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/my`, { headers: this.headers() });
  }

  getById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`, { headers: this.headers() });
  }
}
