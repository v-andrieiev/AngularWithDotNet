import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastr = inject(ToastrService);

  return next(req).pipe(
    catchError(_ => {
      if(_){
        switch (_.status){
          case 400:
            if(_.error.errors){
              const modalStateErrors = [];
              for(const key in _.error.errors){
                if(_.error.errors[key]){
                  modalStateErrors.push(_.error.errors[key]);
                }
              }
              throw modalStateErrors.flat();
            } else {
              toastr.error(_.error, _.status);
            }
            break;
            case 401:
              toastr.error('Unauthorised', _.status);
              break;
            case 404:
              router.navigateByUrl('/not-found');
              break;
            case 500:
              const navigationExtras: NavigationExtras = {state: {error: _.error}}
              router.navigateByUrl('/server-error', navigationExtras);
              break;
            default:
              toastr.error('Something unexpected went wrong');
              break;
          }
      }
      throw _;
    })
  );
};
