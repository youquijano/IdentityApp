import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { AccountRoutingModule } from './account-routing.module';
import { SharedModule } from '../shared/shared.module';
import { SendEmailComponent } from './send-email/send-email.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { RegisterWithThirdPartyComponent } from './register-with-third-party/register-with-third-party.component';
import { ConfirmEmailBakComponent } from './confirm-email-bak/confirm-email-bak.component';



@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent,    
    SendEmailComponent,
    ResetPasswordComponent,
    RegisterWithThirdPartyComponent,
    ConfirmEmailBakComponent
  ],
  imports: [
    CommonModule,
    AccountRoutingModule,
    SharedModule
  ]
})
export class AccountModule { }
