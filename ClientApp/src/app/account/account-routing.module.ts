import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { SendEmailComponent } from './send-email/send-email.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { RegisterWithThirdPartyComponent } from './register-with-third-party/register-with-third-party.component';
import { ConfirmEmailBakComponent } from './confirm-email-bak/confirm-email-bak.component';

const routes : Routes = [
  {
    path: 'login', component : LoginComponent
  },
  {
    path: 'register', component : RegisterComponent
  },
  {
    path: 'confirm-email', component : ConfirmEmailBakComponent
  },
  {
    path: 'send-email/:mode', component : SendEmailComponent
  },
  {
    path: 'reset-password', component : ResetPasswordComponent
  },
  {
    path: 'register/third-party/:provider', component : RegisterWithThirdPartyComponent
  }
]

@NgModule({
  declarations: [],
  imports: [
    RouterModule.forChild(routes)
  ],
  exports :[
    RouterModule
  ]
})
export class AccountRoutingModule { }
