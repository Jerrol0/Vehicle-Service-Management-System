import { Routes } from '@angular/router';

import { Dashboard } from './features/dashboard/dashboard';
import { Customers } from './features/customers/customers';
import { Vehicles } from './features/vehicles/vehicles';
import { ServiceRecords } from './features/service-records/service-records';
import { VehicleHistory } from './features/vehicles/vehicle-history';

import { AddCustomer } from './features/customers/add-customer';
import { EditCustomer } from './features/customers/edit-customer';

import { AddVehicle } from './features/vehicles/add-vehicle';
import { EditVehicle } from './features/vehicles/edit-vehicle';

import { ViewServiceRecord } from './features/service-records/view-service-record';
import { AddServiceRecord } from './features/service-records/add-service-record';
import { EditServiceRecord } from './features/service-records/edit-service-record';

export const routes: Routes = [
  { path: '', component: Dashboard },

  { path: 'customers/new', component: AddCustomer },
  { path: 'customers/:id/edit', component: EditCustomer },

  { path: 'vehicles/new', component: AddVehicle },
  { path: 'vehicles/:id/services', component: VehicleHistory },
  { path: 'vehicles/:id/edit', component: EditVehicle },

  { path: 'service-records/new', component: AddServiceRecord },
  { path: 'service-records/:id/edit', component: EditServiceRecord },
  { path: 'service-records/:id', component: ViewServiceRecord },

  { path: 'customers', component: Customers },
  { path: 'vehicles', component: Vehicles },
  { path: 'service-records', component: ServiceRecords },
];
