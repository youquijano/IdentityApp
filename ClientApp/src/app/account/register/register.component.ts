import { Component, OnInit } from '@angular/core';
import { AccountService } from '../account.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SharedService } from '../../shared/shared.service';
import { Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from '../../shared/models/account/user';

declare const FB: any;

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit{

  registerForm : FormGroup = new FormGroup({});
  submitted = false;
  errorMessages : string[] = [];

  constructor( private accountService : AccountService,
    private formBuilder : FormBuilder,
    private sharedService : SharedService,
    private router : Router
  ) {
    this.accountService.user$.pipe(take(1)).subscribe({
      next : (user : User | null) => {
        if(user){
          this.router.navigateByUrl('/');
        }
      }
    });
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(){
    this.registerForm = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(15)]],
      lastName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(15)]],
      email: ['', [Validators.required, Validators.pattern('^([\\w\\-\\.]+)@((\\[([0-9]{1,3}\\.){3}[0-9]{1,3}\\])|(([\\w\\-]+\\.)+)([a-zA-Z]{2,4}))$')]],
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(15)]],
    });
  }

  register(){
    this.submitted = true;
    this.errorMessages = [];

    if(this.registerForm.valid){
      this.accountService.register(this.registerForm.value).subscribe(
        {
          next : (response : any) => {
            this.sharedService.showNotification(true,response.value.title, response.value.message );
            this.router.navigateByUrl('/account/login');
            //console.log(response);
          },
          error : error =>  {
            if(error.error.errors){
              this.errorMessages = error.error.errors;
            } else {
              this.errorMessages.push(error.error);
            }
            //console.log(error);
          }
        }
      );
    }

    //console.log(this.registerForm.value);
  }

  registerWithFacebook(){
    FB.login(async (fbResult:any) =>{
      //console.log(fbResult);
      if(fbResult.authResponse){
        const accessToken = fbResult.authResponse.accessToken;
        const userId = fbResult.authResponse.userID;
        this.router.navigateByUrl(`/account/register/third-party/facebook?access_token=${accessToken}&userId=${userId}`);
      } else {
        this.sharedService.showNotification(false, "Failed","Unable to register with your facebook");
      }
    })
  }

}
