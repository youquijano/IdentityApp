import { Component, Host, HostListener, OnInit } from '@angular/core';
import { AccountService } from './account/account.service';
import { SharedService } from './shared/shared.service';
import { take } from 'rxjs';
import { User } from './shared/models/account/user';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})

export class AppComponent implements OnInit{  

  constructor(private accountService : AccountService,
    private sharedService : SharedService
  ) {}
  
  ngOnInit(): void {
    //this.sharedService.openExpirySessionCountdown();
    this.refreshUser();
  }


  @HostListener('window:keydown')
  @HostListener('window:mousedown')
  checkUserActivity(){
    this.accountService.user$.pipe(take(1)).subscribe({
      next: (user : User | null) => {
        if(user){
          console.log('user is active');
          clearTimeout(this.accountService.timeoutId);
          this.accountService.checkUserIdleTimeout();
        }
      }
    })
  }

  private refreshUser(){
    const jwt = this.accountService.getJwt();
    if(jwt){
      this.accountService.refreshUser(jwt).subscribe(
        {
          next : _ => {},
          error : error => {
            
            this.accountService.logout();
            this.sharedService.showNotification(false, 'Account blocked', error.error);
          }
        }
      );
    } else {
      this.accountService.refreshUser(null).subscribe();
    }
  }
  
}
