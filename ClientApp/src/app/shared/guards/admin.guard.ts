import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  CanActivateFn,
  GuardResult,
  MaybeAsync,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { Observable, map } from 'rxjs';
import { AccountService } from '../../account/account.service';
import { User } from '../models/account/user';
import { jwtDecode } from 'jwt-decode';
import { SharedService } from '../shared.service';

@Injectable({ providedIn: 'root' })

export class AdminGuard {
  constructor(
    private accountService: AccountService,
    private router: Router,
    private sharedService: SharedService
  ) {}

  canActivate(): Observable<boolean> {
    return this.accountService.user$.pipe(
      map((user: User | null) => {
        if (user) {
          const decodedToken: any = jwtDecode(user.jwt);

          if (decodedToken.role.includes('Admin')) {
            return true;
          }
        }
        this.sharedService.showNotification(false, 'Admin Area', 'Leave Now!');
        this.router.navigateByUrl('/');
        return false;
      })
    );
  }
}
