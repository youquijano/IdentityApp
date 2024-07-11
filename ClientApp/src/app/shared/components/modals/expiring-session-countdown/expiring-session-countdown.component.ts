import { Component, OnDestroy, OnInit } from '@angular/core';
import { interval, Subscription } from 'rxjs';
import { SharedService } from '../../../shared.service';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { AccountService } from '../../../../account/account.service';

@Component({
  selector: 'app-expiring-session-countdown',
  templateUrl: './expiring-session-countdown.component.html',
  styleUrl: './expiring-session-countdown.component.css'
})
export class ExpiringSessionCountdownComponent implements OnInit, OnDestroy {
  modalRef? : BsModalRef;
  targetTime : number = 120; //countdown time in seconds
  remainingTime : number = this.targetTime;

  displayTime : string = this.formatTime(this.remainingTime);
  countdownSubscription : Subscription | undefined;

  constructor(private sharedService : SharedService,
    private bsModalRef : BsModalRef,
    private accountService : AccountService
  ){

  }
  ngOnDestroy(): void {
    this.stopCountDown();
  }
  ngOnInit(): void {
    this.startCountDown();
    
  }

  startCountDown(){
    this.countdownSubscription = interval(1000).subscribe(() => {
      if(this.remainingTime > 0){
        this.remainingTime--;
        this.displayTime = this.formatTime(this.remainingTime);
      } else {
        this.stopCountDown();
        this.sharedService.showNotification(false, 'Logged Out', 'You have been logged out due to inactivity');
      }
    });
  }

  private stopCountDown(){
    if(this.countdownSubscription){
      this.countdownSubscription.unsubscribe();
    }
  }

  private formatTime(seconds : number) : string{
    const minutes = Math.floor(seconds/60);
    const remainingSeconds = seconds % 60;
    return `${this.pad(minutes)}:${this.pad(remainingSeconds)}`;
  }

  private pad(value : number ) : string {
    return value < 10 ? `0${value}` : value.toString();
  }

  logout(){
    this.bsModalRef?.hide();
    this.accountService.logout();
  }

  resumeSession(){
    this.bsModalRef?.hide();
    this.accountService.refreshToken();
  }
}
