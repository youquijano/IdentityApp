import { Component } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.css'
})
export class NotificationComponent {
  isSuccess : boolean = false;
  title: string = '';
  message : string = '';

  /**
   *
   */
  constructor(public bsModalRef : BsModalRef) {}
}
