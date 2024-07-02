import { Component, OnInit } from '@angular/core';
import { AccountService } from '../account.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first, take } from 'rxjs';
import { User } from '../../shared/models/account/user';
import { RegisterWithExternal } from '../../shared/models/account/registerWithExternal';

@Component({
  selector: 'app-register-with-third-party',
  templateUrl: './register-with-third-party.component.html',
  styleUrl: './register-with-third-party.component.css',
})
export class RegisterWithThirdPartyComponent implements OnInit {
  registerForm: FormGroup = new FormGroup({});
  submitted = false;
  provider: string | null = null;
  access_token: string | null = null;
  userId: string | null = null;
  errorMessages: string[] = [];
  constructor(
    private accountService: AccountService,
    private activateRoute: ActivatedRoute,
    private formBuilder: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.accountService.user$.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user) {
          this.router.navigateByUrl('/');
        } else {
          this.activateRoute.queryParamMap.subscribe({
            next: (params: any) => {
              this.provider =
                this.activateRoute.snapshot.paramMap.get('provider');
              this.access_token = params.get('access_token');
              this.userId = params.get('userId');

              // console.log(this.provider);
              // console.log(this.access_token);
              // console.log(this.userId);
              if (
                this.provider &&
                this.access_token &&
                this.userId &&
                (this.provider === 'facebook' || this.provider === 'google')
              ) {
                this.initializeForm();
              } else {
                this.router.navigateByUrl('/acount/register');
              }
            },
          });
        }
      },
    });
  }

  initializeForm() {
    this.registerForm = this.formBuilder.group({
      firstName: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(15),
        ],
      ],
      lastName: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(15),
        ],
      ],
    });
  }

  register() {
    this.submitted = true;
    this.errorMessages = [];

    if (
      this.registerForm.valid &&
      this.userId &&
      this.access_token &&
      this.provider
    ) {
      const firstName = this.registerForm.get('firstName')?.value;
      const lastName = this.registerForm.get('lastName')?.value;
      const model = new RegisterWithExternal(
        firstName,
        lastName,
        this.userId,
        this.access_token,
        this.provider
      );

      this.accountService.registerWithThirdParty(model).subscribe({
        next: _ => {
          //console.log(response);
          this.router.navigateByUrl('/');
        },
        error: (error) => {
          if (error.error.errors) {
            this.errorMessages = error.error.errors;
          } else {
            this.errorMessages.push(error.error);
          }
        },
      });
    }
  }
}
