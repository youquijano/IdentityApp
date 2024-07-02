import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { take } from 'rxjs';
import { ConfirmEmail } from '../../shared/models/account/confirmEmail';
import { User } from '../../shared/models/account/user';
import { SharedService } from '../../shared/shared.service';
import { AccountService } from '../account.service';

@Component({
  selector: 'app-confirm-email-bak',
  templateUrl: './confirm-email-bak.component.html',
  styleUrl: './confirm-email-bak.component.css',
})
export class ConfirmEmailBakComponent implements OnInit {
  success = false;
  constructor(
    private accountService: AccountService,
    private sharedService: SharedService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    console.log('const');
  }

  ngOnInit(): void {
    console.log('ngonit')
    this.accountService.user$.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user) {
          this.router.navigateByUrl('/');
        } else {
          this.activatedRoute.queryParamMap.subscribe({
            next: (params: any) => {
              const confirmEmail: ConfirmEmail = {
                email: params.get('email'),
                token: params.get('token'),
              };

              this.accountService.confirmEmail(confirmEmail).subscribe({
                next: (response: any) => {
                  this.sharedService.showNotification(
                    true,
                    response.value.title,
                    response.value.message
                  );
                },
                error: (error) => {
                  this.success = false;
                  this.sharedService.showNotification(
                    false,
                    'Failed',
                    error.error
                  );
                },
              });
            },
          });
        }
      },
    });
  }

  resendEmailConfirmationLink() {
    this.router.navigateByUrl(
      '/account/send-email/resend-email-confirmation-link'
    );
  }
}
