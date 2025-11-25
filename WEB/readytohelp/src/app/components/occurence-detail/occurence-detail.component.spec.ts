import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrenceDetailComponent } from './occurence-detail.component';

describe('OccurrenceDetailComponent', () => {
  let component: OccurrenceDetailComponent;
  let fixture: ComponentFixture<OccurrenceDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OccurrenceDetailComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OccurrenceDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
