import {
  Component,
  ElementRef,
  OnInit,
  Renderer2,
  ViewChild,
  Inject,
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../account.service';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from '../../shared/models/account/user';
import { SharedService } from '../../shared/shared.service';
import { LoginWithExternal } from '../../shared/models/account/loginWithExternal';
import { DOCUMENT } from '@angular/common';
import { jwtDecode } from 'jwt-decode';
import { CredentialResponse } from 'google-one-tap';
declare const FB: any;

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent implements OnInit {
  @ViewChild('googleButton', { static: true }) googleButton: ElementRef =
    new ElementRef({});

  loginForm: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages: string[] = [];
  returnUrl: string | null = null;

  constructor(
    private accountService: AccountService,
    private formBuilder: FormBuilder,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private sharedService: SharedService,
    private _render2: Renderer2,
    @Inject(DOCUMENT) private _document: Document
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
    this.initializeGoogleButton();
    this.initializeForm();
  }

  ngAfterViewInit() {
    const script1 = this._render2.createElement('script');
    script1.src = 'https://accounts.google.com/gsi/client';
    script1.async = 'true';
    script1.defer = 'true';
    this._render2.appendChild(this._document.body, script1);
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
        next: (_) => {
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
            next: (_) => {
              if (this.returnUrl) {
                this.router.navigateByUrl(this.returnUrl);
              } else {
                this.router.navigateByUrl('/');
              }
            },
            error: (error) => {
              this.sharedService.showNotification(false, "Failed", error.error);
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

  initializeGoogleButton() {
    if (typeof window === 'undefined') {
      console.log('window is not defined');
    }
    (window as any).onGoogleLibraryLoad = () => {
      // @ts-ignore
      google.accounts.id.initialize({
        client_id:
          '867331117312-hm9o6p5qous20gtpvjvls22hippih4r5.apps.googleusercontent.com',
        callback: this.googleCallBack.bind(this),
        auto_select: false,
        cancel_on_tap_outside: true,
      });

      // @ts-ignore
      google.accounts.id.renderButton(this.googleButton.nativeElement, {
        size: 'medium',
        shape: 'rectangular',
        text: 'signin_with',
        logo_alignment: 'center',
      });
    };
  }

  resendEmailConfirmationLink() {
    this.router.navigateByUrl(
      '/account/send-email/resend-email-confirmation-link'
    );
  }

  private googleCallBack(response: CredentialResponse) {
    //console.log(response);
    const decodedToken: any = jwtDecode(response.credential);
    this.accountService
      .loginWithThirdParty(
        new LoginWithExternal(response.credential, decodedToken.sub, 'google')
      )
      .subscribe({
        next: (_) => {
          if (this.returnUrl) {
            this.router.navigateByUrl(this.returnUrl);
          } else {
            this.router.navigateByUrl('/');
          }
        },
        error: (error) => {
          this.sharedService.showNotification(false, "Failed", error.error);
          //console.log(error);
        },
      });
  }
}
