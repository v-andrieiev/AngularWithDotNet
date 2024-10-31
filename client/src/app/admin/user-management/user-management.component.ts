import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { User } from '../../_models/user';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { RolesMadalComponent } from '../../modals/roles-madal/roles-madal.component';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  private modalService = inject(BsModalService);
  users: User[] = [];
  bsModalRef: BsModalRef<RolesMadalComponent> = new BsModalRef<RolesMadalComponent>();

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  openRolesModal(user: User){
    const initialState: ModalOptions = {
      class: 'modal-lg',
      initialState: {
        title: 'User roles',
        username: user.username,
        selectedRoles: [...user.roles],
        availableRoles: ['Admin', 'Moderator', 'Member'],
        users: this.users,
        rolesUpdate: false
      }
    }
    this.bsModalRef = this.modalService.show(RolesMadalComponent, initialState);
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        if(this.bsModalRef.content && this.bsModalRef.content.rolesUpdated){
          const seelectedRoles = this.bsModalRef.content.selectedRoles;
          
          this.adminService.updateUserRoles(user.username, seelectedRoles).subscribe({
            next: roles => user.roles = roles
          });
        }
      }
    });
  }

  getUsersWithRoles(){
    this.adminService.getUserWithRoles().subscribe({
      next: users => this.users = users
    });
  }
}
