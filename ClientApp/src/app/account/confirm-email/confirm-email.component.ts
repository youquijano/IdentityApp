import { Component, OnInit } from '@angular/core';
import { AccountService } from '../account.service';
import { SharedService } from '../../shared/shared.service';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from '../../shared/models/account/user';
import { ConfirmEmail } from '../../shared/models/account/confirmEmail';

@Component({
  selector: 'app-confirm-email',
  templateUrl: './confirm-email.component.html',
  styleUrl: './confirm-email.component.css'
})
export class ConfirmEmailComponent implements OnInit {

  success = false;
  constructor(private accountService : AccountService, 
    private sharedService : SharedService,
    private router : Router,
    private activatedRoute : ActivatedRoute
  ){}

  ngOnInit(): void {
    this.accountService.user$.pipe(take(1)).subscribe({
      next: (user : User | null) =>{
        if(user){
          this.router.navigateByUrl('/')
        } else {
          this.activatedRoute.queryParamMap.subscribe({
            next : (params : any) =>{
              const confirmEmail : ConfirmEmail = {
                email : params.get('email'),
                token : params.get('token')
              }
              
                this.accountService.confirmEmail(confirmEmail).subscribe({
                  next : (response : any) => {
                    this.success = true;
                    this.sharedService.showNotification(true, response.value.title, response.value.message);
                  }, 
                  error : error => {
                    this.success = false;
                    this.sharedService.showNotification(false, "Failed", error.error);
                  }
                });
              
            }
          })
        }
      }
    });
  }

  resendEmailConfirmationLink(){
    this.router.navigateByUrl('/account/send-email/resend-email-confirmation-link');
  }

}
