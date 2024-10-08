import { Component, HostListener, inject, OnInit, viewChild } from '@angular/core';
import { Member } from '../../models/member';
import { AccountService } from '../../_services/account.service';
import { MembersService } from '../../_services/members.service';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { Form, FormsModule, NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { PhotoEditorComponent } from "../photo-editor/photo-editor.component";

@Component({
  selector: 'app-member-edit',
  standalone: true,
  imports: [TabsModule, FormsModule, PhotoEditorComponent, PhotoEditorComponent],
  templateUrl: './member-edit.component.html',
  styleUrl: './member-edit.component.css'
})
export class MemberEditComponent implements OnInit {
  private accountService = inject(AccountService)
  private memberService = inject(MembersService)
  private toastr = inject(ToastrService)
  editForm = viewChild<NgForm>('editForm')
  member?: Member;

  @HostListener('window:beforeunload', ['$event']) notify($event: any) {
    if (this.editForm()?.dirty) {
      $event.returnValue = true;
    }
  }

  ngOnInit(): void {
    this.loadMember();
  }


  loadMember() {
    const user = this.accountService.currentUser();
    if (!user) return;

    this.memberService.getMember(user.username).subscribe({
      next: member => this.member = member
    });
  }

  updateMember() {
    this.memberService.updateMember(this.editForm()?.value).subscribe({
      next: _ => {
        this.toastr.success('Profile updated successfully')
        this.editForm()?.reset(this.member);
      }
    })

  }

}
