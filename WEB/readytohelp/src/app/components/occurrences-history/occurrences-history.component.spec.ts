import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrencesHistoryComponent } from './occurrences-history.component';

describe('OccurrencesHistoryComponent', () => {
  let component: OccurrencesHistoryComponent;
  let fixture: ComponentFixture<OccurrencesHistoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OccurrencesHistoryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OccurrencesHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
