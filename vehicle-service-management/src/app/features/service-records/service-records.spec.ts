import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServiceRecords } from './service-records';

describe('ServiceRecords', () => {
  let component: ServiceRecords;
  let fixture: ComponentFixture<ServiceRecords>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ServiceRecords],
    }).compileComponents();

    fixture = TestBed.createComponent(ServiceRecords);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
