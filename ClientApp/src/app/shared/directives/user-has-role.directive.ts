import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { AccountService } from '../../account/account.service';
import { take } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

@Directive({
  selector: '[appUserHasRole]'
})
export class UserHasRoleDirective implements OnInit {

  @Input() appUserHasRole : string[] = [];

  constructor(
    private viewContainerRef : ViewContainerRef,
    private templateRef : TemplateRef<any>,
    private accountService : AccountService
  ) { }

  ngOnInit(): void {
    this.accountService.user$.pipe(take(1)).subscribe({
      next : user => {
        if(user) {
          const decodedToken : any = jwtDecode(user.jwt);
          if(decodedToken.role.some((role:any) => this.appUserHasRole.includes(role))) {
            this.viewContainerRef.createEmbeddedView(this.templateRef);
          } else {
            this.viewContainerRef.clear();
          }
        } else {
          this.viewContainerRef.clear();
        }
      }
    })
  }

}
