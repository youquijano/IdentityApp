import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { AccountService } from '../account.service';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from '../../shared/models/account/user';
import { SharedService } from '../../shared/shared.service';
import { LoginWithExternal } from '../../shared/models/account/loginWithExternal';
declare const FB: any;

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages: string[] = [];
  returnUrl: string | null = null;

  constructor(
    private accountService: AccountService,
    private formBuilder: FormBuilder,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private sharedService: SharedService
  ) {
    this.accountService.user$.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user) {
          if (this.returnUrl) {
            this.router.navigateByUrl(this.returnUrl);
          } else {
            this.router.navigateByUrl('/');
          }
        } else {
          this.activatedRoute.queryParamMap.subscribe({
            next: (params: any) => {
              if (params) {
                this.returnUrl = params.get('returnUrl');
              }
            },
          });
        }
      },
    });
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.loginForm = this.formBuilder.group({
      userName: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  login() {
    this.submitted = true;
    this.errorMessages = [];

    if (this.loginForm.valid) {
      this.accountService.login(this.loginForm.value).subscribe({
        next: _ => {
          //console.log(response);
          if (this.returnUrl) {
            this.router.navigateByUrl(this.returnUrl);
          } else {
            this.router.navigateByUrl('/');
          }
        },
        error: (error) => {
          if (error.error.errors) {
            this.errorMessages = error.error.errors;
          } else {
            this.errorMessages.push(error.error);
          }
          //console.log(error);
        },
      });
    }

    //console.log(this.registerForm.value);
  }

  loginWithFacebook() {
    FB.login(async (fbResult: any) => {
      //console.log(fbResult);
      if (fbResult.authResponse) {
        const accessToken = fbResult.authResponse.accessToken;
        const userId = fbResult.authResponse.userID;
        //console.log(fbResult);
        this.accountService
          .loginWithThirdParty(
            new LoginWithExternal(accessToken, userId, 'facebook')
          )
          .subscribe({
            next: _ => {
              if (this.returnUrl) {
                this.router.navigateByUrl(this.returnUrl);
              } else {
                this.router.navigateByUrl('/');
              }
            },
            error: (error) => {
              if (error.error.errors) {
                this.errorMessages = error.error.errors;
              } else {
                this.errorMessages.push(error.error);
              }
              //console.log(error);
            },
          });
      } else {
        this.sharedService.showNotification(
          false,
          'Failed',
          'Unable to login with your facebook'
        );
      }
    });
  }

  resendEmailConfirmationLink() {
    this.router.navigateByUrl(
      '/account/send-email/resend-email-confirmation-link'
    );
  }
}
