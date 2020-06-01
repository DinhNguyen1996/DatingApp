import { Injectable } from '@angular/core';
import * as alertifyjs from 'alertifyjs';

@Injectable({
  providedIn: 'root',
})
export class AlertifyService {
  constructor() {}
  confirm(message: string, okCallback: () => any) {
    alertifyjs.confirm(message, (e: any) => {
      if (e) {
        okCallback();
      } else {
      }
    });
  }

  success(message: string) {
    alertifyjs.success(message);
  }

  error(message: string) {
    alertifyjs.error(message);
  }

  warning(message: string) {
    alertifyjs.warning(message);
  }

  message(message: string) {
    alertifyjs.message(message);
  }
}
