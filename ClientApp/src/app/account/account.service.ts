import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Register } from '../shared/models/account/register';
import { environment } from '../../environments/environment.development';
import { Login } from '../shared/models/account/login';
import { User } from '../shared/models/account/user';
import { ReplaySubject, map, of } from 'rxjs';
import { Router } from '@angular/router';
import { ConfirmEmail } from '../shared/models/account/confirmEmail';
import { ResetPassword } from '../shared/models/account/resetPassword';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private userSource = new ReplaySubject<User | null>(1);
  user$ = this.userSource.asObservable();

  constructor(private _http : HttpClient,
    private router: Router
  ) { }

  register(model : Register){
    return this._http.post(`${environment.appUrl}/api/account/register`, model);
  }

  confirmEmail(model : ConfirmEmail){
    return this._http.put(`${environment.appUrl}/api/account/confirm-email`, model);
  }

  resendEmailConfirmationLink(email: string){
    return this._http.post(`${environment.appUrl}/api/account/resend-email-confirmation-link/${email}`, {});
  }

  forgotUsernameOrPassword(email: string){ 
    return this._http.post(`${environment.appUrl}/api/account/forgot-username-or-password/${email}`, {});
  }

  resetPassword(model : ResetPassword){
    return this._http.put(`${environment.appUrl}/api/account/reset-password`, model);
  }

  refreshUser(jwt : string | null){
    if(jwt === null){
      this.userSource.next(null);
      return of(undefined);
    }

    let headers = new HttpHeaders();
    headers = headers.set('Authorization', 'Bearer ' + jwt);

    return this._http.get<User>(`${environment.appUrl}/api/account/refresh-user-token`, {headers}).pipe(      
      map((user:User) => {
        if(user) {
          this.setUser(user);
        }        
      })
    );
  }

  login(model : Login){
    return this._http.post<User>(`${environment.appUrl}/api/account/login`, model).pipe(
      map((user:User) => {
        if(user){
          this.setUser(user);
          //return user;
        }
        //return null;
      })
    );
  }

  logout(){
    localStorage.removeItem(environment.userKey);
    this.userSource.next(null);
    this.router.navigateByUrl('/');
  }

  getJwt(){
    if(typeof window !== 'undefined' && window.localStorage){
      const key = localStorage.getItem(environment.userKey);
      if(key){
        const user : User = JSON.parse(key);
        return user.jwt;
      } else {
        return null;
      }
    }
    return null;
  }

  private setUser(user : User) {
    localStorage.setItem(environment.userKey, JSON.stringify(user));
    this.userSource.next(user);

    // this.user$.subscribe({
    //   next: response => 
    //     console.log(response)
    // });
  }
}
